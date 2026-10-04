# R-W2-W3 — Review gộp Wave 2 + Wave 3

Reviewer: reviewer · Ngày: 2026-10-04 · Phạm vi: T-201, T-202, T-203, T-211, T-301, T-302, T-303, T-304, T-305, T-311, T-312 (+ kiểm lại F-101…F-112 của R-W1).
**Bỏ qua** (review ở R-W4): `Scripts/FX/`, `Scripts/Jev/JevDirector*`, `Area_P3_Rooftop`, bảng debug Jev (T-411), T-321.
Phương pháp: đọc tĩnh toàn bộ code + YAML scene/prefab (không vào Play, không sửa gì). Mọi thứ trong `Assets/_Game` đều chưa được git theo dõi nên không có `git diff`; "Core có bị sửa không" được kiểm bằng mtime (xem mục Core).

## Kết luận chung: **CHANGES REQUESTED** (0 Blocker, 1 Major, còn lại Minor)
Chất lượng tốt: state machine/pause/revive đúng, không rò rỉ event, F-101…F-112 của R-W1 đã xử lý. Chỉ 1 Major (combo không nhân điểm vì cấu hình) — sửa 1 dòng + 1 test, không cần review lại cả wave.

| Task | Kết luận |
|---|---|
| T-201 / T-303 Camera | **APPROVED** (Minor F-205, F-206, F-210) |
| T-202 / T-304 Game | **CHANGES REQUESTED** (F-201 Major; Minor F-207, F-208) |
| T-203 ghép Level_01 | **APPROVED** (Info F-209: chưa nối hostage/pickup/preset — dự kiến T-403) |
| T-211 / T-311 / T-312 UI + Ads | **APPROVED** (Minor F-211, F-212) |
| T-301 / T-302 Combat/Enemy | **APPROVED** (Minor F-202, F-203) |
| T-305 Jev nền | **APPROVED** (Minor F-204) |
| Thư mục lạ / commit | Xem mục "Trước khi commit" |

Console (`read_console`, instance UnityMCP): chỉ còn 1 mục `NoSubscription` (generators.ai.unity.com) — bỏ qua. Instance `unityMCP` không kết nối. Không có lỗi compile / Missing Script. Quét `.meta`: mọi file trong `Assets/_Game`, `Assets/TextMesh Pro` đều có `.meta`, không có meta mồ côi. Quét GUID script trong `Level_01.unity`, `Title.unity`: mọi `m_Script` đều trỏ về `.cs.meta` trong Assets hoặc package Cinemachine/Splines — không Missing.

---

## Vấn đề

### F-201 — Major — Combo không nhân điểm: `applyComboMultiplier = 0` trong asset
- `Assets/_Game/Prefabs/Game/Data/GameConfig.asset` (`applyComboMultiplier: 0`); mặc định trong `Scripts/Game/GameConfig.cs:34-35` (`[SerializeField] bool applyComboMultiplier;` = false, tooltip "M1 = false. M2 bật"); test khoá hành vi này: `Game/Tests/GameTests.cs:41-46` (`Combo_IgnoredInM1_Multiplier1`).
- Hệ quả: ComboTracker/ComboSystem chạy đúng (x1→x5, reset khi Miss/Hostage/bị bắn), HUD hiện "xN COMBO", nhưng `ScoreCalculator.Compute` luôn dùng multiplier = 1 → **điểm không tăng theo combo**, chữ bay không bao giờ hiện "xN" (điều kiện `multiplier > 1.001`). Plan mục 9 và tiêu chí M2 ("combo tăng và reset đúng", hệ số điểm) không đạt về mặt điểm.
- Hướng sửa: bật `applyComboMultiplier` (asset + default trong SO), đổi test thành khẳng định Kill ở streak 3 → ×3 và Shotgun nhiều mục tiêu vẫn không nhân thêm `TargetsHit`. Owner: gameplay-coder.

### F-202 — Minor — Súng máy bắn ngay sau khi nhặt thùng nếu ngón/chuột còn giữ
- `Combat/TapShooter.cs:80-118` + `:195-200`. Tap trúng thùng → `Equip(MachineGun)` (HoldToFire). Nếu ngón còn chạm (`isInProgress`) / chuột còn giữ ở frame kế, nhánh hold-to-fire bắn ngay lên chính vị trí thùng; 1 tap dài ~100 ms có thể tốn 1–2 viên súng máy và combo bị Miss/Environment reset.
- Hướng sửa: khi `Equip` sang vũ khí hold, chỉ bắt đầu bắn sau khi có lần nhấn MỚI (ghi nhớ touchId/trạng thái press hiện tại để bỏ qua), hoặc `nextFireTime = Time.time + 0.15f`. Owner: gameplay-coder.

