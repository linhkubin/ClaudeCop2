# MAP-REARRANGE — Sắp xếp lại Level_01 cho camera FOV 45 dọc

**Trạng thái:** DONE (chưa commit) · level-designer · 2026-10-05 · có bổ sung MAP-FIX2 và MAP-FIX3 ở cuối file (MAP-FIX3 thay số liệu khoảng cách/góc ở các mục trên)
**File đã đổi:** `Assets/_Game/Level/Level_01.prefab` (vị trí chỗ nấp, spawn/Peek, PickupSpawn, PropSlot, CamPoint, RailHint, thêm 14 khối mới), `Assets/_Game/Scenes/Gameplay/Level_01.unity` (pose 18 CameraShot, `frameTargets` của 12 shot Combat, knot của 6 spline `Rails/Rail_*`, vị trí prop trong `Props`).
**Không đổi:** script, CameraFeelProfile, config, EncounterWave (danh sách spawn giữ nguyên tham chiếu), tên mọi CamPoint/Shot/Spawn.
Console: 0 lỗi game (chỉ có thông báo "NoSubscription" cũ của Unity AI). Ảnh kiểm tra (FOV 45, 9:16, có capsule đánh dấu tạm thời): `Assets/Screenshots/MAP_P*_S*.png` (thư mục không commit).

## 1. Tiêu chí đã dùng
- Màn 9:16, FOV dọc 45 → FOV ngang 26.2°. AutoFrame chừa `frameMargin` 5° mỗi bên, nên muốn **không lùi và không xoay** khi tới điểm thì mọi mục tiêu phải nằm trong |h| ≤ 8.1°. Tôi siết chặt hơn: **|h| ≤ 5.7°** (cụm ≤ ~11.5° ngang) để vừa cả máy 9:19.5 (FOV ngang 21.6°).
- Mục tiêu tính khung gồm: enemy, con tin, grenadier, human shield (điểm Peek + 1.5 m), thùng vũ khí (+0.5 m) và thùng nổ của đợt (+0.6 m). `frameTargets` của mỗi shot giờ có đủ các loại này (trước đây chỉ có enemy/con tin/pickup, và nhiều điểm đã cũ).
- Yaw/pitch của shot đặt đúng tâm cụm, nên AutoFrame chỉ xoay ≤ 0.5°.
- Đã kiểm tra tầm nhìn bằng raycast từ shot tới từng mục tiêu: tất cả OK. Đã kiểm tra chồng hình trên màn: chỉ còn 1 chỗ chấp nhận được (mục 5).
- Dùng 3 tầng dọc thay vì trải ngang (màn dọc có 45° chiều cao): tầng thấp gần (pickup, thùng nổ, v ≈ -8..-15°), tầng mặt đất (enemy sau chỗ nấp thấp, v ≈ 0), tầng cao (ban công, nóc container, mái, v ≈ +8..+15°).

## 2. Kết quả từng góc giao tranh (trước → sau)
Độ rộng ngang = khoảng góc giữa hai mục tiêu ngoài cùng. "Cần FOV" = FOV dọc tối thiểu AutoFrame cần (gồm lề 5°); giới hạn là 45.

