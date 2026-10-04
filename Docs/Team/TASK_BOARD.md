# Task Board — ClaudeCop2

> Do **project-manager** duy trì. Trạng thái: TODO · IN PROGRESS · IN REVIEW · DONE · BLOCKED
> Kế hoạch hiện hành: **M3** (lập 2026-10-04, sau khi chủ dự án chấp nhận demo M1 + M2 + màn dọc + offline; **Q1–Q4 chốt mặc định 2026-10-04**). Nguồn: `Docs/Design/Plan_VirtuaCop2_Mobile.md`. Prompt giao việc: [Prompts/W6.md](Prompts/W6.md), [Prompts/W7.md](Prompts/W7.md). MCP kết nối: stdio port 6400. **Sẵn sàng dispatch W6.**
> **Đội gọn:** gameplay-coder (Core, Combat, Props, FX, Enemy, Camera, Game, Jev Offline, ghép scene, tài nguyên dùng chung) · ui-coder (UI/Ads) · level-designer · reviewer = tester (viết test Test Runner + checklist, chủ dự án chạy và phản hồi; không duyệt code). combat-coder, enemy-coder nghỉ (trả thư mục dời M4). jev-coder đã giải thể (game offline).
> **Cập nhật 2026-10-04 (lập M3):** chủ dự án chấp nhận các màn hiện tại (3 Phase, màn dọc, offline). M3 = Props bắn được + lựu đạn + human shield + Jev offline mở rộng (rank) + dọn nợ. Báo cáo cũ: [Reports/W1.md](Reports/W1.md), [Reports/W2-W4.md](Reports/W2-W4.md).

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
            tester        → viết test + checklist; chủ dự án chạy, báo lỗi
                │  5. Liaison mang kết quả về
                ▼
        project-manager   → cập nhật board, viết báo cáo
                │  6. Liaison tóm tắt cho chủ dự án
                ▼
   chủ dự án đồng ý → từng agent commit/push đúng file của mình (tuần tự)
