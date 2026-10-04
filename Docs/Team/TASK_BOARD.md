# Task Board — ClaudeCop2

> Do **project-manager** duy trì. Trạng thái: TODO · IN PROGRESS · IN REVIEW · DONE · BLOCKED
> Kế hoạch hiện hành: **DEMO M1 + M2** (lập 2026-10-04), nguồn: `Docs/Design/Plan_VirtuaCop2_Mobile.md`.
> **Đội gọn:** gameplay-coder (Core, Combat, FX, Enemy, Camera, Game, Jev Offline, ghép scene, tài nguyên dùng chung) · ui-coder (UI/Ads) · level-designer · reviewer (1 lần/wave). combat-coder, enemy-coder, jev-coder nghỉ đến M3/M4.
> **Cập nhật 2026-10-04 (sau W2–W4):** W1–W4 xong, M2 chơi được (bot kiểm Title→Start→3 Phase→Win, Restart, 3 nhánh Revive đều PASS). Đang làm polish W4 (T-404/T-405/T-412). Còn lại: **R-W4** → chủ dự án bấm thử → **commit một lần sau khi demo xong**. Báo cáo: [Reports/W1.md](Reports/W1.md), [Reports/W2-W4.md](Reports/W2-W4.md).

## Quy trình làm việc
> Luật chung (phạm vi sở hữu, asmdef, scene, git, **Play mode**): xem [Conventions.md](Conventions.md)
```
Chủ dự án ⇄ Liaison (agent chính)
                │  1. ý tưởng
                ▼
        game-designer     → phân tích ý tưởng (Docs/Design/)
                │  2. chủ dự án duyệt thiết kế
                ▼
        project-manager   → chia task theo wave, viết prompt giao việc
                │  3. Liaison giao task song song
                ▼
      gameplay-coder · ui-coder · level-designer
                │  4. kết quả
                ▼
            reviewer      → APPROVED / CHANGES REQUESTED (1 lần/wave)
                │  5. Liaison mang kết quả về
                ▼
        project-manager   → cập nhật board, viết báo cáo
                │  6. Liaison tóm tắt cho chủ dự án
                ▼
   chủ dự án đồng ý → từng agent commit/push đúng file của mình (tuần tự)
```

> **Luật Play mode (từ W2):** trong một Unity Editor, **mỗi lúc chỉ 1 phiên agent được vào Play mode**. Chi tiết: Conventions mục 3.

## Lộ trình
| Wave | gameplay-coder | ui-coder | level-designer | reviewer | Mốc |
|---|---|---|---|---|---|
| W1 | T-101 → T-102 Core → T-103 ∥ T-104 | T-111 | T-121 Phase 1 | — | Core có sẵn ✔ |
| W1-fix | T-104 sửa F-101…F-106 | — | — | kiểm lại ở R-W2-W3 ✔ | Core đóng băng ✔ |
| W2 | T-201 ∥ T-202 → T-203 ghép M1 | T-211 | T-221 Phase 2 | R-W1 ✔ | **M1 chơi được** ✔ |
| W3 | T-301 · T-302 · T-303 · T-304 · T-305 | T-311 · T-312 (+ chuyển TMP) | T-321 Phase 3 + SurfaceMaterialTag | R-W2-W3 (gộp) ✔ | Demo M1 ✔ |
| W4 | F-201…F-208 · T-401 FX · T-402 JevDirector → T-403 ghép M2 → polish T-404/T-405 | T-411 · T-412 | (T-404 nếu material thuộc Level) | **R-W4** (sau polish) | **M2 xong** ✔ (chờ R-W4) |

