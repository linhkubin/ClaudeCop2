# R-W1 — Review gộp Wave 1 (T-101, T-102, T-103, T-104, T-111, T-121)

Reviewer: reviewer · Ngày: 2026-10-04 · Phạm vi: toàn bộ thay đổi chưa commit trong working tree thuộc W1.

## Kết luận chung: **APPROVED có điều kiện** (không có Blocker)
Không có Blocker. Hai Major (F-101, F-102) nên sửa **trước khi đóng băng Core / bắt đầu W2**, vì chúng chạm tới hợp đồng mà Game/UI/Jev sẽ dùng. Các mục còn lại là Minor, xử lý dần.

| Task | Kết luận |
|---|---|
| T-101 setup | **APPROVED** (Minor F-110, F-111) |
| T-102 Core | **APPROVED** (C1–C20 đủ, tên đúng; không có thay đổi bắt buộc trong Core) |
| T-103 Combat | **APPROVED** (Minor F-107, F-108, F-109) |
| T-104 Enemy | **CHANGES REQUESTED** (F-101 đổi tên `Enemy`, F-102 thứ tự Cleared/ShotResolved) |
| T-111 UI/Ads | **APPROVED** (Minor F-112, F-113) |
| T-121 Level | **APPROVED** (Minor F-114, F-115) |

Kiểm tra compile: `read_console` (instance UnityMCP) chỉ còn 2 mục có sẵn được bỏ qua (WebSocket MCP, AI generators NoSubscription). Không có lỗi compile. Quét `.meta`: mọi file trong `Assets/_Game` đều có `.meta` (trừ `.gitkeep`, Unity bỏ qua), không có meta mồ côi.

---

## Vấn đề

### F-101 — Major — Class `Enemy` trùng tên namespace `ClaudeCop.Enemy`
- `Assets/_Game/Scripts/Enemy/Enemy.cs:11` (namespace `ClaudeCop.Enemy`, class `Enemy`); dùng ở `EncounterWave.cs:16,18,21,29,81`.
- Trong `ClaudeCop.Enemy` thì biên dịch được (class cùng namespace thắng). Nhưng code ở assembly khác viết `namespace ClaudeCop.Game { ... }` kèm `using ClaudeCop.Enemy;` ở đầu file (đúng kiểu repo đang dùng): C# tìm `Enemy` ở namespace `ClaudeCop` trước, thấy **namespace** `ClaudeCop.Enemy` → lỗi CS0118 "'Enemy' is a namespace but is used like a type". Game (T-202 trở đi), UI (T-211, vòng target nếu cần), Jev đều dính.
- Hướng sửa: đổi tên class + file thành `EnemyActor` (đổi cả file lẫn class trong Unity để giữ GUID script, prefab `Enemy.prefab` và scene Sandbox vẫn trỏ đúng). Cập nhật `EncounterWave` (`List<EnemyActor>`, `Died` signature), tests không đổi. Làm **trước khi** có code khác tham chiếu. Owner: gameplay-coder.
- Lưu ý liên quan (xem F-116): cùng loại va chạm tên sẽ xảy ra với `ClaudeCop.Camera` vs `UnityEngine.Camera`.