| Shot | Trước: rộng ngang / khoảng cách | Sau: rộng ngang | Cần FOV 9:16 / 9:19.5 | Khoảng cách (m) | Lý do / thay đổi chính |
|---|---|---|---|---|---|
| P1_S2 (đường, 3E+H) | 48.4° / 6.7–14 | 11.0° | 36.5 / 43.8 | 6.6–20.8 | Xe dời vào giữa đường; E1 và con tin nấp sau xe (±5°); E3 ló từ chồng thùng ở giữa (15.6 m); E2 lên **ban công mới** ở mặt nhà sau (20.8 m, v +9.7). Thùng nổ và pickup ở tầng thấp. Camera lùi về z 12 (trước là 14). |
| P1_S3 (hẻm, 2E) | 22.8° | 11.0° | 36.5 / 43.8 | 6.6–10.5 | Camera dời sang (-0.5, 1.7, 14.8) để nhìn được vào hẻm (trước góc nhà che). Thùng nổ dời ra miệng hẻm, giữa 2 enemy. Pitch nhìn xuống 5.5°. |
| P1_S5 (ngã tư, 4E+H) | 52.0° / 5.7–13 | 11.0° | 36.5 / 43.8 | 8.3–19.4 | Rào S ở 7.7 m (E và con tin ±5.5°), chồng thùng (E ló bên phải 12 m), rào N (E 15.4 m), **xe tải mới** cuối phố có E đứng trên nóc (19.4 m, v +4.4). |
| P1_S6 (cầu thang thoát hiểm, 2E) | 14.9° | 9.7° | 35.2 / 42.3 | 7.4–9.1 | E dồn lại trên sàn thoát hiểm; camera dịch nhẹ (6.4, 1.65, 29.4), góc thấp ngước lên 9°. |
| P2_S2 (kho, 3E+H+Shield) | 47.1° / 6.3–20 | 11.0° | 36.5 / 43.8 | 6.6–18.1 | Crates/Pallets là 2 chỗ nấp thấp ±5.5° (12.4 m); Shield đi giữa (15 m); **container mới** phía sau có 2 E trên nóc (18.1 m, v +9). Camera lùi về z 6. |
| P2_S3 (gác lửng, 3E) | 17.5° | 11.0° | 37.0 / 44.3 | 9.7–13.7 | E1 ở khe lan can, E2 ló sau lan can B, E3 dưới gác bên thùng cao (đã dời). Dolly nhẹ 0.7 m và pan 46°, ngước lên 7°. |
| P2_S5 (ban công + container, 3E+Gren+H) | 52.5° / 6.4–13.9 | 11.0° | 40.7 / 43.8 | 5.8–12.5 | Khe lan can ban công dời vào giữa (x 100.7–102.3); container (4 m) vào giữa, grenadier trên nóc; pallets ở 7.2 m che 2 E. Camera ở z 26.8 (trước 26), đây là cảnh "tường enemy" cao nhất (v -15..+15). |
| P2_S6 (văn phòng, 3E+Gren) | 45.4° / 10.9–13.8 | 11.0° | 36.5 / 43.8 | 7.6–12.7 | Grenadier trên mái văn phòng; E ở cửa văn phòng; xe nâng và thùng cao dời sát văn phòng. Pan -59° với dolly ngắn. |
| P3_S2 (mái A, 3E+Gren+H) | 49.8° / 6.5–16.4 | 11.0° | 36.5 / 43.8 | 5.4–14.7 | HVAC dời vào giữa (E và con tin ±5.5°, 8.5 m); hộp thấp mới cho E2 ở giữa; bồn nước sau có E3 và Grenadier trên nóc (có gờ). |
| P3_S3 (qua cầu, 3E+H) | 42.2° / 6.5–14.3 | 11.2° | 36.6 / 44.0 | 5.2–19.2 | **Góc mới**: camera dời tới (197, 1.75, 12) nhìn dọc cầu (yaw 90, nhìn xuống 6.5°). E1 và con tin nấp sau 2 lan can mái B (13.7 m); 2 E sau 2 hộp thông gió (19 m); thùng nổ đầu cầu bên B. Đây là cảnh xa và rộng. |
| P3_S5 (mái B, 5E+Gren+2H) | 48.7° / 6.2–13.8 | 11.2° | 38.1 / 44.0 | 5.2–13.1 | 3 tầng: tường chắn 8.5 m (E, con tin, E), bệ 1.8 m (E, con tin), container cao 3.4 m + thùng trên nóc (E, Gren, E). Đây là cảnh cao trào. |
| P3_S6 (phòng máy, 3E+Gren+Shield+H) | 45.1° / 4.2–14 | 11.2° | 36.7 / 44.0 | 5.6–15.6 | Mái phòng máy (thêm 2 gờ thấp) có Gren, con tin, E; hàng thùng thấp xoay vuông góc hướng nhìn che E, Shield, E. Con tin không còn đứng cách camera 4.2 m. |

Kết luận: 12/12 góc vừa khung FOV 45 ở **cả 9:16 và 9:19.5**, không cần lùi và không cần xoay. Không mục tiêu nào gần dưới 3 m (enemy gần nhất 7.4 m, pickup gần nhất 5.2 m); xa nhất 20.8 m (E trên ban công P1_S2, cao khoảng 10% màn).

## 3. Đường ray (Move)
Điểm cuối spline **trùng vị trí** shot Combat kế tiếp (sai số 0.000 m) và **tiếp tuyến cuối = hướng nhìn của shot, kể cả pitch** (sai số 0.00°). Driver nhìn theo tiếp tuyến khi tới cuối, nên tới nơi không còn blend lùi hay xoay lại. Điểm đầu spline trùng shot trước (0.00 m). Knot giữa dùng AutoSmooth, hai đầu dùng Mirrored với hướng chỉ định. RailHint và CamPoint trong prefab đã cập nhật theo.

| Rail | Dài | Cao (m) | Thời gian ước (profile) | Nét chuyển động | Khoảng hở nhỏ nhất |
|---|---|---|---|---|---|
| P1_S1 | 12.4 | 1.60–1.76 | ~4.5 s | Lượn nhẹ trái rồi phải, nâng dần lên 1.75 | > 1.5 m |
| P1_S4 | 16.5 | 1.70–1.85 | ~5.4 s | Vòng qua bên phải xe, cua rộng 90° vào phố ngang, hơi ngước lên khi tới | 1.22 m (xe) |
| P2_S1 | 14.4 | 1.60–1.76 | ~5.1 s | Lượn S nhẹ qua cửa cuốn | > 1.5 m |
| P2_S4 | 21.7 | 1.74–1.95 | **~6.4 s** | Đi vòng phải qua gác lửng, slalom quanh container rồi vào thẳng | 1.16 m (container) |
| P3_S1 | 13.6 | 1.60–1.77 | ~4.9 s | Ra khỏi chòi cầu thang (thẳng qua cửa 1.3 m), lượn nhẹ trái | 0.58 m (lanh tô cửa, khó tránh) |
| P3_S4 | 20.1 | 1.75–2.45 | **~6.0 s** | Lượn qua mái A, chạy thẳng giữa 2 lan can cầu, lên mái B (cao thêm 0.7 m) | 1.0 m (thùng nổ, camera đi phía trên) |

Nhịp từng phase: rộng/vừa nhiều enemy (S2) → pan/dolly sang góc ít enemy hơn (S3) → Move có cua/đổi độ cao → cảnh "tường enemy" nhiều tầng (S5) → pan sang cụm cuối (S6). Combat→Combat đều là blend có pan 39–90°; P1_S3, P2_S3, P3_S3, P1_S6, P2_S6 còn có dolly ngắn 0.6–4.2 m.

