# CAM-SMOOTH - Bao cao do + sua camera (gameplay-coder)

Do bang Play mode that: Level_01, DebugM2Bot (ban het level) + DebugCamTrace (moi) ghi pose `Camera.main` cuoi moi frame
(~5600 frame/lan, ~93 s game). Toc do/gia toc tinh tren cua so 0.1 s (loai jitter dt cua Editor 10-24 ms); "rawFrame" la dot bien 1 frame.
Phan tich bang script tam (scratchpad). So lieu tho: `Temp/trace_before_*.csv`, `Temp/trace_final_*.csv`, `Temp/an2_*.txt` (khong commit).

## 0. Phat hien quan trong
`PlayerPrefs cc_reduce_motion` trong Editor dang = **1** (Giam chuyen dong BAT). O che do nay code cu **Cut** moi lan 2 shot lech > 30 do
(`reduceMotionCutAngle`) => day chinh la "giat khi chuyen goc" nguoi dung thay neu choi voi tuy chon nay. Da do ca hai che do.
(Gia tri PlayerPrefs da khoi phuc ve 1 sau khi do; khong doi.)

## 1. Truoc khi sua - cac diem spike (thoi diem / nguyen nhan)
Che do Giam chuyen dong (ReduceMotion = true), 9 Cut giua man:
| t (s) | Shot | dGoc | dVi tri | Nguyen nhan |
|---|---|---|---|---|
| 9.67 | P1_S2->S3 Combat | 52.1 | 0.85 m | Cut do `reduceMotionCutAngle`=30 |
| 12.06 | P1_S3->Rail P1_S4 | 47.7 | 0 | Cut do reduceMotionCutAngle |
| 20.56 | P1_S5->S6 | 39.3 | 0.90 | nt |
| 31.43 | P2_S2->S3 | 46.7 | 0.67 | nt |
| 42.33 | P2_S5->S6 | 59.0 | 1.53 | nt |
| 54.18 | P3_S2->S3 | 52.0 | 5.69 | nt |
| 65.66 | P3_S5->S6 | 46.1 | 0.72 | nt |
| 22.16 / 44.87 | sang Phase 2 / 3 | 140 / 59 | 100 m | Cut co chu y luc man den (giu lai) |
Toc do xoay frame-don luc Cut 2400-3800 do/s.

Che do thuong (ReduceMotion = false) - khong con Cut giua man (chi 2 Cut sang Phase duoi man den), nhung:
- **FOV nhay**: `SlowZoom.Punch` bat dau lai tu 0 khi co punch moi chong len (offset rơi ve 0 trong 1 frame): toc do FOV 70-212 do/s tai nhieu diem (vd t=35.2 P2_S2 212/s, t=52.6 P2_S6 112/s, t=22.2 P1_S5 102/s). Punch thuong 18-20 do/s.
- **Blend Combat->Combat / Combat->Rail**: dinh xoay 56-60 do/s, gia toc cua so 0.1 s toi 202-331 do/s^2 (EaseInOut co dinh 0.5 s keo dai theo `maxBlendAngularSpeed=40` trung binh => dinh 60).
- **Ray xoay sau khi toi noi**: `RunMove` chi Prepare camera ray theo huong CU roi de ray tu xoay (SmoothDamp 0.35 s, 60 do/s) sau khi blend xong => P1_S4 xoay 48 do trong ~1 s, gia toc 331 do/s^2; P3_S4/P2_S4 gia toc ~224-255.
- Settle FOV dung EaseOut (van toc dau khac 0): ~5 do/s FOV bat dau dot ngot khi toi diem.
- Hit shake o tan so 40 Hz (khong bi loc) - giu nguyen y do, tach rieng khoi luoi lam muot.

