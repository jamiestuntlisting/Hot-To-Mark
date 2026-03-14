using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Pause menu overlay. Self-builds UI hierarchy at runtime.
    /// Shows resume and quit-to-menu buttons.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        private GameObject pausePanel;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            pausePanel = UIFactory.CreatePanel("PausePanel", transform,
                new Color(0, 0, 0, 0.75f));

            // Title
            var title = UIFactory.CreateText("PauseTitle", pausePanel.transform,
                "PAUSED", 42, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(title, new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.75f));

            // Resume button
            var resumeObj = CreatePauseButton("Resume", pausePanel.transform,
                new Vector2(0.25f, 0.42f), new Vector2(0.75f, 0.55f),
                new Color(1f, 0.6f, 0));
            resumeObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            });

            // Quit to menu button
            var quitObj = CreatePauseButton("Quit to Menu", pausePanel.transform,
                new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.38f),
                new Color(0.4f, 0.4f, 0.4f));
            quitObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.QuitToMenu();
            });

            pausePanel.SetActive(false);
        }

        private GameObject CreatePauseButton(string label, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
        {
            var btnObj = UIFactory.CreateImage($"Btn_{label}", parent, bgColor);
            UIFactory.SetAnchors(btnObj, anchorMin, anchorMax);

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            var textObj = UIFactory.CreateText("Label", btnObj.transform,
                label, 22, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(textObj, new Vector2(0, 0), new Vector2(1, 1));

            return btnObj;
        }

        public void Show()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Hide()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }
}
