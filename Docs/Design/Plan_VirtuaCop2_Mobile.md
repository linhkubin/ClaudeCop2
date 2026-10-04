> **Nguồn:** plan đã được chủ dự án duyệt (2026-10-04, cập nhật Props cùng ngày). Đây là tài liệu thiết kế gốc cho cả đội.
>
> ## Phạm vi DEMO hiện tại: M1 + M2 (+ Props cơ bản ở W4)
> - **M1 – Vòng lặp cốt lõi:** camera chạy ray (Cinemachine 3 + Splines) qua 1 Phase, tap bắn, enemy ló ra + vòng target thu nhỏ, đạn/reload, 3 mạng, HUD, thắng/thua, CameraFeelProfile cơ bản.
> - **M2 – Hoàn thiện Level01:** đủ 3 Phase (đường phố, kho hàng, mái nhà), fade + tiêu đề Phase, con tin, combo, Justice Shot, Shotgun/Súng máy + thùng vật phẩm, Revive + quảng cáo giả, màn Title, FX cơ bản (tia lửa, vết đạn, chữ bay, enemy văng, nháy đỏ), tùy chọn "Giảm chuyển động", thiết lập Android. **Jev chỉ dùng `OfflineJevClient`** (luật viết sẵn) + bảng debug.
> - **Props trong demo (W4):** `IShootable`, `SurfaceMaterial`, `PropPool`, hộp bay (`PhysicsProp`), kính vỡ (`BreakableGlass`), thùng nổ (`ExplosiveBarrel`) — theo mục 14 bên dưới.
> - **CHƯA làm (M3/M4):** cây, cửa, đèn, biển hiệu, đồ ẩn trong hộp, lựu đạn, human shield, Jev Proxy/Direct, server Python/Node, cửa sổ Editor chấm log, đánh giá rank.
> - **Giá trị mặc định đã duyệt** cho các chỗ plan còn thiếu: xem `Docs/Team/TASK_BOARD.md` (câu hỏi mở 1–10 của PM, chủ dự án chấp nhận toàn bộ).
>
> Thư mục/asmdef thực tế theo `Docs/Team/Conventions.md` (ghi đè mục "Cấu trúc file" bên dưới nếu khác).

# Kế hoạch: Prototype game bắn súng kiểu Virtua Cop 2 cho mobile

## Bối cảnh
Bạn muốn làm một game giống Virtua Cop 2 (game bắn súng arcade góc nhìn thứ nhất, camera tự di chuyển theo đường định sẵn). Game sẽ chạy trên mobile: thay vì dùng chuột để ngắm, mỗi enemy có một **vòng target thu nhỏ dần**, và người chơi phải **tap đúng vào vị trí đó** để hạ enemy trước khi vòng thu hết. Bạn chọn để mình **làm nhanh toàn bộ**, sau đó bạn xem lại.

Hiện trạng project: Unity 6000.3.9f1, URP (đã có `Mobile_RPAsset`), Input System 1.18. Project gần như trống, chỉ có `Assets/Scenes/SampleScene.unity`. Bản đầu tiên là prototype dùng hình khối có sẵn (cube, capsule), chưa cần model hay art.

## Cấu trúc màn chơi: Phase → Shot (nhiều góc camera)
Giống game gốc, màn chơi được chia thành nhiều **Phase** (ví dụ: Phase 1 ngoài đường, Phase 2 trong kho hàng, Phase 3 trên mái nhà). Mỗi Phase gồm nhiều **Shot** (góc camera) chạy lần lượt:
- **Shot di chuyển:** camera lướt theo đường ray (spline) từ chỗ này sang chỗ khác. Không có enemy, hoặc chỉ vài enemy xuất hiện thoáng qua.
- **Shot giao tranh:** camera dừng ở một góc, enemy xuất hiện. Trong lúc giao tranh, camera có thể **chuyển qua 2–3 góc phụ**, ví dụ quay sang trái để thấy enemy ló ra từ cửa sổ, rồi quay lại giữa.
- **Zoom nhẹ:** mỗi góc có FOV riêng (ví dụ từ 60 xuống 50) và tự đẩy vào chậm (dolly-in) trong lúc giao tranh để tạo cảm giác căng thẳng. Khi enemy cuối cùng chết, camera zoom nhẹ vào nó rồi mới chuyển góc.
- **Cách chuyển góc:** mỗi Shot chọn một trong ba kiểu: *Cut* (cắt cảnh ngay), *Blend* (lướt mượt 0.3–0.8 giây), hoặc *Spline* (chạy theo ray).
- Khi chuyển Phase thì màn hình fade đen ngắn và hiện tên Phase, kiểu "STAGE 1-2".
- Thêm rung camera nhẹ khi người chơi bị bắn (camera shake).

