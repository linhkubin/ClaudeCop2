# Quy ước chung — ClaudeCop2

> Mọi agent **đọc file này trước khi làm task**. Thay đổi file này phải qua PM.

## 1. Phạm vi sở hữu
| Agent | Model | Sở hữu (chỉ được tạo/sửa trong đây) |
|---|---|---|
| project-manager | opus | `Docs/Team/` |
| game-designer | opus | `Docs/Design/` |
| level-designer | opus | `Assets/_Game/Level/`, `Assets/_Game/Scenes/Levels/` |
| gameplay-coder | sonnet | `Assets/_Game/Scripts/Player/`, `Assets/_Game/Scripts/Game/`, `Assets/_Game/Prefabs/Player/`, `Assets/_Game/Scenes/Gameplay/`, `Assets/_Game/Settings/` + tài nguyên dùng chung (mục 4) |
| combat-coder | sonnet | `Assets/_Game/Scripts/Combat/`, `Assets/_Game/Scripts/Core/` |
| enemy-coder | sonnet | `Assets/_Game/Scripts/Enemy/`, `Assets/_Game/Prefabs/Enemies/` |
| ui-coder | sonnet | `Assets/_Game/Scripts/UI/`, `Assets/_Game/UI/` |
| reviewer | sonnet | `Docs/Team/Reviews/` |

Mỗi coder còn có scene thử riêng: `Assets/_Game/Scenes/Sandbox/<agent-name>.unity` (chỉ chủ của nó được sửa).

## 2. Assembly Definition (asmdef)
Mỗi module một asmdef, do chủ thư mục tạo. Chỉ tham chiếu theo chiều mũi tên:

| Assembly | Thư mục | Được tham chiếu |
|---|---|---|
| `ClaudeCop.Core` | `Scripts/Core/` | (không gì) |
| `ClaudeCop.Combat` | `Scripts/Combat/` | Core |
| `ClaudeCop.Enemy` | `Scripts/Enemy/` | Core |
| `ClaudeCop.Player` | `Scripts/Player/` | Core |
| `ClaudeCop.Game` | `Scripts/Game/` | Core, Combat, Enemy, Player |
| `ClaudeCop.UI` | `Scripts/UI/` | Core, Game |

- Hợp đồng dùng chung (interface, struct, event) đặt trong **Core** (chủ: combat-coder). Agent khác cần thêm vào Core → ghi yêu cầu trong báo cáo, PM giao cho combat-coder.
- Không tạo tham chiếu vòng. Cần tham chiếu ngoài bảng → hỏi PM.
- Namespace trùng tên assembly (vd. `ClaudeCop.Enemy`).

## 3. Làm việc chung trong một Unity Editor
- Nhiều agent có thể chạy song song trên cùng Editor. Sau `refresh_unity` → `read_console`:
  - Lỗi **trong thư mục của mình** → phải sửa đến 0 lỗi.
  - Lỗi **ngoài thư mục của mình** → **không sửa**, ghi vào báo cáo (file, lỗi) và coi task của mình là xong nếu phần mình không lỗi.
- Không mở/sửa scene của agent khác. Thử nghiệm trong Sandbox scene của mình.
- Số liệu cân bằng: coder **tự thiết kế ScriptableObject** cho module mình, giá trị mặc định lấy từ `Docs/Design/` (PM ghi rõ con số trong task). Không hard-code số cân bằng.

## 4. Tài nguyên dùng chung (chủ: gameplay-coder)
Tags, Layers, Physics layer matrix (`ProjectSettings/`), file `Assets/InputSystem_Actions.inputactions`.
Agent khác cần tag/layer/input mới → ghi yêu cầu trong báo cáo. Danh sách hiện hành:

| Loại | Tên | Dùng cho |
|---|---|---|
| Tag | _(chưa có)_ | |
| Layer | _(chưa có)_ | |

## 5. Scene
- **level-designer**: dựng môi trường. Bàn giao mỗi map dưới dạng **prefab** `Assets/_Game/Level/Level_XX.prefab` (có collider, `NavMeshSurface` đã bake, các điểm spawn rỗng tên chuẩn `PlayerSpawn`, `EnemySpawn_A01`…). Scene blockout để làm việc: `Scenes/Levels/Level_XX_Blockout.unity`.
- **gameplay-coder**: ghép scene chơi được `Scenes/Gameplay/Level_XX.unity` = Level prefab + Player + enemy/spawner + UI + manager. Chỉ đặt/nối prefab, không sửa nội dung prefab của agent khác.

## 6. Git
- Chỉ commit khi Liaison ra lệnh — sau khi task **APPROVED** và chủ dự án đồng ý.
- Mỗi agent **chỉ `git add` file trong phạm vi sở hữu của mình** (kèm file `.meta`), liệt kê đường dẫn cụ thể. **Cấm** `git add .`, `git add -A`, `git commit -a`.
- Commit message: `[<TaskID>] <agent>: <mô tả ngắn>`.
- Commit/push **tuần tự** từng agent (không song song, tránh khóa `index.lock`). Push lên `origin/main` theo lệnh Liaison; không force push.
- Agent không có Bash (PM, game-designer): Liaison commit hộ, đúng file của agent đó.
