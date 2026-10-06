# L3-L4-TRANSITION: chuyển tiếp từ Level 3 sang Level 4

**Trạng thái:** DONE (2026-10-06, level-designer). Kiểm bằng số (mô phỏng `RailCameraDriver` thật trong Editor) và validator. **Chưa chơi thử trong Play mode.**

## Hiện trạng trước khi sửa
- Ray L4_P1_S1 đã bắt đầu đúng góc cuối L3 (không nhảy vị trí/yaw). Nhưng hướng góc cuối (yaw 99 local) lệch 9° so với cửa hầm nên ray phải uốn hình chữ S (vượt quá ~3° rồi quay lại).
- Góc cuối L3 (P3_S6) dùng FOV dọc 32, ngang ~18° ở màn dọc. Cửa hầm nằm sát mép trái khung (h -2..-14°), một nửa cửa ra ngoài khung. Người chơi gần như không thấy cửa mở trước khi bảng kết quả hiện.
- Lá cửa mở -100° bị xuyên tường Bắc cầu thang (z 164.69 so với mặt tường 164.5). Dưới ô cửa có khe sàn hở x 45.0..45.3.
- Cầu thang L4: chỉ có lan can phía Nam, không có trần (nhìn thấy trời), đèn trần lơ lửng, không có biển tầng. Đoạn stub sau cửa của L3 (chiếu nghỉ 4 m, 6 bậc 0.3 m, tường đen) không khớp kích thước/cao độ với cầu thang L4.

## Đã làm
1. **Góc dwell "nhìn cửa hầm mở"** `CamPoint_P3_S7` (L3 prefab): đặt cùng vị trí S6, nhìn thẳng vào tâm ô cửa (yaw 90.7 local). `LevelAssembler.LevelSpec.exitCamPoint/exitDwell/exitBlend` (mới) thêm vào cuối Phase cuối một shot `Shot_P3_S7_Combat` có `dwell` 0.8 s, blend 1.0 s, không có encounter, FOV = `baseFov` 45 (bằng FOV camera ray). Đây là cơ chế dwell runtime có sẵn, không cần code mới. Shot được dùng ở cả `Level_03.unity` lẫn chuỗi (`L3_Shot_P3_S7_Combat`).
2. **Cửa hầm** (`Level03Assembler.AddDoors`): delay 0.3 s, mở trong 1.6 s (cửa nặng), góc -90° (lá cửa nằm sát tường Bắc, không xuyên tường/trần, không chạm collider nào). Thêm ngưỡng cửa `VaultDoor_Threshold` lấp khe sàn. Trên tường phòng an ninh có thêm 2 đèn báo đỏ và biển xanh "B1 VAULT" quanh cửa.
3. **Ray L4_P1_S1** (`Level04Builder`): bắt đầu từ `CamPoint_P3_S7` (fallback S6), đi thẳng qua cửa rồi xuống cầu thang. Độ cong giảm từ ~1°/m xuống 0.2°/m, không còn chữ S.
4. **Cầu thang bảo mật** (`Level04Builder.BuildStair`): thêm mép bậc vàng (22 bậc), lan can hai bên kèm trụ (lan can Bắc bắt đầu từ x 48.4, sau lá cửa mở), trần phẳng trên chiếu nghỉ rồi dốc xuống B+5.0 ở chân thang, đèn trần áp sát trần, 8 đèn khẩn cấp đỏ trên tường, biển treo xanh "B1 VAULT", chữ tường "1F" (đầu thang, tường Nam) và "B1" (chân thang), chữ "B1" trên cửa thang máy. Chữ dùng `TextMesh` với font có sẵn `LegacyRuntime.ttf` (render được trong URP, đã chụp ảnh kiểm tra). Material mới: `Assets/_Game/Level/Materials/M_Blockout_Green.mat` (URP Lit, tắt specular/reflections).
5. **Stub L3 khớp L4** (`Level03Builder`, group `VaultStair_Stub`): dùng đúng chiếu nghỉ x 45.3..46 (mặt sàn F), bậc 12/22 m × 0.2 m rộng 4 m, tường z 160.35/164.65 và trần chung (`Level04Builder.StairCeiling`) nhưng chỉ 6 bậc rồi màn đen. Chuỗi gỡ cả group (`LevelChainAssembler.AppendLevel4`) rồi đặt cầu thang đầy đủ của L4 vào cùng vị trí.
6. **Chơi riêng Level_04** (`Approach_Standalone`): thêm lá cửa hầm đã mở -90°, đèn báo và biển B1 giống Level 3.

