# Thiết kế 10 level: truy bắt đội cướp ngân hàng

> BẢN NHÁP thiết kế (2026-10-05), chưa dựng level nào. Người dùng sẽ chỉnh bối cảnh và mô tả chi tiết từng phase trước khi triển khai.

## Cốt truyện (một mạch xuyên 10 level)
Băng cướp **Rắn Đỏ** đột nhập Ngân hàng Trung ương, bắt con tin, khoan két rồi tẩu thoát: ngân hàng → hầm xe → kho tiền → cống ngầm → ga tàu → bến cảng → tòa tháp → sân đáp trực thăng. Người chơi (cảnh sát) bám theo một mạch tới thủ lĩnh.

## Độ khó, số phase, thời lượng (đã chốt)
| Độ khó | Level | Số phase | Thời lượng một lượt chơi |
|---|---|---|---|
| Easy | 1, 2, 3, 4, 6, 7, 9 | 3 | 120–150 s |
| Normal | 8 | 4 | 150–180 s |
| Hard | 5 | 5 | 180–240 s |
| Boss | 10 | 5 | khoảng 300 s |
Thời lượng đi theo hoàn thành sự kiện; mốc giây trong storyboard chỉ là dự kiến, không chờ đồng hồ nếu người chơi đã làm xong. Boss level 10: thủ lĩnh Rắn Đỏ **nhiều giai đoạn, đổi vị trí** (cần thêm code boss).

## Nhịp chơi chung của mỗi level (người dùng cung cấp)
- **Đầu 15–25%:** dùng mẫu đã biết hoặc giới thiệu **duy nhất một luật mới**, thường một mục tiêu tại một thời điểm.
- **Giữa 40–50%:** thay đổi hướng xuất hiện, cao độ, chiều sâu, nhân vật dân thường hoặc mục tiêu ưu tiên. Sau một bất ngờ lớn có **nhịp giảm tải 2–4 giây** gắn với hành động kể chuyện/reload.
- **Cuối:** một bài kiểm tra ngắn kết hợp những gì đã học, rồi giải tỏa. **Không thêm luật mới bắt buộc ở những giây cuối.**

### Áp vào số phase
- Easy (3): P1 mở đầu/luật mới · P2 bất ngờ giữa + giảm tải · P3 bài kiểm tra + giải tỏa.
- Normal (4): P1 mở đầu · P2 phát triển · P3 bất ngờ giữa + giảm tải · P4 kiểm tra + giải tỏa.
- Hard/Boss (5): P1 mở đầu · P2 phát triển · P3 bất ngờ giữa + giảm tải · P4 kết hợp luật · P5 kiểm tra cuối (boss: giao chiến thủ lĩnh).
- Mỗi phase có vài way nhỏ gần khung cảnh.

## Bối cảnh 10 level + luật mới (đề xuất, chờ người dùng chỉnh)
| Lv | Độ khó | Bối cảnh liền mạch | Các phase | Luật mới đề xuất ở đầu level |
|---|---|---|---|---|
| 1 | Easy | Mặt tiền ngân hàng (9 giờ sáng) | Vỉa hè và xe cảnh sát · Ngõ hông, bãi đỗ xe · Bậc thềm, cửa chính | Chạm vào vòng target (một enemy mỗi lần) |
| 2 | Easy | Sảnh giao dịch | Cửa xoay, tiếp tân · Dãy quầy giao dịch · Khu chờ, cầu thang | Con tin: không bắn nhầm |
| 3 | Easy | Tầng lửng, văn phòng | Hành lang · Phòng họp kính · Phòng an ninh camera | Justice shot: bắn trúng súng trên tay (cao độ nhiều tầng đã có từ level 1) |
| 4 | Easy | Hầm xe, kho hậu cần | Cầu thang hầm · Bãi hầm xe · Kho hậu cần | Thùng nổ |
| 5 | Hard | Kho tiền | Hành lang an ninh · Phòng két ký gửi · Đại sảnh vault · Phòng đếm tiền · Lõi kho vàng | Khiên người |
| 6 | Easy | Cống ngầm thoát hiểm | Cửa hầm cháy · Kênh cống ngập · Ngã ba hầm bảo trì | Thuốc nổ ném (phải bắn rơi) |
| 7 | Easy | Ga tàu điện ngầm | Sảnh vé · Sân ga · Đường ray, tàu hàng | Nhặt thùng súng (shotgun) |
| 8 | Normal | Bến cảng, bãi container | Cổng cảng · Container chồng tầng · Nhà kho lạnh · Cầu cảng, cần cẩu | Súng máy, nhiều mục tiêu cùng lúc |
| 9 | Easy | Tòa tháp tài chính (đi lên) | Sảnh tháp · Thang thoát hiểm, hành lang kính · Sân thượng phụ | Kính vỡ và vật che động |
| 10 | Boss | Sân đáp trực thăng đỉnh tháp | Cửa lên mái · Giàn mái, anten · Bãi đáp, xe chở tiền · Trực thăng cất cánh · Giao chiến thủ lĩnh | Không luật mới; kết hợp tất cả |
Tổng 35 phase.

## Yếu tố hài hước (cảnh dựng sẵn một lần, không gây áp lực)
Quy tắc chung: không cộng kill/accuracy/điểm/combo, không làm đứt combo; cập nhật số enemy còn lại và điều kiện kết thúc wave ngay; không bù enemy; không trong lúc camera đang chuyển hoặc trước warning đầy đủ; không dùng ở màn con tin boss cuối; bỏ qua/huỷ nếu người chơi hạ mục tiêu trước hoặc bắn nhầm dân.
- **H01 — Ba tên nhảy xuống, một tên dập mặt tự chết.** Ba tay súng nhảy từ gác thấp; hai tên tiếp đất, tên thứ ba vướng lan can, chúi mặt, nằm bất động, mất súng. Tự ngã = kết thúc do tai nạn, không tính kill; vẫn trong quota tối đa ba enemy; nếu người chơi hạ tên đó trước thì kill bình thường và huỷ nhánh ngã. Hai tên còn lại bắn theo luật thường sau warning đầy đủ.
- **H02 — Cứu xong, chạy quá đà đâm tường.** Con tin được cứu giơ tay cảm ơn, chạy cuống, ngoái lại vẫy rồi đâm vào cạnh tường, ngã ngồi, đứng dậy phủi áo, chạy đúng cửa. Chỉ sau khi cứu thành công và khu an toàn; không trừ máu/điểm, không tính civilian hit; dân sống và rời cảnh, hoạt ảnh kết thúc hữu hạn.
- **H03 — Enemy chạy ra, vấp rồi trượt qua chỗ núp.** Tên chạy từ cửa ra định núp, vấp mép sàn/dây rỗng, ngã sấp, trượt quá chỗ núp, đứng dậy nhặt súng rồi ngắm. Thân lộ rộng lúc loạng choạng là cơ hội bắn hợp lệ; còn sống (khác H01); không tấn công khi đang ngã; bị hạ thì tắt hit/attack và bỏ động tác nhặt súng; dựng sẵn một lần, không reset được.
- **H08 — Trượt sàn ướt (level 6).** Một tên cướp chạy dọc bờ kênh cống ướt, trượt dài, đâm vào ống rồi rơi xuống nước và không trở lại. Kết thúc do tai nạn như H01: không tính kill/điểm/combo, cập nhật số enemy còn lại ngay, nằm trong quota tối đa cùng lúc, không bù enemy; nếu người chơi hạ hắn trước khi trượt thì kill bình thường và huỷ nhánh trượt. Chỉ diễn khi camera không đang chuyển, hai tên còn lại bắn sau warning đầy đủ. Các gag H04–H07, H09–H13 từng được đề xuất nhưng **không giữ**.
Đề xuất phân bổ (nháp): H02 ở level 2 sau khi cứu con tin đầu; H01 ở level 3 (phòng họp kính hoặc cầu thang); H03 ở level 4 (hầm xe) hoặc level 8 (container). Mỗi level Easy 1 gag, Normal/Hard 2 gag, Boss 0–1 gag. Còn nhiều chỗ trống để người dùng thêm gag mới.

## Việc kỹ thuật sẽ cần khi triển khai (chưa làm)
- Hệ thống cảnh dựng sẵn (scripted gag) với kết quả "tai nạn": không kill/score/combo, cập nhật wave ngay, nằm trong quota.
- Trạng thái enemy mới: ngã/trượt/đứng dậy (hoạt ảnh nhân vật primitive đơn giản), civilian chạy và đâm tường.
- Cấu trúc dữ liệu level: danh sách phase, way, độ khó, thời lượng mục tiêu, luật mới ở đầu level; boss nhiều giai đoạn có đổi vị trí.
- Level_01 hiện có 3 phase (đường phố – kho hàng – mái nhà) sẽ phải dựng lại thành mặt tiền ngân hàng.

