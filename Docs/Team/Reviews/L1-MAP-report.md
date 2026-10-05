# L1-REBUILD — Dựng lại Level 1 "Mặt tiền ngân hàng" (tuyến một chiều)

**Trạng thái:** DONE (chưa commit) · level-designer · 2026-10-05
**Thay cho** bản L1-MAP cũ (tuyến P2→P3 phải ra ngõ, vòng dọc đường rồi mới vào sân).

**File đã đổi**
- `Assets/_Game/Level/Level_01.prefab`: xoá toàn bộ nội dung cũ, dựng mới (169 khối, 93 collider có `SurfaceMaterialTag`, 50 điểm rỗng).
- `Assets/_Game/Scenes/Gameplay/Level_01.unity`: chỉ chỉnh dữ liệu scene, không thêm hay bỏ hệ thống.
  - Đặt lại pose của 14 Shot và knot của 5 Rail.
  - Thay `Shot_P2_S4_Combat` bằng `Shot_P2_S4_Move`, thêm `Rail_P2_S4`, cập nhật `PhaseDirector.phases`.
  - Trỏ lại `spawnPoints` của 9 `Wave_*` và hai `pivot` của `DoorOpener_Main` vào object mới cùng tên.
  - Xoay `Directional Light`.
- Hệ thống giữ nguyên: Main Camera + CinemachineBrain, ViewmodelCamera/ViewmodelRoot, CameraRig/PhaseDirector/RailCamera, CombatSystems, GameSystems, GameplayUI (EventSystem), FxSystems, RankScoreSystems, PropSystems, Encounters (preset, prefab, maxConcurrent), DoorOpener, skybox `M_Sky_Day`, ambient Trilight.

**Không đổi:** script, config, `CameraFeelProfile`, prefab trong `Prefabs/`, material (dùng lại `M_Blockout_*` có sẵn). Không chạy menu Assemble T-403. Không vào Play mode.

**Kiểm tra**
- Scene đã lưu.
- Instance `Level_01` chỉ còn các override mặc định của root, đã gọi RemoveUnusedOverrides.
- Console: 0 lỗi dự án. Chỉ còn dòng `NoSubscription` của Unity AI.
- Mọi số đo bên dưới lấy trong Editor bằng spline thật và Physics: `SplineContainer.EvaluatePosition/Tangent`, OverlapSphere, Raycast.

## 1. Sơ đồ nhìn từ trên (x = đông →, z = bắc ↑, mặt đất y = 0)
```
 z
 50 |=========== Backdrop_North (14 m) =================================|
    | đường |              SÂN TRƯỚC (đá, ~20 m)        bồn hoa  8 bậc  4 cột  |
 35 | chính |    P3 ●(24.3,29.6) ───yaw 72──▶  W2 x36-38  x38-42  x43.5  |cửa z35 ▶ sảnh (46,35) ▶ cửa xoay x52.6
    |       |   ↗ (camera rẽ phải ra sân)                              | mặt tiền x=45, nhìn về TÂY ra sân
 24 |-------+ miệng ngõ +-----------------------------------------------+
    | NHÀ   |  NGÕ  | KHỐI CÁNH NGÂN HÀNG (x23-45, cao 10 m)           | SẢNH CHÍNH
    | BÊN   | 6 m   |   thang thoát hiểm (z15-19, y3.4), hốc cửa z21-23 | (x45-70)
    | 6 m   | ↑ P2B ●(20.5, 7.5)                                        |
 12 |-------|  ↑    xe van (tây, z10-15)                              |
    | BÃI ĐỖ| ↑ P2A ●(19.2,-2.0)  xe tải nhỏ trong bãi                 |
  0 |-------|  ↑                                                      |
    | GARA  |  bậc thấp (tây) · cột điện                               |
 -9 |-------+-------+----------------------------------------------------
    vỉa hè: thùng rác (16.6)   xe van (x24-29)   trụ biển bus (31)
-18 ================= ĐƯỜNG PHỤ (Đông-Tây, 12 m) =======================
    P1 ●(19.3,-24) nhìn bắc vào miệng ngõ
-27 -------- ngã rẽ x14-30 · dải phong tỏa z-30 · 2 xe cảnh sát z-33 ----
-42       ● P1_S1 bắt đầu (22.3,-41.9), hướng 350
```
- **Tuyến đi một chiều:** đi lên từ sau hai xe cảnh sát, qua đường phụ, rồi vào thẳng ngõ hông đi về hướng bắc.
  - Ngõ chạy dọc hông khối cánh ngân hàng (bên phải).
  - Cuối ngõ mở ra sân trước. Camera rẽ phải một lần (~68°) để nhìn mặt tiền.
  - Lên bậc thềm, qua cửa chính, vào ngưỡng sảnh.
  - Tuyến không quay lại, không cắt chéo đường đã đi, không ra lại đường phố.
