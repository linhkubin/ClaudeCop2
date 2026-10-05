# REV-CAM-HIT-MAP — Review (full, chỉ đọc)

**Kết luận: APPROVED** (không có lỗi 🔴; 0 lỗi compile/runtime trong console; các mục 🟡 nên xử lý trước khi chốt demo).
Giới hạn: không vào Play mode nên nhịp camera được kiểm bằng đọc code + phân tích số liệu scene (script tạm trong scratchpad), chưa quan sát bằng mắt.

## 1. Camera (CAM-FIX)
Đối chiếu REV-CAM-GRENADE A1–A3:
- **A1 zoom-out sau khi tới: đã hết.** Applier giờ dùng trạng thái riêng mỗi camera (`armed/armTime`), `Arm()` gọi sau `WaitTransition` (PhaseDirector.cs:278, 331). Offset FOV chỉ là settle + push-in (zoom-in), không còn `CombatWeight` toàn cục nên camera rời live không tụt về FOV rộng. AutoFrame ép FOV Combat ≤ `min(maxVerticalFov, baseFov)` = 45 = FOV camera ray (CameraShot.cs:121,130) nên blend Move→Combat không zoom-out.
- **A2 giật sang khung khác rồi giật về: đã hết ở mức code.** `trackYaw` giữ khi camera không còn live (không nhả về 0), SmoothDamp 0.8 s / 20°/s, bỏ Grenade, dead-zone 1.5°, bỏ mục tiêu gần < 3 m. Blend được kéo dài theo `maxBlendSpeed/AngularSpeed` (ScaleBlend; `Prepare` đặt transform camera ray trước nên khoảng cách/góc đúng).
- **A3 nhịp:** settle 0.55 s (1.5°) → giữ khung → push-in 2.5° trong 8 s; cầm tay fade-in 0.6 s. Hợp lý.

Vấn đề:
- 🟡 `CameraShot.cs:121-130` / `PhaseDirector.cs:105`: nếu `need` > 45 sau khi lùi tối đa 2.5 m thì FOV bị kẹp 45 và một số điểm tràn khung (dọc không có cơ chế lia). Hiện map báo cần ≤ 44.3 (9:19.5) nên đạt; máy dài hơn (9:21+) sẽ tràn. Hướng: ghi log cảnh báo khi `need > limit` sau lùi, hoặc chấp nhận và ghi vào tài liệu.
- 🟡 `PhaseDirector.cs:105`: khi đổi aspect lúc shot đang live, shot đó bị bỏ qua và `framedAspect` đã cập nhật → không bao giờ được can khung lại cho aspect mới (AutoFramed=true nên Applier cũng không FitFov). Game khoá dọc nên ít xảy ra; hướng sửa: đánh dấu shot cần reframe và làm khi nó hết live.
- 🟡 Dữ liệu scene (không phải code): `Shot_P3_S2 → Shot_P3_S3` góc 89.8°, sát ngưỡng `cutAngle` 90 (sau AutoFrame yaw ±vài độ có thể thành Cut hay Blend tuỳ lần chạy). `Shot_P1_S3 → Rail_P1_S4`: tiếp tuyến đầu ray lệch hướng nhìn P1_S3 khoảng 106° ⇒ chắc chắn Cut cứng (đúng ý thiết kế "Cut" nhưng là một cú nhảy 106° giữa cảnh). Hướng sửa: để level-designer/gameplay-coder quyết định: hạ góc ≤ 85° để luôn Blend, hoặc chấp nhận Cut và đặt `entry=Cut` tường minh.
- 🟢 Combat→Combat: camera cũ đã push-in/settle (FOV hẹp hơn) blend sang camera mới chưa arm (FOV rộng hơn) ⇒ trong lúc blend FOV nới ra rồi settle lại thu vào. Nhẹ, không phải lỗi cũ.
- 🟢 `CameraFeelApplier.ResetFeel()` không ai gọi (OnDisable của PhaseDirector không reset). Nếu `StartLevel` chạy lại trong cùng scene thì mất settle. Không ảnh hưởng luồng hiện tại.
- 🟢 Shot dùng lại: `Arm` idempotent nên quay lại shot sau sub-angle giữ nguyên push-in (đúng ý). `trackYaw` per-camera không rò sang camera khác.

