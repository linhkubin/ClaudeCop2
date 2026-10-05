# REV-WAVE3 - review (reviewer, che do full, chi doc, khong vao Play)

Ket luan: **CHANGES REQUESTED (nhe)**. Khong co loi nghiem trong (🔴). 3 muc 🟡 nen sua truoc khi commit, con lai la goi y. Console Unity: khong co loi compile/runtime (chi 2 dong khong lien quan: NoSubscription generators.ai, PlayerDamageService "khong co receiver").

Pham vi: file Viewmodel/ la untracked nen khong co `git diff` (tests/config chi doc truc tiep); phan con lai doc diff voi commit 9963684.

## 1. Viewmodel
Dung yeu cau:
- asmdef `ClaudeCop.Viewmodel` ref Core + Combat (it hon bang Conventions: khong ref URP, khong can). Khong asmdef/script nao ngoai Viewmodel ref hoac `using ClaudeCop.Viewmodel` (grep sach). Debug driver boc `#if UNITY_EDITOR || DEVELOPMENT_BUILD`. Tests asmdef dung (Editor, UNITY_INCLUDE_TESTS).
- `ViewmodelController`: dang ky 4 event CombatEvents o OnEnable, huy o OnDisable (doi xung). `MuzzleAnchor.Set` khi chon sung, `Clear(socket)` khi doi sung va OnDisable (Clear co kiem tra "chi xoa neu la anchor hien tai" - tot). Chia 0 reload duoc chan: `t > 0.001f ? clipLength/t : max`, roi Clamp theo `reloadSpeedRange`. Khong Find*/GetComponent/cap phat trong LateUpdate (Find/Dictionary chi o init/Start). ReduceMotion nhan `reduceMotionScale` (0.35) cho giat/nghieng.
- Level_01: ViewmodelRoot (layer 8 Viewmodel) + ViewmodelCamera deu la con Main Camera; ViewmodelCamera `Untagged`, CameraType=Overlay (1), cullingMask=256 (chi Viewmodel), clearDepth=1, postprocess tat; Main Camera them `UniversalAdditionalCameraData` voi `m_Cameras` = [ViewmodelCamera], culling mask 4294967039 (= ~256). Chi 1 camera tag MainCamera -> `Camera.main`, `TapShooter.cam`, FxSystem khong bi anh huong. Prefab viewmodel khong co Collider nao -> khong chan raycast tap. Khong dung `PointerBlocker`; hit-test khong doi. Khong code nao dung `Camera.allCameras`/`FindObjectsByType<Camera>`.
- TagManager: them layer `Viewmodel` o index 8; `serializedVersion` 2->3 va bo cac dong layer trong cuoi file la Unity tu chuan hoa, khong anh huong.
- Mobile: Overlay + MSAA (`m_AllowMSAA 1`, MSAA lay tu pipeline asset) OK; vat lieu viewmodel dung shader URP (guid `933532a4...`/`0406db5a...`), tham chieu qua prefab nen khong bi strip.

**Test recoil (yeu cau dac biet)**: khong co bang chung doi gia tri recoil. Toi tinh tay: spring stiffness 260, damping 26 (zeta ~0.81, w ~16.1), impulse 9 => dinh 1 phat ~ 9/16.1 * 0.42 ~ 0.235; ban lien 10 phat/s do 0.226 (khop). Nguong cu 0.3 la uoc luong sai cua test, khong phai cau hinh bi giam -> ha xuong 0.15 la hop ly ve ban chat. Tuy nhien:
- 🟡 `Viewmodel/Tests/ViewmodelMotionTests.cs:36-39` (`Recoil_StaysWithinLimit_UnderRapidFire`): test nay khong con bao ve gi that. Cac assert tran `recoil <= recoilMax (1.6)` va `>= -0.3*max` khong bao gio duoc cham (dinh thuc te 0.23, cach tran ~7 lan) nen luon pass dù code kep (`Tick` dong 74-76) bi xoa. Assert duoi `> 0.15` cung chi dam bao "co giat". Huong sua: them ca test ep tran that (vd. impulse 9 nhan len, hoac `recoilMax` set nho ~0.1, hoac ban 60 phat/s) roi assert recoil dung bang max va khong vuot; giu nguong "thay duoc" 0.15 (don vi: 0.15 * 0.05m ~ 7.5mm, 0.15*6 deg ~ 0.9 deg - kha nho, nen can nhac co lien ket voi clip Fire de nhin thay).
- 🟢 `Tilt_ClampedToMaxAngles` (dong 63-80) gan `s.recoilImpulse = 0f` SAU khi tao `m`/OnShot (dong 69) nen khong co tac dung voi lan thu 1, va khong assert pitch (x). Nho: sua cho khop y dinh.
- 🟢 Animator cua viewmodel chay theo thoi gian co timeScale (slow-mo kill zoom lam clip cham) trong khi spring dung `unscaledDeltaTime`; neu muon dong bo, dat `animator.updateMode = UnscaledTime`.
- 🟢 `ViewmodelController.cs:68`: neu layer `Viewmodel` bi xoa (`NameToLayer < 0`) thi sung se hien lech/khong dung camera ma khong canh bao -> them `Debug.LogWarning`.
- 🟢 Chi phi mobile: them 1 camera Overlay (them mot pass + clear depth). Chap nhan duoc cho 3 mesh nho, nen do tren may that (report da ghi chua test tren thiet bi).