## 4. Khối mới trong prefab (blockout, đã có collider và SurfaceMaterialTag sao từ mẫu)
`Cover_Balcony_W2_Parapet`, `Balcony_W2_Floor` (P1), `Cover_Truck_W5`, `Cover_CrateOnTruck_W5` (P1), `Cover_Container_W2`, `Cover_CrateOnContainer_W2` (P2), `Cover_TankRim_W2`, `Cover_Vent_W3_S`, `MR_RoofParapet_S`, `MR_RoofParapet_W` (P3), cùng các RailHint thêm. **Đổi tên** `Area_P3_Rooftop/Cover/Cover_Container_W5` thành `Cover_ContainerRoof_W5` vì trùng tên với container ở P2 (không script nào tham chiếu tên cover). Tái sử dụng: `Cover_VentStack_W2` thành hộp thấp 1.4×1.0×0.8, `Cover_Crates_W3` thành hộp thông gió, `Cover_HVAC_W5` thành bệ tầng B.

## 5. Còn tồn đọng
1. **P3_S5:** đầu con tin giữa tầng mặt đất chồng nhẹ với ống chân enemy tầng B (ngực cách nhau 6.8°). Chấp nhận được; nếu test thấy bấm nhầm thì đổi tầng B sang ±2.75°.
2. **P2_S4 (~6.4 s) và P3_S4 (~6.0 s)** dài hơn `maxMoveSeconds` 5.4 vì `maxRailSpeed` = 4 chặn trần. Tôi không sửa `speedOverride` (ngoài phạm vi task).
3. Ở P3_S3, nền phía sau là các chỗ nấp của đợt W5 (enemy W5 chưa xuất hiện nên không ảnh hưởng gameplay, chỉ hơi rối mắt trong blockout).
4. Kính vỡ (`Prop_Glass_*`) không bắt buộc vào khung; P1_W2/W5/W6 đã dời gần khung, các tấm khác giữ nguyên.
5. Chưa chạy Play mode (không có lượt Play). Nên chạy DebugCameraProbe một vòng để xác nhận không có SPIKE ở các đoạn blend.

## 6. Việc cho gameplay-coder (không bắt buộc)
- **Đừng chạy lại** `ClaudeCop/Game/Assemble Level_01 (T-403)`. Hàm `FrameTargets()` của nó bỏ qua grenadier, shield và thùng nổ; hàm dựng spline từ RailHint không đặt tiếp tuyến cuối = hướng shot, nên sẽ ghi đè các giá trị ở đây. Nếu cần chạy lại, cập nhật assembler: thêm `grenadierSpawnPoints`, `humanShieldSpawnPoints` và `Prop_Barrel_<key>` vào frameTargets; đặt knot cuối Mirrored theo `shot.transform.forward`.
- Cân nhắc `maxRailSpeed` 4 → 4.5 (hoặc `speedOverride` ≈ 4.5 cho P2_S4 và P3_S4) để giữ Move ≤ 5.5 s.
- `frameMargin` = 5° là khá lớn so với nửa FOV ngang 10.8–13.1°. Level hiện đã vừa với lề 5°; nếu giảm còn khoảng 3° thì có thêm chỗ cho máy 9:20 trở lên.

---

## MAP-FIX2: bỏ hai cú chuyển hướng gắt (2026-10-05, chưa commit)
**Trạng thái:** DONE · level-designer. Không đụng script hay config. Console: 0 lỗi, 0 cảnh báo. Scene đã lưu; instance Level_01 trong scene không có override mới (vẫn 12 mod cũ, 0 object thêm).
**File đã đổi:** `Assets/_Game/Level/Level_01.prefab`, `Assets/_Game/Scenes/Gameplay/Level_01.unity`.

### Góc giữa các cặp liền nhau (hướng nhìn sau AutoFrame; ngưỡng Cut là 90°)
| Phase | Trước | Sau |
|---|---|---|
| P1 | S1→S2 0 · S2→S3 **68.5** · S3→Rail S4 **105.8 (Cut)** · S4→S5 0 · S5→S6 39.3 | 0 · **52.0** · **47.7** · 0 · 39.3 |
| P2 (không đổi) | 0 · 46.7 · 19.9 · 0 · 59.0 | như cũ |
| P3 | S1→S2 0 · S2→S3 **89.8** · S3→Rail S4 8.9 · S4→S5 0 · S5→S6 46.0 | 0 · **52.0** · 7.8 · 0 · 46.0 |

Mọi cặp giờ ≤ 59°. Đầu và cuối của cả 6 rail vẫn trùng shot kề (lệch 0.000 m, 0.0°). AutoFrame xoay 0° ở cả 12 shot Combat. Raycast từ shot tới mọi mục tiêu đều thông. Lỗi lớn nhất còn lại là P2_S5→S6 59.0°: vẫn dưới ngưỡng nhưng sát mức 60° (không thuộc phạm vi task nên tôi để nguyên).