## Bảng task
| ID | Wave | Owner | Mục tiêu | Phụ thuộc | Trạng thái |
|---|---|---|---|---|---|
| T-101 | W1 | gameplay-coder | Khung `Assets/_Game/`, Android/landscape/URP mobile, Input Actions | — | DONE (chờ commit) |
| T-102 | W1 | gameplay-coder | `ClaudeCop.Core` C1–C20 + Core.Tests | T-101 | DONE (chờ commit) — **đóng băng** |
| T-103 | W1 | gameplay-coder | Combat M1: WeaponData ×3, TapShooter, đạn/reload, CombatConfig | T-102 | DONE (chờ commit) |
| T-104 | W1 | gameplay-coder | Enemy M1 + sửa F-101…F-106 | T-102 | DONE (chờ commit) |
| T-111 | W1 | ui-coder | asmdef UI/Ads/UI.Editor, View + prefab, UIConfig, `IRewardedAd`/`FakeRewardedAd` | T-101 | DONE (chờ commit) |
| T-121 | W1 | level-designer | Blockout Phase 1 (Street) → `Level_01.prefab` | T-101 | DONE (chờ commit) |
| R-W1 | W2 | reviewer | Review gộp W1 → `Reviews/R-W1.md` | W1 | DONE (APPROVED có điều kiện) |
| T-201 | W2 | gameplay-coder | Camera M1: PhaseDirector, CameraShot, RailCameraDriver, CameraFeelApplier, shake, cờ `startOnGamePlaying` | T-102 | DONE (chờ commit) |
| T-202 | W2 | gameplay-coder | Game M1: GameManager, PlayerHealth, ScoreSystem, `GameFlow` thuần (test được), 60 FPS | T-104 | DONE (chờ commit) |
| T-211 | W2 | ui-coder | Presenter HUD/vòng target/nháy đỏ/Win/GameOver, `UiPointerBlocker` (F-109), EventSystem nằm trong `GameplayUI` | T-111 | DONE (chờ commit) |
| T-221 | W2 | level-designer | Blockout Phase 2 (Warehouse) + F-114, F-115 | T-121 | DONE (chờ commit) |
| T-203 | W2 | gameplay-coder | Ghép M1 `Scenes/Gameplay/Level_01.unity` + Build Settings | T-201, T-202, T-211, T-121 | DONE (chờ commit) |
| R-W2-W3 | W4 đầu | reviewer | Review gộp W2+W3 → `Reviews/R-W2-W3.md` | W2, W3 | DONE (0 Blocker, 1 Major F-201 → đã sửa) |
| T-301 | W3 | gameplay-coder | Combat M2: Combo, Shotgun/MG, WeaponPickup, multi-touch; F-107, F-108 | T-103 | DONE (chờ commit) |
| T-302 | W3 | gameplay-coder | Enemy M2: `HostageActor`, `EnemyPreset` ×4, Justice + đầu hàng, API cấu hình đợt | T-104 | DONE (chờ commit) |
| T-303 | W3 | gameplay-coder | Camera M2: SlowZoom, zoom punch, chuyển Phase thật, Giảm chuyển động | T-201 | DONE (chờ commit) |
| T-304 | W3 | gameplay-coder | Game M2: Revive, `Title.unity`, `GameSession`, bất tử sau trúng 1.5 s | T-202 | DONE (chờ commit) |
| T-305 | W3 | gameplay-coder | Jev nền: kiểu dữ liệu, `IJevClient`, `OfflineJevClient`, `JevPolicy`, `JevConfig`, `PlayerStatsTracker`, `JevDecisionLog` | T-102 | DONE (chờ commit) |
| T-311 | W3 | ui-coder | RevivePopup + quảng cáo giả, fade + tiêu đề Phase (F-112) | T-211 | DONE (chờ commit) |
| T-312 | W3 | ui-coder | Chữ bay, combo, HUD vũ khí, Title + Giảm chuyển động; **chuyển toàn bộ UI sang TMP** (font `ClaudeCop UI SDF`) | T-211 | DONE (chờ commit) |
| T-321 | W3 | level-designer | Phase 3 (Rooftop) + `SurfaceMaterialTag` (182 collider) + chỉnh P1 S6 + Batching Static | T-221 | DONE (chờ commit) — R-W4 kiểm |
| F-2xx fix | W4 | gameplay-coder / ui-coder | Sửa F-201, F-202, F-204, F-205, F-207, F-208 | R-W2-W3 | DONE (chờ commit) — R-W4 kiểm lại |
| T-401 | W4 | gameplay-coder | FX: `FxSystem` tự raycast theo `ShotFired`, pool, theo SurfaceMaterial; asmdef `ClaudeCop.FX` | T-321 | DONE (chờ commit) — R-W4 |
| T-402 | W4 | gameplay-coder | JevDirector (Offline) áp vào EncounterWave; kèm sửa điểm Shotgun n², dọn `DefaultVolumeProfile`, `Mobile_RPAsset` (MSAA 2, renderScale 1.0, HDR off) | T-302, T-305 | DONE (chờ commit) — R-W4 |
| T-411 | W4 | ui-coder | Bảng debug Jev (bật/tắt) | T-305 | DONE (chờ commit) — R-W4 |
| T-403 | W4 | gameplay-coder | Ghép M2: `Level_01.unity` 3 Phase × 6 Shot, 12 EncounterWave (hostage/pickup/preset — đóng F-209), menu `ClaudeCop/Game/Assemble Level_01 (T-403)`; bot kiểm PASS | T-401, T-402, T-411 | DONE (chờ commit) — R-W4 |
| T-404 | W4 polish | gameplay-coder (URP) / level-designer (material trong `Level/`) | Khử nhiễu lấp lánh: tắt Specular Highlights + Environment Reflections trên material blockout | T-403 | IN PROGRESS |
| T-405 | W4 polish | gameplay-coder | Khử hitch khi spawn enemy (prewarm/pool, tránh Instantiate giữa giao tranh) | T-403 | IN PROGRESS |
| T-412 | W4 polish | ui-coder | Thu nhỏ bảng debug JEV (không che vùng bắn) | T-411 | IN PROGRESS |
| R-W4 | W4 kết | reviewer | Review cuối: T-321, T-401, T-402, T-403, T-404/405/412, F-2xx đã sửa + checklist "Kiểm tra" của plan (trừ Props/Jev online) | T-403…T-412 | TODO |
| COMMIT | sau demo | từng owner (Liaison điều phối) | Commit một lần sau khi R-W4 APPROVED + chủ dự án đồng ý; loại file theo F-111 | R-W4 | TODO |