## Đã chốt (vòng hỏi 1)
- Tông chung: hành động nghiêm túc, thỉnh thoảng một cảnh hài.
- Chuyển level: camera chạy tiếp một mạch sang bối cảnh kế (không cắt); kết quả (điểm, rank) hiện sau khi camera đã đi thêm một đoạn tới cảnh bắt đầu của level sau (xem vòng hỏi 6–7).
- Hết máu: xem quảng cáo để hồi sinh tại đúng vị trí và chơi tiếp; thoát ra thì chơi lại từ đầu level đó (dùng RevivePopup có sẵn).
- Boss thủ lĩnh Rắn Đỏ: 3 giai đoạn đổi vị trí.

## Đã chốt (vòng hỏi 2)
- Boss 3 giai đoạn: (1) nấp sau xe tiền → (2) chạy lên trực thăng → (3) đối đầu ở mép bãi đáp.
- Enemy dùng chung: lính thường, ném thuốc nổ, khiên người; chỉ đổi trang phục/màu theo bối cảnh.
- Chấm điểm: rank S/A/B/C như hiện tại (RankScore), ngưỡng riêng theo độ khó và thời lượng mỗi level.
- Vũ khí: mỗi level bắt đầu bằng Pistol (không mang sang level sau). Về sau sẽ làm cửa hàng để nâng cấp súng. **Súng nhặt trong level không reload được; bắn hết đạn thì quay về súng mặc định** (cần chỉnh TapShooter: hiện súng nhặt vẫn reload được).

## Đã chốt (vòng hỏi 3)
| Độ khó | Enemy tối đa cùng lúc | Thời gian vòng target | Con tin/dân thường mỗi level |
|---|---|---|---|
| Easy | 2 | 2.5 s | 2–3 |
| Normal | 3 | 2.0 s | 4 |
| Hard | 3–4 | 1.6 s | 5–6 |
| Boss | 3–4 | 1.4 s | 5–6 |
- Enemy có thể xuất hiện **lần lượt** hoặc **dồn dập** (đợt dồn dập vẫn tôn trọng số tối đa cùng lúc).
- Máu: 3 tim, không hồi giữa level (luật hiện tại).
- Con tin chủ yếu xuất hiện ở giữa level (nhịp bất ngờ) và là mục tiêu ưu tiên.

## Đã chốt (vòng hỏi 4)
- Cảnh hài: H02 ở level 2, H01 ở level 3, H03 ở level 4 (H03 dùng lại ở container level 8).
- Level 5 (Hard): điểm nhấn giữa level = cửa kho bị đóng, cướp dùng khiên người, khiên là con tin nhân viên ngân hàng.
- Bất ngờ mặc định giữa level Easy: con tin xuất hiện xen giữa enemy. **Ngoại lệ level 1:** con tin là luật mới của level 2, nên level 1 dùng "enemy xuất hiện từ trên cao / xa bất ngờ" làm bất ngờ giữa level.

## Storyboard Level 1 — Mặt tiền ngân hàng (Easy, 3 phase, dự kiến ~135 s)
Luật mới: chạm đúng vòng target, một enemy mỗi lần. Chưa có con tin, thùng nổ, thùng súng, khiên người, thuốc nổ ném (các luật này nằm ở level 2–7). Đạn: Pistol. Enemy tối đa cùng lúc: 2. Vòng target 2.5 s. Vật trang trí (thùng rác, xe, kính) không nổ.

| Mốc (dự kiến) | Phase / shot | Sự kiện | Mục đích nhịp |
|---|---|---|---|
| 0–6 s | Đường vào: xe cảnh sát → vỉa hè (Move) | Mở đầu buổi sáng nắng, còi cảnh sát, đèn xoay của xe cảnh sát. | Giới thiệu |
| 6–18 s | **P1 vỉa hè**, góc trái | W1: 1 enemy ló sau thùng rác, biểu tượng bàn tay chạm vào vòng đỏ (không chữ). | Luật mới (15–25%) |
| 18–28 s | P1, góc phải | W2: 1 enemy sau xe van. | Lặp mẫu, một mục tiêu |
| 28–40 s | P1 | W3: 2 enemy lần lượt (không đồng thời): một bậc thấp rồi một sau cột. | Nâng nhẹ |
| 40–46 s | Move sang ngõ hông | Cảnh sát nạp đạn khi dựa tường. | Giảm tải ngắn |
| 46–58 s | **P2 ngõ hông**, góc phải | W1: 1 enemy gần sau xe van (chiều sâu gần). | Chuẩn bị |
| 58–72 s | P2, mở rộng khung | W2 (**bất ngờ giữa level 40–50%**): 2 enemy dồn dập — một từ cửa sổ tầng 2 (cao), một ở cuối ngõ (xa). | Đổi cao độ và chiều sâu |
| 72–76 s | P2 | Giảm tải 3–4 s: cửa chính bị xe tải chắn, camera lia nhìn lối bậc thềm; có thời gian reload. | Nhịp giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt sau thùng và cột. | Củng cố |
| 88–94 s | Move ra bậc thềm | Camera lướt qua bãi đỗ xe tới sân trước cổng. | Chuyển cảnh |
| 94–110 s | **P3 bậc thềm**, góc thấp | W1: 2 enemy ở sân thấp (gần/ xa), lần lượt. | Bài kiểm tra bắt đầu |
| 110–124 s | P3, góc cao lên bậc thềm | W2: 3 enemy dồn dập (tối đa 2 cùng lúc): bậc thấp, bậc cao, cạnh cửa. | Kết hợp cao độ, chiều sâu |
| 124–135 s | P3, cửa chính | W3: enemy cuối bước ra cửa chính, kill-zoom rồi cửa bật mở; camera chạy tiếp vào sảnh (sang level 2). | Giải tỏa. Không luật mới |

Tổng enemy 15 (P1 4, P2 5, P3 6; P2 giảm tải: wave đồng loạt chỉ 2 enemy, nhịp dwell 3–4 s không encounter ở giữa); bắn trong khoảng 50 s, di chuyển khoảng 25 s, nhịp kể chuyện/giảm tải khoảng 12 s; còn lại là chờ ló ra. Mốc giây chỉ là dự kiến, hoàn thành sự kiện thì chuyển ngay.
Cảnh hài: không có ở level 1 (H02 ở level 2).
Rank mẫu (chưa chốt): thang theo độ chính xác, số lần bị trúng, thời gian hoàn thành so với mốc dự kiến.

## Storyboard Level 2 — Sảnh giao dịch (Easy, 3 phase, ~150 s)
Luật mới: **con tin, không bắn nhầm**. Con tin 3. Cảnh hài H02 (hoạt cảnh chạy đâm cột chưa làm, hiện con tin chạy đi bình thường). Enemy tối đa 2 cùng lúc (wave cuối phase 3: 3), vòng target 2.5 s, không Justice.
Mỗi phase 4 wave (shot S2, S3, S5, S6; S1 và S4 là Move). **Enemy đứng sẵn (S)**: đặt sẵn trong scene, đứng lộ ở chỗ trống, không ló ra từ chỗ nấp; vào thẳng Ngắm khi wave kích hoạt (EnemyActor.SceneStanding). **E** = ló ra từ chỗ nấp.
Camera Combat dùng FOV dọc tối thiểu hẹp hơn Level 1 (S2 40, S3 36, S5 38, S6 32 = kill-zoom; Level 1 đặt 58–60 nên luôn bị chặn ở 45), cụm mục tiêu hẹp (<= ~10 độ) nên zoom thực tế khoảng 32–42 độ.
| Phase / shot | Sự kiện | Enemy | Nhịp |
|---|---|---|---|
| Move S1 | Qua cửa xoay vào sảnh (nối từ level 1) | 0 | Giới thiệu |
| **P1 cửa xoay, tiếp tân** S2 | W1: 1 S đứng trước quầy tiếp tân + 1 E sau quầy | 2 | Mẫu quen |
| P1 S3 | W2: **con tin đầu tiên** nhô sau chậu cây, 2 enemy sau máy ATM cách xa rõ; biểu tượng con tin nhấp nháy, không bắn | 2 + 1 con tin | **Luật mới** |
| Move S4 | Đi dọc sảnh | 0 | Chuyển |
| P1 S5 | W3: 1 S đứng cạnh quầy thông tin | 1 | Giảm tải |
| P1 S6 | W4: 2 E (chậu cây, quầy tiếp tân), kill-zoom; con tin chạy ra cửa (H02 chạy đâm cột dự kiến) | 2 | Hài, giải tỏa |
| Move S1 | Chuông báo động, camera tới dãy quầy giao dịch | 0 | Chuyển cảnh |
| **P2 dãy quầy** S2 | W1: 1 E sau kính quầy + 1 S đứng trước quầy | 2 | Củng cố |
| P2 S3 | W2: 1 E sau quầy giữa | 1 | Nhịp thở |
| Move S4 | Trượt ngang dọc dãy quầy | 0 | Chuyển |
| P2 S5 | W3 (**bất ngờ giữa level**): 1 E + 1 S đứng + 1 con tin xen giữa, phải phân biệt | 2 + 1 con tin | Con tin xen enemy |
| P2 S6 | W4: 1 E + 1 S, kill-zoom; đèn đỏ báo động nhấp nháy | 2 | Giải tỏa |
| Move S1 | Tới khu chờ, cầu thang đôi hiện ra | 0 | Chuyển cảnh |
| **P3 khu chờ, cầu thang** S2 | W1: 1 E trên lan can tầng lửng (cao) + 1 S đứng ở khu ghế chờ (thấp) | 2 | Kiểm tra |
| P3 S3 | W2: 1 E lan can + 1 E sau ghế | 2 | Cao/thấp |
| Move S4 | Tiến thêm một đoạn | 0 | Chuyển |
| P3 S5 | W3: 2 enemy (1 E sau ghế, 1 S đứng) + **con tin thứ hai** ngồi ghế giữa | 2 + 1 con tin | Kết hợp con tin |
| P3 S6 | W4: 3 enemy cuối (lan can, ghế, chân cầu thang; 1 S), kill-zoom; camera sẵn sàng đi lên tầng lửng | 3 | Giải giảo |
Tổng enemy **23** (P1 7, P2 7, P3 9; trong đó 8 đứng sẵn), con tin 3 (tính theo thiết kế mới: 1 mỗi phase), tối đa 2–3 enemy cùng lúc. Level 1 có 13–15 enemy.
Ghi chú: con tin thứ 3 ban đầu trong storyboard cũ (P2 W3) đã gộp vào con tin P3; mỗi phase đúng một con tin.

