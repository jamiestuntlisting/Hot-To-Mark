using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Main menu screen — simple title + single Play button for career mode.
    /// Keyboard: Enter or Space to start.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private GameObject menuPanel;
        private Button playButton;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // Full-screen panel
            menuPanel = UIFactory.CreatePanel("MenuPanel", transform,
                new Color(0.03f, 0.03f, 0.03f, 0.94f));

            // Decorative top bar (orange accent)
            var topBar = UIFactory.CreateImage("TopBar", menuPanel.transform,
                new Color(1f, 0.6f, 0, 0.9f));
            UIFactory.SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 8));

            // Title — 3x original (48 -> 144)
            var title = UIFactory.CreateText("Title", menuPanel.transform,
                "HOT TO MARK", 144, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(title, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.88f));

            // Subtitle — 3x original (16 -> 48)
            var subtitle = UIFactory.CreateText("Subtitle", menuPanel.transform,
                "The Stunt Driving Game", 48, new Color(0.7f, 0.7f, 0.7f),
                FontStyles.Italic, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(subtitle, new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.55f));

            // Divider line
            var divider = UIFactory.CreateImage("Divider", menuPanel.transform,
                new Color(1f, 0.6f, 0, 0.3f));
            UIFactory.SetAnchors(divider, new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.385f));

            // Play button — big and centered
            var playBtnObj = UIFactory.CreateImage("PlayBtn", menuPanel.transform,
                new Color(1f, 0.6f, 0));
            UIFactory.SetAnchors(playBtnObj, new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.34f));

            playButton = playBtnObj.AddComponent<Button>();
            var colors = playButton.colors;
            colors.normalColor = new Color(1f, 0.6f, 0);
            colors.highlightedColor = new Color(1f, 0.7f, 0.2f);
            colors.pressedColor = new Color(0.8f, 0.5f, 0);
            colors.selectedColor = new Color(1f, 0.65f, 0.1f);
            playButton.colors = colors;

            playButton.onClick.AddListener(OnPlay);

            // Button label — 3x original (14 -> 42)
            var playLabel = UIFactory.CreateText("PlayLabel", playBtnObj.transform,
                "PLAY", 54, new Color(0.05f, 0.05f, 0.05f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(playLabel, new Vector2(0, 0), new Vector2(1, 1));

            // Footer — 3x original (9 -> 27)
            var footer = UIFactory.CreateText("Footer", menuPanel.transform,
                "Press ENTER or tap to play", 27, new Color(0.45f, 0.45f, 0.45f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(footer, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.13f));
        }

        void Update()
        {
            if (menuPanel == null || !menuPanel.activeSelf) return;

            // Keyboard support: Enter or Space to play
            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame
                || kb.numpadEnterKey.wasPressedThisFrame
                || kb.spaceKey.wasPressedThisFrame))
            {
                OnPlay();
            }
        }

        private void OnPlay()
        {
            Hide();
            if (GameManager.Instance != null && GameManager.Instance.careerMenu != null)
                GameManager.Instance.careerMenu.Show();
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