## Vấn đề từ review (F-xxx)
### R-W1
| ID | Mức | Nội dung ngắn | Trạng thái |
|---|---|---|---|
| F-101 | Major | `Enemy` → `EnemyActor` | DONE (xác nhận R-W2-W3) |
| F-102 | Major | Hoãn `Cleared` sang `LateUpdate` | DONE (xác nhận R-W2-W3) |
| F-103 | Minor | Re-register khi bật lại lúc Aiming | DONE (xác nhận) |
| F-104 | Minor | Enemy ngã quanh chân | DONE (xác nhận) |
| F-105 | Minor | Spawn làm con của wave; không kẹt khi Destroy | DONE (xác nhận) |
| F-106 | Minor | Code debug bọc `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD` | DONE (xác nhận) |
| F-107 | Minor | Enemy > Pickup > Hostage; nhặt thùng không tốn đạn | DONE (T-301) |
| F-108 | Minor | Multi-touch; xoá `Hold` | DONE (T-301/T-304) — **chưa thử máy thật** |
| F-109 | Minor | UI ref Combat + `PointerBlocker` | DONE (T-211) |
| F-110 | Minor | manifest không diff | DONE (ghi nhận) |
| F-111 | Minor | File không commit | DONE — chủ dự án quyết: **không commit** `Assets/Screenshots/` (+ `.meta`), `ProjectSettings/Packages/com.unity.probuilder/`, `ProjectSettings/SceneTemplateSettings.json` |
| F-112 | Minor | FakeAd bỏ Close, 1 nguồn 3 s | DONE (T-311) |
| F-113 | Minor | Bảng asmdef | DONE |
| F-114 | Minor | Nấp thấp hide y = −1.2 / cover 1.2 m | DONE (T-221/T-321) — R-W4 kiểm |
| F-115 | Minor | Batching Static `Geometry` | DONE (T-221/T-321) — R-W4 kiểm |
| F-116 | Minor | `UnityEngine.Camera` / alias `UCamera` | DONE (quy ước) |