### F-102 — Major — `Cleared` của đợt cuối được phát *trước* `ShotResolved` của phát bắn cuối
- `Enemy.cs:127-141` (`OnTapHit` gọi `Died?.Invoke` đồng bộ) → `EncounterWave.cs:81-92` (`OnEnemyDied` → `Finish` → `RaiseCleared` đồng bộ). TapShooter chỉ phát `ShotResolved` sau khi `OnTapHit` trả về (`TapShooter.cs:194-212`).
- Kịch bản: hạ enemy cuối → `EncounterBase.Cleared` → PhaseDirector bắt đầu chuyển Shot/Phase hoặc phát `LevelCompleted` → Game chuyển sang `Win` (và `CombatPauseSignal.Push`, `timeScale`) → sau đó mới tới `ShotResolved` của chính phát bắn đó. Nếu Game bỏ qua điểm ngoài trạng thái `Playing` thì mất điểm kill cuối (và Justice/combo cuối); ComboSystem (T-301) cũng sẽ nhận thứ tự ngược. Shotgun trúng nhiều mục tiêu: Cleared phát giữa vòng lặp.
- Hướng sửa: `EncounterWave` không phát `Cleared` ngay trong `OnEnemyDied`; đặt cờ `pendingClear` + vị trí, phát ở `LateUpdate` (hoặc đầu `Update` frame sau). Không cần sửa Core. Cũng áp dụng cho nhánh `queue.Count == 0` trong `Begin()` (`EncounterWave.cs:40`): hiện `Cleared` phát đồng bộ ngay trong `Begin()` trước khi người gọi kịp đăng ký → nên phát ở frame sau. Owner: gameplay-coder.

### F-103 — Minor — Enemy bị tắt/bật lại giữa lúc Aiming không còn đăng ký
- `Enemy.cs:153-156` (`OnDisable` hủy đăng ký, đặt `registered=false`) nhưng `EnemyBrain` vẫn ở `Aiming`; `OnAimStarted` chỉ gọi một lần khi vào Aiming (`EnemyBrain.cs:66-68`) → bật lại thì enemy tiếp tục chạy vòng và bắn nhưng không còn trong `TargetRegistry`, người chơi không bắn được.
- Hiện không có ai tắt enemy khi đang Aiming (ngoài `UpdateDeath`), nên chưa lộ lỗi; sẽ lộ khi có pooling/cửa/kính (M2/M3). Hướng sửa: `OnEnable` đăng ký lại nếu `brain.State == Aiming`. Không bắt buộc cho W1. Owner: gameplay-coder.

### F-104 — Minor — Enemy chết bị xoay quanh tâm Body → nằm lơ lửng ~0.5 m
- `Enemy.cs:134-138,147`: `visual` = `Body` (con, local y=1, pivot ở tâm capsule) nên xoay 90° quanh tâm → thân nằm ngang ở độ cao y≈1 thay vì chạm đất. Chỉ là cảm quan nhưng người chơi sẽ thấy ngay ở M1. Hướng sửa: xoay quanh chân (xoay root hoặc đặt pivot ở chân), hoặc hạ `visual` xuống y≈0.5 trong khi ngã. Owner: gameplay-coder. (FX ngã/văng thật làm ở T-401.)

### F-105 — Minor — `EncounterWave` có thể kẹt nếu enemy bị Destroy khi chưa chết; enemy spawn không được dọn
- `EncounterWave.cs:62-63,81-86`: `alive` chỉ giảm qua `Died`. Enemy bị `Destroy` từ bên ngoài (đổi scene một phần, tool) → wave không bao giờ Cleared. Enemy `Instantiate` không có parent (`:57`) nên khi hủy `EncounterWave` enemy vẫn tồn tại. Hướng sửa: đặt enemy spawn làm con của wave (hoặc một root `SpawnedEnemies`), và đếm theo `e == null` trong `Update`. Làm khi sang M2 (pool). Owner: gameplay-coder.

### F-106 — Minor — Code debug nằm trong assembly runtime
- `Scripts/Enemy/Debug/*` (namespace `ClaudeCop.Enemy.Debugging`), `Scripts/Combat/Debug/DummyTapTarget.cs`, `Scripts/UI/UIDebugDriver.cs` biên dịch vào build. Không lỗi, nhưng thêm mã vô ích vào APK và `SandboxWaveStarter` đăng ký lambda không hủy. Hướng sửa: bọc `#if UNITY_EDITOR || DEVELOPMENT_BUILD` hoặc tách asmdef Debug riêng. Chưa cần sửa trước W2. Owner: gameplay-coder / ui-coder.

