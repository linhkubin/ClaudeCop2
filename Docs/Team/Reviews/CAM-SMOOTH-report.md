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

## 7. CAM-LIVELY (gameplay-coder) - sinh dong: Move mem, Combat co nhip + "giat minh quay sang"
Do bang Play mode that, Level_01 + DebugM2Bot + DebugCamTrace (them cot rx,ry,rz = target reaction, cd = dolly combo), ReduceMotion = false, cua so 0.1 s. Du lieu: `Temp/trace_livelybefore.csv`, `Temp/trace_livelyafter.csv`.

**Move (bo lac 2 ben):** bob chi con nhun DOC (bobAmplitude 0.025 -> 0.006, bo thanh phan x); roll rail tat (maxRoll 2 -> 0) va roll cam tay = handheldRollScale 0; cam tay chi chay khi Combat (nhan CombatWeight, nen mat dan khi Move/blend); ray xoay mem hon (maxYawRate 45 -> 26, lookDamping 0.5 -> 1.1).
**Combat:** (a) tho FOV +-1.2 deg chu ky 4.5 s (breathFov/breathPeriod); dolly-in theo combo (toi da 2 deg, 0.25/combo, smooth 0.9 s, ease ra khi het dot); push-in cu giu. (b) Reaction (`CameraReaction.cs`, logic thuan): nghe `TargetRegistry.Registered` (enemy vua targetable), do lech so voi truc camera, gop trong 0.12 s thanh 1 lan huong ve trong tam, vao 0.15 s (ease-out) - giu 0.12 - ve 0.8 s (smoothstep), punch FOV 1.5 deg, cooldown 1.6 s, bien do ~4.5-8 yaw / <=4 pitch. Khong chay khi Move/blend/CombatPause/man den/kill-zoom; punch FOV cua reaction giam theo punch ha enemy dang chay. `CameraPoseSmoother` co KENH reaction rieng (gioi han rieng 170 deg/s, 2200 deg/s^2, FOV 40 deg/s) ap SAU luoi chinh nen luoi chinh van chan spike ngoai y muon. ReduceMotion: reaction/tho/dolly/cam tay tat (reactReduceMotionScale 0). Moi so nam trong CameraFeelProfile (header "Sinh dong (CAM-LIVELY)"). Camera.main that bi xoay nen tap/reticle/sung (con cua Main Camera) di theo; screenshot Game view luc reaction dang len dinh: sung khong xuyen/lech, reticle bam enemy (`Assets/Screenshots/CAMLIVELY_reaction.png`).

| Chi so | Truoc | Sau |
|---|---|---|
| Move: van toc xoay toi da (deg/s, ngoai blend) | 44.5 | 26.1 |
| Move: |roll| toi da (deg) | 0.04 | 0.10 (nhieu, ~0) |
| Move: dao dong doc/ngang vi tri y (std, m) | 0.0152 | 0.0059 (x bob bo) |
| Move: dinh toc do xoay trong 1 doan (deg/s) | 44 | 31 |
| Combat: roll toi da (deg) | 0.45 | 0.77 (handheld khong con roll; so do tang do reaction lam nghieng nhe khi pitch) |
| Reaction / level (bot ban het) | 0 | 18 (yaw toi da 6.2, pitch 4.0, punch FOV 1.5; tat ca luc blend=0) |
| Van toc xoay kenh reaction toi da | - | 111 deg/s (gioi han 170) ; FOV 25.7 deg/s |
| Van toc xoay luoi chinh toi da | 44-45 | 43 (Move+blend khong doi so CAM-SMOOTH) |
| FOV khi Combat | 39.0-45.0 | 37.7-45.0 (tho + dolly combo) |
Ghi chu: gia tri "gia toc toi da" ca hai ben bi nhieu boi Cut luc man den giua Phase, bo qua.
Pitch hay cham tran 4 deg vi nhieu enemy o tang cao/thap; neu qua "nhao" co the ha reactMaxPitch. Yaw nho vi phan lon enemy gan giua khung ngang.
Test: `Game/Tests/CameraReactionTests.cs` (10 test). EditMode 262/262. Asset `Settings/CameraFeelProfile.asset` them cac truong moi (neu mo editor thay gia tri cu, re-import). File: Camera/{CameraReaction(moi),CameraFeelApplier,CameraFeelProfile,CameraFeelState,CameraPoseSmoother,PhaseDirector}.cs, Game/Debug/DebugCamTrace.cs, Game/Tests/CameraReactionTests.cs.


## 8. CAM-VC2 (gameplay-coder) - sung khop dan, giat khi ban, nhip ray + nhin truoc kieu VC2
Do bang Play mode that (Level_01, DebugM2Bot + DebugCamTrace, ReduceMotion = false set bang reflection tren static, KHONG ghi PlayerPrefs). Du lieu: `Temp/trace_vc2before.csv`, `trace_vc2after.csv`, anh `Temp/vc2b_*.png`.