## Storyboard Level 3 — Tầng lửng và văn phòng (Easy, 3 phase, ~135 s)
Luật mới: **Justice shot** (bắn trúng súng trên tay enemy để hạ gọn, thưởng điểm). Con tin 2–3. Cảnh hài H01 là bất ngờ giữa level.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move lên tầng lửng | Nối từ level 2 | Giới thiệu |
| 6–16 s | **P1 hành lang văn phòng** | W1: 1 enemy ló khỏi cửa văn phòng (mẫu quen) | Mẫu quen |
| 16–28 s | P1 | W2: 1 enemy cầm súng lộ rõ, biểu tượng ngắm sáng vào khẩu súng trên tay | **Luật mới** |
| 28–40 s | P1 | W3: 2 enemy lần lượt, thử Justice shot | Củng cố |
| 40–46 s | Move vào phòng họp kính | | Chuyển cảnh |
| 46–56 s | **P2 phòng họp kính** | W1: 1 enemy sau cửa kính, 2 con tin ngồi quanh bàn họp (không bắn) | Con tin quen |
| 56–72 s | P2 | W2 (**bất ngờ giữa level**, **H01**): ba tay súng nhảy từ gác thấp, hai tên tiếp đất, tên thứ ba vướng lan can, chúi mặt, nằm bất động mất súng; hai tên còn lại bắn sau warning đầy đủ | Hài và đổi cao độ |
| 72–76 s | P2 | Giảm tải: chuông thang máy, reload | Giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt sau tủ hồ sơ | Củng cố |
| 88–94 s | Move tới phòng an ninh | | Chuyển cảnh |
| 94–112 s | **P3 phòng an ninh camera** | W1: 2 enemy, có Justice shot; 1 con tin bị trói ghế | Kiểm tra |
| 112–128 s | P3 | W2: 3 enemy dồn dập, tối đa 2 cùng lúc | Kết hợp |
| 128–135 s | P3 | W3: enemy cuối trước màn hình camera, kill-zoom, cửa hầm bật mở | Giải tỏa |
Tổng enemy khoảng 14 (kể cả 1 tên tự ngã không tính kill), con tin 3.

## Storyboard Level 4 — Hầm xe và kho hậu cần (Easy, 3 phase, ~140 s)
Luật mới: **thùng nổ** (bắn thùng để nổ hạ cả nhóm; thùng nổ luôn cách con tin ≥3.5 m). Con tin 2. Cảnh hài H03.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move xuống cầu thang thoát hiểm | Nối từ level 3 | Giới thiệu |
| 6–16 s | **P1 cầu thang xuống hầm** | W1: 1 enemy ở chân cầu thang | Mẫu quen |
| 16–28 s | P1 | W2: 1 enemy đứng cạnh thùng đỏ, thùng đỏ phát sáng nhấp nháy | **Luật mới** |
| 28–40 s | P1 | W3: thùng nổ giữa 2 enemy lần lượt, nổ hạ cả hai | Củng cố |
| 40–46 s | Move vào bãi hầm xe | Đèn pha nhấp nháy | Chuyển cảnh |
| 46–58 s | **P2 bãi hầm xe** | W1: 1 enemy sau xe, 1 con tin sau trụ | Mẫu quen |
| 58–72 s | P2 | W2 (**bất ngờ giữa level**, **H03**): enemy chạy ra từ cửa, vấp mép sàn, trượt qua chỗ núp, đứng dậy nhặt súng; con tin xen giữa ở xe bọc thép | Hài và con tin xen |
| 72–76 s | P2 | Giảm tải: xe bọc thép nổ máy, reload | Giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt, 1 thùng nổ giữa bãi | Củng cố |
| 88–95 s | Move tới kho hậu cần | | Chuyển cảnh |
| 95–113 s | **P3 kho hậu cần** | W1: enemy trên kệ cao + thùng nổ dưới kệ | Kiểm tra |
| 113–130 s | P3 | W2: 3 enemy dồn dập, tối đa 2 cùng lúc, con tin cuối ở xa | Kết hợp |
| 130–140 s | P3 | W3: enemy cuối trước cửa kho tiền, kill-zoom, cửa kho mở (nối level 5) | Giải tỏa |
Tổng enemy khoảng 14, con tin 2.

## Storyboard Level 5 — Kho tiền (Hard, 5 phase, ~210 s)
Luật mới: **khiên người** (kẻ cướp kẹp con tin làm khiên; bắn vào tay cầm súng hoặc đầu enemy lộ ra). Max 3–4 enemy, vòng target 1.6 s, con tin 5–6.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move vào hành lang an ninh | Nối từ level 4 | Giới thiệu |
| 6–22 s | **P1 hành lang cửa vault** | W1: 1 enemy; W2: 1 enemy khiên người, vòng sáng ở tay cầm súng của enemy | **Luật mới** |
| 22–40 s | P1 | W3: 2 enemy lần lượt, 1 con tin nhân viên | Củng cố |
| 40–48 s | Move vào phòng két ký gửi (hai tầng) | | Chuyển cảnh |
| 48–84 s | **P2 phòng két ký gửi** | W1: 2 enemy ở tầng thấp, **thùng shotgun** rơi vào khung (thưởng tùy chọn, không có gợi ý); W2: enemy tầng cao + 1 khiên người; W3: 3 enemy dồn dập, con tin xen | Phát triển |
| 84–92 s | Move vào đại sảnh vault | Cửa kho đóng sầm | Chuyển cảnh |
| 92–112 s | **P3 đại sảnh vault** | W1 (**bất ngờ giữa level**): cửa kho bị đóng, hai tên dùng hai khiên người đều là nhân viên ngân hàng; ưu tiên cứu | Khiên đôi |
| 112–118 s | P3 | Giảm tải: chuông cửa vault, reload | Giảm tải |
| 118–140 s | P3 | W2: 3 enemy + 1 con tin xen; W3: thùng nổ trong đại sảnh, không gần con tin | Củng cố |
| 140–148 s | Move tới phòng đếm tiền | | Chuyển cảnh |
| 148–182 s | **P4 phòng đếm tiền** | Kết hợp: khiên người, Justice shot, thùng nổ, 3–4 enemy; con tin kế toán; **booster giáp** ở đầu phase (nhặt trước bài kiểm tra dồn dập ở P5) và hộp hồi máu | Kết hợp luật |
| 182–190 s | Move tới lõi kho vàng | | Chuyển cảnh |
| 190–210 s | **P5 lõi kho vàng** | W1: 3 enemy dồn dập; W2: đội trưởng có khiên người; kill-zoom, cửa vault vòng mở, tiền sáng, giải tỏa | Bài kiểm tra cuối |
Tổng enemy khoảng 26, con tin 6.