### F-203 — Minor — Hard-code số cân bằng/cảm giác rải rác
- `Combat/TapShooter.cs:183` (0.2 s spam khi hết đạn), `Enemy/EnemyActor.cs:223` (ngã 0.25 s), `Camera/RailCameraDriver.cs:531` (làm mượt roll 0.3 s), `Camera/CameraFeelApplier.cs:120` (tần số shake 40), `UI/HudView.cs` (0.25/0.6 s xung nút), `Jev/OfflineJevClient.cs:183-186,192,197` (trọng số 0.45/0.3/0.25, mốc 0.8–2.0 s, tâm 0.85/0.5/0.15). Không ảnh hưởng chức năng, nhưng Conventions mục 3 yêu cầu số cân bằng nằm trong SO — đặc biệt bộ luật Offline Jev (sẽ phải chỉnh khi cân bằng). Đưa vào `JevConfig`/`CameraFeelProfile`/`EnemyConfig` khi có dịp. Owner: gameplay-coder.

### F-204 — Minor — `PlayerStatsTracker` tính bắn Environment là bắn trượt; Shotgun làm lệch độ chính xác
- `Jev/PlayerStatsTracker.cs:424-426` (`Environment` → `shots++, misses++`). Plan mục 14: bắn vật thể **không** tính là bắn trượt khi đo độ chính xác gửi Jev (đếm riêng). Hiện `TapOutcome.Environment` phát cho mọi tap trúng collider (tường, đất), không phân biệt `IShootable`, và Core đã đóng băng (không có cờ trong `ShotResult`). Với Props ở M3 cần có cách phân biệt (vd. Combat phát `Environment` chỉ khi trúng IShootable, còn tường = `Miss`; hoặc Core thêm cờ khi mở khoá).
- Shotgun phát 1 `ShotResolved` mỗi mục tiêu → 1 phát trúng 3 enemy = `shots=3, hits=3`, còn 1 phát trượt = `shots=1` → accuracy bị thổi phồng (`PlayerStatsTracker.cs:416-420`; `ShotResult.TargetsHit` có sẵn để chia). Hướng sửa: đếm `shots` theo phát bắn (dùng `CombatEvents.ShotFired`) hoặc chia `1/TargetsHit`. Chưa ảnh hưởng demo (Offline chỉ chọn reticle_time) nhưng nên sửa trước T-402. Owner: gameplay-coder.

### F-205 — Minor — Dọn đăng ký/trạng thái khi `PhaseDirector` bị tắt giữa chừng
- `Camera/PhaseDirector.cs:99-107, 245-248`. `OnDisable` nhả pause và `SlowZoom` nhưng coroutine bị Unity dừng ngầm khi disable: `routine` giữ khác null (`IsRunning` sai), `started=true` không cho `StartLevel` lại, và lambda `onCleared` còn đăng ký vào `enc.Cleared`. Hiện không ai tắt PhaseDirector (scene reload tạo mới) nên chưa lộ lỗi; ghi nhận. Hướng sửa: trong `OnDisable` đặt `routine=null`, hủy `onCleared` (giữ biến cấp lớp). Owner: gameplay-coder.

### F-206 — Minor — Phase transition dùng thời gian thực, không dừng khi `timeScale = 0`
- `PhaseDirector.cs:171,180` (`WaitForSecondsRealtime`) khớp với UI fade (unscaled) nên đồng bộ tốt, nhưng nếu Revive/popup đóng băng game giữa lúc fade thì fade vẫn chạy. Hiện không thể xảy ra (chuyển Phase chỉ khi giao tranh đã xong và CombatPause đang giữ, enemy không bắn). Ghi nhận cho khi có Pause menu. `CombatPauseSignal.Push/Pop` của Camera cân bằng ở mọi nhánh (cờ `pauseHeld`/`transitionPauseHeld`, thả trong `OnDisable`). Không cần sửa. Owner: —.

