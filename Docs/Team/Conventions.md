# Quy ước chung — ClaudeCop2

> Mọi agent **đọc file này trước khi làm task**. Thay đổi file này phải qua PM.
> Cập nhật lần cuối: 2026-10-04 sau W2–W4 (bảng asmdef thực tế, TMP, URP Mobile, material blockout, tên `HostageActor`, file không commit). Sau đó: màn dọc, game offline (bỏ Jev online + jev-coder).

## 1. Phạm vi sở hữu
Game: rail shooter kiểu Virtua Cop 2 cho mobile — xem `Docs/Design/Plan_VirtuaCop2_Mobile.md`.

> ### ⚡ Đội gọn cho DEMO (M1 + M2) — chủ dự án chốt 2026-10-04
> Coder **đang hoạt động**: **gameplay-coder** và **ui-coder**, cùng level-designer, reviewer.
> - **gameplay-coder** sở hữu tạm thời **toàn bộ** thư mục của combat-coder, enemy-coder và jev-coder (Core, Combat, Props, FX, Enemy, Camera, Game, Jev + prefab tương ứng), ngoài phạm vi của chính mình.
> - **ui-coder** giữ nguyên phạm vi UI/Ads.
> - **combat-coder, enemy-coder tạm nghỉ** — nhận lại thư mục từ M3/M4. **jev-coder đã giải thể** (2026-10-04: game offline, bỏ Jev online/server) — `Scripts/Jev/` thuộc gameplay-coder.
> - Code vẫn tách module theo bảng asmdef mục 2. Quy tắc "không ai tham chiếu UI/Ads" vẫn áp dụng.
> - Reviewer review **một lần mỗi wave** (gộp).

| Agent | Model | Sở hữu (chỉ được tạo/sửa trong đây) |
|---|---|---|
| project-manager | opus | `Docs/Team/` |
| game-designer | opus | `Docs/Design/` |
| level-designer | opus | `Assets/_Game/Level/` (gồm material blockout), `Assets/_Game/Scenes/Levels/` |
| gameplay-coder | sonnet | `Assets/_Game/Scripts/Camera/`, `Assets/_Game/Scripts/Game/`, `Assets/_Game/Scripts/Jev/`, `Assets/_Game/Prefabs/Game/`, `Assets/_Game/Scenes/Gameplay/`, `Assets/_Game/Scenes/Title.unity`, `Assets/_Game/Settings/` + tài nguyên dùng chung (mục 4) + Player/Build Settings |
| combat-coder | sonnet | `Assets/_Game/Scripts/Core/`, `Assets/_Game/Scripts/Combat/`, `Assets/_Game/Scripts/Props/`, `Assets/_Game/Scripts/FX/`, `Assets/_Game/Prefabs/Combat/`, `Assets/_Game/Prefabs/Props/`, `Assets/_Game/Prefabs/FX/` |
| enemy-coder | sonnet | `Assets/_Game/Scripts/Enemy/`, `Assets/_Game/Prefabs/Enemies/` |
| ui-coder | sonnet | `Assets/_Game/Scripts/UI/`, `Assets/_Game/Scripts/Ads/`, `Assets/_Game/UI/` (gồm `UI/Fonts/`), `Assets/_Game/Prefabs/UI/`, `Assets/TextMesh Pro/` (TMP Essentials) |
| reviewer | sonnet | `Docs/Team/Reviews/` |

Mỗi coder còn có scene thử riêng: `Assets/_Game/Scenes/Sandbox/<agent-name>.unity` (hoặc `<agent-name>-<module>.unity`), chỉ chủ của nó được sửa.

**Không commit** (chủ dự án chốt, F-111): `Assets/Screenshots/` + `Assets/Screenshots.meta`, `ProjectSettings/Packages/com.unity.probuilder/`, `ProjectSettings/SceneTemplateSettings.json`. File không thuộc ai khác → không commit, báo PM/Liaison.

**Offline:** game không gọi mạng, không có API key hay server. Không thêm SDK/HTTP client vào game (quảng cáo thật nếu có làm sau sẽ quyết riêng).

## 2. Assembly Definition (asmdef)
Mỗi module một asmdef, do chủ thư mục tạo. Bảng dưới là **tham chiếu thực tế sau W4** — chỉ được tham chiếu theo bảng; cần thêm → hỏi PM.

> Trong DEMO: chủ thực tế của Core, Combat, FX, Enemy, Jev là **gameplay-coder**.

