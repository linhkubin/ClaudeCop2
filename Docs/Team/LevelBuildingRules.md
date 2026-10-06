# Quy tắc xây map / level mới (rút ra từ Level 1–2, chuỗi level)

Áp dụng cho mọi level mới. Công cụ kiểm tra: menu `ClaudeCop/Validate Level` (báo cáo `Docs/Team/Reviews/validate-<scene>.txt`). Khi dựng/sửa level chạy theo thứ tự: `Build LevelXX Prefab` → `Assemble LevelXX` → `Validate Level` → (nếu có chuỗi) `Assemble Level Chain` → chạy test. Không sửa file `.unity` bằng sed/Python (bị chặn); dùng Unity API (execute_code / menu).

## 1. Enemy xuất hiện (EnemyActor)
- Không enemy nào "mọc từ dưới đất" trừ khi bị vật nấp che **>= 1/2 chiều cao người** (kiểm tra 5 tia từ camera tới thân tại điểm Peek; kính bắn vỡ, enemy, con tin không tính là vật che). Đó là enemy **chui lên** sau vật nấp (hide/peek theo marker, chìm xuống rồi ló lại).
- Mọi enemy khác **chạy vào từ ngoài màn hình** (lệch ngang theo phía camera, ~4.5 m/s, 0.5–2.2 s) khi wave bắt đầu (player vừa dừng). Chạy xong thì **đứng im tại chỗ**; sau mỗi phát bắn chờ **3–5 s** rồi ngắm/bắn tiếp (không rút lui). Chờ thì ẩn vòng target nhưng vẫn bắn hạ được.
- Enemy đặt sẵn (SceneStanding, marker `EnemyStand_*`) là enemy chạy; ẩn hoàn toàn (renderer tắt) cho tới khi kích hoạt. Không để enemy hiện ở vị trí spawn rồi mới nhảy ra ngoài màn hình.
- **Kiểu xuất hiện riêng** (`SpawnPointEntry` trên điểm `EnemySpawn_*`, L3+): `Door` (đi ra từ `Door_Enemy_*` gần nhất ≤ 15 m), `Drop` (nhảy từ trên xuống; `dropHeight` > 0 = độ cao gác/ban công tính từ Peek), `Slide` (lướt từ mép màn hình), `Vault` (nhảy qua vật nấp từ phía sau). Các kiểu này **không cần vật che ≥ 1/2 người** và không chạy vào kiểu Auto. Validator không có mục kiểm vật che nên không cần nới; vẫn áp dụng (a)–(j). Đa dạng kiểu xuất hiện là mục tiêu: enemy ở cửa phòng dùng `Door`, tủ/bàn thấp dùng `Vault`, ban công/gác dùng `Drop`.
- Cảnh hài không phải mục tiêu (vd. H01 tên vấp lan can): `GagFall` (`Gag_P*_W*_NN` trong prefab level, assembler nối vào `EncounterWave.gags`); không collider, không tính kill, không bắn.
- Enemy đứng sẵn kích hoạt ngay khi `Begin` (không xếp hàng so le / maxConcurrent). Wave "dồn dập" cuối stage: preset hideTime 0, stagger 0–0.25 s, maxConcurrent 2 (vd. `EnemyPreset_L1_Rush`).
- Bắn trúng **thân** là hạ (không cần tâm); điểm đạn/vệt đạn = điểm tap trên thân. Bắn qua kính: kính vỡ trước, rồi hạ mục tiêu.

## 2. Vị trí enemy / vật nấp / tường
- Điểm spawn/Peek không được chui vào vật thể (kiểm bằng capsule 0.35 m): enemy sau quầy lùi ra sau mép quầy; vật nấp đặt cách enemy = nửa bề dày vật nấp + 0.5 m.
- Tia nhìn camera → thân enemy không bị chặn (trừ kính) với camera lệch ±0.6 m; validator mục (c).
- Enemy chạy ngang qua tường thì **thêm cửa** tại điểm cắt tường (`Door_Enemy_<P>_<W>_<NN>`: khung + ô đen + lanh tô, nằm trong prefab level). Tìm điểm cắt bằng raycast từ Peek theo hướng ngang của camera tới điểm ẩn (cách = khoảng tới mép màn hình + 1.2 m).
- Không đặt vật nấp/chậu cây nằm trên đường ray camera: cách đường ray **>= 1.3 m ngang**; khe giữa hai vật cản mà ray đi qua phải **>= 2.0 m**. Không dời enemy để né nếu làm vỡ khung hình dọc (|h| <= 5.5°) — dời vật nấp thay vì enemy.
- Hostage đứng im tại chỗ cho tới hết wave (không tự rút/biến mất giữa chừng), hết wave thì đóng băng (không bắn được nữa, không tắt đối tượng). Kính quầy dùng `PropSlot_Glass_P<p>_W<w>_<NN>` (BreakableGlass); thùng vũ khí chìm xuống rồi mới tắt.