### F-207 — Minor — Title chạy ở framerate mặc định (30 FPS trên Android)
- `Game/GameManager.cs:58` đặt `Application.targetFrameRate = 60` ở `Start()` của scene gameplay; `Title.unity` không có GameManager → Title/menu chạy 30 FPS trên Android (mặc định). Hướng sửa: đặt trong `TitleLauncher.Awake` hoặc một `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`. Owner: gameplay-coder.

### F-208 — Minor — Luồng GameManager (timeScale/pause/revive/restart) không có test tự động
- `Game/Tests/GameTests.cs` kiểm `GameFlow`/`LifeTracker`/`ScoreCalculator` (22 test, tốt) nhưng phần glue dễ lỗi nhất — `GameManager` (đặt `timeScale=0` trước khi phát RevivePrompt, nhả `Revive`/`ReviveGrace`/`GameEnd` pause, restart qua `GameSession.PendingStart`) và `PhaseDirector` không có test (EditMode/PlayMode). Review tĩnh thấy đúng (xem "Đã kiểm"), nhưng nên có 1 PlayMode test hoặc checklist tay (mục "Chủ dự án cần bấm thử"). Owner: gameplay-coder.

### F-209 — Info — `Level_01.unity` chưa nối hostage/pickup/preset
- `Scenes/Gameplay/Level_01.unity` (3 `EncounterWave` ở dòng ~133/970/1584): chỉ có `config`, `spawnPoints`, `enemyPrefab`; không có `hostageSpawnPoints`/`hostagePrefab`/`pickupSpawnPoints`/`pickupPrefab`, không có preset, không `subAngles` (scene serialize trước T-301/T-302 nên field mới nhận mặc định rỗng). Level prefab đã có `HostageSpawn_P1_W2_01`, `PickupSpawn_P1_W2_01`, `HostageSpawn_P1_W5_01`. Như vậy bản chơi hiện tại **chưa thể** thử con tin / thùng vũ khí / Justice theo preset trong Level_01 (Justice vẫn bật vì `EnemyConfig.justiceEnabled`). Đây là việc của T-403 (ghép M2), không phải lỗi; ghi để PM không tưởng đã có. Cũng chưa có `JevSystems`/`FxSystems` (T-401/T-402 đang chạy).
- Scene sạch: 1 `EventSystem` (trong `GameplayUI`, module `InputSystemUIInputModule`), 1 Main Camera tag `MainCamera` + `CinemachineBrain`, không trùng light, không Debug component, Build Settings đúng (Title=0, Level_01=1, SampleScene đã gỡ khỏi build). `Assets/Scenes/SampleScene.unity` vẫn còn trong project (không trong build) — nên xoá khi dọn.

### F-210 — Minor — `CameraShot` tạo `CinemachineCamera` lúc runtime; `CameraFeelProfile.asset` chưa re-save
- `Camera/CameraShot.cs:56-67`: các Shot Combat không có `CinemachineCamera` serialize trong scene (chỉ RailCamera có) — thêm bằng `AddComponent` trong `PhaseDirector.Init`. Chạy được nhưng khó kiểm tra bằng mắt/`find_gameobjects`. `Settings/CameraFeelProfile.asset` chưa có các field M2 (punch, killZoom, phaseFade, maxMoveSeconds…) — nhận giá trị mặc định từ code, nhưng nên mở asset + lưu để lộ giá trị thật cho người chỉnh. Owner: gameplay-coder.

### F-211 — Minor — Cấp phát nhỏ theo sự kiện trong UI
- `UI/HudView.cs` `SetAmmo` (`cur + " / " + max`) cấp phát chuỗi mỗi viên đạn (MG ≈10 lần/s); `SetScore` (`ToString("N0")`), `SetWeapon` (`weapon.ToString()`), `FloatingScoreView.Spawn` (nối chuỗi mỗi lần kill). Không phải mỗi frame nhưng với MG sẽ sinh rác liên tục. Dùng `TMP_Text.SetText("{0} / {1}", cur, max)` (đã dùng ở `RevivePopupView`). Owner: ui-coder.
- `UI/TargetReticlePresenter.cs`, `Combat/TapShooter.cs` dùng `Camera` trần: hiện đúng (UI/Combat chưa tham chiếu assembly `ClaudeCop.Camera`); nếu sau này UI thêm ref Camera sẽ dính F-116. Dùng alias `UCamera` như `FloatingScoreView`. Owner: ui-coder / gameplay-coder.