- **Nắng:** `Directional Light` xoay (50, 35, 0), nghĩa là nắng từ hướng tây nam, cao 50°, cường độ và màu giữ nguyên. Góc giữa hướng nhìn và hướng mặt trời: P1 125–145°, P2 ~145°, P3 125–140°, nên nắng luôn ở sau lưng người chơi.
  - Nhà bên (cao 6 m) che bóng dải phía tây của ngõ (~2 m). Đó là nửa râm của ngõ.
  - Mặt tiền nhìn về tây nên được chiếu nắng.
- **Hierarchy prefab** `Level_01` (giữ khung cũ):
  - `Shared_BankBlock/{Ground, Buildings, Bank}`
  - `Area_P1_Sidewalk`, `Area_P2_Alley`, `Area_P3_Steps`: mỗi khu có `Geometry, Cover, Decor, CamPoints, RailHints, Spawns`.
- **Màu theo quy ước:** chỗ nấp màu đỏ (`M_Blockout_Red`). Riêng thùng carton giữ màu carton.
- **Collider:** mọi collider có `SurfaceMaterialTag`: Concrete, Metal (xe, thang, cột đèn), Wood (carton), Glass (cửa, cửa xoay), Foliage (cây trong bồn).
  - Vỉa hè có collider dày 0.12 m, nên enemy P1 đứng ở y = 0.12.
  - Vạch kẻ đường và mặt lát sân là tấm mỏng không có collider.

## 2. Shot, rail, hướng đi tích luỹ
Pose = `CamPoint_*` cùng tên trong prefab, khớp Shot trong scene (lệch 0.000 m / 0.00°). Pitch dương là nhìn xuống.

| Shot | Loại | Vị trí | Yaw / Pitch | Ghi chú |
|---|---|---|---|---|
| Shot_P1_S1_Move | Move, Cut | (22.26, 1.60, −41.93) | 350.1 / 0 | Rail_P1_S1 thẳng, đi giữa hai xe cảnh sát ra mép đường |
| Shot_P1_S2_Combat | Combat, blend 0.5 | (19.15, 1.65, −24.20) | 350.1 / 0.1 | Góc trái: thùng rác |
| Shot_P1_S3_Combat | Combat, blend 1.2 | (19.50, 1.65, −24.00) | 16.7 / 0.1 | Góc phải: xe van |
| Shot_P1_S4_Combat | Combat, blend 1.1 | (19.30, 1.65, −23.70) | 358.3 / 0.0 | Bậc thấp và cột điện ở miệng ngõ |
| Shot_P2_S1_Move | Move, blend 0.4 | = pose P1_S4 | 358.3 / 0.0 | Rail_P2_S1 vào ngõ |
| Shot_P2_S2_Combat | Combat, blend 0.5 | (19.22, 1.65, −2.00) | 1.2 / 0.5 | Đầu bãi đỗ, xe van gần |
| Shot_P2_S3_Combat | Combat, blend 0.9 | (19.92, 1.65, −1.50) | 3.2 / −4.9 | Mở khung: cửa sổ tầng 2 (cao) và cuối ngõ (xa) |
| **Shot_P2_S4_Move** | **Move, blend 0.4, speedOverride 3.3, lookKeys {0.35: 10°, 0.70: 10°}** | = pose P2_S3 | 3.2 / −4.9 | **Nhịp giảm tải 3.7 s:** camera tiến chậm 9 m, lia phải nhìn lối ra sân (thấy sân nắng qua miệng ngõ), có thời gian reload. Không có encounter |
| Shot_P2_S5_Combat | Combat, blend 1.0 | (20.46, 1.65, 7.50) | 3.8 / 0.6 | Thùng carton và cột |
| Shot_P3_S1_Move | Move, blend 0.4, speedOverride 4.8 | = pose P2_S5 | 3.8 / 0.6 | Rail_P3_S1 đi hết ngõ, rẽ phải ra sân |
| Shot_P3_S2_Combat | Combat, blend 0.5 | (24.30, 1.65, 29.60) | 71.7 / 0.6 | Góc thấp, sân trước |
| Shot_P3_S3_Combat | Combat, blend 1.0 | (24.15, 2.50, 29.70) | 74.1 / 1.0 | Góc cao hơn (nâng 0.85 m), bậc thềm |
| Shot_P3_S4_Combat | Combat, blend 0.8 | (24.05, 2.50, 29.95) | 72.3 / −0.5 | Cửa chính |
| Shot_P3_S5_Move | Move, speedOverride 4.8 | = pose P3_S4 | 72.3 / −0.5 | Rail_P3_S5 lên bậc thềm, qua cửa, vào ngưỡng sảnh |

