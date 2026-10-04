using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClaudeCop.Core;

namespace ClaudeCop.UI
{
    /// <summary>Man thua. Nut Restart gui GameCommands.RequestRestart.</summary>
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] Button restartButton;

        void Awake()
        {
            if (restartButton != null) restartButton.onClick.AddListener(GameCommands.RequestRestart);
            Hide();
        }

        public void Show(int score)
        {
            if (scoreText != null) scoreText.text = "SCORE  " + score.ToString("N0");
            if (content != null) content.SetActive(true);
        }

        public void Hide() { if (content != null) content.SetActive(false); }
        public bool IsVisible => content != null && content.activeSelf;
    }
}
