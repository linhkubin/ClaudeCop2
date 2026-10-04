using UnityEngine;

namespace ClaudeCop.Game
{
    /// <summary>So lieu game: mang, diem, bat tu, FPS, revive. Gia tri mac dinh theo TASK_BOARD "So lieu".</summary>
    [CreateAssetMenu(menuName = "ClaudeCop/Game/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField, Min(1)] int maxLives = 3;
        [Tooltip("Giay bat tu sau khi trung dan. 1.5 s (T-304, truoc la 0.5 s): choi thu T-203 cho thay 3 enemy cua wave 2 tru het 3 tim trong ~1.4 s; 1.5 s du de nguoi choi phan ung giua 2 phat.")]
        [SerializeField, Min(0f)] float invulnerableSeconds = 1.5f;
        [Tooltip("Giay bat tu VA tam dung chien dau (CombatPauseSignal 'ReviveGrace') ngay sau khi revive.")]
        [SerializeField, Min(0f)] float reviveGraceSeconds = 1.0f;
        [SerializeField, Min(0)] int killPoints = 100;
        [Tooltip("Thuong sam toi da, tuyen tinh theo (1 - ReticleProgress).")]
        [SerializeField, Min(0)] int earlyBonusMax = 100;
        [SerializeField, Min(0)] int justicePoints = 300;
        [SerializeField, Min(1f)] float justiceGreenMultiplier = 1.5f;
        [Tooltip("Vong con xanh khi progress < nguong nay.")]
        [SerializeField, Range(0f, 1f)] float greenThreshold = 0.4f;
        [Tooltip("M1 = false (multiplier = 1). M2 bat de ap combo.")]
        [SerializeField] bool applyComboMultiplier = true;
        [SerializeField, Min(1)] int targetFrameRate = 60;
        [SerializeField, Min(0)] int revivesPerRun = 1;
        [Tooltip("true = vao Playing ngay khi scene khoi dong (sandbox/debug). false = cho Title (StartGameRequested) hoac Restart; trong Editor, chay thang scene gameplay khong qua Title van tu bat dau.")]
        [SerializeField] bool startImmediately = true;

        public int MaxLives => maxLives;
        public float InvulnerableSeconds => invulnerableSeconds;
        public float ReviveGraceSeconds => reviveGraceSeconds;
        public int KillPoints => killPoints;
        public int EarlyBonusMax => earlyBonusMax;
        public int JusticePoints => justicePoints;
        public float JusticeGreenMultiplier => justiceGreenMultiplier;
        public float GreenThreshold => greenThreshold;
        public bool ApplyComboMultiplier => applyComboMultiplier;
        public int TargetFrameRate => targetFrameRate;
        public int RevivesPerRun => revivesPerRun;
        public bool StartImmediately => startImmediately;
    }
}