**A. Sung - dan khop.** `MuzzleAnchor.TryAimAt(diemTrung, out goc)` (Core, moi): FxSystem goi truoc khi sinh vet; ViewmodelController (ngam trong CUNG lan goi, nen khong phu thuoc thu tu ShotFired/ShotResolved) xoay `AimRoot` (cha moi cua cac sung, khong dung Animator) 3 vong lap de truc nong (+Z cua Muzzle) chi vao diem trung; diem ngam dung cung toa do man hinh qua camera overlay (FOV 45 co dinh khac FOV camera chinh) nen tren man hinh nong va vet dan thang hang. Vet dan xuat phat tu nong quy ve camera chinh. `PreAim` luc ShotFired (raycast) giup sung quay ca khi khong co FX. Kep `aimMaxAngle` 30 do (trong game dung toi ~28, chua kep lan nao trong 98 phat); hold 0.10 s, ve nghi 0.22 s (smoothstep). Snap la TUC THOI (1 frame) de vet va nong khop dung frame ban; khong dung aimSnapTime 0.04-0.06. Huong theo di chuyen: `ViewmodelMotion.TickMove` - vao cua mui sung tre nguoc huong re (0.10 do/(do/s), toi da 3.5 do) + roll 0.05/(do/s) toi da 2.5 do + hat mui 1.2 do theo toc do tien, SmoothDamp 0.18 s, nhan reduceMotionScale; Cut/hitch bi bo qua. So: `ViewmodelConfig` (aim*) va `ViewmodelMotionSettings` (move*).
Do (98 phat, bot): lech goc tren man hinh giua huong nong va vet dan: luc ngam max 0.30 do, cuoi frame ban (da tinh giat spring) 0.32 do; 9:16 5 diem (giua/trai/phai/tren/duoi + goc) = 0.00-0.15 do. Goc 3D 0.1-2.2 do chi do FOV overlay khac FOV camera chinh (khong thay tren man hinh). 9:19.5: anh chup xac nhan nong va vet cung huong; so do 2D cua cach chup RT 1080x2340 sai do overlay camera dung pixel man hinh that (artifact cua cach do, khong co tren thiet bi).
Anh: `Temp/vc2b_1080x1920_{center,left,right,top,bottom}.png`, `vc2b_1080x2340_*`. Dau + xanh = diem tap.

**B. Giat khi ban.** `CameraKick` (thuan) + `CameraFeelState.KickTarget` + kenh rieng trong `CameraPoseSmoother` (gioi han 120 do/s, 6000 do/s^2, FOV 20 do/s). Moi phat: pitch 0.4 do x he so (Pistol 1, Shotgun 1.5, MG 0.45), FOV 0.3 x he so, vao 0.03 s - ve 0.09 s (smoothstep), cong don co tran 0.8 do / 0.5 do. Giam 70% khi CameraReaction/kill-zoom dang chay (`kickReactDamp`). Giam chuyen dong: tat (`kickReduceMotionScale` 0). Tap van dung Camera.main hien tai. Do: 49 phat -> dinh pitch 0.60 do (Shotgun), FOV 0.45, van toc kenh 23 do/s; luoi chinh khong doi (xoay toi da 40 do/s, gia toc 485 do/s^2 la nhieu cua Cut man den nhu truoc). Plan da sua mot dong.

**C. Nhip ray VC2.** `RailSpeedCurve` (thuan) thay hinh thang khi `railCurveEnabled`: tang toc smoothstep 0.8 s, chay deu, giam toc mem tren 25% quang duong cuoi (v*(1-smoothstep), gia toc lien tuc); toc do deu chon sao cho TONG THOI GIAN BANG hinh thang cu (maxMoveSeconds 5.4 giu nguyen; speedOverride van la toc do goc). Nhin truoc: `RailCameraDriver.SetNextLook` (PhaseDirector.RunMove truyen huong cua shot Combat ke trong cung Phase), tu 70% quang duong nghieng dan (smoothstep) ve dung huong shot, damping 0.55, tran xoay 40 do/s. Reaction giu nguyen. Khong doi map/rail/ten CamPoint.
| Chi so (Move, ngoai cac Cut man den) | Truoc | Sau |
|---|---|---|
| Thoi gian doan Move P1_S4 / P2_S4 / P3_S4 | 5.90 / 5.93 / 5.24 s | 5.93 / 5.93 / 5.25 s (bang) |
| Van toc xoay Move toi da | 25.8 do/s | 39.3 do/s (<= 44) |
| Toc do tai 0.5 s dau (P1_S4) | 1.50 m/s | 2.38 m/s |
| Toc do 0.3 s cuoi | 0.67-0.98 m/s | 0.10-0.14 m/s (vao diem mem hon) |
| Toc do dinh | 4.46 m/s | 4.81 m/s |
| Dao dong ngang (std) | 0.058/0.028/0.054 m | 0.056/0.029/0.054 m (khong doi) |
| Blend Move->Combat | 0.49-0.51 s | 0.50 s |
Test: EditMode them `Game/Tests/CameraVc2Tests.cs` (5), `Viewmodel/Tests/ViewmodelAimTests.cs` (4). File: Core/MuzzleAnchor.cs, FX/FxSystem.cs, Viewmodel/{ViewmodelController,ViewmodelConfig,ViewmodelMotion}.cs, Camera/{CameraKick(moi),RailSpeedCurve(moi),RailCameraDriver,PhaseDirector,CameraPoseSmoother,CameraFeelState,CameraFeelProfile}.cs. Asset khong sua (truong moi dung gia tri mac dinh trong script).
Luu y: ban "truoc" trong bang do voi railCurveEnabled/lookNext/kick tat (doan Move dau tien lo ra 1 lan dung duong cong moi, bi loai khoi so truoc/sau).
