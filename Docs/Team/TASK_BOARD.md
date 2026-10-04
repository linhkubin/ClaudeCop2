# Task Board — ClaudeCop2

> Do **project-manager** duy trì. Trạng thái: TODO · IN PROGRESS · IN REVIEW · DONE · BLOCKED
> Kế hoạch hiện hành: **DEMO M1 + M2** (lập 2026-10-04), nguồn: `Docs/Design/Plan_VirtuaCop2_Mobile.md`.
> **Đội gọn:** gameplay-coder (Core, Combat, FX, Enemy, Camera, Game, Jev Offline, ghép scene, tài nguyên dùng chung) · ui-coder (UI/Ads) · level-designer · reviewer (1 lần/wave). combat-coder, enemy-coder, jev-coder nghỉ đến M3/M4.

## Quy trình làm việc
> Luật chung (phạm vi sở hữu, asmdef, scene, git): xem [Conventions.md](Conventions.md)
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

## Lộ trình
| Wave | gameplay-coder (tuần tự theo số) | ui-coder | level-designer | reviewer | Mốc |
|---|---|---|---|---|---|
| W1 | T-101 setup (**chạy một mình trước**) → T-102 Core → T-103 Combat M1 ∥ T-104 Enemy M1 | T-111 View/prefab M1 + Ads | T-121 Phase 1 | — | Core có sẵn |
| W2 | T-201 Camera rail ∥ T-202 Game M1 → **T-203 ghép M1** | T-211 Presenter M1 + vòng target | T-221 Phase 2 | R-W1 | **M1 chơi được** |
| W3 | T-301 Combat M2 · T-302 Enemy M2 · T-303 Camera M2 · T-304 Game M2 + Title · T-305 Jev nền | T-311 Revive/Ad/Phase fade · T-312 chữ bay/combo/vũ khí/Title UI | T-321 Phase 3 + SurfaceMaterialTag | R-W2 (gồm ghép M1) | Demo M1 cho chủ dự án |
| W4 | F-xxx (sửa) · T-401 FX · T-402 JevDirector → **T-403 ghép M2** | T-411 Bảng debug Jev · F-xxx | F-xxx | R-W3 (đầu wave) · **R-W4** (sau T-403) | **M2 xong** |

`→` = phải xong trước; `∥` = chạy song song được (Liaison có thể chạy 2 phiên gameplay-coder cùng lúc nếu thư mục khác nhau).