### Cảm giác camera: sinh động, nhịp nhàng, không gây chóng mặt
Mọi thông số nằm trong một ScriptableObject `CameraFeelProfile` để chỉnh nhanh. Các giá trị dưới đây là điểm khởi đầu:
- **Di chuyển theo ray:**
  - Tốc độ 3–4 m/s, như đi bộ nhanh.
  - Lúc bắt đầu và kết thúc luôn tăng và giảm tốc mượt (ease-in/out), không bao giờ xuất phát hay dừng đột ngột.
  - Camera nhìn trước 3–5 m dọc theo ray, có làm mượt (damping), thay vì nhìn thẳng theo hướng ray.
- **Giới hạn xoay (nguyên nhân chính gây chóng mặt):**
  - Xoay ngang tối đa khoảng 60 độ mỗi giây.
  - Nghiêng camera khi vào cua tối đa 2 độ.
  - Ngẩng hoặc cúi tối đa 10 độ.
  - Cần đổi hướng hơn 90 độ thì dùng **Cut** (cắt cảnh) thay vì lia nhanh.
- **Chuyển giữa các góc trong lúc giao tranh:** Blend kiểu EaseInOut trong 0.5–0.7 giây. Trong lúc đang blend, vòng target của enemy tạm dừng thu nhỏ, để người chơi không bị bắn khi camera đang chuyển.
- **Không bao giờ đứng im hẳn:**
  - Khi giao tranh, camera có rung nhẹ như cầm tay (Perlin noise, biên độ 0.3, tần số 0.4).
  - Khi di chuyển, camera nhún nhẹ theo bước chân, khoảng 2–3 cm.
- **Zoom:**
  - Dolly-in trong lúc giao tranh thu FOV tổng cộng không quá 8–10 độ.
  - Khi hạ enemy cuối của một đợt, có cú "zoom punch": giảm FOV 5 độ trong 0.15 giây, trả lại trong 0.4 giây.
- **Rung camera khi bị bắn:** ngắn 0.2 giây, biên độ vừa phải. Bắn súng không làm rung camera, để người chơi tap vẫn chính xác.
- **Nhịp của một Phase:**
  1. Di chuyển 3–5 giây (tối đa 6 giây).
  2. Giao tranh 8–15 giây.
  3. Chuyển góc phụ 1–2 giây.
  4. Giao tranh tiếp.
  5. Dừng khoảng 0.3 giây để lấy nhịp, rồi mới di chuyển tiếp.
- **Tùy chọn "Giảm chuyển động":** công tắc ở màn tiêu đề. Bật lên sẽ tắt rung cầm tay, nhún và nghiêng camera, và dùng Cut thay cho mọi cú lia nhanh.

**Công nghệ:** dùng **Cinemachine 3** (package `com.unity.cinemachine`, sẽ được thêm vào project). Mỗi góc là một `CinemachineCamera`, `CinemachineBrain` lo việc blend, `CinemachineSplineDolly` cùng Unity Splines lo đoạn camera chạy theo ray, `CinemachineImpulse` lo rung camera. Một script `PhaseDirector` đóng vai đạo diễn, điều khiển việc chuyển góc qua `Priority`.