## Trình tự khi chơi (chuỗi, điểm nối 3→4)
Tên cầm đầu bị hạ → kill-zoom 0.6 s + nghỉ 0.3 s → camera quay 8° và mở FOV 32→45 sang góc S7 trong 1.0 s → dwell 0.8 s → StageCompleted → sau 1.6 s (realtime) bảng kết quả LEVEL 3 hiện (CONTINUE/HOME). Cửa bắt đầu mở ở 0.3 s và xong ở 1.9 s, tức là mở ngay trong lúc camera quay sang. Người chơi nhìn cửa đã mở cùng cầu thang tối phía sau khoảng 2.4 s trước khi bảng hiện.
→ CONTINUE: tracker rank reset (`PlayerStatsTracker.ResetLevel`), Level 4 tính rank riêng → ray đi thẳng qua cửa, xuống cầu thang; tiêu đề "STAGE 4-1" hiện kiểu seamless lúc ray chạy (không fade đen), giống các điểm nối 1→2 và 2→3.

Cách sắp xếp này giữ quy tắc chung của chuỗi: kết quả hiện khi camera dừng ở góc cuối của level, CONTINUE thì ray chạy tiếp. Khác biệt duy nhất là thêm một góc dwell để thấy cửa mở. Khi chọn bắt đầu từ Level 4: `ChainStartSetup` mở sẵn cửa, camera khởi đầu ở đúng góc S7.

## Đo bằng số (Level_Chain)
| Mục | Kết quả |
|---|---|
| S6 → S7 | cùng vị trí (0.00 m), yaw 189.0 → 180.7, FOV 32 → 45 |
| S7 → đầu ray L4 | lệch vị trí 0.000 m, yaw 0.00°, pitch 0.00°, FOV 45 = 45 |
| Ray L4_P1_S1 (mô phỏng driver 60 Hz) | 6.12 s, tốc độ max 7.6 m/s, gia tốc max 14.3 m/s², **yaw rate max 0.4°/s** (trước 5.9), pitch rate max 7.3°/s (pitch bị kẹp ở `maxPitch` 10°), lệch yaw cuối 0.00° |
| Ray → renderer (cửa đã mở) | min 0.69 m (mặt console bàn điều khiển L3, như trước); trần cầu thang cách camera ≥ 1.75 m |
| Sàn dọc làn x 43..62 (3 làn z) | không hở, bước max 0.20 m (bậc thang) |
| Lá cửa mở -90° | 0 collider chồng (chỉ chạm AABB của tường ở bản lề) |
| S7 → ô cửa (20 tia) | 16/20 thông. 4 tia sát chân cửa (cao F+0.4) bị bàn điều khiển thấp che; phần cửa phía trên bàn nhìn rõ |
| Builder L4 | tia chặn 0/93, ray gần renderer nhất 0.95 m |

## Validate / test
- Validate: **Level_03 PASS** (19 shot, 0 FAIL/0 WARN), **Level_04 PASS** (18 shot), **Level_Chain PASS** (69 shot, 45 wave; (c) 0/97 tia bị chặn; (d) lệch nối rail 0.000 m).
- EditMode: 293 test, 290 pass, 2 skip. 1 fail là lỗi có sẵn `W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01`. Console không có lỗi hay cảnh báo.
- Ảnh (không commit): `Assets/Screenshots/L3L4_S7_DoorOpen.png`, `Assets/Screenshots/L3L4_StairDescent.png`.

## File đã đổi
- `Assets/_Game/Scripts/Game/Editor/Level03Builder.cs`, `Level03Assembler.cs`, `Level04Builder.cs`, `LevelAssembler.cs` (LevelSpec.exitCamPoint), `LevelChainAssembler.cs`
- `Assets/_Game/Level/Level_03.prefab`, `Level_04.prefab`, `Materials/M_Blockout_Green.mat` (mới)
- `Assets/_Game/Scenes/Gameplay/Level_03.unity`, `Level_04.unity`, `Level_Chain.unity` (dựng lại)

