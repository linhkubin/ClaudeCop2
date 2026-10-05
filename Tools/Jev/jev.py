"""Jev (TypeSafe System One) helpers that cut Claude tokens in the ClaudeCop2 dev workflow.

Commands (all print ONE compact JSON line; details go to files/logs):
  route   "<task>" [--agent NAME]   pick haiku/sonnet/opus (+ owner agent) for a task
  context "<task>" [--top N]        rank scripts + Conventions sections; writes a context file for the agent
  review  "<task>" [--base REF]     decide light vs full review of the current diff
  bug     "<report>"                owner agent, area, severity, blocks-gameplay for a user bug report
  check                             validate questions offline (no API call)

Key comes only from env TYPESAFE_API_KEY. Every call is appended to logs/decisions.jsonl.
"""
import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parent.parent
CFG = json.loads((HERE / "jev_config.json").read_text(encoding="utf-8"))
MODEL_ORDER = ["haiku", "sonnet", "opus"]

sys.stdout.reconfigure(encoding="utf-8")


# ---------- API ----------

def ask(state, questions, tag):
    """One System One request. Retries twice on 429/529/network errors."""
    key = os.environ.get("TYPESAFE_API_KEY", "").strip()
    if not key:
        raise SystemExit(json.dumps({"error": "TYPESAFE_API_KEY not set"}))
    body = json.dumps({"state": state, "model": CFG["model"], "questions": questions}).encode("utf-8")
    req = urllib.request.Request(CFG["api_url"], data=body, method="POST", headers={
        "Authorization": f"Bearer {key}", "Content-Type": "application/json"})
    t0 = time.time()
    for attempt in range(3):
        try:
            with urllib.request.urlopen(req, timeout=CFG["timeout_s"]) as r:
                data = json.loads(r.read().decode("utf-8"))
            break
        except urllib.error.HTTPError as e:
            if e.code in (429, 529) and attempt < 2:
                time.sleep(1.5 * (attempt + 1))
                continue
            detail = e.read().decode("utf-8", "replace")[:400]
            raise SystemExit(json.dumps({"error": f"HTTP {e.code}", "detail": detail}))
        except urllib.error.URLError as e:
            if attempt < 2:
                time.sleep(1.5 * (attempt + 1))
                continue
            raise SystemExit(json.dumps({"error": f"network: {e.reason}"}))
    log(tag, len(questions), data.get("usage", {}), int((time.time() - t0) * 1000))
    return data["answers"], data.get("usage", {})


def ask_many(state, questions, tag):
    """Split a large question set into parallel requests (fan-out)."""
    items = list(questions.items())
    size = CFG["context"]["chunk_size"]
    chunks = [dict(items[i:i + size]) for i in range(0, len(items), size)]
    answers, tokens = {}, 0
    with ThreadPoolExecutor(max_workers=6) as pool:
        for a, u in pool.map(lambda c: ask(state, c, tag), chunks):
            answers.update(a)
            tokens += u.get("input_tokens", 0)
    return answers, tokens


def log(tag, n_questions, usage, ms):
    path = HERE / "logs" / "decisions.jsonl"
    path.parent.mkdir(exist_ok=True)
    rec = {"ts": time.strftime("%Y-%m-%dT%H:%M:%S"), "cmd": tag, "questions": n_questions,
           "input_tokens": usage.get("input_tokens"), "ms": ms}
    with path.open("a", encoding="utf-8") as f:
        f.write(json.dumps(rec) + "\n")


def out(obj):
    print(json.dumps(obj, ensure_ascii=False, separators=(",", ":")))


def r2(x):
    return round(float(x), 2)


# ---------- shared menus (rebuilt from the repo each run) ----------

def agents(active_only=True):
    """name -> one-line description, read from .claude/agents/*.md frontmatter."""
    res = {}
    for p in sorted((REPO / ".claude" / "agents").glob("*.md")):
        m = re.search(r"^name:\s*(\S+).*?^description:\s*(.+?)$", p.read_text(encoding="utf-8"), re.M | re.S)
        if m and (not active_only or m.group(1) in CFG["active_agents"]):
            res[m.group(1)] = m.group(2).strip()[:400]
    return res


MODEL_CRITERIA = {
    "haiku": "Small, fully specified change in one or two files: adjust values, rename, fix text or a field, "
             "write a checklist or a simple test. No design decision and no investigation needed.",
    "sonnet": "Normal feature or bug fix across several files following existing patterns, routine Unity editor "
              "work, or a standard code review.",
    "opus": "Open-ended design or planning, cross-module architecture, ambiguous requirements, a bug whose cause "
            "is unknown, or large scene/level work needing judgment.",
}


