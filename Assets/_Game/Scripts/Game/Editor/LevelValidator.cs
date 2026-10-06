using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Unity.Mathematics;
using ClaudeCop.Camera;
using ClaudeCop.Core;
using ClaudeCop.Enemy;
using ClaudeCop.Game.Validation;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// Kiem tra level bang MOT lenh, khong can Play mode, khong sua scene. Menu ClaudeCop/Validate Level hoac
    /// LevelValidator.Run(scenePath). In mot khoi Debug.Log ngan, ban day du ghi vao Docs/Team/Reviews/validate-ten-scene.txt.
    /// Nguong nam trong Assets/_Game/Settings/LevelValidationRules.asset.
    /// </summary>
    public static class LevelValidator
    {
        public const string DefaultScene = "Assets/_Game/Scenes/Gameplay/Level_01.unity";
        public const string RulesPath = "Assets/_Game/Settings/LevelValidationRules.asset";
        const string OutDir = "Docs/Team/Reviews";

        enum St { Pass, Warn, Fail }

        class Group
        {
            public string id, title, summary = ""; public St st = St.Pass; public List<string> details = new List<string>();
            public void Fail(string m) { st = St.Fail; details.Add("FAIL " + m); }
            public void Warn(string m) { if (st == St.Pass) st = St.Warn; details.Add("WARN " + m); }
            public void Info(string m) { details.Add("     " + m); }
        }

        struct Tgt { public string name, kind; public Vector3 pos; public Transform spawn; }
        class WaveInfo { public EncounterWave wave; public CameraShot shot; public List<Tgt> targets = new List<Tgt>(); }

        [MenuItem("ClaudeCop/Validate Level")]
        public static void Menu() { Run(null); }

        public static LevelValidationRules LoadOrCreateRules()
        {
            var r = AssetDatabase.LoadAssetAtPath<LevelValidationRules>(RulesPath);
            if (r != null) return r;
            r = ScriptableObject.CreateInstance<LevelValidationRules>();
            AssetDatabase.CreateAsset(r, RulesPath);
            AssetDatabase.SaveAssets();
            return r;
        }

        static PhaseDirector FindDirector(Scene s)
        {
            if (!s.IsValid() || !s.isLoaded) return null;
            foreach (var g in s.GetRootGameObjects()) { var d = g.GetComponentInChildren<PhaseDirector>(true); if (d != null) return d; }
            return null;
        }

        /// <summary>Chay validator. scenePath null = scene dang mo co PhaseDirector, neu khong thi Level_01 (mo additive roi dong). Tra ve khoi tom tat.</summary>
        public static string Run(string scenePath = null)
        {
            if (EditorApplication.isPlaying) return Emit("[LevelValidator] Thoat Play mode truoc khi chay (tool chi chay o Edit mode).", null, null);
            var rules = LoadOrCreateRules();
            Scene scene = default; bool opened = false;
            try
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    var a = SceneManager.GetActiveScene();
                    if (FindDirector(a) != null) scene = a; else scenePath = DefaultScene;
                }
                if (!scene.IsValid())
                {
                    scene = SceneManager.GetSceneByPath(scenePath);
                    if (!scene.isLoaded)
                    {
                        if (!File.Exists(scenePath)) return Emit("[LevelValidator] Khong thay scene " + scenePath, null, null);
                        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive); opened = true;
                    }
                }
                return Validate(scene, rules);
            }
            finally { if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true); }
        }

        static string Emit(string msg, string sceneName, string full)
        {
            Debug.Log(msg);
            if (sceneName != null && full != null)
            {
                try
                {
                    string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, OutDir);
                    Directory.CreateDirectory(dir);
                    File.WriteAllText(Path.Combine(dir, "validate-" + sceneName + ".txt"), full, new UTF8Encoding(false));
                }
                catch (Exception e) { Debug.LogWarning("[LevelValidator] Khong ghi duoc file: " + e.Message); }
            }
            return msg;
        }

        // ------------------------------------------------------------------
        static string Validate(Scene scene, LevelValidationRules rules)
        {
            string sceneName = scene.name;
            var rule = rules.RuleFor(sceneName);
            var director = FindDirector(scene);
            var groups = new List<Group>();
            Group G(string id, string title) { var g = new Group { id = id, title = title }; groups.Add(g); return g; }

            if (director == null) return Emit("[LevelValidator] " + sceneName + ": khong co PhaseDirector.", sceneName, "no PhaseDirector");
            var profile = director.profile;
            var shots = new List<CameraShot>(); var shotPhase = new List<int>();
            for (int p = 0; p < director.phases.Count; p++)
                foreach (var s in director.phases[p].shots) { shots.Add(s); shotPhase.Add(p); }

            var all = new List<Transform>();
            foreach (var r in scene.GetRootGameObjects()) all.AddRange(r.GetComponentsInChildren<Transform>(true));

            // ----- thu thap wave / muc tieu
            var waves = new List<WaveInfo>();
            var gI = G("i", "Tham chieu (missing/null)");
            foreach (var r in scene.GetRootGameObjects())
                foreach (var w in r.GetComponentsInChildren<EncounterWave>(true))
                    waves.Add(ReadWave(w, shots, rules, gI));
            for (int i = 0; i < shots.Count; i++)
            {
                var s = shots[i];
                if (s == null) { gI.Fail("Phase " + (shotPhase[i] + 1) + " co shot null"); continue; }
                if (s.kind == ShotKind.Move && (s.spline == null || s.spline.Splines.Count == 0)) gI.Fail(s.name + ": Move thieu spline");
                if (s.kind == ShotKind.Combat && s.encounter == null && s.dwell <= 0f) gI.Warn(s.name + ": Combat khong co encounter va dwell=0");
                if (s.kind == ShotKind.Combat && s.encounter != null && s.frameTargets.Count == 0) gI.Warn(s.name + ": frameTargets rong");
            }
            if (director.railCamera == null) gI.Warn("PhaseDirector.railCamera null");
            if (director.brain == null) gI.Warn("PhaseDirector.brain null");
            foreach (var wi in waves) if (wi.shot == null) gI.Warn(wi.wave.name + ": khong shot nao tro toi wave nay");
            gI.summary = waves.Count + " wave, " + shots.Count + " shot, " + CountNon(gI) + " van de";

            // ----- (a) khoang cach, (b) vua khung, (c) tam nhin
            var gA = G("a", "Khoang cach >= " + rules.minTargetDistance + " m" + (rules.maxTargetDistance > 0f ? ", <= " + rules.maxTargetDistance + " m" : ""));
            var gB = G("b", "Vua khung 9:16 + 9:19.5");
            var gC = G("c", "Tam nhin (raycast)");
            float minDist = float.MaxValue, maxH = 0f, maxW = 0f, maxFovP = 0f, maxFovT = 0f; int blocked = 0, rays = 0;
            Physics.SyncTransforms();
            foreach (var wi in waves)
            {
                if (wi.shot == null || wi.targets.Count == 0) continue;
                Vector3 cp = wi.shot.transform.position; Quaternion cr = wi.shot.transform.rotation;
                var pts = new List<Vector3>();
                foreach (var t in wi.targets)
                {
                    pts.Add(t.pos);
                    float d = Vector3.Distance(cp, t.pos);
                    bool pick = t.kind == "Pickup";
                    float need = pick ? rules.minPickupDistance : rules.minTargetDistance;
                    if (!pick) minDist = Mathf.Min(minDist, d);
                    if (d < need - 1e-3f)
                    {
                        string m = wi.shot.name + " -> " + t.name + " " + d.ToString("0.0") + " m < " + need;
                        if (pick) gA.Warn(m); else gA.Fail(m);
                    }
                    if (!pick && rules.maxTargetDistance > 0f && d > rules.maxTargetDistance + 1e-3f)
                        gA.Fail(wi.shot.name + " -> " + t.name + " " + d.ToString("0.0") + " m > " + rules.maxTargetDistance);
                    // raycast moi muc tieu
                    {
                        rays++;
                        string blk = FirstBlocker(cp, t, rules);
                        if (blk != null) { blocked++; gC.Fail(wi.shot.name + " -> " + t.name + " bi chan boi " + blk); }
                    }
                }
                LevelGeometry.ClusterSpread(cp, cr * Vector3.forward, pts, out float width, out float mabs);
                maxH = Mathf.Max(maxH, mabs); maxW = Mathf.Max(maxW, width);
                if (mabs > rules.maxHorizontalOffsetDeg + 1e-3f) gB.Fail(wi.shot.name + ": lech ngang " + mabs.ToString("0.0") + " > " + rules.maxHorizontalOffsetDeg);
                if (width > rules.maxClusterWidthDeg + 1e-3f) gB.Fail(wi.shot.name + ": cum rong " + width.ToString("0.0") + " > " + rules.maxClusterWidthDeg);
                float fp = LevelGeometry.RequiredVerticalFov(cp, cr, pts, rules.aspectPortrait, rules.fovMarginDeg);
                float ft = LevelGeometry.RequiredVerticalFov(cp, cr, pts, rules.aspectTall, rules.fovMarginDeg);
                maxFovP = Mathf.Max(maxFovP, fp); maxFovT = Mathf.Max(maxFovT, ft);
                if (fp > rules.maxVerticalFov + 1e-3f) gB.Fail(wi.shot.name + ": can FOV " + fp.ToString("0.0") + " > " + rules.maxVerticalFov + " o 9:16");
                if (ft > rules.maxVerticalFov + 1e-3f) gB.Fail(wi.shot.name + ": can FOV " + ft.ToString("0.0") + " > " + rules.maxVerticalFov + " o 9:19.5");
                // frameTargets phai co du muc tieu (AutoFrame dung chung)
                foreach (var t in wi.targets)
                {
                    bool found = false;
                    foreach (var f in wi.shot.frameTargets) if (Vector3.Distance(f, t.pos) < 0.3f) { found = true; break; }
                    if (!found) { gB.Warn(wi.shot.name + ": frameTargets thieu " + t.name); break; }
                }
            }
            gA.summary = "min " + (minDist == float.MaxValue ? "-" : minDist.ToString("0.0")) + " m";
            gB.summary = "|h| max " + maxH.ToString("0.0") + " (<=" + rules.maxHorizontalOffsetDeg + "), rong max " + maxW.ToString("0.0") + " (<=" + rules.maxClusterWidthDeg + "), FOV can " + maxFovP.ToString("0.0") + "/" + maxFovT.ToString("0.0") + " (<=" + rules.maxVerticalFov + ")";
            gC.summary = blocked + "/" + rays + " tia bi chan";

            // ----- (d) noi shot/rail
            var gD = G("d", "Noi shot/rail");
            float worstPos = 0f, worstAng = 0f; int pairs = 0;
            for (int i = 1; i < shots.Count; i++)
            {
                var a = shots[i - 1]; var b = shots[i];
                if (a == null || b == null) continue;
                if (b.entry == ShotEntry.Cut) continue;
                string pn = a.name + " -> " + b.name; pairs++;
                if (a.kind == ShotKind.Combat && b.kind == ShotKind.Combat)
                {
                    float ang = LevelGeometry.AngleBetween(a.transform.forward, b.transform.forward);
                    float dist = Vector3.Distance(a.transform.position, b.transform.position);
                    worstAng = Mathf.Max(worstAng, ang);
                    if (ang > rules.combatPairMaxAngle + 1e-3f) gD.Fail(pn + ": lech huong " + ang.ToString("0.0") + " > " + rules.combatPairMaxAngle);
                    if (dist > rules.combatPairMaxDistance + 1e-3f) gD.Fail(pn + ": dich " + dist.ToString("0.00") + " m > " + rules.combatPairMaxDistance);
                    continue;
                }
                if (a.kind == ShotKind.Move && !TryRail(a, true, out Vector3 ep, out Vector3 ed)) { gD.Fail(pn + ": khong doc duoc rail"); continue; }
                if (a.kind == ShotKind.Move && b.kind == ShotKind.Move) { /* Move->Move: rail cuoi vs rail dau */ }
                Vector3 aPos, aDir; bool aSkipDir = false;
                if (a.kind == ShotKind.Move) { TryRail(a, true, out aPos, out aDir); aSkipDir = a.lookKeys.Count > 0; }
                else { aPos = a.transform.position; aDir = a.transform.forward; }
                Vector3 bPos, bDir; bool bSkipDir = false;
                if (b.kind == ShotKind.Move) { if (!TryRail(b, false, out bPos, out bDir)) { gD.Fail(pn + ": khong doc duoc rail"); continue; } bSkipDir = b.lookKeys.Count > 0; }
                else { bPos = b.transform.position; bDir = b.transform.forward; }
                float dp = Vector3.Distance(aPos, bPos); worstPos = Mathf.Max(worstPos, dp);
                if (dp > rules.railJoinPosTolerance) gD.Fail(pn + ": vi tri lech " + dp.ToString("0.000") + " m > " + rules.railJoinPosTolerance);
                if (!aSkipDir && !bSkipDir)
                {
                    float da = LevelGeometry.AngleBetween(aDir, bDir); worstAng = Mathf.Max(worstAng, da);
                    if (da > rules.railJoinAngleTolerance) gD.Fail(pn + ": huong lech " + da.ToString("0.0") + " > " + rules.railJoinAngleTolerance);
                }
            }
            gD.summary = pairs + " cap, lech vi tri max " + worstPos.ToString("0.000") + " m (rail), goc max " + worstAng.ToString("0.0");

            // ----- (e) thoi gian Move
            var gE = G("e", "Thoi gian Move <= " + rules.maxMoveSeconds + " s");
            float maxT = 0f; int moves = 0;
            foreach (var s in shots)
            {
                if (s == null || s.kind != ShotKind.Move || s.spline == null || s.spline.Splines.Count == 0) continue;
                moves++;
                float len = s.spline.CalculateLength(0);
                float t = LevelGeometry.MoveSeconds(len, profile.railSpeed, s.speedOverride, profile.easeInTime, profile.easeOutTime, profile.maxMoveSeconds, profile.maxRailSpeed, s.dwell);
                maxT = Mathf.Max(maxT, t);
                gE.Info(s.name + ": " + len.ToString("0.0") + " m, " + t.ToString("0.0") + " s");
                if (t > rules.maxMoveSeconds + 1e-3f) gE.Fail(s.name + ": " + t.ToString("0.0") + " s > " + rules.maxMoveSeconds + " (dai " + len.ToString("0.0") + " m)");
            }
            gE.summary = moves + " rail, max " + maxT.ToString("0.0") + " s";

            // ----- (f) tuyen
            var gF = G("f", "Tuyen (yaw, dao chieu, cat cheo)");
            float worstYaw = 0f; int worstRev = 0; int crossings = 0;
            for (int p = 0; p < director.phases.Count; p++)
            {
                var pos2 = new List<Vector2>(); var yaws = new List<float>();
                foreach (var s in director.phases[p].shots)
                {
                    if (s == null) continue;
                    if (s.kind == ShotKind.Combat) { var pp = s.transform.position; pos2.Add(new Vector2(pp.x, pp.z)); yaws.Add(LevelGeometry.Yaw(s.transform.forward)); }
                    else if (s.spline != null && s.spline.Splines.Count > 0)
                    {
                        int n = Mathf.Max(2, rules.railSamples);
                        for (int k = 0; k <= n; k++)
                        {
                            SplineUtility.Evaluate(s.spline.Splines[0], k / (float)n, out float3 lp, out float3 lt, out float3 lu);
                            Vector3 wp = s.spline.transform.TransformPoint((Vector3)lp);
                            Vector3 wd = s.spline.transform.TransformDirection((Vector3)lt);
                            pos2.Add(new Vector2(wp.x, wp.z)); if (wd.sqrMagnitude > 1e-8f) yaws.Add(LevelGeometry.Yaw(wd));
                        }
                    }
                }
                float net = LevelGeometry.NetYaw(yaws), abs = LevelGeometry.AbsYaw(yaws);
                int rev = LevelGeometry.CountTurnReversals(yaws, rules.turnReversalMinDeg);
                worstYaw = Mathf.Max(worstYaw, Mathf.Abs(net)); worstRev = Mathf.Max(worstRev, rev);
                string pn = "Phase " + (p + 1);
                gF.Info(pn + ": yaw rong " + net.ToString("0") + ", tong " + abs.ToString("0") + ", dao chieu " + rev);
                if (Mathf.Abs(net) > rules.maxCumulativeYaw) gF.Fail(pn + ": yaw tich luy " + net.ToString("0") + " > " + rules.maxCumulativeYaw);
                if (rev > rules.maxTurnReversals) gF.Fail(pn + ": dao chieu re " + rev + " lan > " + rules.maxTurnReversals);
                var poly = LevelGeometry.Simplify(pos2, rules.routeMinSegment);
                if (LevelGeometry.FindSelfIntersection(poly, out int sa, out int sb)) { crossings++; gF.Fail(pn + ": tuyen tu cat cheo (doan " + sa + " x " + sb + ", quay vong lai)"); }
            }
            gF.summary = "yaw rong max " + worstYaw.ToString("0") + " (<=" + rules.maxCumulativeYaw + "), dao chieu max " + worstRev + " (<=" + rules.maxTurnReversals + "), cat cheo " + crossings;

            // ----- (g) so luong + marker cam
            var gG = G("g", "So luong enemy + marker cam (" + (rule == rules.fallbackRule ? "fallback" : rule.sceneNameContains) + ")");
            int total = 0, maxWave = 0;
            foreach (var wi in waves)
            {
                int ne = 0; foreach (var t in wi.targets) if (t.kind == "Enemy" || t.kind == "Grenadier" || t.kind == "Shield") ne++;
                total += ne; maxWave = Mathf.Max(maxWave, ne);
                int conc = ReadInt(wi.wave, "maxConcurrent");
                gG.Info(wi.wave.name + ": " + ne + " enemy, maxConcurrent " + conc);
                if (ne > rule.maxEnemiesPerWave) gG.Fail(wi.wave.name + ": " + ne + " enemy > " + rule.maxEnemiesPerWave + "/wave");
                if (rule.maxConcurrentCap > 0 && ne > rule.maxConcurrentCap && (conc <= 0 || conc > rule.maxConcurrentCap))
                    gG.Fail(wi.wave.name + ": maxConcurrent " + conc + " (can 1.." + rule.maxConcurrentCap + " vi " + ne + " enemy)");
            }
            string totalNote = "tong <=" + rule.maxEnemiesTotal;
            if (rule.maxEnemiesPerLevel > 0)
            {
                // Scene chuoi: dem theo level con; tong <= maxEnemiesPerLevel x so level, moi level <= rule rieng cua no.
                var perLevel = new SortedDictionary<string, int>();
                foreach (var wi in waves)
                {
                    int ne = 0; string root = null;
                    foreach (var t in wi.targets)
                    {
                        if (t.spawn != null && root == null && t.spawn.root.name.StartsWith("Level_", StringComparison.Ordinal)) root = t.spawn.root.name;
                        if (t.kind == "Enemy" || t.kind == "Grenadier" || t.kind == "Shield") ne++;
                    }
                    string key = LevelValidationRules.LevelKeyOf(wi.wave.name, root);
                    perLevel.TryGetValue(key, out int c); perLevel[key] = c + ne;
                }
                int limit = LevelValidationRules.ChainTotalLimit(rule, perLevel.Count);
                var parts = new List<string>();
                foreach (var kv in perLevel)
                {
                    int lim = rules.PerLevelLimit(rule, kv.Key);
                    parts.Add(kv.Key + " " + kv.Value + "/" + lim);
                    if (kv.Value > lim) gG.Fail(kv.Key + ": " + kv.Value + " enemy > " + lim + " (rule level)");
                }
                gG.Info("Theo level: " + string.Join(", ", parts));
                if (total > limit) gG.Fail("Tong " + total + " enemy > " + limit + " (" + rule.maxEnemiesPerLevel + " x " + perLevel.Count + " level)");
                totalNote = "tong <=" + limit + " = " + rule.maxEnemiesPerLevel + "x" + perLevel.Count + " level";
            }
            else if (total > rule.maxEnemiesTotal) gG.Fail("Tong " + total + " enemy > " + rule.maxEnemiesTotal);
            int forbidden = 0;
            CountForbidden(all, rule.forbidHostage, rules.hostagePrefixes, "con tin", gG, ref forbidden);
            CountForbidden(all, rule.forbidBarrel, rules.barrelPrefixes, "thung no", gG, ref forbidden);
            CountForbidden(all, rule.forbidWeaponCrate, rules.weaponCratePrefixes, "thung sung", gG, ref forbidden);
            CountForbidden(all, rule.forbidHumanShield, rules.humanShieldPrefixes, "khien nguoi", gG, ref forbidden);
            CountForbidden(all, rule.forbidGrenadier, rules.grenadierPrefixes, "grenadier", gG, ref forbidden);
            gG.summary = total + " enemy (max/wave " + maxWave + " <=" + rule.maxEnemiesPerWave + ", " + totalNote + "), marker cam: " + forbidden;

            // ----- (h) ten
            var gH = G("h", "Ten object");
            var seen = new Dictionary<string, int>(); int bad = 0;
            var regs = new List<KeyValuePair<MarkerNameRule, Regex>>();
            foreach (var nr in rules.nameRules) regs.Add(new KeyValuePair<MarkerNameRule, Regex>(nr, new Regex(nr.regex)));
            foreach (var t in all)
            {
                foreach (var kv in regs)
                {
                    if (!t.name.StartsWith(kv.Key.prefix, StringComparison.Ordinal)) continue;
                    if (!kv.Value.IsMatch(t.name)) { bad++; gH.Warn("sai quy uoc " + kv.Key.label + ": " + PathOf(t)); }
                    string dk = t.root.name + "/" + t.name;   // chuoi level: moi level (root Level_XX) co bo marker rieng cung ten
                    if (seen.TryGetValue(dk, out int c)) { seen[dk] = c + 1; if (c == 1) gH.Fail("trung ten: " + dk); }
                    else seen[dk] = 1;
                    break;
                }
            }
            gH.summary = bad + " sai quy uoc, " + CountFail(gH) + " trung ten";

            // ----- (j) PropSlot vs Prop
            var gJ = G("j", "PropSlot khop Prop");
            var slots = new Dictionary<string, Transform>(); var props = new Dictionary<string, Transform>();
            foreach (var t in all)
            {
                string pn = StripLevelPrefix(t.name);   // chuoi level: Prop ghep voi tien to L2_/L3_
                if (t.name.StartsWith("PropSlot_", StringComparison.Ordinal)) slots[t.name.Substring("PropSlot_".Length)] = t;
                else if (pn.StartsWith("Prop_", StringComparison.Ordinal) && pn.Split('_').Length >= 5) props[pn.Substring("Prop_".Length)] = t;
            }
            int matched = 0, off = 0;
            foreach (var kv in slots)
            {
                if (!props.TryGetValue(kv.Key, out var pr)) { gJ.Warn("PropSlot_" + kv.Key + " khong co Prop_" + kv.Key + " trong scene"); continue; }
                float d = Vector3.Distance(kv.Value.position, pr.position);
                if (d > rules.propSlotTolerance) { off++; gJ.Fail("Prop_" + kv.Key + " lech slot " + d.ToString("0.00") + " m"); } else matched++;
            }
            foreach (var kv in props) if (!slots.ContainsKey(kv.Key)) gJ.Warn("Prop_" + kv.Key + " khong co PropSlot");
            gJ.summary = matched + "/" + slots.Count + " khop, " + off + " lech";

            // ----- ket qua
            groups.Sort((x, y) => string.CompareOrdinal(x.id, y.id));
            var full = new StringBuilder(); var shortSb = new StringBuilder();
            St worst = St.Pass; int fails = 0, warns = 0;
            foreach (var g in groups) { if (g.st > worst) worst = g.st; if (g.st == St.Fail) fails++; else if (g.st == St.Warn) warns++; }
            string head = "[LevelValidator] " + sceneName + ": " + worst.ToString().ToUpper() + " (" + fails + " FAIL, " + warns + " WARN) - " + shots.Count + " shot, " + waves.Count + " wave";
            full.AppendLine(head); shortSb.AppendLine(head);
            int shortLines = 1;
            foreach (var g in groups)
            {
                string line = "(" + g.id + ") " + g.st.ToString().ToUpper().PadRight(4) + " " + g.title + " :: " + g.summary;
                full.AppendLine(line); shortSb.AppendLine(line); shortLines++;
                int shown = 0;
                foreach (var d in g.details)
                {
                    full.AppendLine("      " + d);
                    if (g.st != St.Pass && !d.StartsWith("     ") && shown < 2 && shortLines < 38) { shortSb.AppendLine("      " + d); shown++; shortLines++; }
                }
                int hidden = 0; foreach (var d in g.details) if (!d.StartsWith("     ")) hidden++;
                if (hidden > shown && g.st != St.Pass && shortLines < 39) { shortSb.AppendLine("      ... +" + (hidden - shown) + " nua, xem file"); shortLines++; }
            }
            shortSb.Append("Chi tiet: " + OutDir + "/validate-" + sceneName + ".txt");
            return Emit(shortSb.ToString(), sceneName, full.ToString());
        }

        // ------------------------------------------------------------------
        /// <summary>Bo tien to level cua chuoi (vd. "L2_Prop_Glass_..." -> "Prop_Glass_...").</summary>
        static string StripLevelPrefix(string n) => Regex.Replace(n, @"^L\d+_", "");

        static int CountNon(Group g) { int n = 0; foreach (var d in g.details) if (!d.StartsWith("     ")) n++; return n; }
        static int CountFail(Group g) { int n = 0; foreach (var d in g.details) if (d.StartsWith("FAIL")) n++; return n; }

        static string PathOf(Transform t) { var s = t.name; for (var p = t.parent; p != null; p = p.parent) s = p.name + "/" + s; return s; }

        static int ReadInt(UnityEngine.Object o, string field)
        {
            var p = new SerializedObject(o).FindProperty(field); return p != null ? p.intValue : 0;
        }

        static void CountForbidden(List<Transform> all, bool forbid, List<string> prefixes, string label, Group g, ref int total)
        {
            if (!forbid) return;
            int n = 0; var names = new List<string>();
            foreach (var t in all)
                foreach (var pf in prefixes)
                    if (t.name.StartsWith(pf, StringComparison.Ordinal)) { n++; if (names.Count < 3) names.Add(t.name); break; }
            if (n > 0) { total += n; g.Fail("level cam " + label + " nhung co " + n + " marker (vd " + string.Join(", ", names) + ")"); }
        }

        static bool TryRail(CameraShot s, bool end, out Vector3 pos, out Vector3 dir)
        {
            pos = default; dir = Vector3.forward;
            if (s.spline == null || s.spline.Splines.Count == 0) return false;
            var sp = s.spline.Splines[0];
            float t = end ? 1f : 0f;
            SplineUtility.Evaluate(sp, t, out float3 lp, out float3 lt, out float3 lu);
            if (math.lengthsq(lt) < 1e-10f) SplineUtility.Evaluate(sp, end ? 0.99f : 0.01f, out lp, out lt, out lu);
            pos = s.spline.transform.TransformPoint((Vector3)lp);
            dir = s.spline.transform.TransformDirection((Vector3)lt).normalized;
            return true;
        }

        static string FirstBlocker(Vector3 from, Tgt t, LevelValidationRules rules)
        {
            Vector3 d = t.pos - from; float dist = d.magnitude; if (dist < 0.01f) return null;
            var hits = Physics.RaycastAll(from, d / dist, dist, rules.sightlineMask, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var h in hits)
            {
                if (h.distance > dist - rules.sightlineEndTolerance) break;
                if (t.spawn != null && h.collider.transform.IsChildOf(t.spawn)) continue;
                bool ign = false;
                string cn = StripLevelPrefix(h.collider.name), rn = StripLevelPrefix(h.collider.transform.root.name);
                foreach (var pf in rules.sightlineIgnorePrefixes) if (cn.StartsWith(pf, StringComparison.Ordinal) || rn.StartsWith(pf, StringComparison.Ordinal)) { ign = true; break; }
                if (ign) continue;
                return h.collider.name + " @" + h.distance.ToString("0.0") + " m";
            }
            return null;
        }

        static WaveInfo ReadWave(EncounterWave w, List<CameraShot> shots, LevelValidationRules rules, Group gI)
        {
            var wi = new WaveInfo { wave = w };
            foreach (var s in shots) if (s != null && s.encounter == w) { wi.shot = s; break; }
            var so = new SerializedObject(w);
            void Lists(string field, string kind, float up, bool usePeek)
            {
                var p = so.FindProperty(field); if (p == null) return;
                for (int i = 0; i < p.arraySize; i++)
                {
                    var e = p.GetArrayElementAtIndex(i);
                    var tr = e.objectReferenceValue as Transform;
                    if (tr == null)
                    {
                        if (e.objectReferenceValue == null && e.objectReferenceInstanceIDValue != 0) gI.Fail(w.name + "." + field + "[" + i + "] tro toi object da bi xoa (missing)");
                        else gI.Fail(w.name + "." + field + "[" + i + "] = null");
                        continue;
                    }
                    Vector3 pos = tr.position;
                    if (usePeek) { var pk = tr.Find("Peek"); if (pk != null) pos = pk.position; }
                    wi.targets.Add(new Tgt { name = tr.name, kind = kind, pos = pos + Vector3.up * up, spawn = tr });
                }
            }
            Lists("spawnPoints", "Enemy", rules.aimHeight, true);
            Lists("hostageSpawnPoints", "Hostage", rules.aimHeight, true);
            Lists("grenadierSpawnPoints", "Grenadier", rules.aimHeight, true);
            Lists("humanShieldSpawnPoints", "Shield", rules.aimHeight, true);
            Lists("pickupSpawnPoints", "Pickup", 0.5f, false);
            foreach (var f in new[] { "sceneEnemies", "sceneHostages", "scenePickups" })
            {
                var p = so.FindProperty(f); if (p == null) continue;
                for (int i = 0; i < p.arraySize; i++)
                {
                    var e = p.GetArrayElementAtIndex(i);
                    if (e.objectReferenceValue == null) gI.Fail(w.name + "." + f + "[" + i + "] " + (e.objectReferenceInstanceIDValue != 0 ? "missing (da xoa)" : "null"));
                    // Enemy/con tin dat san trong scene (vd. enemy dung san) cung la muc tieu cua wave: tinh vao khoang cach/vua khung/tam nhin/so luong.
                    else if (e.objectReferenceValue is Component sc && (f == "sceneEnemies" || f == "sceneHostages"))
                        wi.targets.Add(new Tgt { name = sc.name, kind = f == "sceneEnemies" ? "Enemy" : "Hostage", pos = sc.transform.position + Vector3.up * rules.aimHeight, spawn = sc.transform });
                }
            }
            void NeedPrefab(string listField, string prefabField)
            {
                var l = so.FindProperty(listField); var pf = so.FindProperty(prefabField);
                if (l != null && pf != null && l.arraySize > 0 && pf.objectReferenceValue == null) gI.Fail(w.name + "." + prefabField + " null nhung co " + l.arraySize + " diem " + listField);
            }
            NeedPrefab("spawnPoints", "enemyPrefab"); NeedPrefab("hostageSpawnPoints", "hostagePrefab");
            NeedPrefab("grenadierSpawnPoints", "grenadierPrefab"); NeedPrefab("humanShieldSpawnPoints", "humanShieldPrefab");
            NeedPrefab("pickupSpawnPoints", "pickupPrefab");
            return wi;
        }
    }
}