## Bổ sung: L3-L4-DESKHOP (lỗi người dùng báo: camera đi xuyên bàn)
- **Nguyên nhân:** `Control_Desk` cùng mặt console (`Control_Desk_Console`) trong phòng an ninh L3 (x 39.9..40.7 local, z 160..165, chắn ngang làn, mặt trên F+0.96) nằm ngay trên ray L4_P1_S1. Camera bay qua ở độ cao mắt F+1.65, chỉ cao hơn mặt bàn 0.69 m nên trông như đi xuyên qua bàn. Bố cục ở Level_Chain và Level_03 giống nhau. Ngoài bàn này, không có vật nào khác nằm trên làn (cửa hầm lúc chạy đã mở).
- **Sửa** (`Level04Builder.DeskHop`): ray **nâng thêm +0.5 m** khi đi qua bàn. Đoạn đỉnh phẳng kéo dài từ 0.5 m trước bàn tới 0.5 m sau bàn; lên và xuống mỗi đoạn dài 3 m theo đường smootherstep C2. Đoạn ray tới cửa có 23 điểm hint, cách nhau khoảng 0.58 m. Kích thước bàn chuyển thành hằng `Level03Builder.DeskX/DeskHalf/DeskTop`. Level_04 chơi riêng có thêm bàn giống hệt trong `Approach_Standalone`. Builder có thêm phép đo "camera trên mặt bàn" (FAIL nếu < 1.0 m).
- **Đo** (mô phỏng `RailCameraDriver` trong Level_Chain, cửa mở): camera cao hơn mặt bàn tối thiểu **1.19 m** (trước 0.69). Renderer gần nhất cách 1.15 m (lanh tô cửa; trước là mặt console 0.69 m). Vận tốc đứng tối đa 2.33 m/s, pitch tối đa 8.9° (dưới maxPitch 10°), tốc độ quay dọc tối đa 7.4°/s, tốc độ 7.6 m/s, Move 6.13 s. Đầu ray lệch 0.000 m / 0.00°.
- Validate Level_03/Level_04/Level_Chain PASS (chuỗi (c) 0/97 tia bị chặn). EditMode 290/293 pass, 2 skip; chỉ còn lỗi có sẵn BuildSettings.

## Bổ sung 2 (2026-10-06): profile nhảy qua bàn, enemy cuối Level 1, chơi tiếp chuỗi từ scene lẻ

### Việc 1: profile cao độ qua bàn 0 → +0.5 → −0.2 → 0
- `Level04Builder.DeskHopOffset`: các mốc nối bằng smootherstep (C2, không gấp khúc). Lên +0.5 m trong 3.0 m, đỉnh phẳng trên bàn ±0.5 m, hạ nhanh trong 1.8 m ngay sau mép bàn xuống −0.2 m, rồi từ từ về 0 tại ngưỡng cửa hầm (x 45.6). Đoạn ray tới cửa có 31 điểm hint.
- Builder ghi thêm: cao độ tương đối −0.20..+0.50 m; camera trên sàn phòng an ninh ≥ 1.45 m (FAIL nếu < 0.5 m); camera trên mặt bàn ≥ 1.18 m (FAIL nếu < 1.0 m).
- Mô phỏng `RailCameraDriver`:

| Đại lượng | Giá trị |
|---|---|
| Thời gian Move | 6.15 s |
| Tốc độ max | 7.6 m/s |
| Pitch max | 9.0° (dưới 10°) |
| Tốc độ đổi pitch max | 7.4°/s |
| Tốc độ đổi yaw max | 0.4°/s |
| Vận tốc đứng max | 4.4 m/s |
| Gia tốc đứng đỉnh | 66 m/s², ở đoạn hạ nhanh |
| Khoảng trên bàn | 1.19 m |
| Khoảng trên sàn | 1.45 m |
| Renderer gần nhất | 1.16 m (lanh tô cửa) |

  Mức −0.2 m không cần giảm.

### Việc 2: enemy cuối Level 1 đi xuyên tường
- **Nguyên nhân:** Wave_P3_W4 #01 (tên cuối) và Wave_P3_W3 #03 đứng trên bậc tam cấp trước cửa chính, kiểu xuất hiện Auto. Kiểu này cho enemy chạy ngang theo camera nên điểm xuất phát nằm trong khối Bank_Main_South, và đường chạy cắt `Facade_S`. Marker `Door_Enemy_P3_W4_01` là một cửa giả dán trên Facade_S, đè lên khung cửa chính và không được dùng.
- **Sửa** (`Level01Assembler.ApplyMainDoorFix`, menu `ClaudeCop/Game/Fix Level_01 Main Door Entries`, idempotent; `LevelChainAssembler` gọi trước khi copy Level_01.unity):
  - Trong Level_01.prefab: bỏ cửa giả; đặt marker rỗng `Door_Enemy_P3_W4_01` trong ô cửa chính (45.8, 1.2, 35.0); gắn `SpawnPointEntry(Door)` cho hai enemy trên.
  - Trong Level_01.unity: `DoorOpener_Main` (cửa kính trượt) giờ mở khi **Wave_P3_W2** Cleared (delay 0.5 s, mở trong 1.0 s), thay cho Wave_P3_W4. Cửa bật mở khi camera sang góc S3, hai enemy bước ra từ cửa; hết W4 thì camera đi qua cửa đã mở vào Level 2.
  - Đã đo trong chuỗi: đường đi từ cửa tới Peek không cắt renderer nào ở các độ cao 0.3/1.0/1.7 m; marker không nằm trong khối nào.
