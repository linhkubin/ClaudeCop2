# Jev (TypeSafe) — công cụ tiết kiệm token cho đội agent

`jev.py` dùng Jev (TypeSafe System One) cho các quyết định nhỏ để Claude không phải tự đọc/suy luận.
Chỉ dùng thư viện chuẩn Python 3.12+, key lấy từ biến môi trường `TYPESAFE_API_KEY`. Mỗi lệnh in **một dòng JSON**.

| Lệnh | Dùng khi | Kết quả |
|---|---|---|
| `python Tools/Jev/jev.py route "<task>" [--agent X]` | Trước khi giao task | `model` (haiku/sonnet/opus) + `owner` gợi ý |
| `python Tools/Jev/jev.py context "<task>"` | Trước khi giao task code | File `Tools/Jev/out/ctx-*.md`: 8 file code liên quan + trích mục Conventions cần thiết |
| `python Tools/Jev/jev.py review "<task>" [--paths ...]` | Sau khi agent làm xong | `mode` light/full + `reviewer_model` |
| `python Tools/Jev/jev.py bug "<mô tả lỗi>"` | Chủ dự án báo lỗi | `owner`, `area`, `severity` 0–4, `blocks`, `clear` (thấp → hỏi thêm) |
| `python Tools/Jev/jev.py check` | Kiểm tra cài đặt | Không gọi API |

## Luật quyết định (code, chỉnh trong `jev_config.json`)
- **route**: dùng model Jev chọn nếu confidence ≥ 0.6, ngược lại dùng mặc định của agent; task `risky` ≥ 0.6 không chạy haiku;
  PM / game-designer / level-designer tối thiểu sonnet.
- **context**: lấy tối đa 8 file có điểm ≥ 0.9 (thang 0–2), tối đa 4 mục Conventions + luôn kèm mục "1. Phạm vi sở hữu".
- **review**: `full` nếu đụng `.unity/.prefab/.asmdef/ProjectSettings/manifest/Scripts/Core`, hoặc risk ≥ 1.5, hoặc
  đổi hợp đồng module ≥ 0.5; còn lại `light` (reviewer chạy haiku, checklist mục 1–4).

Danh sách agent và thư mục được đọc lại từ repo mỗi lần chạy, nên thêm/bớt agent không cần sửa script.

## Chi phí
Mỗi `route`/`bug` ≈ 1.200 token Jev, `context` ≈ 30.000 token (≈ 0,001 USD), `review` 1.000–10.000.
Log từng lần gọi: `Tools/Jev/logs/decisions.jsonl` (không commit).