### R-W2-W3
| ID | Mức | Owner | Nội dung ngắn | Trạng thái |
|---|---|---|---|---|
| F-201 | Major | gameplay-coder | Combo không nhân điểm | DONE — `applyComboMultiplier` bật (asset + mặc định SO), test đổi theo |
| F-202 | Minor | gameplay-coder | MG bắn ngay sau khi nhặt thùng nếu còn giữ ngón | DONE |
| F-203 | Minor | gameplay-coder | Số cân bằng hard-code (TapShooter 0.2 s, ngã 0.25 s, roll 0.3 s, shake 40, HUD 0.25/0.6 s, luật Offline Jev 0.45/0.3/0.25…) | **Dời sau demo** — không ảnh hưởng chức năng; chuyển vào SO (`JevConfig`, `CameraFeelProfile`, `EnemyConfig`, `CombatConfig`, `UIConfig`) ở đợt dọn dẹp |
| F-204 | Minor | gameplay-coder | Stats Jev: Environment tính trượt; Shotgun thổi phồng accuracy | DONE (trước T-402). Phần phân biệt Environment vs IShootable còn cho M3 Props |
| F-205 | Minor | gameplay-coder | Dọn trạng thái khi `PhaseDirector` bị tắt | DONE |
| F-206 | Minor | — | Phase fade dùng thời gian thực | Bỏ qua — không xảy ra trong demo; xem lại khi có Pause menu |
| F-207 | Minor | gameplay-coder | Title chạy 30 FPS | DONE |
| F-208 | Minor | gameplay-coder | Thiếu test luồng GameManager | DONE — có thêm kiểm bằng bot (T-403: Win, Restart, 3 nhánh Revive PASS) |
| F-209 | Info | gameplay-coder | Level_01 chưa nối hostage/pickup/preset | DONE trong T-403 (12 EncounterWave) — R-W4 kiểm |
| F-210 | Minor | gameplay-coder | `CameraShot` tạo CinemachineCamera lúc runtime; `CameraFeelProfile.asset` chưa re-save | Dời sau demo (re-save asset nên làm trước commit nếu kịp) |
| F-211 | Minor | ui-coder | Cấp phát chuỗi theo sự kiện HUD (MG) | Dời sau demo (`SetText` định dạng) |
| F-212 | Minor | project-manager | Tài liệu lạc hậu (asmdef, TMP, Hold, bất tử 1.5 s) | DONE — cập nhật Conventions + board này |

---

## Hợp đồng Core (T-102) — **ĐÃ ĐÓNG BĂNG sau R-W1** (2026-10-04)
Assembly `ClaudeCop.Core`, namespace `ClaudeCop.Core`, không tham chiếu assembly game nào. Bus/registry tĩnh phải **tự reset khi vào Play mode**. Chỉ chủ ghi ở cột "Phát" được raise. **ui-coder chỉ bind qua các hợp đồng này** (+ `JevDecisionLog`). Muốn đổi Core → gửi yêu cầu qua PM.

**Sai khác đã chấp nhận:** `EncounterBase.Id`/`Description` là virtual, `RaiseCleared` là protected · `ITapTarget` không có Transform (UI dùng `AimPoint`) · `UserSettings` key `cc_reduce_motion` · `ShotResult.TargetKind` mặc định Enemy khi Miss/Environment → consumer luôn phân biệt bằng `Outcome` · mọi event phải raise từ main thread (Jev bất đồng bộ phải marshal về).

