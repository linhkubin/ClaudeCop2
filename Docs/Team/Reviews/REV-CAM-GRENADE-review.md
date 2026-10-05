# REV-CAM-GRENADE — Review (full, chi doc)

Ket luan: **CHANGES REQUESTED**. Console: khong co loi compile/runtime cua game (chi 1 loi "NoSubscription" cua Unity AI generators, khong lien quan).
Phan tich tinh (khong Play). Gia tri lay tu `Assets/_Game/Settings/CameraFeelProfile.asset` va `Level_01.unity` (shot fov 58-60, rail baseFov 50).

## A1. Camera toi diem den roi "zoom out"  (nguyen nhan xep theo kha nang)

1. 🔴 **Combat shot rong hon + lui xa hon camera ray luc toi noi** — `CameraShot.cs:88-128` (AutoFrame), `PhaseDirector.cs:92`, `CameraFeelApplier.cs:25-26,155-163`.
   - Rail camera luon FOV 50 (`RailCameraDriver.cs:70`, baseFov=50). Moi Combat shot trong Level_01 co fov 58-60, nhung bi kep `maxVerticalFov=55` (AutoFrame `:126` hoac FitFov) => blend Move->Combat di tu 50 len 55 = **zoom out +5 do** ngay luc toi noi.
   - AutoFrame cong `extra = dollyInFov (8)` vao FOV can (`:117-122`) nen hau het shot (man doc) vuot 55 va bi **lui ra sau toi 8 m** (`frameMaxPullBack=8`) — `transform.SetPositionAndRotation(pos + back*dist)` (`:125`). Spline Move ket thuc tai vi tri cu cua shot, shot lai nam sau do 0-8 m => camera toi diem roi blend NGUOC ra sau (1-2.5 s do `ScaleBlend`). Dung la "toi noi roi lui/zoom out".
   - Dolly-in (-8 do trong 12 s) chi bat dau tu 0 sau do (`PhaseDirector.cs:254`, `CameraFeelState.cs:26`), nen cam giac: toi -> rong ra -> chi tu tu hep lai.
   - Huong sua (loi): dat FOV khoi dau cua Combat shot <= FOV cuoi cua rail (hoac nang baseFov rail len bang FOV Combat da fit); khong cong `extra` dolly vao nhu cau FOV, thay vao do chap nhan dolly-in bat dau o FOV da fit (gia tri `fit + dolly` -> `fit` luc dau, hep xuong sau khong rong ra); bo/giam pull-back (dua pull-back vao vi tri ket thuc spline: de Move dung den dung diem da lui, khong blend ngược).
2. 🔴 **Mat dolly-in/handheld khi roi Combat** — `CameraFeelState.cs:24-25` + `PhaseDirector.cs:224` (`Combat=false` truoc Activate). CombatWeight giam 1->0 trong `modeFade=0.4s` (tuyen tinh) trong khi camera cu van live suot blend (0.5-2.5 s): FOV dolly-in (toi -8 do), handheld va `trackYaw` bi go bo trong 0.4 s tren camera dang roi di => zoom-out + xoay lai ro ret o dau Move.
3. 🔴 **`ResetDolly()` toan cuc lam FOV camera cu nhay** — `PhaseDirector.cs:254`. Combat->Combat: `DollyProgress` la static dung chung moi camera; reset ve 0 luc bat dau blend lam camera cu (dang live) **nhay +x do FOV tuc thi** (x = 8*smoothstep(progress)). Dung cho "giat". Huong sua: dolly theo tung camera (luu thoi diem bat dau trong Applier/Shot, khong static), hoac chi reset sau khi blend xong.
4. 🟡 Punch (5 do, tra 0.4 s) + KillZoom (6 do, 0.5 s) chay chong nhau tren enemy cuoi (`PhaseDirector.cs:129,303`; `CameraFeelApplier.cs:76-93`): FOV lom roi tha ra roi lai thu vao => nhap nho nguoc. Va KillZoom giu (weight=1) qua restAfterClear roi blend sang shot moi o FOV chua tra => cam giac "zoom in roi bi keo ra".
5. 🟢 Blend Move->Combat moi dung `EaseInOut` ca hai dau; dung ben ngoai khong thay "settle". (xem A3)

## A2. Giat sang khung khac roi giat ve