Level01 (prototype): 3 Phase, mỗi Phase có 1 đoạn di chuyển và 2 lần giao tranh, mỗi lần giao tranh có 1–3 góc. Tổng cộng khoảng 6 lần giao tranh.

## Gameplay (vòng lặp cốt lõi)
1. Camera chạy theo các Shot của từng Phase (xem phần trên).
2. Ở mỗi Shot giao tranh, enemy (capsule màu đỏ) lần lượt xuất hiện từ sau vật che hoặc trồi lên. Mỗi đợt enemy gắn với một góc camera; dọn xong đợt này thì chuyển sang góc tiếp theo.
3. Mỗi enemy có một **vòng target (UI)** bám theo vị trí của nó trên màn hình. Vòng bắt đầu to, thu nhỏ dần và đổi màu xanh → vàng → đỏ trong khoảng 2–3 giây (thời gian chỉnh được).
4. **Tap trúng** (trong bán kính vòng quanh enemy): enemy chết và người chơi được điểm. Tap càng sớm thì điểm thưởng càng cao.
5. **Vòng thu hết** mà chưa tap: enemy bắn, người chơi mất 1 mạng, màn hình nháy đỏ.
6. Hạ hết enemy thì camera đi tiếp đến điểm kế. Đi hết đường thì thắng; hết mạng thì Game Over.
7. **Đạn và Reload:** súng lục có 6 viên, tap vào nút Reload ở góc màn hình để nạp lại. Tap trượt chỉ tốn đạn, không bị phạt thêm.
8. **Con tin (dân thường, capsule màu xanh dương):** không có vòng target. Tap trúng con tin thì mất 1 mạng và combo về 0.
9. **Combo:** các lần trúng liên tiếp tăng hệ số điểm (x1, x2, x3, tối đa x5). Tap trượt hoặc bị bắn thì combo về 0.
10. **Justice Shot:** mỗi enemy có một chấm nhỏ ở tay cầm súng. Tap đúng chấm đó thì tước vũ khí, enemy đầu hàng và người chơi được điểm thưởng lớn. Điểm cũng cao hơn nếu tap khi vòng còn ở giai đoạn xanh.
11. **Đổi vũ khí:** thỉnh thoảng có thùng vật phẩm xuất hiện, tap vào để nhặt. Khi hết đạn đặc biệt thì quay về súng lục.
    - **Shotgun:** 6 viên, bán kính trúng lớn hơn và có thể trúng nhiều enemy ở gần nhau.
    - **Súng máy:** 30 viên, giữ ngón tay để bắn liên tục.

12. **Mạng và Revive:** người chơi có 3 mạng, hiện bằng 3 trái tim trên HUD. Khi mất hết mạng, game tạm dừng và hiện ô **Revive** có đếm ngược 10 giây:
    - Nhấn "Revive (xem quảng cáo)": hiện một màn **quảng cáo giả** toàn màn hình, đếm 3 giây, chỉ đóng được khi đếm xong. Sau đó hồi đủ 3 tim và chơi tiếp ngay tại chỗ.
    - Nhấn "Bỏ qua" hoặc hết giờ: sang màn Game Over.
    - Số lần revive mỗi lượt chỉnh được, mặc định 1 lần.
    - Quảng cáo giả đặt sau interface `IRewardedAd` (`FakeRewardedAd`), để sau này thay bằng quảng cáo thật (ví dụ Unity LevelPlay hoặc AdMob) mà không phải sửa `RevivePopup`.
13. **Hiệu ứng hình ảnh khi bắn** (không có âm thanh, rung hay slow-motion trong bản demo):
    - Tia lửa ở chỗ tap.
    - Vết đạn trên tường.
    - Enemy ngã văng ra (bật ragdoll đơn giản hoặc thêm lực vào Rigidbody).
    - Chữ điểm bay lên, kèm chữ "JUSTICE!" và "x3 COMBO".
    - Màn hình nháy đỏ khi bị bắn.

