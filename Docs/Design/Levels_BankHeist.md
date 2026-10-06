# Thiết kế 10 level: truy bắt đội cướp ngân hàng

> BẢN NHÁP thiết kế (2026-10-05), chưa dựng level nào. Người dùng sẽ chỉnh bối cảnh và mô tả chi tiết từng phase trước khi triển khai.

## Cốt truyện (một mạch xuyên 10 level)
Băng cướp **Rắn Đỏ** đột nhập Ngân hàng Trung ương, bắt con tin, khoan két rồi tẩu thoát: ngân hàng → ~~hầm xe → kho tiền~~ **kho tiền → hầm xe (vòng 10: vào cướp trước, rồi chở tiền tẩu thoát)** → cống ngầm → ga tàu → bến cảng → tòa tháp → sân đáp trực thăng. Người chơi (cảnh sát) bám theo một mạch tới thủ lĩnh.

## Độ khó, số phase, thời lượng (đã chốt)
| Độ khó | Level | Số phase | Thời lượng một lượt chơi | Tổng enemy mỗi level (vòng 9) |
|---|---|---|---|---|
| Easy | 1, 2, 3, 4, 6, 7, 9 | 3 | 120–150 s → **150–185 s (vòng 9)** → **~165–185 s (vòng 11)** | 24–33 (**vòng 11**) |
| Normal | 8 | 4 | 150–180 s → **185–215 s (vòng 9)** | 30 |
| Hard | 5 | 5 | 180–240 s → **210–250 s (vòng 9)** → **~230 s (vòng 11)** | **36 (vòng 11)** |
| Boss | 10 | 5 | khoảng 300 s → **khoảng 310 s (vòng 9)** → **~335 s (vòng 11)** | **40** + thủ lĩnh |
**(vòng 9)** Mỗi level +10 enemy so với bản trước. **(vòng 11)** Trần nâng từ 30 lên **50 enemy/level cho tất cả level**; L2, L5, L10 nay đủ +10 so với số gốc (33 / 36 / 40); L4 và L6 giữ +13 (27 / 26). Số đồng thời tối đa trên màn hình vẫn nhỏ (xem "Đã chốt (vòng hỏi 11)"). Ước lượng thêm khoảng 2.5–3 s mỗi enemy nên thời lượng tăng 25–35 s mỗi level (vòng 11: L2 +9 s, L5 +18 s, L10 +25 s so với vòng 9).
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
| 4 | Easy | **Kho tiền (vòng 10)** | Hành lang an ninh cửa vault · Phòng két hai tầng · Đại sảnh vault và lõi vàng | Khiên người |
| 5 | Hard | **Hầm xe và khu hậu cần, tẩu thoát (vòng 10)** | Bến nạp tiền · Bãi hầm P-1 · Lối dốc xoắn · Kho hậu cần · Trạm bơm thoát nước | Thùng nổ |
| 6 | Easy | Cống ngầm thoát hiểm | Cửa xả trạm bơm (vòng 10) · Kênh cống ngập · Ngã ba hầm bảo trì | Thuốc nổ ném (phải bắn rơi) |
| 7 | Easy | Ga tàu điện ngầm | Sảnh vé · Sân ga · Đường ray, tàu hàng | Nhặt thùng súng (shotgun) |
| 8 | Normal | Bến cảng, bãi container | Cổng cảng · Container chồng tầng · Nhà kho lạnh · Cầu cảng, cần cẩu | Súng máy, nhiều mục tiêu cùng lúc |
| 9 | Easy | Tòa tháp tài chính (đi lên) | Sảnh tháp · Thang thoát hiểm, hành lang kính · Sân thượng phụ | Kính vỡ và vật che động |
| 10 | Boss | Sân đáp trực thăng đỉnh tháp | Cửa lên mái · Giàn mái, anten · Bãi đáp, xe chở tiền · Trực thăng cất cánh · Giao chiến thủ lĩnh | Không luật mới; kết hợp tất cả |
Tổng 35 phase.
**(vòng 9)** Không đổi bối cảnh hay luật mới; chỉ đổi mật độ: L4–6 enemy đứng xa hơn (18–28 m) nên mỗi khu giao tranh cần chiều sâu nhìn thấy **≥ 30 m** (**vòng 11:** có xạ thủ tới **35 m** nên chiều sâu nhìn thấy cần **≥ 38 m** ở mọi khu có xạ thủ xa); từ L4 stage cuối là đợt dồn dập; L5 P5 có "thác enemy"; L8 P4 **là** "Vòng vây cầu cảng" (7 enemy cùng lúc; vòng 11: thay toàn bộ nội dung P4, không thêm phase). Con tin có thêm hai kiểu xuất hiện: chui lên từ dưới đất và chạy ngang qua màn hình.

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
- **(vòng 9)** Con tin chạy ngang và chui lên, wave "thác" sinh enemy liên tiếp, trần enemy trên màn hình tách khỏi trần vòng target đếm cùng lúc, cảnh báo mũi tên mép màn hình, nới ngưỡng validator. Chi tiết ở "Câu hỏi mở" (mục rủi ro vòng 9).

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
- **(vòng 9)** Trần tuyệt đối enemy cùng lúc trên màn hình vẫn là **4** (bảng trên giữ nguyên cho wave thường). Ngoại lệ có tên: stage cuối L4+ được +1 so với bảng (Easy 3, Normal/Hard/Boss 4); "thác enemy" L5 P5 tối đa 6 trên màn hình nhưng 4 đang ngắm; "Vòng vây cầu cảng" L8 P4 có 7 trên màn hình, tối đa 4 vòng target đếm cùng lúc. Con tin **chui lên** và **chạy ngang** (vòng 9) là kiểu xuất hiện của con tin trong cột trên; mỗi level được thêm tối đa 1 con tin chạy ngang ngoài số này.
- Enemy có thể xuất hiện **lần lượt** hoặc **dồn dập** (đợt dồn dập vẫn tôn trọng số tối đa cùng lúc).
- Máu: 3 tim, không hồi giữa level (luật hiện tại).
- Con tin chủ yếu xuất hiện ở giữa level (nhịp bất ngờ) và là mục tiêu ưu tiên.

## Đã chốt (vòng hỏi 4)
- Cảnh hài: H02 ở level 2, H01 ở level 3, H03 ở level 4 (H03 dùng lại ở container level 8). **(vòng 10)** H03 đi theo bối cảnh hầm xe sang level 5; level 4 (kho tiền) để trống chỗ cho gag mới.
- Level 5 (Hard): điểm nhấn giữa level = cửa kho bị đóng, cướp dùng khiên người, khiên là con tin nhân viên ngân hàng. **(vòng 10)** Cảnh khiên đôi này chuyển sang đầu stage cuối của level 4 (kho tiền); điểm nhấn giữa level 5 mới là **lính khiên cứng** tiến xuống lối dốc xoắn.
- Bất ngờ mặc định giữa level Easy: con tin xuất hiện xen giữa enemy. **Ngoại lệ level 1:** con tin là luật mới của level 2, nên level 1 dùng "enemy xuất hiện từ trên cao / xa bất ngờ" làm bất ngờ giữa level.

## Storyboard Level 1 — Mặt tiền ngân hàng (Easy, 3 phase, dự kiến ~135 s → **~165 s (vòng 9)**)
Luật mới: chạm đúng vòng target, một enemy mỗi lần. Chưa có con tin, thùng nổ, thùng súng, khiên người, thuốc nổ ném (các luật này nằm ở level 2–7). Đạn: Pistol. Enemy tối đa cùng lúc: 2. Vòng target 2.5 s. Vật trang trí (thùng rác, xe, kính) không nổ.

| Mốc (dự kiến) | Phase / shot | Sự kiện | Mục đích nhịp |
|---|---|---|---|
| 0–6 s | Đường vào: xe cảnh sát → vỉa hè (Move) | Mở đầu buổi sáng nắng, còi cảnh sát, đèn xoay của xe cảnh sát. | Giới thiệu |
| 6–18 s | **P1 vỉa hè**, góc trái | W1: 1 enemy ló sau thùng rác, biểu tượng bàn tay chạm vào vòng đỏ (không chữ). | Luật mới (15–25%) |
| 18–28 s | P1, góc phải | W2: 1 enemy sau xe van. | Lặp mẫu, một mục tiêu |
| 28–40 s | P1 | W3: 2 enemy lần lượt (không đồng thời): một bậc thấp rồi một sau cột. | Nâng nhẹ |
| +12 s (vòng 9) | P1, góc giữa | W4: 3 enemy lần lượt (tối đa 2 cùng lúc): 2 chạy vào từ hai đầu vỉa hè, 1 ló sau trụ biển bus. | Ôn mẫu, tăng mật độ |
| 40–46 s | Move sang ngõ hông | Cảnh sát nạp đạn khi dựa tường. | Giảm tải ngắn |
| 46–58 s | **P2 ngõ hông**, góc phải | W1: 1 enemy gần sau xe van (chiều sâu gần). | Chuẩn bị |
| 58–72 s | P2, mở rộng khung | W2 (**bất ngờ giữa level 40–50%**): 2 enemy dồn dập — một từ cửa sổ tầng 2 (cao), một ở cuối ngõ (xa). | Đổi cao độ và chiều sâu |
| 72–76 s | P2 | Giảm tải 3–4 s: cửa chính bị xe tải chắn, camera lia nhìn lối bậc thềm; có thời gian reload. | Nhịp giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt sau thùng và cột. | Củng cố |
| +10 s (vòng 9) | P2, đầu bãi đỗ | W4: 3 enemy lần lượt: 2 chạy vào từ bãi đỗ xe, 1 trên thang sắt (cao). | Củng cố cao độ |
| 88–94 s | Move ra bậc thềm | Camera lướt qua bãi đỗ xe tới sân trước cổng. | Chuyển cảnh |
| 94–110 s | **P3 bậc thềm**, góc thấp | W1: 2 → **3 enemy (vòng 9)** ở sân thấp (gần/ xa), lần lượt. | Bài kiểm tra bắt đầu |
| 110–124 s | P3, góc cao lên bậc thềm | W2: 3 enemy dồn dập (tối đa 2 cùng lúc): bậc thấp, bậc cao, cạnh cửa. | Kết hợp cao độ, chiều sâu |
| +10 s (vòng 9) | P3, bồn hoa hai bên | W3: 3 enemy dồn dập (tối đa 2 cùng lúc): bồn hoa trái, bồn hoa phải, sau cột đá. | Kết hợp |
| 124–135 s | P3, cửa chính | W4 (vòng 9, trước là W3): enemy cuối bước ra cửa chính, kill-zoom rồi cửa bật mở; camera chạy tiếp vào sảnh (sang level 2). | Giải tỏa. Không luật mới |

**(vòng 9)** Hàng "+N s" là wave chèn thêm: mọi mốc phía sau lùi thêm N giây (tổng +32 s). Mốc cũ giữ nguyên để dễ đối chiếu.
Tổng enemy 15 → **25 (vòng 9)**: P1 4 → 7 (W1 1, W2 1, W3 2, W4 3), P2 5 → 8 (W1 1, W2 2, W3 2, W4 3), P3 6 → 10 (W1 3, W2 3, W3 3, W4 1). Tối đa 2 cùng lúc. **(vòng 11) Phân bố khoảng cách:** gần 12–16 m **15**, vừa 17–22 m **8**, xa 26–30 m **2** (T xa bình thường không laser: P2 W2 cuối ngõ ~28 m, P3 W1 sân thấp phía xa ~30 m; vòng target các tên xa +0.5 s). P2 vẫn giữ nhịp giảm tải 3–4 s không encounter sau wave bất ngờ. Không có con tin (luật của level 2), nên không có con tin chui lên/chạy ngang. Bắn khoảng 70 s, di chuyển khoảng 25 s, nhịp kể chuyện/giảm tải khoảng 12 s; còn lại là chờ ló ra. Mốc giây chỉ là dự kiến, hoàn thành sự kiện thì chuyển ngay.
Cảnh hài: không có ở level 1 (H02 ở level 2).
Rank mẫu (chưa chốt): thang theo độ chính xác, số lần bị trúng, thời gian hoàn thành so với mốc dự kiến.

## Storyboard Level 2 — Sảnh giao dịch (Easy, 3 phase, ~150 s → **~170 s (vòng 9)**)
Luật mới: **con tin, không bắn nhầm**. Con tin 3 (+1 con tin chạy ngang, vòng 9).
**(vòng 9)** +7 enemy (23 → 30) bằng cách tăng số enemy trong wave sẵn có, **không thêm shot** (giữ 3 phase x 6 shot đã dựng); mỗi wave vẫn ≤ 4 enemy. Enemy thêm là kiểu chạy vào (E chạy) hoặc Door. **(vòng 11)** Trần nâng lên 50 nên đủ +10: **23 → 33** (+3 enemy: P1 S3, P2 S2, P3 S5, mỗi chỗ +1). Khoảng cách: gần 12–16 m **20**, vừa 17–22 m **10**, xa 26–32 m **3** (T xa bình thường, không laser vì xạ thủ S laser mới giới thiệu ở level 4: P1 S3 cuối sảnh ~28 m, P2 S6 cuối dãy quầy ~30 m, P3 S6 chân cầu thang ~32 m; vòng target các tên xa +0.5 s). Cảnh hài H02 (hoạt cảnh chạy đâm cột chưa làm, hiện con tin chạy đi bình thường). Enemy tối đa 2 cùng lúc (wave cuối phase 3: 3), vòng target 2.5 s, không Justice.
Mỗi phase 4 wave (shot S2, S3, S5, S6; S1 và S4 là Move). **Enemy đứng sẵn (S)**: đặt sẵn trong scene, đứng lộ ở chỗ trống, không ló ra từ chỗ nấp; vào thẳng Ngắm khi wave kích hoạt (EnemyActor.SceneStanding). **E** = ló ra từ chỗ nấp.
Camera Combat dùng FOV dọc tối thiểu hẹp hơn Level 1 (S2 40, S3 36, S5 38, S6 32 = kill-zoom; Level 1 đặt 58–60 nên luôn bị chặn ở 45), cụm mục tiêu hẹp (<= ~10 độ) nên zoom thực tế khoảng 32–42 độ.
| Phase / shot | Sự kiện | Enemy | Nhịp |
|---|---|---|---|
| Move S1 | Qua cửa xoay vào sảnh (nối từ level 1) | 0 | Giới thiệu |
| **P1 cửa xoay, tiếp tân** S2 | W1: 1 S đứng trước quầy tiếp tân + 1 E sau quầy | 2 | Mẫu quen |
| P1 S3 | W2: **con tin đầu tiên** nhô sau chậu cây (tự cúi xuống sau ~3 s), 2 enemy sau máy ATM cách xa rõ **+ 1 T xa ~28 m cuối sảnh (vòng 11)**; biểu tượng con tin nhấp nháy, không bắn | 2 → **3** + 1 con tin | **Luật mới** |
| Move S4 | Đi dọc sảnh | 0 | Chuyển |
| P1 S5 | W3: 1 S đứng cạnh quầy thông tin + **1 E chạy vào từ cửa xoay (vòng 9)** | 1 → **2** | Giảm tải nhẹ |
| P1 S6 | W4: 2 E (chậu cây, quầy tiếp tân) + **1 E chạy vào từ hành lang ATM (vòng 9)**, kill-zoom; con tin chạy ra cửa (H02 chạy đâm cột dự kiến) | 2 → **3** | Hài, giải tỏa |
| Move S1 | Chuông báo động, camera tới dãy quầy giao dịch | 0 | Chuyển cảnh |
| **P2 dãy quầy** S2 | W1: 1 E sau kính quầy + 1 S đứng trước quầy **+ 1 E chạy vào cuối dãy (vòng 11)** | 2 → **3** | Củng cố |
| P2 S3 | W2: 1 E sau quầy giữa + **1 E chạy vào cuối dãy quầy (vòng 9)**; **con tin chạy ngang (vòng 9)**: nhân viên quầy chạy từ ô quầy số 1 sang cửa thoát hiểm, không dừng, chạy trước khi enemy vào ngắm | 1 → **2** + 1 con tin chạy ngang | Nhịp thở, dân chạy ngang lần đầu |
| Move S4 | Trượt ngang dọc dãy quầy | 0 | Chuyển |
| P2 S5 | W3 (**bất ngờ giữa level**): 1 E + 1 S đứng + 1 con tin xen giữa (ló sau quầy, tự cúi sau ~3 s), phải phân biệt | 2 + 1 con tin | Con tin xen enemy |
| P2 S6 | W4: 1 E + 1 S + **1 E chạy vào (vòng 9)**, kill-zoom; đèn đỏ báo động nhấp nháy | 2 → **3** | Giải tỏa |
| Move S1 | Tới khu chờ, cầu thang đôi hiện ra | 0 | Chuyển cảnh |
| **P3 khu chờ, cầu thang** S2 | W1: 1 E trên lan can tầng lửng (cao) + 1 S đứng ở khu ghế chờ (thấp) + **1 E chạy vào từ máy rút số (vòng 9)** | 2 → **3** | Kiểm tra |
| P3 S3 | W2: 1 E lan can + 1 E sau ghế + **1 E chạy dọc lan can (vòng 9)** | 2 → **3** | Cao/thấp |
| Move S4 | Tiến thêm một đoạn | 0 | Chuyển |
| P3 S5 | W3: 2 enemy (1 E sau ghế, 1 S đứng) **+ 1 T xa (vòng 11)** + **con tin thứ hai** ló lên từ sau lưng ghế giữa, tự cúi xuống sau ~3 s (không ngồi im) | 2 → **3** + 1 con tin | Kết hợp con tin |
| P3 S6 | W4: 3 enemy cuối (lan can, ghế, chân cầu thang; 1 S) + **1 E chạy xuống cầu thang (vòng 9)**, kill-zoom; camera sẵn sàng đi lên tầng lửng | 3 → **4** | Giải tỏa |
Tổng enemy **23 → 30 (vòng 9) → 33 (vòng 11, đủ +10)**: P1 7 → 9 → **10**, P2 7 → 9 → **10**, P3 9 → 12 → **13** (đứng sẵn vẫn 8), con tin 3 (1 mỗi phase) + 1 con tin chạy ngang ở P2 S3, tối đa 2–3 enemy cùng lúc. Level 2 không có con tin chui lên (sảnh đá không có lối dưới sàn). Level 1 có 25 enemy (vòng 9). Mỗi wave vẫn ≤ 4 enemy.
Ghi chú: con tin thứ 3 ban đầu trong storyboard cũ (P2 W3) đã gộp vào con tin P3; mỗi phase đúng một con tin.

