# TEST-W6 — Checklist test tay Wave 6 (M3 nền)

Đánh dấu cột **KQ**: ✅ Đạt / ❌ Không đạt, ghi chú vào cột cuối. Lỗi ghi ở phần "Phản hồi lỗi" cuối file.
Trạng thái lúc soạn: mới T-600 có code; các mục T-601…T-621 là checklist **chờ task xong** (tên prefab/scene có thể lệch, ghi lại nếu khác).

## A. Cách chạy test tự động
1. Unity: `Window ▸ General ▸ Test Runner`.
2. Tab **EditMode** → Run All (hoặc chọn assembly: `ClaudeCop.Tests.EditMode` là bộ của tester; các bộ `ClaudeCop.<Module>.Tests` là của chủ module).
3. Lọc theo wave/task: ô tìm kiếm không lọc Category; dùng `Category` ở dropdown mũi tên (Unity 6: nút ▾ cạnh ô tìm → Category) rồi chọn `Wave6`, `T600`, `T604`, `T621`.
4. Tab **PlayMode** → Run (chưa có test PlayMode ở lượt này; sẽ thêm khi Props/Enemy xong: thùng nổ trong/ngoài 3 m, mảnh vỡ biến mất sau ~4 s).
5. Kết quả mong đợi: không test đỏ. Test có `Ignore` (vàng) = tính năng chưa có, không phải lỗi.
6. Gửi lại: số pass/fail + tên test đỏ + dòng message.

## B. T-600 Core/Combat (ngữ nghĩa Environment/Miss)
Scene: `Scenes/Sandbox/gameplay-coder-combat.unity` (Play).
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 600-1 | Bắn vào tường/sàn trơ | Có vết đạn/tia lửa; combo về 0; tính là trượt | | |
| 600-2 | Bắn vào ngoài trời (không trúng gì) | Combo về 0; tốn 1 đạn | | |
| 600-3 | Có combo ≥3, bắn vào vật IShootable (DummyTapTarget loại Environment, sau này hộp) | Tốn 1 đạn, combo **giữ nguyên** | | |
| 600-4 | Console sau mỗi lần Play | 0 lỗi | | |
| 600-5 | Mở Inspector CombatConfig/WeaponData | Có field cho giá trị trước đây hard-code (vd. 0.2 s TapShooter); cảm giác bắn không đổi | | |

## C. T-601 Props: hộp, thùng nổ, PropPool
Scene: `Scenes/Sandbox/gameplay-coder-props.unity`.
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 601-1 | Tap hộp bằng súng thường | Hộp bay theo hướng đạn, rồi nằm yên | | |
| 601-2 | Tap hộp bằng Shotgun | Bay mạnh hơn rõ rệt so với 601-1 | | |
| 601-3 | Tap thùng đỏ | Nổ 1 lần, thành mảnh vỡ | | |
| 601-4 | Enemy ĐANG LÓ trong 3 m của thùng | Chết khi thùng nổ | | |
| 601-5 | Enemy ngoài 3 m | Không sao | | |
| 601-6 | Enemy đang ẩn (không lộ) trong 3 m | Không bị hạ (mặc định Q1) | | |
| 601-7 | Con tin trong 3 m | Con tin ngã, mất ĐÚNG 1 mạng (kể cả khi 2 con tin) | | |
| 601-8 | Hai thùng cạnh nhau, bắn 1 | Thùng kia nổ dây chuyền sau ~0.15 s | | |
| 601-9 | Chờ sau vụ nổ | Mảnh vỡ biến mất sau ~4 s | | |
| 601-10 | Bắn/nổ liên tục nhiều lần | Số Rigidbody hoạt động không quá 40 (xem debug/Hierarchy) | | |
| 601-11 | Điểm/combo sau nổ (chưa nối Game) | Không tự cộng điểm trong sandbox; không lỗi console | | |

