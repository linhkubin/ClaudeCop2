# Task Board — ClaudeCop2

> Do **project-manager** duy trì. Trạng thái: TODO · IN PROGRESS · IN REVIEW · DONE · BLOCKED

## Quy trình làm việc
> Luật chung (phạm vi sở hữu, asmdef, scene, git): xem [Conventions.md](Conventions.md)
```
Chủ dự án ⇄ Liaison (agent chính)
                │  1. ý tưởng
                ▼
        game-designer     → phân tích ý tưởng (Docs/Design/)
                │  2. chủ dự án duyệt thiết kế
                ▼
        project-manager   → chia task theo wave, viết prompt giao việc
                │  3. Liaison giao task song song
                ▼
 level-designer · gameplay-coder · combat-coder · enemy-coder · ui-coder
                │  4. kết quả
                ▼
            reviewer      → APPROVED / CHANGES REQUESTED
                │  5. Liaison mang kết quả về
                ▼
        project-manager   → cập nhật board, viết báo cáo
                │  6. Liaison tóm tắt cho chủ dự án
                ▼
   chủ dự án đồng ý → từng agent commit/push đúng file của mình (tuần tự)
```

## Bảng task
| ID | Wave | Owner | Mục tiêu | Phụ thuộc | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| — | — | — | (chưa có task) | — | — | — |