- **Các enemy khác của L1:** những đường cắt tường còn lại đều đi qua cửa có sẵn (Garage_SideDoor, Door_Enemy_P1_W4_02, Frame/Opening P2, Wing_SideDoor_W5). Enemy P2_W3_01 trên cầu thang thoát hiểm bị lan can che ≥ 3/5 nên chui lên, không chạy. Riêng P3_W2_02 có chân sượt qua chậu cây Cover_Planter_W2_B ở độ cao 0.3 m; đây không phải tường, chưa sửa.
- **Lưu ý:** `Level01Assembler.Assemble` (T-403) đã lỗi thời. Level_01.unity đã được chỉnh tay (Phase 3 chỉ còn 5 shot), chạy lại sẽ lỗi `CamPoint_P1_S5`. Không dùng menu này; dùng `Fix Level_01 Main Door Entries`.

### Việc 3: Play ở scene lẻ thì chơi tiếp các level sau
- **Cách chọn:** biến mỗi scene lẻ thành **cửa vào chuỗi**. Component mới `Assets/_Game/Scripts/Game/StandaloneLevelRedirect.cs` (root `ChainEntryRedirect`, `levelIndex` = 0..3) có `DefaultExecutionOrder(-10000)`. Khi bấm Play thẳng ở Level_0X.unity trong Editor (chưa qua Title):
  1. tắt mọi root khác của scene lẻ;
  2. đặt `GameCommands.SelectedLevel = levelIndex`;
  3. nạp Level_Chain (`EditorSceneManager.LoadSceneInPlayMode`).

  Từ đó chơi level X rồi X+1… với bảng kết quả, rank riêng mỗi level và CONTINUE như chuỗi (dùng `PhaseDirector.FirstPhaseOfSelectedLevel` và `ChainStartSetup` có sẵn).
- **Không đổi gì khi đã qua Title**, vì Title vốn nạp thẳng Level_Chain. Chọn level N chơi từ N tới hết chuỗi như cũ. Không đổi Build Settings. Muốn chỉ chơi riêng một level thì tắt `redirectToChain` trong component.
- `LevelChainAssembler.EnsureStandaloneRedirects` (menu `Add Chain Redirect To Level Scenes`, cũng chạy cuối `Assemble`) đặt component vào cả 4 scene. Chuỗi xóa component này khỏi bản sao Level_01 để không tự nạp lại chính nó; thêm vào đó `ShouldRedirect` trả false khi scene đang chạy là Level_Chain.
- Test EditMode mới `StandaloneLevelRedirectTests` (2 test).
- **Đã thử Play mode:** Play ở Level_02.unity → scene active là Level_Chain, SelectedLevel 1, Phase 3 = `L2_Shot_P1_S1_Move` (đầu Level 2).

### Kiểm tra chung
- Validate Level_01, Level_02, Level_03, Level_04 và Level_Chain đều PASS (chuỗi: 69 shot, 0/97 tia bị chặn).
  - Có một lần Level_02 báo FAIL do `Neighbor_North` (vật của Level 1) chặn tia. Lần đó validate chạy ngay sau Level_01, và chạy lại riêng thì PASS. Nhiều khả năng do dữ liệu physics của scene trước còn sót, không phải lỗi level.
- EditMode: 295 test, 292 pass, 2 skip. 1 fail là lỗi có sẵn `BuildSettings_OnlyTitleThenLevel01`.
- Console không có lỗi dự án.

## Tồn đọng / cần agent khác
- **Âm thanh/đèn động khi cửa mở:** `DoorOpener` không có hook âm thanh hay đèn. Đèn báo đỏ và biển hiện chỉ là decor tĩnh. Nếu cần tiếng khóa xoay hoặc còi, hoặc đèn nháy khi mở, gameplay-coder có thể thêm event `Opened`/`OpenStarted` trong `DoorOpener`.
- `LevelAssembler.cs` và `LevelChainAssembler.cs` thuộc gameplay-coder. Thay đổi nhỏ và có chú thích (thêm trường `exitCamPoint`), cần gameplay-coder duyệt.
- `TextMesh` (legacy 3D text) dùng cho chữ blockout vì asmdef `ClaudeCop.Game.Editor` không tham chiếu TMP. Ở bước art nên thay bằng decal/texture.
- Pitch khi xuống thang bị `CameraFeelProfile.maxPitch` = 10° giới hạn (thang dốc 20°). Muốn cảm giác "đi xuống" mạnh hơn thì tăng maxPitch (ảnh hưởng mọi level, thuộc gameplay-coder).
- Tổng thời gian từ phát bắn cuối tới lúc bảng kết quả hiện khoảng 4.3 s. Nếu thấy dài, giảm `exitDwell` (0.8) trong `LevelSpec` hoặc `StageResultPresenter.showDelay` (1.6, ui-coder).
- Chưa kiểm chứng bằng Play mode.