### P1 — hẻm (W3) và Rail_P1_S4
- Trong hẻm đông–tây, góc nhìn bắt buộc hướng về phía tây. Vì vậy tôi đưa cụm enemy sát mép bắc của hẻm, gần miệng hẻm, để nhìn chéo từ phố được. CamPoint/Shot_P1_S3 đổi từ (-0.5, 1.7, 14.8) yaw 291 thành **(-0.3, 1.7, 12.6), yaw 307.5, nhìn xuống 4.6°**. Vị trí này chỉ cách S2 0.85 m, nên phần chuyển cảnh chủ yếu là pan.
- `EnemySpawn_P1_W3_01` (-9.7, -1.2, 19.0) và `_02` (-7.6, -1.2, 18.9) xoay mặt về camera. `Cover_Dumpster_W3` đặt tại (-9.09, 0.55, 18.11), `Cover_LowCrates_W3` tại (-6.96, 0.55, 18.34), cả hai xoay yaw 34° để vuông góc với hướng nhìn. Thùng nổ (`PropSlot_Barrel_P1_W3_01` và `Props/Prop_Barrel_P1_W3_01`) đặt tại (-6.3, 0, 17.4). Kết quả: h ±3.3°, v ±3.6°, khoảng cách 7.8–11.4 m.
- `Rail_P1_S4` giờ có 6 knot: (-0.3, 1.7, 12.6) bắt đầu yaw **355** → (0.6, 16.4) → (2.4, 19.6) → (3.1, 22.6) → (3.7, 25.4) → (6.8, 1.75, 28.6) yaw 90 (giữ nguyên). Rail dài 18.2 m, khoảng hở nhỏ nhất 1.05 m (xe). Tôi thêm `RailHint_P1_S4_06`. CamPoint_P1_S4 và Shot_P1_S4_Move cũng đặt yaw 355.

### P3 — qua cầu (W3) và Rail_P3_S4
- Camera chuyển sang góc mái A, phía tây nam đầu cầu, đặt cao hơn để nhìn chéo qua lan can: CamPoint/Shot_P3_S3 đổi từ (197, 1.75, 12) yaw 90 thành **(205.6, 2.5, 8.3), yaw 52, nhìn xuống 3.5°**.
- Đợt W3 dựng lại theo trục nhìn yaw 52, chia ba lớp:
  - **Gần (10.5 m):** E1 `EnemySpawn_P3_W3_01` (213.35, -0.6, 15.44) và con tin `HostageSpawn_P3_W3_01` (214.40, -0.6, 14.10), ở hai bên ±4.6°, nấp sau `Cover_Crates_W3` và `Cover_Vent_W3_S` (đã dời và xoay 52°).
  - **Giữa (14.5 m):** E2 `EnemySpawn_P3_W3_02` (217.03, -0.6, 17.23), nấp sau khối mới `Cover_Box_W3_C`.
  - **Cao (18.3 m):** E3 `EnemySpawn_P3_W3_03` (220.02, **1.0**, 19.57) đứng trên khối mới `Cover_Stack_W3` (cao 1.6 m, mặt trên ở y 2.2), có gờ mới `Cover_StackRim_W3` (0.7 m) che lúc E3 nấp. Cách dựng giống bồn nước W2.
  - **Thấp:** `PickupSpawn_P3_W3_01` (211.63, 0.6, 12.44) và thùng nổ (212.21, 0.6, 13.65), đặt ở cả slot và Props.
  - Kết quả: h ±4.7°, v ±7.3°, khoảng cách 7.4–18.3 m, cả 6 tia nhìn đều thông.
- Các khối mới được sao từ `Cover_Crates_W3`, nên giữ nguyên collider, material và SurfaceMaterialTag. Chúng không lọt vào khung của S5 và S6: S6 lệch hơn 24° ra ngoài, S5 ở phía sau camera.
- `Rail_P3_S4` giờ có 6 knot: (205.6, 2.5, 8.3) bắt đầu yaw **45** → (207.9, 2.45, 11.6), bay trên lan can A ở độ cao 1.35 m rồi vào cầu → (210.6, 12.1) → (212.8, 12.0) → (215.0, 12.0) → (217, 2.35, 12) yaw 90. Rail dài **13.2 m** (trước là 20.1), nên ước khoảng 4.6 s, dưới `maxMoveSeconds` 5.4. **P3_S4 không còn cần đổi maxRailSpeed.** Khoảng hở nhỏ nhất 1.10 m (cột cầu). Các RailHint_P3_S4_01..06 và CamPoint_P3_S4 đã cập nhật theo.

### Ảnh kiểm tra (FOV 45, 9:16, capsule đánh dấu tạm, đã xoá)
Ảnh lưu ở scratchpad của phiên: `FIX2_Shot_P1_S3_Combat.png`, `FIX2_Shot_P3_S3_Combat.png`. Ở P1, hai enemy ló trên dumpster và thùng thấp, thùng nổ ở giữa phía trước. Ở P3 có E1, E2, E3 xếp 3 tầng cùng con tin, pickup và thùng nổ, không chồng lên nhau; lan can cầu chỉ cắt mép dưới khung.

### Còn tồn đọng
1. P3_S3 nhìn ra nền mái B, phía sau còn thấy cover của W5 và W6. Cảnh hơi rối mắt nhưng các mục tiêu của W5 và W6 chưa xuất hiện ở thời điểm này.
2. P2_S4 vẫn dài khoảng 6.4 s và cần `maxRailSpeed` hoặc `speedOverride` (việc của gameplay-coder, xem mục 6).
3. Chưa chạy Play mode. Nên chạy DebugCameraProbe qua hai đoạn S2→S3→S4 của P1 và P3 để xác nhận chuyển cảnh là Blend.
4. Vẫn **không chạy lại** `Assemble Level_01 (T-403)`.

---