## Storyboard Level 6 — Cống ngầm thoát hiểm (Easy, 3 phase, ~130 s)
Luật mới: **thuốc nổ ném** (grenadier ném, bắn rơi thuốc nổ trước khi chạm đất). Con tin 2. Nước, đèn khẩn cấp, đường ống.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move vào cửa hầm bị cháy | Nối từ level 5 | Giới thiệu |
| 6–16 s | **P1 cửa hầm cháy** | W1: 1 enemy | Mẫu quen |
| 16–28 s | P1 | W2: 1 grenadier, thuốc nổ bay có vệt sáng | **Luật mới** |
| 28–40 s | P1 | W3: 1 grenadier + 1 enemy lần lượt | Củng cố |
| 40–46 s | Move xuống kênh cống | | Chuyển cảnh |
| 46–58 s | **P2 kênh cống ngập** | W1: 2 enemy hai bờ, lần lượt | Củng cố |
| 58–72 s | P2 | W2 (**bất ngờ giữa level**, **H08**): 3 enemy dồn dập từ hai bờ (tối đa 2 cùng lúc), một tên chạy trên bờ ướt trượt dài, đâm ống, rơi xuống nước (tự thoát, không tính kill); grenadier trên cầu đi bộ (cao), 1 con tin bị trói trên cầu xen giữa | Hài, cao độ, con tin xen |
| 72–76 s | P2 | Giảm tải: đèn pin, nước chảy, reload | Giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt | Củng cố |
| 88–94 s | Move tới ngã ba hầm bảo trì | | Chuyển cảnh |
| 94–110 s | **P3 ngã ba hầm bảo trì** | W1: 2 grenadier lần lượt + thùng nổ trên ống | Kiểm tra |
| 110–124 s | P3 | W2: 3 enemy dồn dập, 1 con tin | Kết hợp |
| 124–130 s | P3 | W3: enemy cuối chạy vào cổng hầm, tiếng tàu điện; cổng mở nối ga tàu | Giải tỏa |
Tổng enemy khoảng 13, con tin 2.

## Storyboard Level 7 — Ga tàu điện ngầm (Easy, 3 phase, ~135 s)
Luật mới: **thùng súng** (bắn thùng nhận shotgun, bắn chùm nhiều mục tiêu). Súng nhặt **không reload**; hết đạn quay về Pistol. Con tin 2.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move vào sảnh vé | Nối từ level 6 | Giới thiệu |
| 6–16 s | **P1 sảnh vé, cổng soát vé** | W1: 1 enemy | Mẫu quen |
| 16–30 s | P1 | W2: thùng súng shotgun rơi vào khung, thùng súng phát sáng | **Luật mới** |
| 30–42 s | P1 | W3: 2 enemy đứng gần nhau, shotgun hạ cả hai | Củng cố |
| 42–48 s | Move ra sân ga | | Chuyển cảnh |
| 48–60 s | **P2 sân ga** | W1: 2 enemy lần lượt sau cột | Củng cố |
| 60–74 s | P2 | W2 (**bất ngờ giữa level**): tàu vào ga, cửa mở, 3 enemy + 1 con tin xen xuống ga | Đổi hướng và con tin xen |
| 74–78 s | P2 | Giảm tải: thông báo ga, reload | Giảm tải |
| 78–90 s | P2 | W3: grenadier trên băng ghế, 1 enemy | Củng cố |
| 90–96 s | Move xuống đường ray | | Chuyển cảnh |
| 96–114 s | **P3 đường ray, tàu hàng** | W1: enemy trên toa tàu (cao), thùng nổ giữa đường ray | Kiểm tra |
| 114–128 s | P3 | W2: 3 enemy dồn dập, 1 con tin trong toa | Kết hợp |
| 128–135 s | P3 | W3: enemy cuối nhảy khỏi tàu hàng, tàu chạy qua mở lối ra bến cảng | Giải tỏa |
Tổng enemy khoảng 14, con tin 2.

## Storyboard Level 8 — Bến cảng và bãi container (Normal, 4 phase, ~165 s)
Luật mới: **súng máy** (thùng MG; giữ ngón để bắn liên tục) và **nhiều mục tiêu cùng lúc**. Enemy tối đa 3, vòng target 2.0 s, con tin 4. Cảnh hài H03 lặp lại ở bãi container.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move ra cổng cảng | Nối từ level 7 | Giới thiệu |
| 6–36 s | **P1 cổng cảng, trạm hải quan** | W1: 1 enemy; W2: thùng súng máy, biểu tượng ngón tay giữ; W3: 3 enemy dồn dập thử súng máy | **Luật mới** |
| 36–42 s | Move vào bãi container | | Chuyển cảnh |
| 42–76 s | **P2 bãi container chồng tầng** | W1: enemy ba tầng container (cao độ); W2: grenadier + 2 enemy; con tin 1 xen giữa | Phát triển |
| 76–82 s | Move vào nhà kho lạnh | | Chuyển cảnh |
| 82–122 s | **P3 nhà kho lạnh** | W1 (**bất ngờ giữa level**, **H03**): enemy chạy ra từ cửa lạnh, vấp dây rỗng, trượt qua chỗ núp; 2 con tin ngồi xen giữa; W2: khiên người ở cuối kho; W3: 3 enemy dồn dập | Hài, con tin xen, khiên |
| 98–102 s | P3 | Giảm tải: hơi lạnh bay, tiếng quạt, reload | Giảm tải |
| 122–130 s | Move ra cầu cảng | | Chuyển cảnh |
| 130–165 s | **P4 cầu cảng và cần cẩu** | Bài kiểm tra: enemy trên cần cẩu (cao), thuốc nổ ném, thùng nổ trên tàu hàng, 3 enemy dồn dập, 1 con tin; enemy cuối ngã khỏi tàu, kill-zoom, camera đi tiếp tới tòa tháp | Giải tỏa |
Tổng enemy khoảng 20, con tin 4.

## Storyboard Level 9 — Tòa tháp tài chính, đi lên (Easy, 3 phase, ~140 s)
Luật mới: **kính vỡ và vật che động** (vách kính che enemy; bắn vỡ để lộ rồi hạ). Con tin 3.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move vào sảnh tháp | Nối từ level 8 | Giới thiệu |
| 6–16 s | **P1 sảnh tháp** | W1: 1 enemy | Mẫu quen |
| 16–30 s | P1 | W2: 1 enemy sau vách kính, vết nứt sáng trên kính | **Luật mới** |
| 30–42 s | P1 | W3: 2 enemy lần lượt sau vách kính | Củng cố |
| 42–48 s | Move vào thang thoát hiểm | | Chuyển cảnh |
| 48–62 s | **P2 thang thoát hiểm, hành lang kính** | W1: 2 enemy lần lượt từ cầu thang | Củng cố |
| 62–76 s | P2 | W2 (**bất ngờ giữa level**): vách kính vỡ lộ 2 enemy; con tin xen giữa ở phía sau kính | Con tin xen |
| 76–80 s | P2 | Giảm tải: gió thổi qua kính vỡ, reload | Giảm tải |
| 80–92 s | P2 | W3: grenadier + 1 enemy | Củng cố |
| 92–98 s | Move lên sân thượng phụ | | Chuyển cảnh |
| 98–116 s | **P3 sân thượng phụ, giàn điều hòa** | W1: 2 enemy sau giàn điều hòa, thùng nổ cạnh giàn | Kiểm tra |
| 116–132 s | P3 | W2: 3 enemy dồn dập, khiên người xen giữa | Kết hợp |
| 132–140 s | P3 | W3: enemy cuối chạy về cửa mái, kill-zoom, cửa mái mở (nối level 10) | Giải tỏa |
Tổng enemy khoảng 14, con tin 3.

## Storyboard Level 10 — Sân đáp trực thăng đỉnh tháp (Boss, 5 phase, ~300 s)
Không luật mới; kết hợp tất cả. Max 3–4 enemy, vòng target 1.4 s, con tin 5–6. Thủ lĩnh Rắn Đỏ **3 giai đoạn** đổi vị trí (số máu/điểm yếu mỗi giai đoạn chưa chốt). Không cảnh hài (hoặc tối đa một cảnh nhỏ nếu bạn muốn).
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move lên cửa mái | Nối từ level 9 | Giới thiệu |
| 6–52 s | **P1 cửa lên mái** | Mẫu quen: enemy, con tin, Justice shot, khiên người; 3 enemy dồn dập cuối phase | Mở đầu, nhắc lại luật |
| 52–62 s | Move ra giàn mái | Gió mạnh, trực thăng đậu xa dưới nắng chiều | Chuyển cảnh |
| 62–120 s | **P2 giàn mái, bồn nước, anten** | Phát triển: grenadier, thùng nổ ở bồn, enemy trên anten (cao), con tin xen | Phát triển |
| 120–128 s | Move ra bãi đáp | Thấy xe chở tiền và trực thăng | Chuyển cảnh |
| 128–192 s | **P3 bãi đáp, xe chở tiền** | **Giai đoạn 1 (bất ngờ giữa level):** thủ lĩnh nấp sau xe chở tiền, chỉ hé ra ngắn, đồng bọn ném thuốc nổ; 2 con tin ở xe. Sau khi thủ lĩnh bị thương lần 1, hắn bỏ chạy | Boss giai đoạn 1 |
| 192–198 s | P3 | Giảm tải: thủ lĩnh quát lớn (giọng không lời), tiền giấy bay, reload | Giảm tải |
| 198–206 s | Move tới trực thăng | | Chuyển cảnh |
| 206–256 s | **P4 trực thăng cất cánh** | **Giai đoạn 2:** thủ lĩnh chạy lên trực thăng, cánh quạt quay, đồng bọn trên trực thăng ném thuốc nổ, khiên người; bắn hạ đồng bọn để hạ trực thăng | Boss giai đoạn 2 |
| 256–264 s | Move tới mép bãi đáp | Trực thăng trục trặc | Chuyển cảnh |
| 264–300 s | **P5 mép bãi đáp** | **Giai đoạn 3:** thủ lĩnh dùng con tin làm khiên ở mép bãi đáp; Justice shot, bắn đúng điểm yếu; kill-zoom, giải tỏa: nắng chiều, còi, kết thúc chuỗi 10 level | Boss giai đoạn 3, giải tỏa |
Tổng enemy khoảng 30, con tin 6.