## Storyboard Level 3 — Tầng lửng và văn phòng (Easy, 3 phase, ~135 s → **~165 s (vòng 9)**)
Luật mới: **Justice shot** (bắn trúng súng trên tay enemy để hạ gọn, thưởng điểm). Con tin 2–3 (+1 chạy ngang, vòng 9). Cảnh hài H01 là bất ngờ giữa level. Khoảng cách **(vòng 11)**: gần 12–16 m **12**, vừa 17–22 m **9**, xa 26–32 m **3** (T xa không laser: P1 W4 cuối hành lang ~28 m, P2 W3 qua bàn họp ~26 m, P3 phòng server sâu ~32 m; vòng target các tên xa +0.5 s).
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move lên tầng lửng | Nối từ level 2 | Giới thiệu |
| 6–16 s | **P1 hành lang văn phòng** | W1: 1 enemy ló khỏi cửa văn phòng (mẫu quen) | Mẫu quen |
| 16–28 s | P1 | W2: 1 enemy cầm súng lộ rõ, biểu tượng ngắm sáng vào khẩu súng trên tay | **Luật mới** |
| 28–40 s | P1 | W3: 2 enemy lần lượt, thử Justice shot | Củng cố |
| +12 s (vòng 9) | P1, cuối hành lang | W4: 3 enemy lần lượt (tối đa 2 cùng lúc) kiểu Door từ hai dãy cửa văn phòng, 1 cầm súng lộ rõ để Justice shot | Ôn luật mới |
| 40–46 s | Move vào phòng họp kính | | Chuyển cảnh |
| 46–56 s | **P2 phòng họp kính** | W1: 1 enemy sau cửa kính + **1 enemy chạy vào từ hành lang (vòng 9)**, 2 con tin núp dưới bàn họp, ló lên rồi tự cúi xuống sau ~3 s (không ngồi im, không bắn) | Con tin quen |
| 56–72 s | P2 | W2 (**bất ngờ giữa level**, **H01**): ba tay súng nhảy từ gác thấp, hai tên tiếp đất, tên thứ ba vướng lan can, chúi mặt, nằm bất động mất súng; hai tên còn lại bắn sau warning đầy đủ | Hài và đổi cao độ |
| 72–76 s | P2 | Giảm tải: chuông thang máy, reload | Giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt sau tủ hồ sơ + **1 enemy Vault qua bàn họp (vòng 9)**; **con tin chạy ngang (vòng 9)**: nhân viên chạy từ cửa phòng họp về phía thang máy vừa mở (nối với chuông thang máy ở nhịp giảm tải), không dừng | Củng cố, dân chạy ngang |
| 88–94 s | Move tới phòng an ninh | | Chuyển cảnh |
| 94–112 s | **P3 phòng an ninh camera** | W1: 2 → **3 enemy (vòng 9)**, có Justice shot; **con tin chui lên (vòng 9)**: kỹ thuật viên trốn dưới sàn nâng của phòng server, nhấc ô sàn chui lên ngang thắt lưng, tự cúi xuống sau ~3 s (thay con tin bị trói ghế) | Kiểm tra |
| 112–128 s | P3 | W2: 3 enemy dồn dập, tối đa 2 cùng lúc | Kết hợp |
| +8 s (vòng 9) | P3, cửa phòng server phụ | W3: 2 enemy kiểu Door từ phòng server phụ, lần lượt | Kết hợp |
| 128–135 s | P3 | W4 (vòng 9, trước là W3): enemy cuối trước màn hình camera, kill-zoom, cửa hầm bật mở | Giải tỏa |
Tổng enemy khoảng 14 → **24 (vòng 9)**: P1 7 (1, 1, 2, 3), P2 8 (2, 3 gồm 1 tên tự ngã H01 không tính kill, 3), P3 9 (3, 3, 2, 1). Con tin 3 (2 núp bàn họp ló lên rồi tự cúi ở P2 W1, 1 chui lên ở P3 W1) + 1 chạy ngang ở P2 W3. Hàng "+N s" lùi các mốc sau (tổng +20 s, cộng thời gian bắn thêm ≈ +30 s).

## Storyboard Level 4 — Kho tiền (Easy, 3 phase, ~175 s) — viết lại vòng 10
> **(vòng 10)** Đổi thứ tự bối cảnh: Level 4 là **kho tiền** (trước đây là Level 5), Level 5 là **hầm xe và khu hậu cần** (trước đây là Level 4). Lý do và bảng tóm tắt ở "Đã chốt (vòng hỏi 10)". Ký hiệu loại enemy: T thường (ló), C chạy vào, D nhảy từ trên xuống, X xung phong, S xạ thủ, K khiên người, KC khiên cứng, GI lính giáp, ĐX kẻ đẩy xe, G ném thuốc nổ, CC chui cống (xem bảng loại enemy ở vòng 10).

Luật mới: **khiên người** (kẻ cướp kẹp nhân viên làm khiên; bắn vào tay cầm súng hoặc đầu enemy lộ ra). Enemy mới làm quen: **X xung phong**, **S xạ thủ**. Enemy tối đa 2 cùng lúc (stage cuối 3), vòng target 2.5 s, khoảng cách chủ yếu **18–28 m** (khiên người 14–20 m). **(vòng 11) Phân bố khoảng cách:** gần 14–18 m **6** (gồm khiên người), vừa 18–26 m **15**, xa 26–35 m **6** (xạ thủ S: P2 W3 trên ban công ~32 m, P3 W2 cuối đại sảnh ~35 m; còn lại là C chạy vào từ lối xa 28–30 m); số enemy giữ 27 (+13) vì đã vượt +10. Con tin 3 (1 chui lên) + 1 chạy ngang. Hộp hồi máu 1 (đầu P3). Không có thùng nổ (luật của level 5). Cảnh hài: chưa có (chỗ trống cho gag mới).
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move: thang máy bảo mật từ phòng an ninh xuống tầng kho tiền, cửa thang trượt mở | Nối từ level 3 | Giới thiệu |
| 6–16 s | **P1 hành lang an ninh cửa vault** | W1: 1 T ló khỏi hốc cửa chớp (~20 m) | Mẫu quen |
| 16–30 s | P1 | W2: 1 K kẹp nhân viên kiểm soát (~16 m), vòng sáng ở tay cầm súng, biểu tượng ngón tay chỉ vào tay súng (không chữ) | **Luật mới** |
| 30–40 s | P1 | W3: 1 C ra từ phòng kiểm soát (Door) + 1 T ló; con tin nhân viên ló sau quầy kiểm soát | Củng cố |
| 40–52 s | P1 | W4: 3 C chạy vào liên tiếp từ hai cửa chớp đang kéo lên, cách nhau 0.8 s, tối đa 2, 22–26 m | Cao trào P1 |
| 52–58 s | Move vào phòng két ký gửi | Cửa chớp kẹt nửa chừng, camera cúi qua | Chuyển cảnh |
| 58–70 s | **P2 phòng két hai tầng** | W1: 2 T ló giữa hàng két tầng thấp; **con tin chui lên** từ cửa sập két sàn (nắp rung 0.4 s trước), tự cúi sau ~3 s | Mẫu quen, con tin chui lên |
| 70–82 s | P2 | W2 (**bất ngờ giữa level**): 2 D nhảy từ sàn lưới tầng trên + 1 **X** chạy thẳng dọc lối giữa (lần đầu: tiếng hét + vòng vàng, chỉ bắn khi đã dừng ở 14 m) | Đổi cao độ và kiểu tấn công |
| 82–85 s | P2 | Giảm tải 3 s: còi báo động tắt, đèn đỏ chuyển trắng, reload | Giảm tải |
| 85–97 s | P2 | W3: 1 K kẹp nhân viên đang mở két + 1 **S** trên ban công tầng hai (~26 m → **~32 m, vòng 11**, lần đầu: tia laser đỏ 1.5 s trước khi bắn) | Ưu tiên mục tiêu |
| 97–107 s | P2 | W4: 2 C chạy xuống cầu thang sắt liên tiếp; **con tin chạy ngang**: nhân viên chạy từ hàng két ra cầu thang, chạy ngay đầu wave | Dồn lại, dân chạy ngang |
| 107–113 s | Move vào đại sảnh vault | Cửa kho phía sau đóng sầm | Chuyển cảnh |
| 113–125 s | **P3 đại sảnh vault và lõi vàng** (stage cuối) | Hộp hồi máu; W1: 2 K, hai khiên người đều là nhân viên ngân hàng; ưu tiên cứu | Khiên đôi |
| 125–137 s | P3 | W2: 3 dồn dập (1 T sau xe đẩy tiền + **1 S xạ thủ ở cuối đại sảnh ~35 m (vòng 11, thay 1 T)** + 1 X), tối đa 2; con tin thứ ba ló ở xa rồi tự cúi sau ~3 s | Kết hợp |
| 137–139 s | P3 | Lấy hơi 1.5 s: cửa vault tròn quay mở, đèn xoay vàng, mũi tên ở mép màn hình | Lấy hơi |
| 139–155 s | P3 | W3 **"Dồn dập lõi vàng"**: 5 C chạy ra liên tiếp từ cửa vault và hai lối bên, cách nhau 0.8 s, tối đa **3** cùng lúc; không có con tin | Cao trào level |
| 155–158 s | P3 | Hết nhịp 3 s, reload | Giảm tải |
| 158–175 s | P3 | W4: tên cầm đầu nhóm vault ló sau xe đẩy vàng (1 T), kill-zoom; cửa cuốn nạp tiền sau lõi vàng kéo lên, lộ đường chuyển tiền ra hầm xe (nối level 5) | Giải tỏa |
Tổng enemy **27**: P1 7 (1, 1, 2, 3), P2 9 (2, 3, 2, 2), P3 11 (2, 3, 5, 1). Con tin 3 (P1 W3 ló, P2 W1 chui lên, P3 W2 ở xa) + 1 chạy ngang (P2 W4); khiên người không tính vào số con tin. Mọi khu giao tranh cần chiều sâu nhìn thấy ≥ 30 m (**vòng 11:** ≥ 38 m ở P2 và P3 vì có xạ thủ 32–35 m).

## Storyboard Level 5 — Hầm xe và khu hậu cần, tẩu thoát (Hard, 5 phase, ~215 s) — viết lại vòng 10
Luật mới: **thùng nổ** (thùng nhiên liệu máy phát; bắn để nổ hạ cả nhóm; luôn cách con tin ≥ 3.5 m). Enemy mới: **KC khiên cứng** (bất ngờ giữa level), **GI lính giáp**, **ĐX kẻ đẩy xe**; ôn K, X, S từ level 4. Max 3 cùng lúc (stage cuối: thác 6 trên màn hình / 4 vòng), vòng target 1.6 s (thác 2.0 s), khoảng cách chủ yếu **18–28 m**. **(vòng 11)** Enemy **30 → 36** (đủ +10). **Phân bố khoảng cách:** gần 14–18 m **7**, vừa 18–26 m **20**, xa 26–35 m **9** (xạ thủ S: P2 W1 cuối bãi ~33 m, P3 W2 cuối dốc ~35 m, P4 W1 trên kệ cao ~30 m; thác chạy ra từ cửa cuốn 24–28 m). Con tin 5 + 1 chạy ngang. Cảnh hài H03. Hộp hồi máu 2 (sau giảm tải P3; đầu P4), booster giáp đầu P4, thùng shotgun thưởng ở P2.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move theo đường chuyển tiền (băng chuyền, cửa cuốn) | Nối từ level 4 | Giới thiệu |
| 6–14 s | **P1 bến nạp tiền** | W1: 1 T ló sau băng chuyền | Mẫu quen |
| 14–26 s | P1 | W2: 2 T đứng hai bên thùng nhiên liệu đỏ của máy phát (thùng nhấp nháy sáng), một phát nổ hạ cả hai | **Luật mới** |
| 26–38 s | P1 | W3: 1 X chạy thẳng tới cạnh thùng nhiên liệu thứ hai (bắn thùng hoặc bắn người) **+ 1 T ló (vòng 11)**; nhân viên áp tải ló rồi tự cúi sau ~3 s (con tin, cách thùng ≥ 3.5 m) | Cao trào P1 |
| 38–44 s | Move ra bãi hầm P-1 | Đèn pha xe bọc thép rọi | Chuyển cảnh |
| 44–56 s | **P2 bãi hầm P-1** | W1: 2 T sau xe **+ 1 S xạ thủ cuối bãi ~33 m (vòng 11)**; **thùng shotgun** thưởng; **con tin chui lên** từ hố kiểm tra gầm xe | Phát triển |
| 56–68 s | P2 | W2 (**H03**): 1 C vấp mép sàn, trượt qua chỗ núp, nhặt súng + 1 K (ôn luật level 4) | Hài, ôn luật |
| 68–82 s | P2 | W3: 3 dồn dập (1 X + 2 D nhảy từ nóc xe tải), thùng nổ giữa bãi, tối đa 3 | Cao trào P2 |
| 82–88 s | Move xuống lối dốc xoắn P-2 | Tiếng lốp rít | Chuyển cảnh |
| 88–100 s | **P3 lối dốc xoắn** | W1 (**bất ngờ giữa level**): 2 **KC** tiến xuống dốc (lần đầu: đạn vào khiên nảy tia lửa, tay súng sáng khi hắn hạ khiên để bắn); thùng nổ cạnh lan can dốc là lối tắt | Kiểu tấn công mới |
| 100–103 s | P3 | Giảm tải 3 s: xe bọc thép của cướp đâm cột ở dốc trên, còi xe kêu, reload; hộp hồi máu | Giảm tải |
| 103–117 s | P3 | W2: 2 D nhảy từ lan can dốc tầng trên + 1 S cuối dốc (~28 m → **~35 m, vòng 11**) **+ 1 T xa (vòng 11)**; **con tin chạy ngang**: bảo vệ hầm chạy ngang mặt dốc, chạy ngay đầu wave | Cao độ và tầm xa |
| 117–125 s | P3 | W3: 1 **ĐX** đẩy xe nâng chở pallet tiến lên làm vật che **+ 1 T hộ tống (vòng 11)**; bắn thùng nổ trên pallet hoặc chờ hắn ló ra bắn | Kiểu tấn công mới nhẹ |
| 125–131 s | Move vào kho hậu cần | | Chuyển cảnh |
| 131–145 s | **P4 kho hậu cần** | Booster giáp + hộp hồi máu; W1: 1 S trên kệ cao 3 m (~30 m, vòng 11) + 1 **GI** dưới kệ (lần đầu: phát 1 bắn bay mũ sắt, tia lửa, khựng 0.5 s) | Kết hợp |
| 145–160 s | P4 | W2: 1 K kẹp thủ kho + 1 T cạnh thùng nổ (thùng cách con tin ≥ 3.5 m) **+ 1 T ló (vòng 11)** | Kết hợp luật |
| 160–166 s | Move tới trạm bơm thoát nước | | Chuyển cảnh |
| 166–168 s | **P5 trạm bơm** (stage cuối) | Lấy hơi 1.5 s: hai cửa cuốn kho hai bên kéo lên, đèn xoay vàng, tiếng bước chân dồn, mũi tên mép màn hình | Lấy hơi |
| 168–188 s | P5 | W1 **"Thác enemy"**: 8 enemy (5 C, 2 X, 1 GI) chạy ra liên tiếp từ hai cửa cuốn, luân phiên trái/phải, **cách nhau 0.7 s, không chờ tên trước bị hạ**; dòng chỉ ngừng khi đủ 6 còn sống trên màn hình; tối đa **4 vòng target đếm cùng lúc**, vòng 2.0 s, so le ≥ 0.4 s; 1 thùng nổ giữa hai cửa hạ được 2–3 tên; không có con tin | **Thác enemy** |
| 188–191 s | P5 | Hết nhịp 3 s: khói tan, reload | Giảm tải |
| 191–215 s | P5 | W2: đội trưởng tẩu thoát kẹp thủ quỹ làm khiên cạnh miệng cống lớn (1 K) **+ 1 T hộ vệ (vòng 11)**; kill-zoom; cửa xả trạm bơm mở xuống cống (nối level 6) | Kết thúc |
Tổng enemy **30 → 36 (vòng 11, đủ +10, không còn bị trần cắt)**: P1 5 (1, 2, 2), P2 8 (3, 2, 3), P3 8 (2, 4, 2), P4 5 (2, 3), P5 10 (thác 8, đội trưởng + hộ vệ 2). Con tin 5 (P1 áp tải, P2 chui lên, P2 khiên, P4 thủ kho, P5 thủ quỹ) + 1 chạy ngang (P3 W2).