### F-212 — Minor — Bảng asmdef trong Conventions lạc hậu
- `ClaudeCop.Ads` hiện ref `Unity.ugui` + `Unity.TextMeshPro` (bảng ghi chỉ ugui); `ClaudeCop.Camera` ref thêm `Unity.Mathematics`; `ClaudeCop.UI` ref `Unity.TextMeshPro`; Mục 4 vẫn ghi TextMeshPro "Chưa dùng" và Hold "xoá ở T-304" (đã xoá). Số "bất tử 0.5 s" trong TASK_BOARD "PM đề xuất" nay là 1.5 s (`GameConfig`). Owner: project-manager cập nhật tài liệu.

---

## F-1xx của R-W1 — trạng thái kiểm lại
| ID | Kết quả |
|---|---|
| F-101 `Enemy`→`EnemyActor` | **Đã xử lý** (`EnemyActor.cs`, `EncounterWave`, `Enemy.prefab` dùng class mới; không còn class `Enemy`) |
| F-102 `Cleared` hoãn sang `LateUpdate` | **Đã xử lý** (`EncounterWave.FlushClear` ở `LateUpdate`, kể cả đợt rỗng; `Begin()` không phát đồng bộ) |
| F-103 re-register khi bật lại lúc Aiming | **Đã xử lý** (`EnemyActor.OnEnable`, `HostageActor.OnEnable`) |
| F-104 enemy ngã quanh chân | **Đã xử lý** (xoay root theo trục `Cross(up, dir)`) |
| F-105 spawn làm con của wave; không kẹt khi bị Destroy | **Đã xử lý** (`Instantiate(..., transform)`, `AliveCount` bỏ enemy null; `HasEnemyToActivate` bỏ slot null) |
| F-106 code debug bọc `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD` | **Đã xử lý** cho `Enemy/Debug`, `Combat/Debug`, `Camera/Debug`, `Game/Debug`, `UI/UIDebugDriver`. Không có Debug component trong `Level_01.unity`/`Title.unity` |
| F-107 Enemy > Pickup > Hostage, nhặt thùng hoàn đạn | **Đã xử lý** (`TargetSelector.Select` 3 tầng; `TapShooter.cs:195-200`; test `EnemyBeatsCloserPickup`, `PickupSelectedWhenNoEnemy_AndBeatsHostage`) |
| F-108 multi-touch, xoá `Hold` | **Đã xử lý** (đọc `Touchscreen.touches`, bỏ qua `Touchscreen` trong `OnTapPerformed`, action `Hold` đã xoá). Cần thử trên máy thật |
| F-109 UI ref Combat + `PointerBlocker` | **Đã xử lý** (`UiPointerBlocker` dùng `EventSystem.RaycastAll`, không dùng pointerId; gán/gỡ ở `HudPresenter`; chỉ `ReloadButton` có `raycastTarget`) |
| F-112 FakeAd: bỏ Close, 1 nguồn thời lượng | **Đã xử lý** (prefab không còn `CloseButton`; `fakeAdSeconds` đã xoá khỏi UIConfig, `durationSeconds` nằm ở `FakeRewardedAd`) |
| F-114, F-115 (level) | Ngoài phạm vi review này (T-221/T-321) |

---

## Đã kiểm — không có vấn đề

**Core đóng băng**: tất cả `.cs` trong `Scripts/Core/` có mtime 12:58:36 (cùng lúc tạo, trước R-W1 lúc 13:16) → không bị sửa từ đó. API dùng thêm `CombatPauseSignal.HasReason` đã có sẵn ở thời điểm đóng băng. Không có định nghĩa trùng hợp đồng Core ngoài Core.

**asmdef**: không vòng; Core `references: []`; Camera → Core/Cinemachine/Splines/Mathematics; Combat → Core + InputSystem; Enemy → Core; Jev → Core + Enemy; Game → Core, Combat, Enemy (không cần Camera/Jev); Ads → ugui + TMP (không Core); UI → Core, Combat, Ads, ugui, TMP. **Không ai tham chiếu UI/Ads ngoài UI** (đã grep `ClaudeCop.UI|Ads` ngoài thư mục UI/Ads: không có). Tất cả asmdef Tests là Editor-only + `UNITY_INCLUDE_TESTS`.

