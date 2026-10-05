# ClaudeCop2 — quy tắc làm việc cho mọi agent

Rail shooter mobile (portrait) kiểu Virtua Cop 2, Unity 6 + URP + Cinemachine 3. Trả lời người dùng bằng tiếng Việt. Quy ước module/asmdef/sở hữu: `Docs/Team/Conventions.md`. Thiết kế 10 level: `Docs/Design/Levels_BankHeist.md`.

## Tiết kiệm token (bắt buộc)
1. **Đọc ít, đúng chỗ.** Không đọc nguyên file `.unity`, `.prefab`, `.asset` (vài trăm KB). Dùng Unity MCP (`find_gameobjects`, `manage_scene` get_hierarchy có giới hạn độ sâu/số dòng) hoặc Grep có số dòng. Đọc file code theo đoạn (offset/limit).
2. **Gom lệnh.** Nhiều thao tác Unity cùng loại thì dùng `batch_execute` (tối đa 25 lệnh một lần) thay vì gọi lẻ.
3. **Lọc đầu ra.** `read_console` chỉ lấy lỗi/cảnh báo, giới hạn số dòng, bỏ stack trace. Chạy test theo assembly hoặc tên (`ClaudeCop.<Module>.Tests`), chỉ lấy test lỗi; chạy toàn bộ bộ test một lần ở cuối task.
4. **Đo bằng số, không bằng ảnh.** Dùng bot/`DebugCamTrace`/`DebugPoseRecorder` ghi CSV rồi tóm tắt bằng script. Chỉ chụp ảnh khi thật sự cần nhìn hình, tối đa vài ảnh mỗi task.
5. **Có công cụ kiểm tra thì dùng.** Dựng/sửa level: chạy menu `ClaudeCop/Validate Level` (hoặc `LevelValidator.Run()`; không cần Play mode, ngưỡng ở `Assets/_Game/Settings/LevelValidationRules.asset`, kết quả đầy đủ ở `Docs/Team/Reviews/validate-<scene>.txt`) thay vì tự đo từng thứ.
6. **Task nhỏ, báo cáo ngắn.** Ghi báo cáo đầy đủ vào file `Docs/Team/Reviews/` hoặc `Docs/Team/Reports/`; tin nhắn trả về tối đa ~10 dòng, không dán lại nội dung báo cáo.
7. **Dùng Jev để rẻ hơn.** Người điều phối chạy `python Tools/Jev/jev.py route|context|review|bug` trước khi giao việc; agent đọc file `Tools/Jev/out/ctx-*.md` nếu prompt có, không đọc cả `Conventions.md`.
8. **Chỉ một agent dùng Play mode một lúc**; thoát Play mode khi xong. Không làm lại phần đã có: kiểm tra hiện trạng trước.
9. **Không commit/push** nếu người dùng chưa yêu cầu. Không đưa `.claude/settings.json` và `Assets/Screenshots/` vào commit.