14. **Vật thể tương tác (bắn vào môi trường).**
    - **Phạm vi:** bản demo chỉ làm hộp bay (`PhysicsProp`), kính vỡ (`BreakableGlass`) và thùng nổ (`ExplosiveBarrel`), cùng với `IShootable`, `SurfaceMaterial` và `PropPool`. Các vật thể còn lại để sang M3: cây, cửa, đèn, biển hiệu, đồ ẩn trong hộp.
    - **Cách làm việc khi sửa plan:** khi được phép sửa file, cập nhật `Docs/Design/Plan_VirtuaCop2_Mobile.md` và `Docs/Team/TASK_BOARD.md`. Sau đó chuyển cho game-designer, rồi project-manager, theo đúng [[agent-team-workflow]].
    
    - Mọi vật thể bắn được đều dùng chung interface `IShootable.OnShot(hitPoint, hitNormal, weapon)`.
    - **Thứ tự ưu tiên khi tap:** `TapShooter` xét enemy và lựu đạn trước (theo vòng target), rồi đến con tin, cuối cùng raycast vào vật thể môi trường.
    - **Hộp, thùng carton, lon, chai:** dùng Rigidbody và `AddForceAtPosition` theo hướng đạn, nên vật bay và xoay tự nhiên. Shotgun đẩy mạnh hơn. Bắn lon đang bay lên thì được điểm thưởng "tâng lon".
    - **Cây và bụi cây:** bung một đợt lá rơi (particle) tại chỗ trúng, cây rung nhẹ trong 0.5 giây (dao động bằng code, không dùng vật lý).
    - **Kính (cửa sổ, tủ kính):** khối kính được thay bằng các mảnh vỡ cắt sẵn (khoảng 6–10 mảnh có Rigidbody) cùng bụi kính. Kính đã vỡ thì không vỡ lại. Có thể có enemy nấp sau kính.
    - **Cửa:** dùng HingeJoint hoặc xoay bằng code. Bắn vào cửa thì cửa bật mở về phía bị đẩy. Sau cửa có thể có enemy, con tin hoặc thùng vũ khí; những thứ này do `EncounterWave` cài sẵn và Jev có thể chọn.
    - **Thùng nổ (màu đỏ):** bắn 1 phát là nổ, đẩy văng vật xung quanh và hạ các enemy trong bán kính 3 m. Nếu có con tin trong bán kính thì người chơi bị phạt.
    - **Bóng đèn, biển hiệu:** bóng đèn tắt kèm tia lửa; biển hiệu bị đạn đánh rơi xuống.
    - **Vết đạn và tia lửa** dùng hiệu ứng khác nhau theo chất liệu (gỗ, kim loại, kính, lá cây), khai báo bằng `SurfaceMaterial`.
    - **Luật bắn vật thể:** tốn 1 viên đạn nhưng **giữ combo**. Không tính là bắn trượt khi đo độ chính xác gửi cho Jev; được đếm riêng là "bắn môi trường".
    - **Vai trò trong gameplay** (giữ tất cả các vai trò):
      - Thùng nổ hạ được cả nhóm enemy, nhưng gây hại cho con tin ở gần.
      - Hộp có thể chứa đồ ẩn: điểm thưởng, 1 tim (không vượt quá 3), hoặc thùng vũ khí. Đồ trong hộp do `EncounterWave` cài sẵn, Jev có thể chọn qua câu hỏi `weapon_drop`.
      - Cửa và kính che enemy: enemy nấp phía sau chưa có vòng target cho tới khi bị lộ ra. Lộ ra theo một trong hai cách: người chơi bắn mở hoặc bắn vỡ, hoặc tự enemy phá ra sau vài giây.
      - Phần lớn vật thể còn lại chỉ để trang trí và cho vui.
    - **Hiệu năng trên mobile:**
      - Mỗi loại mảnh vỡ và particle có pool riêng.
      - Mảnh vỡ tự biến mất sau 4 giây.
      - Tối đa khoảng 40 Rigidbody đang hoạt động cùng lúc.
      - Vật thể ngoài khung hình thì ngủ (sleep).