| # | Tên | Nội dung | Phát / Cài đặt | Nghe / Dùng |
|---|---|---|---|---|
| C1 | `SurfaceMaterial` (enum) | Concrete (mặc định), Wood, Metal, Glass, Foliage, Flesh | — | FX, Props (M3) |
| C2 | `SurfaceMaterialTag` (component) | Gắn chất liệu cho collider môi trường; thiếu thì coi là Concrete | level-designer gắn (182 collider) | FX |
| C3 | `WeaponKind` (enum) | Pistol, Shotgun, MachineGun | — | Combat, Enemy, Jev, UI |
| C4 | `ShotInfo` (struct) | Vị trí tap (screen), điểm trúng, pháp tuyến, hướng đạn, WeaponKind, hệ số lực đẩy | Combat | IShootable, ITapTarget |
| C5 | `IShootable` | `OnShot(ShotInfo)` — vật thể môi trường | Props (M3) | TapShooter |
| C6 | `TargetKind` (enum) | Enemy, Hostage, Pickup, Grenade (dự trữ) | — | |
| C7 | `ITapTarget` | Kind · IsTargetable · AimPoint · HasJusticePoint + JusticePoint · ShowsReticle · ReticleProgress · ExposedTime · Id · `OnTapHit` → `TapOutcome` | EnemyActor, HostageActor, WeaponPickup | TapShooter, UI |
| C8 | `TargetRegistry` (static) | Register/Unregister · danh sách · event Registered/Unregistered | Target | TapShooter, UI |
| C9 | `TapOutcome` (enum) | Miss, Kill, JusticeKill, HostageHit, PickupCollected, Environment, Blocked | | |
| C10 | `ShotResult` (struct) | Outcome · TargetKind · điểm · ReticleProgress · ReactionTime · WeaponKind · ComboMultiplier · TargetsHit | Combat | Game, Jev, UI |
| C11 | `CombatPauseSignal` (static) | Push/Pop có đếm · IsPaused · HasReason · Changed | Camera, Game | Enemy, TapShooter |
| C12 | `DamageSource` (enum) | EnemyShot, HostageHit, Explosion (dự trữ) | | |
| C13 | `IPlayerDamageReceiver` + `PlayerDamageService` | Register/Unregister · `Damage(source, pos)` | PlayerHealth | Enemy, TapShooter |
| C14 | `GameState` (enum) | Title, Playing, RevivePrompt, Win, GameOver | | |
| C15 | `GameEvents` (static) | LivesChanged · PlayerDamaged · ScoreChanged · ScoreAwarded · GameStateChanged · ReviveAvailabilityChanged · snapshot | Game | UI, Camera, Combat, Jev |
| C16 | `CombatEvents` (static) | ShotFired · ShotResolved · AmmoChanged · WeaponChanged · ReloadStateChanged · ComboChanged · OutOfAmmo · snapshot | Combat | UI, Game, Jev, FX |
| C17 | `EncounterBase` (abstract) | Begin · IsActive · IsCleared · Cleared · Id · mô tả | EncounterWave | Camera, Jev |
| C18 | `RailEvents` (static) | PhaseStarted · PhaseTransition · MoveSegmentStarted · EncounterStarted · EncounterCleared · LevelCompleted | Camera | Game, UI, Jev |
| C19 | `UserSettings` (static) | ReduceMotion (key `cc_reduce_motion`) · Changed | UI ghi | Camera đọc |
| C20 | `GameCommands` (static) | RequestReload · RequestStartGame · RequestRestart · RequestRevive · DeclineRevive · RequestDebugOverlayToggle | UI | Combat, Game |

- `Time.timeScale` do **Game** điều khiển; UI dùng unscaled time.
- `IRewardedAd` ở `ClaudeCop.Ads` (không ở Core).
- `JevDecisionLog` (Jev): event DecisionMade + bản ghi gần nhất → bảng debug (T-411).