### F-107 — Minor — Quy tắc chọn mục tiêu: Pickup cạnh tranh ngang hàng với Enemy
- `TargetSelector.cs:38`: nhóm chính gồm mọi mục tiêu không phải Hostage (Enemy, Grenade, **Pickup**) sắp theo khoảng cách. Một thùng vũ khí gần điểm tap hơn enemy (trong bán kính 90 px) sẽ "ăn" phát bắn của Pistol, enemy không bị trúng; Shotgun thì trúng cả thùng lẫn enemy (một phát tốn 1 đạn nhưng nhặt thùng, `plan`: nhặt thùng không tốn đạn — hiện vẫn tốn đạn, `TapShooter.cs:139`).
- Plan chỉ định thứ tự "enemy/lựu đạn → con tin → môi trường"; Pickup chưa được chỉ định. Đề xuất: ưu tiên Enemy/Grenade; Pickup chỉ xét khi không có Enemy/Grenade trong bán kính; khi `PickupCollected` thì hoàn lại viên đạn (PM đã chốt "nhặt thùng không tốn đạn"). Làm ở T-301. Các test hiện có vẫn hợp lệ (test Justice, Hostage, Shotgun đúng với plan).
- Đã kiểm: Enemy luôn thắng Hostage kể cả Hostage gần hơn (đúng plan mục 14 "tap vào enemy đứng trước vật thể thì luôn trúng enemy"); Justice thắng; Shotgun cắt `MaxTargetsPerShot`, sắp xếp đúng.

### F-108 — Minor — Multi-touch: chỉ bắt tap thứ nhất khi ngón thứ nhất còn giữ
- `Assets/InputSystem_Actions.inputactions` map `Gameplay`: binding `<Pointer>/press` và `<Pointer>/position`. Trên màn hình cảm ứng, ngón thứ hai chạm khi ngón đầu chưa nhấc sẽ không tạo `performed` mới (nút đã actuated) và `position` chỉ theo touch chính. Người chơi bắn nhanh bằng hai ngón sẽ mất tap. Hướng sửa (khi có thiết bị thử): thêm binding `<Touchscreen>/touch*/press` + `<Touchscreen>/touch*/position` hoặc đọc `EnhancedTouch`. Chấp nhận được cho demo M1. Action `Hold` hiện không ai dùng (TapShooter đọc `Tap.IsPressed()`); nên xóa nếu không dùng. Owner: gameplay-coder.

### F-109 — Minor — Phụ thuộc cho T-211: `PointerBlocker` và EventSystem
- `TapShooter.cs:18` (`public static Func<Vector2,bool> PointerBlocker`) đúng thiết kế (Combat không tham chiếu UI) nhưng `ClaudeCop.UI.asmdef` hiện **chưa** tham chiếu `ClaudeCop.Combat` (được phép theo bảng); T-211 cần thêm. Nếu không nối, bấm nút RELOAD (Pointer press) cũng bắn một phát và tốn đạn. Lưu ý khi dùng `EventSystem.IsPointerOverGameObject` với Input System/touch phải truyền đúng pointerId. Đây là việc của T-211, ghi lại để không sót. Owner: ui-coder.
- Ghi chú an toàn đã kiểm: `ReloadStateChanged` được phát trong `OnDisable` (`TapShooter.cs:71`) — UI phải hủy đăng ký/kiểm tra null khi tắt scene.