### Runtime
| Assembly | Thư mục | Tham chiếu | Chủ |
|---|---|---|---|
| `ClaudeCop.Core` | `Scripts/Core/` | _(không — `references: []`)_ — **đóng băng** | combat-coder |
| `ClaudeCop.Combat` | `Scripts/Combat/` (Props M3: asmdef riêng `ClaudeCop.Props`, cùng quy tắc) | Core, Unity.InputSystem | combat-coder |
| `ClaudeCop.FX` | `Scripts/FX/` | Core (FX tự nghe `CombatEvents` + raycast, không ai tham chiếu FX) | combat-coder |
| `ClaudeCop.Enemy` | `Scripts/Enemy/` | Core | enemy-coder |
| `ClaudeCop.Camera` | `Scripts/Camera/` | Core, Unity.Cinemachine, Unity.Splines, Unity.Mathematics | gameplay-coder |
| `ClaudeCop.Jev` | `Scripts/Jev/` (gồm `JevDirector`; chỉ offline) | Core, Enemy | gameplay-coder |
| `ClaudeCop.Game` | `Scripts/Game/` | Core, Combat, Enemy, Camera, Jev, Unity.Cinemachine (Camera/Jev/Cinemachine dùng cho bot debug, bọc define) | gameplay-coder |
| `ClaudeCop.Ads` | `Scripts/Ads/` | Unity.ugui, Unity.TextMeshPro (**không** ref Core/module game) | ui-coder |
| `ClaudeCop.UI` | `Scripts/UI/` | Core, Combat, Ads, Jev, Unity.ugui, Unity.TextMeshPro | ui-coder |

### Editor-only (`includePlatforms: [Editor]`)
| Assembly | Thư mục | Tham chiếu | Chủ |
|---|---|---|---|
| `ClaudeCop.Camera.Editor` | `Scripts/Camera/Editor/` | Core, Camera, Cinemachine, Splines, Mathematics | gameplay-coder |
| `ClaudeCop.Game.Editor` | `Scripts/Game/Editor/` (menu `ClaudeCop/Game/Assemble Level_01 (T-403)`) | Core, Camera, Camera.Editor, Enemy, Combat, Game, Cinemachine, Splines, Mathematics | gameplay-coder |
| `ClaudeCop.FX.Editor` | `Scripts/FX/Editor/` | Core, FX | combat-coder |
| `ClaudeCop.UI.Editor` | `Scripts/UI/Editor/` | UI, Ads, Jev, Core, ugui, TextMeshPro | ui-coder |

### Tests (Editor-only + `defineConstraints: UNITY_INCLUDE_TESTS`, ref thêm TestRunner)
`ClaudeCop.Core.Tests` (Core) · `ClaudeCop.Combat.Tests` (Core, Combat) · `ClaudeCop.Enemy.Tests` (Core, Enemy) · `ClaudeCop.Jev.Tests` (Core, Jev, Enemy) · `ClaudeCop.Game.Tests` (Core, Game) · `ClaudeCop.FX.Tests` (Core, FX) · `ClaudeCop.UI.Tests` (Core, Combat, UI, Ads, Jev, ugui, TMP). Mỗi asmdef Tests nằm trong `Tests/` của module, chủ = chủ module.

### Luật
- **Không ai được tham chiếu UI hoặc Ads** (ngoài UI và test/editor của UI). Gameplay báo cho UI bằng event C#.
- Module "thấp" cần gọi module "cao" → dùng **interface/event/service trong Core**. Combat biết UI đang che con trỏ qua hook tĩnh `TapShooter.PointerBlocker` (UI gán).
- Hợp đồng dùng chung đặt trong **Core**. **Core đã đóng băng sau R-W1** — muốn thêm/đổi gửi yêu cầu qua PM.
- Không tạo tham chiếu vòng. Namespace trùng tên assembly.
- Code chỉ dùng để thử (driver, dummy, bot, sandbox starter) phải bọc `#if UNITY_EDITOR || DEVELOPMENT_BUILD` hoặc nằm trong asmdef Editor riêng (F-106).

### Đặt tên tránh va chạm namespace (F-101, F-116)
- **Không đặt tên class trùng đoạn cuối của một namespace `ClaudeCop.*`** (Core, Combat, Enemy, Camera, Jev, Game, UI, Ads, Props, FX). Đã áp dụng: **`EnemyActor`** (không `Enemy`), **`HostageActor`** (không `Hostage`, cho đồng bộ), **`FxSystem`** (không `FX`).
- Trong `ClaudeCop.Camera` và mọi code tham chiếu assembly Camera (Game, Game.Editor, Camera.Editor…): **không viết `Camera` trần** — dùng `UnityEngine.Camera` hoặc alias `using UCamera = UnityEngine.Camera;`. Khuyến nghị dùng alias ở **mọi** module (UI/Combat hiện dùng `Camera` trần vẫn đúng vì chưa ref Camera, nhưng sẽ vỡ nếu thêm ref). **Không đổi namespace** `ClaudeCop.Camera`.

## 3. Làm việc chung trong một Unity Editor
- Nhiều agent có thể chạy song song trên cùng Editor. Sau `refresh_unity` → `read_console`:
  - Lỗi **trong thư mục của mình** → phải sửa đến 0 lỗi.
  - Lỗi **ngoài thư mục của mình** → **không sửa**, ghi vào báo cáo (file, lỗi).