## MAP-FIX3: đẩy enemy ra xa camera và thu nhỏ góc chuyển giữa các shot (2026-10-05, chưa commit)
**Trạng thái:** DONE · level-designer. Không sửa script, config hay CameraFeelProfile. Không chạy menu Assemble (T-403). Không vào Play mode.
**File đã đổi:** `Assets/_Game/Level/Level_01.prefab`, `Assets/_Game/Scenes/Gameplay/Level_01.unity` (pose 12 shot Combat và `frameTargets`, pose 6 shot Move, knot của 6 rail, vị trí prop trong `Props`). Scene đã lưu. Instance Level_01 trong scene vẫn chỉ có 12 override cũ, không thêm object.
**Console:** 0 lỗi game. Chỉ còn thông báo cổng MCP và lỗi "NoSubscription" cũ của Unity AI.
**Tên giữ nguyên:** mọi CamPoint, Shot, Spawn, Rail và RailHint. Số knot của mỗi rail bằng số RailHint đang có.

### Cách đo
- Khoảng cách tính 3D, từ vị trí shot tới **ngực** mục tiêu: điểm Peek + 1.5 m (pickup + 0.5 m, thùng nổ + 0.6 m). Đây là cùng quy ước với `frameTargets`.
- Cỡ trên màn hình được tính theo chiều cao người 1.8 m so với chiều cao khung FOV 45. Cỡ 8% tương ứng khoảng 27 m. Mọi enemy đều ≤ 24.5 m, tức ≥ 8.9%.
- "FOV cần" dùng công thức `RequiredFov` của AutoFrame, có lề 5°.
- Sau khi đặt pose, yaw và pitch của shot nằm đúng tâm cụm mục tiêu. Vì vậy AutoFrame không phải xoay thêm, và góc trong bảng dưới cũng là góc sau AutoFrame.
- Đã raycast từ mọi shot tới mọi mục tiêu (0 bị chặn). Đã đo góc tách nhỏ nhất giữa hai mục tiêu bất kỳ trong cùng một shot ("sep").

### 1. Khoảng cách từng đợt (trước → sau)
| Shot | Enemy/con tin trước (m) | Enemy/con tin sau (m) | Pickup / thùng nổ trước → sau (m) | Cỡ nhỏ nhất | \|h\| max | FOV cần 9:16 / 9:19.5 | sep |
|---|---|---|---|---|---|---|---|
| P1_S2 | 11.6–20.8 | **14.8–22.1** | 6.6 / 7.1 → 10.2 / 10.5 | 9.8% | 5.2 | 35.4 / 42.5 | 3.3° |
| P1_S3 | 9.6–11.4 | **13.2–14.9** | – / 7.8 → – / 10.8 | 14.6% | 2.3 | 25.7 / 31.0 | 4.6° |
| P1_S5 | 8.3–19.4 | **13.1–23.2** | – | 9.4% | 4.4 | 32.8 / 39.4 | 2.9° |
| P1_S6 | 7.4–9.1 | **12.9–15.0** | – | 14.5% | 1.5 | 23.0 / 27.9 | 3.5° |
| P2_S2 | 12.4–18.1 | **15.0–19.3** | 6.6 / – → 11.3 / – | 11.3% | 4.6 | 33.4 / 40.1 | 4.1° |
| P2_S3 | 9.7–13.7 | **16.0–19.6** | – | 11.1% | 5.3 | 35.9 / 43.1 | 5.7° |
| P2_S5 | 8.1–12.5 | **17.2–21.8** | 5.8 / 5.9 → 14.3 / 15.4 | 10.0% | 4.8 | 34.2 / 41.2 | 2.6° |
| P2_S6 | 9.8–12.7 | **20.4–22.7** | – / 7.6 → – / 18.4 | 9.6% | 4.8 | 34.1 / 41.0 | 3.2° |
| P3_S2 | 8.5–14.7 | **14.4–20.7** | 5.4 / – → 11.5 / – | 10.5% | 5.1 | 35.0 / 42.1 | 3.9° |
| P3_S3 | 10.5–18.3 | **18.4–24.5** | 7.4 / 8.6 → 9.1 / 21.0 | 8.9% | 5.4 | 36.2 / 43.4 | 2.8° |
| P3_S5 | 8.5–13.1 | **16.0–20.5** | 5.2 / 7.3 → 13.6 / 14.8 | 10.6% | 4.5 | 33.2 / 39.9 | 2.9° |
| P3_S6 | 9.6–15.6 | **15.5–21.8** | 5.6 / – → 11.6 / – | 10.0% | 5.3 | 35.7 / 42.9 | 2.3° |

Enemy và con tin gần nhất giờ là **12.9 m** (trước là 7.4 m), xa nhất là 24.5 m. Pickup gần nhất là 9.1 m (P3_S3, xem tồn đọng 2), thùng nổ gần nhất là 10.5 m. Cả 12 góc đều vừa FOV 45 ở **cả 9:16 và 9:19.5**, \|h\| ≤ 5.4° (giới hạn 5.5°), nên không cần lùi hay nới FOV.

### 2. Góc và khoảng cách giữa các shot liền nhau (trước → sau)
| Cặp | Trước | Sau |
|---|---|---|
| P1_S2→S3 | 52° / 0.85 m | **29.5° / 0.64 m** |
| P1_S3→Rail_P1_S4 (tiếp tuyến đầu) | 48° / 0 m | **0° / 0 m** |
| P1_S5→S6 | 39° / 0.90 m | **21.5° / 0.73 m** |
| P2_S2→S3 | 47° / 0.67 m | **28.3° / 0.72 m** |
| P2_S5→S6 | 59° / 1.53 m | **23.4° / 0.86 m** |
| P3_S2→S3 | 52° / 5.69 m | **29.4° / 0.96 m** |
| P3_S5→S6 | 46° / 0.72 m | **28.8° / 0.57 m** |

