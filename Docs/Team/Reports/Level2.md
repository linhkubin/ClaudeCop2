# Báo cáo dựng Level 2 — Sảnh giao dịch (2026-10-05)

## Sản phẩm
- Scene `Assets/_Game/Scenes/Gameplay/Level_02.unity` (nhân đôi từ Level_01.unity rồi thay prefab level; có trong Build Settings, index 2).
- Prefab blockout `Assets/_Game/Level/Level_02.prefab` do `Level02Builder` (menu `ClaudeCop/Game/Build Level_02 Prefab`) sinh từ primitive: sàn đá, mặt tiền kính + cửa xoay, bàn tiếp tân, quầy thông tin, ATM, chậu cây, cột (cột H02), 6 quầy giao dịch + cột vuông + kính + biển số, đèn báo động, khu ghế chờ, 2 cánh cầu thang đá, tầng lửng + lan can kính. Vật liệu mới `M_Blockout_Wood` (nâu gỗ), còn lại dùng bộ Blockout sẵn có.
- `Level02Assembler` (menu `ClaudeCop/Game/Assemble Level_02`) ghép Rails/Encounters/Shots/Phase (3 phase x 6 shot).
- Refactor: lõi chung `LevelAssembler` (LevelSpec + WaveDef); `Level01Assembler` chỉ còn spec + gọi lõi (hành vi giữ nguyên, API `Level01Assembler.Assemble()` giữ).

## Yêu cầu của người dùng
1. Nhiều enemy hơn: 23 enemy (P1 7, P2 7, P3 9) so với 13–15 của Level 1; mỗi wave 1–3 enemy, `maxConcurrent` 2 (wave cuối P3: 3); 3 con tin (P1 S3, P2 S5 xen giữa enemy, P3 S5 ngồi ghế). Đã cập nhật bảng storyboard + tổng trong `Docs/Design/Levels_BankHeist.md` (mỗi phase 4 wave).
2. Zoom nhẹ: shot Combat đặt FOV tối thiểu S2 40, S3 36, S5 38, S6 32 (kill-zoom cuối mỗi phase), blend 0.7 cho S3/S6. Lưu ý cơ chế: FOV thật = max(fov, FOV cần để vừa cụm mục tiêu) và bị chặn ở baseFov 45 ở màn dọc, nên các giá trị 52–60 như Level 1 không có tác dụng (luôn ra 45); FOV cần của Level 2 là 35/42 (9:16 / 9:19.5). Push-in/kill-zoom dùng cơ chế sẵn có trong CameraFeelProfile (không đổi). Rail Move có `lookKeys` (giữ hướng nhìn rồi xoay dần về shot kế); tốc độ xoay blend vẫn tính bằng công thức 40 độ/s của assembler.
3. Enemy đứng sẵn: thêm `EnemyActor.SceneStanding` (field `sceneStanding`, `SetSceneStanding(bool)`) + `EnemyBrain.StandsGround/ActivateStanding()`: không có pha ló, vào thẳng Ngắm (vòng target chạy ngay), hết vòng thì bắn rồi ngắm lại (không rút). Đứng nguyên chỗ trong scene từ lúc nạp, kích hoạt khi wave tới lượt. Hành vi cũ không đổi. Điểm đặt trong prefab: `EnemyStand_P*_W*_NN`; assembler instance prefab Enemy làm con wave, gán `sceneEnemies`. 8 enemy đứng sẵn: P1 2, P2 3, P3 3. Thêm 3 test vào `EnemyBrainTests`.

## Kiểm tra
- `Validate Level` Level_02: PASS 0 FAIL 0 WARN (23 enemy, min cách 13.4 m, FOV cần 35.1/42.2, 0/26 tia bị chặn, Move max 5.4 s). Báo cáo: `Docs/Team/Reviews/validate-Level_02.txt`. Level_01 vẫn PASS.
- Sửa công cụ: `LevelValidator` tính cả `sceneEnemies/sceneHostages` vào mục tiêu của wave; thêm luật `Level_02` (cho phép con tin, tối đa 4/wave, 28 tổng, concurrent <= 3) vào `Settings/LevelValidationRules.asset`.
- Test EditMode (Enemy + Camera + Game): 108/108 pass. Console không có lỗi compile.

## Chưa làm / lưu ý
- Hoạt cảnh H02 (con tin chạy đâm cột): chưa có code, con tin vẫn thoát như cũ. Cột `Column_Hall_H02` đã đặt sẵn ở sảnh.
- Nối vào luồng game: mới thêm vào Build Settings. `TitleLauncher.levelScene` vẫn là "Level_01" (đổi thành "Level_02" để thử); chưa có màn chọn level hay chuyển level khi thắng.
- Chưa chạy Play mode. Assembler ghép lại CameraFeelProfile.asset (reserialize, thêm field mới từ code chưa commit của người dùng).
- Mọi thứ chưa commit. Level_01 (scene/prefab/assembler spec) không bị sửa nội dung.
