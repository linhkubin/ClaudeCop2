# LEVEL-VALIDATOR (2026-10-05, chua commit)

**Trang thai:** DONE. Menu `ClaudeCop/Validate Level` / `LevelValidator.Run(string scenePath = null) : string`. Edit mode, khong sua scene. Scene mac dinh: scene dang mo co PhaseDirector, neu khong thi Level_01 (mo additive roi dong).

## File
- `Scripts/Game/Editor/LevelValidator.cs` (menu + doc scene + 10 nhom kiem tra a..j).
- `Scripts/Game/Validation/LevelGeometry.cs` (logic thuan, asmdef ClaudeCop.Game) va `LevelValidationRules.cs` (SO nguong).
- `Assets/_Game/Settings/LevelValidationRules.asset` (tu tao o lan chay dau voi gia tri mac dinh).
- `Scripts/Game/Tests/LevelValidatorTests.cs` (7 test: tuyen thang PASS, tuyen quay vong FAIL, yaw wrap/dao chieu, lech ngang/cum, FOV can, thoi gian Move).
- `CLAUDE.md` muc 5 (dong 10) cap nhat cach goi. Ket qua: `Docs/Team/Reviews/validate-<scene>.txt`.

## Quy uoc kiem tra (nguong deu trong Rules)
- (a) khoang cach shot -> nguc muc tieu (Peek + aimHeight 1.5) >= 12 m; pickup chi WARN < 8 m.
- (b) |h| <= 5.5, cum rong <= 21, FOV doc can <= 45 o 9:16 va 9:19.5 (cong thuc RequiredFov cua AutoFrame, le 5); WARN neu frameTargets thieu muc tieu.
- (c) RaycastAll tu pose shot toi tung muc tieu (bo trigger, Prop_Glass, con cua spawn).
- (d) dau/cuoi rail trung shot ke (0.05 m, 0.5 do, bo qua huong khi co lookKeys); Combat->Combat <= 30 do va <= 1 m. Cap co `entry = Cut` (dau moi Phase) duoc bo qua.
- (e) thoi gian Move theo cong thuc RailCameraDriver + dwell, nguong 5.8 s (bang profile).
- (f) tinh THEO TUNG PHASE (giua Phase la Cut): yaw rong tich luy <= 150, so lan dao chieu re (doan >= 15 do) <= 4, cat cheo doan 2D tren XZ (gop diem gan nhau < 0.5 m).
- (g) so enemy/wave, tong, maxConcurrent, marker cam theo ten level (Level_01: con tin, thung no, thung sung = PickupSpawn_, khien nguoi, grenadier). Gia tri maxEnemiesPerWave 4 / tong 24 / concurrent 3 la mac dinh toi dat, level-designer/PM chinh trong asset.
- (h) regex ten theo tien to, trung ten. (i) null/missing trong EncounterWave, shot, prefab thieu. (j) PropSlot_X vs Prop_X (0.05 m).

## Chay tren Level_01 hien tai (scene dang duoc dung lai, ket qua chi de chung minh tool chay)
FAIL 2 / WARN 1: (e) P3_S1_Move 62.2 m = 13.4 s, P2_S1_Move 6.3 s; (f) Phase 3 yaw rong -182 > 150; (h) `CamPoint_P3_S5_End` sai quy uoc. a, b, c, d, g, i, j PASS (min 12.2 m, |h| max 4.9, 0/15 tia chan). Bo test Game: 58/58 PASS. Console khong loi.

## Luu y
- Pose shot lay tho (chua AutoFrame); level hien tai de AutoFrame xoay ~0 nen khop.
- (f) neu muon tinh xuyen Phase thi doi `Cut` thanh noi lien; hien tai coi moi Phase la mot tuyen.
