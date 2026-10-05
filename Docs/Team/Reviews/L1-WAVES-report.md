# L1-SEAMLESS / L1-WAVES report (gameplay-coder, 2026-10-05) - DONE, chua commit

## Hien trang ban dau
Level_01.unity da noi dang: 9 wave (P1 W2/W3/W4, P2 W2/W3/W5, P3 W2/W3/W4) gan vao Shot Combat, 12 wave cu da xoa, DoorOpener_Main (L -90, R +90, trigger Wave_P3_W4), Shot_P2_S4 dwell 3.5 s, maxConcurrent. Con thieu: lien mach giua phase (P1_S4 -> P2_S1 cach 8.5 m/78 do, P2_S5 -> P3_S1 cach 40 m), fade den + Cut.

## Thay doi
- CameraFeelProfile.seamlessPhaseTransitions (mac dinh true). PhaseDirector: khi bat, khong BeginPhaseTransition/forceCut; tieu de phase phat qua RailEvents.PhaseBanner luc ray bat dau chay (hoac toi diem Combat dau phase). Tat = kieu cu (fade den).
- CameraShot.lookKeys (LookKey progress/yaw) + RailCameraDriver.SetLookKeys: ray Move giu huong camera luc vao, xoay mem (smoothstep, toi da lookNextMaxYawRate 40) qua cac moc, ket thuc o huong Shot ke. Cho phep ray di lui/di ngang thay vi quay dau 170 do.
- PhaseTransitionPresenter: nghe PhaseBanner (chu, khong lop den); PhaseStarted index>0 khong tu hien banner. Test moi Phase_Banner_Seamless_Shows_Title_Without_Black.
- Scene: Rail_P2_S1 noi dai (24 m) bat dau dung pose Shot_P1_S4 (58,1.65,5.2) -> ngo -> P2_S2. Rail_P3_S1 noi dai (62 m, speed 5) bat dau dung pose P2_S5 (41.2,1.65,-2.2) -> ra ngo -> doc duong x~59 -> vao san P3_S2. Shot marker P2_S1/P3_S1 doi sang pose bat dau moi, entry Blend, blend 0.4. lookKeys: P2_S1 {0.55:270}; P3_S1 {0.12:263.2, 0.42:355, 0.58:355}. Khong doi hinh hoc map (spline la cua gameplay). Do thong thoang >= 1.5 m (sat nhat 1.5 m o xe van).
- DebugSeamlessProbe (moi, debug-only): ghi moi frame pose/xoay/alpha den/enemy ngoai man; chup tieu de phase.
- Docs/Design/Levels_BankHeist.md: 14 -> 15 (P1 4, P2 5, P3 6), mo ta giam tai P2.

## Do (Play, Title->Win bang DebugM2Bot, ReduceMotion=false runtime)
- 15 enemy ha (15/15 hits), 0 sat thuong, Win score 16520. 0 frame enemy targetable ngoai man hinh.
- Alpha den PhaseFade: max 0, 0 frame > 0.
- Nhay vi tri: chi 1 lan luc vao level (Cut dau level, cho phep). Buoc lon nhat con lai 0.34 m/frame o frame treo (dt 0.13-0.2 s).
- Toc do xoay (thoi gian game): Move P2_S1 40.9, P3_S1 40.2 do/s, P1_S1 12.5, P3_S5 5.1 (<= 44); blend Combat toi da 33.8. Khong nhay huong (>150 do/s = 0).
- Ray P2_S1 6.8 s, P3_S1 15 s, khong dung giua doan.
- EditMode 275: 273 pass, 2 Ignored (T621).
- Anh: scratchpad run1/banner_STAGE_1-2.png, banner_STAGE_1-3.png (1440x2960 Game view doc).

## Ton dong
- Cam dung ~0.5 s dau doan noi (blend 0.4 + ease-in), chu hien dung luc do.
- Chua ghi PlayerPrefs; ReduceMotion mac dinh game van true.
- Doan P3_S1 dai 15 s (map yeu cau quay ve phia dong roi len bac); level-designer co the rut ngan bang lối khac.

---
# L1-WIRE (gameplay-coder, 2026-10-05) - DONE, chua commit
- Banner stage: logic cu da phat PhaseBanner 1 lan/Phase (dat khi doi Phase, xa khi ray bat dau chay), nhung them `Core/PhaseBannerGate` (moi Phase index chi TryShow 1 lan; Reset khi StartLevel) lam chot chan trong PhaseDirector. Hoi sinh khong chay lai Run() nen khong hien lai. Test EditMode: `Core.Tests/PhaseBannerGateTests` (2 test). Banner 1.85 s (0.25+1.2+0.4), khong lop den, khong chan input. Tieu de phase giu "STAGE 1-1/1-2/1-3".
- P2 giam tai: phases dung (P2: Move, Combat, Combat, Move(P2_S4 speed 3.3), Combat); doan P2_S4 249 frame lien tuc, dt max 0.029 s, khong dung.
- speedOverride 4.8 (P3_S1, P3_S5): giu co chu y, ghi tooltip `maxRailSpeed` (speedOverride bo qua tran); khong tang maxRailSpeed (se doi toc do cac Move khac).
- Cua chinh: DoorOpener_Main mo (1.2 s) khi Wave_P3_W4 xong; luc Shot_P3_S5_Move bat dau doorOpened=True.
- LevelValidationRules.maxMoveSeconds 5.8 -> 6.0 (asset + default); CamPoint_P3_S5_End doi thanh CamPoint_P3_S6 (trong Level_01.prefab, khong co tham chieu code/scene khac). Validate Level: PASS 0 FAIL 0 WARN.
- Bot Title->Win (ReduceMotion=false runtime): 15/15 ha, 0 sat thuong, 0 enemy targetable ngoai man, toi da 2 cung luc, Win score 17308, 1 luot ~54.3 s game time. Banner: PhaseStarted 0 (stage 1) + PhaseBanner 1-2 (t14.5, Move P2_S1) + 1-3 (t33.5, Move P3_S1) = 3 lan. PhaseFade alpha max 0. Nhay vi tri: chi Cut luc vao level. Xoay Move max 41.6 deg/s (P3_S1), khong nhay huong (>150 = 0).
- EditMode 284: 282 pass, 2 Ignored (T621). Anh: scratchpad/run2/banner_STAGE_1-2.png, banner_STAGE_1-3.png (banner stage 1 khong chup).