## Storyboard Level 6 — Cống ngầm thoát hiểm (Easy, 3 phase, ~130 s → **~170 s (vòng 9)**)
Luật mới: **thuốc nổ ném** (grenadier ném, bắn rơi thuốc nổ trước khi chạm đất). Con tin 2. Nước, đèn khẩn cấp, đường ống.
**(vòng 10)** Nối từ trạm bơm của level 5 (hầm xe) thay vì từ kho tiền. Enemy mới: **CC chui cống** (chui lên từ nước/cửa thăm cống); ôn X, S, GI. Mở màn P1 đổi thành "cửa xả trạm bơm" (cướp phá bằng thuốc nổ nên vẫn có lửa nhỏ).
**(vòng 9)** Enemy đứng **xa hơn: 18–28 m** (**vòng 11:** thêm xạ thủ xa tới 35 m) và **nhiều hơn: 13 → 26** (+13, giữ nguyên vì đã vượt +10). **(vòng 11) Phân bố khoảng cách:** gần 14–18 m **4**, vừa 18–26 m **15**, xa 26–35 m **7** (xạ thủ S: P2 W3 trên ống lớn ~33 m; 3 C chạy vào cuối ống cống P1 W4 ~30 m; CC chui cống ở 26–28 m). Grenadier ở 18–28 m: thuốc nổ bay lâu hơn (vệt sáng giữ nguyên), thời gian bắn rơi không được ngắn hơn bản cũ. Stage cuối (P3) dồn dập, tối đa **3** cùng lúc. Con tin 2 (1 chui lên) + 1 chạy ngang.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move xuống thang sắt qua cửa xả trạm bơm bị phá (vòng 10) | Nối từ level 5 | Giới thiệu |
| 6–16 s | **P1 cửa xả trạm bơm, ống cống chính** (vòng 10) | W1: 1 enemy | Mẫu quen |
| 16–28 s | P1 | W2: 1 grenadier, thuốc nổ bay có vệt sáng | **Luật mới** |
| 28–40 s | P1 | W3: 1 grenadier + 1 enemy lần lượt | Củng cố |
| +12 s (vòng 9) | P1, cuối ống cống | W4: 3 enemy chạy vào từ cuối ống cống (24–28 m), lần lượt, tối đa 2 cùng lúc | Làm quen tầm xa |
| 40–46 s | Move xuống kênh cống | | Chuyển cảnh |
| 46–58 s | **P2 kênh cống ngập** | W1: 2 enemy hai bờ, lần lượt → **(vòng 10)** 2 **CC** chui lên từ mặt nước sát bờ (bọt nước sủi 0.5 s trước, lộ ngang ngực) | Kiểu tấn công mới |
| 58–72 s | P2 | W2 (**bất ngờ giữa level**, **H08**): 3 enemy dồn dập từ hai bờ (tối đa 2 cùng lúc), một tên chạy trên bờ ướt trượt dài, đâm ống, rơi xuống nước (tự thoát, không tính kill); grenadier trên cầu đi bộ (cao), 1 con tin ló sau lan can cầu xen giữa rồi tự cúi sau ~3 s (thay con tin trói) | Hài, cao độ, con tin xen |
| 72–76 s | P2 | Giảm tải: đèn pin, nước chảy, reload | Giảm tải |
| 76–88 s | P2 | W3: 2 enemy lần lượt (vòng 10: 1 X lội nước chạy thẳng + 1 S trên ống lớn ~26 m → **~33 m, vòng 11**); **con tin chạy ngang (vòng 9)**: công nhân cống chạy ngang qua cầu đi bộ (cao, tách tầng với enemy dưới bờ), không dừng | Củng cố, dân chạy ngang |
| 88–94 s | Move tới ngã ba hầm bảo trì | | Chuyển cảnh |
| 94–110 s | **P3 ngã ba hầm bảo trì** | W1: 2 grenadier lần lượt + thùng nổ trên ống | Kiểm tra |
| 110–124 s | P3 | W2: 3 enemy dồn dập; 1 con tin là **con tin chui lên (vòng 9)** từ cửa thăm cống giữa ngã ba (cách thùng nổ ≥3.5 m), tự cúi sau ~3 s | Kết hợp |
| +18 s (vòng 9) | P3, ba cửa hầm | W3 **"Dồn dập ngã ba"**: cảnh báo 1 s (nước bắn ở ba cửa hầm + mũi tên mép màn hình), 5 enemy chạy ra liên tiếp từ ba cửa hầm, cách nhau 0.8 s, tối đa **3** cùng lúc (vòng 10: 2 C, 1 CC chui lên từ cửa thăm cống, 1 GI); 1 trong 5 là grenadier, ném sau cùng khi còn ≤ 2 enemy; thùng nổ thứ hai cạnh cửa hầm giữa hạ được 2 tên; không có con tin | Bài kiểm tra dồn dập |
| 124–130 s | P3 | W4 (vòng 9, trước là W3): enemy cuối chạy vào cổng hầm, tiếng tàu điện; cổng mở nối ga tàu | Giải tỏa |
Tổng enemy khoảng 13 → **26 (vòng 9)**: P1 7 (1, 1, 2, 3), P2 8 (2, 4 gồm 1 tên trượt H08 không tính kill, 2), P3 11 (2, 3, 5, 1). Con tin 2 (P2 W2 ló sau lan can cầu, P3 W2 chui lên) + 1 chạy ngang (P2 W3). Mọi khu giao tranh cần chiều sâu nhìn thấy ≥ 30 m (**vòng 11:** ≥ 38 m ở P2).

## Storyboard Level 7 — Ga tàu điện ngầm (Easy, 3 phase, ~135 s → **~170 s (vòng 9)**)
Luật mới: **thùng súng** (bắn thùng nhận shotgun, bắn chùm nhiều mục tiêu). Súng nhặt **không reload**; hết đạn quay về Pistol. Con tin 2 (+1 chui lên, +1 chạy ngang, vòng 9).
**(vòng 9)** Khoảng cách 12–24 m (không đổi nhiều). **(vòng 11) Phân bố khoảng cách:** gần 12–16 m **10**, vừa 17–24 m **11**, xa 26–32 m **3** (S xạ thủ trên toa tàu P3 W1 ~30 m, grenadier P2 W3 cuối sân ga ~28 m, 1 T xa dọc đường ray ~32 m); thấp hơn level 4–6. Stage cuối (P3) dồn dập, tối đa **3** cùng lúc, có thùng shotgun mới để hạ chùm.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move vào sảnh vé | Nối từ level 6 | Giới thiệu |
| 6–16 s | **P1 sảnh vé, cổng soát vé** | W1: 1 enemy | Mẫu quen |
| 16–30 s | P1 | W2: thùng súng shotgun rơi vào khung, thùng súng phát sáng | **Luật mới** |
| 30–42 s | P1 | W3: 2 enemy đứng gần nhau, shotgun hạ cả hai | Củng cố |
| +10 s (vòng 9) | P1, cổng soát vé | W4: 3 enemy chạy vào qua cổng soát vé (2 dừng sát nhau cho shotgun, 1 ở xa), tối đa 2 cùng lúc | Củng cố shotgun |
| 42–48 s | Move ra sân ga | | Chuyển cảnh |
| 48–60 s | **P2 sân ga** | W1: 2 → **3 enemy (vòng 9)** lần lượt sau cột; **con tin chui lên (vòng 9)**: nhân viên ga chui lên từ hốc trú ẩn dưới mép sân ga, tự cúi sau ~3 s | Củng cố, con tin chui lên |
| 60–74 s | P2 | W2 (**bất ngờ giữa level**): tàu vào ga, cửa mở, 3 enemy + 1 con tin xen xuống ga | Đổi hướng và con tin xen |
| 74–78 s | P2 | Giảm tải: thông báo ga, reload | Giảm tải |
| 78–90 s | P2 | W3: grenadier trên băng ghế, 1 enemy; **con tin chạy ngang (vòng 9)**: hành khách chạy dọc sân ga từ toa tàu tới cầu thang lên, không dừng, chạy trước khi grenadier ném | Củng cố, dân chạy ngang |
| 90–96 s | Move xuống đường ray | | Chuyển cảnh |
| 96–114 s | **P3 đường ray, tàu hàng** | W1: enemy trên toa tàu (cao) + **1 enemy cạnh thùng nổ (vòng 9)**, thùng nổ giữa đường ray | Kiểm tra |
| 114–128 s | P3 | W2: 3 enemy dồn dập, 1 con tin ló ở cửa toa rồi tự cúi sau ~3 s | Kết hợp |
| +16 s (vòng 9) | P3, giữa hai toa | W3 **"Dồn dập sân ray"**: thùng shotgun rơi vào khung khi camera dừng, cảnh báo 1 s (còi tàu hàng + mũi tên mép màn hình), 4 enemy nhảy xuống từ toa và chạy vào liên tiếp, cách nhau 0.8 s, đi theo cặp đứng sát nhau (shotgun hạ cặp), tối đa **3** cùng lúc; không có con tin | Bài kiểm tra dồn dập |
| 128–135 s | P3 | W4 (vòng 9, trước là W3): enemy cuối nhảy khỏi tàu hàng, tàu chạy qua mở lối ra bến cảng | Giải tỏa |
Tổng enemy khoảng 14 → **24 (vòng 9)**: P1 6 (1, 0, 2, 3), P2 8 (3, 3, 2), P3 10 (2, 3, 4, 1). Con tin 3 (P2 W1 chui lên, P2 W2 xen, P3 W2 trong toa) + 1 chạy ngang (P2 W3).

## Storyboard Level 8 — Bến cảng và bãi container (Normal, 4 phase, ~165 s → **~200 s (vòng 9)**)
Luật mới: **súng máy** (thùng MG; giữ ngón để bắn liên tục) và **nhiều mục tiêu cùng lúc**. Enemy tối đa 3, vòng target 2.0 s, con tin 4. Cảnh hài H03 lặp lại ở bãi container.
**(vòng 9)** Số enemy 20 → **30**. **P4 thành stage mới "Vòng vây cầu cảng"**: 7 enemy tấn công cùng lúc (xem "Đã chốt (vòng hỏi 9)"). **(vòng 11, đã chốt)** Vòng vây **thay hẳn nội dung P4** (không thêm phase; level 8 vẫn 4 phase); 2 enemy cần cẩu, grenadier, thùng nổ tàu và con tin chui lên của P4 cũ bỏ/chuyển sang P2–P3. Khoảng cách 12–24 m, xạ thủ xa tới 35 m. Con tin 4 (1 chui lên, vòng 11: ở P3 W2) + 1 chạy ngang.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move ra cổng cảng | Nối từ level 7 | Giới thiệu |
| 6–36 s | **P1 cổng cảng, trạm hải quan** | W1: 1 enemy; W2: thùng súng máy, biểu tượng ngón tay giữ; W3: 3 enemy dồn dập thử súng máy | **Luật mới** |
| +8 s (vòng 9) | P1, sau xe tải | W4: 2 enemy chạy ra từ sau xe tải, lần lượt | Củng cố |
| 36–42 s | Move vào bãi container | | Chuyển cảnh |
| 42–76 s | **P2 bãi container chồng tầng** | W1: 3 enemy ở ba tầng container (cao độ); W2: grenadier + 2 enemy; con tin 1 xen giữa (ló sau container rồi tự cúi sau ~3 s); **W3 (vòng 9)**: 1 enemy nhảy xuống (Drop) từ container tầng 3 **+ 1 S xạ thủ trên container tầng 3 ~34 m (vòng 11)** + **con tin chạy ngang**: tài xế xe nâng chạy ngang lối container, không dừng | Phát triển, dân chạy ngang |
| 76–82 s | Move vào nhà kho lạnh | | Chuyển cảnh |
| 82–122 s | **P3 nhà kho lạnh** | W1 (**bất ngờ giữa level**, **H03**): enemy chạy ra từ cửa lạnh, vấp dây rỗng, trượt qua chỗ núp **+ 1 enemy sau kệ (vòng 11)**; 2 con tin ló sau kệ/pallet rồi tự cúi sau ~3 s (không ngồi im); W2: khiên người ở cuối kho (~28 m) + **1 enemy sau kệ (vòng 9)** + **con tin chui lên từ hố thông gió sàn kho (vòng 11, chuyển từ P4)**; W3: 3 enemy dồn dập (tối đa 3 cùng lúc) | Hài, con tin xen, khiên |
| 98–102 s | P3 | Giảm tải: hơi lạnh bay, tiếng quạt, reload | Giảm tải |
| 122–130 s | Move ra cầu cảng | | Chuyển cảnh |
| 130–133 s | **P4 "Vòng vây cầu cảng"** (stage cuối; **vòng 11: thay toàn bộ nội dung cũ của P4**, số phase giữ 4) | Camera dừng giữa cầu cảng; thùng súng máy rơi vào khung giữa cầu cảng; lấy hơi 1.5 s (còi tàu dài + mũi tên ở ba hướng, reload được); không có con tin | Lấy hơi |
| 133–150 s | P4 | W1 **"Vòng vây cầu cảng"**: 7 enemy xông ra gần như cùng lúc (so le 0.25 s) từ ba hướng: 3 từ dãy container trái, 2 trên boong tàu (cao, 1 trong số đó là S xạ thủ ~30 m), 2 sau bích neo phải; cụm ≤ 21° ngang, 14–24 m (xạ thủ 30 m); tối đa **4 vòng target đếm cùng lúc**, vòng 2.4 s (thay 2.0 s), so le ≥ 0.4 s; súng máy quét được cả cụm | **Stage mới, bài kiểm tra** |
| 150–153 s | P4 | Hết nhịp 3 s: reload (súng máy hết đạn thì quay về Pistol) | Giảm tải |
| 153–165 s | P4 | W2: enemy cuối ngã khỏi tàu, kill-zoom, camera đi tiếp tới tòa tháp | Giải tỏa |
Tổng enemy khoảng 20 → **30 (vòng 9; vòng 11 giữ 30)**: P1 6 (1, 0, 3, 2), P2 8 (3, 3, 2), P3 8 (2, 3, 3), P4 8 (7 vòng vây + 1 enemy cuối). Con tin 4 (P2 W2 ló, P3 W1 hai con tin ló, P3 W2 chui lên) + 1 chạy ngang (P2 W3); P4 không có con tin. **(vòng 11) Phân bố khoảng cách:** gần 12–16 m **8**, vừa 17–24 m **16**, xa 26–35 m **6** (S xạ thủ: P2 W3 container ~34 m, P4 vòng vây ~30 m; T xa: P1 trạm hải quan ~28 m, P3 cuối kho ~28 m, P2 container tầng cao ~32 m).

