# REV-WAVE2 - Review đợt 2 (CAM-FIX2/CAM-SMOOTH, ENEMY-SHOOTABLE, HIT-FRONT, MAP-FIX3/4)

Chế độ full, chỉ đọc (không Play, không sửa/lưu scene). Ngày 2026-10-05.
Phạm vi: phần MỚI so với REV-CAM-HIT-MAP-review.md.

## Kết luận: CHANGES REQUESTED (1 lỗi bắt buộc sửa, ở scene; code không có lỗi chặn)

Console: sạch (chỉ có thông báo cổng MCP và "NoSubscription" của Unity AI). Test EditMode: tôi KHÔNG chạy lại được (không được phép), kết quả "tất cả đạt" lấy từ báo cáo level-designer, chưa tự xác nhận.

---

## Vấn đề

### 🟡 1. MAP-FIX4 chưa có hiệu lực trong scene: `Prop_Barrel_P3_W3_01` vẫn ở vị trí cũ
- `Assets/_Game/Scenes/Gameplay/Level_01.unity` dòng ~715-740 (PrefabInstance &175530011, cha là `Props` ở gốc 0,0,0): `m_LocalPosition = (214.3, 0.6, 19.3)`.
- `Level_01.prefab:23976` `PropSlot_Barrel_P3_W3_01` đã dời sang (217.5, 0.6, 18.2).
- Báo cáo MAP-FIX4 nói "scene dùng prefab instance nên thùng tự cập nhật". Sai: `Props/Prop_*` là các prefab instance riêng (prefab thùng d52c7a8e...) với override vị trí, KHÔNG phụ thuộc `PropSlot`. Tôi so sánh cả 32 `Prop_*` trong scene với `PropSlot_*` trong prefab bằng script: 31 khớp, chỉ thùng này lệch 3.38 m.
- Hậu quả: test `Level01_BarrelsKeepAwayFromHostages_ExceptOneTrapInP3` chỉ đo `PropSlot` nên PASS, nhưng thùng thật trong game vẫn cách `HostageSpawn_P3_W3_01` 2.85 m (vi phạm ngưỡng 3.5 m mà test muốn bảo vệ), và sửa của MAP-FIX4 không có tác dụng ở runtime.
- Liên quan: `frameTargets` của `Shot_P3_S3_Combat` (scene dòng ~4596) vẫn chứa điểm thùng cũ (214.3, 1.2, 19.3). Nếu chỉ dời Prop về (217.5, 0.6, 18.2) mà không cập nhật `frameTargets`, khung hình không tính thùng mới.
- Rủi ro khi dời thật: từ CamPoint_P3_S3 (200.55, 3, 3.5) hướng 39° (yaw), thùng mới nằm ở yaw 49.1° tức lệch ngang 10°. FOV dọc 45 ở 9:16 chỉ có nửa FOV ngang 13.1°; cộng lề `frameMargin` 5° thì cần 15° > 13.1°. Nếu thùng nằm trong `frameTargets`, AutoFrame sẽ xoay/lùi/nới FOV, phá giả định "AutoFrame xoay 0°" và có thể đẩy cặp P3_S2->S3 (hiện 29.3°) vượt 30°. Báo cáo chỉ kiểm tra "vẫn trong khung", chưa tính lề 5° (cạnh 9:19.5 chỉ còn 0.8° dư).
- Hướng sửa: level-designer đồng bộ `Prop_Barrel_P3_W3_01` và `frameTargets` với `PropSlot`, rồi đo lại FOV cần/góc cặp S2->S3. Tốt hơn là chọn vị trí thùng gần trục nhìn hơn (|h| <= 5.4° như các thùng khác) mà vẫn >= 3.5 m cách con tin/Peek và không chồng collider kính `Skylight_B_Glass` (x 214.3..216.7). Nên thêm vào test một kiểm tra khớp `Prop_*` (scene) với `PropSlot_*` để lỗi dạng này không lặp.

