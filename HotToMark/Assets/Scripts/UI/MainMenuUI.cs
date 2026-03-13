using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Stage 7: Main menu screen.
    /// Title, subtitle, 4 mode buttons with descriptions.
    /// Orange/black film-industry visual theme.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject menuPanel;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Mode Buttons")]
        [SerializeField] private Button standardButton;
        [SerializeField] private Button speedRunButton;
        [SerializeField] private Button smoothButton;
        [SerializeField] private Button exactMPHButton;

        [Header("Mode Descriptions")]
        [SerializeField] private TextMeshProUGUI standardDesc;
        [SerializeField] private TextMeshProUGUI speedRunDesc;
        [SerializeField] private TextMeshProUGUI smoothDesc;
        [SerializeField] private TextMeshProUGUI exactMPHDesc;

        void Start()
        {
            // Wire up buttons
            if (standardButton != null)
                standardButton.onClick.AddListener(() => StartMode(GameMode.Standard));
            if (speedRunButton != null)
                speedRunButton.onClick.AddListener(() => StartMode(GameMode.SpeedRun));
            if (smoothButton != null)
                smoothButton.onClick.AddListener(() => StartMode(GameMode.SmoothOperator));
            if (exactMPHButton != null)
                exactMPHButton.onClick.AddListener(() => StartMode(GameMode.ExactMPH));

            // Set text content
            if (titleText != null) titleText.text = "HOT TO MARK";
            if (subtitleText != null) subtitleText.text = "The Stunt Driving Game";
            if (standardDesc != null) standardDesc.text = "Drive to the mark, stop accurately, reverse to one.";
            if (speedRunDesc != null) speedRunDesc.text = "Complete the entire take as fast as possible.";
            if (smoothDesc != null) smoothDesc.text = "Minimize jerky inputs. Be cinematic.";
            if (exactMPHDesc != null) exactMPHDesc.text = "Hit exactly the target speed at the checkpoint.";
        }

        private void StartMode(GameMode mode)
        {
            GameManager.Instance.StartGame(mode);
        }

        public void Show()
        {
            if (menuPanel != null) menuPanel.SetActive(true);
        }

        public void Hide()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
        }
    }
}
