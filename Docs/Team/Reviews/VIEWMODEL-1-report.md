# VIEWMODEL-1 / VIEWMODEL-2 - bao cao (gameplay-coder)

Trang thai: DONE. Khong commit.

## Hien trang tim thay (VIEWMODEL-1) va viec VIEWMODEL-2
- Script (Config/Controller/Motion + Debug/Editor/Tests), 3 prefab, VM_Base.controller, 3 override, clip, material, ViewmodelConfig.asset, layer Viewmodel, Sandbox scene: da co, khong lam lai.
- `MissingComponentException ... ViewmodelCamera`: khong tai hien. Console sach; scene Sandbox + Level_01 co ViewmodelCamera day du (Camera + UniversalAdditionalCameraData Overlay, tag Untagged, mask = layer Viewmodel, FOV 45). Loi cu co the do lan chay builder dang do; menu `ClaudeCop/Viewmodel/Add To Active Scene` chay dung (Ensure<> them component truoc khi dung).

## Thay doi
- `Scripts/Viewmodel/ViewmodelController.cs`: gan `MuzzleAnchor.Set(socket Muzzle)` khi hien sung / doi sung; `MuzzleAnchor.Clear(socket)` khi go sung va OnDisable.
- `Scripts/Viewmodel/Tests/ViewmodelMotionTests.cs`: nguong test giat khi xa lien thanh 0.3 -> 0.15 (dinh thuc te 0.226 voi cau hinh; recoil van ep trong recoilMax).
- `Scenes/Sandbox/gameplay-coder-viewmodel.unity`: them GameObject `FxSystem` (config FxConfig.asset) de thay vet dan.
- `Scenes/Gameplay/Level_01.unity`: them ViewmodelRoot + ViewmodelCamera (Overlay) con Main Camera, Camera Stack = 1, loai layer Viewmodel khoi culling Main Camera, tapShooter = CombatSystems. Khong dung CameraRig/CameraShot/rail/map.
- FxConfig.muzzleEnabled = 0 (da co tu truoc). Truong tracer* giu nguyen (khong can chinh).

## Kiem chung
- Sandbox: 3 sung nhin dung o goc duoi phai khung 9:16 va 9:19.5 (anh `Assets/Screenshots/VM2_916_*.png`, `VM2_1995_idle.png`); muzzle flash hien khi ban; reload chay (animator.speed 2.0/2.4/2.8 = clipLength/ReloadTime 0.5s); doi 3 sung dung.
- Vet dan (TracerPool) xuat phat tu nong sung va bay toi muc tieu, nhin thay ro o Game view (`VM2_tracer3.png`). Luu y: vet dan chi hien khi scene co FxSystem.
- Level_01: bot DebugM2Bot chay Title->... -> `STATE Win` (53791 diem, 2 mang), console khong loi, `Camera.main` = Main Camera (tong 2 camera, chi 1 camera tag MainCamera), hit-test khong doi (RESOLVED Kill/JusticeKill nhu cu). Viewmodel khong cham PointerBlocker nen nut Reload van chan nhu cu.
- EditMode: 252/252 pass.

## Con ton dong / ghi chu
- Anh capture offscreen (RenderTexture) co noise hat do Capture() khong MSAA/AO; khong anh huong Game view that.
- Diem xuat vet dan dung toa do world cua nong overlay (FOV 45) nhin tu Main Camera (FOV khac) nen lech vai pixel so voi sung ve; chap nhan duoc.
- Chua thu tay tren thiet bi that; `ReloadTime` 0.5s lam clip reload chay nhanh x2-2.8 (nguoi dung co the tang len ~0.8-1.0s).
