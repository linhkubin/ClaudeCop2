# L5-BUILD — Level 5 "Kho tiền" (Hard, 5 phase) (level-designer, 2026-10-06)

**Trạng thái: DONE** (booster giáp / hộp máu chưa có runtime, xem "Tồn đọng"). Không commit.

## Việc phụ (làm trước): rule riêng cho chuỗi
- `LevelValidationRules.cs`: thêm `LevelRule.maxEnemiesPerLevel` (> 0 = scene chuỗi) + `LevelKeyOf()`, `PerLevelLimit()`, `ChainTotalLimit()`.
- `LevelValidator.cs` mục (g): rule có `maxEnemiesPerLevel` thì đếm enemy theo level con (root `Level_XX` của điểm spawn, hoặc tiền tố wave `L5_`). Tổng ≤ `maxEnemiesPerLevel × số level`; mỗi level ≤ rule riêng của nó (Level_01/02/03/05), level không có rule riêng (Level_04) ≤ `maxEnemiesPerLevel`.
- `LevelValidationRules.asset`: thêm rule `Level_05` (khiên người, thùng nổ, thùng súng được phép; cấm grenadier; 4/wave, tổng 32, đồng thời 3) và `Level_Chain` (`maxEnemiesPerLevel 32`, 4/wave, đồng thời 3). Luật cũ vẫn giữ.
- Test mới trong `LevelValidatorTests`: `Chain_LevelKey_FromRootOrWavePrefix`, `Chain_Limits_ScaleWithLevelCount_AndUseOwnLevelRule`.

## File
- Mới: `Assets/_Game/Scripts/Game/Editor/Level05Builder.cs` (menu `ClaudeCop/Game/Build Level_05 Prefab`), `Level05Assembler.cs` (menu `ClaudeCop/Game/Assemble Level_05`).
- Sinh ra: `Assets/_Game/Level/Level_05.prefab`, `Assets/_Game/Scenes/Gameplay/Level_05.unity` (bản sao Level_01.unity như L3/L4), vật liệu `Assets/_Game/Level/Materials/M_Blockout_{Steel,Gold,LampWhite}.mat`, preset `Assets/_Game/Prefabs/Enemies/Data/EnemyPreset_{L5,L5_Plain,L5_Rush}.asset` (vòng target 1.6 s).
- Sửa `LevelAssembler.cs`: `LevelSpec.phaseCount` (mặc định 3); ShieldSpawn_/GrenadierSpawn_ nối trong `WireWave` cho **mọi Phase, kể cả khi append** (trước đây bỏ P1 và bỏ hẳn trong chuỗi); `frameTargets` gồm cả khiên người/grenadier. Level 1–4 không có marker này nên không đổi.
- Sửa `LevelChainAssembler.cs`: menu `Assemble Level Chain (1-5)`, `AppendLevel5` (gỡ `VaultGate_Dark` của L4 vì chắn ray), `showResultsAfter` ở Phase 3/6/9/12, `ChainStartSetup.laterDoors` = [null, L3 cửa hầm, L4 cửa kho tiền, L5 tường thoát hiểm], level select 5 nút 184×130, cách 20.
- Dựng lại: `Level_Chain.unity`, `Title.unity` (LevelSelect 5 nút).
- Không thêm Level_05 vào Build Settings (L3/L4 cũng không có).

## Bố cục (toạ độ local chung với Level_02/03/04; sàn B = -3.0)
```
                         P5 lõi kho vàng x181..201 z308..344 (trần B+6) [tường Bắc trượt mở x~191 -> Level 6]
                         bậc thỏi vàng 2 bên, cột thép
                         ^ lỗ x~189.9 (rẽ Bắc, 4.2 độ/m)
 P3 đại sảnh vault x129.6..161.6 z267.8..301.8 (trần B+8)  ->  P4 phòng đếm tiền x161.6..193.6 z288..308 (nhìn Đông)
 cửa vault khổng lồ (đóng) giữa tường Bắc, 2 bục cao     lỗ z~297.6   bàn đếm tiền dài, cửa sổ quan sát
 ^ lỗ x~145.7 (dưới sàn lưới)
 P2 phòng két ký gửi x133.6..157.6 z237.8..267.8 (trần B+7), sàn lưới thép B+3 ở z259.8..267.8, cầu thang sắt tường Tây
 ^ lỗ x~145.6
 P1 hành lang an ninh 5 m x143.1..148.1 z196.3..237.8 (trần B+3.5): 4 cửa chớp hai bên, cửa trần, laser trang trí trên tường
 ^ cửa kho tiền Level 4 (z196, ray qua x=145.1, khung 143.0..147.4)
```
- Ray P1_S1 bắt đầu đúng `CamPoint_P3_S6` của Level_04.prefab (133.7, B+1.65, 181.2, yaw 50), cong mềm qua cửa kho tiền (33.1 m, rẽ max 2.7°/m). Group `Approach_Standalone` (sàn + tường Bắc kho L4 có lỗ cửa) chỉ để chơi riêng, chuỗi gỡ.

