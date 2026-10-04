using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClaudeCop.UI
{
    /// <summary>View man Title: ten game, nut Start, cong tac "Giam chuyen dong". Chi hien thi + phat su kien.</summary>
    public class TitleView : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button startButton;
        [SerializeField] Toggle reduceMotionToggle;

        public event Action StartClicked;
        public event Action<bool> ReduceMotionToggled;

        public Button StartButton => startButton;
        public Toggle ReduceMotionToggle => reduceMotionToggle;
        public bool ReduceMotionOn => reduceMotionToggle != null && reduceMotionToggle.isOn;

        void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(() => StartClicked?.Invoke());
            if (reduceMotionToggle != null) reduceMotionToggle.onValueChanged.AddListener(v => ReduceMotionToggled?.Invoke(v));
        }

        public void SetReduceMotion(bool on) { if (reduceMotionToggle != null) reduceMotionToggle.SetIsOnWithoutNotify(on); }
        public void Show() { if (content != null) content.SetActive(true); }
        public void Hide() { if (content != null) content.SetActive(false); }
    }
}