## Storyboard Level 9 — Tòa tháp tài chính, đi lên (Easy, 3 phase, ~140 s → **~170 s (vòng 9)**)
Luật mới: **kính vỡ và vật che động** (vách kính che enemy; bắn vỡ để lộ rồi hạ). Con tin 3.
**(vòng 9)** Số enemy 14 → **24**, khoảng cách 12–24 m. **(vòng 11) Phân bố khoảng cách:** gần 12–16 m **9**, vừa 17–24 m **12**, xa 26–32 m **3** (S xạ thủ sau giàn điều hòa P3 W1 ~30 m, 1 T xa cuối hành lang kính P2 W3 ~28 m, 1 T xa sân thượng ~32 m). Stage cuối (P3) dồn dập, tối đa **3** cùng lúc. Con tin 3 (1 chui lên) + 1 chạy ngang.
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move vào sảnh tháp | Nối từ level 8 | Giới thiệu |
| 6–16 s | **P1 sảnh tháp** | W1: 1 enemy | Mẫu quen |
| 16–30 s | P1 | W2: 1 enemy sau vách kính, vết nứt sáng trên kính | **Luật mới** |
| 30–42 s | P1 | W3: 2 enemy lần lượt sau vách kính | Củng cố |
| +8 s (vòng 9) | P1, thang máy sảnh | W4: 2 enemy đi ra từ thang máy (Door), lần lượt | Củng cố |
| 42–48 s | Move vào thang thoát hiểm | | Chuyển cảnh |
| 48–62 s | **P2 thang thoát hiểm, hành lang kính** | W1: 2 enemy lần lượt từ cầu thang | Củng cố |
| 62–76 s | P2 | W2 (**bất ngờ giữa level**): vách kính vỡ lộ 2 enemy; con tin ló phía sau khung kính đã vỡ rồi tự cúi sau ~3 s | Con tin xen |
| 76–80 s | P2 | Giảm tải: gió thổi qua kính vỡ, reload | Giảm tải |
| 80–92 s | P2 | W3: grenadier + 1 enemy | Củng cố |
| +10 s (vòng 9) | P2, cuối hành lang kính | W4: 2 enemy nhảy xuống (Drop) từ chiếu nghỉ tầng trên, lần lượt; **con tin chạy ngang**: nhân viên văn phòng chạy ngang đầu hành lang (không có vách kính chắn giữa người chơi và con tin), không dừng | Đổi cao độ, dân chạy ngang |
| 92–98 s | Move lên sân thượng phụ | | Chuyển cảnh |
| 98–116 s | **P3 sân thượng phụ, giàn điều hòa** | W1: 2 enemy sau giàn điều hòa, thùng nổ cạnh giàn; **con tin chui lên (vòng 9)**: kỹ thuật viên chui lên từ cửa sập bảo trì trên mái (cách thùng nổ ≥3.5 m), tự cúi sau ~3 s; hộp hồi máu | Kiểm tra |
| 116–132 s | P3 | W2: 3 enemy dồn dập, khiên người xen giữa | Kết hợp |
| +14 s (vòng 9) | P3, cửa mái phụ và giàn điều hòa | W3 **"Dồn dập sân thượng"**: cảnh báo 1 s (cửa mái phụ đập mở + mũi tên mép màn hình), 4 enemy chạy ra liên tiếp, cách nhau 0.8 s, 2 tên đứng sau vách kính chắn gió (bắn vỡ trước), tối đa **3** cùng lúc; không có con tin | Bài kiểm tra dồn dập |
| 132–140 s | P3 | W4 (vòng 9, trước là W3): enemy cuối chạy về cửa mái, kill-zoom, cửa mái mở (nối level 10) | Giải tỏa |
Tổng enemy khoảng 14 → **24 (vòng 9)**: P1 6 (1, 1, 2, 2), P2 8 (2, 2, 2, 2), P3 10 (2, 3 gồm khiên người, 4, 1). Con tin 3 (P2 W2 sau kính, P3 W1 chui lên, P3 W2 khiên) + 1 chạy ngang (P2 W4).

## Storyboard Level 10 — Sân đáp trực thăng đỉnh tháp (Boss, 5 phase, ~300 s → **~310 s (vòng 9)**)
Không luật mới; kết hợp tất cả. Max 3–4 enemy, vòng target 1.4 s, con tin 5–6. Thủ lĩnh Rắn Đỏ **3 giai đoạn** đổi vị trí (số máu/điểm yếu mỗi giai đoạn chưa chốt). Không cảnh hài (hoặc tối đa một cảnh nhỏ nếu bạn muốn).
| Mốc | Phase / shot | Sự kiện | Nhịp |
|---|---|---|---|
| 0–6 s | Move lên cửa mái | Nối từ level 9 | Giới thiệu |
| 6–52 s | **P1 cửa lên mái** | Mẫu quen: enemy, con tin, Justice shot, khiên người; 3 enemy dồn dập cuối phase | Mở đầu, nhắc lại luật |
| 52–62 s | Move ra giàn mái | Gió mạnh, trực thăng đậu xa dưới nắng chiều | Chuyển cảnh |
| 62–120 s | **P2 giàn mái, bồn nước, anten** | Phát triển: grenadier, thùng nổ ở bồn, enemy trên anten (cao), con tin xen | Phát triển |
| 120–128 s | Move ra bãi đáp | Thấy xe chở tiền và trực thăng | Chuyển cảnh |
| 128–192 s | **P3 bãi đáp, xe chở tiền** | **Giai đoạn 1 (bất ngờ giữa level):** thủ lĩnh nấp sau xe chở tiền, chỉ hé ra ngắn, đồng bọn ném thuốc nổ; 2 con tin núp sau xe, ló rồi tự cúi (không đứng im). Sau khi thủ lĩnh bị thương lần 1, hắn bỏ chạy | Boss giai đoạn 1 |
| 192–198 s | P3 | Giảm tải: thủ lĩnh quát lớn (giọng không lời), tiền giấy bay, reload | Giảm tải |
| 198–206 s | Move tới trực thăng | | Chuyển cảnh |
| 206–256 s | **P4 trực thăng cất cánh** | **Giai đoạn 2:** thủ lĩnh chạy lên trực thăng, cánh quạt quay, đồng bọn trên trực thăng ném thuốc nổ, khiên người; bắn hạ đồng bọn để hạ trực thăng | Boss giai đoạn 2 |
| 256–264 s | Move tới mép bãi đáp | Trực thăng trục trặc | Chuyển cảnh |
| 264–282 s (vòng 9) | **P5 mép bãi đáp** | W1 **"Đợt cuối Rắn Đỏ"**: 1.5 s cảnh báo (cửa thang máy hàng mở + mũi tên mép màn hình), 6 đồng bọn chạy lên từ cầu thang mái và thang máy hàng, cách nhau 0.6 s, tối đa **4** cùng lúc, vòng target 1.8 s (thay 1.4 s); thùng nổ cạnh cửa thang máy hàng hạ được 2–3 tên; thủ lĩnh chưa ra, không có con tin; sau đó hết nhịp 3 s (reload) | Bài kiểm tra dồn dập |
| 282–310 s | P5 | **Giai đoạn 3:** thủ lĩnh dùng con tin làm khiên ở mép bãi đáp; Justice shot, bắn đúng điểm yếu; kill-zoom, giải tỏa: nắng chiều, còi, kết thúc chuỗi 10 level | Boss giai đoạn 3, giải tỏa |
**(vòng 9)** Con tin chui lên ở P2: kỹ thuật viên mái chui lên từ cửa sập kỹ thuật giữa bồn nước và giàn anten (cách thùng nổ ≥3.5 m), tự cúi sau ~3 s. Con tin chạy ngang ở P3: nhân viên áp tải tiền chạy ngang bãi đáp từ xe chở tiền ra cửa thang, không dừng, chạy ngay đầu giai đoạn 1 trước khi đồng bọn ném thuốc nổ.
Tổng enemy khoảng 30 → **40 + thủ lĩnh (vòng 11: trần 50 nên đủ +10)**; phân bổ: P1 9, P2 9, P3 7, P4 8, P5 7 (6 trong "Đợt cuối Rắn Đỏ" + 1 hộ vệ thủ lĩnh) (+ thủ lĩnh); thời lượng ~335 s. **(vòng 11) Phân bố khoảng cách:** gần 12–16 m **12**, vừa 17–26 m **20**, xa 26–35 m **8** (S xạ thủ trên anten P2 ~35 m, 2 đồng bọn trên trực thăng P4 ~30 m, xạ thủ trên xe chở tiền P3 ~32 m; thủ lĩnh và khiên người 14–20 m). Con tin 6 (P1 1, P2 chui lên, P3 hai con tin ló sau xe, P4 khiên người, P5 khiên của thủ lĩnh) + 1 chạy ngang (P3).

## Đã chốt (vòng hỏi 5)
- Các câu hỏi mở (máu boss, ngưỡng rank, thùng súng nhỏ, phân bổ con tin từng way) **tính sau**.
- Thêm **hộp hồi máu** (bắn để nhận +1 tim, tối đa 3 tim) ở một số level. Máu vẫn không hồi tự động giữa các level; chỉ hồi bằng hộp trong level.
- **Liền mạch giữa các level:** cảnh kết thúc của level này chính là cảnh bắt đầu của level sau (cùng vị trí, hướng nhìn, bối cảnh); riêng level 10 (boss) là kết thúc chuỗi.

### Hộp hồi máu (đề xuất vị trí)
Hộp xanh có dấu chữ thập, rơi vào khung ở nhịp giảm tải hoặc đầu phase kế; tồn tại khoảng 8 s hoặc đến hết phase; không đặt gần thùng nổ/khiên người; luôn nhìn thấy rõ, không che enemy. Hiện chưa có trong code (cần thêm vật phẩm như thùng súng và API hồi tim).
| Level | Số hộp | Vị trí |
|---|---|---|
| 1, 2, 3, 7 | 0 | Easy giai đoạn học luật |
| 4 | 1 | Đầu P3 (đại sảnh vault, vòng 10) |
| 5 | 2 | Sau giảm tải P3 (lối dốc); đầu P4 (kho hậu cần) (vòng 10) |
| 6 | 1 | Giảm tải P2 (kênh cống) |
| 8 | 1 | Giảm tải P3 (kho lạnh) |
| 9 | 1 | Đầu P3 (sân thượng phụ) |
| 10 | 2 | Cuối P2 (trước bãi đáp); đầu P4 (trước trực thăng) |

### Điểm nối giữa các level (kết thúc → bắt đầu)
| Nối | Cảnh kết thúc level trước | Cảnh bắt đầu level sau |
|---|---|---|
| 1 → 2 | Enemy cuối bước ra cửa chính, cửa bật mở, camera đi vào | Cửa chính vừa mở, camera vào cửa xoay sảnh |
| 2 → 3 | Enemy cuối chạy lên cầu thang, camera theo lên tầng lửng | Camera đã ở cầu thang, đi tiếp vào hành lang văn phòng |
| 3 → 4 (vòng 10) | Cửa thang máy bảo mật sau phòng an ninh trượt mở (phòng an ninh giữ quyền truy cập tầng kho tiền) | Thang máy xuống tầng kho tiền, cửa mở ra hành lang an ninh cửa vault |
| 4 → 5 (vòng 10) | Hạ tên cầm đầu nhóm vault, cửa cuốn nạp tiền sau lõi vàng kéo lên | Camera theo đường chuyển tiền (băng chuyền) ra bến nạp tiền của hầm xe |
| 5 → 6 (vòng 10) | Hạ đội trưởng ở trạm bơm, cửa xả trạm bơm mở xuống cống | Camera xuống thang sắt vào ống cống chính |
| 6 → 7 | Cổng hầm mở nối ga tàu | Camera ra khỏi cổng, vào sảnh vé |
| 7 → 8 | Tàu hàng chạy qua, mở lối ra cảng | Camera từ lối ra tới cổng cảng |
| 8 → 9 | Enemy cuối ngã khỏi tàu, camera đi tiếp tới tòa tháp | Camera tới sảnh tòa tháp |
| 9 → 10 | Cửa mái mở | Camera lên cửa mái, thấy bãi đáp |
| 10 | Kết thúc chuỗi (nắng chiều, còi) | — |
Kỹ thuật (chưa làm): bảng kết quả (điểm, rank) hiện lên trong lúc camera chạy đoạn Move cuối; cần cách nối level liền mạch (cùng scene hoặc nạp scene kế trước khi hết level), tư thế camera cuối level n trùng tư thế đầu level n+1.

- Level 5 có **thùng shotgun thưởng** ở P2, nhặt tùy chọn, không có gợi ý. **Không có thùng súng máy** ở level 5. Súng nhặt không reload, hết đạn quay về Pistol. Level 7 (shotgun) và level 8 (súng máy) vẫn là nơi giới thiệu chính thức, có biểu tượng gợi ý và dùng trong bài kiểm tra bắt buộc.
- Level 5 có **1 booster giáp** ở đầu P4 (phase trước phase dồn dập P5): bắn để nhận, giáp chặn **1 lần bị trúng** (không cộng vào 3 tim, mất khi dùng hoặc khi chết/hồi sinh). Biểu tượng gợi ý ngắn khi xuất hiện lần đầu. Hiện chưa có trong code (cần vật phẩm giáp và API chặn sát thương trong PlayerHealth).

## Mô tả chi tiết bối cảnh từng level (toàn bộ ban ngày)
Quy ước chung (lưu ý vòng 10: mô tả level 4 và level 5 đã đổi chỗ, các ghi chú "(vòng 9)" trong mô tả level 4–6 đã gộp vào bản viết lại): portrait; camera ở độ cao mắt người (1.6–1.7 m); enemy cách camera ≥12 m (**vòng 9:** level 1–3 và 7–10 dùng 12–22/24 m, level 4–6 dùng **18–28 m** nên khu giao tranh cần chiều sâu nhìn thấy ≥ 30 m; **vòng 11:** enemy xa tới **35 m** (xạ thủ) nên khu có xạ thủ cần chiều sâu ≥ 38 m; khu có stage dồn dập cần chỗ cho 2–3 lối vào nằm ngoài khung, mỗi lối có cửa/góc khuất); mỗi cụm vừa khung khoảng 21° ngang; enemy xếp theo ba tầng độ cao (thấp / ngang / cao). Vật che gồm vật che cứng (cột, tường, xe) và vật che vỡ được (kính, thùng carton). Thời gian trong ngày chạy một mạch từ sáng đến chiều muộn; mọi level đều có ánh sáng ban ngày (nắng trực tiếp hoặc ánh sáng ngày qua cửa kính, giếng trời, miệng cống), không có cảnh đêm. Ngoại lệ duy nhất: level 5 nằm sâu dưới đất nên dùng đèn, vẫn sáng đều.
Mỗi phase mô tả: **Không gian** (bố cục), **Camera** (đường và khung), **Che chắn và cao độ**, **Diễn biến** (theo way), **Hợp lý** (lý do hợp lý hoặc lưu ý).

### Level 1 — Mặt tiền ngân hàng (9 giờ sáng, nắng nhẹ)
Không khí: phố tài chính buổi sáng, nắng chiếu xiên làm bóng đổ dài, dải phong tỏa vàng-đen của cảnh sát, người dân đã được sơ tán. Màu: xanh trời, kem đá của ngân hàng, đỏ-xanh xe cảnh sát. Âm thanh: còi, chim bồ câu bay lên.
- **P1 vỉa hè và xe cảnh sát.** *Không gian:* đường phố rộng khoảng 12 m, vỉa hè rộng 3 m, hai xe cảnh sát nằm chéo chắn đường, dải phong tỏa. *Camera:* đi thẳng từ sau xe cảnh sát ra vỉa hè rồi dừng ở góc trái. *Che chắn:* thùng rác lớn bên trái (thấp), xe van bên phải (ngang), trụ biển bus. *Diễn biến:* W1 1 cướp ló sau thùng rác; W2 1 cướp sau xe van; W3 2 cướp ló lần lượt ở bậc thấp và sau cột. *Hợp lý:* nắng từ sau lưng người chơi nên mặt cướp luôn sáng, dễ nhận ra; chưa có cao độ để học luật nhẹ nhàng.
- **P2 ngõ hông và bãi đỗ xe.** *Không gian:* ngõ rộng 6 m giữa ngân hàng và tòa nhà kế bên, tường gạch, thang thoát hiểm sắt, một xe van và một xe tải nhỏ, bãi đỗ nhỏ phía sau. *Camera:* lướt dọc ngõ rồi dừng ở khoảng trống đầu bãi đỗ. *Che chắn:* xe van (gần), thùng carton (vỡ được), cửa sổ tầng 2 và thang sắt (cao), cuối ngõ (xa). *Diễn biến:* W1 1 cướp sau xe van; W2 bất ngờ: 1 cướp ở cửa sổ tầng 2 và 1 cướp cuối ngõ cùng ló; giảm tải 3–4 s khi camera lia nhìn lối bậc thềm khi cửa chính bị xe tải chắn; W3 2 cướp lần lượt sau thùng và cột. *Hợp lý:* nắng chỉ chiếu một nửa ngõ, nửa còn lại là bóng râm; đặt enemy ở nửa có nắng để dễ thấy.
- **P3 bậc thềm và cửa chính.** *Không gian:* sân trước 15 m, 8 bậc thềm đá, bốn cột đá, cửa kính chính có rèm sắt kéo nửa chừng, bồn hoa hai bên. *Camera:* lên sân trước rồi dừng thấp, sau đó nâng lên góc cao hơn ở bậc thềm. *Che chắn:* bồn hoa (thấp), cột đá (ngang), bậc thềm cao (cao), cạnh cửa. *Diễn biến:* W1 2 cướp ở sân thấp lần lượt; W2 3 cướp dồn dập (tối đa 2 cùng lúc) ở bậc thấp, bậc cao, cạnh cửa; W3 cướp cuối bước ra cửa, kill-zoom, cửa bật mở. *Hợp lý:* bốn cột đá tạo nhịp ló ra rõ ràng; cửa mở là điểm nối sang sảnh level 2.
- **(vòng 9)** Thêm 10 cướp: P1 W4 3 cướp chạy vào từ hai đầu vỉa hè và trụ biển bus; P2 W4 3 cướp từ bãi đỗ xe và thang sắt; P3 W1 thêm 1 cướp ở sân thấp, P3 W3 mới 3 cướp ở bồn hoa hai bên và sau cột đá. Không đổi không gian; hai đầu vỉa hè và lối vào bãi đỗ là chỗ cướp chạy vào từ ngoài khung.