**Phạm vi sở hữu**: không thấy sửa tag/layer (`TagManager` không đổi); input/Build Settings/Player Settings do gameplay-coder (đúng). Thay đổi `TimeManager.asset` (Fixed Timestep dạng phân số 2822399/141120000 = 0.02), `QualitySettings`, `Mobile_RPAsset`, `DefaultVolumeProfile.asset` (Unity tự dọn 5 override Missing script — đây chính là nguồn log "Unknown script missing" của R-W1), `ShaderGraphSettings`, `URPProjectSettings` là Unity tự nâng cấp/serialize lại, vô hại. `Level_01.prefab` có sửa lúc 13:29 (level-designer, sau R-W1) — hợp lệ.

**Camera (T-201/T-303)**
- Chuỗi Shot đúng: Move (ease trapezoid, nhìn trước, giới hạn yaw 60°/s cưỡng bức bằng `Mathf.Clamp(DeltaAngle…)`, pitch ≤10°, roll ≤2° tắt khi Giảm chuyển động), Combat (Begin, chờ Cleared, kill zoom, nghỉ `restAfterClear`). Auto tăng tốc `RailCameraDriver.Prepare` (khi `speedOverride<=0` và thời gian dự kiến > `maxMoveSeconds` 5.8 s: tăng tới `maxRailSpeed` 4) – công thức đúng, giữ ≤6 s với ray S4 17.6 m.
- `CombatPauseSignal` cân bằng: `WaitTransition` Push/Pop trong cặp, `PhaseTransition` Push ở `BeginPhaseTransition`, Pop ở `FinishPhaseTransition`; có nhả trong `OnDisable`; không nhánh `yield break` nào bỏ sót (Phase không có Shot chạy được vẫn gọi `FinishPhaseTransition`).
- `Update` không Find/GetComponent/alloc (Find chỉ ở `Init`). Đăng ký/hủy `PlayerDamaged`, `GameStateChanged`, `ShotResolved` đối xứng. `startOnGamePlaying`: `Start` kiểm snapshot + `OnGameStateChanged` kiểm `!started` → chạy đúng 1 lần, không chạy lại khi Revive (Playing lần 2). Thứ tự Start giữa GameManager/PhaseDirector khi Restart đã được xử lý bằng cả hai nhánh.
- `ReduceMotion`: tắt cầm tay/nhún/dolly/roll/punch/kill zoom, shake nhân `hitShakeReduceScale`, fade ngắn, ngưỡng Cut 30°; `CameraFeelApplier` đọc `UserSettings.ReduceMotion` mỗi frame (cập nhật tức thì).
- Statics reset bằng `[RuntimeInitializeOnLoadMethod]`; `Camera` dùng đúng `UnityEngine.Camera` (F-116).

**Game (T-202/T-304)**
- `GameManager`: `timeScale=0` đặt **trước** `RaiseGameStateChanged(RevivePrompt)` nên popup không tự đóng băng/khôi phục (khớp `RevivePopupPresenter.froze=false`). Mọi đường thoát nhả pause + `timeScale=1`: Accept (`PopRevivePause`, `timeScale=1`), Decline (`PopRevivePause`, `timeScale=1`, `EndRun`), `OnDisable` (`ReleaseAllPauses`, `timeScale=1`), Restart/ReturnToTitle. Revive 1 lần/lượt (`AcceptRevive` giảm `RevivesRemaining`; `Begin` đặt lại). Hồi đủ 3 tim (`ResetLives()`), bất tử `ReviveGraceSeconds` 1.0 s + CombatPause "ReviveGrace" dùng `WaitForSecondsRealtime` (đúng khi `timeScale` vừa trở lại 1). Bất tử sau trúng đạn 1.5 s (SO). Decline lặp/Win sau GameOver được `GameFlow` chặn.
- Restart: Title snapshot → `LoadScene(buildIndex)` → `Start()` đọc `PendingStart` → `BeginRun()` → Playing; UI `EndScreenPresenter` ẩn panel vì snapshot Title; combo/ammo được reset bởi `OnEnable` của ComboSystem/TapShooter. `TitleLauncher` → `PendingStart` → Level_01; chạy Level_01 trực tiếp trong Editor (không qua Title) vẫn tự bắt đầu (`editorDirect`).
- `ScoreSystem` nghe `ShotResolved` chỉ khi `Playing`; Shotgun không nhân `TargetsHit` (đã sửa n²). Điểm: Kill 100 + thưởng sớm ≤100 tuyến tính, Justice 300 (×1.5 nếu vòng xanh < 0.4) – đúng số PM đề xuất. (Combo xem F-201.)
- `PlayerHealth` dùng `Time.unscaledTime` nên đúng khi `timeScale=0`; `Damage` bỏ qua khi không `Playing`.

