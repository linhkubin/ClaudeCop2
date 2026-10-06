using UnityEngine;
using UnityEngine.SceneManagement;
using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>
    /// L-CHAIN-ENTRY: dat trong scene level choi rieng (Level_01..04.unity). Bam Play THANG o scene nay trong Editor (chua qua Title)
    /// -> nap scene chuoi Level_Chain bat dau tu level nay (GameCommands.SelectedLevel = levelIndex): choi xong level nay chay tiep
    /// level ke (bang ket qua, rank rieng moi level, Continue) giong chuoi that. Qua Title (build / Title -> chon level) khong lam gi:
    /// Title da nap thang Level_Chain. Tat redirectToChain de thu rieng mot level (thiet ke/sandbox).
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class StandaloneLevelRedirect : MonoBehaviour
    {
        public const string ChainSceneName = "Level_Chain";
        public const string ChainScenePath = "Assets/_Game/Scenes/Gameplay/Level_Chain.unity";

        [Tooltip("Chi so level cua scene nay trong chuoi (0 = Level 1, 1 = Level 2 ...).")]
        [SerializeField] int levelIndex;
        [Tooltip("Bat: Play thang scene nay -> chay chuoi tu level nay toi het. Tat: chi choi rieng level nay.")]
        [SerializeField] bool redirectToChain = true;

        public int LevelIndex => levelIndex;

        /// <summary>Logic thuan (test): chi chuyen khi bat, chua qua Title, khong dang o chinh scene chuoi, chi so hop le.</summary>
        public static bool ShouldRedirect(bool redirectEnabled, bool titleVisited, string activeSceneName, int levelIndex)
            => redirectEnabled && !titleVisited && levelIndex >= 0 && activeSceneName != ChainSceneName;

        void Awake()
        {
            if (!ShouldRedirect(redirectToChain, GameSession.TitleVisited, gameObject.scene.name, levelIndex)) return;
            // Tat moi doi tuong khac cua scene le truoc khi chung Awake/Start (khong chay 1 frame level rieng).
            foreach (var g in gameObject.scene.GetRootGameObjects())
                if (g != gameObject) g.SetActive(false);
            GameCommands.SelectedLevel = levelIndex;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(ChainScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(ChainSceneName);
#endif
        }
    }
}
