# L456-REMOVED — Xóa Level 4, 5, 6; chuỗi về 1-3

**Trạng thái:** DONE (2026-10-06, level-designer)

## Sao lưu
`D:\Unity\ClaudeCop2_backup_L456\` — giữ nguyên đường dẫn `Assets/...`, 74 file (kèm .meta). Danh sách: `_deleted_files.txt`. Bản trước khi sửa của LevelChainAssembler.cs, LevelValidationRules.asset, LevelValidatorTests.cs, Level_Chain.unity, Title.unity nằm trong `_preedit/`.

## Đã xóa (qua `AssetDatabase.DeleteAssets`, .meta đi kèm, không còn file mồ côi)
- `Scripts/Game/Editor/Level04|05|06 Builder.cs + Assembler.cs` (6)
- `Level/Level_04|05|06.prefab`, `Scenes/Gameplay/Level_04|05|06.unity` (6)
- `Prefabs/Enemies/Data/EnemyPreset_L5, L5_Plain, L5_Rush, L6, L6_Plain, L6_Rush` (6; không có EnemyPreset_L4*)
- Material blockout chỉ L4-6 dùng (đã kiểm GUID trong mọi .unity/.prefab/.asset/.mat và tên trong .cs): `M_Blockout_Concrete, Daylight, EmergencyRed, ExitGreen, Fire, Gold, LampWhite, Moss, Rust, Steel, SunBeam, Water` (12). Các material còn lại đều được Level 1-3 / code khác dùng → giữ. (`M_Blockout_Ground` không do L4-6 dùng → giữ.)
- `Assets/Screenshots/L4_*, L5_*, L6_*` (7 ảnh)

## Đã sửa
- `LevelChainAssembler.cs`: menu `ClaudeCop/Game/Assemble Level Chain (1-3)`; bỏ AppendLevel4/5/6 và danh sách RemoveFromLevel3/4/5 (L3 giữ nguyên cửa hầm + cầu thang); ChainStartSetup.laterDoors = [null, L3_DoorOpener_Vault]; Title level select `levelCount=3`, nút 300x130, spacing 50.
- `LevelValidationRules.asset`: bỏ rule `Level_05`, `Level_06` (không có rule Level_04). Giữ rule 01/02/03/Chain và mọi field chung.
- `LevelValidatorTests.cs`: test LevelKey/limit chuyển sang tên L2/L3; thay `Level06_Rule_...` bằng `RulesAsset_HasOwnRuleForLevel01To03_AndChain`.
- Dựng lại `Level_Chain.unity` (root: Level_01/02/03, 9 Phase, bảng kết quả sau Phase 3, 6) và `Title.unity` (3 nút).
- Build Settings: không có L4-6 (vốn không có).

## Kiểm tra
- Validate: Level_01 PASS (14 shot, 9 wave), Level_02 PASS (18/12), Level_03 PASS (18/12), Level_Chain PASS (50 shot, 33 wave) — 0 FAIL, 0 WARN.
- Console: 0 lỗi sau biên dịch/ghép.
- EditMode: 290 test, 287 pass, 2 skip, 1 fail = lỗi sẵn có `W6AssetIntegrityTests.BuildSettings_OnlyTitleThenLevel01` (Build Settings có 4 scene).

## Tồn đọng
- Comment trong `Level03Builder.cs` (dòng ~486, "nối Level 4"), `LevelAssembler.cs` (dòng 51, 219, ví dụ "Level 5") và `LevelValidationRules.cs` (dòng 105, ví dụ "L5_") vẫn nhắc L4/L5 — chỉ là chú thích, để nguyên (mã chung).
- `Docs/Design/Levels_BankHeist.md` vẫn mô tả Level 4-6 (thuộc game-designer).