Góc là góc 3D, gồm cả pitch. Mọi cặp Combat→Combat đều ≤ 30° và ≤ 1 m. Mọi điểm đầu và cuối rail đều trùng vị trí và hướng của shot kề: lệch 0.000 m và 0.0°, kể cả pitch. Cả 18 CamPoint trong prefab trùng pose của Shot tương ứng (lệch 0).

### 3. Đường ray
Thời gian tính theo `RailCameraDriver`: tốc độ 3.5 m/s, được tăng tối đa tới 4.5 m/s để giữ ≤ 5.4 s.
| Rail | Dài | Cao (m) | Thời gian | Khoảng hở nhỏ nhất | Dốc lớn nhất | Nét |
|---|---|---|---|---|---|---|
| P1_S1 | 11.0 | 1.60–1.75 | 4.2 s | > 1.6 m | 1.4° | Xuất phát yaw 20 (Cut đầu level), cua trái vào góc nhìn chéo phố. Planter_P1_02 đã dời sang (5, 0, 2). |
| P1_S4 | 19.5 | 1.73–1.78 | **5.4 s** (sát trần) | 1.45 m (xe) | 3.5° | Bắt đầu theo hướng S3 (nhìn vào hẻm, yaw 307), cua phải lên phố, cuối đoạn rẽ phải 90° vào phố ngang. |
| P2_S1 | 11.7 | 1.60–1.75 | 4.3 s | 0.95 m (mép cửa cuốn) | 1.1° | Đi qua cửa cuốn ở x ≈ 102, cua nhẹ trái. |
| P2_S4 | 15.5 | 1.74–1.88 | 5.4 s | > 1.6 m | 5.6° | Bắt đầu ngước lên theo S3 (nhìn gác lửng), đi dọc phía đông, cuối đoạn quay trái về phía ban công. |
| P3_S1 | 7.8 | 1.59–2.10 | 3.2 s | 0.60 m (lanh tô cửa, như trước) | 9.4° | Ra khỏi chòi cầu thang rồi nâng lên 2.1 m. |
| P3_S4 | 13.0 | 2.35–3.00 | 4.7 s | 1.25 m | 7.1° | Hạ dần từ 3.0 m, đi chéo mái A, lên cầu và **dừng giữa cầu** (S5/S6). |
Rail P2_S4 (trước 6.4 s) và P3_S4 (trước 6.0 s) đều đã ≤ 5.4 s, nên không cần đổi `maxRailSpeed` hay `speedOverride`.

### 4. Thay đổi bố cục theo từng phase
**P1 — Phố.**
- Cặp camera S2/S3 dời sang mép đông phố: S2 ở (3.8, 1.75, 10.0) yaw 336, S3 ở (3.4, 1.75, 10.5) yaw 307.
- Cụm W2 dời sang trái để S2 nhìn chéo về phía hẻm:
  - `Cover_Car_W2` dời tới (−1.97, 0.55, 22.32) và xoay 155° để vuông góc với hướng nhìn. E1 và con tin nấp sau xe.
  - `Cover_CrateStack_W2` và E3 dời tới (−3.05, 27.3). E3 vẫn ló ngang.
  - **Ban công W2 chuyển sang mặt đông của nhà L_C**: sàn ở (−5.5, 3.3, 29.8), lan can ở (−4.9, 3.4, 29.8), cả hai xoay 90°. E2 đứng tại (−5.5, 2.2, 29.8).
  - Pickup W2 ở (0.6, 0, 19.6), thùng nổ ở (−1.1, 0, 19.2), Box_W2_05 ở (−0.4, 0, 20.9).
- W3 (hẻm): E1 ở (−8.8, −1.2, 19.0), E2 ở (−6.8, −1.2, 18.9). Dumpster ở (−8.27, 18.14) và LowCrates ở (−6.02, 18.27) được đặt lại và xoay vuông góc với hướng nhìn của S3. Thùng nổ ở (−5.0, 0, 17.2).
- Cặp camera S5/S6 lùi về giao lộ: S5 ở (3.0, 1.75, 28.6) yaw 90, S6 ở (2.6, 1.65, 29.2) yaw 110.
  - W5: `Cover_BarrierS_W5` +1 m theo x. E4 và con tin ở x 16.06, z 29.6 và 27.6. E2 ở z 29.1.
  - W6: **cả cụm cầu thang thoát hiểm dời +3 m theo x** (`FireEscape_W6`, 3 lan can, 2 spawn, PropSlot/Prop_Glass_P1_W6_01).

**P2 — Kho.**
- Cặp camera S2/S3 ở gần cửa: S2 ở (102.2, 1.75, 3.4) yaw 353.5, S3 ở (102.6, 1.75, 4.0) yaw 21.3.
- W2:
  - Container W2, thùng trên container và 2 E trên nóc dời −1.7 m theo z (container giờ ở z 20.6–23.0).
  - Crates, pallets, E3, con tin và pickup dời +0.5 m theo x.
  - Shield ra từ (100.13, 0, 20.3), ngay trước container.
- W3 dời về đoạn giữa gác lửng:
  - E1 ló ở khe lan can z 16.7–18.3; spawn ở (110.37, 3.55, 16.81).
  - E2 nấp sau lan can C tại (109.6, 2.35, 22.0).
  - E3 ở dưới gác: spawn (108.94, 0, 22.15). `Cover_CrateHigh_W3` ở (108.8, 0.975, 21.0) và hạ cao còn 1.95 m để không chạm fascia.