**Giao diện ngoài trận:** chỉ có màn hình tiêu đề (logo chữ và dòng "TAP TO START"), sau đó vào Level01. Bản demo **chưa có boss**; boss sẽ làm sau.

## Tích hợp Jev (TypeSafe)
Jev chỉ trả về các quyết định có kiểu (Choice, Score, Noul) kèm xác suất, qua API `POST https://api.typesafe.ai/v1/systemone` với `Authorization: Bearer <key>` và `model: "jev-latest"`. Vì mỗi lần gọi phải qua mạng, Jev **chỉ quyết định giữa các đợt giao tranh**. Các phần chạy mỗi frame (tap, kiểm tra trúng, vòng target, camera) vẫn viết bằng code thường.

**Những gì Jev quyết định.** Các câu hỏi được gửi chung trong một request và Jev trả lời song song.
1. **Bộ điều phối (JevDirector):** gọi khi Shot di chuyển bắt đầu, để có kết quả sẵn trước đợt giao tranh kế tiếp.
   - `state`: tóm tắt bằng chữ các chỉ số của người chơi (độ chính xác, thời gian phản xạ trung bình, số mạng, combo cao nhất, số lần trúng con tin, đạn còn lại, vũ khí đang cầm) cộng mô tả đợt giao tranh sắp tới (bối cảnh, các điểm xuất hiện).
   - `player_skill`: Score 5 mức, từ "mới chơi" đến "rất giỏi".
   - `wave_preset`: Choice giữa calm, standard, intense, hostage_heavy. Mỗi lựa chọn ứng với một bộ cấu hình đợt làm sẵn.
   - `reticle_time`: Choice 2.0, 2.5 hoặc 3.0 giây.
   - `enemy_tactic`: Choice giữa peek (ló ra từng con), rush (xông lên cùng lúc), grenade (ném lựu đạn), human_shield (bắt con tin làm khiên).
   - `hostage_spot`: Choice trong các điểm đặt con tin của đợt đó, mô tả bằng chữ.
   - `weapon_drop`: Choice giữa none, shotgun, machinegun.
2. **Đánh giá cuối màn:** `rank` là Choice giữa S, A, B, C. `weakness` là Choice giữa phản xạ chậm, bắn trượt nhiều, hay bắn nhầm con tin, reload sai lúc. Mỗi điểm yếu ứng với một câu gợi ý soạn sẵn hiện trên màn kết quả.
3. **Công cụ khi phát triển (chỉ chạy trong Editor):** một cửa sổ đọc log các lần chơi thử. Jev chấm mỗi đợt từ "quá dễ" đến "quá khó" để bạn cân bằng màn chơi.

**Giữ an toàn khi gọi Jev:**
- **Chấp nhận theo độ tự tin:** nếu `confidence` thấp hơn ngưỡng (khởi đầu 0.6, chỉnh được) thì dùng giá trị mặc định.
- **Giới hạn thời gian:** mỗi request chờ tối đa khoảng 1.5 giây. Quá thời gian, mất mạng, hoặc bị giới hạn tần suất (lỗi 429/529) thì `OfflineJevClient` dùng luật viết sẵn. Game luôn chơi được khi không có mạng.
- **Bảng debug trên màn hình (bật/tắt được):** hiện quyết định gần nhất của Jev kèm xác suất. Bảng này rất hữu ích khi trình diễn bản demo.

**Ba cách kết nối, chọn trong `JevConfig`:**
- `ProxyJevClient`: Unity gọi server trung gian trên localhost. Có hai bản server cùng một giao diện, chọn bản nào cũng được:
  - `Server/python/`: FastAPI kèm `typesafe-sdk`.
  - `Server/node/`: Express kèm SDK JavaScript.
  
  Cả hai đọc `TYPESAFE_API_KEY` từ file `.env` (file này được gitignore) và có endpoint `POST /jev` (chuyển tiếp yêu cầu) cùng `GET /health`. Điện thoại dùng chung Wi-Fi với máy bạn cũng gọi được qua địa chỉ IP LAN.