```

> **Luật Play mode (từ W2):** trong một Unity Editor, **mỗi lúc chỉ 1 phiên agent được vào Play mode**. Chi tiết: Conventions mục 3.

## Lộ trình
| Wave | gameplay-coder | ui-coder | level-designer | tester | Mốc |
|---|---|---|---|---|---|
| W1 | T-101 → T-102 Core → T-103 ∥ T-104 | T-111 | T-121 Phase 1 | — | Core có sẵn ✔ |
| W1-fix | T-104 sửa F-101…F-106 | — | — | kiểm lại ở R-W2-W3 ✔ | Core đóng băng ✔ |
| W2 | T-201 ∥ T-202 → T-203 ghép M1 | T-211 | T-221 Phase 2 | R-W1 ✔ | **M1 chơi được** ✔ |
| W3 | T-301 · T-302 · T-303 · T-304 · T-305 | T-311 · T-312 (+ TMP) | T-321 Phase 3 + SurfaceMaterialTag | R-W2-W3 ✔ | Demo M1 ✔ |
| W4 | F-2xx · T-401 FX · T-402 JevDirector → T-403 ghép M2 → polish T-404/T-405 | T-411 · T-412 | — | (R-W4 → thay bằng TEST-W6) | **M2 xong** ✔ |
| W5 | T-500 màn dọc + offline · T-503 auto-frame · T-501 enemy nhiều tầng | (theo T-500) | — | — | Chủ dự án chấp nhận ✔ |
| **G-600** | **Cổng: commit baseline M1+M2+W5** (khuyến nghị, chờ lệnh chủ dự án) | | | | Tách diff M3 |
| **W6** | Phiên A: T-600 → T-601 → T-605 · Phiên B: T-602 · Phiên C: T-603 (sau T-600) · Phiên D: T-604 | T-611 | T-621 marker M3 | TEST-W6 | Module M3 xong trong sandbox |
| **W7** | T-701 → T-702 ghép M3 | T-711 Win rank | T-721 (dự phòng) | TEST-W7 | **M3 chơi được** |
| sau W7 | COMMIT M3 (tuần tự theo owner) | | | | |

Lượt Play W6: T-601 → T-602 → T-605 → T-611. Lượt Play W7: T-711 → T-702.

## Bảng task — M3 (W6, W7)
| ID | Wave | Owner | Mục tiêu | Phụ thuộc | Trạng thái |
|---|---|---|---|---|---|
| G-600 | trước W6 | Liaison / chủ dự án | Commit baseline M1+M2+W5 (loại file theo F-111) trước khi M3 sửa Core/Combat/Enemy | — | TODO (khuyến nghị) |
| T-600 | W6 | gameplay-coder (A) | **Mở băng Core tối thiểu**: C21 `BlastEvents`, C22 `BlastReport`; làm rõ C9 (Environment = trúng IShootable, tường trơ = Miss → đóng F-204); F-203 phần Combat | Q1 duyệt | TODO |
| T-601 | W6 | gameplay-coder (A) | Asmdef `ClaudeCop.Props`; `PropConfig`, `PropPool` (≤ 40 RB, mảnh 4 s), `PhysicsProp` (hộp), `ExplosiveBarrel` (3 m, nổ dây chuyền, raise BlastEvents); prefab + sandbox | T-600 | TODO |
| T-605 | W6 | gameplay-coder (A) | `BreakableGlass` 6–10 mảnh + menu cắt mảnh; FX nổ (FX nghe BlastEvents) | T-601 | TODO |
| T-602 | W6 | gameplay-coder (B) | Enemy: Grenadier + `Grenade` (ITapTarget Kind Grenade, bay 1.5 s), `HumanShieldEnemy` (đầu 45 px / Justice / thân con tin = HostageHit); API EncounterWave mới; F-203 phần Enemy | — | TODO |
| T-603 | W6 | gameplay-coder (C) | Jev: `JevRankEvaluator` + hợp đồng rank (`JevRank`, `JevWeakness`, `JevRankResult`, điểm công bố tĩnh); `wave_preset`, `weapon_drop`; stats cả màn + Environment/Blast; F-203 phần Jev | T-600 | TODO |
| T-604 | W6 | gameplay-coder (D) | Dọn nợ: F-203 Camera/Game (+ field M3: rung nổ, điểm nổ 100, lựu đạn 50), F-210 re-save asset, xoá `SampleScene` | — | TODO |
| T-611 | W6 | ui-coder | Vòng target lựu đạn (0.6×, cam), banner "LỰU ĐẠN!", F-211 `SetText`, F-203 phần UI | — | TODO |
| T-621 | W6 | level-designer | Marker M3 trong `Level_01.prefab`: `PropSlot_Barrel/Box/Glass_*`, `GrenadierSpawn_*`, `ShieldSpawn_*` (+ Peek) cho P1–P3 | — | TODO |
| TEST-W6 | W6 kết | reviewer (tester) | Test EditMode/PlayMode (asmdef `ClaudeCop.Tests.EditMode/PlayMode`) + `Testing/TEST-W6.md` (gồm hồi quy M2); xử lý `Assets/Tests/` lạc | W6 | TODO |
| T-701 | W7 | gameplay-coder | Score nghe BlastEvents (100 × combo), lựu đạn 50, Explosion trừ mạng; Camera rung nổ; JevDirector áp wave_preset/weapon_drop; công bố rank khi LevelCompleted; bot mở rộng | T-601…T-604 | TODO |
| T-711 | W7 | ui-coder | WinPanel rank S/A/B/C + weakness + thống kê; chữ bay "NỔ!"; bảng debug Jev thêm quyết định mới | T-603, T-611 | TODO |
| T-702 | W7 | gameplay-coder | Assembler: PropSlot → prefab Props, Grenadier/Shield → EncounterWave; PropPool vào scene; bot Title→Win với nổ/lựu đạn/shield/rank; ≤ 40 RB | T-701, T-621, T-711, T-605 | TODO |
| T-721 | W7 | level-designer | (Dự phòng) chỉnh marker theo T-702/TEST-W7 | T-702 | TODO (nếu cần) |
| TEST-W7 | W7 kết | reviewer (tester) | PlayMode smoke Level_01 + `Testing/TEST-W7.md` toàn M3 + hồi quy | W7 | TODO |
| COMMIT-M3 | sau W7 | từng owner (Liaison điều phối) | Commit sau khi chủ dự án chạy TEST-W7 và đồng ý; loại file F-111 | TEST-W7 | TODO |

## Bảng task — M1/M2/W5 (lịch sử)
| ID | Wave | Owner | Mục tiêu | Phụ thuộc | Trạng thái |
|---|---|---|---|---|---|
| T-101 | W1 | gameplay-coder | Khung `Assets/_Game/`, Android, URP mobile, Input Actions | — | DONE (chờ commit) |
| T-102 | W1 | gameplay-coder | `ClaudeCop.Core` C1–C20 + Core.Tests | T-101 | DONE (chờ commit) — đóng băng (mở có chủ đích ở T-600) |
| T-103 | W1 | gameplay-coder | Combat M1: WeaponData ×3, TapShooter, đạn/reload, CombatConfig | T-102 | DONE (chờ commit) |
| T-104 | W1 | gameplay-coder | Enemy M1 + sửa F-101…F-106 | T-102 | DONE (chờ commit) |
| T-111 | W1 | ui-coder | asmdef UI/Ads/UI.Editor, View + prefab, UIConfig, `IRewardedAd`/`FakeRewardedAd` | T-101 | DONE (chờ commit) |
| T-121 | W1 | level-designer | Blockout Phase 1 (Street) → `Level_01.prefab` | T-101 | DONE (chờ commit) |
| R-W1 | W2 | reviewer | Review gộp W1 → `Reviews/R-W1.md` | W1 | DONE |
| T-201 | W2 | gameplay-coder | Camera M1: PhaseDirector, CameraShot, RailCameraDriver, CameraFeelApplier, shake | T-102 | DONE (chờ commit) |
| T-202 | W2 | gameplay-coder | Game M1: GameManager, PlayerHealth, ScoreSystem, `GameFlow`, 60 FPS | T-104 | DONE (chờ commit) |
| T-211 | W2 | ui-coder | Presenter HUD/vòng target/nháy đỏ/Win/GameOver, `UiPointerBlocker` | T-111 | DONE (chờ commit) |
| T-221 | W2 | level-designer | Blockout Phase 2 (Warehouse) + F-114, F-115 | T-121 | DONE (chờ commit) |
| T-203 | W2 | gameplay-coder | Ghép M1 `Level_01.unity` + Build Settings | T-201, T-202, T-211, T-121 | DONE (chờ commit) |
| R-W2-W3 | W4 | reviewer | Review gộp W2+W3 | W2, W3 | DONE |
| T-301 | W3 | gameplay-coder | Combat M2: Combo, Shotgun/MG, WeaponPickup, multi-touch | T-103 | DONE (chờ commit) |
| T-302 | W3 | gameplay-coder | Enemy M2: `HostageActor`, `EnemyPreset` ×4, Justice, API đợt | T-104 | DONE (chờ commit) |
| T-303 | W3 | gameplay-coder | Camera M2: SlowZoom, zoom punch, chuyển Phase, Giảm chuyển động | T-201 | DONE (chờ commit) |
| T-304 | W3 | gameplay-coder | Game M2: Revive, `Title.unity`, `GameSession`, bất tử 1.5 s | T-202 | DONE (chờ commit) |
| T-305 | W3 | gameplay-coder | Jev nền Offline | T-102 | DONE (chờ commit) |
| T-311 | W3 | ui-coder | RevivePopup + quảng cáo giả, fade + tiêu đề Phase | T-211 | DONE (chờ commit) |
| T-312 | W3 | ui-coder | Chữ bay, combo, HUD vũ khí, Title; chuyển UI sang TMP | T-211 | DONE (chờ commit) |
| T-321 | W3 | level-designer | Phase 3 (Rooftop) + `SurfaceMaterialTag` (182 collider) | T-221 | DONE (chờ commit) |
| F-2xx fix | W4 | gameplay-coder / ui-coder | Sửa F-201, F-202, F-204, F-205, F-207, F-208 | R-W2-W3 | DONE (chờ commit) |
| T-401 | W4 | gameplay-coder | FX: `FxSystem`, pool, theo SurfaceMaterial | T-321 | DONE (chờ commit) |
| T-402 | W4 | gameplay-coder | JevDirector (Offline) + URP mobile | T-302, T-305 | DONE (chờ commit) |
| T-411 | W4 | ui-coder | Bảng debug Jev | T-305 | DONE (chờ commit) |
| T-403 | W4 | gameplay-coder | Ghép M2: 3 Phase × 6 Shot, 12 EncounterWave, menu Assemble; bot PASS | T-401, T-402, T-411 | DONE (chờ commit) |
| T-404 | W4 polish | gameplay-coder / level-designer | Khử nhiễu lấp lánh (tắt Specular + Env Reflections) | T-403 | DONE (chủ dự án chấp nhận màn 2026-10-04 — Liaison xác nhận) |
| T-405 | W4 polish | gameplay-coder | Khử hitch spawn enemy (prewarm/pool) | T-403 | DONE (như trên). Pool tổng quát → M4 |
| T-412 | W4 polish | ui-coder | Thu nhỏ bảng debug JEV | T-411 | DONE (như trên) |
| R-W4 | W4 kết | reviewer | Review cuối M2 | — | THAY THẾ — reviewer thành tester; hồi quy M2 nằm trong TEST-W6 |
| T-500 | W5 | (Liaison) | Màn dọc + offline (bỏ Jev online, jev-coder) | — | DONE |
| T-503 | W5 | (Liaison) | Camera tự căn khung theo tỉ lệ màn thật | T-500 | DONE |
| T-501 | W5 | (Liaison) | Enemy nhiều tầng (`MultiFloorBuilder`, nhóm `T501_MultiFloor`) | T-500 | DONE (chủ dự án chấp nhận) |
| T-502 | W5 | chủ dự án | Chơi thử 3 Stage màn dọc | T-501 | DONE (chấp nhận 2026-10-04) |
| COMMIT | sau demo | từng owner | Commit demo M1+M2+W5 | — | TODO → xem G-600 |

## Vấn đề từ review (F-xxx)
### R-W1
F-101…F-116: **DONE hết** (F-108 multi-touch chưa thử máy thật; F-111: không commit `Assets/Screenshots/` + `.meta`, `ProjectSettings/Packages/com.unity.probuilder/`, `ProjectSettings/SceneTemplateSettings.json`). Chi tiết: `Reviews/R-W1.md`.

### R-W2-W3
| ID | Mức | Owner | Nội dung ngắn | Trạng thái |
|---|---|---|---|---|
| F-201 | Major | gameplay-coder | Combo không nhân điểm | DONE |
| F-202 | Minor | gameplay-coder | MG bắn ngay sau khi nhặt thùng | DONE |
| F-203 | Minor | gameplay-coder / ui-coder | Số cân bằng hard-code | **M3**: chia theo module — Combat (T-600), Enemy (T-602), Jev (T-603), Camera/Game (T-604), UI (T-611) |
| F-204 | Minor | gameplay-coder | Stats Jev: Environment tính trượt; Shotgun | DONE phần Shotgun; phần Environment vs IShootable → **T-600 + T-603** |
| F-205 | Minor | gameplay-coder | Dọn trạng thái PhaseDirector | DONE |
| F-206 | Minor | — | Phase fade dùng thời gian thực | Bỏ qua — xem lại khi có Pause menu (M4) |
| F-207 | Minor | gameplay-coder | Title 30 FPS | DONE |
| F-208 | Minor | gameplay-coder | Thiếu test luồng GameManager | DONE |
| F-209 | Info | gameplay-coder | Level_01 chưa nối hostage/pickup/preset | DONE (T-403) |
| F-210 | Minor | gameplay-coder | CinemachineCamera runtime; asset chưa re-save | **M3 T-604**: re-save asset; phần runtime camera chấp nhận |
| F-211 | Minor | ui-coder | Cấp phát chuỗi HUD | **M3 T-611** |
| F-212 | Minor | project-manager | Tài liệu lạc hậu | DONE |

---

## Hợp đồng Core (T-102) — **ĐÓNG BĂNG sau R-W1**; mở có chủ đích ở T-600 (M3)
Assembly `ClaudeCop.Core`, namespace `ClaudeCop.Core`, không tham chiếu assembly game nào. Bus/registry tĩnh phải **tự reset khi vào Play mode**. Chỉ chủ ghi ở cột "Phát" được raise. **ui-coder chỉ bind qua các hợp đồng này** (+ `JevDecisionLog`, hợp đồng rank của Jev). Muốn đổi Core → gửi yêu cầu qua PM.

**Sai khác đã chấp nhận:** `EncounterBase.Id`/`Description` là virtual, `RaiseCleared` là protected · `ITapTarget` không có Transform (UI dùng `AimPoint`) · `UserSettings` key `cc_reduce_motion` · `ShotResult.TargetKind` mặc định Enemy khi Miss/Environment → consumer luôn phân biệt bằng `Outcome` · mọi event phải raise từ main thread.

| # | Tên | Nội dung | Phát / Cài đặt | Nghe / Dùng |
|---|---|---|---|---|
| C1 | `SurfaceMaterial` (enum) | Concrete (mặc định), Wood, Metal, Glass, Foliage, Flesh | — | FX, Props |
| C2 | `SurfaceMaterialTag` (component) | Gắn chất liệu cho collider môi trường; thiếu thì coi là Concrete | level-designer, prefab Props | FX |
| C3 | `WeaponKind` (enum) | Pistol, Shotgun, MachineGun | — | Combat, Enemy, Jev, UI |
| C4 | `ShotInfo` (struct) | Vị trí tap (screen), điểm trúng, pháp tuyến, hướng đạn, WeaponKind, hệ số lực đẩy | Combat (Props dựng khi nổ) | IShootable, ITapTarget |
| C5 | `IShootable` | `OnShot(ShotInfo)` — vật thể môi trường | Props (M3) | TapShooter |
| C6 | `TargetKind` (enum) | Enemy, Hostage, Pickup, **Grenade (dùng từ M3)** | — | |
| C7 | `ITapTarget` | Kind · IsTargetable · AimPoint · HasJusticePoint + JusticePoint · ShowsReticle · ReticleProgress · ExposedTime · Id · `OnTapHit(shot, isJustice)` → `TapOutcome` | EnemyActor, HostageActor, WeaponPickup, **Grenade, HumanShieldEnemy (M3)** | TapShooter, UI, Props (nổ) |
| C8 | `TargetRegistry` (static) | Register/Unregister · danh sách · event Registered/Unregistered | Target | TapShooter, UI, Props |
| C9 | `TapOutcome` (enum) | Miss, Kill, JusticeKill, HostageHit, PickupCollected, Environment, Blocked. **Từ T-600: Environment = trúng IShootable; tường trơ / không trúng = Miss** | | |
| C10 | `ShotResult` (struct) | Outcome · TargetKind · điểm · ReticleProgress · ReactionTime · WeaponKind · ComboMultiplier · TargetsHit | Combat | Game, Jev, UI |
| C11 | `CombatPauseSignal` (static) | Push/Pop có đếm · IsPaused · HasReason · Changed | Camera, Game | Enemy (+ Grenade), TapShooter |
| C12 | `DamageSource` (enum) | EnemyShot, HostageHit, **Explosion (dùng từ M3: lựu đạn, con tin trúng nổ)** | | |
| C13 | `IPlayerDamageReceiver` + `PlayerDamageService` | Register/Unregister · `Damage(source, pos)` | PlayerHealth | Enemy, TapShooter, Props |
| C14 | `GameState` (enum) | Title, Playing, RevivePrompt, Win, GameOver | | |
| C15 | `GameEvents` (static) | LivesChanged · PlayerDamaged · ScoreChanged · ScoreAwarded · GameStateChanged · ReviveAvailabilityChanged · snapshot | Game | UI, Camera, Combat, Jev |
| C16 | `CombatEvents` (static) | ShotFired · ShotResolved · AmmoChanged · WeaponChanged · ReloadStateChanged · ComboChanged · OutOfAmmo · snapshot | Combat | UI, Game, Jev, FX |
| C17 | `EncounterBase` (abstract) | Begin · IsActive · IsCleared · Cleared · Id · mô tả | EncounterWave | Camera, Jev |
| C18 | `RailEvents` (static) | PhaseStarted · PhaseTransition · MoveSegmentStarted · EncounterStarted · EncounterCleared · LevelCompleted | Camera | Game, UI, Jev |
| C19 | `UserSettings` (static) | ReduceMotion (key `cc_reduce_motion`) · Changed | UI ghi | Camera đọc, UI |
| C20 | `GameCommands` (static) | RequestReload · RequestStartGame · RequestRestart · RequestRevive · DeclineRevive · RequestDebugOverlayToggle | UI | Combat, Game |
| **C21** | `BlastEvents` (static) — **M3, T-600, chờ duyệt Q1** | Event Blasted(BlastReport) · Raise · reset khi vào Play | Props (ExplosiveBarrel) | Game (điểm), Jev (stats), FX (nổ), Camera (rung), UI (chữ "NỔ!") |
| **C22** | `BlastReport` (struct) — **M3, T-600** | Tâm, bán kính, số enemy hạ, số con tin trúng, Id nguồn | Props | như C21 |

**Lý do mở băng (C21/C22):** kill do nổ không đi qua `TapShooter` nên không có `ShotResolved`; Props chỉ ref Core nên cần một bus trong Core để Game cộng điểm, Jev đếm riêng (không làm sai accuracy), FX/Camera/UI phản hồi — không tạo tham chiếu Props ↔ Game/Combat. Lựu đạn và human shield **không** cần hợp đồng mới (dùng C6 Grenade, C12 Explosion, C7, C13 sẵn có).

- `Time.timeScale` do **Game** điều khiển; UI dùng unscaled time.
- `IRewardedAd` ở `ClaudeCop.Ads` (không ở Core).
- `JevDecisionLog` (Jev): event DecisionMade + bản ghi gần nhất → bảng debug.
- **Hợp đồng rank (Jev, M3 T-603)**: enum `JevRank` (S/A/B/C), enum `JevWeakness` (None, Accuracy, Reaction, Damage, Hostage), struct `JevRankResult` (Rank, Weakness, WeaknessText, Accuracy, AvgReactionTime, DamageTaken, HostageHits, RevivesUsed, BlastKills), điểm công bố tĩnh (event RankEvaluated, HasResult, Last; reset khi Play/lượt mới). UI bind ở T-711. Tên chính xác chốt theo báo cáo T-603.

## API module hiện có (ngoài Core)
- **Combat:** `TapShooter` (`Equip`, `StartReload`, `CurrentWeapon`, `Ammo`, `IsReloading`, static `PointerBlocker`), `ComboSystem`/`ComboTracker`, `TargetSelector` (Enemy/Grenade > Pickup > Hostage), `WeaponPickup`; multi-touch. SO: `WeaponData` ×3, `CombatConfig`.
- **Enemy:** `EnemyActor`, `HostageActor`, `EncounterWave : EncounterBase` (hostage/pickup spawn, preset, `ApplyReticleTime`, `ApplyPreset`, `SetPickupPrefab`), `EnemyPreset` ×4 (Calm/Standard/Intense/HostageHeavy), `EnemyConfig`. M3 thêm: Grenadier, `Grenade`, `HumanShieldEnemy`, field spawn mới trong EncounterWave (T-602).
- **Props (M3, mới):** `PropConfig`, `PropPool`, `PhysicsProp`, `ExplosiveBarrel`, `BreakableGlass`; prefab `Prop_Box`, `Prop_Barrel`, `Prop_Glass` (T-601/T-605).
- **Camera:** `PhaseDirector`, `CameraShot` (Move/Combat, AutoFrame), `RailCameraDriver`, `CameraFeelApplier`, `CameraFeelProfile`, SlowZoom/zoom punch/kill zoom.
- **Game:** `GameManager`, `PlayerHealth`, `ScoreSystem`, `GameFlow`/`LifeTracker`/`ScoreCalculator`, `GameSession`, `TitleLauncher`, `GameConfig`; menu `ClaudeCop/Game/Assemble Level_01 (T-403)`, `ClaudeCop/Level/Build Multi-floor Enemies (T-501)`; bot debug.
- **Jev:** `IJevClient`, `OfflineJevClient`, `JevPolicy`, `JevConfig`, `PlayerStatsTracker`, `JevDecisionLog`, `JevDirector`. M3 thêm: `JevRankEvaluator` + hợp đồng rank, câu hỏi `wave_preset`, `weapon_drop`.
- **FX:** `FxSystem` (nghe `ShotFired`, tự raycast, pool). M3: nghe `BlastEvents`.
- **UI:** TMP toàn bộ; `GameplayUI` (HUD, DamageFlash, PhaseFade, FloatingScore, RevivePopup, GameOver, Win, EventSystem), `TitlePanel`, bảng debug Jev. M3: vòng lựu đạn, banner, Win rank.
- **Level:** `Level_01.prefab` 3 Phase × 6 Shot, `SurfaceMaterialTag`, nhóm `T501_MultiFloor`. M3: nhóm `M3_Markers`.

## Quy ước level
- Prefab `Assets/_Game/Level/Level_01.prefab`, nhóm `Area_P1_Street`, `Area_P2_Warehouse`, `Area_P3_Rooftop`; scene làm việc `Assets/_Game/Scenes/Levels/Level_01_Blockout.unity`.
- Shot `S1..Sn` theo thứ tự chạy; spawn cùng số với góc giao tranh: `CamPoint_P1_S2` ↔ `EnemySpawn_P1_W2_01`, `HostageSpawn_P1_W2_01`, `PickupSpawn_P1_W2_01`.
- `EnemySpawn_*`/`HostageSpawn_*` = vị trí nấp (ở chân); con `Peek` = vị trí ló ra. Nấp thấp: hide y = −1.2 (hoặc cover ≥ 1.2 m).
- **M3 (T-621):** `PropSlot_Barrel_P<p>_W<w>_NN`, `PropSlot_Box_…`, `PropSlot_Glass_…` (scale X/Y = kích thước kính, Z hướng ra camera), `GrenadierSpawn_…` + `Peek`, `ShieldSpawn_…` + `Peek`; gom trong `Area_*/M3_Markers`. Level chỉ đặt **marker**, prefab Props do assembler đặt vào scene (T-702).
- `RailHint_P<p>_S<s>_NN` dọc đoạn di chuyển. Môi trường layer `Default`, `Geometry` Batching Static (Props KHÔNG static). Camera 1.6 m; enemy 2 m; nấp thấp 1.1 m; cửa 2.2 m; enemy cách camera 6–15 m; đoạn di chuyển 10–20 m; tránh cua > 90°.
- Khung dọc 9:16: enemy/hostage/prop cách mép ≥ 8%; tâm hai mục tiêu cách nhau ≥ 180 px (chuẩn 1080).
- Material blockout: **tắt Specular Highlights và Environment Reflections**.

## Số liệu
**Từ plan:** ray 3–4 m/s (mặc định 3.5) · nhìn trước 3–5 m · xoay ngang ≤ 60°/s · nghiêng ≤ 2° · ngẩng/cúi ≤ 10° · đổi hướng > 90° dùng Cut · Blend 0.3–0.8 s, góc phụ 0.5–0.7 s · rung cầm tay Perlin 0.3 / 0.4 · nhún 2–3 cm · dolly-in ≤ 8–10° FOV · zoom punch −5° trong 0.15 s, trả 0.4 s · shake trúng đạn 0.2 s · di chuyển 3–5 s (≤ 6), giao tranh 8–15 s, nghỉ 0.3 s · vòng target 2.5 s · Pistol 6 · Shotgun 6 · MG 30 · 3 mạng · Revive 10 s, quảng cáo giả 3 s, 1 lần/lượt, hồi 3 tim · combo x1…x5 · 2–5 enemy/đợt · Jev offline: confidence 0.6, reticle_time 2.0/2.5/3.0 · Canvas 1080×1920 dọc · 60 FPS.
**Props (plan mục 14):** thùng nổ 1 phát, bán kính 3 m · kính 6–10 mảnh, vỡ 1 lần · mảnh vỡ 4 s · ≤ 40 Rigidbody hoạt động · ngoài khung thì ngủ · bắn vật thể: tốn 1 đạn, giữ combo, không tính trượt (đếm riêng) · Shotgun đẩy mạnh hơn.

**Đang dùng trong SO (đã chốt cho demo):** bán kính trúng Pistol 90 px / Shotgun 180 px ≤ 5 mục tiêu · Justice 35 px · reload 0.5 s · MG 10 phát/s · hạ enemy 100 + thưởng sớm ≤ 100 · Justice 300, ×1.5 nếu vòng còn xanh · combo nhân điểm bật, Shotgun không n² · màu vòng xanh < 0.4 ≤ vàng < 0.75 ≤ đỏ · ló ra 0.3 s · spawn so le 0.4–1.0 s · nấp 0.8 s · bất tử 1.5 s sau khi trúng · ân hạn 1.0 s sau revive · fade Phase 0.4 / tiêu đề 1.0 / 0.4 s · `maxMoveSeconds` 5.4 s · blend tối thiểu = góc/40.
**Đồ họa (Mobile_RPAsset):** MSAA 2×, renderScale 1.0, HDR tắt.

**M3 — mặc định PM đề xuất (chờ chủ dự án xác nhận Q1–Q4):**
- Nổ: chỉ hạ enemy đang ló trong 3 m · con tin trong 3 m → mất 1 mạng/vụ nổ · điểm 100/enemy × combo hiện tại, combo không đổi vì nổ · nổ dây chuyền trễ 0.15 s · lực nổ 800 (up 1.0) · lực hộp 4 × ImpulseScale · rung nổ 0.6 × shake trúng đạn, 0.25 s.
- Lựu đạn: grenadier ném khi vòng target thu hết (thay vì bắn) · bay 1.5 s, cung ~1.5 m, đáp cách camera ~1.5 m · vòng 0.6× màu cam · trúng = mất 1 mạng (Explosion) · bắn rơi 50 điểm, combo tăng · không nhặt/ném lại · từ Phase 2 (P2 ×2, P3 ×3).
- Human shield: tap đầu ≤ 45 px → Kill; chấm tay 35 px → Justice; thân con tin → HostageHit (mất mạng) · P2 ×1, P3 ×1–2.
- Rank (cả màn): kỹ năng = 0.45·acc + 0.30·phản xạ + 0.25·an toàn − 0.15·con tin · S ≥ 0.85 + không mất mạng + không revive · A ≥ 0.70 · B ≥ 0.50 · C còn lại · revive → tối đa B · weakness = thành phần thấp nhất (< 0.8, ngược lại None).
- Jev `wave_preset`: kỹ năng ≥ 0.75 intense · ≤ 0.40 calm · không trúng con tin + ≥ 0.60 → hostage_heavy (từ P2) · còn lại standard. `weapon_drop`: ≤ 0.40 machinegun · ≤ 0.70 shotgun · cao hơn none · chỉ đợt có PickupSpawn.

## Quyết định chủ dự án (đã chốt)
- Dùng **TextMeshPro** (font `ClaudeCop UI SDF`).
- Không commit `Assets/Screenshots/`, `ProjectSettings/Packages/com.unity.probuilder/`, `ProjectSettings/SceneTemplateSettings.json`.
- **Màn hình dọc** (Canvas 1080×1920), **offline hoàn toàn** (Jev = luật viết sẵn).
- 2026-10-04: chấp nhận các màn hiện tại; **tiếp tục M3**.

## Câu hỏi mở M3 — **ĐÃ CHỐT (2026-10-04, mặc định PM + chủ dự án)**
| # | Câu hỏi | Quyết định |
|---|---|---|
| Q1 | Mở băng Core thêm C21 `BlastEvents` + C22 `BlastReport`; luật thùng nổ | ✅ Mở; chỉ hạ enemy đang ló trong 3 m; con tin trong 3 m → mất 1 mạng; 100 đ/enemy × combo, không đổi combo |
| Q2 | Lựu đạn: bắn rơi hay nhặt/ném lại; khi nào ném | ✅ Chỉ bắn rơi; grenadier ném khi vòng target hết; bay 1.5 s; trúng mất 1 mạng; 50 đ |
| Q3 | Human shield: cách hạ | ✅ Tap đầu (45 px) hoặc chấm tay (Justice); trúng thân con tin = mất mạng |
| Q4 | Rank dựa trên gì; Jev có được đổi preset/vũ khí của đợt | ✅ Theo kỹ năng (độ chính xác, phản xạ, mất mạng, con tin), không theo điểm; revive → tối đa B; Jev được đổi preset + thùng vũ khí (khi confidence ≥ 0.6) |

## Backlog M4 (dời từ M3)
- Props còn lại: `FoliageProp` (cây/bụi), `ShootableDoor`, `LampProp`, `HangingSign`; **đồ ẩn trong hộp** (điểm/tim/vũ khí, Jev `weapon_drop` vào hộp); "tâng lon"; enemy nấp sau kính/cửa chưa có vòng target cho tới khi lộ.
- Pool tổng quát cho enemy/hostage/pickup/FX (dùng lại `PropPool`).
- FX hạt đẹp hơn (tia lửa, mảnh vụn, vết đạn theo chất liệu).
- Trả thư mục cho combat-coder / enemy-coder; boss, âm thanh, rung, slow-motion, quảng cáo thật, Pause menu (xem lại F-206).
- F-210 phần CinemachineCamera serialize trong scene (nếu cần chỉnh tay).
- Kiểm trên Android thật + profile hiệu năng (có thể làm sớm hơn nếu chủ dự án có máy — TEST-W7 có mục tuỳ chọn).
- **Đã bỏ:** Jev online, Proxy/Direct client, server, API key.