Pose cuối: `CamPoint_P3_S5_End` (46.0, 2.85, 35.0), yaw 90, pitch 0. Camera đứng trong ngưỡng cửa chính, nhìn vào sảnh, cửa xoay `Lobby_RevolvingDoor` ở trước mặt (x 52.6). Đây là pose bắt đầu của Level 2.

**Rail** (knot Bezier, tangent Broken, hai đầu theo đúng hướng pose kể cả pitch). Mọi đầu và cuối rail lệch **0.000 m / 0.00°** so với shot kề, đo bằng tangent spline so với forward của Shot.

Thời gian tính theo driver: `1 + L/v`. Trong đó v = speedOverride nếu có; nếu không, v = 3.5 m/s, tăng tối đa tới 4.5 m/s để giữ trong 5.4 s.

| Rail | Dài | Thời gian | Hướng đi (tiếp tuyến) | Đổi hướng | Bước rẽ trái lớn nhất | Cao (m) | Khoảng hở nhỏ nhất |
|---|---|---|---|---|---|---|---|
| Rail_P1_S1 | 18.00 m | 5.4 s | 350.1 → 350.1 | 0 (thẳng) | 0 | 1.60–1.65 | 1.10 m (xe cảnh sát B) |
| Rail_P2_S1 | 21.70 m | 5.8 s | 358.3 → 1.2 | +2.9 phải | 0 | 1.65–1.68 | 1.07 m (tường bậc thấp) |
| Rail_P2_S4 | 9.02 m | 3.7 s | 3.2 → 3.8 | +0.6 phải | 0 | 1.65–1.78 | 2.5 m |
| Rail_P3_S1 | 23.09 m | 5.8 s | 3.8 → 71.7 | +67.9 phải | 0 (−0.002°, sai số mẫu) | 1.61–1.66 | 1.28 m (`Cover_Pier_W5`) |
| Rail_P3_S5 | 22.65 m | 5.7 s | 72.3 → 90.0 | +17.7 phải | 0 | 2.50–2.85 | 0.75 m (dưới rèm sắt), cửa đã mở |

- Tổng thời gian Move là 26.5 s. Không rail nào quá 6 s. Cộng 15 enemy, level ước tính khoảng 125–140 s.
- **Hướng đi tích luỹ** của toàn tuyến là +99.9°, đi từ 350.1 lên 90.0, và mọi đoạn đều rẽ phải:
  - các rail: 0 + 2.9 + 0.6 + 67.9 + 17.7;
  - chỗ nối tại các điểm dừng: +8.2 (P1), +2.0 (P2 A), 0 (P2 B), +0.6 (P3).
  - Không có đoạn nào rẽ trái. Giới hạn là ≤150°.