- `DirectJevClient`: Unity gọi thẳng API của TypeSafe. API key lấy từ EditorPrefs hoặc một file bị gitignore. **Chỉ có trong Editor và bản Development Build**; bản release bị loại bỏ phần này bằng `#if`.
- `OfflineJevClient`: chỉ dùng luật viết sẵn, đồng thời là phương án dự phòng cho hai cách trên.

Câu hỏi gửi cho Jev được định nghĩa trong C#. Server chỉ chuyển tiếp nên cả ba cách dùng chung một logic.

## Cấu trúc file
Đặt trong `Assets/_Game/`:
- `Scripts/Camera/PhaseDirector.cs`: chạy danh sách Phase và Shot theo thứ tự, bật `CinemachineCamera` tương ứng, chờ di chuyển hoặc giao tranh xong rồi chuyển tiếp, làm fade và hiện tiêu đề khi đổi Phase.
- `Scripts/Camera/CameraShot.cs`: dữ liệu của một Shot gồm loại (Move/Combat), kiểu chuyển (Cut/Blend/Spline), thời gian blend, FOV, tốc độ dolly-in, và danh sách góc phụ, mỗi góc phụ có `EncounterWave` riêng.
- `Scripts/Camera/SlowZoom.cs`: giảm FOV từ từ trong lúc Shot đang hoạt động, kèm cú zoom punch.
- `Scripts/Camera/CameraFeelProfile.cs` (ScriptableObject chứa các thông số ở mục "Cảm giác camera") và `Scripts/Camera/CameraFeelApplier.cs`: áp noise, nhún, nghiêng và giới hạn xoay; đọc tùy chọn "Giảm chuyển động" lưu trong PlayerPrefs.
- `Scripts/EncounterWave.cs`: danh sách điểm xuất hiện của enemy, con tin và vật phẩm trong một đợt. Cho chúng xuất hiện so le và báo khi đã dọn sạch.
- `Scripts/Enemy.cs`: các trạng thái (Hiện ra → Ngắm → Bắn/Chết), giữ timer cho vòng thu nhỏ, gọi `PlayerHealth` khi bắn.
- `Scripts/TargetReticleUI.cs`: vòng target trên Canvas, chuyển vị trí từ world sang screen mỗi frame, chỉnh scale và màu theo tiến độ của enemy.
- `Scripts/TapShooter.cs`: đọc tap bằng Input System (`Touchscreen` và `Pointer`, nên trong Editor vẫn dùng chuột được), tìm enemy đang hoạt động có vòng chứa điểm tap (so khoảng cách trên màn hình, kèm raycast làm dự phòng), quản lý đạn và reload.
- `Scripts/Hostage.cs`: dân thường, bị tap trúng thì phạt mạng.
- `Scripts/WeaponData.cs` (ScriptableObject cho Pistol, Shotgun, MachineGun: số đạn, bán kính trúng, tự động bắn) và `Scripts/WeaponPickup.cs` (thùng vật phẩm).
- `Scripts/ComboSystem.cs`: hệ số combo, được `GameManager` dùng khi tính điểm.
- `Scripts/PlayerHealth.cs`: 3 mạng, hiệu ứng nháy đỏ khi bị bắn. Hết mạng thì gọi `RevivePopup`.
- `Scripts/UI/RevivePopup.cs`: popup đếm ngược 10 giây, giới hạn số lần revive, dừng game bằng `Time.timeScale` trong lúc chờ.
- `Scripts/Ads/IRewardedAd.cs` và `Scripts/Ads/FakeRewardedAd.cs`: màn quảng cáo giả 3 giây, kết quả trả về qua callback.
- `Scripts/UI/HeartsUI.cs`: 3 icon trái tim.
- `Scripts/UI/TitleScreen.cs` và `Scenes/Title.unity`.
- `Scripts/Props/`:
  - `IShootable.cs`
  - `SurfaceMaterial.cs`
  - `PhysicsProp.cs`: hộp, lon, chai.
  - `FoliageProp.cs`: cây, bụi cây.
  - `BreakableGlass.cs`
  - `ShootableDoor.cs`
  - `ExplosiveBarrel.cs`
  - `LampProp.cs`
  - `HangingSign.cs`
  - `PropPool.cs`