## 2. Bắn trúng thân (HIT-BODY)
Đúng yêu cầu: tap trong hình chữ nhật màn hình của collider enemy (8 góc AABB, padding 8px theo scale) thì trúng; Justice vẫn ưu tiên; Hostage/Pickup chỉ xét khi nhóm chính rỗng; enemy chưa lộ (`IsTargetable=false` hoặc collider tắt) bị bỏ. Chữ ký `Select` thêm tham số tuỳ chọn nên tương thích ngược; không đổi Core; có 3 test mới.
- 🟡 `TapShooter.cs:234-244`: cache `bodyColliders` theo `t.Id` không bao giờ dọn. Id enemy là `nextId++` duy nhất theo instance và enemy không dùng pool nên **không bị stale/sai**, nhưng dictionary tăng dần (mỗi enemy một mảng chứa Collider đã Destroy) suốt phiên; enemy không có collider (`Length==0`) thì tạo List+array mỗi tap. Hướng sửa: dọn khi `TargetRegistry` huỷ đăng ký (hoặc khi mục `cols[0]==null`/phát hiện hết sống), và cache cả trường hợp rỗng.
- 🟢 Con tin nhân bản (human shield): `ShieldHostageTarget` là class thường nên `GetComponentInParent<ITapTarget>` của collider trả về enemy; prefab Enemy_HumanShield chỉ có 1 CapsuleCollider (Body), HostageBody không có collider nên không lọt vào rect. Tap vào vùng con tin chồng lên rect enemy ⇒ trúng enemy (an toàn cho người chơi, đúng quy tắc "enemy trước con tin"). Cần xác nhận với game-designer đó là ý muốn.
- 🟢 Cover chắn: không có kiểm tra che khuất (giống hành vi bán kính AimPoint cũ); an toàn vì collider enemy tắt khi ẩn. Enemy ló một phần sau cover vẫn bắn được toàn rect — hợp với mục tiêu "bắn vào bất kỳ phần thân".
- 🟢 Mỗi tap không cấp phát (delegate cache ở Awake, `Rect?`/`Vector2?` là struct) trừ lúc cache miss.
- Ngoài phạm vi ghi nhận: `ResolveEnvironment` đổi sang `ShotClassifier` (Environment chỉ khi trúng IShootable, tường trơ = Miss reset combo) — có test `ShotClassifierTests`, cần PM biết đây là đổi hành vi.

## 3. Map (MAP-REARRANGE)
Kiểm bằng phân tích YAML scene/prefab (script tạm) + MCP:
- 18 CamPoint tên đúng (P1–P3 × S1–S6), 3 nhóm `CamPoints`; không có tên trùng trong cùng cha (các trùng như `Cover/Peek/Spawns` là tên nhóm theo từng Area; `Cover_ContainerRoof_W5` đã đổi). Scene chỉ có `m_Name` rỗng trùng (bình thường).
- **Ray khớp shot: đạt.** 6 ray: điểm cuối trùng vị trí shot kế tiếp 0.00 m và tiếp tuyến cuối (áp Rotation của knot) lệch hướng nhìn shot kế tiếp 0.0°; điểm đầu trùng shot trước 0.00 m (P1_S4, P2_S4, P3_S4). Đầu ray P1_S1/P2_S1/P3_S1 là đầu Phase (Cut) — cách shot trước 100 m là bình thường.
- **Trong khung FOV 45 dọc:** góc ngang các spawn so với trục shot nằm trong ±5.5° ở hầu hết shot; P2_S3 (-1.0..11.7°) và P2_S6 (-5.5..9.9°) lệch tâm nhưng AutoFrame xoay ≤ 12° để căn giữa nên vẫn vừa. `frameTargets` mỗi shot có số điểm khớp số spawn (+thùng). Báo cáo của designer nói "không xoay" hơi lạc quan cho 2 shot này (🟢).
- Console: chỉ có "NoSubscription" của Unity AI và 1 thông báo MCP; không có lỗi game.
- 🟢 Chưa kiểm bằng Play mode: xe/cover có chắn tầm nhìn tap không (không liên quan raycast vì tap chọn theo màn hình), SPIKE ở đoạn blend (designer đã đề nghị chạy DebugCameraProbe).
- 🟢 P2_S4 (~6.4 s) và P3_S4 (~6.0 s) vượt `maxMoveSeconds` 5.4 do `maxRailSpeed`=4 (designer đã báo); việc của gameplay-coder/PM.
- Cảnh báo quy trình: **không chạy lại** `Assemble Level_01 (T-403)` (sẽ ghi đè rail/frameTargets).

## Việc cần làm
- gameplay-coder: dọn cache `bodyColliders` (🟡), xử lý/ghi nhận reframe khi đổi aspect (🟡), cân nhắc cảnh báo khi FOV bị kẹp.
- level-designer/PM: quyết định góc P3_S2→S3 sát 90° và Cut 106° ở P1_S3→P1_S4 (🟡).
- Người dùng: chạy Play một vòng Phase 1 (và DebugCameraProbe) để xác nhận bằng mắt A1–A3.