### F-110 — Minor — T-101: `manifest.json` không có thay đổi trong working tree
- `Packages/manifest.json` đã chứa `com.unity.splines 2.8.2` từ commit trước (e39cd9c), nên không có diff — ổn, chỉ ghi nhận để PM không chờ commit manifest từ T-101.
- Đã kiểm: Android landscape-only (`allowedAutorotateToPortrait*: 0`, hai landscape = 1, `defaultScreenOrientation: 4` AutoRotation), quality `Mobile` mặc định (`m_CurrentQuality: 0`), `Mobile_RPAsset` thay đổi chỉ là Unity tự nâng cấp phiên bản asset (k_AssetVersion 13), không do tay. **Không thêm tag/layer** (`TagManager.asset` không đổi). Không đổi `Active Build Target`/Build Settings (vẫn chỉ `SampleScene`); thêm scene vào build là việc T-203/T-304.
- Có thêm `scriptingDefineSymbols` cho Android (`SENTIS_ANALYTICS_ENABLED;APP_UI_EDITOR_ONLY`) — do Unity tự sinh khi chuyển nền tảng, vô hại.

### F-111 — Minor — File lạ ngoài phạm vi
- `Assets/Screenshots/screenshot-20261004-130755.png` (+ meta) nằm ngoài `Assets/_Game`, không thuộc agent nào. Nên xóa hoặc đưa vào `.gitignore`, **không commit**. Cũng không commit `ProjectSettings/Packages/com.unity.probuilder/` và `SceneTemplateSettings.json` nếu không chủ ý (do Unity/ProBuilder sinh); để PM/Liaison quyết.
- Scene Sandbox phụ: `Scenes/Sandbox/gameplay-coder-combat.unity`, `gameplay-coder-enemy.unity` — Conventions chỉ nêu `<agent-name>.unity`. Chấp nhận được, ghi nhận.

### F-112 — Minor — FakeRewardedAd có nút Close đóng sớm
- `FakeAdOverlay.prefab` có `CloseButton` đang active; `FakeRewardedAd.cs:55` `Close()` kết thúc với `onFailed`. Plan: quảng cáo "chỉ đóng được khi đếm xong". Nút đóng sớm cho người chơi thoát mà không phải đếm; RevivePopup (T-311) sẽ coi là thất bại (không hồi tim) — chấp nhận nếu chủ ý, nhưng nên ẩn nút cho tới khi đếm xong hoặc loại bỏ. Ngoài ra `durationSeconds` (3 s) lặp ở `UIConfig.fakeAdSeconds` nhưng `FakeRewardedAd` không đọc `UIConfig` (và `ClaudeCop.Ads` không nên ref UI): chọn một nơi duy nhất. Owner: ui-coder.

### F-113 — Minor — UI/Ads: sai lệch asmdef nhẹ so với bảng (chấp nhận)
- `ClaudeCop.Ads` là asmdef riêng, chỉ tham chiếu `Unity.ugui`, **không** tham chiếu Core (đúng ý đồ "IRewardedAd không ở Core"); `ClaudeCop.UI` tham chiếu Core, Ads, ugui; `ClaudeCop.UI.Editor` Editor-only. Không ai tham chiếu UI, không vòng, không vượt quyền. Bảng Conventions mục 2 ghi gộp Ads vào ClaudeCop.UI — cần PM cập nhật bảng cho khớp (thêm hàng `ClaudeCop.Ads`). Dùng `UnityEngine.UI.Text` thay TMP: chấp nhận cho prototype; chữ hiện không dấu (ví dụ "QUANG CAO"). `UIAssetBuilder` chỉ ghi vào `Assets/_Game/UI`, `Prefabs/UI` và sandbox `ui-coder.unity` — đúng phạm vi. Owner: PM (cập nhật bảng).