## D. T-605 Kính vỡ + FX nổ
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 605-1 | Tap tấm kính | Vỡ thành 6–10 mảnh khớp kích thước tấm; có bụi kính | | |
| 605-2 | Đổi scale tấm kính (vd. 2×1) rồi bắn | Mảnh vẫn khớp kích thước | | |
| 605-3 | Bắn lại chỗ kính đã vỡ | Không vỡ lần nữa; đạn xuyên qua (trúng vật phía sau) | | |
| 605-4 | Thùng nổ | Có cầu lửa + khói ngắn (≤ ~1 s) | | |
| 605-5 | Vỡ 3 kính + 2 thùng liên tiếp | Tổng Rigidbody ≤ 40; không khựng rõ | | |
| 605-6 | Menu `ClaudeCop ▸ Props ▸ Build Glass Shards` chạy 2 lần | Không lỗi, ghi đè được | | |

## E. T-602 Lựu đạn + Human Shield
Scene: `Scenes/Sandbox/gameplay-coder-enemy.unity` (dùng nút/phím debug tạo Grenadier / HumanShield).
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 602-1 | Grenadier ló ra, để vòng chạy hết | Ném lựu đạn (không bắn ngay); lựu đạn có vòng riêng | | |
| 602-2 | Tap lựu đạn đang bay | Nổ giữa không, KHÔNG mất mạng | | |
| 602-3 | Bỏ mặc lựu đạn | Sau ~1.5 s mất đúng 1 mạng, màn hình nháy đỏ | | |
| 602-4 | Lựu đạn đang bay, mở Revive/pause (CombatPause) | Lựu đạn đứng yên | | |
| 602-5 | Bắn Grenadier trước khi ném | Chết như enemy thường; Justice vẫn được | | |
| 602-6 | Wave còn lựu đạn bay | Wave chưa Cleared, camera chưa di chuyển | | |
| 602-7 | Human Shield: tap đầu enemy | Enemy chết, con tin thoát chạy đi | | |
| 602-8 | Human Shield: tap chấm Justice ở tay | JusticeKill, con tin thoát, không phạt | | |
| 602-9 | Human Shield: tap thân con tin (xa đầu) | Mất 1 mạng, enemy vẫn sống, vòng tiếp tục | | |
| 602-10 | Human Shield: để vòng hết | Enemy bắn bình thường | | |
| 602-11 | Màn hình dọc 1080×1920 và 1170×2532 | Bán kính chọn đầu (45 px) hợp lý, không bắn nhầm | | |

## F. T-603 RankScore mở rộng (Scene Level_01, bật bảng debug RankScore)
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 603-1 | Chơi hết màn, chiến thắng | Bảng debug có bản ghi "rank"; hiện S/A/B/C | | |
| 603-2 | Thua (Game Over) | Không có rank | | |
| 603-3 | Dùng Revive rồi thắng | Rank tối đa B | | |
| 603-4 | Chơi tệ có chủ ý (bắn tường nhiều) | Weakness = Bắn trượt… ("Bắn trượt nhiều — ngắm kỹ hơn") | | |
| 603-5 | Chơi hoàn hảo | "Hoàn hảo!" | | |
| 603-6 | Bắn hộp/thùng | Không bị tính trượt; thùng nổ hạ enemy không đổi accuracy | | |
| 603-7 | Phase 2+: xem bảng debug đầu Shot di chuyển | Có câu hỏi wave_preset, weapon_drop; ít dữ liệu → Noul/mặc định | | |
| 603-8 | RankScoreConfig.enabled = false | Không câu hỏi nào được áp | | |
| 603-9 | Bắt đầu lượt mới | Thống kê cả màn và rank reset | | |