# ---------- route ----------

def route_questions(agent_menu):
    return {
        "model": {"type": "choice", "criteria": MODEL_CRITERIA, "instructions":
                  "Choose the cheapest AI model tier that can reliably complete `task` for a Unity C# mobile game team."},
        "owner": {"type": "choice", "criteria": agent_menu, "instructions":
                  "Choose the team member who should own `task`, based on each member's responsibility."},
        "complexity": {"type": "score", "instructions": "How much work and judgment does `task` need?", "criteria": [
            "Trivial: one obvious edit", "Small: a few related edits with a clear plan",
            "Medium: several files or some investigation", "Large: many files, design choices or unknown cause"]},
        "risky": {"type": "noul", "instructions":
                  "Does `task` change shared contracts between modules (Core interfaces/events, asmdef references), "
                  "project settings, tags/layers/input, or assemble/modify a scene where mistakes break other parts?"},
    }


def cmd_route(a):
    menu = agents()
    ans, usage = ask({"task": a.text}, route_questions(menu), "route")
    m, o = ans["model"], ans["owner"]
    rc = CFG["route"]
    owner = a.agent or o["choice"]
    default = rc["default_model"].get(owner, "sonnet")
    confident = m["confidence"] >= rc["confidence_threshold"]
    pick, reason = (m["choice"], "jev") if confident else (default, "low-confidence->default")
    if ans["risky"]["noul"] >= rc["risky_threshold"] and pick == "haiku":
        pick, reason = "sonnet", "risky->sonnet"
    floor = rc["min_model"].get(owner)
    if floor and MODEL_ORDER.index(pick) < MODEL_ORDER.index(floor):
        pick, reason = floor, "min_model"
    out({"model": pick, "owner": owner, "owner_jev": o["choice"], "owner_conf": r2(o["confidence"]),
         "jev_model": m["choice"], "model_conf": r2(m["confidence"]), "complexity": r2(ans["complexity"]["score"]),
         "risky": r2(ans["risky"]["noul"]), "reason": reason, "tokens": usage.get("input_tokens")})


# ---------- context ----------

def summarize_cs(path):
    text = path.read_text(encoding="utf-8", errors="replace")
    types = re.findall(r"\b(?:class|interface|struct|enum)\s+\w+(?:\s*:\s*[\w<>, .]+)?", text)
    summary = re.search(r"///\s*<summary>\s*(?:///\s*)?(.+?)\s*(?:///\s*)?</summary>", text, re.S)
    comment = summary.group(1) if summary else (re.search(r"^\s*//\s*(.+)$", text, re.M) or [None, ""])[1]
    comment = re.sub(r"\s*///\s*", " ", comment or "").strip()[:220]
    methods = re.findall(r"public\s+(?:static\s+|override\s+|virtual\s+)*[\w<>\[\],]+\s+(\w+)\s*\(", text)
    events = re.findall(r"public\s+(?:static\s+)?event\s+[\w<>, ]+\s+(\w+)", text)
    parts = [" | ".join(list(dict.fromkeys(t.strip() for t in types))[:3])]
    if comment:
        parts.append(comment)
    if methods or events:
        parts.append("API: " + ", ".join(dict.fromkeys(events + methods)) [:200])
    return " — ".join(p for p in parts if p)


def conventions_sections():
    """Split Conventions.md at ## and ### headings; ### keeps its parent title."""
    text = (REPO / CFG["context"]["conventions"]).read_text(encoding="utf-8")
    sections, title, parent, buf = [], "Preamble", "", []
    for line in text.splitlines():
        h = re.match(r"^(##+)\s+(.*)", line)
        if h and len(h.group(1)) <= 3:
            if buf:
                sections.append((title, "\n".join(buf).strip()))
            if len(h.group(1)) == 2:
                parent, title = h.group(2).strip(), h.group(2).strip()
            else:
                title = f"{parent} › {h.group(2).strip()}"
            buf = [line]
        else:
            buf.append(line)
    if buf:
        sections.append((title, "\n".join(buf).strip()))
    return [(t, b) for t, b in sections if t != "Preamble"]


RELEVANCE = ["Unrelated to the task", "Possibly useful background",
             "Must be read or edited to do the task"]