## Bảng task
| ID | Wave | Owner | Mục tiêu | Phụ thuộc | Trạng thái |
|---|---|---|---|---|---|
| T-101 | W1 | gameplay-coder | Khung `Assets/_Game/`, Android/landscape/URP mobile, tag/layer, Input Actions | — | TODO |
| T-102 | W1 | gameplay-coder | `ClaudeCop.Core`: hợp đồng C1–C20 | T-101 | TODO |
| T-103 | W1 | gameplay-coder | `ClaudeCop.Combat` M1: WeaponData ×3, TapShooter (Pistol), đạn/reload, CombatSystems prefab | T-102 | TODO |
| T-104 | W1 | gameplay-coder | `ClaudeCop.Enemy` M1: Enemy state machine, EncounterWave, Enemy prefab | T-102 | TODO |
| T-111 | W1 | ui-coder | asmdef UI, View + prefab HUD/vòng target/nháy đỏ/Win/GameOver, ring sprite, `IRewardedAd`/`FakeRewardedAd` | T-101 | TODO |
| T-121 | W1 | level-designer | Blockout Phase 1 (Street) → `Level_01.prefab` | T-101 | TODO |
| R-W1 | W2 | reviewer | Review gộp T-101…T-121 | W1 | TODO |
| T-201 | W2 | gameplay-coder | `ClaudeCop.Camera` M1: PhaseDirector, CameraShot, ray spline, Cut/Blend/Spline, CameraFeelProfile + Applier cơ bản, shake | T-102 | TODO |
| T-202 | W2 | gameplay-coder | `ClaudeCop.Game` M1: GameConfig, GameManager, PlayerHealth, GameSystems prefab | T-102..T-104 | TODO |
| T-211 | W2 | ui-coder | Presenter bind Core cho HUD, `TargetReticleUI`, nháy đỏ, Win/GameOver; EventSystem | T-102, T-111 | TODO |
| T-221 | W2 | level-designer | Blockout Phase 2 (Warehouse) | T-121 | TODO |
| T-203 | W2 | gameplay-coder | **Ghép M1**: `Scenes/Gameplay/Level_01.unity` (Phase 1) chơi được | T-121, T-201, T-202, T-211 | TODO |
| R-W2 | W3 | reviewer | Review gộp W2 (gồm Play mode M1) | W2 | TODO |
| T-301 | W3 | gameplay-coder | Combat M2: ComboSystem, Justice, Shotgun/MG, WeaponPickup prefab | T-103 | TODO |
| T-302 | W3 | gameplay-coder | Enemy M2: Hostage, Justice point + đầu hàng, preset/tactic, API cấu hình đợt | T-104 | TODO |
| T-303 | W3 | gameplay-coder | Camera M2: SlowZoom, zoom punch, giới hạn xoay, Giảm chuyển động, chuyển Phase | T-201 | TODO |
| T-304 | W3 | gameplay-coder | Game M2: Revive flow, `Scenes/Title.unity`, luồng Title → Level_01 | T-202 | TODO |
| T-305 | W3 | gameplay-coder | Jev nền: JevTypes, IJevClient, OfflineJevClient, JevConfig, PlayerStatsTracker, JevDecisionLog | T-102 | TODO |
| T-311 | W3 | ui-coder | RevivePopup + quảng cáo giả, fade + tiêu đề Phase | T-211 | TODO |
| T-312 | W3 | ui-coder | Chữ điểm bay, combo, HUD vũ khí, Title screen prefab + công tắc Giảm chuyển động | T-211 | TODO |
| T-321 | W3 | level-designer | Blockout Phase 3 (Rooftop) + `SurfaceMaterialTag` P1–P3 + chỉnh khung hình theo M1 | T-221, T-102 | TODO |
| R-W3 | W4 | reviewer | Review gộp W3 | W3 | TODO |
| T-401 | W4 | gameplay-coder | FX: tia lửa, vết đạn theo SurfaceMaterial, pool | T-321 | TODO |
| T-402 | W4 | gameplay-coder | JevDirector: gọi Offline khi bắt đầu Shot di chuyển, áp vào EncounterWave | T-302, T-305 | TODO |
| T-411 | W4 | ui-coder | Bảng debug Jev (bật/tắt) | T-305 | TODO |
| T-403 | W4 | gameplay-coder | **Ghép M2**: Level_01 đủ 3 Phase + Title + toàn bộ M2 | T-401, T-402, T-411, F-xxx | TODO |
| R-W4 | W4 kết | reviewer | Review + checklist kiểm tra M2 (mục "Kiểm tra" của plan, trừ Props/Jev online) | T-403 | TODO |

---

## Hợp đồng Core (T-102) — viết một lần, sau R-W1 coi như đóng băng
Assembly `ClaudeCop.Core`, namespace `ClaudeCop.Core`, không tham chiếu assembly game nào. Bus/registry tĩnh phải **tự reset khi vào Play mode** (chạy đúng cả khi tắt Domain Reload). Chỉ chủ ghi ở cột "Phát" được raise. **ui-coder chỉ bind qua các hợp đồng này** (+ `JevDecisionLog` ở W4) nên tên phải giữ nguyên như bảng.