- Prefab cho từng loại vật thể nằm trong `Prefabs/Props/`. Mảnh vỡ kính được tạo sẵn bằng cách chia một quad thành các mảnh tam giác bằng code Editor.
- `Scripts/FX/HitFX.cs`: các particle cho tia lửa và vết đạn.
- `Scripts/FX/FloatingText.cs`: chữ điểm bay lên.
- `Scripts/GameManager.cs`: điểm số, trạng thái thắng/thua, khởi động lại.
- `Scripts/HUD.cs`: hiển thị điểm, mạng, đạn, nút Reload, màn Win/Game Over kèm nút Chơi lại.
- `Scripts/Jev/`:
  - `IJevClient.cs`
  - `JevTypes.cs`: request, câu hỏi Choice/Score/Noul, response; serialize bằng `JsonUtility` hoặc Newtonsoft.
  - `ProxyJevClient.cs`, `DirectJevClient.cs`, `OfflineJevClient.cs`
  - `JevConfig.cs` (ScriptableObject: chọn cách kết nối, URL, thời gian chờ, ngưỡng confidence)
  - `JevDirector.cs`: soạn state và câu hỏi, gọi Jev trước mỗi đợt, áp kết quả vào `EncounterWave`.
  - `PlayerStatsTracker.cs`
  - `JevRankEvaluator.cs`
  - `JevDebugOverlay.cs`
- `Scripts/Editor/JevPlaytestReview.cs`: cửa sổ Editor để chấm độ khó từ log chơi thử.
- `Scripts/Grenade.cs`: lựu đạn bay tới kèm vòng target nhỏ, tap để bắn hạ.
- `Scripts/HumanShieldEnemy.cs`: enemy giữ con tin làm khiên, phải tap đúng chấm Justice Shot hoặc đầu enemy.
- Ở thư mục gốc repo (ngoài `Assets`): `Server/python/{app.py, requirements.txt}`, `Server/node/{server.js, package.json}`, `Server/.env.example`, và thêm `.env` vào `.gitignore`.
- `Prefabs/Enemy.prefab`, `Prefabs/TargetReticle.prefab`, ảnh vòng tròn (tạo sprite dạng ring bằng code hoặc dùng `Image` có sẵn với sprite tròn).
- `Scenes/Level01.unity`: 3 khu vực dựng bằng cube (đường phố, kho hàng, mái nhà), các spline ray nối giữa chúng, các `CinemachineCamera` cho từng góc, mỗi đợt 2–5 enemy.

## Thiết lập cho mobile
- Chỉ làm cho Android: chuyển build target sang Android (nếu module Android chưa cài thì mình sẽ báo, không tự cài), khóa màn hình dọc (portrait; đổi từ ngang ngày 2026-10-04).
- Canvas dùng `Scale With Screen Size` với độ phân giải chuẩn 1920×1080. Nút Reload đủ to cho ngón tay.
- Dùng `Mobile_RPAsset` sẵn có. Đặt `Application.targetFrameRate = 60`.