1. 🔴 **TrackTargets xoay toi 25 do roi nha ve ngay khi doi mode** — `CameraFeelApplier.cs:103-148`. Man doc half-FOV ngang chi ~14-15 do, `frameMargin=5` => `lim ~ 10 do`: enemy lo ra o bat ky cho nao lech >10 do deu keo camera. Goc xoay `trackYaw` (toi `trackMaxYaw=25`) duoc nhan `cw`; khi ket thuc giao tranh cw 1->0 trong 0.4 s => ~25 do xoay lai trong 0.4 s (60 do/s) = giat ve. Huong sua: giu trackYaw (khong nhan cw) cho den khi camera moi lam chu, hoac nha yaw ve 0 bang cung duong cong voi blend, tang `modeFade`/dung SmoothDamp thoi gian dai khi thoat.
2. 🔴 **Grenade/enemy bay gan camera lam muc tieu "nhay"** — `CameraFeelApplier.cs:124-133`: grenade `ShowsReticle=true`, `AimPoint` bay sat camera (diem dap 1.5 m truoc camera, `EnemyConfig grenadeLandDistance=1.5`). Goc ngang `atan2(lateral, z)` tang rat nhanh khi z nho; ngay khi grenade no/bi ban (Unregister) muc tieu bien mat => `want` doi dot ngot. Ket qua: lia theo luu dan roi tra lai. Huong sua: bo Kind==Grenade (va muc tieu z < ~3 m) ra khoi TrackTargets.
3. 🟡 Lim phu thuoc FOV sau punch/dolly/kill (`CameraFeelApplier.cs:120`, tinh o cuoi Noise stage): moi lan punch (-5 do) lim co lai => cung cap nhat `want` => camera co "giat nhe" theo moi cu ha enemy. Nen tinh lim tu FOV chua cong zoom tam thoi.
4. 🟡 Move nối Move (cung `railCamera`) khong cut: `PhaseDirector.cs:227-239` — `Prepare()` dat lai transform/spline cua chinh camera dang live khi `active == railCamera` (blend tu chinh no) => teleport. Level_01 co cac Move cach nhau bang Combat nen it xay ra, nhung Phase moi dung forceCut. Nen ep Cut khi `active == railCamera`.
5. 🟡 AutoFrame chi tinh 1 lan trong `Init()` (`PhaseDirector.cs:86-93`) theo `outCam.aspect` luc Awake. Doi kich thuoc cua so Game / xoay may => khung hu (shot lui ra/FOV theo ti le cu). Ngoai ra SphereCast luc Awake dung `~0` (ke ca enemy/props dong) => lui ra bi cut ngan khong on dinh giua cac lan chay.
6. 🟢 Thu tu update on dinh: Driver ghi transform o `Update` (`PhaseDirector.cs:153`), Brain `LateUpdate` (da ep UpdateMethod). Khong thay hai he thong ghi FOV cung luc (chi Applier). Spline dolly: `dolly.CameraPosition` va driver cung ghi position nhung nhat quan.

## A3. Hien trang & de xuat nhip camera (chua code)

Dang co: ease-in/out ray (1 s moi dau), look-ahead + roll theo toc do xoay, bob buoc chan, handheld Perlin (Combat), dolly-in FOV 8 do/12 s, punch khi kill, kill-zoom + aim 0.5 s, hit/explosion shake, track yaw theo muc tieu, sub-angle, blend EaseInOut keo dai theo toc do, auto-frame theo man.
Thieu: (a) khong co "settle" khi toi (camera dung hang roi de dolly 12 s qua cham, 8 do quá it); (b) khong co push-in rieng cho Justice shot/combo; (c) khong co "tha" co kiem soat (ease-out FOV) khi roi Combat — chi bi ngat boi cw; (d) khong co sway nhe luc Move dung/idle; (e) FOV khong lien tuc giua Move va Combat (xem A1).
De xuat nhip: **Toi** (rail ease-out, FOV Move = FOV Combat luc dau) -> **Settle** 0.4-0.6 s (drift nhe 1-1.5 do + giam nhe FOV ~1.5 do, ease-out) -> **Giu khung** (handheld nho, khong zoom out; track chi trong dead-zone rong) -> **Push-in** 2-3 do/0.25 s khi Justice shot hoac combo moc (tra ve cham 0.8 s, nhung khong vuot FOV cuoi) -> **Cleared**: kill-zoom giu, **ease ra** bang cung duong cong voi blend sang Move (khong reset dolly/handheld dot ngot; cw giam theo thoi gian blend). Quy tac: FOV chi giam (zoom-in) trong mot shot; moi tang FOV chi xay ra qua blend co chu dich.

