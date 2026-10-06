# L6-BUILD — Level 6 "Cống ngầm thoát hiểm" (Easy, 3 phase) (level-designer, 2026-10-06)

**Trạng thái: DONE.** Không commit. Chưa chơi thử Play mode (không được giao lượt Play).

## File
- Mới: `Assets/_Game/Scripts/Game/Editor/Level06Builder.cs` (menu `ClaudeCop/Game/Build Level_06 Prefab`), `Level06Assembler.cs` (menu `ClaudeCop/Game/Assemble Level_06`).
- Sinh ra: `Assets/_Game/Level/Level_06.prefab`, `Assets/_Game/Scenes/Gameplay/Level_06.unity` (bản sao Level_01.unity như L3–L5), vật liệu `Assets/_Game/Level/Materials/M_Blockout_{Moss,Rust,Water,Fire,EmergencyRed,SunBeam}.mat`, preset `Assets/_Game/Prefabs/Enemies/Data/EnemyPreset_{L6,L6_Plain,L6_Rush}.asset` (vòng target 2.5 s, Easy).
- Sửa `LevelChainAssembler.cs`: menu `Assemble Level Chain (1-6)`, `AppendLevel6` (gỡ `EscapeWall_Dark`, `EscapeWall_Side_W/E` của L5 vì chắn ray, mảng `RemoveFromLevel5` internal để builder dùng chung), `showResultsAfter` ở Phase 3/6/9/12/**17**, `ChainStartSetup.laterDoors` thêm phần tử 5 = `L6_DoorOpener_TunnelGate` (cho Level 7 sau này), level select **6 nút 150×130, cách 20** (6×150+5×20 = 1000).
- Sửa `Assets/_Game/Scripts/UI/LevelSelectPresenter.cs` (thuộc ui-coder, sửa tối thiểu theo task): nhãn nút `NoWrap` + `enableAutoSizing` (54 → min 24), hộp chữ nhỏ hơn nút 16 px → "LEVEL 6" không xuống dòng.
- Sửa `LevelValidationRules.asset`: thêm rule `Level_06` (cho phép grenadier, con tin, thùng nổ; cấm khiên người, thùng súng; 4/wave, tổng 20, đồng thời 3), đặt trước `Level_Chain`.
- Test mới `LevelValidatorTests.Level06_Rule_AllowsGrenadierAndHostage_ForbidsShieldAndCrate` (đọc asset thật; kiểm cả `PerLevelLimit` trong chuỗi = 20 và `LevelKeyOf("L6_…")`).
- Dựng lại: `Level_Chain.unity`, `Title.unity` (LevelSelect 6 nút).
- Không thêm Level_06 vào Build Settings (như L3–L5).

## Bố cục (toạ độ local chung với Level_02..05; sàn B = -3.0)
```
 P3 ngã ba hầm bảo trì x180..202 z442..472 (trần B+6.6)
   tường Bắc: cửa hầm Tây(184) | CỔNG GIỮA 191 (trượt Đông 3.4 m khi Wave_P3_W6 Cleared -> ga tàu L7) | cửa hầm Đông(198)
   catwalk B+3.6 (z470.2..472) trên 3 cửa; ống sàn z464 (đỉnh B+0.5) + thùng nổ trên ống; ống chằng chịt tường/trần
   ^ lỗ x~191.2 (z442)
 P2 kênh cống ngập x187.2..203 z392..442 (trần B+6): bờ trái 187.2..192 | nước 192..200 (mặt B-0.6) | bờ phải 200..203
   cầu đi bộ sắt z431..433.4, mặt B+2.8 (camera đi dưới cầu, cách 0.75 m); vệt nắng qua lưới sắt trần, ống xả, đèn khẩn cấp
   ^ miệng ống cống (8 m) z392, sân bê tông z392..396
 P1 ống cống tròn x187.2..195.2 z350..392 (trần B+5, vòm xiên 2 góc): mảnh vỡ + lửa nhỏ, cánh cửa hầm đổ, miệng cống trên trần
   ^ cửa hầm cháy z350 (lỗ 3.6 m tại x 192.1, vết cháy đen) <- lối nối z344.3..350
 ^ tường thoát hiểm Level 5 (z344.15, lỗ 189.2..193.2, ray qua x=192.2)
```
- Ray P1_S1 bắt đầu đúng `CamPoint_P5_S6` của Level_05.prefab, đi thẳng qua tường thoát hiểm (30.9 m, rẽ max 1.3°/m). Group `Approach_Standalone` (sàn/tường cuối lõi kho vàng có lỗ) chỉ để chơi riêng, chuỗi gỡ.
- Lighting: group `Lighting` 3 point light không đổ bóng (lửa cam P1, nắng P2, đỏ khẩn cấp P3) + vật liệu phát sáng (lửa, đèn đỏ, vệt nắng).

## Spawn (12 enemy + 5 grenadier = 17, 2 con tin, 1 thùng nổ, 1 cảnh hài)
| Phase/shot | Nội dung | Kiểu xuất hiện |
|---|---|---|
| P1 S2 | 1 enemy sau mảnh vỡ bê tông | Vault |
| P1 S3 | **grenadier đầu tiên** | Door (tường Đông ống) |
| P1 S5 | grenadier | Drop từ miệng cống trên trần (5.0 m) |
| P1 S6 | 1 enemy (lần lượt sau grenadier) | Door (tường Tây) |
| P2 S2 | 1 enemy bờ trái | Door |
| P2 S3 | 1 enemy bờ phải (camera xoay 29°, FOV 34, xa 17.8 m) | Door (tường Đông) |
| P2 S5 | **H08**: 1 enemy bờ trái (Door), grenadier trên cầu (Door ở cao độ cầu), con tin bị trói trên cầu xen giữa, 1 enemy chạy qua cầu từ bờ phải (Auto) + `Gag_P2_W5_01` (trượt bờ ướt, đâm ống, rơi xuống nước); L6_Rush, tối đa 2 | |
| P2 S6 | 2 enemy lần lượt (maxConcurrent 1): bờ trái + trên cầu | Door + Auto |
| P3 S2 | grenadier + `PropSlot_Barrel_P3_W2_01` trên ống (cách 2.3 m) | Door (cửa hầm Tây) |
| P3 S3 | grenadier thứ hai (cách thùng 2.8 m) | Auto |
| P3 S5 | 3 dồn dập (2 từ 2 cửa hầm, 1 chạy trên catwalk) + con tin trước cổng giữa; L6_Rush, tối đa 2 | Door/Door/Auto |
| P3 S6 | enemy cuối chạy tới cổng, kill-zoom FOV 32 → `DoorOpener_TunnelGate` trượt mở | Auto |

## Kiểm tra (bằng số)
- **Validate Level_06: PASS (0 FAIL, 0 WARN)** — 18 shot, 12 wave, min 12.4 m, |h| max 5.3°, FOV cần 35.7/42.9, 0/19 tia chặn, góc cặp shot max 29.0, Move max 5.5 s, 17 enemy (≤20).
- **Validate Level_Chain: PASS (0 FAIL, 0 WARN)** — 116 shot, 77 wave, 20 Phase, 0/139 tia chặn, Move max 5.9 s, 121 enemy ≤ 192 (32×6), 13/13 PropSlot.
- Đo riêng builder (log `[Level06Builder]`, tia qua **mọi renderer kể cả decor không collider, ống, lưới, lan can, vệt nắng**; Level 5 đặt tạm với tường thoát đã mở): **0/57 tia bị chặn**; ray cách vật ≥ 0.75 m (dầm cầu khi đi dưới cầu; mép tường thoát L5 0.99 m); không điểm nào chui vào vật (capsule 0.35); đường chạy vào 0/16 cắt vật; thùng–con tin min 6.6 m. 1 vệt nắng tuỳ chọn bị bỏ vì chạm tia nhìn.
- EditMode toàn bộ: 287/290 pass, 2 skip, 1 fail **có từ trước** (`W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01`, Build Settings đã có 4 scene). Test mới pass.
- Console: không lỗi (chỉ "NoSubscription" của Unity AI generators).
- Ảnh: `Assets/Screenshots/L6_P2_S5.png`, `L6_P3_S5.png` (marker spawn không hiện; người cảnh hài hiện trong edit mode vì GagFall chỉ ẩn khi chạy).

## Giải pháp tạm / tồn đọng
1. **"2 enemy hai bờ" (P2 W1)**: luật |h| ≤ 5.5° không cho 2 bờ cách 8 m nước cùng một khung → tách thành S2 (bờ trái) + S3 (bờ phải, camera xoay 29° tại chỗ) = "lần lượt". Enemy bờ phải xa 17.8 m (FOV 34 bù).
2. **H08**: dùng `GagFall` có sẵn (hop = chỗ đâm ống, land = dưới mặt nước). Người hài hiện ra tại chỗ rồi trượt, không có đoạn chạy vào; "tự thoát" chỉ là nằm chìm trong nước. Muốn chạy-trượt-ngoi lên cần code GagFall mới (enemy/gameplay-coder).
3. "Giảm tải: đèn pin, nước chảy" chỉ là bối cảnh (nước tràn, ống xả, vệt nắng); không có hiệu ứng đèn pin. "Tiếng tàu điện" khi cổng mở chưa có âm thanh.
4. Vệt nắng là khối phát sáng đục (blockout), không phải volumetric; đã đặt ngoài tia nhìn/ray.
5. `LevelSelectPresenter.cs` thuộc ui-coder — đã sửa tối thiểu (autoSize nhãn); ui-coder nên xem lại.
6. Chưa kiểm bằng chơi thật: grenadier Door ở cao độ cầu (B+2.8), Drop 5 m qua trần không collider, enemy Auto chạy dọc cầu/catwalk.
