using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Pause menu overlay.
    /// Keyboard: Enter/Space to resume, Escape to quit to menu.
    /// Text sizes 3x for readability.
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

            // Title — 3x (42 -> 126)
            var title = UIFactory.CreateText("PauseTitle", pausePanel.transform,
                "PAUSED", 126, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(title, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.82f));

            // Resume button — 3x (22 -> 66)
            var resumeObj = CreatePauseButton("Resume", pausePanel.transform,
                new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.55f),
                new Color(1f, 0.6f, 0));
            resumeObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            });

            // Quit to menu button — 3x (22 -> 66)
            var quitObj = CreatePauseButton("Quit to Menu", pausePanel.transform,
                new Vector2(0.2f, 0.18f), new Vector2(0.8f, 0.35f),
                new Color(0.4f, 0.4f, 0.4f));
            quitObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.QuitToMenu();
            });

            // Hint text
            var hint = UIFactory.CreateText("Hint", pausePanel.transform,
                "ENTER to resume, ESC to quit", 30, new Color(0.45f, 0.45f, 0.45f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(hint, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.16f));

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
                label, 66, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(textObj, new Vector2(0, 0), new Vector2(1, 1));

            return btnObj;
        }

        void Update()
        {
            if (pausePanel == null || !pausePanel.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Space))
            {
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.Instance != null) GameManager.Instance.QuitToMenu();
            }
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