### F-114 — Minor — T-121: chỗ nấp thấp không có biên an toàn
- Quy ước: chỗ nấp thấp spawn ở y=−1 (capsule 2 m → đỉnh y=+1), cover cao 1.1 m. Camera ở y=1.6: ví dụ `EnemySpawn_P1_W2_01` (−2,−1,23.6) sau `Cover_Car_W2` (z=22, đỉnh 1.1): đường nhìn từ CamPoint_P1_S2 (0,1.6,14) tới đỉnh đầu enemy qua đỉnh cover ở độ cao ≈1.1–1.13 m → đỉnh đầu có thể lộ vài cm khi "đang nấp". Gợi ý: y=−1.2 cho hide hoặc nâng cover 1.2. (Peek cục bộ (0,1,0) → feet đúng y=0 khi lên. Đã đúng quy ước "đặt ở chân".)
- Hỏi đáp va chạm Ground: **không có va chạm**. Enemy không có Rigidbody; collider của `Body` bị tắt trong `Awake` (`Enemy.cs:58`) và chỉ bật khi `Aiming` (feet y=0, chạm mặt trên Ground, top y=0). Ground dày 0.1 m (y=−0.05); enemy ở y=−1 nằm hẳn dưới đáy ground khi Hidden, lên thẳng đứng qua ground trong 0.3 s — nhìn từ camera thấy trồi từ sau cover/mặt đất, không xuyên cạnh nào. Với spawn y=0 (không có cover thấp) enemy đứng lộ thiên khi Hidden (vì không có chỗ ẩn) — đúng thiết kế các điểm W2_02/W2_03/W5_01/03 sau vật cao.

### F-115 — Minor — T-121: các mục nhỏ
- `Level_01.prefab`: không có MonoBehaviour, không có Light (đã kiểm: chỉ Transform/MeshFilter/MeshRenderer/BoxCollider); 21 BoxCollider cho 21 khối hình học; tên điểm theo quy ước (`CamPoint_P1_S1..S5`, `EnemySpawn_P1_W2/W3/W5_NN`, `HostageSpawn_P1_W2_01`, `W5_01`, `PickupSpawn_P1_W2_01`, `RailHint_P1_S1_01..03`, `RailHint_P1_S4_01..05`); mỗi Enemy/Hostage spawn có con `Peek` với rotation cục bộ identity (forward spawn hướng về camera); khoảng cách enemy–camera 9–14 m (trong 6–15 m). `PickupSpawn` không có `Peek` (hợp lý, quy ước chỉ yêu cầu Enemy/Hostage).
- Static flags = 0 trên toàn bộ khối (`m_StaticEditorFlags: 0`) → không batching tĩnh/occlusion; nên đặt Batching Static cho `Geometry` ở T-221 hoặc khi tối ưu. Không ảnh hưởng chức năng.
- `Level_01_Blockout.unity` chỉ có prefab instance + Directional Light, không camera (hợp lý cho scene blockout). Ghi nhận: `CamPoint_P1_S2` và `S4` cùng vị trí (0,1.6,14) — S4 là Shot di chuyển, chấp nhận.

### F-116 — Minor (cho PM) — Quy ước "namespace = tên assembly" gây va chạm tên với Unity
- Cùng cơ chế F-101: `ClaudeCop.Camera` (T-201) sẽ khiến `Camera` (kể cả trong chính assembly Camera) bị hiểu là namespace khi viết `namespace ClaudeCop.Camera { ... Camera cam }` hoặc trong Game/UI (tham chiếu assembly Camera). Cần `UnityEngine.Camera` đầy đủ hoặc alias `using Cam = UnityEngine.Camera;`, hoặc đổi namespace sang `ClaudeCop.CameraRig`. Nên chốt trước T-201. Combat/Enemy hiện chưa tham chiếu assembly Camera nên không lỗi.

---

## Đã kiểm — không có vấn đề