def cmd_context(a):
    cc = CFG["context"]
    files = sorted(p for root in cc["roots"] for p in (REPO / root).rglob("*.cs"))
    secs = conventions_sections()
    qs = {}
    for i, p in enumerate(files):
        qs[f"f{i}"] = {"type": "score", "criteria": RELEVANCE, "instructions": {
            "judgment": "Rate how much a developer working on `task` needs this source file of the Unity project.",
            "file": str(p.relative_to(REPO)).replace("\\", "/"), "summary": summarize_cs(p)}}
    for i, (t, body) in enumerate(secs):
        qs[f"s{i}"] = {"type": "score", "criteria": RELEVANCE, "instructions": {
            "judgment": "Rate how much a developer working on `task` needs this section of the team conventions.",
            "section": t, "excerpt": body[:600]}}
    ans, tokens = ask_many({"task": a.text}, qs, "context")

    ranked_files = sorted(((ans[f"f{i}"]["score"], p) for i, p in enumerate(files)), key=lambda x: -x[0])
    top = a.top or cc["top_files"]
    chosen = [(s, p) for s, p in ranked_files[:top] if s >= cc["min_file_score"]]
    ranked_secs = sorted(((ans[f"s{i}"]["score"], i) for i in range(len(secs))), key=lambda x: -x[0])
    keep = {i for s, i in ranked_secs[:cc["top_sections"]] if s >= cc["min_section_score"]}
    keep |= {i for i, (t, _) in enumerate(secs) if t in cc["always_sections"]}

    slug = hashlib.sha1(a.text.encode("utf-8")).hexdigest()[:8]
    path = HERE / "out" / f"ctx-{slug}.md"
    path.parent.mkdir(exist_ok=True)
    lines = [f"# Ngữ cảnh cho task (Jev chọn)\n\n> Task: {a.text}\n",
             "## File nên đọc trước (xếp theo độ liên quan 0–2)\n"]
    lines += [f"- `{str(p.relative_to(REPO)).replace(chr(92), '/')}` ({r2(s)}) — {summarize_cs(p)}" for s, p in chosen]
    lines += ["\nChỉ mở thêm file khác khi thật sự cần (vd. theo tham chiếu trong các file trên).\n",
              "## Trích Conventions liên quan\n",
              f"(Đầy đủ: `{cc['conventions']}` — chỉ tra khi cần mục khác.)\n"]
    lines += [secs[i][1] + "\n" for i in sorted(keep)]
    path.write_text("\n".join(lines), encoding="utf-8")
    out({"context_file": str(path.relative_to(REPO)).replace("\\", "/"),
         "files": [str(p.relative_to(REPO)).replace("\\", "/") for _, p in chosen],
         "sections": [secs[i][0] for i in sorted(keep)],
         "chars": path.stat().st_size, "jev_tokens": tokens})


# ---------- review ----------

def git(*args):
    return subprocess.run(["git", *args], cwd=REPO, capture_output=True, text=True,
                          encoding="utf-8", errors="replace").stdout


TEXT_SUFFIXES = {".cs", ".md", ".json", ".asmdef", ".py", ".txt", ".uss", ".uxml", ".shader", ".hlsl",
                 ".asset", ".cmd", ".ps1", ".sh", ".yml", ".yaml", ".gitignore"}


def cmd_review(a):
    rc = CFG["review"]
    base = a.base or "HEAD"
    changed = [l for l in git("diff", "--name-only", base).splitlines() if l]
    untracked = [l for l in git("ls-files", "--others", "--exclude-standard").splitlines()
                 if l and not l.endswith(".meta")]
    if a.paths:
        keep = tuple(p.rstrip("/") for p in a.paths)
        changed = [c for c in changed if c.startswith(keep)]
        untracked = [u for u in untracked if u.startswith(keep)]
    files = changed + untracked
    if not files:
        return out({"mode": "none", "reason": "no changes"})
    hits = sorted({f for f in files for pat in rc["risky_paths"] if re.search(pat, f)})

    diff = git("diff", base, "--", *changed) if changed else ""
    for u in untracked:
        p = REPO / u
        if p.suffix in TEXT_SUFFIXES and p.is_file() and p.stat().st_size < 40000:
            diff += f"\n+++ new file {u}\n" + p.read_text(encoding="utf-8", errors="replace")
    diff = diff[:rc["max_diff_chars"]]

    qs = {
        "contract": {"type": "noul", "instructions":
                     "Does `diff` change public APIs, events, interfaces or data types that other modules or "
                     "prefabs/scenes depend on (not just private implementation)?"},
        "scope": {"type": "noul", "instructions":
                  "Does `diff` change behaviour beyond what `task` asks for?"},
        "risk": {"type": "score", "instructions": "How risky is `diff` for introducing bugs in the game?", "criteria": [
            "Cosmetic: text, comments, values, docs", "Low: local logic change in one place",
            "Medium: new logic or several interacting changes", "High: lifecycle, state machines, event wiring or "
            "concurrency changes that can break gameplay"]},
    }
    ans, usage = ask({"task": a.text, "files": files, "diff": diff}, qs, "review")
    risk, contract, scope = ans["risk"]["score"], ans["contract"]["noul"], ans["scope"]["noul"]
    full = bool(hits) or risk >= rc["full_risk_score"] or contract >= rc["contract_threshold"]
    out({"mode": "full" if full else "light", "reviewer_model": "sonnet" if full else "haiku",
         "risky_paths": hits[:5], "risky_count": len(hits), "risk": r2(risk), "contract": r2(contract), "out_of_scope": r2(scope),
         "files": len(files), "diff_truncated": len(diff) >= rc["max_diff_chars"],
         "tokens": usage.get("input_tokens")})