## 2. Vet dan (FX / MuzzleAnchor)
- `TracerPool`: tao san `capacity` LineRenderer luc khoi tao, `Spawn` ghi de vet cu nhat (vong), `Tick` chi lap qua mang co dinh; chi cap phat struct `Color` (khong heap). Pool 24 phu hop. `TracerMath` thuan, co test.
- `Blocked` khong sinh vet (`FxSystem.cs SpawnTracers` dong dau tien). Thuc te `TapShooter.TryFire` thoat truoc khi phat bat ky event nao khi bi PointerBlocker nen cung khong co ShotResolved.
- MuzzleAnchor het han: `anchor != null` dung ve overload Unity (socket bi destroy -> false) => FxSystem tu fallback ve canh duoi man hinh; viewmodel OnDisable cung Clear. Dung. `ResetStatics` SubsystemRegistration co.
- Vat lieu `Shader.Find("Sprites/Default")` luc chay: `Sprites/Default` nam trong Always Included Shaders (fileID 10753 co trong GraphicsSettings.asset) nen build khong bi strip; shader nay khong co LightMode nen URP ve qua pass SRPDefaultUnlit, ho tro mau dinh diem + alpha - OK tren URP mobile. 🟢 De chac hon, gan san `FxConfig.tracerMaterial` (vat lieu asset) thay vi Find luc chay; vat lieu tao runtime (`DontSave`) khong bi Destroy khi FxSystem huy (ro nho, khong dang ke).
- ReduceMotion: giam alpha va tail (0.5), toi da 2 vet. OK.
- 🟢 Viewmodel nong sung la vat the world nam sat camera (0.45m) nen dau vet co the bi near-plane cua Main Camera cat vai frame dau; chap nhan duoc (report da ghi lech vai pixel).
- Phat sinh: `Core/MuzzleAnchor.cs` va `Core/PriorityShootable.cs` la file MOI trong Core ma Conventions ghi "dong bang sau R-W1, them qua PM". Xem muc 🟡-3 duoi.

## 3. Thung no uu tien
- Thu tu: Justice (count>0 && hits[0].Justice -> bo qua thung) > thung > enemy/pickup. Chi ap dung khi vu khi 1 muc tieu (Pistol/MG); shotgun khong uu tien thung (chap nhan, nen ghi vao spec). Hook dat sau `TargetSelector.Select` nen khong anh huong body-hit / NearestTargetFirst; delegate cache 1 lan (khong cap phat moi tap); `Physics.RaycastNonAlloc`.
- Tap len nut UI: `PointerBlocker` chay truoc trong `TryFire` -> khong den ResolveShot. OK. Ammo da tru truoc ResolveShot, `ResolveBarrel` tra `null` (khong phai pickup) nen khong hoan dan nham. `EmitEnvironmentResult(rayHit true, shootable true)` giu combo - nhat quan voi duong raycast cu.
- Dang ky/huy: `Awake` Register, `OnDestroy` Unregister (list static, `Reset` o SubsystemRegistration) -> load scene khong ro. `Restore()` khong can dang ky lai. 
- 🟡-1 `Props/ExplosiveBarrel.cs` (`IsPriorityLive => !exploded`, dong ~28): khong kiem tra `gameObject.activeInHierarchy`. Thung nam trong doan/phong bi `SetActive(false)` (da Awake truoc do) van con trong registry; `PriorityBounds` van tinh duoc tu collider -> tap vao vung man hinh do se no thung "ma" o phong an (kich ban: level bat/tat khu vuc theo encounter). Huong sua: `IsPriorityLive => !exploded && isActiveAndEnabled` KHONG dung duoc (component `enabled=false` thuong xuyen) -> dung `gameObject.activeInHierarchy`. Them test.
- 🟢 Khong gioi han khoang cach (>`MaxRayDistance`) khi chon thung: thung rat xa van tap duoc voi padding 8px. Them kiem tra khoang cach neu can.
- 🟢 Enemy dung truoc thung chong rect: thung thang (theo yeu cau "uu tien"); occlusion bo qua ITapTarget nen enemy khong che. Dung y dinh nhung nen noi ro cho nguoi choi/level designer.

