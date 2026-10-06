# Level 3 — Tầng lửng và văn phòng (gameplay-coder, 2026-10-05)

Trạng thái: **DONE (dựng + ghép + validate)**; **chưa kiểm chứng bằng chơi thật** (Play mode không chạy được vì cửa sổ Unity không ở foreground, Editor kẹt "reloading" rồi đã thoát Play mode).
Đã nạp và dùng skill `unity-mcp-skill` (kiểm tra trạng thái Editor/compile trước mỗi đợt, `refresh_unity` wait_for_ready, `read_console` chỉ lỗi, test chỉ lấy test lỗi).

## Lỗi chuỗi "bot không bắn ở L2_Wave_P1_W2" (2026-10-06)
- **Giả thuyết Level_03 chắn tia: SAI (đo số).** Trong Level_Chain, raycast từ L2_Shot_P1_S2/S3 tới mọi mục tiêu L2 P1 W2/W3: 0 va chạm; collider Level_03 gần camera L2 P1 nhất là `Office_Floor` cách **107 m** (L3 nằm sau tường sau sảnh L2, z local ≥ 113). Không render chồng: L3 chỉ ở sau tường sau sảnh/tầng lửng; phải bật sẵn vì cuối L2 camera đã nhìn thấy cầu thang/hành lang L3. → Không thêm bật/tắt Level_03.
- **Nguyên nhân thật (đo trong Play):** harness gọi `GameCommands.RequestContinue()` ngay khi có `StageResultsPauseReason`, TRƯỚC khi `StageResultPresenter` hiện bảng (`showDelay`, WaitForSecondsRealtime). Bảng `StageResultPanel` hiện ra SAU khi đã chạy tiếp và không bao giờ ẩn (chỉ ẩn qua nút CONTINUE của chính nó) → `TapShooter.PointerBlocker` (UI) chặn mọi tap. Đo lúc GameOver: `StageResultPanel active=True`, `blockerCenter=True`, 0 RESOLVED sau Continue. Chơi tay (bấm nút) không bị; Level_02 riêng không có bảng nên không bị.
- **Sửa:** harness bấm Continue qua presenter (đợi `StageResultPanel` hiện rồi gọi `OnContinue`, như người bấm nút). **Yêu cầu ui-coder** (ngoài phạm vi của tôi): `StageResultPresenter` nghe `GameCommands.ContinueRequested` → dừng `routine` + `view.Hide()`, để Continue từ nguồn khác (bot, phím tắt, quảng cáo…) không để lại bảng chặn tap.

- **Kiểm chứng (Play, bot DebugM2Bot Kill, Level_Chain từ đầu; harness bấm Continue chỉ khi `StageResultPresenter.IsVisible`, log `scratchpad/chainbot3.log`):** L1 Win (bảng kết quả t=58.3, điểm 17 636, mạng 3/3) → L2 Win (t=119.7, tổng 49 779 → L2 = 32 143, mạng 3/3; L2_Wave_P1_W2 Cleared t=62.4) → L3 **Win** (STATE Win t=174.9, tổng 70 567 → L3 = 20 788, mạng 3/3). L3: 15 kill (5 Justice), 12 đợt Cleared, `GagFall` done, `L3_DoorOpener_Elevator` + `L3_DoorOpener_Vault` opened; không exception. Đã thoát Play mode.
- Sau đó: Validate Level_Chain PASS 0/0; test EditMode 284/287 (fail có sẵn `BuildSettings_OnlyTitleThenLevel01`, 2 skip). Không đổi code/scene ở lượt này (chỉ harness tạm trong Play mode).
- Chưa kiểm: bắt đầu từ Level 3 ở Title (SelectedLevel=2) bằng bot.

## Tiến độ khi dừng (lệnh DỪNG của người dùng, 2026-10-05)
- **Đã xong:** builder + assembler L3, prefab, scene Level_03, ghép L3 vào Level_Chain, Title 3 nút, validator PASS (L1, L2, L3, Chain), test EditMode (1 fail có sẵn), tài liệu quy tắc, báo cáo này.
- **Đang dở:** không có lệnh nào đang chạy. Lần chơi thử dừng giữa chừng (Editor kẹt "reloading" vì không ở foreground) và đã thoát Play mode.
- **Đã lưu:** `EditorSceneManager.SaveOpenScenes()` + `AssetDatabase.SaveAssets()` → scene đang mở `Level_03.unity` không còn thay đổi chưa lưu. Các scene/prefab sau đều đã lưu ra đĩa: `Level/Level_03.prefab`, `Scenes/Gameplay/Level_03.unity`, `Scenes/Gameplay/Level_Chain.unity`, `Scenes/Title.unity`, preset `EnemyPreset_L3*.asset`, `Settings/LevelValidationRules.asset`. Không ở Play mode; console 0 lỗi compile (chỉ còn lỗi "NoSubscription" của Unity AI, ngoài phạm vi).
- **Việc còn lại (theo thứ tự):**
  1. Chơi thử Level_03.unity khi Unity ở foreground: thời điểm Drop/Door/Vault/Slide, GagFall chạy đúng lúc wave H01, cửa thang máy mở khi H01 xong, cửa hầm mở sau tên cuối, Justice shot.
  2. Chơi thử Level_Chain: bảng kết quả sau L2 → Continue sang L3 liền mạch, rank riêng của L3, WinPanel khi hết L3; chọn Level 3 ở Title (SelectedLevel=2, cửa L1 + cửa hầm mở sẵn đúng).
  3. Chỉnh theo kết quả chơi (độ cao Drop, delay GagFall, cảnh camera đi chéo lên cầu thang).
  4. Reviewer/PM: cập nhật test `BuildSettings_OnlyTitleThenLevel01` theo chuỗi level; quyết định có thêm Level_03.unity vào Build Settings không (hiện chưa thêm).
  5. Tùy chọn: âm "ding" thang máy, tư thế ngồi/trói cho con tin.