**T-102 Core (đối chiếu C1–C20)**: C1 `SurfaceMaterial`; C2 `SurfaceMaterialTag` (+`Resolve`, thiếu tag → Concrete); C3 `WeaponKind`; C4 `ShotInfo` (+ `ImpulseScale`); C5 `IShootable`; C6 `TargetKind`; C7 `ITapTarget` (Id, Kind, IsTargetable, AimPoint, HasJusticePoint, JusticePoint, ShowsReticle, ReticleProgress, ExposedTime, OnTapHit) — không có Transform là đủ vì UI dùng `AimPoint`; C8 `TargetRegistry` (Register/Unregister idempotent, `IReadOnlyList`, hai event); C9 `TapOutcome`; C10 `ShotResult`; C11 `CombatPauseSignal` (Push/Pop có đếm theo lý do, Pop không khớp → cảnh báo & bỏ qua, `Changed` chỉ phát khi 0↔1); C12 `DamageSource`; C13 `IPlayerDamageReceiver`+`PlayerDamageService` (không receiver → cảnh báo); C14 `GameState`; C15 `GameEvents` + `Current` (snapshot cập nhật **trước** khi phát event); C16 `CombatEvents` + `Current`; C17 `EncounterBase` (Id/Description virtual, `RaiseCleared` protected — chấp nhận, đúng hướng); C18 `RailEvents`; C19 `UserSettings` (key cố định `cc_reduce_motion`, lazy load, `Changed`); C20 `GameCommands`. Mọi static có `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` reset (kể cả `current` snapshot, handler, danh sách, đếm pause) — đúng khi tắt Domain Reload (hiện dự án bật reload: `m_EnterPlayModeOptions: 0`). Không định nghĩa trùng, không tham chiếu assembly game (`references: []`), asmdef Core Tests Editor-only + `UNITY_INCLUDE_TESTS`. Sai khác đã khai (EncounterBase Id/Description virtual; ITapTarget không Transform) → **chấp nhận**, nên đóng băng như hiện tại. Lưu ý cho consumer (không cần sửa Core): `ShotResult.TargetKind` mặc định `Enemy` khi Miss/Environment — luôn dùng `Outcome` để phân biệt; mọi event phát từ main thread (Jev bất đồng bộ ở W3/W4 phải marshal về main thread trước khi `Raise*`).

**T-103**: một `ShotResolved` mỗi mục tiêu cho Shotgun (đúng), `TargetsHit = count` ở mọi bản ghi; `HostageHit` gây `PlayerDamageService.Damage` đúng một lần/phát; hold-to-fire MG dùng `Tap.IsPressed()` trong `Update`, `OnTapPerformed` bỏ qua vũ khí hold (không bắn đôi); lấy `ExposedTime/ReticleProgress/Kind` trước `OnTapHit` (đúng, vì enemy có thể bị hủy đăng ký); `Time.timeScale<=0` và `CombatPauseSignal.IsPaused` chặn bắn; đăng ký/hủy `performed` và `GameCommands.ReloadRequested` đối xứng ở `OnEnable/OnDisable`; không cấp phát mỗi frame (list `hits` tái dùng, `projectFunc` cache, `Vector2?` không alloc); số cân bằng ở `WeaponData`/`CombatConfig` (Pistol 6/90 px, Shotgun 6/180 px/5 mục tiêu, MG 30/10 phát/s/hold, Justice 35 px, reload 0.5 s); asmdef Combat → Core + Unity.InputSystem, đúng bảng.

**T-104**: `EnemyBrain` thuần, hữu hạn trạng thái, không kẹt (Hidden→Peeking→Aiming→Retreating→Hidden; Kill chỉ trong Aiming; chia cho 0 đã chặn bằng clamp tối thiểu 0.001); Register khi Aiming, Unregister khi hết vòng/chết/`OnDisable` (kể cả Destroy, vì `OnDisable` chạy khi Destroy; đổi scene cũng chạy) — không rò rỉ; pause dừng cả `Enemy.Update` lẫn bộ đếm stagger của `EncounterWave`, và `IsTargetable` trả false khi pause; `EncounterWave` hỗ trợ cả hai nguồn (`sceneEnemies` và `spawnPoints`+`enemyPrefab`, lấy `Peek` bằng `Find`, hide-pos = vị trí spawn, peek-pos/rot từ con `Peek`) và hủy đăng ký `Died` ở `OnDestroy`; prefab Enemy pivot ở chân (Body y=1, capsule cao 2, collider tắt); số liệu ở `EnemyConfig`; asmdef Enemy → Core, đúng. Không có LINQ/closure/`GetComponent` trong `Update`; `Update` ghi transform mỗi frame cho enemy cả khi Hidden (rẻ, chấp nhận).