### Level 2 — Sảnh giao dịch (giữa buổi sáng, ánh sáng tràn qua cửa kính lớn)
Không khí: sảnh đá cẩm thạch rộng, ánh nắng qua vách kính mặt tiền tạo vệt sáng dài trên sàn, đèn báo động đỏ nhấp nháy ở trần. Màu: trắng đá, nâu gỗ quầy, đỏ báo động. Âm thanh: chuông báo động, tiếng dội của sảnh.
- **P1 cửa xoay và tiếp tân.** *Không gian:* cửa xoay kính, bàn tiếp tân dài 6 m, quầy hướng dẫn, chậu cây lớn, máy ATM. *Camera:* qua cửa xoay chậm rồi dừng giữa sảnh nhìn bàn tiếp tân. *Che chắn:* bàn tiếp tân (thấp), chậu cây (ngang). *Diễn biến:* W1 1 cướp sau bàn tiếp tân; W2 con tin đầu tiên nhô khỏi quầy, cướp cách xa rõ, có biểu tượng gợi ý; W3 cướp kề con tin, hạ cướp thì H02 (con tin chạy đâm cột). *Hợp lý:* cột sảnh nằm phía trước nên H02 diễn ra mà không chắn camera.
- **P2 dãy quầy giao dịch.** *Không gian:* sáu quầy kính liền nhau, ô giao dịch hẹp, cột vuông giữa các quầy, biển số quầy. *Camera:* trượt ngang dọc dãy quầy, dừng ở góc nghiêng khoảng 25°. *Che chắn:* kính quầy (vỡ được), cột vuông (cứng). *Diễn biến:* W1 2 cướp ló sau kính; W2 bất ngờ: 2 cướp và 1 con tin xen giữa, phải phân biệt; giảm tải: đèn đỏ nhấp nháy; W3 1 cướp và con tin ở xa. *Hợp lý:* nhân viên ngân hàng nấp sau quầy nên có sẵn con tin.
- **P3 khu chờ và cầu thang giữa.** *Không gian:* hàng ghế chờ, máy rút số, cầu thang đá xoắn đôi lên tầng lửng, lan can kính. *Camera:* tiến một đoạn, dừng ngang ghế chờ rồi ngẩng nhẹ tới lan can. *Che chắn:* ghế chờ (thấp), cột (ngang), lan can (cao). *Diễn biến:* W1 cướp ở lan can và cướp ở ghế chờ; W2 3 cướp dồn dập, 1 con tin ló sau lưng ghế rồi tự cúi; W3 cướp cuối chạy lên cầu thang, camera theo lên. *Hợp lý:* cầu thang giữa là lối duy nhất lên tầng lửng nên nối level 3 hợp lý.
- **(vòng 9)** Thêm 7 cướp vào các wave sẵn có (không thêm shot): chạy vào từ cửa xoay, hành lang ATM, cuối dãy quầy, máy rút số, lan can và cầu thang. Ở dãy quầy, một nhân viên quầy **chạy ngang** từ ô quầy số 1 sang cửa thoát hiểm (lối chạy phía trước dãy quầy, không bị kính che). Không có con tin chui lên (sàn đá liền).

### Level 3 — Tầng lửng và văn phòng (cuối buổi sáng, ánh sáng ngày qua tường kính)
Không khí: tầng hành chính trên sảnh, tường kính nhìn xuống sảnh, giấy tờ rải rác, ánh sáng trắng của ban ngày. Màu: xám văn phòng, gỗ ấm. Âm thanh: máy photocopy, thang máy, tiếng bước chân vang.
- **P1 hành lang văn phòng.** *Không gian:* hành lang dài khoảng 20 m, hai dãy cửa văn phòng, tủ hồ sơ, máy lọc nước. *Camera:* tiến thẳng dọc hành lang rồi dừng gần giữa. *Che chắn:* khung cửa (ngang), tủ hồ sơ (thấp). *Diễn biến:* W1 cướp ló từ cửa; W2 cướp cầm súng lộ rõ, biểu tượng gợi ý Justice shot; W3 2 cướp lần lượt. *Hợp lý:* các cánh cửa lần lượt mở tạo nhịp ló ra tự nhiên.
- **P2 phòng họp kính.** *Không gian:* phòng họp tường kính, bàn họp dài, ghế xoay, màn hình, nhìn ra gác thấp (mezzanine). *Camera:* vào cửa kính rồi dừng ở góc cao nhìn xuống bàn họp. *Che chắn:* bàn họp, tủ hồ sơ, lan can gác thấp (cao). *Diễn biến:* W1 1 cướp ở cửa kính, 2 con tin núp dưới bàn ló lên rồi tự cúi; W2 H01 (ba tay súng nhảy xuống, một tên dập mặt); giảm tải: chuông thang máy; W3 2 cướp sau tủ. *Hợp lý:* gác thấp đủ cao để nhảy xuống mà không nguy hiểm; kính vỡ được nên cẩn thận hướng bắn gần con tin.
- **P3 phòng an ninh camera.** *Không gian:* phòng nhỏ, tường đầy màn hình sáng, bàn điều khiển, tủ server. *Camera:* vào cửa rồi dừng thấp nhìn bàn điều khiển. *Che chắn:* bàn điều khiển, tủ server. *Diễn biến:* W1 2 cướp có Justice shot, 1 con tin chui lên từ ô sàn nâng phòng server (thay con tin trói ghế); W2 3 cướp dồn dập; W3 cướp cuối trước màn hình, cửa hầm bật mở. *Hợp lý:* phòng an ninh là nơi có lối xuống hầm kỹ thuật.
- **(vòng 9)** Thêm 10 cướp: P1 W4 3 cướp từ hai dãy cửa văn phòng; P2 thêm 1 cướp ở W1 và 1 cướp nhảy qua bàn họp ở W3, nhân viên **chạy ngang** về thang máy vừa mở; P3 W1 thêm 1 cướp, W3 mới 2 cướp từ phòng server phụ. *Không gian P3 đổi:* phòng an ninh mở thông sang **phòng server có sàn nâng**, tổng chiều sâu ≥ 24 m (enemy vẫn ≥ 12 m); kỹ thuật viên **chui lên** bằng cách nhấc ô sàn nâng (lỗ sàn nhìn thấy rõ, không mọc xuyên sàn liền).

### Level 4 — Kho tiền (đèn trắng, kho ngầm) — vòng 10, trước là level 5
Không khí: tầng kho tiền dưới phòng an ninh, ánh đèn trắng lạnh, kim loại bóng, tia laser trang trí; không có ánh ngày vì nằm sâu dưới đất (ngoại lệ ánh sáng duy nhất). Màu: bạc, xanh thép, vàng thỏi. Âm thanh: cửa kim loại đóng sầm, còi báo động, nhịp hồi hộp. Enemy đứng 18–28 m (khiên người 14–20 m).
- **P1 hành lang an ninh cửa vault.** *Không gian:* hành lang rộng 5 m, dài ≥ 32 m, tường thép, camera trần, hai cửa chớp bên, quầy kiểm soát. *Camera:* ra khỏi thang máy bảo mật, tiến chậm, dừng giữa. *Che chắn:* khe cửa chớp, hốc tường, quầy kiểm soát (thấp). *Diễn biến:* W1 1 cướp ló; W2 khiên người đầu tiên, biểu tượng gợi ý; W3 cướp từ phòng kiểm soát + 1 ló, nhân viên ló sau quầy; W4 3 cướp chạy vào liên tiếp từ hai cửa chớp. *Hợp lý:* cướp cần nhân viên mở cửa vault nên kẹp nhân viên làm khiên; phòng an ninh (level 3) giữ thang máy bảo mật nên đi thẳng xuống đây.
- **P2 phòng két hai tầng.** *Không gian:* hàng tủ két hai tầng, sâu ≥ 30 m, cầu thang sắt, sàn lưới thép, ban công tầng hai, **cửa sập két sàn**. *Camera:* lướt qua hàng két rồi dừng giữa. *Che chắn:* tủ két (cứng), sàn lưới và ban công (cao). *Diễn biến:* W1 2 cướp ló, nhân viên chui lên từ cửa sập; W2 bất ngờ: 2 cướp nhảy từ sàn lưới + 1 xung phong; giảm tải: báo động tắt; W3 khiên người ở két đang mở + xạ thủ trên ban công; W4 2 cướp chạy xuống cầu thang, nhân viên chạy ngang ra cầu thang. *Hợp lý:* sàn lưới cho cao độ và chỗ nhảy xuống; ban công là chỗ đứng tự nhiên của xạ thủ.
- **P3 đại sảnh vault và lõi vàng (stage cuối).** *Không gian:* sảnh tròn đường kính ≥ 36 m, cửa vault tròn khổng lồ ở giữa dẫn vào lõi vàng (thỏi vàng xếp bậc), hai lối bên, xe đẩy tiền, cửa cuốn nạp tiền phía sau lõi vàng. *Camera:* vào sảnh, dừng góc rộng nhìn cửa vault. *Che chắn:* xe đẩy tiền (thấp), bậc vàng, cột. *Diễn biến:* hộp hồi máu; W1 cửa kho phía sau đóng sầm, hai khiên người; W2 3 cướp dồn dập, con tin ở xa; lấy hơi khi cửa vault quay mở; W3 "Dồn dập lõi vàng" 5 cướp chạy ra liên tiếp; W4 tên cầm đầu nhóm vault, cửa cuốn nạp tiền kéo lên. *Hợp lý:* cướp đang chất vàng thì bị bắt gặp nên dồn ra chặn; đường nạp tiền là lối chở tiền xuống hầm xe nên nối level 5.

### Level 5 — Hầm xe và khu hậu cần, tẩu thoát (giữa trưa, ánh sáng ngày lọt qua lối dốc) — vòng 10, trước là level 4
Không khí: hầm bê tông nhiều tầng, ánh sáng giữa trưa lọt qua lối dốc ra vào, đèn tuýp nhấp nháy, ống thông gió, khói xả xe. Màu: xám bê tông, vàng đèn, đỏ thùng nhiên liệu. Âm thanh: động cơ xe bọc thép, lốp rít, tiếng vang, máy bơm. Enemy đứng 18–28 m.
- **P1 bến nạp tiền.** *Không gian:* băng chuyền tiền từ kho xuống, cửa cuốn, máy phát điện dự phòng với hai thùng nhiên liệu đỏ, sâu ≥ 30 m. *Camera:* đi theo băng chuyền rồi dừng. *Che chắn:* băng chuyền (thấp), máy phát (ngang), thùng đỏ. *Diễn biến:* W1 1 cướp ló; W2 2 cướp cạnh thùng đỏ (luật mới); W3 1 xung phong chạy tới cạnh thùng thứ hai, nhân viên áp tải ló. *Hợp lý:* tiền xuống bằng băng chuyền tới chỗ xe bọc thép nhận; thùng nhiên liệu máy phát là lý do có thùng nổ.
- **P2 bãi hầm P-1.** *Không gian:* bãi sâu ≥ 36 m, cột vuông lớn, xe sedan đỗ chéo, xe tải nhỏ, xe bọc thép của ngân hàng, **hố kiểm tra gầm xe**. *Camera:* tiến vào giữa bãi rồi dừng chéo. *Che chắn:* cột, xe (cứng), nóc xe tải (cao). *Diễn biến:* W1 2 cướp sau xe, thùng shotgun thưởng, con tin chui lên từ hố kiểm tra; W2 H03 + khiên người; W3 xung phong + 2 cướp nhảy từ nóc xe tải, thùng nổ giữa bãi. *Hợp lý:* xe bọc thép là xe tẩu thoát mà băng cướp nhắm tới.
- **P3 lối dốc xoắn xuống P-2.** *Không gian:* dốc xoắn rộng 8 m, lan can bê tông tầng trên, nhìn dọc dốc ≥ 30 m. *Camera:* lượn theo dốc (cong mềm, ≤ 5°/m) rồi dừng nhìn xuống dốc. *Che chắn:* lan can (cao), cột giữa dốc, pallet trên xe nâng (di động). *Diễn biến:* W1 bất ngờ: 2 lính khiên cứng tiến xuống dốc, thùng nổ cạnh lan can; giảm tải: xe bọc thép đâm cột, hộp hồi máu; W2 2 cướp nhảy từ lan can + xạ thủ cuối dốc, bảo vệ chạy ngang mặt dốc; W3 kẻ đẩy xe nâng. *Hợp lý:* cướp đem khiên chống bạo động lấy từ kho của đội bảo vệ ngân hàng.
- **P4 kho hậu cần.** *Không gian:* lối kệ thép cao 3 m dài ≥ 34 m, pallet, xe nâng, thùng nhiên liệu. *Camera:* tiến dọc lối kệ rồi dừng chéo. *Che chắn:* kệ cao (cao), pallet (thấp), thùng đỏ. *Diễn biến:* booster giáp + hộp hồi máu; W1 xạ thủ trên kệ + lính giáp; W2 khiên người kẹp thủ kho + 1 cướp cạnh thùng nổ. *Hợp lý:* phase chuẩn bị trước thác enemy nên có vật phẩm.
- **P5 trạm bơm thoát nước (stage cuối).** *Không gian:* phòng bơm lớn sâu ≥ 34 m, hai cửa cuốn kho hai bên, ống bơm to, miệng cống lớn có cửa xả. *Camera:* tiến tới rồi dừng ở trung tâm. *Che chắn:* máy bơm, ống, thùng nổ giữa hai cửa (cách chỗ đội trưởng ≥ 3.5 m). *Diễn biến:* lấy hơi; W1 "Thác enemy" 8 cướp; hết nhịp; W2 đội trưởng kẹp thủ quỹ cạnh miệng cống; cửa xả mở. *Hợp lý:* xe bọc thép bị chặn ở dốc nên băng cướp đổi sang lối thoát qua cống, tiền chuyển xuống qua cửa xả.

### Level 6 — Cống ngầm thoát hiểm (đầu giờ chiều, nắng rọi qua lưới sắt; vòng 10: đổi từ "giữa trưa" để thời gian chạy đúng thứ tự sau level 5)
**(vòng 10)** P1 đổi thành **cửa xả trạm bơm**: cửa xả bị cướp phá bằng thuốc nổ (lửa nhỏ, mảnh vỡ), thang sắt xuống ống cống chính. P2 có 2 cướp **chui lên từ mặt nước** sát bờ; P3 đợt dồn dập có 1 cướp chui lên từ nắp cống ở cửa hầm trái (khác lỗ với con tin chui lên ở W2).
Không khí: đường hầm cống cũ, tia nắng buổi trưa rọi xuống qua miệng cống và lưới sắt tạo cột bụi sáng, hơi nước, đám cháy nhỏ ở mảnh vỡ. Màu: xanh rêu, cam lửa, vệt nắng vàng. Âm thanh: nước chảy, lách tách lửa.
- **P1 cửa hầm cháy.** *Không gian:* cửa hầm sắt vừa bị phá, mảnh vỡ, lửa nhỏ, ống cống tròn lớn. *Camera:* qua cửa hầm rồi dừng giữa ống. *Che chắn:* mảnh vỡ, vòm ống. *Diễn biến:* W1 1 cướp; W2 grenadier đầu tiên, biểu tượng gợi ý; W3 grenadier và cướp lần lượt. *Hợp lý:* lửa từ vụ nổ khi cướp phá lối thoát.
- **P2 kênh cống ngập.** *Không gian:* kênh rộng 8 m, hai bờ đi bộ, cầu đi bộ sắt bắc ngang. *Camera:* đi bờ trái rồi dừng nhìn kênh. *Che chắn:* bờ trái và phải, cầu (cao), ống lớn. *Diễn biến:* W1 2 cướp hai bờ; W2 H08 và con tin ló sau lan can cầu rồi tự cúi; giảm tải: ánh đèn pin; W3 2 cướp lần lượt. *Hợp lý:* tia nắng qua miệng cống làm bờ ướt phản chiếu, hợp với H08.
- **P3 ngã ba hầm bảo trì.** *Không gian:* ba cửa hầm, ống chằng chịt, thùng nổ trên ống. *Camera:* tới ngã ba rồi dừng nhìn ba cửa. *Che chắn:* cửa hầm, ống. *Diễn biến:* W1 2 grenadier lần lượt và thùng nổ; W2 3 cướp và 1 con tin; W3 cướp cuối chạy vào cổng hầm. *Hợp lý:* ngã ba có sẵn thùng bảo trì.
- **(vòng 9)** Enemy đứng 18–28 m, thêm 13 cướp. *Không gian đổi:* ống cống P1 thẳng ≥ 32 m (W4 3 cướp chạy vào từ cuối ống); kênh P2 nhìn dọc ≥ 32 m, cầu đi bộ là lối **chạy ngang** của công nhân cống (cao, tách tầng với cướp dưới bờ); ngã ba P3 ba cửa hầm ở 20–28 m, giữa ngã ba có **cửa thăm cống** (con tin chui lên, cách thùng nổ ≥ 3.5 m), ba cửa hầm là lối vào của đợt "Dồn dập ngã ba" (5 cướp).