### 🟢 2. CameraPoseSmoother: thiết kế đúng, ghi chú nhỏ
Đã kiểm theo yêu cầu, không thấy lỗi chặn:
- Thứ tự: chạy trong `CinemachineCore.CameraUpdatedEvent`, ngay sau Brain (UpdateMethod LateUpdate), trước mọi LateUpdate khác. Pose thô lấy từ `Brain.State` chứ không đọc transform nên gọi nhiều lần/frame vẫn idempotent (mỗi lần tính lại từ `committed`, `committed` chỉ tiến 1 lần/frame). `TapShooter`/`ScreenPointToRay` (Update) thấy pose cuối frame trước = đúng hình người chơi đang thấy; `TargetReticlePresenter` (LateUpdate) thấy pose đã làm mượt. Không có lệch hướng bắn.
- Pause/timeScale: nhánh `Time.timeScale > 0f` giữ `cand = committed` khi timeScale = 0 (RevivePrompt đặt 0) nên không tích lũy trễ; `CombatPauseSignal` không đụng timeScale, camera vẫn chạy bình thường khi blend. `dt` kẹp 0.05 s chống hitch.
- Cut: `RequestSnap` chỉ gọi ở `RunMove` khi `!hasPrev || forceCutNext` và `RunCombat` khi `cut` (tức vào level hoặc giữa Phase lúc màn đen). Snap chờ tới khi `ActiveVirtualCamera == snapExpect` nên không bị snap sớm vào pose cũ. `RailCameraDriver.Prepare` đặt transform ray ngay (đoạn 69), nên pose thô frame sau Activate đã là pose mới; nhảy 100 m giữa Phase không trượt dài. Tôi đã grep: không còn chỗ nào dùng `ShouldCut`/`cutAngle`/`Styles.Cut` ngoài nhánh cut hợp lệ (chỉ `Level01Assembler.cs:98` còn gán `ShotEntry.Cut`, vô hiệu).
- Giới hạn gia tốc vs blend: đỉnh blend EaseInOut = 1.5x trung bình (30 độ/s -> 45; 7 m/s -> 10.5) nằm dưới trần smoother (70 độ/s, 12 m/s); gia tốc blend tối đa 110 độ/s^2, 14 m/s^2 nằm dưới 260 và 30. Độ trễ ổn định khi theo đuổi tốc độ không đổi = v^2/(2*0.9*a) (khớp số đo 3.4-6.2 độ, 0.36-0.55 m) và hội tụ về 0 khi pose thô dừng (blend EaseInOut kết thúc với vận tốc 0), nên camera tới đích kịp; enemy chỉ lộ sau stagger >= 0.4 s + peek 0.3 s nên có dư thời gian. Mọi shot có `fov` 58/60 bị kẹp về 45 (`maxVerticalFov`), cùng FOV ray (`baseFov` 45) nên không có chênh FOV lớn mà bộ giới hạn FOV 14 độ/s phải đuổi.
- Rung (hit/nổ) được tách bằng `CameraFeelState.ShakePos/ShakeRot` rồi cộng lại sau: đúng.
- Giảm chuyển động: nhân 0.6 cho cả 3 trần và blend dài ra theo (`need / 0.6`), không Cut: khớp. Trần xoay 42 độ/s hơi thấp hơn `maxYawRate` 45 của driver; chỉ gây trễ tích lũy nếu một đoạn ray quay liên tục > 42 độ/s (hiện các cua dài ~4 s nên không xảy ra).
- Gợi ý (không bắt buộc): (a) `PhaseDirector.OnDisable` đặt `started=false` nhưng không xóa `active` và `smoother.has`; nếu một PhaseDirector bị tắt/bật lại thì lần StartLevel sau không snap (hasPrev = true). Hiện không thấy đường dùng như vậy (Revive không tắt PhaseDirector), chỉ cần lưu ý. (b) Bóc rung giả định mọi vcam live đều có `CameraFeelApplier` ghi `ShakePos`; vcam không có Applier (nếu có) sẽ lệch rất nhỏ khi blend. (c) `Step()` là hàm thuần nhưng chưa có test EditMode; nên thêm vài test (không vượt gia tốc, không vọt lố, tới đích) vì đây là lưới an toàn cuối pipeline.
- `DebugCamTrace`: nằm trong `Scripts/Game/Debug`, bọc `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, KHÔNG gắn vào scene nào (grep scene: không có); không ảnh hưởng bản build release. `DebugM2Bot` cũng chưa gắn scene.
- Reframe aspect: `ReframeShots` chạy trong Update khi aspect đổi, dùng `AutoFrameSmooth` (lerp theo `unscaledDeltaTime`) cho shot đang live và `AutoFrame` cho shot khác; `AutoFrame` đặt lại pose gốc (`basePos/baseRot`) trước khi tính nên gọi nhiều lần không trôi. Không thấy null reference (kiểm `shot==null`, `VCam==null`, `brain==null`). Events đăng ký/hủy đối xứng ở OnEnable/OnDisable.

### 🟢 3. ENEMY-SHOOTABLE: logic đúng, đã kiểm đủ các kịch bản yêu cầu
- `EnemyBrain`: `BeginTargetable/EndTargetable` có cờ `targetable` bảo vệ nên `AimStarted`/`AimEnded` đi cặp đúng 1 lần mỗi lần lộ (Peeking vượt ngưỡng -> Aiming -> Retreating dưới ngưỡng, hoặc Kill). Chết khi Peeking/Retreating: `Kill()` -> `EndTargetable()` -> `OnAimEnded` -> `TargetRegistry.Unregister` một lần; chết trước ngưỡng bị từ chối (`targetable=false`). `EnemyActor.OnDisable/OnEnable` đăng ký lại đúng khi tắt/bật giữa lúc lộ. `HumanShieldEnemy` đăng ký proxy con tin trong `OnTargetsRegistered` (gọi từ AimStarted) và hủy trong `OnTargetsUnregistered`/`OnKilled`: không rò. Có 9 test brain bao phủ các nhánh trên.
- Điểm phản xạ: Peeking cho `ReticleProgress = 0` (thưởng sớm tối đa, `ScoreCalculator` dòng 41; Justice còn được nhân "xanh" vì progress < GreenThreshold). Retreating giữ `ReticleProgress = 1` (không thưởng sớm). Hành vi nhất quán, nhưng là quyết định thiết kế: nhờ vậy bắn vào lúc vừa ló luôn được điểm cao nhất; nên để game-designer xác nhận đúng ý.
- Enemy đang rút vẫn đã bắn (Fired trước khi Retreating) và vẫn bị bắn: đúng yêu cầu; không tính mất mạng thêm.
- Justice point: `HasJusticePoint` không phụ thuộc state; điểm bám theo transform nên theo enemy lúc ló/rút. Ổn.
- Camera `TrackTargets` dùng `IsTargetable || ShowsReticle` nên bắt đầu lia sớm khi enemy lộ (đúng ý), kèm lọc `Grenade`, `trackMinDepth`, SmoothDamp 1 s. Lưu ý nhỏ: enemy đang rút vẫn được tính là mục tiêu để lia (vì còn targetable), có thể lia theo một enemy sắp biến mất; hiệu ứng nhẹ (trackMaxYaw 12°, 12°/s).
- `TargetReticlePresenter`: vòng chỉ hiện khi `ShowsReticle` (Aiming) nên không nháy lúc Peeking/Retreating; đăng ký sớm vẫn lấy View từ pool, được trả khi Unregister. Ổn.
- Body-hit collider: collider bật khi `PeekT >= targetableThreshold` (0.35) và tắt khi tụt dưới ngưỡng lúc rút/Kill. `BodyRect` chỉ dùng collider đang bật và hợp bounds toàn thân (AABB) nên tại PeekT = 0.35 phần thân còn khuất sau cover vẫn nằm trong vùng trúng. Không có kiểm tra che khuất (đã là hành vi của HIT-BODY ở đợt trước). Giữ nguyên nếu thiết kế chấp nhận; nếu thấy "bắn xuyên cover" khi test tay thì tăng `targetableThreshold` (hiện 0.35) hoặc dùng bounds thu hẹp.
- Gợi ý: `EnemyConfig_Default.asset` (và các asset EnemyConfig khác) chưa serialize trường `targetableThreshold` (grep không thấy), nên đang chạy bằng giá trị mặc định trong code 0.35; bật Inspector rồi lưu asset để thông số tinh chỉnh được nhìn thấy trong dữ liệu.
- `EncounterWave` chỉ phụ thuộc `Died`, không phụ thuộc state Aiming, nên Cleared vẫn đếm đúng. Cleared phát ở LateUpdate như cũ.

### 🟢 4. Combat HIT-FRONT
- `TargetSelector.Select`: nhóm hit được sắp xếp Justice trước rồi theo khoảng cách màn hình, sau đó với `maxHits == 1` mới đổi cho mục tiêu gần camera nhất (bỏ qua nếu `results[0].Justice`). Cắt `RemoveRange(maxHits)` làm sau bước hoán đổi nên không mất ứng viên. Đúng với mô tả "đạn không xuyên".
- `TapShooter`: `TargetRegistry.Unregistered` đăng ký ở OnEnable, hủy ở OnDisable, `bodyColliders.Clear()` khi tắt; cache theo `t.Id` và xóa khi Unregister (kể cả proxy con tin: xóa thừa vô hại). Cache `NoColliders` cho kết quả rỗng nên không cấp phát lại. Cấp phát `GetComponentsInChildren`/`ToArray` chỉ mỗi lần lộ, không mỗi frame. `CombatConfig` dùng hằng `Default*` thay cho số cứng: đúng quy tắc "không hard-code".
- Ghi nhận: với nearest-first, một enemy gần có `BodyRect` (AABB màn hình + padding 8 px) chứa điểm chạm sẽ giành phát bắn của enemy xa dù người chơi bấm sát đầu enemy xa. Đúng ý "đạn không xuyên" nhưng có thể gây cảm giác "bắn nhầm" khi 2 enemy chồng hình; kết hợp với việc enemy lộ sớm hơn (ENEMY-SHOOTABLE) tăng cơ hội chồng. Chấp nhận được; theo dõi khi test tay.

### 🟢 5. Map (MAP-FIX3/4), kiểm tra đo đạc qua file prefab/scene (chỉ đọc)
Tôi dựng script đọc YAML prefab (thế giới hóa Transform) để kiểm lại các số của level-designer:
- 18 `CamPoint_P{1..3}_S{1..6}` đúng tên, không thiếu/thừa. Tên trùng trong prefab chỉ là các nút nhóm cố ý (`Cover`, `RailHints`, `CamPoints`, `Peek`, `Spawns`, `Props`...); không có tên trùng ở `EnemySpawn_*`, `PropSlot_*`, `Cover_*` có nghĩa, `CamPoint_*`, `HostageSpawn_*`, `PickupSpawn_*`.
- Khoảng cách enemy/con tin từ CamPoint của đợt tới điểm Peek + 1.5 m: nhỏ nhất 12.9 m (EnemySpawn_P1_W6_01), kế tiếp 13.1 m: đạt ngưỡng >= 12 m. Khoảng cách tới vị trí nấp đều >= 12.7 m.
- Cặp Combat->Combat (hướng + vị trí CamPoint): P1 S2->S3 29.5°/0.64 m, P1 S5->S6 21.4°/0.73 m, P2 28.2°/0.72 m và 23.4°/0.86 m, P3 29.3°/0.96 m và 28.8°/0.57 m: khớp báo cáo, đều <= 30° và <= 1 m. Hai cặp P1_S2->S3 và P3_S2->S3 chỉ còn dư ~0.5-0.7° so với ngưỡng nên rất nhạy với 🟡 1 (nếu AutoFrame bắt đầu xoay).
- Rail đầu/cuối trùng shot kề: không kiểm lại số (cần đọc spline); báo cáo ghi sai số 0.000 m/0.0°. Điểm này nên được `DebugCamTrace` hoặc test xác nhận khi vào Play (chưa Play).
- Instance prefab `Level_01` trong scene (PrefabInstance &467437541) chỉ có override gốc (tên, active, transform gốc = 0): không có override thừa. Scene `Level_01` đang `isDirty=false`.
- `PropSlot_Barrel_P3_W3_01` (217.5, 0.6, 18.2): không đè collider cover nào (gần nhất `Skylight_B_Glass` cách ~0.8 m cạnh, `PickupSpawn_P3_W6_01` 2.5 m, box slot 3.7 m); nhưng xem 🟡 1 vì vật thể thật chưa dời.
- Khối mới và cover dời: không phát hiện cover chắn đường nhìn sai (báo cáo đã raycast 0 chặn); tôi không thể kiểm lại raycast ở chế độ chỉ đọc.

### 🟢 6. Ngoài phạm vi đợt này nhưng đáng ghi nhận (không chặn)
- Cây làm việc còn nhiều thay đổi chưa commit ngoài phạm vi: xóa module Jev (Scripts/Jev, JevConfig, JevSystems, JevDebugPanel), sửa chú thích "Jev" -> "RankScore" trong Core, 26 ảnh `Assets/Screenshots/MAP_*` chưa theo dõi, sandbox `gameplay-coder-props.unity` mới và hai sandbox scene (`gameplay-coder-enemy`, `ui-coder`) được thêm `SandboxEnemySpawner`/`TargetReticlePresenter` (chỉ sandbox, không vào build). Theo quyết định demo, không commit cho tới khi demo xong; khi commit nên tách ảnh chụp ra khỏi commit.
- `Level01Assembler.cs` vẫn gán `ShotEntry.Cut` cho shot S1 (vô hiệu từ nay). Level-designer đã cảnh báo không chạy lại menu Assemble (nó ghi đè frameTargets và tiếp tuyến ray); nên ghi chú vào chính menu hoặc cập nhật assembler.

---

## Việc cần làm
1. level-designer: sửa 🟡 1 (đồng bộ `Prop_Barrel_P3_W3_01` + `frameTargets` của `Shot_P3_S3_Combat` với `PropSlot`, tính lại FOV cần/lề 5° và góc cặp P3_S2->S3; cân nhắc đặt thùng gần trục nhìn hơn), rồi nhờ reviewer duyệt nhanh lại. Có thể thêm test so khớp Prop (scene) và PropSlot (prefab).
2. (tùy chọn) gameplay-coder: test EditMode cho `CameraPoseSmoother.Step`; lưu `targetableThreshold` vào các asset EnemyConfig.
3. Người dùng/PM: xác nhận hành vi điểm "bắn lúc vừa ló = thưởng sớm tối đa".
4. Chạy lại bộ test EditMode sau khi sửa (tôi chưa tự xác nhận được).