**UI / Ads (T-211/T-311/T-312)**: presenter bind/unbind đối xứng ở `OnEnable/OnDisable` (kể cả `TargetRegistry`, `UserSettings.Changed`, `PointerBlocker` so sánh trước khi gỡ); view an toàn khi đích đã bị Destroy (các `Set*` đều kiểm null Unity). Pool: `TargetReticlePresenter` (Stack, không alloc theo frame, `LateUpdate` đọc `Camera.main` có cache), `FloatingScoreView` (pool 12, tái dùng cái cũ nhất, không alloc theo frame ngoài lúc Spawn). Thời gian UI dùng `unscaledDeltaTime`/`WaitForSecondsRealtime`/`FakeRewardedAd` unscaled → chạy đúng khi `timeScale=0`. `raycastTarget` chỉ bật ở `ReloadButton` (HUD) và các nút/panel lúc hiển thị. TMP: font `ClaudeCop UI SDF` (Dynamic, nguồn LiberationSans có Latin Extended Additional → đủ dấu tiếng Việt, chuỗi "QUẢNG CÁO" dùng dấu). `GameplayUI` gồm HUD, DamageFlash, PhaseFade, FloatingScore, RevivePopup, GameOver, Win + 1 EventSystem; `Title.unity` dùng `TitlePanel` riêng (EventSystem riêng) → không trùng. Revive: không bấm được khi đang chạy quảng cáo, đếm ngược 10 s tạm dừng khi xem quảng cáo, hết giờ/Bỏ qua → `DeclineRevive`, quảng cáo thất bại → giữ popup.

**Combat/Enemy (T-301/T-302)**: ComboTracker x1→x5 (phát 1 = x1, `ComboMultiplier` trong `ShotResult` là hệ số sau phát), reset khi Miss/Hostage/Environment không-shootable/bị bắn; Pickup/Blocked trung tính. WeaponPickup đăng ký/hủy đối xứng, tắt khi nhặt, `Hide()` trong `OnDisable`. Về Pistol khi hết đạn đặc biệt (`TryRevertToStartingWeapon`, đầy băng). `TargetSelector` Enemy/Grenade > Pickup > Hostage; Justice ưu tiên; Shotgun cắt `MaxTargetsPerShot`. Multi-touch không alloc (`Touchscreen.touches` đọc theo chỉ số). HostageActor: chỉ trúng khi `Exposed`, tắt khi bị bắn/đi, `Left` phát khi tự rời; `EncounterWave` Cleared **không tính con tin**, sau Cleared `DismissExtras` (con tin còn lại, thùng chưa nhặt); preset/`ApplyReticleTime` chỉ có hiệu lực với enemy chưa kích hoạt. `EnemyPreset` ×4 tồn tại.

**Jev nền (T-305)**: kiểu dữ liệu `JevQuestion/JevRequest/JevChoiceAnswer/JevResponse` khớp API TypeSafe (state/model/questions; Choice criteria dạng dictionary {tên: mô tả} theo ghi chú 422; Noul/Choice/Score; `answers[id]` với `choice/probabilities/confidence`); `OfflineJevClient` đồng bộ, deterministic, trả cùng kiểu online; `JevPolicy` (timeout/lỗi/noul/confidence < 0.6/Jev tắt → mặc định 2.5 s); `JevDecisionLog` có `DecisionMade` + reset tĩnh; asmdef Jev → Core + Enemy đúng bảng. `PlayerStatsTracker` reset theo `PhaseStarted`, hủy đăng ký đối xứng. (Xem F-204.)

**Hiệu năng mobile**: không Find/GetComponent/LINQ/alloc trong `Update` của Camera/Game/Combat/Enemy/UI; `TapShooter.hits/pending` tái dùng, `projectFunc` cache; `UiPointerBlocker` không alloc.

