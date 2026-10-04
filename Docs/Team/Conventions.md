# Quy ước chung — ClaudeCop2

> Mọi agent **đọc file này trước khi làm task**. Thay đổi file này phải qua PM.

## 1. Phạm vi sở hữu
Game: rail shooter kiểu Virtua Cop 2 cho mobile — xem `Docs/Design/Plan_VirtuaCop2_Mobile.md`.

> ### ⚡ Đội gọn cho DEMO (M1 + M2) — chủ dự án chốt 2026-10-04
> Coder **đang hoạt động**: **gameplay-coder** và **ui-coder**, cùng level-designer, reviewer.
> - **gameplay-coder** sở hữu tạm thời **toàn bộ** thư mục của combat-coder, enemy-coder và jev-coder (Core, Combat, Props, FX, Enemy, Camera, Game, Jev + prefab tương ứng), ngoài phạm vi của chính mình.
> - **ui-coder** giữ nguyên phạm vi UI/Ads.
> - **combat-coder, enemy-coder, jev-coder tạm nghỉ** — sẽ nhận lại thư mục của mình từ M3/M4 (Props, Grenade/HumanShield, Jev online + Server).
> - Bảng asmdef ở mục 2 **vẫn giữ nguyên** (code vẫn tách module, chỉ là cùng một người viết). Quy tắc "không ai tham chiếu UI" vẫn áp dụng.
> - Reviewer review **một lần mỗi wave** (gộp mọi task của wave), không review từng task.

| Agent | Model | Sở hữu (chỉ được tạo/sửa trong đây) |
|---|---|---|
| project-manager | opus | `Docs/Team/` |
| game-designer | opus | `Docs/Design/` |
| level-designer | opus | `Assets/_Game/Level/`, `Assets/_Game/Scenes/Levels/` |
| gameplay-coder | sonnet | `Assets/_Game/Scripts/Camera/`, `Assets/_Game/Scripts/Game/`, `Assets/_Game/Prefabs/Game/`, `Assets/_Game/Scenes/Gameplay/`, `Assets/_Game/Scenes/Title.unity`, `Assets/_Game/Settings/` + tài nguyên dùng chung (mục 4) + Player/Build Settings |
| combat-coder | sonnet | `Assets/_Game/Scripts/Core/`, `Assets/_Game/Scripts/Combat/`, `Assets/_Game/Scripts/Props/`, `Assets/_Game/Scripts/FX/`, `Assets/_Game/Prefabs/Combat/`, `Assets/_Game/Prefabs/Props/`, `Assets/_Game/Prefabs/FX/` |
| enemy-coder | sonnet | `Assets/_Game/Scripts/Enemy/`, `Assets/_Game/Prefabs/Enemies/` |
| ui-coder | sonnet | `Assets/_Game/Scripts/UI/`, `Assets/_Game/Scripts/Ads/`, `Assets/_Game/UI/`, `Assets/_Game/Prefabs/UI/` |
| jev-coder | sonnet | `Assets/_Game/Scripts/Jev/`, `Assets/_Game/Scripts/Editor/Jev/`, `Server/` (gốc repo) |
| reviewer | sonnet | `Docs/Team/Reviews/` |

Mỗi coder còn có scene thử riêng: `Assets/_Game/Scenes/Sandbox/<agent-name>.unity` (chỉ chủ của nó được sửa).

## 2. Assembly Definition (asmdef)
Mỗi module một asmdef, do chủ thư mục tạo. Chỉ tham chiếu theo bảng:

> Trong DEMO: chủ thực tế của Core, Combat, Enemy, Jev là **gameplay-coder** (xem mục 1).

| Assembly | Thư mục | Được tham chiếu | Chủ |
|---|---|---|---|
| `ClaudeCop.Core` | `Scripts/Core/` | Unity.InputSystem (nếu cần) | combat-coder |
| `ClaudeCop.Combat` | `Scripts/Combat/`, `Scripts/Props/`, `Scripts/FX/` (1 asmdef ở `Combat/`, hoặc asmdef riêng `ClaudeCop.Props`/`ClaudeCop.FX` cùng quy tắc) | Core, Unity.InputSystem | combat-coder |
| `ClaudeCop.Enemy` | `Scripts/Enemy/` | Core | enemy-coder |
| `ClaudeCop.Camera` | `Scripts/Camera/` | Core, Unity.Cinemachine, Unity.Splines | gameplay-coder |
| `ClaudeCop.Jev` | `Scripts/Jev/` | Core, Enemy | jev-coder |
| `ClaudeCop.Game` | `Scripts/Game/` | Core, Combat, Enemy, Camera, Jev | gameplay-coder |
| `ClaudeCop.UI` | `Scripts/UI/`, `Scripts/Ads/` | Core, Combat, Enemy, Camera, Game, Jev | ui-coder |
| `ClaudeCop.Jev.Editor` | `Scripts/Editor/Jev/` (Editor only) | Jev | jev-coder |