## Các bước thực hiện (qua Unity MCP)
-1. Môi trường: máy đã có Python 3.12. Cài Node.js LTS qua `winget install OpenJS.NodeJS.LTS` (bạn đã yêu cầu). Bạn đã có TypeSafe API key và sẽ tự dán vào `Server/.env`.
0. Thêm package Cinemachine 3 (và Splines nếu chưa có) qua `manage_packages`.
1. Tạo thư mục và các script. Refresh Unity, rồi đọc console để chắc chắn không có lỗi compile.
2. Dựng scene Level01: môi trường, waypoint, các EncounterZone, Canvas và HUD.
3. Tạo các prefab Enemy và TargetReticle, rồi gán tham chiếu.
4. Cấu hình Player Settings và Build Settings cho mobile.
5. Viết các lớp Jev trong Unity, có `OfflineJevClient` trước để game chạy được ngay cả khi chưa có key.
6. Viết hai server Python và Node, sau đó nối `ProxyJevClient` và `DirectJevClient`.
7. Viết bảng debug, màn đánh giá cuối màn, và cửa sổ Editor chấm log chơi thử.

## Kiểm tra
- `read_console` sau mỗi lần compile: không có lỗi.
- Vào Play mode qua MCP, mở Game view hoặc Device Simulator rồi click chuột giả lập tap, kiểm tra:
  - Camera chạy đúng thứ tự Phase → Shot → góc phụ; Cut, Blend, Spline đều hoạt động; zoom nhẹ thấy rõ; fade và tiêu đề hiện khi đổi Phase; bị bắn thì camera rung.
  - Vòng target vẫn bám đúng enemy khi camera đang blend hoặc zoom, và tạm dừng thu nhỏ trong lúc blend.
  - Cảm giác camera: không có cú xuất phát hay dừng đột ngột; tốc độ xoay không vượt giới hạn; chơi liền 3 Phase không thấy chóng mặt; bật "Giảm chuyển động" thì hết rung và nghiêng.
  - Vòng target thu nhỏ, đổi màu và bám theo enemy.
  - Tap trúng thì enemy chết và được cộng điểm; tap trượt thì tốn đạn; vòng thu hết thì mất mạng.
  - Tap trúng con tin thì mất mạng; combo tăng và reset đúng; tap trúng chấm ở tay thì ra Justice Shot; nhặt shotgun và súng máy hoạt động, hết đạn đặc biệt thì quay về súng lục.
  - Mất tim thì icon trái tim giảm theo. Hết tim thì ô Revive hiện ra; bấm Revive thì quảng cáo giả chạy 3 giây rồi hồi 3 tim, còn hết giờ hoặc bỏ qua thì sang Game Over; khi đã dùng hết lượt revive thì nút Revive bị khóa.
  - Vật thể tương tác:
    - Hộp bay theo hướng đạn; cây rụng lá; kính vỡ thành mảnh; cửa bật mở và lộ ra thứ phía sau; thùng nổ hạ enemy ở gần; bóng đèn tắt.
    - Mảnh vỡ tự biến mất, số Rigidbody đang hoạt động không vượt giới hạn.
    - Tap vào enemy đứng trước vật thể thì luôn trúng enemy.
  - Màn tiêu đề "TAP TO START" vào được Level01. Các hiệu ứng tia lửa, vết đạn, chữ điểm bay lên và enemy văng ra đều hiện đúng.
  - Màn Win và Game Over hiện đúng, nút Chơi lại hoạt động.
- Kiểm tra Jev:
  - `GET /health` của cả hai server trả về OK.
  - Một request mẫu gửi tới `/jev` nhận về các câu trả lời đúng kiểu.
  - Chơi thử lần lượt với ba cách kết nối (Proxy, Direct, Offline). Bảng debug phải hiện quyết định của Jev, và đợt giao tranh phải thay đổi theo quyết định đó.
  - Tắt server hoặc ngắt mạng giữa trận: game tự chuyển sang Offline, không bị đứng.
  - Chơi hai lượt với hai phong cách khác nhau (bắn chuẩn và bắn ẩu). `player_skill` và `wave_preset` phải khác nhau giữa hai lượt.
  - Bản release không chứa `DirectJevClient`, và trong build không có API key.
- Chụp screenshot Game view để bạn xem.
- Lưu ý: lỗi Burst cache (mã 4551) không ảnh hưởng prototype này. Nếu thấy cần, có thể xóa `Library/BurstCache` riêng.