- **Play mode: mỗi lúc chỉ 1 phiên agent được vào Play mode.** Trước khi Play phải được Liaison cho lượt; xong phải thoát Play mode trước khi kết thúc task. Thấy Editor đang Play do phiên khác → không đụng, ghi báo cáo.
- Không mở/sửa scene của agent khác. Thử nghiệm trong Sandbox scene của mình.
- Số liệu cân bằng: coder **tự thiết kế ScriptableObject** cho module mình, giá trị mặc định lấy từ `Docs/Design/` (PM ghi trong task). Không hard-code số cân bằng (nợ còn lại: F-203, dọn sau demo). Một con số chỉ có **một nguồn**.

## 4. Tài nguyên dùng chung (chủ: gameplay-coder)
Tags, Layers, Physics layer matrix, Player/Build/Quality Settings (`ProjectSettings/`), URP asset (`Assets/Settings/`), package (`Packages/manifest.json`), file `Assets/InputSystem_Actions.inputactions`.
Agent khác cần tag/layer/input/package mới → ghi yêu cầu trong báo cáo. Danh sách hiện hành (sau W4):

| Loại | Tên | Dùng cho |
|---|---|---|
| Tag | `MainCamera` (mặc định Unity) — không thêm tag game | |
| Layer | _(không có — môi trường để `Default`)_ | Chọn mục tiêu theo **màn hình** qua `TargetRegistry`; FX raycast môi trường |
| Input map | `Gameplay` | Map duy nhất cho gameplay |
| Input action | `Gameplay/Tap` — Button, `<Pointer>/press` | Bắn (tap); MG giữ để bắn |
| Input action | `Gameplay/TapPosition` — Value, `<Pointer>/position` | Vị trí tap |
| Input action | ~~`Gameplay/Hold`~~ | **Đã xoá** (F-108) |
| Multi-touch | Combat đọc trực tiếp `Touchscreen.touches` (ngón thứ 2 khi ngón 1 còn giữ) | F-108 — **chưa thử máy thật** |
| Package | `com.unity.splines` 2.8.2, `com.unity.cinemachine`, `com.unity.probuilder`, `com.unity.inputsystem`, URP, ugui (gồm TextMeshPro) | |
| TextMeshPro | **Đang dùng** cho toàn bộ UI. Essentials ở `Assets/TextMesh Pro/` (commit). Font tiếng Việt `Assets/_Game/UI/Fonts/ClaudeCop UI SDF.asset` (Dynamic, nguồn LiberationSans, đủ dấu). Không dùng `UnityEngine.UI.Text` cho chữ mới | |
| Player Settings | Android, **portrait-only** (màn hình dọc, đổi 2026-10-04), ARM64 | |
| Quality / URP | Mức `Mobile`, URP asset `Assets/Settings/Mobile_RPAsset.asset`: **MSAA 2×, renderScale 1.0, HDR tắt**. `DefaultVolumeProfile.asset` đã dọn override Missing script | |
| Material blockout | URP Lit: **tắt Specular Highlights + Environment Reflections** (khử nhiễu lấp lánh trên mobile, T-404). Material mới của level phải theo | level-designer |
| Frame rate | 60 FPS — đặt ở cả Title và gameplay (F-207) | |
| Build Settings | Title (index 0), Level_01 (index 1); SampleScene không trong build | |

## 5. Scene
- **level-designer**: dựng môi trường, bàn giao **prefab** `Assets/_Game/Level/Level_XX.prefab` (collider, chỗ nấp, điểm rỗng tên chuẩn `CamPoint_P1_S1`, `EnemySpawn_P1_W1_01`, `HostageSpawn_…`, `PickupSpawn_…`, con `Peek`, `RailHint_P<p>_S<s>_NN`; chỉ số `W` trùng `S` của góc giao tranh) + `SurfaceMaterialTag` trên collider. Chi tiết TASK_BOARD mục "Quy ước level". Không cần NavMesh. Scene blockout: `Scenes/Levels/Level_XX_Blockout.unity`.
- **gameplay-coder**: ghép scene chơi được `Scenes/Gameplay/Level_XX.unity` = Level prefab + ray/spline + CinemachineCamera + PhaseDirector + EncounterWave + UI + manager (+ FX, Jev). Chỉ đặt/nối prefab, không sửa nội dung prefab của agent khác. `Level_01.unity` dựng lại được bằng menu `ClaudeCop/Game/Assemble Level_01 (T-403)`.
- `GameplayUI` chứa **EventSystem duy nhất** của scene gameplay; `Title.unity` có EventSystem riêng trong `TitlePanel`. Không thêm EventSystem khác.

## 6. Git
- Chỉ commit khi Liaison ra lệnh — sau khi task **APPROVED** và chủ dự án đồng ý. Demo M1+M2: **commit một lần sau R-W4**.
- Mỗi agent **chỉ `git add` file trong phạm vi sở hữu của mình** (kèm `.meta`), liệt kê đường dẫn cụ thể. **Cấm** `git add .`, `git add -A`, `git commit -a`.
- Commit message: `[<TaskID>] <agent>: <mô tả ngắn>`.
- Commit/push **tuần tự** từng agent. Push lên `origin/main` theo lệnh Liaison; không force push.
- Agent không có Bash (PM, game-designer): Liaison commit hộ.
- Xem mục 1 về file không commit và bí mật.