## Đã chốt (vòng hỏi 5)
- Các câu hỏi mở (máu boss, ngưỡng rank, thùng súng nhỏ, phân bổ con tin từng way) **tính sau**.
- Thêm **hộp hồi máu** (bắn để nhận +1 tim, tối đa 3 tim) ở một số level. Máu vẫn không hồi tự động giữa các level; chỉ hồi bằng hộp trong level.
- **Liền mạch giữa các level:** cảnh kết thúc của level này chính là cảnh bắt đầu của level sau (cùng vị trí, hướng nhìn, bối cảnh); riêng level 10 (boss) là kết thúc chuỗi.

### Hộp hồi máu (đề xuất vị trí)
Hộp xanh có dấu chữ thập, rơi vào khung ở nhịp giảm tải hoặc đầu phase kế; tồn tại khoảng 8 s hoặc đến hết phase; không đặt gần thùng nổ/khiên người; luôn nhìn thấy rõ, không che enemy. Hiện chưa có trong code (cần thêm vật phẩm như thùng súng và API hồi tim).
| Level | Số hộp | Vị trí |
|---|---|---|
| 1, 2, 3, 7 | 0 | Easy giai đoạn học luật |
| 4 | 1 | Đầu P3 (kho hậu cần) |
| 5 | 2 | Sau giảm tải P3; đầu P4 |
| 6 | 1 | Giảm tải P2 (kênh cống) |
| 8 | 1 | Giảm tải P3 (kho lạnh) |
| 9 | 1 | Đầu P3 (sân thượng phụ) |
| 10 | 2 | Cuối P2 (trước bãi đáp); đầu P4 (trước trực thăng) |

### Điểm nối giữa các level (kết thúc → bắt đầu)
| Nối | Cảnh kết thúc level trước | Cảnh bắt đầu level sau |
|---|---|---|
| 1 → 2 | Enemy cuối bước ra cửa chính, cửa bật mở, camera đi vào | Cửa chính vừa mở, camera vào cửa xoay sảnh |
| 2 → 3 | Enemy cuối chạy lên cầu thang, camera theo lên tầng lửng | Camera đã ở cầu thang, đi tiếp vào hành lang văn phòng |
| 3 → 4 | Cửa hầm sau phòng an ninh bật mở | Từ cửa hầm xuống cầu thang thoát hiểm |
| 4 → 5 | Cửa kho tiền mở sau khi hạ enemy cuối | Camera qua cửa kho tiền vào hành lang an ninh |
| 5 → 6 | Enemy cuối ngã, tường sau lõi kho vàng trượt mở lối thoát hầm bí mật | Camera qua lối đó tới cửa hầm bị cháy (cống ngầm) |
| 6 → 7 | Cổng hầm mở nối ga tàu | Camera ra khỏi cổng, vào sảnh vé |
| 7 → 8 | Tàu hàng chạy qua, mở lối ra cảng | Camera từ lối ra tới cổng cảng |
| 8 → 9 | Enemy cuối ngã khỏi tàu, camera đi tiếp tới tòa tháp | Camera tới sảnh tòa tháp |
| 9 → 10 | Cửa mái mở | Camera lên cửa mái, thấy bãi đáp |
| 10 | Kết thúc chuỗi (nắng chiều, còi) | — |
Kỹ thuật (chưa làm): bảng kết quả (điểm, rank) hiện lên trong lúc camera chạy đoạn Move cuối; cần cách nối level liền mạch (cùng scene hoặc nạp scene kế trước khi hết level), tư thế camera cuối level n trùng tư thế đầu level n+1.

- Level 5 có **thùng shotgun thưởng** ở P2, nhặt tùy chọn, không có gợi ý. **Không có thùng súng máy** ở level 5. Súng nhặt không reload, hết đạn quay về Pistol. Level 7 (shotgun) và level 8 (súng máy) vẫn là nơi giới thiệu chính thức, có biểu tượng gợi ý và dùng trong bài kiểm tra bắt buộc.
- Level 5 có **1 booster giáp** ở đầu P4 (phase trước phase dồn dập P5): bắn để nhận, giáp chặn **1 lần bị trúng** (không cộng vào 3 tim, mất khi dùng hoặc khi chết/hồi sinh). Biểu tượng gợi ý ngắn khi xuất hiện lần đầu. Hiện chưa có trong code (cần vật phẩm giáp và API chặn sát thương trong PlayerHealth).

## Mô tả chi tiết bối cảnh từng level (toàn bộ ban ngày)
Quy ước chung: portrait; camera ở độ cao mắt người (1.6–1.7 m); enemy cách camera ≥12 m; mỗi cụm vừa khung khoảng 21° ngang; enemy xếp theo ba tầng độ cao (thấp / ngang / cao). Vật che gồm vật che cứng (cột, tường, xe) và vật che vỡ được (kính, thùng carton). Thời gian trong ngày chạy một mạch từ sáng đến chiều muộn; mọi level đều có ánh sáng ban ngày (nắng trực tiếp hoặc ánh sáng ngày qua cửa kính, giếng trời, miệng cống), không có cảnh đêm. Ngoại lệ duy nhất: level 5 nằm sâu dưới đất nên dùng đèn, vẫn sáng đều.
Mỗi phase mô tả: **Không gian** (bố cục), **Camera** (đường và khung), **Che chắn và cao độ**, **Diễn biến** (theo way), **Hợp lý** (lý do hợp lý hoặc lưu ý).

### Level 1 — Mặt tiền ngân hàng (9 giờ sáng, nắng nhẹ)
Không khí: phố tài chính buổi sáng, nắng chiếu xiên làm bóng đổ dài, dải phong tỏa vàng-đen của cảnh sát, người dân đã được sơ tán. Màu: xanh trời, kem đá của ngân hàng, đỏ-xanh xe cảnh sát. Âm thanh: còi, chim bồ câu bay lên.
- **P1 vỉa hè và xe cảnh sát.** *Không gian:* đường phố rộng khoảng 12 m, vỉa hè rộng 3 m, hai xe cảnh sát nằm chéo chắn đường, dải phong tỏa. *Camera:* đi thẳng từ sau xe cảnh sát ra vỉa hè rồi dừng ở góc trái. *Che chắn:* thùng rác lớn bên trái (thấp), xe van bên phải (ngang), trụ biển bus. *Diễn biến:* W1 1 cướp ló sau thùng rác; W2 1 cướp sau xe van; W3 2 cướp ló lần lượt ở bậc thấp và sau cột. *Hợp lý:* nắng từ sau lưng người chơi nên mặt cướp luôn sáng, dễ nhận ra; chưa có cao độ để học luật nhẹ nhàng.
- **P2 ngõ hông và bãi đỗ xe.** *Không gian:* ngõ rộng 6 m giữa ngân hàng và tòa nhà kế bên, tường gạch, thang thoát hiểm sắt, một xe van và một xe tải nhỏ, bãi đỗ nhỏ phía sau. *Camera:* lướt dọc ngõ rồi dừng ở khoảng trống đầu bãi đỗ. *Che chắn:* xe van (gần), thùng carton (vỡ được), cửa sổ tầng 2 và thang sắt (cao), cuối ngõ (xa). *Diễn biến:* W1 1 cướp sau xe van; W2 bất ngờ: 1 cướp ở cửa sổ tầng 2 và 1 cướp cuối ngõ cùng ló; giảm tải 3–4 s khi camera lia nhìn lối bậc thềm khi cửa chính bị xe tải chắn; W3 2 cướp lần lượt sau thùng và cột. *Hợp lý:* nắng chỉ chiếu một nửa ngõ, nửa còn lại là bóng râm; đặt enemy ở nửa có nắng để dễ thấy.
- **P3 bậc thềm và cửa chính.** *Không gian:* sân trước 15 m, 8 bậc thềm đá, bốn cột đá, cửa kính chính có rèm sắt kéo nửa chừng, bồn hoa hai bên. *Camera:* lên sân trước rồi dừng thấp, sau đó nâng lên góc cao hơn ở bậc thềm. *Che chắn:* bồn hoa (thấp), cột đá (ngang), bậc thềm cao (cao), cạnh cửa. *Diễn biến:* W1 2 cướp ở sân thấp lần lượt; W2 3 cướp dồn dập (tối đa 2 cùng lúc) ở bậc thấp, bậc cao, cạnh cửa; W3 cướp cuối bước ra cửa, kill-zoom, cửa bật mở. *Hợp lý:* bốn cột đá tạo nhịp ló ra rõ ràng; cửa mở là điểm nối sang sảnh level 2.