- Cặp camera S5/S6 đặt bên phải container W2: S5 ở (107.0, 1.75, 18.5) yaw 339, S6 ở (106.5, 1.75, 19.2) yaw 316.
- W5:
  - **Khe lan can ban công dời tới x 99.6–101.2**: `Balcony_Railing_A` dài 14.6, `Balcony_Railing_B` dài 13.8. E1 ló ở khe.
  - Con tin ở (99.4, 2.85, 38.62).
  - Container W5 −0.9 m và pallets W5 −0.7 m theo x. Grenadier ló ở mép đông container.
  - Pickup ở (102.0, 0, 31.8), thùng nổ ở (100.4, 0, 32.4).
- W6:
  - `Forklift_W6` dời (+2.5, 0, +2.0). E3 nấp sau xe nâng tại (93.6, −1.2, 35.0), vì trước đó E3 trùng hình với E2 ở cửa văn phòng.
  - Thùng nổ W6 ở (93.0, 0, 31.6).

**P3 — Mái nhà.**
- Cặp camera S2/S3 ở góc tây nam mái A: S2 ở (200.4, 2.1, 3.2) yaw 10, S3 ở (200.55, 3.0, 3.5) yaw 39, cao hơn để nhìn qua lan can mái B.
- **Cụm W2 dời +3.5 m theo x**: HVAC, bồn nước, vành bồn, hộp thông gió, 5 spawn, pickup và 2 box. E2 và hộp thông gió lùi lại ở x 202 để không trùng hình với E1.
- **Cụm W3 chuyển sang góc tây bắc mái B**, chia ba lớp:
  - Gần: E1 ở (212.4, 18.0) và con tin ở (213.4, 16.6).
  - Giữa: E2 ở (212.6, 21.6).
  - Cao: E3 đứng trên Stack_W3 tại (215.4, 1.0, 23.0).
  - Các cover W3 được dời theo và xoay vuông góc với hướng nhìn.
  - Thùng nổ ở (214.3, 0.6, 19.3). Pickup ở trên mái A tại (206.0, 0, 10.4).
- Cặp camera S5/S6 **đứng trên cầu**: S5 ở (209.5, 2.35, 12.0) yaw 90, S6 ở (209.9, 2.35, 12.4) yaw 61.
- W5: 2 E trên nóc container giãn ra z 13.6 và 10.4. Pickup ở (223.0, 0.6, 12.48).
- W6:
  - `Cover_LowCrates_W6` xoay 64° và dời tới (223.15, 1.2, 18.77).
  - Grenadier ở (227.35, 2.6, 24.2), E2 trên mái phòng máy ở (228.9, 2.6, 22.7), pickup ở (220.0, 0.6, 18.0).
- Box_P3_W3_01 dời tới (201.0, 0, 14.8) vì trước đó chồng lên HVAC.

Mọi spawn kiểu ló lên được xoay mặt về camera của đợt mình. Spawn kiểu ló ngang được tính lại vị trí để điểm Peek rơi đúng chỗ đã chọn. `Prop_*` trong scene khớp vị trí với `PropSlot_*` tương ứng (lệch 0).

### 5. Ảnh kiểm tra
Ảnh chụp theo pose của 12 shot Combat, FOV 45, khung 540×960, có marker tạm: đỏ là enemy, xanh là con tin, vàng là pickup. Marker và camera tạm đã xoá; scene không bị dirty. Ảnh nằm ở `C:\Users\ADMIN\AppData\Local\Temp\claude\D--Unity-ClaudeCop2\16eccb52-5070-4ac8-bebe-d1a012cb4aeb\scratchpad\shots\FIX3_P*_S*.png` (thư mục tạm, không commit).

### 6. Còn tồn đọng
1. **Bố cục mỗi cặp góc là kết quả cân giữa ≤ 30° và ≥ 12 m.** P1_S2→S3 (29.5°), P2_S2→S3 (28.3°), P3_S2→S3 (29.4°) và P3_S5→S6 (28.8°) đều sát ngưỡng 30°. Nếu dời thêm một cụm, cần đo lại.
2. **P3_S3:** pickup ở mái A chỉ cách 9.1 m. Đặt trên mái B thì lan can B che, còn trên cầu thì lan can cầu che. Thùng nổ W3 chỉ hở khoảng 4 cm trên lan can B ở tâm, nhưng phần nắp thùng vẫn thấy rõ (xem ảnh). Các mục tiêu ở P3_S3 cũng xa nhất level: E3 cách 24.5 m, cỡ 8.9%.
3. **Góc tách nhỏ nhất 2.3–2.9°** (khoảng 0.8–1 m ở 20 m) ở P3_S6 (E1 với shield), P2_S5 (E1 ở khe với con tin), P3_S3 và P3_S5. Không có cặp nào trùng hình hoàn toàn. Nếu test máy thật thấy bấm nhầm thì giãn thêm 0.5 m.
4. Rail P1_S4 và P2_S4 chạm đúng trần 5.4 s, vì driver tự tăng tốc tới 4.4 và 3.53 m/s.
5. Rail P3_S1 dốc tới 9.4° khi nâng từ 1.6 lên 2.1 m sau cửa chòi. Giá trị này dưới `maxPitch` 10°.
6. Ở P2_S6, container W2 (đã hết enemy) chiếm khoảng 10% mép trái khung.
7. Một số chỗ nấp chạm sát thân enemy ở mức khoảng 1 cm theo kiểm tra capsule bán kính 0.22 m, gồm pallets W5 ở P2 và HVAC W2 ở P3. Đây là quan hệ cũ, không đổi.
8. Chưa chạy Play mode (không được giao lượt). Nên chạy lại DebugCamTrace/DebugM2Bot để xác nhận thời gian blend đã ngắn lại, dự kiến khoảng 1 s cho các cặp ≤ 30°.
9. Vẫn **không chạy lại** `Assemble Level_01 (T-403)`, vì menu này sẽ ghi đè frameTargets và tiếp tuyến rail.