## B. Thuoc no / Grenade (`Enemy/Grenade.cs`, `GrenadeFlight.cs`, `EnemyActor.cs:283-293`, `EncounterWave.cs`, `Props/ExplosiveBarrel.cs`)

Luong: EnemyActor.OnFired -> ThrowGrenade (Instantiate, Launch toi `camPos + camFwd*1.5`) -> `GrenadeThrown` -> EncounterWave dang ky `Resolved` -> Tap: `OnTapHit` Kill -> Finish(true) / het gio: Finish(false) + `PlayerDamageService.Damage(Explosion)`. Pause + dang ky/huy TargetRegistry, huy `Resolved` trong OnDestroy/BuildQueue deu dung. Khong thay loi null/leak nghiem trong.

1. 🟡 `EnemyActor.cs:287-289`: diem dap tinh theo `Camera.main` luc nem. Neu nem trong luc blend/cut/track-yaw xoay camera, luu dan co the dap vao khoang khong cach xa huong nhin (khong thanh "trung nguoi" ro). Nen tinh lai diem dap moi frame (hoac lerp theo Camera.main) o `Tick`.
2. 🟡 Tuong tac TrackTargets (A2.2): camera lia theo luu dan roi quay lai; nen loai Grenade khoi TrackTargets.
3. 🟡 Grenade ban ha co `TapOutcome.Kill` => `TapShooter.cs:276` tinh vao combo (co the muon) va `PhaseDirector.OnShotResolved:129` cho SlowZoom.Punch tren moi Kill ke ca Grenade (khong kiem `TargetKind`); `ScoreSystem` nghe moi Kill => grenade duoc cong diem nhu enemy neu khong loc theo TargetKind (xac nhan voi game-designer co chu y khong). Neu khong: loc theo `r.TargetKind`.
4. 🟡 Grenade khong phat `BlastEvents`/FX he thong: no vao nguoi choi chi co hit-shake (qua PlayerDamaged), khong co explosionShake/FX pool; chi co `explosionFx` scale tay (khong pool, `Destroy(gameObject)` sau no). Luu dan duoc `Instantiate` moi lan nem, khong pre-warm => hitch tien an (ngược voi chu thich "pre-spawn tranh hitch ~100 ms" trong EncounterWave).
5. 🟡 `Grenade.cs:101`: `fxTimer/dur` — `grenadeExplosionTime` co `[Min(0.01)]` nen ok, nhung cfg null-fallback sau Launch. Lua chon an toan, khong loi.
6. 🟡 Thung no no keo theo Grenade trong ban kinh (`ExplosiveBarrel.cs:89`): `OnTapHit` tra Kill nhung Barrel khong dem vao `enemies`; khong cong combo/diem — chap nhan duoc, nhung luu dan bi no chua phat ShotResolved nen khong co FX/diem; can thong nhat voi game-designer.
7. 🟢 Enemy ra man hinh: grenade luon bay toi `camPos + fwd*1.5`; khong co dieu kien huy khi enemy bi ha giua luc bay (dung — luu dan van tiep tuc; wave khong Cleared cho den khi giai quyet, `EncounterWave.cs:356`). Khi Pause (Game Over/Revive) luu dan dung dung (Tick chan boi `IsPaused`), nhung `UpdateFx` (`:58`) van chay.
8. 🟢 `Abort()` o OnDisable goi `Resolved` khi scene unload/disable: co the goi vao EncounterWave da bi Destroy (da loc `g != null` + huy dang ky o `OnDestroy`), an toan.

## Viec de xuat giao (khong sua tai day)
- gameplay-coder (Camera): A1.1-3, A2.1-2, A2.4, A2.5, A3 thiet ke nhip.
- enemy-coder: B.1, B.4 (pool/pre-warm), B.2 (loai Grenade khoi tracking, phoi hop gameplay-coder).
- combat-coder/game-designer: B.3, B.6 (diem/combo/Punch cho Grenade).
