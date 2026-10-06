# L4-REBUILD — Dựng lại Level 4 "Kho tiền" (GDD vòng 10–11)

**Trạng thái:** DONE (2026-10-06, level-designer). Chưa kiểm chứng bằng Play mode (chỉ đo bằng số + validator).

## File
- Mới: `Assets/_Game/Scripts/Game/Editor/Level04Builder.cs` (menu `ClaudeCop/Game/Build Level_04 Prefab`), `Level04Assembler.cs` (menu `ClaudeCop/Game/Assemble Level_04`), `Assets/_Game/Level/Level_04.prefab`, `Assets/_Game/Scenes/Gameplay/Level_04.unity`, preset `Prefabs/Enemies/Data/EnemyPreset_L4.asset`, `EnemyPreset_L4_Far.asset`, `EnemyPreset_L4_Rush.asset`.
- Sửa: `LevelChainAssembler.cs` (menu `Assemble Level Chain (1-4)`, `AppendLevel4`, gỡ chiếu nghỉ/bậc/tường đen sau cửa hầm L3, `laterDoors` = [null, L3 cửa hầm, L4 cửa cuốn], Title 4 nút 220×130, cách 40), `Level_Chain.unity`, `Title.unity`.
- Validator: `LevelValidationRules.cs` thêm `maxTargetDistance` (35 m, ≤ 0 = tắt), `LevelValidator.cs` mục (a) FAIL nếu mục tiêu xa hơn; `LevelValidationRules.asset`: rule `Level_04` (cho con tin + khiên người, cấm thùng nổ/thùng súng/grenadier, 5/wave, tổng 27, cap đồng thời 4), rule `Level_Chain` nâng 5/wave và cap 4 (để chứa đợt dồn dập L4). Test: `RulesAsset_HasOwnRuleForLevel01To04_AndChain` (thay bản 01–03), mới `RulesAsset_MaxTargetDistance_Is35m`.
- Ảnh: `Assets/Screenshots/L4_P2_S5.png`, `L4_P3_S3.png` (không commit).

## Bố cục (tọa độ local trùng Level 2/3, sàn kho B = 0)
```
 L3 phòng an ninh (F=4.4) --cửa hầm x=45--> cầu thang bảo mật x46..58 (22 bậc, z160.5..164.5) --> cửa thang máy (trượt mở sẵn)
 P1 sảnh kiểm soát  x58..104  z150..175 (cao 5)   cửa chớp kẹt nửa chừng ở x=104 z=168
 P2 phòng két 2 tầng x104..156 z152..184 (cao 8.5) sàn lưới Bắc (z179..184, +4 m), ban công Đông (x150..156, +4 m), cầu thang sắt dọc tường Nam
    hành lang z144..152 (x~135) --> P3
 P3 đại sảnh vault  x112..160 z90..144 (cao 8)    lõi vàng x130..142 z100..112, cửa vault tròn mặt Bắc, cửa cuốn nạp tiền tường Nam x~156 (nối Level 5)
```
Ray P1_S1 bắt đầu đúng `CamPoint_P3_S6` của Level 3 (30.2, 6.05, 162.7, yaw 99), qua cửa hầm, xuống cầu thang (độ cao camera theo đường cong em đầu/cuối, dốc tối đa ~25°), 6 m cuối thẳng/ngang với P1_S2. Trong chuỗi khoảng cách ray tới renderer L3/L4 nhỏ nhất 0.69 m (mặt console bàn điều khiển L3).

## Wave (27 enemy: P1 7, P2 9, P3 11; 4 khiên người; 4 con tin)
Mỗi Phase đổi góc **hai lần giữa stage**: S2→S3 xoay ~26°, S5→S6 xoay ~26° (validator cặp Combat ≤ 30°, ≤ 1 m), wave mới lộ ngay ở góc mới (blend 0.7 s + `restAfterClear` 0.3 s; không có shot chờ). Preset L4: stagger 0.3–0.6 s, hide 0.6 s.

| Shot | yaw | Nội dung | Khoảng cách |
|---|---|---|---|
| P1 S2 | 90 | 1 T ló sau rào kiểm soát | 20 |
| P1 S3 | 116 | **K khiên người đầu tiên** ra từ cửa phòng kiểm soát (Door) | 16 |
| P1 S5 | 90 | T sau quầy + C ra cửa (Door) + con tin ló sau quầy rồi tự cúi | 22 / 22.5 / H 19 |
| P1 S6 | 64 | 3 C từ hai cửa chớp phía Bắc (Door), đồng thời 3 | 20 / 22 / 27 |
| P2 S2 | 90 | 2 T ló giữa hàng két + **con tin chui lên** từ cửa sập két sàn | 19 / 26 / H 15 |
| P2 S3 | 64.5 | bất ngờ: 2 D nhảy từ sàn lưới (4 m) + 1 X xung phong, đồng thời 3 | 18 / 20 / 14.5 |
| P2 S5 | 89 | K ở két đang mở + **S xạ thủ trên ban công** (preset L4_Far vòng 3.0 s) | 16 / 31 |
| P2 S6 | 116 | 2 C ra từ chân cầu thang sắt + con tin ló/cúi (thay "chạy ngang") | 20 / 26 / H 16 |
| P3 S2 | 180 | khiên đôi (2 K) | 15 / 18 |
| P3 S3 | 206 | T sau xe đẩy tiền + **S xạ thủ 34 m** + X từ lồng tiền + con tin xa ló/cúi (L4_Far), đồng thời 3 | 26 / 34 / 14.5 / H 28 |
| P3 S5 | 180 | **"Dồn dập lõi vàng"**: cửa vault quay mở (khi xong S3), 5 C: 3 Door từ cửa vault + 2 Slide hai lối bên, đồng thời **4**, stagger 0.5–0.8 s, không con tin | 15–18.5 |
| P3 S6 | 154 | tên cầm đầu sau xe vàng, kill-zoom (FOV 32), cửa cuốn nạp tiền trượt lên | 18 |