## Spawn (30 enemy gồm 6 khiên người, 5 con tin riêng + 6 con tin trong khiên, 2 thùng nổ, 1 thùng shotgun)
| Phase/shot | Nội dung | Kiểu xuất hiện |
|---|---|---|
| P1 S2 | 1 enemy | Door (cửa chớp Tây) |
| P1 S3 | **khiên người đầu tiên** (`ShieldSpawn_P1_W3_01`) | Door (Đông) |
| P1 S5 | 2 enemy lần lượt + 1 nhân viên | Door + Door |
| P1 S6 | 1 enemy nhảy từ cửa trần (dropHeight 3.6) | Drop |
| P2 S2 | 2 enemy tầng thấp + `PickupSpawn_P2_W2_01` (Shotgun, không gợi ý) | Auto + Vault (tủ két) |
| P2 S3 | 1 enemy tầng cao (sàn lưới B+3) + 1 khiên người | Auto |
| P2 S5 | 3 dồn dập (L5_Rush, tối đa 2) + con tin xen, góc Tây Bắc (lệch khỏi lối ra) | Auto |
| P2 S6 | 1 enemy ra từ cửa dưới sàn lưới | Door |
| P3 S2 | **khiên đôi** (2 khiên người, nhân viên) | Auto |
| P3 S3 | 1 enemy nhảy lên bục cao 1.2 m (Justice) | Vault |
| P3 S5 | 2 enemy + thùng nổ giữa + con tin xa (4.2 m tới thùng) | Auto |
| P3 S6 | 1 enemy | Auto |
| P4 S2 | 1 khiên người; marker `BoosterHint_Armor_P4`, `BoosterHint_Health_P4` | Auto |
| P4 S3 | 2 enemy (Justice) + thùng nổ | Auto |
| P4 S5 | 1 enemy (Justice) + kế toán bị trói | Auto |
| P4 S6 | 1 enemy | Auto |
| P5 S2 | 3 dồn dập (L5_Rush, tối đa 2) | Auto |
| P5 S3 | 1 enemy sau chồng thỏi vàng | Vault |
| P5 S5 | 1 enemy + 1 con tin | Auto |
| P5 S6 | **đội trưởng có khiên người**, kill-zoom FOV 32 → `L5_/DoorOpener_EscapeWall` trượt mở | Auto |

Mỗi level một rank riêng: Phase 12 (hết Level 4) có `showResultsAfter`; Level 5 là level cuối chuỗi (thắng → WinPanel).

## Kiểm tra (bằng số)
- **Validate Level_05: PASS (0 FAIL, 0 WARN)** — 30 shot, 20 wave, min 12.6 m, |h| max 5.4°, FOV cần 36.0/43.2, 0/36 tia chặn, Move max 5.4 s, yaw rộng max 92, 30 enemy (≤32), 2/2 PropSlot.
- **Validate Level_Chain: PASS (0 FAIL, 0 WARN)** — 98 shot, 65 wave, 0/120 tia chặn, Move max 5.9 s, 104 enemy ≤ 160 (32×5 level); theo level: L1 15/24, L2 23/28, L3 20/20, L4 16/32, L5 30/32; 13/13 PropSlot.
- Đo riêng của builder (log `[Level05Builder]`): tia nhìn qua **mọi renderer kể cả decor không collider** (MeshCollider tạm), con tin đóng băng wave trước và thùng nổ: **0/105 bị chặn**. Ray P1_S1 đo cùng hình khối Level 4 (cửa kho đã mở, bỏ VaultGate_Dark như chuỗi). Ray cách vật thể ≥ 0.75 m (khung cửa dưới sàn lưới P2→P3), cách vật nấp/thùng ≥ 2.8 m, cách con tin đóng băng ≥ 1.2 m; rẽ max 4.2°/m (P3→P4, P4→P5). Thùng–con tin min 4.2 m, thùng–enemy min 1.2 m. Không điểm nào chui vào vật thể (capsule 0.35), đường chạy vào 0/29 cắt tường.
- EditMode toàn bộ: 286/289 pass, 2 skip, 1 fail **có từ trước** (`W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01`, Build Settings đã có 4 scene từ trước). Test validator mới pass.
- Console: không lỗi (chỉ "NoSubscription" của Unity AI generators).
- Ảnh: `Assets/Screenshots/L5_P2_S3.png`, `L5_P3_S2.png` (marker spawn không hiện trong ảnh edit mode).

## Tồn đọng / cần agent khác
1. **Booster giáp + hộp hồi máu (P4)**: chưa có code (vật phẩm giáp, API chặn 1 lần trúng trong PlayerHealth, hộp máu). Chỉ đặt marker gợi ý `Area_P4_CountingRoom/Hints/BoosterHint_*` — cần gameplay-coder/combat-coder.
2. "Cửa kho đóng sầm" (Move P2→P3), "chuông cửa vault" (giảm tải P3), "tiền sáng" cuối P5: chỉ là bối cảnh (cửa vault khổng lồ P3 đóng sẵn, tường thoát P5 trượt mở). Muốn có hiệu ứng thì cần code/FX.
3. Nhãn "LEVEL 5" trên nút 184 px với cỡ chữ 54 có thể xuống dòng (LevelSelectPresenter thuộc ui-coder; nên bật autoSize hoặc giảm cỡ chữ).
4. `LevelBuildingRules.md` (PM) nên ghi: rule chuỗi `maxEnemiesPerLevel`; |h| ≤ 5.5° tính cả thùng súng (PickupSpawn) nên đặt thùng gần cụm enemy; cột Cylinder dẹt cần MeshCollider (CapsuleCollider thành cầu).
5. Chưa chơi thử Play mode (không được giao lượt Play); hành vi khiên người chạy vào kiểu Auto/Door (kéo theo con tin) chưa kiểm chứng bằng mắt.
