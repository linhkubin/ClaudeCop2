# TAP-SHIELD-CENTER

Nguyen nhan: TargetSelector chon enemy khi tap trong HitRadiusPx cua vu khi (90px), nhung HumanShieldEnemy.OnTapHit phan loai bang shieldHeadRadiusPx (45px) -> tap 45..90px quanh tam bi tinh HostageHit.

Sua:
- EnemyConfig: them `shieldCenterTapRadius` (px chuan, mac dinh 90, Tooltip).
- HumanShieldEnemy.OnTapHit: ban kinh = max(shieldHeadRadiusPx, shieldCenterTapRadius).
- Khong doi: Justice, thung no, TargetSelector, HostageActor (con tin dung rieng), tap ngoai vung tam van HostageHit.
- Test moi (HumanShieldTests): tap 80px -> Kill; tap 100px -> HostageHit. Enemy+Combat EditMode: 85/85 pass.
- Luu y: asset EnemyConfig_Default tu nhan gia tri 90 cho field moi (chua serialize); khong sua scene.