### Level 7 — Ga tàu điện ngầm (đầu giờ chiều, đã sơ tán, ánh sáng ngày qua giếng trời)
Không khí: ga vắng sau khi sơ tán, ánh sáng ngày lọt xuống qua giếng trời giữa sân ga, đèn huỳnh quang trắng, quảng cáo, biển điện tử. Màu: trắng gạch, xanh biển báo, đỏ biển cấm. Âm thanh: loa thông báo, tàu xa.
- **P1 sảnh vé và cổng soát vé.** *Không gian:* dãy cổng soát vé, máy bán vé, sơ đồ tuyến, quầy thông tin. *Camera:* đi chậm tới giữa sảnh. *Che chắn:* cổng soát vé (thấp), máy bán vé (ngang). *Diễn biến:* W1 1 cướp; W2 thùng shotgun sau máy bán vé, biểu tượng gợi ý; W3 2 cướp gần nhau. *Hợp lý:* đường hầm cống nối vào sảnh phụ của ga.
- **P2 sân ga.** *Không gian:* sân ga dài, cột lớn, ghế chờ, biển điện tử, tàu vào ga. *Camera:* theo mép sân ga rồi dừng. *Che chắn:* cột (cứng), ghế, băng ghế. *Diễn biến:* W1 2 cướp sau cột; W2 tàu vào ga, 3 cướp và 1 con tin xen từ toa; giảm tải: thông báo ga; W3 grenadier trên băng ghế và 1 cướp. *Hợp lý:* tàu cuối được giữ lại ga nên cướp trốn trên đó.
- **P3 đường ray và tàu hàng.** *Không gian:* đường ray, tàu hàng đỗ, container, tín hiệu. *Camera:* xuống đường ray rồi dừng nhìn toa tàu. *Che chắn:* toa tàu (cao), container, ray. *Diễn biến:* W1 cướp trên toa và thùng nổ giữa ray; W2 3 cướp dồn dập, 1 con tin; W3 cướp cuối nhảy khỏi tàu, tàu hàng chạy qua mở lối. *Hợp lý:* tàu hàng chạy ra cảng nên nối level 8.
- **(vòng 9)** Thêm 10 cướp: P1 W4 3 cướp qua cổng soát vé; P2 W1 thêm 1, nhân viên ga **chui lên** từ hốc trú ẩn dưới mép sân ga (có thật ở ga tàu điện), hành khách **chạy ngang** dọc sân ga tới cầu thang ở W3; P3 W1 thêm 1, W3 mới "Dồn dập sân ray" 4 cướp nhảy khỏi toa theo cặp, có thùng shotgun mới.

### Level 8 — Bến cảng và bãi container (giữa trưa, nắng gắt)
Không khí: cảng hàng hóa dưới nắng trưa gắt, bóng container đổ ngắn, mặt bê tông phản chiếu, gió biển, cần cẩu khổng lồ. Màu: xám thép, cam rỉ sét, xanh biển. Âm thanh: còi tàu, dây cáp, sóng.
- **P1 cổng cảng và trạm hải quan.** *Không gian:* cổng sắt, thanh chắn, chòi hải quan, xe tải. *Camera:* qua cổng rồi dừng nhìn chòi hải quan. *Che chắn:* chòi, xe tải, thanh chắn. *Diễn biến:* W1 1 cướp; W2 thùng súng máy, biểu tượng gợi ý; W3 3 cướp dồn dập thử súng máy. *Hợp lý:* chòi hải quan là điểm chặn tự nhiên.
- **P2 bãi container chồng tầng.** *Không gian:* container rỉ chồng ba tầng, lối đi hẹp, xe nâng container. *Camera:* đi dọc lối rồi dừng nhìn lên. *Che chắn:* container (cứng, ba tầng cao độ), xe nâng. *Diễn biến:* W1 cướp ở ba tầng; W2 grenadier tầng hai, 2 cướp và con tin ló sau container rồi tự cúi; W3 1 cướp nhảy xuống và xạ thủ trên container tầng ba. *Hợp lý:* ba tầng container cho đủ cao độ; nắng gắt làm bóng ngắn nên enemy dễ nhìn.
- **P3 nhà kho lạnh.** *Không gian:* kho lạnh, hơi lạnh bay, kệ thép phủ sương, cửa kho nặng. *Camera:* vào cửa kho rồi dừng chéo. *Che chắn:* kệ, cửa kho, pallet. *Diễn biến:* W1 H03 và 2 con tin ló rồi tự cúi; giảm tải: hơi lạnh, tiếng quạt; W2 khiên người ở cuối kho và con tin chui lên từ hố thông gió sàn; W3 3 cướp dồn dập. *Hợp lý:* cửa kho lạnh dày nên có chỗ ló ra.
- **P4 cầu cảng "Vòng vây".** *Không gian:* cầu cảng bê tông, tàu hàng neo, cần cẩu giàn cao, thùng hàng trên tàu. *Camera:* ra cầu cảng rồi dừng giữa. *Che chắn:* thùng hàng, bích neo, dãy container trái, boong tàu (cao). *Diễn biến (vòng 11, thay nội dung cũ):* lấy hơi, thùng súng máy rơi, "Vòng vây cầu cảng" 7 cướp từ ba hướng, hết nhịp, cướp cuối ngã khỏi tàu. *Hợp lý:* cầu cảng giáp cổng hàng hóa của tòa tháp nên nối level 9; cầu cảng là chỗ bọn cướp tập trung chặn đường truy đuổi.
- **(vòng 9)** Thêm 10 cướp: P1 W4 2 cướp sau xe tải; P2 W3 1 cướp nhảy từ container tầng 3, tài xế xe nâng **chạy ngang** lối container; P3 thêm 1 cướp ở W2, W3 lên 4 cướp. *Không gian P4 đổi:* cầu cảng rộng, khung nhìn gồm dãy container trái, boong tàu (cao) và bích neo phải trong ≤ 21° ngang, 14–24 m; thùng súng máy rơi giữa cầu cảng; thủy thủ **chui lên** từ cửa khoang hàng trên boong ở W1. W2 là stage mới **"Vòng vây cầu cảng"** (7 cướp cùng lúc). *Hợp lý:* băng cướp dồn toàn bộ người còn lại ra cầu cảng để giữ tàu tẩu thoát; súng máy (luật của level này) là công cụ đúng để phá vòng vây.

### Level 9 — Tòa tháp tài chính (đầu chiều, kính phản chiếu nắng)
Không khí: tòa tháp kính cao nhìn ra sông và cảng, nắng chiều dội lên kính gây chói nhẹ (giữ chói dưới mức khó nhìn), sảnh cao hai tầng. Màu: xanh kính, trắng kim loại, vàng nắng. Âm thanh: gió, thang máy, tiếng nứt kính.
- **P1 sảnh tháp.** *Không gian:* sảnh cao hai tầng, vách kính lớn, quầy lễ tân đá đen, tác phẩm điêu khắc. *Camera:* vào từ cổng hàng hóa nối cầu cảng, dừng giữa sảnh. *Che chắn:* vách kính (vỡ được), quầy đá. *Diễn biến:* W1 1 cướp; W2 cướp sau vách kính, biểu tượng gợi ý; W3 2 cướp sau vách lần lượt. *Hợp lý:* cổng hàng hóa của tháp nối thẳng với cầu cảng.
- **P2 thang thoát hiểm và hành lang kính.** *Không gian:* thang bê tông, hành lang kính nhìn ra sông. *Camera:* lên thang rồi dừng đầu hành lang kính. *Che chắn:* vách kính, lan can. *Diễn biến:* W1 2 cướp từ cầu thang; W2 kính vỡ lộ 2 cướp, con tin sau kính; giảm tải: gió qua kính vỡ; W3 grenadier và 1 cướp. *Hợp lý:* kính vỡ thật nên cần chú ý hướng bắn với con tin.
- **P3 sân thượng phụ và giàn điều hòa.** *Không gian:* sân thượng phụ, giàn điều hòa lớn, ống dẫn. *Camera:* ra sân thượng rồi dừng. *Che chắn:* giàn điều hòa, ống. *Diễn biến:* W1 2 cướp và thùng nổ; W2 3 cướp và khiên người; W3 cướp cuối chạy tới cửa mái. *Hợp lý:* sân thượng phụ nối cửa mái bằng cầu thang ngắn.
- **(vòng 9)** Thêm 10 cướp: P1 W4 2 cướp từ thang máy sảnh; P2 W4 2 cướp nhảy từ chiếu nghỉ tầng trên, nhân viên **chạy ngang** đầu hành lang (không có kính chắn giữa người chơi và con tin); P3 kỹ thuật viên **chui lên** từ cửa sập bảo trì trên mái ở W1, W3 mới "Dồn dập sân thượng" 4 cướp từ cửa mái phụ và sau giàn điều hòa, có vách kính chắn gió.

### Level 10 — Sân đáp trực thăng đỉnh tháp (chiều muộn, nắng vàng cam)
Không khí: đỉnh tháp lộng gió, nắng chiều muộn vàng cam chiếu nghiêng, bóng dài, trực thăng đen đậu trên bãi đáp. Màu: đen trực thăng, vàng cam nắng, đỏ cảnh báo. Âm thanh: gió, cánh quạt, nhạc đạt đỉnh.
- **P1 cửa lên mái.** *Không gian:* cánh cửa mái, cầu thang nhỏ. *Camera:* lên cầu thang rồi dừng sát cửa. *Che chắn:* khung cửa, thùng kỹ thuật. *Diễn biến:* mẫu quen: cướp, con tin, Justice shot, khiên người; 3 cướp dồn dập cuối phase. *Hợp lý:* mở màn nhắc lại luật.
- **P2 giàn mái, bồn nước, anten.** *Không gian:* bồn nước to, giàn anten, dây cáp, lan can thấp. *Camera:* đi giữa giàn rồi dừng. *Che chắn:* bồn nước, giàn anten (cao). *Diễn biến:* grenadier sau bồn, cướp trên giàn, thùng nổ cạnh bồn, con tin xen; hộp hồi máu cuối phase. *Hợp lý:* giàn anten tạo cao độ tự nhiên.
- **P3 bãi đáp và xe chở tiền.** *Không gian:* bãi đáp rộng, xe điện chở hàng nhỏ chất thùng tiền (đưa lên bằng thang máy hàng), vạch bãi đáp. *Camera:* ra bãi đáp rồi dừng. *Che chắn:* xe chở tiền (cứng), thùng tiền. *Diễn biến:* giai đoạn 1: thủ lĩnh nấp sau xe, đồng bọn ném thuốc nổ; sau khi bị thương lần 1 hắn bỏ chạy; giảm tải. *Hợp lý:* xe điện nhỏ lên được bằng thang máy hàng.
- **P4 trực thăng cất cánh.** *Không gian:* trực thăng đen, cánh quạt quay. *Camera:* tiến tới trực thăng rồi dừng. *Che chắn:* thân trực thăng, bánh. *Diễn biến:* giai đoạn 2: thủ lĩnh lên trực thăng, đồng bọn trên thân ném thuốc nổ, khiên người; hộp hồi máu đầu phase. *Hợp lý:* gió cánh quạt làm bụi mù nhưng không che khung nhìn.
- **P5 mép bãi đáp.** *Không gian:* mép bãi đáp không rào chắn, thành phố và sông phía dưới. *Camera:* tiến tới mép rồi dừng. *Che chắn:* thủ lĩnh dùng con tin làm khiên. *Diễn biến:* (vòng 9) trước giai đoạn 3 là "Đợt cuối Rắn Đỏ": 6 đồng bọn chạy lên từ cầu thang mái và thang máy hàng; rồi giai đoạn 3: Justice shot và điểm yếu; kill-zoom; kết thúc: nắng chiều, còi cảnh sát. *Hợp lý:* kết thúc chuỗi 10 level; đồng bọn cuối cùng lên chặn cảnh sát để thủ lĩnh có thời gian chạy ra mép bãi đáp.
- **(vòng 9)** ~~Tổng vẫn 30 (trần), phân bổ lại P1 6, P2 7, P3 5, P4 6, P5 6.~~ **(vòng 11) Tổng 40, phân bổ P1 9, P2 9, P3 7, P4 8, P5 7.** P2 có **cửa sập kỹ thuật** giữa bồn nước và giàn anten (con tin chui lên); P3 nhân viên áp tải **chạy ngang** bãi đáp từ xe chở tiền ra cửa thang; P5 cần cầu thang mái và cửa thang máy hàng nằm ngoài khung làm lối vào cho 6 đồng bọn.

### Đánh giá tính hợp lý (đã rà)
- **Mạch thời gian:** sáng (level 1) đến chiều muộn (level 10), mỗi level có ánh sáng ngày; level 4 (kho tiền, vòng 10) nằm sâu dưới đất nên dùng đèn, là ngoại lệ duy nhất.
- **Không gian liền mạch (vòng 10):** 1→2 (cửa chính), 2→3 (cầu thang), 3→4 (thang máy bảo mật xuống kho tiền), 4→5 (đường chuyển tiền ra hầm xe), 5→6 (cửa xả trạm bơm xuống cống), 6→7 (cổng hầm vào ga), 7→8 (tàu hàng ra cảng), 8→9 (cầu cảng nối cổng hàng hóa tòa tháp), 9→10 (cửa mái).
- **Rủi ro cần kiểm khi dựng:** kính vỡ gần con tin (level 2, 3, 9); thùng nổ phải cách con tin ≥3.5 m; nắng chói trên kính level 9; bóng râm làm khó nhìn enemy ở ngõ level 1, nên đặt enemy ở vùng có nắng; số enemy cùng lúc không vượt trần theo độ khó. **(vòng 9)** Thêm: đường chạy của con tin chạy ngang và lỗ chui của con tin chui lên cũng phải cách thùng nổ ≥ 3.5 m và không có kính giữa người chơi và con tin; ở 18–28 m (level 4–6) đặt enemy chỗ sáng, nền tương phản, không đứng sát mép khung.

## Đã chốt (vòng hỏi 6–7)
- **Cứu con tin** = hạ kẻ canh con tin; con tin tự chạy khỏi cảnh; cứu được thì cộng điểm rank; bắn nhầm con tin mất tim và reset combo. **(vòng 9)** Con tin chạy ngang không có kẻ canh: để nó chạy qua an toàn là "cứu" ~~(cộng điểm như cứu)~~ **(vòng 11) không cộng điểm cứu** (chỉ cần không bắn nhầm; "cứu" có điểm chỉ khi hạ kẻ canh con tin); con tin chui lên tự cúi sau ~3 s, không bị bắn nhầm. **(vòng 11) Con tin không bao giờ đứng im:** hoặc chạy ngang qua màn hình, hoặc nấp đi (cúi/chui xuống/thụt vào chỗ nấp) trông tự nhiên. Ba kiểu xuất hiện xem "Đã chốt (vòng hỏi 9)".
- **Không có tính năng radio** và không có lời thoại chữ/phụ đề trong cảnh. Gợi ý luật mới bằng biểu tượng, hiệu ứng sáng và hoạt ảnh; nhịp giảm tải dùng âm thanh môi trường, chuyển động camera và reload. UI điểm/rank/nút giữ như hiện có.
- **Hồi sinh bằng quảng cáo:** tối đa 2 lần mỗi level, hồi sinh tại đúng vị trí; không ảnh hưởng rank (rank chỉ phụ thuộc điểm và độ chính xác). Thoát ra thì chơi lại từ đầu level đó.
- **Kết thúc level:** camera đi thêm một đoạn tới cảnh bắt đầu của level sau, rồi hiện bảng kết quả dừng chờ bấm "Tiếp tục"; camera đứng ở cảnh đầu level sau, thở nhẹ trong lúc chờ. Level 10 kết thúc chuỗi, không có đoạn đi thêm.