## MAP-FIX4: chỉ còn 1 thùng nổ gần con tin (2026-10-05, chưa commit)

- Lỗi: test `Level01_BarrelsKeepAwayFromHostages_ExceptOneTrapInP3` (ngưỡng <= 3.5 m tới `HostageSpawn_*` hoặc `Peek` của nó) đếm 2 thùng: `PropSlot_Barrel_P3_W5_01` (1.48 m, thùng bẫy theo thiết kế) và `PropSlot_Barrel_P3_W3_01` (2.85 m tới `HostageSpawn_P3_W3_01`).
- Sửa (chỉ `Level_01.prefab`): `PropSlot_Barrel_P3_W3_01` (214.3, 0.6, 19.3) -> (217.5, 0.6, 18.2), vẫn trên `Deck_B`, cách kính `Skylight_B_Glass` ~0.8 m, không chồng cover. Cách con tin gần nhất 4.40 m.
- Khung hình: từ CamPoint_P3_S3/S4 (200.55, 3, 3.5) yaw thùng 49.1° (trục camera 39.3°, nửa FOV ngang portrait ~13°) -> vẫn trong khung, nằm bên phải con tin (44.4°), không che enemy W3 (33.7–39.3°). Không đụng enemy, CamPoint hay thông số camera của MAP-FIX3.
- Scene `Scenes/Gameplay/Level_01.unity` dùng prefab instance, thùng cập nhật theo (scene không bẩn, không cần lưu).
- Kiểm tra: EditMode `Level01_BarrelsKeepAwayFromHostages_ExceptOneTrapInP3` + `Level01_M3Markers_NamesCountsAndPeek` PASS 2/2. Console: không lỗi dự án (chỉ 1 dòng "NoSubscription" của Unity AI generators, không liên quan).

## MAP-FIX5: đồng bộ thùng P3_W3 trong scene + đặt gần trục nhìn (2026-10-05, chưa commit)

- Lỗi (REV-WAVE2 🟡 1): MAP-FIX4 chỉ dời `PropSlot_Barrel_P3_W3_01`; `Props/Prop_Barrel_P3_W3_01` trong `Scenes/Gameplay/Level_01.unity` là prefab instance riêng, vẫn ở (214.3, 0.6, 19.3) cách con tin 2.85 m; `frameTargets[5]` của `Shot_P3_S3_Combat` vẫn trỏ điểm cũ. Ghi chú "thùng tự cập nhật" ở MAP-FIX4 là sai.
- Vị trí mới (cả 3 nơi): (213.7, 0.6, 20.5) trên `Deck_B`, giữa `Cover_Box_W3_C` (cách 0.7 m) và kính `Skylight_B_Glass` (cách 0.3 m), cách `Cover_Stack_W3` ~1 m, không chồng collider nào.
  - `Level_01.prefab`: `PropSlot_Barrel_P3_W3_01` -> (213.7, 0.6, 20.5).
  - Scene: `Prop_Barrel_P3_W3_01` -> (213.7, 0.6, 20.5); `Shot_P3_S3_Combat.frameTargets[5]` -> (213.7, 1.2, 20.5).
- Đo từ CamPoint_P3_S3/S4 (200.55, 3, 3.5), trục 39.3°:
  - Lệch ngang thùng -1.3° (cũ +1.7°, vị trí MAP-FIX4 +9.8°). Cụm frameTargets h = -5.40..+5.40 -> AutoFrame xoay 0.00°, không lùi.
  - FOV cần (lề 5°): 9:16 = 36.2° (dư 2.7° ngang ở FOV 45); 9:19.5 = 43.4° (dư 0.4°) -> cả hai <= 45, không nới FOV.
  - Cặp P3_S2->S3: 29.38° / 0.96 m (không đổi, <= 30°). Enemy W3 cách camera 19.1 / 22.0 / 24.6 m (>= 12 m).
  - Khoảng cách tới con tin gần nhất: `HostageSpawn_P3_W3_01/Peek` ~3.9 m (>= 3.5). Gần enemy W3_02 ~2.0 m (thùng bẫy enemy, không bẫy con tin).
  - Che khuất: tia camera tới đầu/thân enemy W3 và con tin không đi qua thùng (thùng lệch E2 4.1°, nằm sau E1 về độ sâu, không che).
- Rà khớp toàn bộ: 32/32 `Props/Prop_*` trong scene trùng `PropSlot_*` tương ứng trong prefab (sai số <= 0.01 m).
- Tồn đọng (không chặn): từ CamPoint_P3_S3, lan can `Parapet_B_W_N` (đỉnh y 1.7) che phần dưới thùng, chỉ thấy ~0.3 m trên cùng (vị trí cũ ~0.4 m). Thùng vẫn bắn được từ trên; nếu muốn lộ rõ hơn cần hạ lan can hoặc đặt thùng lên bệ (việc khác).
- Kiểm tra: lưu scene; EditMode 225/225 PASS; console không lỗi dự án (chỉ dòng "NoSubscription" của Unity AI generators).
- Đề xuất (gameplay-coder/reviewer): thêm test so khớp `Prop_*` (scene) với `PropSlot_*` (prefab) và đo ngưỡng 3.5 m trên `Prop_*` thật.
