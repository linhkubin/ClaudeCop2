# L4-BUILD — Level 4 "Hầm xe và kho hậu cần" (level-designer, 2026-10-06)

**Trạng thái: DONE** (có 2 mục chưa có runtime, xem "Tồn đọng"). Không commit.

## File
- Mới: `Assets/_Game/Scripts/Game/Editor/Level04Builder.cs` (menu `ClaudeCop/Game/Build Level_04 Prefab`), `Assets/_Game/Scripts/Game/Editor/Level04Assembler.cs` (menu `ClaudeCop/Game/Assemble Level_04`).
- Sinh ra: `Assets/_Game/Level/Level_04.prefab`, `Assets/_Game/Scenes/Gameplay/Level_04.unity` (bản sao Level_01.unity như L3), vật liệu `Assets/_Game/Level/Materials/M_Blockout_{Concrete,ExitGreen,Daylight}.mat`.
- Sửa: `LevelChainAssembler.cs` (menu đổi thành `Assemble Level Chain (1-4)`; thêm `AppendLevel4`, gỡ 10 vật L3 chồng lên lối hầm, `showResultsAfter` ở Phase 6 và 9, `ChainStartSetup.laterDoors` = [null, L3 cửa hầm, L4 cửa kho tiền]; level select 4 nút 235×130, cách 20). Dựng lại: `Level_Chain.unity`, `Title.unity` (LevelSelect).
- Không thêm Level_04 vào Build Settings (L3 cũng không có, theo mẫu).

## Bố cục (toạ độ local chung với Level_02/03; sàn hầm B = -3.0, sàn văn phòng L3 F = 4.4)
```
 z196 +---------------- P3 kho hậu cần (x120..156, trần B+7) --[cửa kho tiền x~145, trượt]--+
      |  dãy kệ 3 m     kệ ngang 12 m (enemy đứng trên)  thùng nổ dưới kệ   xe nâng, pallet  |
 z192 +--- P2 bãi hầm xe (x82..120, trần B+3.6) ---+ lỗ x=120 z~173.8                         |
      | xe đỗ chéo, cột 8 m, lõi kỹ thuật (cửa H03) |                                         |
      | sedan (W1) + trụ/con tin   thùng nổ giữa bãi|                                          |
 z171 +-- P1 phòng máy phát --+ lỗ x=82 z~165.3     | xe bọc thép chắn lối dốc (ánh ngày) z150 |
      | thùng nổ x2, thùng gỗ  |cửa enemy x=82 z157.5                                          |
 z154 +--------x58------------+
 cầu thang thoát hiểm x46.5..58 (37 bậc, F -> B), z160.9..164.1; chiếu nghỉ x45..46.5 ngay sau cửa hầm L3 (x=45)
```
- Ray P1_S1 bắt đầu đúng `CamPoint_P3_S6` của Level_03.prefab (đọc lúc Build), qua cửa hầm, xuống cầu thang, dừng ở chân cầu thang (36.6 m, 5.8 s).
- Tường Bắc cầu thang lùi thành hốc (z 165.1, x 45.3..48.9) để lá cửa hầm L3 mở −100° không cắm vào tường (đo ComputePenetration: chỉ chạm sàn chiếu nghỉ 0.00 m).

## Spawn (16 enemy, 2 con tin, 4 thùng nổ) — tên chuẩn `EnemySpawn_P*_W*_NN` (+Peek, SpawnPointEntry)
| Phase/shot | Nội dung | Kiểu xuất hiện |
|---|---|---|
| P1 S2 | 1 enemy chân cầu thang | Door (cửa x=82) |
| P1 S3 | 1 enemy cạnh thùng đỏ `PropSlot_Barrel_P1_W3_01` (1.6 m) | Auto (chạy vào từ trái, không qua thùng) |
| P1 S5 | 2 enemy lần lượt, thùng `P1_W5_01` ở giữa (1.15 m mỗi bên) | Door + Slide |
| P1 S6 | 1 enemy sau thùng gỗ | Vault |
| P2 S2 | 1 cướp sau sedan (cao 1.25 m → chui lên) + `HostageSpawn_P2_W2_01` sát trụ | Auto/cover |
| P2 S3 | H03: chạy ra từ cửa lõi kỹ thuật, ngang qua rào bê tông `Cover_Barrier_H03` | Door |
| P2 S5 | 2 enemy lần lượt + thùng `P2_W5_01` giữa bãi | Auto + Slide |
| P2 S6 | 1 enemy sau rào bê tông | Vault |
| P3 S2 | 1 cướp đứng trên kệ cao 3 m (chạy dọc mặt kệ) | Auto |
| P3 S3 | 1 cướp dưới kệ cạnh `PropSlot_Barrel_P3_W2_01` (1.8 m) | Auto |
| P3 S5 | 3 cướp dồn dập (L3_Rush, tối đa 2) + `HostageSpawn_P3_W5_01` ở xa | Auto |
| P3 S6 | tên cuối trước cửa kho tiền (kill-zoom FOV 32) → `L4_/DoorOpener_VaultGate` trượt mở | Auto |
Preset dùng lại của Level 3 (L3_Plain, L3 Justice ở P2_W6/P3_W3, L3_Rush). Rank riêng: Phase 9 có `showResultsAfter` → Level 4 có bảng/rank riêng; Level 4 là level cuối chuỗi.