## File
Mới:
- `Assets/_Game/Scripts/Game/Editor/Level03Builder.cs` — menu `ClaudeCop/Game/Build Level_03 Prefab` → `Assets/_Game/Level/Level_03.prefab`.
- `Assets/_Game/Scripts/Game/Editor/Level03Assembler.cs` — menu `ClaudeCop/Game/Assemble Level_03` → `Assets/_Game/Scenes/Gameplay/Level_03.unity` (bản sao Level_01.unity, bỏ Level_01 + DoorOpener_Main). Tạo preset `EnemyPreset_L3` (Justice mọi enemy của đợt), `EnemyPreset_L3_Plain` (không Justice), `EnemyPreset_L3_Rush` (hide 0, stagger 0–0.25).
- `Assets/_Game/Scripts/Enemy/GagFall.cs` — cảnh hài H01 (không phải mục tiêu, không collider, không tính kill, không bắn).
Sửa:
- `LevelAssembler.cs`: `LevelSpec.linkFirst` (ray P1_S1 cũng là ray nối, nhìn theo tiếp tuyến, linkSpeed); nối `Gag_P*_W*_NN` vào `EncounterWave.gags`.
- `LevelChainAssembler.cs`: menu đổi thành `Assemble Level Chain (1-3)`; thêm Level 3 (Phase 7–9, tiền tố `L3_`), Phase 6 `showResultsAfter = true` (L3 là level cuối chuỗi → WinPanel). Gỡ khỏi L2 trong chuỗi: `Wall_Back`, `Mezzanine_Backwall`, `Stairs_Rail_In_E`. `AddLevelSelectToTitle` đặt `levelCount = 3` (đã chạy, Title.unity cập nhật).
- `ChainStartSetup.cs`: thêm `laterDoors[]` (phần tử k mở sẵn khi `SelectedLevel >= k+2`); chuỗi: [null (L2 không có cửa), L3_DoorOpener_Vault].
- `EnemyActor.cs`: overload `SetEntryStyle(style, dropHeight)`; Drop dùng độ cao riêng nếu > 0.
- `SpawnPointEntry.cs`: field `dropHeight`. `EncounterWave.cs`: list `gags`, `Begin()` gọi `GagFall.Play()`; truyền `dropHeight`.
- `LevelValidator.cs` (sửa tối thiểu cho chuỗi): bỏ tiền tố `L\d+_` khi so tên kính bỏ qua/Prop–PropSlot; xét trùng tên theo root level. Không cần nới gì cho EntryStyle (validator không có mục kiểm vật che).
- `Assets/_Game/Settings/LevelValidationRules.asset`: thêm rule `Level_03` (con tin được phép; cấm thùng nổ/thùng súng/khiên/grenadier; ≤4/wave, ≤20 tổng, maxConcurrent ≤3).
- `Docs/Team/LevelBuildingRules.md` mục 1 (EntryStyle riêng không cần vật che ≥1/2 người, GagFall) và mục 4 (level cùng hệ tọa độ, linkFirst, Approach_Standalone).
- Scene: `Level_03.unity` (mới), `Level_Chain.unity` (dựng lại), `Title.unity` (LevelSelect 3 nút). Không đụng Level_01/02.unity và prefab L1/L2.