## 4. Camera sinh dong
- `CameraReaction` (logic thuan) + 10 test: gop Notify theo cua so, cooldown, ease vao-giu-ra, Target ve dung `Vector3.zero` sau khi het. `PhaseDirector`: dang ky `ComboChanged` + `TargetRegistry.Registered` o OnEnable, huy o OnDisable cung luc `reaction.Cancel()` va xoa `ReactTarget/ComboDolly` (doi xung, khong ro). `reactionOk` chi true sau `ArmFeel`, tat trong `Activate` + het dot; `allowed` chan khi khong Combat / dang blend / pause / transition / kill-zoom. Cooldown (1.6s tinh tu luc bat dau, hon tong 1.07s) nen khong chong len nhau.
- `CameraPoseSmoother`: kenh reaction (rA/rAV/rF/rFV) dung `Step` co gioi han van toc/gia toc rieng, ap SAU luoi chinh; `Step` luon dung dung o dich khi du gan nen ve dung 0 (khong ket lech). Snap dau tien (`!has`) khoi tao `committed` rA=0. Khi `timeScale==0` frame bi dung dong bang, tiep tuc khi het -> khong spike. Cancel dot ngot chi keo `ReactTarget` ve 0, smoother tu ease ve (toi da 170 do/s, 2200 do/s^2) - khong giat cut. Hai lan goi CameraUpdated/frame van dung committed/cand pattern cu.
- Huong ban: `TapShooter` dung `cam.ScreenPointToRay` voi pose Main Camera da bao gom reaction (cung transform dang render), nen WYSIWYG; `fieldOfView` cung gan sau Cinemachine -> projection khop. Reticle/aim-assist dung chung `Project` => khop.
- ReduceMotion: `reactReduceMotionScale=0` => reaction tat han; dolly combo/breath/handheld/bob dung nhanh `!reduce`. **Mac dinh `UserSettings.ReduceMotion` = false khi chua co PlayerPrefs** (`GetInt(key, 0) == 1`); UI co toggle `ReduceMotionToggle` o man Title (`TitleView`, co test). Trong Editor PlayerPrefs `cc_reduce_motion=1` nen hieu ung moi se TAT: can bo tick "Giam chuyen dong" o Title (hoac xoa key) truoc khi danh gia cam giac. Chua co toggle trong luc choi (pause menu), chi o Title.
- Bo roll/bob ngang: `maxRoll=0`, `handheldRollScale=0`, `bobAmplitude 0.025->0.006`, bob ngang xoa. Nguon lac con lai khi Move: chi bob doc 0.006m (gan nhu khong), handheld da gioi han `* CombatWeight` (chi Combat). Viewmodel khong co sway rieng khi di chuyen, chi giat theo phat ban -> khi Move camera se rat "phang" (hop voi yeu cau "bo lac", nhung neu cam thay chet co the tang `bobAmplitude` hoac them idle-sway nhe cho viewmodel). Khong phai loi.
- 🟡-2 `PhaseDirector.TickLiveliness` dung `Time.deltaTime` cho `SmoothDamp` (ComboDolly) va `Time.time` cho reaction; khi timeScale rat nho (slow-mo kill zoom) dolly gan nhu dung - khong hong gi, chi la hanh vi. Khong yeu cau sua, ghi de biet.
- 🟢 `TickLiveliness` tren duong `Registered` bat ca enemy dang ky lai (`EnemyActor.cs:265`, khi lo ra lan nua sau khi nap dan) -> chap nhan nho cooldown 1.6s.
- 🟢 DebugCamTrace chi them 4 cot CSV, khong anh huong runtime (nam trong thu muc Game/Debug da boc define - da co tu truoc).

## Cac muc nen sua (🟡)
1. `ExplosiveBarrel.IsPriorityLive` them `gameObject.activeInHierarchy` (thung o khu bi tat van tap/no duoc).
2. Test `Recoil_StaysWithinLimit_UnderRapidFire` khong bao ve gioi han tran that - them ca test ep tran (vd. `recoilMax` nho / impulse lon) de assert clamp co hieu luc.
3. Phat sinh 2 file moi trong Core (`MuzzleAnchor`, `PriorityShootable`) trong khi Conventions ghi Core dong bang: PM can ghi nhan trong `Conventions.md`/TASK_BOARD (hop dong moi + nguoi duyet) - diff Conventions hien chi them Viewmodel, chua nhac hai file Core nay.

## Scene / console
- Level_01 lap ghep dung: ViewmodelRoot/Camera con Main Camera, tapShooter tro CombatSystems, config dung; khong lap cac component trung. Sandbox `gameplay-coder-combat.unity` chi them `UniversalAdditionalLightData` (Unity tu sinh) - vo hai.
- Console khong loi compile; 252/252 EditMode theo bao cao agent (toi khong chay test).