## G. T-604 Dọn nợ Camera/Game
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 604-1 | Mở `Settings/CameraFeelProfile.asset`, `GameConfig.asset` | Có field mới (rung nổ 0.6×/0.25 s; điểm nổ 100; lựu đạn 50) hiện giá trị thật | | |
| 604-2 | File ▸ Build Profiles/Settings | Chỉ Title (0), Level_01 (1); không còn SampleScene | | |
| 604-3 | Chơi Level_01, bắn trúng thử | Độ rung camera, thời gian chuyển Phase giống trước | | |

## H. T-611 UI lựu đạn
Scene: `Scenes/Sandbox/ui-coder.unity` (nút debug giả lập grenade).
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 611-1 | Tạo grenade giả | Vòng cam nhỏ (~0.6× vòng enemy), nhấp nháy, bám đúng, không có chấm Justice | | |
| 611-2 | Banner "LỰU ĐẠN!" | Hiện khi có grenade, ẩn khi hết; chữ tiếng Việt đúng dấu; không che vùng bắn giữa, nằm dưới safe area | | |
| 611-3 | Bật Giảm chuyển động trong Cài đặt | Banner và vòng không nhấp nháy | | |
| 611-4 | Bắn Machine Gun liên tục, mở Profiler (GC Alloc của HUD) | Không có GC sinh từ HUD mỗi frame | | |
| 611-5 | Các tỉ lệ 9:16, 9:19.5, 9:21 | Không tràn, không chồng nút | | |

## I. T-621 Level_01 marker M3
Mở `Scenes/Levels/Level_01_Blockout.unity` (Scene view). Test tự động `Level01_M3Markers_*` (Category T621) kiểm tên/đếm.
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| 621-1 | Hierarchy: tìm `M3_Markers` mỗi Area | Có đủ nhóm | | |
| 621-2 | Mỗi Phase | 2 thùng, 2–4 kính, 4–6 hộp; Grenadier P1:0 P2:2 P3:3; Shield P1:0 P2:1 P3:1–2 | | |
| 621-3 | Từ mỗi CamPoint, Game view 1080×1920 | Marker trong khung, cách mép ≥ 8%; không che đường ló của enemy | | |
| 621-4 | Thùng vs con tin | Chỉ 1 thùng bẫy ở P3 có con tin trong 3 m | | |
| 621-5 | Console sau khi mở prefab | 0 lỗi, không Missing | | |

## J. Hồi quy M2 (Combat/Enemy đã đổi) — Scene Title → Level_01 (Play)
| ID | Bước | Mong đợi | KQ | Ghi chú |
|---|---|---|---|---|
| R-1 | Title → bấm Bắt đầu | Vào Level_01, HUD hiện đạn/điểm | | |
| R-2 | Chơi hết 3 Phase × 6 Shot | Chuyển Phase có màn chuyển; kết thúc Win, màn kết thúc hiện | | |
| R-3 | Win → Chơi lại | Về trạng thái đầu, điểm/combo reset | | |
| R-4 | Mất hết mạng → Revive popup, chọn nhánh 1 | Hành vi đúng như spec M2 (xem Plan) | | |
| R-5 | Revive nhánh 2 | Đúng spec | | |
| R-6 | Revive nhánh 3 (từ chối / Game Over) | Vào Game Over, không rank | | |
| R-7 | Hạ liên tiếp | Combo tăng, chữ bay hiện, bắn trượt reset combo | | |
| R-8 | Tap vào chấm Justice | JusticeKill, +điểm lớn, hiệu ứng khác | | |
| R-9 | Bắn con tin | Mất mạng, nháy đỏ, rung | | |
| R-10 | Nhặt vũ khí (Shotgun/MG) | Đổi vũ khí, đạn đúng | | |
| R-11 | Chơi ở 3 tỉ lệ màn dọc | Camera tự khung, UI không tràn | | |
| R-12 | FPS trên thiết bị | Ổn định mục tiêu; ghi FPS nếu tụt | | |

## Phản hồi lỗi
Mẫu: `[ID] mô tả · bước tái hiện · mong đợi/thực tế · ảnh/log`

- (trống)