## 3. Camera và đường ray
- Mọi đoạn Move: camera nhìn **theo hướng di chuyển** (tiếp tuyến ray), không đi ngang, không giật. Không ray gấp khúc: tốc độ xoay tối đa profile 26°/s ⇒ khúc cua phải ≤ ~5°/m ở 7.6 m/s.
- Ray **nối Phase** (Move đầu Phase 2+): bắt đầu tại góc cuối Combat trước, đi thẳng theo hướng đó, cong mềm (Hermite, hệ số 1.0) tới điểm cách shot kế 6 m, 6 m cuối thẳng hàng với yaw VÀ pitch của shot kế (validator mục d: lệch hướng <= 0.5°). Tốc độ ~7.6 m/s (<= 6 s). Shot dùng `lookDampingOverride 0.3`, `maxYawRateOverride 70`, `lookAheadOverride 10`, `lookKeys` rỗng.
- Ray không xuyên tường/quầy/cột: mọi điểm mẫu cách mọi renderer **>= 0.5 m** (ngoại trừ sàn/trần). Cổng/khe mà ray đi qua rộng >= 2.0 m.
- Kiểm tra bằng số, không bằng ảnh: mẫu spline 100–120 điểm, đo khoảng cách tới renderer; in yaw tiếp tuyến theo tiến độ.

## 4. Chuỗi level trong một scene
- Level kế được đặt ngay sau cửa cuối của level trước (xoay/anchor khớp cửa, sàn cùng cao độ), nội thất level trước chồng lên bị gỡ; ray đầu level kế bắt đầu đúng góc camera cuối level trước. Không load scene giữa level (không giật).
- Cờ `RailPhase.showResultsAfter` ở Phase cuối mỗi level (trừ level cuối): camera dừng, hiện bảng kết quả (dùng lại `WinPanel`: điểm + rank + thống kê; nút CONTINUE / HOME), `GameCommands.RequestContinue` chạy tiếp. Mỗi level **một rank riêng** (tracker reset khi Continue). Home chỉ phát lệnh (logic thêm sau).
- Level dựng trong **cùng hệ tọa độ local** với level trước (vd. Level_03 dùng gốc/hướng của Level_02) thì chuỗi chỉ cần đặt cùng transform; ray P1_S1 đi từ đúng `CamPoint_P3_S6` của level trước (`LevelSpec.linkFirst`: nhìn theo tiếp tuyến, đầu ray thẳng theo hướng góc cuối). Phần chung chỉ để chơi riêng đặt trong group `Approach_Standalone` (chuỗi gỡ). Validator chuỗi bỏ tiền tố `L2_`/`L3_` khi so tên Prop/kính và xét trùng tên theo từng root level.
- Chọn level ở Title: `GameCommands.RequestSelectLevel(i)` → `SelectedLevel`; PhaseDirector bắt đầu từ Phase đầu của level đó; bắt đầu ở level sau thì cửa level trước mở sẵn (`ChainStartSetup`).
- Cửa ngân hàng/cửa lớn: dùng đúng một kiểu — tấm kính phẳng thì **trượt** (`DoorOpener.slide`), không xoay (đẩy).

## 5. Quy trình & lưu ý
- Giữ nguyên đường ray/enemy của level đã chốt trừ khi người dùng yêu cầu sửa; sửa tối thiểu (vd. chỉ dời vật thể gây lỗi).
- Cố định yaw shot theo validator: yaw của shot Combat = trung bình các hướng tới cụm enemy; ray Move kết thúc đúng yaw đó.
- Sau mỗi thay đổi level: Validate PASS (0 FAIL), test EditMode toàn bộ pass. Play mode chỉ chạy được khi cửa sổ Unity ở foreground; nếu không thì báo rõ là chưa kiểm chứng bằng chơi thật.
- Không commit/push nếu chưa được yêu cầu; không đưa `.claude/settings.json` và `Assets/Screenshots/` vào commit.