### Level 2 — Sảnh giao dịch (giữa buổi sáng, ánh sáng tràn qua cửa kính lớn)
Không khí: sảnh đá cẩm thạch rộng, ánh nắng qua vách kính mặt tiền tạo vệt sáng dài trên sàn, đèn báo động đỏ nhấp nháy ở trần. Màu: trắng đá, nâu gỗ quầy, đỏ báo động. Âm thanh: chuông báo động, tiếng dội của sảnh.
- **P1 cửa xoay và tiếp tân.** *Không gian:* cửa xoay kính, bàn tiếp tân dài 6 m, quầy hướng dẫn, chậu cây lớn, máy ATM. *Camera:* qua cửa xoay chậm rồi dừng giữa sảnh nhìn bàn tiếp tân. *Che chắn:* bàn tiếp tân (thấp), chậu cây (ngang). *Diễn biến:* W1 1 cướp sau bàn tiếp tân; W2 con tin đầu tiên nhô khỏi quầy, cướp cách xa rõ, có biểu tượng gợi ý; W3 cướp kề con tin, hạ cướp thì H02 (con tin chạy đâm cột). *Hợp lý:* cột sảnh nằm phía trước nên H02 diễn ra mà không chắn camera.
- **P2 dãy quầy giao dịch.** *Không gian:* sáu quầy kính liền nhau, ô giao dịch hẹp, cột vuông giữa các quầy, biển số quầy. *Camera:* trượt ngang dọc dãy quầy, dừng ở góc nghiêng khoảng 25°. *Che chắn:* kính quầy (vỡ được), cột vuông (cứng). *Diễn biến:* W1 2 cướp ló sau kính; W2 bất ngờ: 2 cướp và 1 con tin xen giữa, phải phân biệt; giảm tải: đèn đỏ nhấp nháy; W3 1 cướp và con tin ở xa. *Hợp lý:* nhân viên ngân hàng nấp sau quầy nên có sẵn con tin.
- **P3 khu chờ và cầu thang giữa.** *Không gian:* hàng ghế chờ, máy rút số, cầu thang đá xoắn đôi lên tầng lửng, lan can kính. *Camera:* tiến một đoạn, dừng ngang ghế chờ rồi ngẩng nhẹ tới lan can. *Che chắn:* ghế chờ (thấp), cột (ngang), lan can (cao). *Diễn biến:* W1 cướp ở lan can và cướp ở ghế chờ; W2 3 cướp dồn dập, 1 con tin ngồi ghế; W3 cướp cuối chạy lên cầu thang, camera theo lên. *Hợp lý:* cầu thang giữa là lối duy nhất lên tầng lửng nên nối level 3 hợp lý.

### Level 3 — Tầng lửng và văn phòng (cuối buổi sáng, ánh sáng ngày qua tường kính)
Không khí: tầng hành chính trên sảnh, tường kính nhìn xuống sảnh, giấy tờ rải rác, ánh sáng trắng của ban ngày. Màu: xám văn phòng, gỗ ấm. Âm thanh: máy photocopy, thang máy, tiếng bước chân vang.
- **P1 hành lang văn phòng.** *Không gian:* hành lang dài khoảng 20 m, hai dãy cửa văn phòng, tủ hồ sơ, máy lọc nước. *Camera:* tiến thẳng dọc hành lang rồi dừng gần giữa. *Che chắn:* khung cửa (ngang), tủ hồ sơ (thấp). *Diễn biến:* W1 cướp ló từ cửa; W2 cướp cầm súng lộ rõ, biểu tượng gợi ý Justice shot; W3 2 cướp lần lượt. *Hợp lý:* các cánh cửa lần lượt mở tạo nhịp ló ra tự nhiên.
- **P2 phòng họp kính.** *Không gian:* phòng họp tường kính, bàn họp dài, ghế xoay, màn hình, nhìn ra gác thấp (mezzanine). *Camera:* vào cửa kính rồi dừng ở góc cao nhìn xuống bàn họp. *Che chắn:* bàn họp, tủ hồ sơ, lan can gác thấp (cao). *Diễn biến:* W1 1 cướp ở cửa kính, 2 con tin ngồi bàn; W2 H01 (ba tay súng nhảy xuống, một tên dập mặt); giảm tải: chuông thang máy; W3 2 cướp sau tủ. *Hợp lý:* gác thấp đủ cao để nhảy xuống mà không nguy hiểm; kính vỡ được nên cẩn thận hướng bắn gần con tin.
- **P3 phòng an ninh camera.** *Không gian:* phòng nhỏ, tường đầy màn hình sáng, bàn điều khiển, tủ server. *Camera:* vào cửa rồi dừng thấp nhìn bàn điều khiển. *Che chắn:* bàn điều khiển, tủ server. *Diễn biến:* W1 2 cướp có Justice shot, 1 con tin bị trói ghế; W2 3 cướp dồn dập; W3 cướp cuối trước màn hình, cửa hầm bật mở. *Hợp lý:* phòng an ninh là nơi có lối xuống hầm kỹ thuật.

### Level 4 — Hầm xe và kho hậu cần (đầu giờ chiều, ánh sáng ngày lọt qua lối dốc)
Không khí: tầng hầm bê tông, ánh sáng ngày lọt qua lối dốc ra vào, đèn tuýp nhấp nháy, ống thông gió. Màu: xám bê tông, vàng đèn, đỏ thùng nổ. Âm thanh: nước nhỏ giọt, động cơ xe bọc thép, tiếng vang.
- **P1 cầu thang thoát hiểm xuống hầm.** *Không gian:* cầu thang bê tông, đèn thoát hiểm xanh, cửa sắt sơn đỏ. *Camera:* đi xuống và dừng ở chân cầu thang. *Che chắn:* tay vịn, tường, thùng đỏ cạnh tường. *Diễn biến:* W1 cướp ở chân cầu thang; W2 cướp cạnh thùng đỏ, biểu tượng gợi ý; W3 thùng nổ giữa 2 cướp lần lượt. *Hợp lý:* thùng đỏ là thùng nhiên liệu của máy phát điện, nằm xa con tin.
- **P2 bãi hầm xe.** *Không gian:* cột vuông lớn, xe sedan đỗ chéo, một xe bọc thép của ngân hàng chắn lối, biển P-1. *Camera:* tiến vào giữa bãi rồi dừng chéo. *Che chắn:* cột (cứng), xe (cứng), cửa nhỏ bên trái. *Diễn biến:* W1 cướp sau xe, con tin sau trụ; W2 H03 và con tin xen ở xe bọc thép; giảm tải: xe bọc thép nổ máy; W3 2 cướp lần lượt và thùng nổ. *Hợp lý:* xe bọc thép là xe chuyển tiền của ngân hàng nên có ở hầm.
- **P3 kho hậu cần.** *Không gian:* kệ thép cao 3 m, pallet thùng hàng, xe nâng, thùng xăng đỏ, cửa kho tiền xám ở cuối. *Camera:* tiến dọc lối kệ rồi dừng chéo. *Che chắn:* kệ cao (cao), pallet (thấp), thùng đỏ. *Diễn biến:* W1 cướp trên kệ và thùng nổ dưới kệ; W2 3 cướp dồn dập, con tin cuối ở xa; W3 cướp cuối trước cửa kho tiền, cửa kho mở. *Hợp lý:* kệ cao cho độ cao tự nhiên.