- **Không ai được tham chiếu UI.** Gameplay báo cho UI bằng event C# (vd. `PlayerHealth.OnOutOfLives` → UI mở RevivePopup).
- Module "thấp" cần gọi module "cao" (vd. Enemy bắn trúng người chơi, Enemy cần biết camera đang blend để dừng vòng target) → dùng **interface/event/service trong Core** (vd. `IPlayerDamageReceiver`, `CombatPauseSignal`). Thêm vào Core: gửi yêu cầu qua PM cho combat-coder.
- Hợp đồng dùng chung (interface, struct, event) đặt trong **Core**.
- Không tạo tham chiếu vòng. Cần tham chiếu ngoài bảng → hỏi PM.
- Namespace trùng tên assembly (vd. `ClaudeCop.Enemy`).

## 3. Làm việc chung trong một Unity Editor
- Nhiều agent có thể chạy song song trên cùng Editor. Sau `refresh_unity` → `read_console`:
  - Lỗi **trong thư mục của mình** → phải sửa đến 0 lỗi.
  - Lỗi **ngoài thư mục của mình** → **không sửa**, ghi vào báo cáo (file, lỗi) và coi task của mình là xong nếu phần mình không lỗi.
- Không mở/sửa scene của agent khác. Thử nghiệm trong Sandbox scene của mình.
- Số liệu cân bằng: coder **tự thiết kế ScriptableObject** cho module mình, giá trị mặc định lấy từ `Docs/Design/` (PM ghi rõ con số trong task). Không hard-code số cân bằng.

## 4. Tài nguyên dùng chung (chủ: gameplay-coder)
Tags, Layers, Physics layer matrix, Player/Build Settings (`ProjectSettings/`), package (`Packages/manifest.json`), file `Assets/InputSystem_Actions.inputactions`.
Agent khác cần tag/layer/input mới → ghi yêu cầu trong báo cáo. Danh sách hiện hành:

| Loại | Tên | Dùng cho |
|---|---|---|
| Tag | _(chưa có)_ | |
| Layer | _(chưa có)_ | |

## 5. Scene
- **level-designer**: dựng môi trường. Bàn giao mỗi map dưới dạng **prefab** `Assets/_Game/Level/Level_XX.prefab` (có collider, chỗ nấp, các điểm rỗng tên chuẩn: `CamPoint_P1_S1` (gợi ý vị trí camera), `EnemySpawn_P1_W1_01`, `HostageSpawn_P1_W1_01`, `PickupSpawn_P1_W1_01`…). Mỗi điểm spawn có con rỗng `Peek` (vị trí ló ra, forward hướng về camera); `RailHint_P<p>_S<s>_NN` đánh dấu dọc đoạn di chuyển; chỉ số `W` của spawn trùng chỉ số `S` của góc camera giao tranh. Chi tiết trong TASK_BOARD mục "Quy ước level". Không cần NavMesh. Scene blockout để làm việc: `Scenes/Levels/Level_XX_Blockout.unity`.
- **gameplay-coder**: ghép scene chơi được `Scenes/Gameplay/Level_XX.unity` = Level prefab + ray/spline + các CinemachineCamera + PhaseDirector + EncounterWave/enemy + UI + manager. Chỉ đặt/nối prefab, không sửa nội dung prefab của agent khác.

## 6. Git
- Chỉ commit khi Liaison ra lệnh — sau khi task **APPROVED** và chủ dự án đồng ý.
- Mỗi agent **chỉ `git add` file trong phạm vi sở hữu của mình** (kèm file `.meta`), liệt kê đường dẫn cụ thể. **Cấm** `git add .`, `git add -A`, `git commit -a`.
- Commit message: `[<TaskID>] <agent>: <mô tả ngắn>`.
- Commit/push **tuần tự** từng agent (không song song, tránh khóa `index.lock`). Push lên `origin/main` theo lệnh Liaison; không force push.
- Agent không có Bash (PM, game-designer): Liaison commit hộ, đúng file của agent đó.