## API module hiện có (ngoài Core)
- **Combat:** `TapShooter` (`Equip`, `StartReload`, `CurrentWeapon`, `Ammo`, `IsReloading`, static `PointerBlocker` — UI gán qua `UiPointerBlocker`), `ComboSystem`/`ComboTracker`, `TargetSelector` (Enemy/Grenade > Pickup > Hostage), `WeaponPickup`; multi-touch đọc `Touchscreen.touches`. SO: `WeaponData` ×3, `CombatConfig`.
- **Enemy:** `EnemyActor`, `HostageActor`, `EncounterWave : EncounterBase` (hostage/pickup spawn, preset, `ApplyReticleTime` cho enemy chưa kích hoạt), `EnemyPreset` ×4, `EnemyConfig`.
- **Camera:** `PhaseDirector` (`startOnGamePlaying`), `CameraShot` (Move/Combat), `RailCameraDriver`, `CameraFeelApplier`, `CameraFeelProfile` (SO, `Settings/`), SlowZoom/zoom punch/kill zoom.
- **Game:** `GameManager`, `PlayerHealth`, `ScoreSystem`, `GameFlow`/`LifeTracker`/`ScoreCalculator` (thuần, có test), `GameSession` (`PendingStart`), `TitleLauncher`, `GameConfig`; menu Editor `ClaudeCop/Game/Assemble Level_01 (T-403)`; bot debug (bọc define).
- **Jev:** `IJevClient`, `OfflineJevClient`, `JevPolicy`, `JevConfig`, `PlayerStatsTracker`, `JevDecisionLog`, `JevDirector`.
- **FX:** `FxSystem` (nghe `ShotFired`, tự raycast, pool, theo `SurfaceMaterialTag`).
- **UI:** TMP toàn bộ; `GameplayUI` (HUD, DamageFlash, PhaseFade, FloatingScore, RevivePopup, GameOver, Win, EventSystem), `TitlePanel`, bảng debug Jev.
- **Level:** `Level_01.prefab` 3 Phase (Street/Warehouse/Rooftop) × 6 Shot, `SurfaceMaterialTag` trên 182 collider.

## Quy ước level
- Prefab `Assets/_Game/Level/Level_01.prefab`, nhóm `Area_P1_Street`, `Area_P2_Warehouse`, `Area_P3_Rooftop`; scene làm việc `Assets/_Game/Scenes/Levels/Level_01_Blockout.unity`.
- Shot `S1..Sn` theo thứ tự chạy; spawn cùng số với góc giao tranh: `CamPoint_P1_S2` ↔ `EnemySpawn_P1_W2_01`, `HostageSpawn_P1_W2_01`, `PickupSpawn_P1_W2_01`.
- `EnemySpawn_*`/`HostageSpawn_*` = vị trí nấp (ở chân); con `Peek` = vị trí ló ra. Nấp thấp: hide y = −1.2 (hoặc cover ≥ 1.2 m).
- `RailHint_P<p>_S<s>_NN` dọc đoạn di chuyển. Môi trường layer `Default`, `Geometry` Batching Static. Camera 1.6 m; enemy 2 m; nấp thấp 1.1 m; cửa 2.2 m; enemy cách camera 6–15 m; đoạn di chuyển 10–20 m; tránh cua > 90°.
- Material blockout: **tắt Specular Highlights và Environment Reflections** (T-404, khử nhiễu lấp lánh trên mobile).

## Số liệu
**Từ plan:** ray 3–4 m/s (mặc định 3.5) · nhìn trước 3–5 m · xoay ngang ≤ 60°/s · nghiêng ≤ 2° · ngẩng/cúi ≤ 10° · đổi hướng > 90° dùng Cut · Blend 0.3–0.8 s, góc phụ 0.5–0.7 s · rung cầm tay Perlin 0.3 / 0.4 · nhún 2–3 cm · dolly-in ≤ 8–10° FOV · zoom punch −5° trong 0.15 s, trả 0.4 s · shake trúng đạn 0.2 s · di chuyển 3–5 s (≤ 6), giao tranh 8–15 s, nghỉ 0.3 s · vòng target 2.5 s · Pistol 6 · Shotgun 6 · MG 30 · 3 mạng · Revive 10 s, quảng cáo giả 3 s, 1 lần/lượt, hồi 3 tim · combo x1…x5 · 2–5 enemy/đợt · Jev: timeout 1.5 s, confidence 0.6, reticle_time 2.0/2.5/3.0 · Canvas 1920×1080 · 60 FPS.