### Level 5 — Kho tiền (đèn trắng, kho ngầm)
Không khí: khu bảo mật ngầm, ánh đèn trắng lạnh, kim loại bóng, tia laser trang trí; không có ánh ngày vì nằm sâu dưới đất. Màu: bạc, xanh thép, vàng thỏi. Âm thanh: cửa kim loại đóng sầm, nhịp hồi hộp.
- **P1 hành lang an ninh cửa vault.** *Không gian:* hành lang hẹp 5 m, tường thép, camera trần, cửa chớp. *Camera:* tiến chậm vào hành lang, dừng giữa. *Che chắn:* khe cửa chớp, hốc tường. *Diễn biến:* W1 1 cướp; W2 khiên người đầu tiên ở cuối hành lang, biểu tượng gợi ý; W3 2 cướp lần lượt và 1 nhân viên. *Hợp lý:* cướp cần nhân viên mở cửa vault nên có khiên người.
- **P2 phòng két ký gửi (hai tầng).** *Không gian:* hàng tủ két hai tầng, cầu thang sắt, sàn lưới thép. *Camera:* lướt qua hàng két rồi dừng giữa. *Che chắn:* tủ két (cứng), sàn lưới (tầng cao). *Diễn biến:* W1 2 cướp tầng thấp và thùng shotgun; W2 cướp tầng cao và khiên người; W3 3 cướp dồn dập, con tin xen. *Hợp lý:* sàn lưới thép cho cao độ; thùng shotgun gần cầu thang.
- **P3 đại sảnh vault.** *Không gian:* sảnh tròn, cửa vault khổng lồ ở giữa, hai bục cao, xe đẩy tiền. *Camera:* vào sảnh rồi dừng góc rộng nhìn cửa vault. *Che chắn:* bục (cao), xe đẩy (thấp), cột. *Diễn biến:* W1 cửa vault đóng sầm, hai khiên người là nhân viên; giảm tải: chuông cửa; W2 3 cướp và 1 con tin xen; W3 thùng nổ xa con tin. *Hợp lý:* vault đóng để gài bẫy là hợp tình huống.
- **P4 phòng đếm tiền.** *Không gian:* bàn đếm dài, máy đếm, tủ lạnh nhỏ, cửa sổ quan sát. *Camera:* vào cửa rồi dừng chéo. *Che chắn:* bàn, tủ, cửa sổ quan sát (cao). *Diễn biến:* kết hợp khiên người, Justice shot, thùng nổ; booster giáp và hộp hồi máu ở đầu phase; kế toán bị trói. *Hợp lý:* phase chuẩn bị trước bài kiểm tra nên có vật phẩm.
- **P5 lõi kho vàng.** *Không gian:* hầm vàng, thỏi vàng xếp thành bậc, tường thép. *Camera:* tiến chậm rồi dừng ở trung tâm. *Che chắn:* bậc vàng, cột thép. *Diễn biến:* W1 3 cướp dồn dập; W2 đội trưởng khiên người; kill-zoom, tường sau trượt mở lối thoát. *Hợp lý:* tường mở là lối thoát bí mật nối cống ngầm.

### Level 6 — Cống ngầm thoát hiểm (giữa trưa, nắng rọi qua lưới sắt)
Không khí: đường hầm cống cũ, tia nắng buổi trưa rọi xuống qua miệng cống và lưới sắt tạo cột bụi sáng, hơi nước, đám cháy nhỏ ở mảnh vỡ. Màu: xanh rêu, cam lửa, vệt nắng vàng. Âm thanh: nước chảy, lách tách lửa.
- **P1 cửa hầm cháy.** *Không gian:* cửa hầm sắt vừa bị phá, mảnh vỡ, lửa nhỏ, ống cống tròn lớn. *Camera:* qua cửa hầm rồi dừng giữa ống. *Che chắn:* mảnh vỡ, vòm ống. *Diễn biến:* W1 1 cướp; W2 grenadier đầu tiên, biểu tượng gợi ý; W3 grenadier và cướp lần lượt. *Hợp lý:* lửa từ vụ nổ khi cướp phá lối thoát.
- **P2 kênh cống ngập.** *Không gian:* kênh rộng 8 m, hai bờ đi bộ, cầu đi bộ sắt bắc ngang. *Camera:* đi bờ trái rồi dừng nhìn kênh. *Che chắn:* bờ trái và phải, cầu (cao), ống lớn. *Diễn biến:* W1 2 cướp hai bờ; W2 H08 và con tin bị trói trên cầu; giảm tải: ánh đèn pin; W3 2 cướp lần lượt. *Hợp lý:* tia nắng qua miệng cống làm bờ ướt phản chiếu, hợp với H08.
- **P3 ngã ba hầm bảo trì.** *Không gian:* ba cửa hầm, ống chằng chịt, thùng nổ trên ống. *Camera:* tới ngã ba rồi dừng nhìn ba cửa. *Che chắn:* cửa hầm, ống. *Diễn biến:* W1 2 grenadier lần lượt và thùng nổ; W2 3 cướp và 1 con tin; W3 cướp cuối chạy vào cổng hầm. *Hợp lý:* ngã ba có sẵn thùng bảo trì.

### Level 7 — Ga tàu điện ngầm (đầu giờ chiều, đã sơ tán, ánh sáng ngày qua giếng trời)
Không khí: ga vắng sau khi sơ tán, ánh sáng ngày lọt xuống qua giếng trời giữa sân ga, đèn huỳnh quang trắng, quảng cáo, biển điện tử. Màu: trắng gạch, xanh biển báo, đỏ biển cấm. Âm thanh: loa thông báo, tàu xa.
- **P1 sảnh vé và cổng soát vé.** *Không gian:* dãy cổng soát vé, máy bán vé, sơ đồ tuyến, quầy thông tin. *Camera:* đi chậm tới giữa sảnh. *Che chắn:* cổng soát vé (thấp), máy bán vé (ngang). *Diễn biến:* W1 1 cướp; W2 thùng shotgun sau máy bán vé, biểu tượng gợi ý; W3 2 cướp gần nhau. *Hợp lý:* đường hầm cống nối vào sảnh phụ của ga.
- **P2 sân ga.** *Không gian:* sân ga dài, cột lớn, ghế chờ, biển điện tử, tàu vào ga. *Camera:* theo mép sân ga rồi dừng. *Che chắn:* cột (cứng), ghế, băng ghế. *Diễn biến:* W1 2 cướp sau cột; W2 tàu vào ga, 3 cướp và 1 con tin xen từ toa; giảm tải: thông báo ga; W3 grenadier trên băng ghế và 1 cướp. *Hợp lý:* tàu cuối được giữ lại ga nên cướp trốn trên đó.
- **P3 đường ray và tàu hàng.** *Không gian:* đường ray, tàu hàng đỗ, container, tín hiệu. *Camera:* xuống đường ray rồi dừng nhìn toa tàu. *Che chắn:* toa tàu (cao), container, ray. *Diễn biến:* W1 cướp trên toa và thùng nổ giữa ray; W2 3 cướp dồn dập, 1 con tin; W3 cướp cuối nhảy khỏi tàu, tàu hàng chạy qua mở lối. *Hợp lý:* tàu hàng chạy ra cảng nên nối level 8.

### Level 8 — Bến cảng và bãi container (giữa trưa, nắng gắt)
Không khí: cảng hàng hóa dưới nắng trưa gắt, bóng container đổ ngắn, mặt bê tông phản chiếu, gió biển, cần cẩu khổng lồ. Màu: xám thép, cam rỉ sét, xanh biển. Âm thanh: còi tàu, dây cáp, sóng.
- **P1 cổng cảng và trạm hải quan.** *Không gian:* cổng sắt, thanh chắn, chòi hải quan, xe tải. *Camera:* qua cổng rồi dừng nhìn chòi hải quan. *Che chắn:* chòi, xe tải, thanh chắn. *Diễn biến:* W1 1 cướp; W2 thùng súng máy, biểu tượng gợi ý; W3 3 cướp dồn dập thử súng máy. *Hợp lý:* chòi hải quan là điểm chặn tự nhiên.
- **P2 bãi container chồng tầng.** *Không gian:* container rỉ chồng ba tầng, lối đi hẹp, xe nâng container. *Camera:* đi dọc lối rồi dừng nhìn lên. *Che chắn:* container (cứng, ba tầng cao độ), xe nâng. *Diễn biến:* W1 cướp ở ba tầng; W2 grenadier tầng hai, 2 cướp và con tin bị trói cạnh container. *Hợp lý:* ba tầng container cho đủ cao độ; nắng gắt làm bóng ngắn nên enemy dễ nhìn.
- **P3 nhà kho lạnh.** *Không gian:* kho lạnh, hơi lạnh bay, kệ thép phủ sương, cửa kho nặng. *Camera:* vào cửa kho rồi dừng chéo. *Che chắn:* kệ, cửa kho, pallet. *Diễn biến:* W1 H03 và 2 con tin xen; giảm tải: hơi lạnh, tiếng quạt; W2 khiên người ở cuối kho; W3 3 cướp dồn dập. *Hợp lý:* cửa kho lạnh dày nên có chỗ ló ra.
- **P4 cầu cảng và cần cẩu.** *Không gian:* cầu cảng bê tông, tàu hàng neo, cần cẩu giàn cao, thùng hàng trên tàu. *Camera:* ra cầu cảng rồi dừng nhìn cần cẩu. *Che chắn:* thùng hàng, bích neo, cần cẩu (cao). *Diễn biến:* cướp trên cần cẩu, grenadier, thùng nổ trên tàu, 3 cướp dồn dập, 1 con tin; cướp cuối ngã khỏi tàu. *Hợp lý:* cầu cảng giáp cổng hàng hóa của tòa tháp nên nối level 9.

