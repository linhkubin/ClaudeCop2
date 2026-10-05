# Phân tích camera Virtua Cop 2 → áp dụng cho ClaudeCop2

> Nguồn: tìm kiếm web chỉ xác nhận cấu trúc cơ bản (camera chạy theo đường ray định sẵn, người chơi không điều khiển camera). Các đặc điểm bên dưới là tổng hợp từ kiến thức về dòng game arcade rail-shooter của Sega và `Plan_VirtuaCop2_Mobile.md`; cần người dùng đối chiếu bằng video gameplay gốc.

## Đặc điểm camera của Virtua Cop 2
1. **Luôn có cảm giác tiến lên.** Camera hiếm khi đứng yên cứng: đến điểm giao tranh thì giảm tốc dần, và khi dọn xong thì tăng tốc trở lại ngay. Tốc độ có nhịp (nhanh – chậm – dừng ngắn), không đều đều.
2. **Nhìn trước mối đe dọa.** Trên đường tới điểm giao tranh, hướng nhìn đã xoay dần về phía nơi enemy sẽ xuất hiện (cửa, góc hẻm), nên khi tới nơi khung hình đã sẵn sàng. Không có cú xoay muộn rồi giật lại.
3. **Phản ứng khi enemy xuất hiện.** Enemy ló ra ở mép khung thì camera lia nhanh một góc nhỏ về phía đó rồi ổn định lại ("giật mình quay sang"). Lia ngắn, có ease, không kéo theo cả cảnh.
4. **Góc phụ trong một điểm giao tranh.** Một đợt có thể chuyển 2–3 hướng nhìn (trái – giữa – phải) khi enemy xuất hiện ở nhiều phía; mỗi lần chuyển mượt, không Cut.
5. **Vào cua nghiêng/lia mượt.** Đổi hướng của ray được làm tròn (look-ahead), có nghiêng rất nhẹ; không xoay tại chỗ gắt.
6. **Súng không rung camera mạnh.** Bắn không làm khung hình nhảy; phản hồi bắn nằm ở súng/hiệu ứng. Bị trúng đạn thì màn đỏ + rung.
7. **Kết thúc đợt.** Zoom nhẹ vào enemy cuối rồi mới đi tiếp.

## So với ClaudeCop2 hiện tại (sau CAM-SMOOTH / CAM-LIVELY)
| Đặc điểm VC2 | Hiện trạng | Việc cần làm |
|---|---|---|
| 1. Nhịp tốc độ, giảm tốc khi vào giao tranh | Rail chạy tốc độ gần đều, dừng khi tới shot | Đường cong tốc độ: tăng tốc ra đầu rail, giảm tốc mềm ở ~25% cuối, không dừng đột ngột |
| 2. Nhìn trước vùng giao tranh | Look-ahead theo rail, chưa hướng về cụm enemy kế | Ở ~30% cuối của Move, yaw/pitch nghiêng dần về trọng tâm encounter kế tiếp, khớp đúng hướng shot khi tới |
| 3. Giật mình quay sang | Có (`CameraReaction`) | Giữ; chỉnh nhịp nếu cần |
| 4. Góc phụ | Có trong thiết kế (`CameraShot` góc phụ) | Kiểm tra dùng được; không bắt buộc đợt này |
| 5. Vào cua | Look-ahead + ray mềm | Giữ |
| 6. Bắn | Không rung (theo plan cũ) | **Người dùng yêu cầu giật nhẹ camera khi bắn** → thêm kick rất nhỏ, hồi nhanh, tắt khi Giảm chuyển động |
| 7. Kết thúc đợt | Kill-zoom có | Giữ |

## Quyết định
- Thêm **camera kick khi bắn**: ngẩng nhẹ 0.3–0.6°, FOV punch ≤0.5°, hồi trong ~0.1 s, MG cộng dồn có trần; số liệu trong `CameraFeelProfile`.
- Thêm **đường cong tốc độ rail** và **nhìn trước encounter kế**.
- Súng viewmodel **hướng theo hướng bắn và hướng di chuyển**; vệt đạn đi đúng từ nòng tới điểm trúng.