# ---------- bug ----------

def script_areas():
    root = REPO / "Assets/_Game/Scripts"
    areas = {d.name: f"Code in Assets/_Game/Scripts/{d.name}" for d in sorted(root.iterdir()) if d.is_dir()}
    areas["Level"] = "Level geometry, cover, spawn points, lighting, scene layout"
    areas["Unknown"] = "Not enough information to tell where the problem is"
    return areas


def bug_questions():
    return {
        "owner": {"type": "choice", "criteria": agents(), "instructions":
                  "Which team member should fix the bug described in `report`?"},
        "area": {"type": "choice", "criteria": script_areas(), "instructions":
                 "Which part of the Unity project most likely contains the cause of the bug in `report`?"},
        "severity": {"type": "score", "instructions": "How severe is the bug in `report` for players?", "criteria": [
            "Cosmetic: visual or text only", "Minor: small wrong behaviour, easy to ignore",
            "Major: wrong gameplay result in a rare case", "Serious: wrong gameplay result in a common case",
            "Critical: crash, softlock, or the game cannot continue"]},
        "blocks": {"type": "noul", "instructions": "Does the bug in `report` stop the player from finishing a stage?"},
        "clear": {"type": "noul", "instructions":
                  "Does `report` give enough detail (what happened, where, expected result) to start fixing?"},
    }


def cmd_bug(a):
    ans, usage = ask({"report": a.text}, bug_questions(), "bug")
    out({"owner": ans["owner"]["choice"], "owner_conf": r2(ans["owner"]["confidence"]),
         "area": ans["area"]["choice"], "severity": r2(ans["severity"]["score"]),
         "blocks": r2(ans["blocks"]["noul"]), "clear": r2(ans["clear"]["noul"]),
         "tokens": usage.get("input_tokens")})


# ---------- check ----------

def cmd_check(_):
    qsets = {"route": route_questions(agents()), "bug": bug_questions()}
    problems = []
    for name, qs in qsets.items():
        for qid, q in qs.items():
            c = q.get("criteria")
            if q["type"] == "choice" and not (isinstance(c, dict) and 1 <= len(c) <= 255):
                problems.append(f"{name}.{qid}: choice criteria must be dict of 1-255")
            if q["type"] == "score" and not (isinstance(c, list) and 2 <= len(c) <= 10):
                problems.append(f"{name}.{qid}: score criteria must be list of 2-10")
    files = sum(1 for r in CFG["context"]["roots"] for _ in (REPO / r).rglob("*.cs"))
    out({"ok": not problems, "problems": problems, "agents": list(agents()), "scripts": files,
         "conventions_sections": len(conventions_sections()), "key_set": bool(os.environ.get("TYPESAFE_API_KEY"))})


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)
    p = sub.add_parser("route"); p.add_argument("text"); p.add_argument("--agent")
    p = sub.add_parser("context"); p.add_argument("text"); p.add_argument("--top", type=int)
    p = sub.add_parser("review"); p.add_argument("text"); p.add_argument("--base")
    p.add_argument("--paths", nargs="*", help="only consider changes under these paths")
    p = sub.add_parser("bug"); p.add_argument("text")
    sub.add_parser("check")
    a = ap.parse_args()
    {"route": cmd_route, "context": cmd_context, "review": cmd_review, "bug": cmd_bug, "check": cmd_check}[a.cmd](a)


if __name__ == "__main__":
    main()