### Quy tắc chuyển stage (vòng hỏi 8, áp dụng cho tất cả stage)
- Giả định: "stage" = phase (P1, P2, P3...) trong một level. Nếu bạn muốn nói cả chuyển giữa các level thì quy tắc này đã khớp với quy tắc kết thúc level ở trên.
- Kết thúc stage này, nhân vật **di chuyển liền mạch** tới stage kế và bắt đầu ngay: **không tối màn hình, không fade đen, không cắt cảnh**. Đoạn Move cuối của mỗi stage phải kết thúc đúng vị trí và hướng nhìn của shot đầu stage kế (đã là quy tắc rail).
- Khi bắt đầu stage mới vẫn **hiện chữ stage** (tiêu đề) như hiện nay, nhưng hiện chồng lên cảnh đang chạy, không chặn gameplay và không cần màn hình đen.
- Đây là ngoại lệ có chủ đích của quy tắc "không dùng chữ": tiêu đề stage vẫn dùng chữ.
- Hệ quả: không dùng nhảy xa kiểu cũ giữa các phase (đoạn Cut 100 m); mọi stage phải nối bằng rail liên tục. Code hiện có `PhaseDirector` và `PhaseFade` đang fade đen khi đổi phase, cần đổi thành tiêu đề không fade.
- **(vòng 9) Stage dồn dập** (stage cuối level 4–10, "Thác enemy" L5 P5, "Vòng vây cầu cảng" L8 P4) chuyển vào và ra như mọi stage (liền mạch, không fade, vẫn hiện tiêu đề stage). Thêm hai nhịp bắt buộc: (1) **lấy hơi** 1–1.5 s sau khi camera dừng và trước enemy đầu tiên (cảnh báo bằng âm thanh + mũi tên ở mép màn hình chỉ hướng enemy sẽ vào, không chữ; người chơi reload được); (2) **hết nhịp** 2–3 s sau enemy cuối của đợt dồn dập, không enemy, trước wave/kill-zoom tiếp theo hoặc trước đoạn Move.
- **(vòng 9)** Không bắt đầu đợt dồn dập khi camera còn đang Move; tiêu đề stage không được che vùng enemy chạy vào (tiêu đề tắt trước khi hết nhịp lấy hơi).
- **(vòng 9)** Wave chưa xong thì không chuyển stage: con tin chạy ngang phải chạy hết ra khỏi khung, con tin chui lên phải cúi xuống xong, rồi mới tính wave kết thúc.

### Lưu dữ liệu và dây level (về sau)
- Lưu dữ liệu từng level: trạng thái mở, rank tốt nhất, điểm cao nhất, số lần chơi.
- Về sau nối thành "dây level" và hiện hạng từng level để chơi lại; menu chọn level chưa làm trong đợt này.

## Đã chốt (vòng hỏi 9) — mật độ enemy, con tin mới, stage dồn dập
Người dùng yêu cầu 2026-10-06. Mọi chỗ đổi trong file đánh dấu "(vòng 9)". Số enemy level 4–6 tính theo kịch bản viết lại ở vòng 10.

### Số enemy và khoảng cách
| Lv | Gốc (cũ) | Vòng 9 | **Vòng 11 (hiện hành)** | Phân bổ theo phase (vòng 11) | Thời lượng dự kiến | Khoảng cách enemy (xem bảng vòng 11) |
|---|---|---|---|---|---|---|
| 1 | 15 | 25 | **25** | 7 / 8 / 10 | ~165 s | 12–22 m + xa 26–30 m |
| 2 | 23 | 30 (trần) | **33** | 10 / 10 / 13 | ~180 s | 12–22 m + xa 26–32 m |
| 3 | 14 | 24 | **24** | 7 / 8 / 9 | ~165 s | 12–22 m + xa 26–32 m |
| 4 kho tiền | 14 | 27 | **27** (+13) | 7 / 9 / 11 | ~175 s | 18–28 m + xa 26–35 m |
| 5 hầm xe | 26 | 30 (trần) | **36** | 5 / 8 / 8 / 5 / 10 | ~230 s | 18–28 m + xa 26–35 m |
| 6 cống | 13 | 26 | **26** (+13) | 7 / 8 / 11 | ~170 s | 18–28 m + xa 26–35 m |
| 7 | 14 | 24 | **24** | 6 / 8 / 10 | ~170 s | 12–24 m + xa 26–32 m |
| 8 | 20 | 30 | **30** | 6 / 8 / 8 / 8 | ~200 s | 12–24 m + xa 26–35 m |
| 9 | 14 | 24 | **24** | 6 / 8 / 10 | ~170 s | 12–24 m + xa 26–32 m |
| 10 | 30 | 30 + thủ lĩnh (trần) | **40 + thủ lĩnh** | 9 / 9 / 7 / 8 / 7 | ~335 s | 12–26 m + xa 26–35 m |
- **Trần enemy mỗi level: ~~30~~ → 50 (vòng 11, mọi level)** (không tính thủ lĩnh, không tính tên tự ngã trong gag). Level 2, 5, 10 nay đủ +10 so với số gốc (33 / 36 / 40). Level 4 và 6 giữ +13 (yêu cầu "nhiều hơn"). Số đồng thời tối đa trên màn hình **không đổi** (4; xem dưới).
- **Trần cùng lúc:** 4 enemy trên màn hình cho wave thường; Easy 2, Normal 3, Hard/Boss 3–4 như bảng vòng 3; stage cuối từ level 4 được +1 (Easy 3, còn lại 4). Hai ngoại lệ có tên: "Thác enemy" (6 trên màn hình, 4 vòng đếm) và "Vòng vây cầu cảng" (7 trên màn hình, 4 vòng đếm).
- **Lý do 18–28 m ở level 4–6:** thân người 1.8 m ở 28 m chiếm khoảng 3.7° dọc, bằng 8% chiều cao khung khi FOV dọc 45° (khoảng 157 px trên màn 1920 px), vẫn lớn hơn bán kính chạm 90 px (chuẩn 1080p) của Pistol; xa hơn nữa sẽ khó đọc trên điện thoại dọc. Enemy chạy vào ở 28 m cần chạy khoảng 7.7 m (nửa bề ngang khung + 1.2 m) ≈ 1.7 s ở 4.5 m/s, vẫn nằm trong 0.5–2.2 s của luật chạy vào. Cụm 21° ở 28 m rộng khoảng 10 m. Tối thiểu 80% enemy trong dải 18–28 m, tối đa 20% ở 14–18 m để giữ nhịp gần/xa. **Khiên người và Justice shot đặt 14–20 m** vì mục tiêu nhỏ (tay, đầu, súng).
- Đối chiếu validator (`LevelValidationRules.asset`): `minTargetDistance` 12 m chung, chưa có kiểm khoảng cách tối đa (**vòng 11: cần thêm `maxTargetDistance` = 35 m, nới ngưỡng**); `maxEnemiesTotal` L1 24, L2 28, L3 20, L6 20 (thấp hơn số mới), L5 32, chưa có luật riêng cho Level_04 (dùng fallback 8/wave, 80 tổng, 6 cùng lúc), chuỗi `maxEnemiesPerLevel` 32 (**vòng 11: nâng cho đủ số mới, ≤ 50**); `maxEnemiesPerWave` 4 và `maxConcurrentCap` 3 thấp hơn các đợt dồn dập (5–8 enemy/wave, 4–7 cùng lúc). Cần PM giao chỉnh (xem Câu hỏi mở).

### Con tin: ba kiểu xuất hiện
| Kiểu | Hành vi | Con số | Công bằng |
|---|---|---|---|
| **Ló** (đã có) | Ló sau vật nấp, **tự cúi/nấp xuống sau ~3 s** (vòng 11: bỏ lựa chọn "đứng cố định tới hết wave"; con tin che enemy thì tự nấp hoặc chạy đi sau ~3 s) | lộ 3 s | Như cũ |
| **Chui lên** (mới) | Từ lỗ nhìn thấy được trong khung từ đầu wave (nắp cống, cửa sập, ô sàn nâng, hố kiểm tra xe, hốc dưới sân ga, khoang hàng); nhô lên tới ngang ngực, lộ rồi tự cúi xuống lại | báo trước 0.4 s (nắp rung/bọt nước), nhô 0.6 s, lộ ~3 s, chìm 0.5 s | Không mọc xuyên sàn liền; lỗ cách thùng nổ ≥ 3.5 m; tách ≥ 5° ngang với mọi enemy |
| **Chạy ngang** (mới) | Từ ngoài khung một bên chạy thẳng ra ngoài khung bên kia (hoặc giữa hai góc khuất trong khung), **không bao giờ dừng, không cúi** | 3.0–4.0 m/s (chậm hơn enemy 4.5 m/s), lộ 2.5–4 s; dáng chạy khác enemy (tay ôm đầu, áo dân thường) | Tối đa 1 mỗi wave, 1 mỗi level ngoài số con tin; chạy ngay đầu wave, trước vòng target đầu tiên; khi che một enemy (chồng trong 5°) thì vòng của enemy đó còn ≥ 1.0 s hoặc đang ẩn; đường chạy cách thùng nổ còn nguyên ≥ 3.5 m; không có kính giữa người chơi và con tin; khác tầng cao độ với cụm enemy khi người chơi có thể đang cầm súng máy |
- **(vòng 11) Con tin chạy ngang qua an toàn KHÔNG được cộng điểm cứu** (không cộng, không trừ). Mọi con tin đều không đứng im: ló thì tự cúi, chui lên thì tự chìm, chạy ngang thì chạy hết ra khỏi khung. Con tin trói ghế (nếu có) chỉ là **cảnh cố định** nằm ngoài vùng enemy, không che enemy, không phải mục tiêu và không đếm vào số con tin; con tin bị kẹp làm khiên người thuộc cơ chế khiên (không tính "đứng im"). Con tin che enemy (chồng trong 5°) phải tự nấp hoặc chạy đi sau ~3 s.
- Chạm vào con tin (mọi kiểu) = mất tim và reset combo như luật hiện tại. 5° ngang > 2 lần bán kính chạm 90 px chuẩn 1080p (khoảng 4.2° ở FOV dọc 45°), nên một lần chạm không thể trúng cả enemy lẫn con tin.
- **Không có con tin** trong đợt dồn dập, Thác enemy, Vòng vây cầu cảng và đợt cuối Rắn Đỏ.
- Phân bổ: L1 không có (luật con tin bắt đầu ở L2). L2 chạy ngang P2 S3. L3 chạy ngang P2 W3, chui lên P3 W1 (sàn nâng). L4 chui lên P2 W1 (cửa sập két sàn), chạy ngang P2 W4. L5 chui lên P2 W1 (hố kiểm tra xe), chạy ngang P3 W2 (mặt dốc). L6 chạy ngang P2 W3 (cầu đi bộ), chui lên P3 W2 (cửa thăm cống). L7 chui lên P2 W1 (hốc dưới sân ga), chạy ngang P2 W3. L8 chạy ngang P2 W3, chui lên P4 W1 (khoang hàng). L9 chạy ngang P2 W4, chui lên P3 W1 (cửa sập mái). L10 chui lên P2 (cửa sập kỹ thuật), chạy ngang P3.

### Stage cuối dồn dập (level 4–10)
| Lv | Stage cuối | Enemy trong stage | Đợt dồn dập | Nhịp vào | Trên màn hình / vòng đếm cùng lúc | Vòng target | Công cụ hạ nhiều |
|---|---|---|---|---|---|---|---|
| 4 | P3 đại sảnh vault, lõi vàng | 11 | "Dồn dập lõi vàng" 5 | 0.8 s, 3 lối | 3 / 3 | 2.5 s | — (Easy, 5 tên) |
| 5 | P5 trạm bơm | 10 | **"Thác enemy" 8** | 0.7 s, không chờ, 2 cửa | 6 / 4 | 2.0 s (thay 1.6) | thùng nổ |
| 6 | P3 ngã ba hầm | 11 | "Dồn dập ngã ba" 5 | 0.8 s, 3 cửa | 3 / 3 | 2.5 s | thùng nổ thứ hai |
| 7 | P3 đường ray | 10 | "Dồn dập sân ray" 4 | 0.8 s, theo cặp | 3 / 3 | 2.5 s | thùng shotgun |
| 8 | P4 cầu cảng (vòng 11: nội dung P4 là vòng vây) | 8 (7 + 1) | **"Vòng vây cầu cảng" 7** | gần cùng lúc (so le 0.25 s), 3 hướng | 7 / 4 | 2.4 s (thay 2.0) | thùng súng máy |
| 9 | P3 sân thượng phụ | 10 | "Dồn dập sân thượng" 4 | 0.8 s, 2 lối | 3 / 3 | 2.5 s | — (vách kính là thử thách) |
| 10 | P5 mép bãi đáp | 7 + thủ lĩnh | "Đợt cuối Rắn Đỏ" 6 | 0.6 s, 2 lối | 4 / 4 | 1.8 s (thay 1.4) | thùng nổ |

### Thác enemy (level 5, P5) — "enemy chạy ra liên tiếp"
8 enemy chạy ra luân phiên từ hai cửa cuốn, mỗi 0.7 s một tên, không chờ tên trước bị hạ; dòng chỉ tạm ngừng khi đã có 6 tên còn sống trên màn hình. Tối đa 4 vòng target đếm cùng lúc; tên thứ 5–6 đứng ngắm chờ (ẩn vòng, vẫn bắn hạ được). Toàn đợt khoảng 14–20 s. Lý do đặt ở stage cuối level 5 (Hard): level duy nhất trong nửa đầu có 5 phase, có booster giáp và hộp hồi máu ngay phase trước; bối cảnh băng cướp dồn toàn lực giữ lối thoát qua cống.

### Stage mới: "Vòng vây cầu cảng" (level 8, P4) — 7 enemy tấn công cùng lúc
**(vòng 11, đã chốt):** chọn phương án C và **thay toàn bộ nội dung P4 của level 8**, không thêm phase. So sánh: (A) Level 5 stage cuối: đã có Thác enemy, chồng thêm thì quá tải và level 5 không có súng máy. (B) Level 7 stage cuối: Easy chỉ 2–3 cùng lúc, nhảy lên 7 phá đường cong độ khó, shotgun chỉ hạ được cặp. (C) **Khuyến nghị: Level 8 P4** — luật mới của level 8 chính là "súng máy, nhiều mục tiêu cùng lúc", nên stage cuối là bài kiểm tra đúng luật; cầu cảng rộng đủ ba hướng; level 9 sau đó là Easy để giảm tải. Thay nội dung P4 chứ không thêm phase (giữ 4 phase Normal) — **đã chốt vòng 11**.
Thiết kế: hết nhịp 3 s + thùng súng máy rơi giữa cầu cảng + 1.5 s cảnh báo (còi tàu dài, mũi tên ba hướng); 7 enemy xông ra so le 0.25 s (3 container trái, 2 boong tàu cao, 2 bích neo phải), cả cụm trong 21° ngang, 14–24 m; 4 vòng đếm cùng lúc, vòng 2.4 s, so le ≥ 0.4 s; đạn súng máy đủ hạ 7 tên dư 50%; không có con tin; sau đợt hết nhịp 3 s rồi enemy cuối. (Vòng 11: đây là toàn bộ P4, tổng 8 enemy gồm 1 xạ thủ ở cụm.)

### Luật công bằng chung cho đợt dồn dập
1. Lấy hơi 1–1.5 s trước enemy đầu tiên: âm thanh + mũi tên ở mép màn hình chỉ hướng vào (không chữ), reload được.
2. Enemy không bắn khi đang chạy; vòng target đầu tiên của một tên hiện ≥ 0.8 s sau khi tên đó dừng.
3. Tối đa 4 vòng đếm cùng lúc (Easy stage cuối 3), bắt đầu so le ≥ 0.4 s để không hai vòng hết cùng lúc.
4. Đợt đặc biệt (Thác, Vòng vây, Đợt cuối Rắn Đỏ) cộng 0.4 s vào vòng target của độ khó.
5. Mỗi đợt từ 5 enemy trở lên có một công cụ hạ nhiều mục tiêu theo luật đã học (thùng nổ, shotgun, súng máy).
6. Hết nhịp 2–3 s sau đợt. Enemy vào từ lối nhìn thấy được (cửa, góc khuất), không mọc giữa khung.
7. **(vòng 11, đã chốt)** Sau khi người chơi mất tim: **bất tử 0.5 s** (không phải 1.5 s) và các vòng đang đếm dừng lại trong 0.5 s đó. Cần đổi `GameConfig.invulnerableSeconds` từ 1.5 xuống 0.5.

### Metric kiểm tra "vui"
- Tỉ lệ lượt mất ≥ 2 tim trong một đợt dồn dập: ≤ 25% (Easy), ≤ 40% (Normal/Hard/Boss).
- Thác enemy hoàn thành trong 14–20 s; < 10 s là quá dễ, > 25 s là quá dài.
- Bắn nhầm con tin chạy ngang/chui lên: 5–15% lượt (thấp hơn = không có áp lực, cao hơn = không công bằng).
- Độ chính xác ở 18–28 m không thấp hơn quá 10 điểm % so với 12–22 m; nếu tụt hơn thì giảm khoảng cách hoặc hẹp FOV combat.
- ≥ 60% lượt dùng công cụ hạ nhiều trong đợt dồn dập.
- Không có khoảng "không có gì để bắn" > 4 s ngoài nhịp giảm tải chủ ý; thời lượng đo thật trong ±15% bảng.