### Level 9 — Tòa tháp tài chính (đầu chiều, kính phản chiếu nắng)
Không khí: tòa tháp kính cao nhìn ra sông và cảng, nắng chiều dội lên kính gây chói nhẹ (giữ chói dưới mức khó nhìn), sảnh cao hai tầng. Màu: xanh kính, trắng kim loại, vàng nắng. Âm thanh: gió, thang máy, tiếng nứt kính.
- **P1 sảnh tháp.** *Không gian:* sảnh cao hai tầng, vách kính lớn, quầy lễ tân đá đen, tác phẩm điêu khắc. *Camera:* vào từ cổng hàng hóa nối cầu cảng, dừng giữa sảnh. *Che chắn:* vách kính (vỡ được), quầy đá. *Diễn biến:* W1 1 cướp; W2 cướp sau vách kính, biểu tượng gợi ý; W3 2 cướp sau vách lần lượt. *Hợp lý:* cổng hàng hóa của tháp nối thẳng với cầu cảng.
- **P2 thang thoát hiểm và hành lang kính.** *Không gian:* thang bê tông, hành lang kính nhìn ra sông. *Camera:* lên thang rồi dừng đầu hành lang kính. *Che chắn:* vách kính, lan can. *Diễn biến:* W1 2 cướp từ cầu thang; W2 kính vỡ lộ 2 cướp, con tin sau kính; giảm tải: gió qua kính vỡ; W3 grenadier và 1 cướp. *Hợp lý:* kính vỡ thật nên cần chú ý hướng bắn với con tin.
- **P3 sân thượng phụ và giàn điều hòa.** *Không gian:* sân thượng phụ, giàn điều hòa lớn, ống dẫn. *Camera:* ra sân thượng rồi dừng. *Che chắn:* giàn điều hòa, ống. *Diễn biến:* W1 2 cướp và thùng nổ; W2 3 cướp và khiên người; W3 cướp cuối chạy tới cửa mái. *Hợp lý:* sân thượng phụ nối cửa mái bằng cầu thang ngắn.

### Level 10 — Sân đáp trực thăng đỉnh tháp (chiều muộn, nắng vàng cam)
Không khí: đỉnh tháp lộng gió, nắng chiều muộn vàng cam chiếu nghiêng, bóng dài, trực thăng đen đậu trên bãi đáp. Màu: đen trực thăng, vàng cam nắng, đỏ cảnh báo. Âm thanh: gió, cánh quạt, nhạc đạt đỉnh.
- **P1 cửa lên mái.** *Không gian:* cánh cửa mái, cầu thang nhỏ. *Camera:* lên cầu thang rồi dừng sát cửa. *Che chắn:* khung cửa, thùng kỹ thuật. *Diễn biến:* mẫu quen: cướp, con tin, Justice shot, khiên người; 3 cướp dồn dập cuối phase. *Hợp lý:* mở màn nhắc lại luật.
- **P2 giàn mái, bồn nước, anten.** *Không gian:* bồn nước to, giàn anten, dây cáp, lan can thấp. *Camera:* đi giữa giàn rồi dừng. *Che chắn:* bồn nước, giàn anten (cao). *Diễn biến:* grenadier sau bồn, cướp trên giàn, thùng nổ cạnh bồn, con tin xen; hộp hồi máu cuối phase. *Hợp lý:* giàn anten tạo cao độ tự nhiên.
- **P3 bãi đáp và xe chở tiền.** *Không gian:* bãi đáp rộng, xe điện chở hàng nhỏ chất thùng tiền (đưa lên bằng thang máy hàng), vạch bãi đáp. *Camera:* ra bãi đáp rồi dừng. *Che chắn:* xe chở tiền (cứng), thùng tiền. *Diễn biến:* giai đoạn 1: thủ lĩnh nấp sau xe, đồng bọn ném thuốc nổ; sau khi bị thương lần 1 hắn bỏ chạy; giảm tải. *Hợp lý:* xe điện nhỏ lên được bằng thang máy hàng.
- **P4 trực thăng cất cánh.** *Không gian:* trực thăng đen, cánh quạt quay. *Camera:* tiến tới trực thăng rồi dừng. *Che chắn:* thân trực thăng, bánh. *Diễn biến:* giai đoạn 2: thủ lĩnh lên trực thăng, đồng bọn trên thân ném thuốc nổ, khiên người; hộp hồi máu đầu phase. *Hợp lý:* gió cánh quạt làm bụi mù nhưng không che khung nhìn.
- **P5 mép bãi đáp.** *Không gian:* mép bãi đáp không rào chắn, thành phố và sông phía dưới. *Camera:* tiến tới mép rồi dừng. *Che chắn:* thủ lĩnh dùng con tin làm khiên. *Diễn biến:* giai đoạn 3: Justice shot và điểm yếu; kill-zoom; kết thúc: nắng chiều, còi cảnh sát. *Hợp lý:* kết thúc chuỗi 10 level.

### Đánh giá tính hợp lý (đã rà)
- **Mạch thời gian:** sáng (level 1) đến chiều muộn (level 10), mỗi level có ánh sáng ngày; level 5 nằm sâu dưới đất nên dùng đèn, là ngoại lệ duy nhất.
- **Không gian liền mạch:** 1→2 (cửa chính), 2→3 (cầu thang), 3→4 (hầm kỹ thuật), 4→5 (cửa kho tiền), 5→6 (tường trượt mở lối cống), 6→7 (cổng hầm vào ga), 7→8 (tàu hàng ra cảng), 8→9 (cầu cảng nối cổng hàng hóa tòa tháp), 9→10 (cửa mái).
- **Rủi ro cần kiểm khi dựng:** kính vỡ gần con tin (level 2, 3, 9); thùng nổ phải cách con tin ≥3.5 m; nắng chói trên kính level 9; bóng râm làm khó nhìn enemy ở ngõ level 1, nên đặt enemy ở vùng có nắng; số enemy cùng lúc không vượt trần theo độ khó.

## Đã chốt (vòng hỏi 6–7)
- **Cứu con tin** = hạ kẻ canh con tin; con tin tự chạy khỏi cảnh; cứu được thì cộng điểm rank; bắn nhầm con tin mất tim và reset combo.
- **Không có tính năng radio** và không có lời thoại chữ/phụ đề trong cảnh. Gợi ý luật mới bằng biểu tượng, hiệu ứng sáng và hoạt ảnh; nhịp giảm tải dùng âm thanh môi trường, chuyển động camera và reload. UI điểm/rank/nút giữ như hiện có.
- **Hồi sinh bằng quảng cáo:** tối đa 2 lần mỗi level, hồi sinh tại đúng vị trí; không ảnh hưởng rank (rank chỉ phụ thuộc điểm và độ chính xác). Thoát ra thì chơi lại từ đầu level đó.
- **Kết thúc level:** camera đi thêm một đoạn tới cảnh bắt đầu của level sau, rồi hiện bảng kết quả dừng chờ bấm "Tiếp tục"; camera đứng ở cảnh đầu level sau, thở nhẹ trong lúc chờ. Level 10 kết thúc chuỗi, không có đoạn đi thêm.

### Quy tắc chuyển stage (vòng hỏi 8, áp dụng cho tất cả stage)
- Giả định: "stage" = phase (P1, P2, P3...) trong một level. Nếu bạn muốn nói cả chuyển giữa các level thì quy tắc này đã khớp với quy tắc kết thúc level ở trên.
- Kết thúc stage này, nhân vật **di chuyển liền mạch** tới stage kế và bắt đầu ngay: **không tối màn hình, không fade đen, không cắt cảnh**. Đoạn Move cuối của mỗi stage phải kết thúc đúng vị trí và hướng nhìn của shot đầu stage kế (đã là quy tắc rail).
- Khi bắt đầu stage mới vẫn **hiện chữ stage** (tiêu đề) như hiện nay, nhưng hiện chồng lên cảnh đang chạy, không chặn gameplay và không cần màn hình đen.
- Đây là ngoại lệ có chủ đích của quy tắc "không dùng chữ": tiêu đề stage vẫn dùng chữ.
- Hệ quả: không dùng nhảy xa kiểu cũ giữa các phase (đoạn Cut 100 m); mọi stage phải nối bằng rail liên tục. Code hiện có `PhaseDirector` và `PhaseFade` đang fade đen khi đổi phase, cần đổi thành tiêu đề không fade.

### Lưu dữ liệu và dây level (về sau)
- Lưu dữ liệu từng level: trạng thái mở, rank tốt nhất, điểm cao nhất, số lần chơi.
- Về sau nối thành "dây level" và hiện hạng từng level để chơi lại; menu chọn level chưa làm trong đợt này.

## Câu hỏi mở
- Người dùng chỉnh tên/thứ tự/luật mới của 10 bối cảnh nếu muốn.
- Phân bổ gag H01–H03 và gag mới (nếu có).
- Tông cảnh và chi tiết từng phase (loại/số enemy, con tin, thùng nổ, thùng vũ khí, way nhỏ).