---

## Trước khi commit (nhắc Liaison)
- **KHÔNG commit** (theo chủ dự án): `Assets/Screenshots/` (+ `Assets/Screenshots.meta`, hiện 13 ảnh), `ProjectSettings/Packages/com.unity.probuilder/`, `ProjectSettings/SceneTemplateSettings.json`. Gợi ý thêm `Assets/Screenshots*` vào `.gitignore`.
- `Assets/TextMesh Pro/` (TMP Essentials: Fonts, Resources, Shaders, Sprites; đủ `.meta`) — hợp lệ để commit. Cần đi kèm `Assets/_Game/UI/Fonts/ClaudeCop UI SDF.asset` (font Dynamic ~2 MB, đã có trong `_Game`).
- Có thể commit (Unity tự serialize, không gây hại): `Assets/Settings/Mobile_RPAsset.asset`, `DefaultVolumeProfile.asset`, `ProjectSettings/{ProjectSettings,QualitySettings,TimeManager,EditorBuildSettings}.asset`. `ShaderGraphSettings.asset`, `URPProjectSettings.asset` mới bị Unity sửa (15:52–15:53) nên cân nhắc loại nếu không có lý do.
- Commit `FX/`, `JevDirector*`, `Area_P3`, bảng debug Jev **sau** R-W4, không gộp vào commit của W2/W3.

## Những gì reviewer KHÔNG kiểm chứng được — chủ dự án cần tự bấm thử (reviewer không vào Play)
1. **Luồng đầy đủ Title → Level_01 → thắng/thua → Restart**: Mở `Title.unity`, Play: bấm START (vào Level_01 và camera chạy ngay), chơi hết Phase 1 → Win; Restart → vào lại và camera chạy lại, điểm/tim/đạn/combo về ban đầu, không còn panel Win/GameOver.
2. **Revive**: để mất đủ 3 tim → game đứng hình, popup đếm 10 s. Thử: (a) Xem quảng cáo → đếm 3 s → hồi 3 tim, 1 s ân hạn (kẻ địch đứng yên), chơi tiếp tại chỗ; (b) Bỏ qua / để hết 10 s → Game Over; (c) Mất tim lần 2 sau revive → vào Game Over luôn (không còn lượt). Sau mỗi nhánh kiểm tra `Time.timeScale = 1` (enemy còn chạy, không đứng yên mãi).
3. **Khung hình & độ mượt camera** thực tế: tốc độ 3.5–4 m/s, ray S4 (rẽ 90°) không chóng mặt, blend vào góc S3 (~1 s), kill zoom, nhún/rung cầm tay; bật "Giảm chuyển động" ở Title rồi so sánh.
4. **Cảm giác bắn**: nhịp chuyển Move→Combat có bị dừng quá lâu (pause `CameraBlend`); chữ điểm bay; combo x1→x5 hiển thị (điểm sẽ chưa nhân cho tới khi sửa F-201).
5. **Thiết bị Android**: multi-touch (ngón thứ hai khi ngón đầu còn giữ), tap nhanh < 1 frame có bị mất không (`wasPressedThisFrame` trên touch), 60 FPS trong gameplay (Title 30 FPS — F-207), safe area/notch, bấm nút RELOAD không bắn thêm 1 phát, súng máy giữ ngón (F-202).
6. **Tiếng Việt có dấu trong TMP** (tiêu đề Phase, "QUẢNG CÁO", Title) hiển thị đủ dấu trên máy thật (font Dynamic nạp glyph lần đầu có thể giật nhẹ).
7. **Sandbox**: `gameplay-coder-camera.unity` (Probe log xoay/roll), `gameplay-coder-combat.unity` (Shotgun/MG/pickup), `gameplay-coder-enemy.unity` (con tin, Justice, preset) – chạy được các thử nghiệm đã nêu trong báo cáo W2/W3.

## Việc đề nghị theo thứ tự
1. F-201 (bật combo nhân điểm + sửa test) — trước khi cho chủ dự án xem demo M1/M2.
2. F-202, F-207, F-204 (nhỏ, nên làm ở đầu W4 trước T-402).
3. Cập nhật tài liệu F-212; ghi F-209 vào T-403.
4. F-203/F-205/F-206/F-208/F-210/F-211 xử lý dần.