| # | Tên | Nội dung | Phát / Cài đặt | Nghe / Dùng |
|---|---|---|---|---|
| C1 | `SurfaceMaterial` (enum) | Concrete (mặc định), Wood, Metal, Glass, Foliage, Flesh | — | FX, Props (M3) |
| C2 | `SurfaceMaterialTag` (component) | Gắn chất liệu cho collider môi trường; thiếu thì coi là Concrete | level-designer gắn | FX |
| C3 | `WeaponKind` (enum) | Pistol, Shotgun, MachineGun | — | Combat, Enemy, Jev, UI |
| C4 | `ShotInfo` (struct) | Vị trí tap (screen), điểm trúng, pháp tuyến, hướng đạn, WeaponKind, hệ số lực đẩy | Combat | IShootable, ITapTarget |
| C5 | `IShootable` | Nhận `OnShot(ShotInfo)` — vật thể môi trường | Props (M3) | TapShooter |
| C6 | `TargetKind` (enum) | Enemy, Hostage, Pickup, Grenade (dự trữ) | — | |
| C7 | `ITapTarget` | Kind · IsTargetable · AimPoint (world) · HasJusticePoint + JusticePoint (world) · ShowsReticle · ReticleProgress 0→1 · ExposedTime (s) · ổn định Id · `OnTapHit(ShotInfo, isJustice)` trả `TapOutcome` | Enemy, Hostage, WeaponPickup | TapShooter, UI vòng target |
| C8 | `TargetRegistry` (static) | Register / Unregister · danh sách chỉ đọc · event Registered(target), Unregistered(target) | Target tự đăng ký khi ló ra, hủy khi chết/ẩn/disable | TapShooter, UI |
| C9 | `TapOutcome` (enum) | Miss, Kill, JusticeKill, HostageHit, PickupCollected, Environment, Blocked | | |
| C10 | `ShotResult` (struct) | Outcome · TargetKind · điểm world · ReticleProgress lúc trúng · ReactionTime · WeaponKind · ComboMultiplier sau phát · số mục tiêu trúng | Combat | Game, Jev, UI |
| C11 | `CombatPauseSignal` (static) | Push(lý do)/Pop(lý do) có đếm · IsPaused · event Changed(bool) | Camera (blend, chuyển Phase), Game (Revive/Win/GameOver, ân hạn sau revive) | Enemy (dừng vòng, không bắn, dừng spawn), TapShooter (chặn tap) |
| C12 | `DamageSource` (enum) | EnemyShot, HostageHit, Explosion (dự trữ) | | |
| C13 | `IPlayerDamageReceiver` + `PlayerDamageService` (static) | Register/Unregister · `Damage(source, worldPos)`; không có receiver → cảnh báo, bỏ qua | PlayerHealth cài | Enemy (bắn), TapShooter (trúng con tin) |
| C14 | `GameState` (enum) | Title, Playing, RevivePrompt, Win, GameOver | | |
| C15 | `GameEvents` (static) | LivesChanged(cur,max) · PlayerDamaged(source,pos,livesLeft) · ScoreChanged(total) · ScoreAwarded(points,pos,isJustice,multiplier) · GameStateChanged(state) · ReviveAvailabilityChanged(remaining) · **Current snapshot** (state, lives, score, revives) để UI đọc khi OnEnable | Game | UI, Camera (shake), Combat (reset combo), Jev |
| C16 | `CombatEvents` (static) | ShotFired(weapon,screenPos) · ShotResolved(ShotResult) · AmmoChanged(cur,max,weapon) · WeaponChanged(weapon) · ReloadStateChanged(bool) · ComboChanged(streak,multiplier) · OutOfAmmo · snapshot hiện tại | Combat | UI, Game, Jev |
| C17 | `EncounterBase` (abstract MonoBehaviour) | Begin() · IsActive · IsCleared · event Cleared(encounter, lastKillWorldPos) · Id · mô tả ngắn (cho Jev) | EncounterWave kế thừa | CameraShot/PhaseDirector, Jev |
| C18 | `RailEvents` (static) | PhaseStarted(index,title) · PhaseTransition(title,fadeOut,hold,fadeIn) · MoveSegmentStarted(EncounterBase sắp tới hoặc null) · EncounterStarted · EncounterCleared · LevelCompleted | Camera (PhaseDirector) | Game (Win), UI (fade/tiêu đề), Jev |
| C19 | `UserSettings` (static) | ReduceMotion (lưu PlayerPrefs, key cố định) · event Changed | UI ghi | Camera đọc |
| C20 | `GameCommands` (static) | RequestReload · RequestStartGame · RequestRestart · RequestRevive · DeclineRevive · RequestDebugOverlayToggle (tuỳ) | UI phát | Combat (Reload), Game (còn lại) |

- `Time.timeScale` do **Game** điều khiển (RevivePrompt → 0, revive/kết thúc → 1); UI dùng unscaled time cho đếm ngược/quảng cáo/fade.
- `IRewardedAd` **không** ở Core — ở `Scripts/Ads/` (ui-coder), chỉ UI dùng.
- `JevDecisionLog` (Jev, T-305): event DecisionMade + bản ghi gần nhất (câu hỏi, giá trị chọn, xác suất các lựa chọn, confidence, nguồn Offline/Mặc định, thời điểm) → UI bảng debug (T-411).