Phân bố: gần < 18 m 9, vừa 12, xa ≥ 26 m 6 (GDD 6/15/6 — đợt dồn dập buộc phải 15–18.5 m vì cụm ±5.5° trước mặt lõi vàng). Kiểu xuất hiện: ló (T), Door, Drop, Slide, khiên người, X (tạm), S.

## Kiểm tra
- Builder đo bằng số (`Temp/Level04Build.txt`): tia camera (lệch ±0.6 m) → điểm ngắm 93/93 không bị chặn bởi **bất kỳ renderer nào, kể cả vật trang trí không collider**; đường chạy vào (Auto/Slide) không cắt vật; ray cách renderer ≥ 0.95 m; |h| max 5.2°; con tin tách enemy ≥ 4.9°; độ cong ray ≤ 2.5°/m.
- Validate: **Level_04 PASS** (18 shot, 12 wave, 0 FAIL/0 WARN; Move max 5.8 s; yaw ròng max 52, đảo chiều ≤ 2). **Level_Chain PASS** (68 shot, 45 wave; L1 15/24, L2 23/28, L3 20/20, L4 27/27).
- EditMode: 291 test, 290 pass, 1 fail = lỗi sẵn có `W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01`. Console: 0 lỗi dự án (chỉ log "NoSubscription" của Unity AI generators).

## Cần code runtime (giải pháp tạm đang dùng)
1. **X xung phong**: chưa có hướng chạy về camera. Tạm: `Door` với `Door_Enemy_*` đặt sau lưng X ~9 m (vách dưới sàn lưới ở P2; cửa lồng tiền đứng tự do ở P3) → chạy chéo về phía camera, dừng ở 14.5 m. Thiếu tiếng hét + vòng vàng khi chạy.
2. **S xạ thủ**: thiếu laser 1.5 s + chớp nòng 0.3 s và vòng riêng từng enemy (+0.5/+0.8 s). Tạm: preset `L4_Far` (vòng 3.0 s) cho cả wave có xạ thủ; đèn trắng sau lưng xạ thủ (decor).
3. **Con tin chạy ngang** (P2 W4): runtime chưa có → thay bằng con tin ló sau xe đẩy rồi tự cúi sau ~3 s (`hostagesStandStill = false`, `hostageExposeTime` 3 s). Con tin chui lên dùng spawn dưới sàn → Peek (chưa có rung nắp 0.4 s).
4. **Hộp hồi máu** đầu P3: chỉ có marker `HealthHint_P3_S2` (chưa có pickup máu).
5. Cảnh báo dồn dập (mũi tên mép màn hình, đèn xoay) và "cửa kho đóng sầm" chỉ là decor tĩnh; cửa vault tròn quay mở thật (`DoorOpener_VaultCore` khi Wave_P3_W3 Cleared), cửa cuốn nạp tiền trượt lên (`DoorOpener_LoadingShutter`, Wave_P3_W6).

## L4-HOSTFIX (bổ sung) — con tin đứng im đầu stage 4-2
- **Nguyên nhân:** `HostageActor.Freeze()` (EncounterWave gọi khi wave Cleared) đóng băng con tin đang lộ tại chỗ (luật cũ "hết wave thì đứng im"). P2 W2 con tin chui lên ở cửa sập không vật nấp: hạ 2 enemy trong < 3 s là con tin đứng im giữa phòng suốt Phase 2. Phụ: điểm nấp sâu 1.15 m làm lộ đầu trên miệng cửa sập trước/sau khi lộ.
- **Đã sửa:** (1) runtime tạm (`Scripts/Enemy/HostageActor.cs`, chủ gameplay-coder cần duyệt): `Freeze()` → con tin hết bắn được ngay rồi **tự cúi** xuống chỗ nấp trong `retreatDuration` rồi tắt (áp dụng mọi level, đúng GDD vòng 11); test `HostageDoesNotCountForCleared_AndIsDismissedOnClear` thêm kiểm tra tự tắt. (2) Level04Builder: con tin không vật nấp nấp sâu 2.1 m. (3) Level04Assembler: ép `hostagesStandStill = false` cho mọi wave Level 4 (cả chuỗi `L4_`).
- Rà soát Level_04 + Level_Chain: 4 con tin (P1 W5, P2 W2, P2 W6, P3 W3), 0 wave standStill, không ghế trói/dummy/gag tĩnh. Validate cả hai PASS; EditMode 291: chỉ lỗi sẵn có BuildSettings.

## Tồn đọng / ghi chú
- Blockout không có trần (giống L1–L3) nên nhìn thấy trời; GDD muốn "ngầm, đèn trắng". Thêm trần cần đèn/ánh sáng riêng — đề xuất làm ở bước art.
- GDD nói hành lang P1 rộng 5 m, sảnh P3 tròn Ø ≥ 36 m: blockout dùng sảnh 25 m (cần chỗ cho góc xoay 26°) và sảnh chữ nhật 48 × 54 m có vòng tròn vàng trên sàn.
- Rule `Level_Chain` đã nâng 5/wave + cap 4 (ảnh hưởng mọi level trong chuỗi; L1–L3 vẫn có rule riêng khi chơi rời).
- `LevelBuildingRules.md` mục 2 (con tin đứng im) vẫn lỗi thời — thuộc PM sửa.
- Bất tử 0.5 s: không làm ở task này. Build Settings không thêm Level_04.unity (chơi qua Level_Chain).