- Góc lia khi camera **đứng yên** trong lúc giao tranh không tính vào hướng đi: P1 350→17→358, P2 1→3, P3 72→74→72.
- Khi Shot_P2_S4_Move có `lookKeys`, driver giữ hướng nhìn lúc vào ray và lia tới 10°. Cuối ray, driver khớp đúng hướng Shot_P2_S5.

**Góc và khoảng cách giữa các Combat liền nhau** (góc 3D, có tính pitch; giới hạn ≤30° và ≤1 m):

| Cặp | Góc | Khoảng cách |
|---|---|---|
| P1 S2→S3 | 26.7° | 0.40 m |
| P1 S3→S4 | 18.4° | 0.36 m |
| P2 S2→S3 | 5.7° | 0.86 m |
| P3 S2→S3 | 2.4° | 0.87 m |
| P3 S3→S4 | 2.4° | 0.27 m |

P2 S3→S5 nối qua Rail_P2_S4 (nhịp giảm tải), không phải cặp Combat liền nhau.

## 3. Spawn theo wave (quy ước W = S của shot)
Ký hiệu cách ló:
- **pop**: ló lên. Spawn nằm thấp hơn Peek 1.2 m.
- **side**: ló ngang. Peek lệch sang bên so với Spawn.

Mọi spawn đã xoay mặt về camera của wave mình. Raycast từ shot tới ngực và đầu lúc nấp đều bị chỗ nấp chặn.

| Wave (storyboard) | Shot / Encounter | Điểm | Peek (world) | Kiểu / chỗ nấp |
|---|---|---|---|---|
| P1 W1 | P1_S2 / `Wave_P1_W2` | `EnemySpawn_P1_W2_01` | (16.60, 0.12, −9.65) | pop / `Cover_Dumpster_W2` (thùng rác lớn, trái) |
| P1 W2 | P1_S3 / `Wave_P1_W3` | `EnemySpawn_P1_W3_01` | (23.35, 0.12, −11.20) | side / `Cover_Van_W3` (xe van, phải) |
| P1 W3 (lần lượt) | P1_S4 / `Wave_P1_W4` | `EnemySpawn_P1_W4_01` bậc thấp | (17.55, 0.30, −5.35) | pop / `Cover_StoopWall_W4` trên `Stoop_Garage` |
| | | `EnemySpawn_P1_W4_02` sau cột | (19.90, 0.00, −7.55) | side / `Cover_Pole_W4` (cột điện) |
| P2 W1 | P2_S2 / `Wave_P2_W2` | `EnemySpawn_P2_W2_01` | (19.60, 0.00, 16.00) | side / `Cover_Van_W2` (xe van gần) |
| P2 W2 (dồn dập, 2 cùng lúc) | P2_S3 / `Wave_P2_W3` | `EnemySpawn_P2_W3_01` cửa sổ tầng 2 (cao) | (22.35, 3.50, 17.00) | pop / `Cover_FE_Railing_W3` (chiếu nghỉ thang thoát hiểm, trước `Window_F2`) |
| | | `EnemySpawn_P2_W3_02` cuối ngõ (xa) | (19.40, 0.00, 23.40) | side / `Cover_Bins_W3` |
| P2 W3 (lần lượt) | P2_S5 / `Wave_P2_W5` | `EnemySpawn_P2_W5_01` sau thùng | (20.55, 0.00, 20.85) | side / `Cover_Cartons_W5` |
| | | `EnemySpawn_P2_W5_02` sau cột | (22.30, 0.00, 22.10) | side / `Cover_Pier_W5`, bước ra từ hốc cửa hông của khối cánh |
| P3 W1 (lần lượt) | P3_S2 / `Wave_P3_W2` | `EnemySpawn_P3_W2_01` gần | (36.90, 0.00, 32.75) | pop / `Cover_Planter_W2` (bồn hoa) |
| | | `EnemySpawn_P3_W2_02` xa | (38.00, 0.00, 35.30) | pop / `Cover_Planter_W2_B` |
| P3 W2 (dồn dập, tối đa 2) | P3_S3 / `Wave_P3_W3` | `EnemySpawn_P3_W3_01` bậc thấp | (39.30, 0.30, 32.85) | pop / `Cover_Urn_W3` |
| | | `EnemySpawn_P3_W3_02` bậc cao | (41.95, 1.05, 36.20) | pop / `Cover_Pedestal_W3` |
| | | `EnemySpawn_P3_W3_03` cạnh cửa | (43.45, 1.20, 34.35) | side / `Column_Stone_2` |
| P3 W3 | P3_S4 / `Wave_P3_W4` | `EnemySpawn_P3_W4_01` cửa chính | (44.40, 1.20, 35.10) | side, từ sau `Column_Stone_3` bước ra trước cửa |