## Quy ước level (T-121/T-221/T-321)
- Prefab `Assets/_Game/Level/Level_01.prefab`, nhóm `Area_P1_Street`, `Area_P2_Warehouse`, `Area_P3_Rooftop`; scene làm việc `Assets/_Game/Scenes/Levels/Level_01_Blockout.unity`.
- Mỗi Phase đánh số Shot `S1..Sn` theo thứ tự chạy (Shot di chuyển và mỗi **góc** giao tranh đều là 1 Shot). Spawn của góc nào dùng **cùng số**: `CamPoint_P1_S2` ↔ `EnemySpawn_P1_W2_01`, `HostageSpawn_P1_W2_01`, `PickupSpawn_P1_W2_01`.
- Điểm `EnemySpawn_*`/`HostageSpawn_*` = vị trí **nấp**; con rỗng tên `Peek` = vị trí **ló ra** (forward hướng về camera).
- Gợi ý ray: `RailHint_P1_S1_01, _02…` dọc đường di chuyển.
- Môi trường để layer `Default`. Camera cao 1.6 m; capsule enemy 2 m; chỗ nấp thấp 1.1 m; cửa 2.2 m; enemy cách camera 6–15 m; đoạn di chuyển 10–20 m (≈3.5 m/s × 3–5 s, tối đa 6 s); tránh cua > 90° trên ray.

## Số liệu
**Từ plan:** ray 3–4 m/s (mặc định 3.5), ease-in/out · nhìn trước 3–5 m có damping · xoay ngang ≤ 60°/s · nghiêng ≤ 2° · ngẩng/cúi ≤ 10° · đổi hướng > 90° dùng Cut · Blend 0.3–0.8 s, góc phụ EaseInOut 0.5–0.7 s · rung cầm tay Perlin biên độ 0.3, tần số 0.4 · nhún 2–3 cm · dolly-in tổng ≤ 8–10° FOV (vd. 60 → 50) · zoom punch −5° trong 0.15 s, trả trong 0.4 s · shake khi trúng đạn 0.2 s · nhịp: di chuyển 3–5 s (≤ 6), giao tranh 8–15 s, góc phụ 1–2 s, nghỉ 0.3 s · vòng target 2–3 s (mặc định 2.5) xanh → vàng → đỏ · Pistol 6 viên · Shotgun 6 viên, bán kính lớn, trúng nhiều · Súng máy 30 viên, giữ để bắn · 3 mạng · Revive đếm 10 s, quảng cáo giả 3 s, 1 lần/lượt, hồi 3 tim · combo x1…x5 · 2–5 enemy/đợt · Level01: 3 Phase × (1 di chuyển + 2 giao tranh × 1–3 góc) ≈ 6 giao tranh · Jev: timeout 1.5 s, confidence 0.6, reticle_time 2.0/2.5/3.0 · Canvas 1920×1080 · 60 FPS.

**PM đề xuất (chờ chủ dự án xác nhận, dùng làm mặc định trong SO):** bán kính trúng Pistol 90 px / Shotgun 180 px (chuẩn 1080p, cố định, không co theo vòng) · Justice 35 px · reload 0.5 s · súng máy 10 phát/s · hạ enemy 100 + thưởng sớm tối đa 100 (tuyến tính theo 1 − progress) · Justice 300, ×1.5 nếu vòng còn xanh · combo +1 mỗi phát trúng liên tiếp, tối đa x5 · màu vòng: xanh < 0.4 ≤ vàng < 0.75 ≤ đỏ · ló ra 0.3 s · spawn so le 0.4–1.0 s · enemy bắn xong nấp 0.8 s rồi ló lại với vòng mới · bất tử 0.5 s sau khi trúng đạn · ân hạn 1.0 s sau revive · fade Phase 0.4 / tiêu đề 1.0 / 0.4 s · nhặt thùng không tốn đạn · hết đạn đặc biệt → Pistol đầy băng.

## Backlog M3/M4 (chưa chia task)
- Props tương tác (PhysicsProp, FoliageProp, BreakableGlass + tool cắt mảnh, ShootableDoor, ExplosiveBarrel, LampProp, HangingSign, PropPool, ≤ 40 Rigidbody, mảnh vỡ 4 s).
- Grenade, HumanShieldEnemy (enemy_tactic grenade/human_shield thật).
- Jev: ProxyJevClient, DirectJevClient (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`), `Server/python` + `Server/node`, `.env`/`.gitignore`, rank S/A/B/C + weakness, cửa sổ Editor `JevPlaytestReview`.
- Trả thư mục cho combat-coder / enemy-coder / jev-coder; boss, âm thanh, rung, slow-motion, quảng cáo thật.