## Thiết kế dựng
Level_03 dùng **cùng hệ tọa độ local với Level_02**; trong chuỗi đặt cùng transform với Level_02. Sàn văn phòng F = 4.4 (mặt tầng lửng L2).
- P1_S1 (ray nối, 37.4 m, 5.9 s): bắt đầu đúng `CamPoint_P3_S6` của L2 (đo trong chuỗi: lệch vị trí 0.000 m, hướng tiếp tuyến 97.0° = yaw góc cuối L2), cong mềm sang cầu thang Đông, lên cầu thang (mắt camera cách bậc ≥ 1.45 m), qua cửa ở tường sau sảnh vào hành lang.
- **P1 hành lang** (6 × 34.5 m, 2 dãy cửa, máy lọc nước, tủ hồ sơ): S2 W1 1 enemy Door; S3 W2 1 enemy Door + Justice; S5 W3a Vault qua tủ thấp + Justice; S6 W3b Door + Justice (kill-zoom FOV 32).
- **P2 phòng họp kính** (tường kính, bàn họp, gác thấp cao 3 m có lan can kính, văn phòng kính dưới gác): S2 W1 1 enemy Door sau cửa kính bắn vỡ được (`PropSlot_Glass_P2_W2_01`) + 2 con tin ở bàn; S3 W2 = **H01**: 2 enemy `Drop` (dropHeight 3 m từ mép gác) + `Gag_P2_W3_01` (nhảy, vướng lan can, chúi mặt nằm bất động, súng văng); S4 giảm tải: thang máy mở cửa trượt (`DoorOpener_Elevator`, trigger W2 H01 Cleared) — chưa có âm "ding" (không có hệ thống âm thanh); S5 W3a chui lên sau tủ hồ sơ cao (Auto, che ≥ 1/2); S6 W3b Vault qua tủ thấp.
- **P3 phòng an ninh** (tường màn hình, bàn điều khiển, tủ server): S2 W1a 1 enemy Justice + 1 con tin trói ghế; S3 W1b 1 enemy Slide + Justice; S5 W2 3 enemy Rush (maxConcurrent 2); S6 W3 tên cuối trước màn hình, kill-zoom → `DoorOpener_Vault` (trigger Wave_P3_W6 Cleared, xoay -100°) mở cửa hầm, sau cửa có chiếu nghỉ + bậc xuống (chỗ nối L4).
- Tổng 15 enemy + 1 tên tự ngã (không tính) + 3 con tin. Kiểu xuất hiện: Door 4, Drop 2, Vault 2, Slide 1, Auto 6.
- GDD có 3 wave/phase, assembler cố định 4 góc Combat/phase → W3 của P1/P2 tách 2 góc (S5, S6), W1 của P3 tách S2/S3.

## Kiểm tra (đo bằng số)
- Validate: Level_03 **PASS 0 FAIL 0 WARN**; Level_Chain **PASS 0/0** (trước khi sửa validator: 2 FAIL giả do tiền tố L2_/L3_ — kính bị coi là vật chặn, trùng tên marker giữa các level); Level_01 PASS, Level_02 PASS (chạy khi scene mở riêng; mở additive cùng Level_Chain thì L2 báo chặn bởi Neighbor_North do 2 scene chung physics — lỗi cách chạy, không phải level).
- Khoảng cách ray tới renderer (bỏ sàn/bậc): min 1.05 m (P2_S1 qua cửa hành lang), 1.35 m (P1_S1 lanh tô, P3_S1 lanh tô). Độ cong rời rạc: P1_S1 5.1°/m, P2_S1 2.3°/m, P3_S1 5.3°/m (ray nối có maxYawRate 70°/s → giới hạn thật ~9°/m ở 7.6 m/s; hơi trên mốc 5°/m của quy tắc).
- GagFall (Step thủ công trong Edit mode): t=3 s thân nằm sấp tại sàn (pitch 90, feet z 169.2), súng ở (12.3, 167.0), IsDone = true. Wave_P2_W3.gags = 1.
- Test EditMode: 287, pass 284, skip 2, **fail 1 có sẵn**: `W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01` (Build Settings đã có Level_02 + Level_Chain từ trước; tôi không thêm scene build nào). Test thuộc thư mục reviewer — không sửa.
- Console: 0 lỗi trong thư mục của tôi (chỉ lỗi "NoSubscription" của Unity AI generators, ngoài phạm vi).

## Chưa kiểm chứng / tồn đọng
- Chưa chơi thật: thời điểm Drop/Door/Vault/Slide, GagFall chạy đúng lúc wave, cửa thang máy/cửa hầm mở, bảng kết quả sau L2 → Continue vào L3, rank riêng L3, chọn Level 3 ở Title (`SelectedLevel=2`).
- Chuông thang máy chỉ có hình (cửa trượt + đèn), chưa có âm thanh.
- Con tin "ngồi" vẫn là tư thế đứng của prefab Hostage (chưa có tư thế ngồi/trói) — yêu cầu enemy-coder/art nếu cần.
- Camera P1_S1 lúc ở z 92–96 cao hơn sàn bên cạnh cầu thang (đi chéo lên cầu thang) — ổn theo số đo, cần nhìn thật.
- Test BuildSettings cần reviewer/PM cập nhật theo chuỗi level.