- Tổng 15 enemy: P1 4, P2 5, P3 6.
- `maxConcurrent` giữ như cũ: W3 của P2 và W3 của P3 là 2, các wave khác là 1.
- Không có HostageSpawn, PickupSpawn, PropSlot, GrenadierSpawn hay ShieldSpawn. Vật trang trí không nổ, không vỡ.

## 4. Đo khung và khoảng cách
Cách đo:
- Ngắm vào ngực (Peek + 1.5 m), FOV dọc 45, lề 5°.
- "FOV cần" là FOV dọc nhỏ nhất để cụm enemy vừa khung 9:16 và 9:19.5.
- "Cỡ" là chiều cao người 1.8 m so với chiều cao khung.
- "Thông" nghĩa là raycast từ shot tới ngực không bị chặn.
- "Nắng" nghĩa là raycast từ ngực về phía mặt trời không bị chặn.

| Shot | Khoảng cách (m) | \|h\| max | \|v\| max | FOV cần 9:16 / 9:19.5 | Tách nhỏ nhất | Cỡ nhỏ nhất | Tầm nhìn | Nắng |
|---|---|---|---|---|---|---|---|---|
| P1_S2 | 14.8 | 0 | 0 | 17.7 / 21.5 | – | 14.7% | thông | có |
| P1_S3 | 13.4 | 0 | 0 | 17.7 / 21.5 | – | 16.3% | thông | có |
| P1_S4 | 18.4 / 16.2 | 3.8 | 0.5 | 30.7 / 37.0 | 7.6° | 11.8% | thông | có |
| P2_S2 | 18.0 | 0 | 0 | 17.7 / 21.5 | – | 12.1% | thông | có |
| P2_S3 | 19.0 / 24.9 | 4.3 | 5.3 | 32.6 / 39.2 | 13.6° | 8.7% | thông | có |
| P2_S5 | 13.4 / 14.7 | 3.4 | 0.0 | 29.4 / 35.5 | 6.8° | 14.8% | thông | có |
| P3_S2 | 13.0 / 14.8 | 4.3 | 0.0 | 32.4 / 39.0 | 8.6° | 14.6% | thông | có |
| P3_S3 | 15.5 / 18.9 / 19.9 | 4.2 | 1.6 | 32.0 / 38.5 | 3.6° | 10.9% | thông | có |
| P3_S4 | 21.0 | 3.5 | 0 | 29.8 / 35.9 | – | 10.4% | thông | râm (dưới mái hiên) |

- Gần nhất 13.0 m, xa nhất 24.9 m. Mọi enemy cách camera ≥12 m.
- Mọi cụm có \|h\| ≤ 4.3° (giới hạn 5.5°) và vừa FOV 45 ở cả hai tỉ lệ màn.
- P3_S4 lệch tâm +3.5° có chủ đích. Shot nhìn hơi trái cửa để Rail_P3_S5 đi lên cửa chỉ rẽ phải.