**Đang dùng trong SO (đã chốt cho demo):** bán kính trúng Pistol 90 px / Shotgun 180 px ≤ 5 mục tiêu · Justice 35 px · reload 0.5 s · MG 10 phát/s · hạ enemy 100 + thưởng sớm ≤ 100 · Justice 300, ×1.5 nếu vòng còn xanh · **combo nhân điểm: bật (`applyComboMultiplier`)**, Shotgun không nhân thêm theo số mục tiêu (đã bỏ n²) · màu vòng xanh < 0.4 ≤ vàng < 0.75 ≤ đỏ · ló ra 0.3 s · spawn so le 0.4–1.0 s · nấp 0.8 s · **bất tử 1.5 s sau khi trúng đạn** (thay 0.5 s) · ân hạn 1.0 s sau revive · fade Phase 0.4 / tiêu đề 1.0 / 0.4 s · **`maxMoveSeconds` 5.4 s** (tự tăng tốc tới `maxRailSpeed` 4 m/s) · **thời lượng blend tối thiểu theo góc: góc (độ) / 40** · nhặt thùng không tốn đạn · hết đạn đặc biệt → Pistol đầy băng.
**Đồ họa (Mobile_RPAsset):** MSAA 2×, renderScale 1.0, HDR tắt.

## Quyết định chủ dự án (đã chốt)
- Dùng **TextMeshPro** (chữ tiếng Việt có dấu, font `ClaudeCop UI SDF`).
- Không commit `Assets/Screenshots/`, `ProjectSettings/Packages/com.unity.probuilder/`, `ProjectSettings/SceneTemplateSettings.json`.
- **Commit một lần sau khi demo xong** (sau R-W4).
- Ghi chú Jev: API choice cần `criteria` dạng **dictionary** {tên: mô tả}; key TypeSafe trong biến môi trường `TYPESAFE_API_KEY` (đã kiểm, jev-1.13.0) — **không ghi key vào repo**.

## Backlog sau demo (dọn dẹp, chưa chia task)
- F-203: đưa số cân bằng hard-code vào SO (đặc biệt luật Offline Jev).
- F-210 (re-save `CameraFeelProfile.asset`, CinemachineCamera serialize), F-211 (HUD không cấp phát chuỗi).
- Pool tổng quát (enemy/hostage/pickup/FX) nếu T-405 chỉ là prewarm.
- FX hạt đẹp hơn (tia lửa, mảnh vụn, vết đạn theo chất liệu).
- Xoá `Assets/Scenes/SampleScene.unity` (không trong build).
- Kiểm trên Android thật + profile hiệu năng.

## Backlog M3/M4 (giữ nguyên)
- Props tương tác (PhysicsProp, FoliageProp, BreakableGlass + tool cắt mảnh, ShootableDoor, ExplosiveBarrel, LampProp, HangingSign, PropPool, ≤ 40 Rigidbody, mảnh vỡ 4 s); phân biệt Environment vs IShootable cho stats Jev (F-204 phần còn lại).
- Grenade, HumanShieldEnemy (enemy_tactic grenade/human_shield thật).
- Jev: ProxyJevClient (Jev Online qua proxy — nhớ `criteria` dictionary), DirectJevClient (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`), `Server/python` + `Server/node`, `.env`/`.gitignore`, rank S/A/B/C + weakness, cửa sổ Editor `JevPlaytestReview`.
- Trả thư mục cho combat-coder / enemy-coder / jev-coder; boss, âm thanh, rung, slow-motion, quảng cáo thật, Pause menu (xem lại F-206).