## Đã chốt (vòng hỏi 10) — viết lại level 4–6, enemy đa dạng
Người dùng yêu cầu 2026-10-06. Chỗ đổi đánh dấu "(vòng 10)".

### Thứ tự bối cảnh mới
**Level 4 kho tiền → Level 5 hầm xe và khu hậu cần (tẩu thoát) → Level 6 cống ngầm.** Lý do: băng cướp vào để cướp nên phải tới kho tiền trước rồi mới chở tiền đi; đường đi một chiều "xuống sâu rồi thoát ra": phòng an ninh (giữ thang máy bảo mật) → kho tiền → đường chuyển tiền (băng chuyền) ra bến nạp tiền của hầm xe → xe bọc thép bị chặn ở dốc nên đổi sang lối trạm bơm → cống → ga tàu. Bản cũ đi hầm xe trước kho tiền là "đi ra rồi quay vào", và lối tường bí mật trong lõi vàng xuống cống không có lý do. Đã cân nhắc phương án giữ hầm xe ở level 4 làm "đường đột nhập" nhưng bị loại vì cảnh sát bám theo cướp, không có lý do đi đường vòng.
- Điểm nối: 3→4 cửa thang máy bảo mật sau phòng an ninh; 4→5 cửa cuốn nạp tiền sau lõi vàng; 5→6 cửa xả trạm bơm; 6→7 giữ nguyên (cổng hầm vào ga).
- Luật mới theo level: L4 **khiên người** (nhân viên kho tiền bị bắt mở két, hợp bối cảnh); L5 **thùng nổ** (thùng nhiên liệu máy phát, bãi xe); L6 **thuốc nổ ném** giữ nguyên. Độ khó giữ theo số level: L4 Easy 3 phase, L5 Hard 5 phase, L6 Easy 3 phase. Booster giáp, 2 hộp hồi máu, thùng shotgun thưởng vẫn ở level 5; H03 theo hầm xe sang level 5; level 4 chưa có gag.
- Thời gian trong ngày: L4 đèn (ngầm), L5 giữa trưa, L6 đổi sang đầu giờ chiều.

### Nhịp mới (áp cho level 4–6)
Mỗi phase kết thúc bằng một wave cao trào (đông nhất của phase), không có shot chỉ để chờ; giảm tải đúng một lần mỗi level Easy (sau bất ngờ giữa level), hai lần ở level 5; kiểu tấn công đổi mỗi wave (ló → chạy vào liên tiếp → nhảy xuống → xung phong → chui lên → khiên → ném thuốc nổ); stage cuối đông nhất. Mỗi level giới thiệu một luật mới + tối đa hai loại enemy mới, loại mới không xuất hiện lần đầu ở stage cuối.

### Bảng loại enemy
| Mã | Loại | Hành vi | Tín hiệu cảnh báo | Cách hạ | Công bằng | Code |
|---|---|---|---|---|---|---|
| T | Thường (ló) | Ló sau vật nấp, ngắm, bắn | Vòng target đỏ | 1 phát | Như cũ | Có sẵn (EnemyActor) |
| C | Chạy vào | Chạy vào từ ngoài khung rồi đứng | Bước chân | 1 phát | Không bắn khi chạy | Có sẵn (EnemyActor run-in, SpawnPointEntry Door/Slide/Vault) |
| D | Nhảy từ trên xuống | Nhảy từ gác/sàn lưới/nóc xe | Bụi rơi 0.3 s | 1 phát (cả lúc đang rơi) | Vòng chỉ hiện khi đã đáp | Có sẵn (Drop) |
| X | **Xung phong** | Chạy thẳng về phía camera, dừng ở 14 m (≥ 12 m validator) rồi bắn ngay | Tiếng hét + vòng target vàng hiện từ lúc chạy | 1 phát, bắn được khi đang chạy | Vòng đếm chỉ bắt đầu khi đã dừng; tối đa 1 X mỗi wave thường | **Mới nhỏ**: hướng chạy về camera thay vì ngang |
| S | **Xạ thủ** (vòng 11: tới 35 m) | Đứng xa **24–35 m** hoặc trên cao (ban công, kệ cao, anten, toa tàu), bắn một phát chắc chắn trúng | Tia laser đỏ chỉ về camera **1.5 s** trước khi bắn + **chớp nòng sáng 0.3 s** ngay trước phát bắn (nhìn thấy cả khi xa nhỏ) | 1 phát (Pistol/súng máy; shotgun kém hiệu quả ở xa) | Vòng target **+0.5 s** (ở 30–35 m **+0.8 s**); tối đa 1 S trên màn hình; chỉ một phát mỗi lần ngắm, laser không bám theo sau khi camera chuyển; S ở 30–35 m phải đứng chỗ sáng, nền tương phản | **Mới**: hiệu ứng laser + chớp nòng + thời gian ngắm riêng |
| K | Khiên người | Kẹp con tin, lộ tay súng/đầu | Vòng sáng ở tay súng | Bắn tay súng/đầu | 14–20 m; bắn trúng con tin = mất tim | Có sẵn (HumanShieldEnemy) |
| KC | **Khiên cứng** | Cầm khiên thép tiến chậm 1 m/s, dừng hạ khiên để bắn | Viền khiên sáng + tay súng sáng khi hạ khiên (cửa sổ 1.2 s) | Bắn tay/thân khi hạ khiên, hoặc thùng nổ | Đạn vào khiên nảy tia lửa (không phạt combo); chỉ bắn khi đã hạ khiên đủ 1.2 s | **Mới** (vùng trúng như HumanShieldEnemy, thêm trạng thái hạ/giơ khiên) |
| GI | **Lính giáp** | Như T nhưng cần 2 phát | Mũ sắt, áo giáp màu sẫm | Phát 1 làm bay mũ, tia lửa, khựng 0.5 s; phát 2 hạ | Khựng 0.5 s làm chậm phát bắn kế; tính 1 kill | **Mới nhỏ**: máu 2 cho EnemyActor |
| ĐX | **Kẻ đẩy xe** | Đẩy xe nâng/xe đẩy làm vật che tiến lên 1.2 m/s, dừng thì ló bắn | Tiếng bánh xe, thùng nổ trên pallet nhấp nháy | Bắn thùng nổ trên xe, hoặc bắn khi hắn ló | Xe dừng ≥ 14 m; 1 ĐX mỗi wave | **Mới**: vật che di động |
| G | Ném thuốc nổ | Ném thuốc nổ theo đường cong | Vệt sáng | Bắn rơi thuốc nổ hoặc bắn người | Như cũ; ở 18–28 m thời gian bay không ngắn hơn bản cũ | Có sẵn (Grenade, ConfigureGrenadiers) |
| CC | **Chui cống** | Chui lên từ nắp cống/mặt nước, lộ ngang ngực, ngắm | Nắp cống rung hoặc bọt nước sủi 0.5 s | 1 phát | Lỗ nhìn thấy được từ đầu wave; lỗ của CC khác lỗ của con tin chui lên | **Mới nhỏ**: kiểu xuất hiện mới (giống Drop ngược) |

| Level | Loại enemy dùng | Loại mới giới thiệu |
|---|---|---|
| 4 kho tiền | T, C, D, K, X, S | K (luật mới), X, S |
| 5 hầm xe | T, C, D, K, X, S, KC, GI, ĐX | KC (bất ngờ giữa level), GI, ĐX |
| 6 cống | T, C, G, X, S, GI, CC | G (luật mới), CC |
| 7–10 | dùng lại tất cả, không thêm loại | — |

**(vòng 11)** Cách đọc: xạ thủ S laser xuất hiện từ level 4; ở level 1–3 enemy xa (26–32 m) là **T xa bình thường**, không laser, vòng target +0.5 s. Mọi level có phân bố khoảng cách gần/vừa/xa riêng (ghi ở đầu mỗi storyboard).

## Đã chốt (vòng hỏi 11) — trần 50, vòng vây thay P4, con tin không đứng im, bất tử 0.5 s, enemy xa 35 m
Người dùng yêu cầu 2026-10-06. Chỗ đổi đánh dấu "(vòng 11)".
1. **Trần enemy 50 cho TẤT CẢ level** (thay trần 30). Mỗi level được +10 so với số gốc: L1 25, L2 **33**, L3 24, L4 27 (+13), L5 **36**, L6 26 (+13), L7 24, L8 30, L9 24, L10 **40** + thủ lĩnh. Số đồng thời tối đa trên màn hình vẫn nhỏ (4; ngoại lệ có tên: Thác 6/4, Vòng vây 7/4).
2. **"Vòng vây cầu cảng" THAY nội dung P4 của level 8**, không thêm phase (level 8 vẫn 4 phase). P4 = lấy hơi, vòng vây 7 enemy, hết nhịp, enemy cuối (8 enemy); phần cũ của P4 bỏ, con tin chui lên chuyển sang P3 W2.
3. **Con tin chạy ngang qua an toàn KHÔNG cộng điểm cứu.**
4. **Con tin không bao giờ đứng im:** hoặc chạy ngang qua màn hình, hoặc nấp đi (cúi / chui xuống / thụt vào chỗ nấp) trông tự nhiên. Con tin che enemy phải tự nấp/chạy đi sau ~3 s. Con tin trói ghế (nếu có) chỉ là cảnh cố định, không che enemy. **Việc cần làm:** sửa mục 2 `Docs/Team/LevelBuildingRules.md` ("Hostage đứng im tại chỗ cho tới hết wave") theo quy tắc này (chưa sửa).
5. **Bất tử 0.5 s sau khi người chơi mất tim** (không phải 1.5 s). Code hiện đặt `GameConfig.invulnerableSeconds = 1.5`, cần đổi 0.5 (việc code, chưa làm).
6. **Level 5 giới thiệu 3 loại enemy mới (KC, GI, ĐX) cùng lúc: chấp nhận được.**
7. **Đa dạng khoảng cách, không bắt buộc đứng gần:** có enemy xa như xạ thủ, **tới 35 m** (trần khoảng cách 35 m). Level 4–6 vẫn xa hơn Level 1–3.

### Khung hình 9:16 và validator ở 35 m
- Thân người 1.8 m ở 35 m chiếm ~2.95° dọc = 6.5% chiều cao khung (FOV dọc 45°), khoảng 125 px trên màn 1920 px; bề ngang thân ~0.5 m ≈ 33 px (1080p). Bán kính chạm 90 px (Pistol) vẫn phủ được; đầu/tay (Justice shot, khiên người) quá nhỏ nên **chỉ đặt ở 14–20 m**.
- Chạy vào ở 35 m: nửa bề ngang khung ≈ 8.2 m + 1.2 m ≈ 9.4 m, ở 4.5 m/s ≈ 2.1 s, vẫn trong 0.5–2.2 s. Không cho enemy chạy vào từ xa hơn 35 m.
- Xạ thủ xa: vòng target +0.5 s (30–35 m +0.8 s), laser 1.5 s + chớp nòng 0.3 s, đứng chỗ sáng và nền tương phản. Shotgun giảm hiệu quả ở xa (S phải hạ bằng Pistol/súng máy).
- Validator: chưa có kiểm khoảng cách tối đa; **cần thêm `maxTargetDistance` 35 m** và nới ngưỡng khoảng cách theo level, nâng `maxEnemiesTotal` từng level và `maxEnemiesPerLevel` chuỗi (≤ 50). Chiều sâu nhìn thấy ≥ 38 m ở mọi khu có xạ thủ xa.

### Phân bố khoảng cách theo level (gần 12–18 m / vừa 18–26 m / xa 26–35 m)
| Lv | Gần | Vừa | Xa | Xạ thủ / enemy xa nằm ở |
|---|---|---|---|---|
| 1 | 15 | 8 | 2 | T xa (không laser): P2 W2 cuối ngõ, P3 W1 sân thấp |
| 2 | 20 | 10 | 3 | T xa: P1 S3 ATM, P2 S6 cuối dãy quầy, P3 S6 chân cầu thang |
| 3 | 12 | 9 | 3 | T xa: P1 W4, P2 W3, P3 phòng server sâu |
| 4 | 6 | 15 | 6 | S: P2 W3 ban công ~32 m, P3 W2 ~35 m |
| 5 | 7 | 20 | 9 | S: P2 W1 ~33 m, P3 W2 ~35 m, P4 W1 kệ cao ~30 m |
| 6 | 4 | 15 | 7 | S: P2 W3 ống lớn ~33 m; C chạy vào cuối ống P1 W4 ~30 m |
| 7 | 10 | 11 | 3 | S: P3 W1 toa tàu ~30 m; grenadier P2 W3 ~28 m |
| 8 | 8 | 16 | 6 | S: P2 W3 container ~34 m, P4 vòng vây ~30 m |
| 9 | 9 | 12 | 3 | S: P3 W1 sau giàn điều hòa ~30 m |
| 10 | 12 | 20 | 8 | S: anten P2 ~35 m, trực thăng P4 ~30 m, xe chở tiền P3 ~32 m |
Level 4–6 vẫn xa hơn Level 1–3 (tỉ lệ vừa + xa 78–87% so với 40–60%).

## Câu hỏi mở
- Người dùng chỉnh tên/thứ tự/luật mới của 10 bối cảnh nếu muốn.
- Phân bổ gag H01–H03 và gag mới (nếu có); level 4 (kho tiền) đang trống gag.
- Tông cảnh và chi tiết từng phase (loại/số enemy, con tin, thùng nổ, thùng vũ khí, way nhỏ).
- **(vòng 11, còn mở)** Con tin **chui lên** có cộng điểm cứu không? Đề xuất: không (nhất quán với con tin chạy ngang; chưa chốt).
- ~~(vòng 10) Level 5 giới thiệu 3 loại enemy mới cùng lúc~~ **đã chốt vòng 11: chấp nhận được** (không dời ĐX).

### Rủi ro và việc cần PM giao (vòng 9–10)
- **Hiệu năng mobile:** 6–8 enemy + súng máy (tracer, vỏ đạn) + nổ cùng lúc; cần đo FPS trên máy tầm thấp ở Thác enemy và Vòng vây (mục tiêu ≥ 30 fps, lý tưởng 60).
- **Độ dài:** chuỗi 10 level từ khoảng 25 phút lên khoảng 31 phút; level Easy 165–175 s. Theo dõi tỉ lệ bỏ giữa level.
- **Validator** (`Assets/_Game/Settings/LevelValidationRules.asset`): nâng `maxEnemiesTotal` mỗi level lên đúng số mới (**vòng 11: ≤ 50**; L2 33, L5 36, L10 40), thêm luật riêng cho Level_04, `maxEnemiesPerLevel` chuỗi 50; **(vòng 11) thêm `maxTargetDistance` 35 m** (hiện chỉ có `minTargetDistance` 12 m), nới ngưỡng khoảng cách theo level (xem vòng 11); `maxEnemiesPerWave` 4 và `maxConcurrentCap` 3 cần ngoại lệ cho wave dồn dập (5–8/wave, 4–7 cùng lúc); thêm ngưỡng khoảng cách theo level (L4–6: 14–28 m) và kiểm khoảng cách tối đa; kiểm đường chạy con tin cách thùng nổ ≥ 3.5 m. Luật chung `Docs/Team/LevelBuildingRules.md` mục 2 ("Hostage đứng im tại chỗ cho tới hết wave") cần thêm ngoại lệ cho con tin chạy ngang/chui lên. **(vòng 11) Cần SỬA hẳn mục 2 (không chỉ thêm ngoại lệ): con tin không bao giờ đứng im** (chạy ngang hoặc nấp sau ~3 s; che enemy thì tự nấp/chạy đi sau ~3 s; trói ghế chỉ là cảnh cố định không che enemy). File đó chưa sửa, giao PM.
- **Code runtime mới:** con tin chạy ngang và chui lên; wave "thác" sinh liên tiếp theo nhịp với trần trên màn hình tách khỏi trần vòng đếm cùng lúc; cảnh báo mũi tên mép màn hình; vòng target riêng theo wave cho đợt đặc biệt; enemy X, S, KC, GI, ĐX, CC (bảng vòng 10); **(vòng 11, đã chốt)** bất tử 0.5 s sau khi mất tim (đổi `GameConfig.invulnerableSeconds` 1.5 → 0.5).
- **Dựng lại:** Level 1–3 chỉ thêm wave/enemy (L1 thêm 1 wave mỗi phase; L2 thêm enemy vào wave sẵn có, không thêm shot; L3 thêm wave và mở rộng P3 thành phòng an ninh + phòng server sâu ≥ 24 m). **Level 4, 5, 6 phải dựng lại gần như toàn bộ**: level 4 và 5 đổi bối cảnh cho nhau (prefab Level_04 thành kho tiền, Level_05 thành hầm xe 5 phase), level 6 đổi P1 và nới chiều sâu ≥ 30 m mọi khu giao tranh; điểm nối 3→4, 4→5, 5→6 đổi nên đường ray nối và `Level_Chain.unity` phải ghép lại. Level 7–10 chưa dựng, làm thẳng theo bản này.