## 5. Việc cho gameplay-coder (đã nối sẵn, cần rà lại)
1. **PhaseDirector.phases** (đã đặt):
   - P1: `Shot_P1_S1_Move, S2, S3, S4_Combat`
   - P2: `Shot_P2_S1_Move, S2_Combat, S3_Combat, Shot_P2_S4_Move, S5_Combat`
   - P3: `Shot_P3_S1_Move, S2, S3, S4_Combat, Shot_P3_S5_Move`

   Tiêu đề giữ "STAGE 1-1..3".
2. **Encounter:**
   - 9 `Wave_*` vẫn gắn đúng Shot Combat như cũ.
   - `spawnPoints` đã trỏ sang spawn mới cùng tên.
   - Preset, prefab Enemy và maxConcurrent không đổi.
   - `frameTargets` của Shot đã điền (ngực enemy).
3. **Nhịp giảm tải P2** giờ là đoạn Move ngắn (`Shot_P2_S4_Move` + `Rail_P2_S4`), không còn Combat dwell 3.5 s:
   - Lý do: tách quãng ngõ để mọi rail ≤6 s và cả tuyến chỉ rẽ một chiều.
   - Nếu muốn quay lại kiểu dwell: đặt một Shot Combat dwell 3.5 s ở pose P2_S3, rồi rail nối thẳng P2_S3 → P2_S5.
4. **speedOverride 4.8** trên `Shot_P3_S1_Move` và `Shot_P3_S5_Move` để hai rail dài 23 m chạy trong 5.8 s. Mức này trên `maxRailSpeed` 4.5 của profile; driver vẫn có ease-in/out.
5. **Cửa chính** (`Area_P3_Steps/Geometry/DoorPivot_Main_L` bản lề z 36.4, `DoorPivot_Main_R` bản lề z 33.6, cánh kính có collider):
   - Mở vào trong (+x): L xoay yaw −90, R xoay +90. `DoorOpener_Main` giữ nguyên giá trị này, trigger vẫn là `Wave_P3_W4`.
   - **Cửa phải mở trước Rail_P3_S5**, nếu không camera sẽ đi xuyên kính.
   - Khi cửa mở, camera cách rèm sắt `Shutter_Main_HalfDown` (y 3.6–4.8) 0.75 m và cách sàn/bậc ≥1.58 m.
6. Không chạy `Assemble Level_01 (T-403)`, vì menu này dựng lại theo bố cục cũ.

## 6. Còn tồn đọng
1. Chưa chạy Play mode. Nên chạy bot (DebugM2Bot / DebugCamTrace) một vòng để xem tốc độ xoay thực tế, nhất là Rail_P3_S1: rẽ 68° trong ~6 m cuối, `lookNextMaxYawRate` 40°/s.
2. Enemy cửa chính (P3_W4_01) đứng dưới mái hiên, nên ở vùng râm, chỉ có ánh sáng môi trường. Các enemy khác đều có nắng.
3. Thùng carton P2 (`Cover_Cartons_W5`) là khối tĩnh, SurfaceMaterial Wood, chưa vỡ được. Muốn vỡ thì thay bằng `Prop_Box` (việc của gameplay).
4. Enemy xa nhất là P2_W3_02 (cuối ngõ): 24.9 m, cỡ 8.7% khung.
5. P3 S2→S3 vẫn nâng camera 0.85 m tại chỗ, không có bục đỡ, giống bản trước.
6. Ảnh kiểm tra nằm ở thư mục tạm của phiên, không commit: `C:\Users\ADMIN\AppData\Local\Temp\claude\D--Unity-ClaudeCop2\16eccb52-5070-4ac8-bebe-d1a012cb4aeb\scratchpad\shots_v2\L1_*.png`.
   - Ảnh chụp ở FOV 45, 540×960, capsule đỏ đánh dấu Peek (marker đã xoá khỏi scene).
   - `L1_Overview.png` là ảnh nhìn từ trên xuống.
7. Công cụ dựng (data → prefab) cũng nằm ở thư mục tạm, cùng nơi: `scratchpad\l1\` (`plan.py`, `p3.py`, `route.py` sinh ra `spec.txt`). Có thể dùng lại nếu cần chỉnh số.