**Phạm vi sở hữu**: không thấy sửa ngoài phạm vi (xem F-111 về file lạ); `.claude/agents`, `Docs/` không thuộc W1. Không thêm tag/layer. Không asmdef tham chiếu vòng hoặc vượt bảng.

**Log "referenced script (Unknown) on this Behaviour is missing" (3 lần)**: tìm ở mọi scene/prefab dưới `Assets/_Game` — **không có** `m_Script: {fileID: 0}`; mọi GUID script đều khớp một `.cs.meta` trong `Assets` hoặc script có trong package (ugui, Input System, URP). Nguồn khả dĩ nhất là **tài sản có sẵn không thuộc W1**: `Assets/Settings/DefaultVolumeProfile.asset` (commit đầu tiên, 5 mục `m_Script: {fileID: 0}` với `hideFlags: 3` — Volume override của URP trỏ script đã bị gỡ), được nạp khi vào Play. Chưa thể xác nhận bằng console do reviewer không vào Play; chủ dự án có thể chọn asset đó trong Project để xem Inspector báo "Missing". Không phải lỗi của wave này.

---

## Những gì reviewer KHÔNG kiểm chứng được — chủ dự án cần tự bấm thử
1. Mở `Scenes/Sandbox/gameplay-coder-combat.unity`, Play: tap/click lên `DummyTapTarget`, kiểm tra Pistol (6 viên, reload bằng `GameCommands.RequestReload`), đổi `startingWeapon` sang Shotgun (một phát trúng nhiều mục tiêu, log `[Dummy]`) và MachineGun (giữ chuột/ngón để bắn liên tục ~10 phát/s). Xác nhận dùng được cả chuột và cảm ứng (Device Simulator).
2. Mở `gameplay-coder-enemy.unity`, Play: enemy trồi lên 0.3 s rồi vòng chạy 2.5 s; hết vòng thì log `[FakeReceiver] Bi ban`; enemy thụt vào 0.3 s, nấp 0.8 s rồi ló lại; `SandboxAutoShooter.killAfter` để thử Kill; `Cleared` log khi hạ hết. **Reload hoàn tất sau 0.5 s và enemy chạy theo thời gian thật** chưa được kiểm chứng (Editor không focus khi build) — cần xem trong Editor có focus (đặc biệt `Time.time` của reload và `Time.deltaTime` của enemy).
3. `ui-coder.unity`: HUD/vòng target/nháy đỏ/Win/GameOver/FakeAd chạy bằng `UIDebugDriver` (ContextMenu "Show Win/Game Over/Fake Ad/Flash"), kiểm tra Canvas 1920×1080 và safe area ở Device Simulator.
4. `Level_01_Blockout.unity`: nhìn góc camera từ từng `CamPoint_P1_S*` (camera 1.6 m, FOV 60) — đã kiểm tĩnh nhưng chưa xem khung hình thật; kiểm tra enemy ở y=−1 thật sự không lộ đầu sau cover (F-114).
5. Trên thiết bị Android: multi-touch (F-108), 60 FPS (chưa đặt `Application.targetFrameRate`, việc của T-202).

## Việc đề nghị theo thứ tự
1. (Trước W2) Sửa F-101 (đổi `Enemy` → `EnemyActor`) và F-102 (hoãn `Cleared` sang frame sau); PM chốt F-116 trước T-201. Sau đó coi Core đã đóng băng (không cần đổi Core).
2. Ghi F-109, F-112 cho ui-coder (T-211/T-311); F-107 cho T-301; F-103/F-105 cho T-302.
3. Dọn F-111 trước khi commit.