## Kiểm tra (bằng số)
- `Validate Level` **Level_04: PASS (0 FAIL, 0 WARN)** — min 12.3 m, |h| max 5.1°, FOV cần 35.1/42.1, 0/18 tia chặn, Move max 5.8 s. **Level_Chain: PASS (0/0)** — 68 shot, 45 wave, 74 enemy, Move max 5.9 s, PropSlot 11/11. Chi tiết: `Docs/Team/Reviews/validate-Level_04.txt`, `validate-Level_Chain.txt`.
- Kiểm tra riêng của builder (in ở log `[Level04Builder]`): tia nhìn tới mọi mục tiêu với camera lệch −0.6/0/+0.6 m qua **mọi renderer kể cả decor không collider** (MeshCollider tạm), con tin đóng băng của wave trước và thùng nổ: **0/54 tia bị chặn**. Ray cách vật thể ≥ 1.15 m (≥ 0.69 m trong chuỗi: bàn điều khiển L3), cách vật nấp/thùng ≥ 2.7 m; rẽ max 4.0°/m. Thùng–con tin min **16.4 m** (≥ 3.5). Thùng–enemy min 1.2 m, thùng–đường ra cửa min 1.1 m. Đường chạy vào Auto/Slide/Vault không cắt tường (raycast 13/13 sạch).
- Vật trang trí tuỳ chọn (cột, xe đỗ, kệ, pallet, xe nâng, xe bọc thép) chỉ đặt khi không chạm ray/tia nhìn/đường chạy (bỏ 10 vật).
- EditMode: 284/287 pass, 2 skip; 1 fail **có từ trước** (`W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01`: Build Settings đã có 4 scene trước task này, không do L4).
- Console: không lỗi (chỉ log "NoSubscription" của Unity AI generators, không liên quan).
- Ảnh: `Assets/Screenshots/L4_P1_S5.png`, `L4_P2_S2.png`, `L4_P3_S2.png`.

## Tồn đọng / cần agent khác
1. **H03 chưa có runtime** (vấp → ngã sấp → trượt qua chỗ nấp → nhặt súng, không bắn khi đang ngã). Hiện tạm bằng enemy kiểu `Door` chạy ngang qua rào. Đã đặt marker gợi ý `Area_P2_Parking/Gags/GagH03_P2_W3_01` (con: Door, Trip, CoverPass, SlideEnd, GunDrop) — cần gameplay-coder thêm EntryStyle/GagTrip.
2. **Thùng đỏ phát sáng nhấp nháy** (P1 W2 gợi ý luật mới), "đèn pha nhấp nháy" lúc Move P2, "xe bọc thép nổ máy" (giảm tải) — chưa có code; cần gameplay-coder/combat-coder nếu muốn.
3. Tài liệu lệch: storyboard ghi "con tin 2" nhưng liệt kê 3 chỗ (P2 W1, P2 W2, P3 W2). Đã theo task: 2 con tin (P2 W1 sau trụ, P3 W2 ở xa); H03 không có con tin xen.
4. Chuỗi hiện 74 enemy, fallback validator giới hạn 80 → Level 5 sẽ vượt; cần thêm rule `Level_Chain` vào `LevelValidationRules.asset` (gameplay-coder).
5. Chưa chơi thử Play mode (không được giao lượt Play).