## 2. Sua (tat ca so lieu o `CameraFeelProfile`, asset `Settings/CameraFeelProfile.asset`)
1. **Khong con Cut** tru: lan vao level dau tien va luc man den giua Phase (`forceCutNext`; 100 m khong the blend). `ShouldCut`, `cutAngle`, `reduceMotionCutAngle`, `ShotEntry.Cut` khong con tac dung (field giu lai, = 360). Giam chuyen dong KHONG Cut: chi blend cham hon (thoi gian / `smoothReduceMotionScale` 0.6) va tat noise/punch/kill-zoom.
2. **Thoi gian blend tu co gian** (`PhaseDirector.ScaleBlend`): T = max(defaultBlend, d/maxBlendSpeed, goc/maxBlendAngularSpeed, sqrt(6d/blendMaxAccel), sqrt(6goc/blendMaxAngAccel)); EaseInOut co dinh (van toc ve 0 mem o 2 dau; dinh = 1.5 x TB, gia toc dinh = 6*delta/T^2). Tran `maxBlendTime` 2.5 -> 4 s. `maxBlendAngularSpeed` 40 -> 30, `blendMaxAngAccel` = 110, `blendMaxAccel` = 14.
3. **Camera ray vao dung huong tiep tuyen ngay luc blend** (Cinemachine xoay, co gian theo goc) thay vi xoay sau khi toi noi. Rail->Rail (cung camera) giu huong cu.
4. **Punch FOV lien tuc**: `SlowZoom` giu 2 o (dang chay + duoi), do lech = max(hai o) => khong con nhay khi punch chong. Bien do nho lai: punch 2->1.2 do (vao 0.22 s, ra 0.6 s), Justice 3->1.8 (0.3/1.0), kill-zoom FOV 3->2, xoay ve kill 4->2.5 do, thoi gian 0.5->0.6. Settle FOV doi sang smoothstep.
5. **Pan theo muc tieu (trackYaw)** chi 8 do (truoc 12), 12 do/s (truoc 20), smooth 1.0 s (truoc 0.8). Ray: `maxYawRate` 60->45, `lookDamping` 0.35->0.5.
6. **Luoi an toan cuoi pipeline** `CameraPoseSmoother` (moi, `Camera/CameraPoseSmoother.cs`): chay trong `CinemachineCore.CameraUpdatedEvent` (ngay sau Brain, truoc moi LateUpdate khac nen reticle/tap thay dung pose), doc pose tho tu `Brain.State`; xoay/vi tri/FOV di theo pose tho nhung KHONG vuot van toc va gia toc toi da (xoay 70 do/s, 260 do/s^2; vi tri 12 m/s, 30 m/s^2; FOV 14 do/s, 50 do/s^2; x0.6 khi Giam chuyen dong), tu phanh bang `sqrt(2a d)` nen khong vot lo. Rung trung dan/no tach rieng (`CameraFeelState.ShakePos/ShakeRot`) va cong lai sau nen khong bi loc mat. Do tre toi da quan sat: 3.4-6.2 do, 0.36-0.55 m. Snap chi khi `RequestSnap` (vao level / man den giua Phase) - cho den khi camera dich da live.
7. Khong doi Priority khi dang blend: giu nguyen quy trinh cu (`WaitTransition` cho blend xong, blend sub-angle va kill-zoom noi tiep, khong ngat giua chung); luoi an toan bao ve khi co ngoai le.

## 3. Sau khi sua (cung kich ban bot, Level_01 tron ven)
| Chi so (cua so 0.1 s, loai Cut duoi man den) | Truoc (thuong) | Sau (thuong) | Truoc (Giam chuyen dong) | Sau (Giam chuyen dong) |
|---|---|---|---|---|
| Cut giua man | 0 | 0 | 7 (39-59 do) | 0 |
| Van toc xoay toi da (do/s) | 60.0 | 44.2 | 586 (tho 3824) | 42.0 |
| Gia toc xoay toi da (do/s^2) | 331 | 174 | 5824 | 135 |
| Van toc vi tri toi da (m/s) | 5.9 | 4.8 | 56.6 | 4.5 |
| Gia toc vi tri toi da (m/s^2) | 15.2 | 16.4 | 563 | 7.9 |
| Toc do FOV toi da (do/s) | 21.8 (dot bien frame 70-382) | 8.3 | 0 | 0 |
| Gia toc FOV toi da (do/s^2) | 363 | 66 | 0 | 0 |
Trong luc blend Combat->Combat sau sua: dinh xoay 40-44 do/s, gia toc 130-174 do/s^2 (truoc 56-60 va 202-331). Luoi an toan do duoc: van toc xoay toi da 45 do/s, FOV 8.6 do/s, do tre 3.4 do.
Console: 0 loi, 0 canh bao. Scene khong luu (khong dirty).

## 4. Viec cho level-designer (chi bao cao, khong sua scene/map)
Khong con Cut nen cac cap shot lech lon se blend dai (T tu co gian toi 4 s). De blend ngan/dep, nen xoay lai hoac dat gan nhau:
- Combat->Combat lech goc: P1_S2->S3 52 do (0.85 m), P2_S5->S6 59 do (1.53 m), P3_S2->S3 52 do (**5.69 m**, nen dich vi tri gan nhau), P2_S2->S3 47 do, P3_S5->S6 46 do, P1_S5->S6 39 do. Muc tieu <= 30 do va <= 1 m => blend ~1 s.
- P1_S3 -> ray P1_S4: lech 48 do giua huong nhin P1_S3 va tiep tuyen dau ray (hien blend ~1.5 s). Xoay P1_S3 ve huong tiep tuyen dau ray (< 20 do) la dep nhat.
- P1_S6->P2_S1 va P2_S6->P3_S1 nhay 100 m: giu Cut luc man den (hop le); neu muon khong Cut thi phai noi lien khong gian.
- `ShotEntry.Cut` tren cac shot khong con tac dung (neu muon dung de nhay du kien thi bao gameplay-coder).

## 5. File da doi
`Scripts/Camera/`: `CameraPoseSmoother.cs` (moi), `PhaseDirector.cs`, `CameraFeelProfile.cs`, `CameraFeelApplier.cs`, `CameraFeelState.cs`, `SlowZoom.cs`;
`Scripts/Game/Debug/DebugCamTrace.cs` (moi, chi Editor/Dev, chua gan vao scene); `Settings/CameraFeelProfile.asset`.
Khong dung toi `Combat/`, `Enemy/`, scene, map. Khong commit.

## 6. Cach do lai
Vao Play `Level_01`, gan `DebugCamTrace` (outPath) + `DebugM2Bot` bang `execute_code`, doi `LEVEL_COMPLETED`, phan tich CSV. `PhaseDirector.Smoother` lo ra `MaxAngVel/MaxAngAcc/MaxVel/MaxAcc/MaxFovRate/MaxLagAngle/MaxLagPos`.
