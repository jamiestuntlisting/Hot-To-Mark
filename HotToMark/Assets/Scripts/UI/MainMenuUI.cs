using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.Vehicle;
using HotToMark.Scoring;

namespace HotToMark.UI
{
    /// <summary>
    /// Stage 7: Main menu screen.
    /// Self-builds its own UI hierarchy at runtime — no serialized field wiring needed.
    /// Orange/black film-industry visual theme.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private GameObject menuPanel;
        private GameMode lastSelectedMode = GameMode.Standard;
        private VehicleType selectedVehicle = VehicleType.Sedan;
        private TextMeshProUGUI vehicleLabelText;
        private Image[] vehicleButtonBgs = new Image[3];

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
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 6));

            // Title
            var title = UIFactory.CreateText("Title", menuPanel.transform,
                "HOT TO MARK", 48, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(title, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.92f));

            // Subtitle
            var subtitle = UIFactory.CreateText("Subtitle", menuPanel.transform,
                "The Stunt Driving Game", 16, new Color(0.7f, 0.7f, 0.7f),
                FontStyles.Italic, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(subtitle, new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.78f));

            // Divider line
            var divider = UIFactory.CreateImage("Divider", menuPanel.transform,
                new Color(1f, 0.6f, 0, 0.3f));
            UIFactory.SetAnchors(divider, new Vector2(0.15f, 0.70f), new Vector2(0.85f, 0.705f));

            // ---- Vehicle Selector (F-1) ----
            var vehicleLabel = UIFactory.CreateText("VehicleLabel", menuPanel.transform,
                "SELECT VEHICLE", 13, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(vehicleLabel, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.695f));

            CreateVehicleButton("Sedan", VehicleType.Sedan, 0, menuPanel.transform);
            CreateVehicleButton("Muscle", VehicleType.MuscleCar, 1, menuPanel.transform);
            CreateVehicleButton("SUV", VehicleType.SUV, 2, menuPanel.transform);

            vehicleLabelText = UIFactory.CreateText("VehicleDesc", menuPanel.transform,
                "Balanced handling. The industry standard.", 10,
                new Color(0.5f, 0.5f, 0.5f), FontStyles.Italic, TextAlignmentOptions.Center)
                .GetComponent<TextMeshProUGUI>();
            UIFactory.SetAnchors(vehicleLabelText.gameObject,
                new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.62f));

            UpdateVehicleHighlight();

            // Mode buttons
            CreateModeButton("Standard Take",
                "Drive to the mark, stop accurately, reverse to one.",
                GameMode.Standard, 0.46f, menuPanel.transform);

            CreateModeButton("Speed Run",
                "Complete the entire take as fast as possible.",
                GameMode.SpeedRun, 0.35f, menuPanel.transform);

            CreateModeButton("Smooth Operator",
                "Minimize jerky inputs. Be cinematic.",
                GameMode.SmoothOperator, 0.24f, menuPanel.transform);

            CreateModeButton("Exact MPH",
                "Hit exactly the target speed at the checkpoint.",
                GameMode.ExactMPH, 0.13f, menuPanel.transform);

            // Footer
            var footer = UIFactory.CreateText("Footer", menuPanel.transform,
                "Tap a mode to begin your take", 12, new Color(0.45f, 0.45f, 0.45f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(footer, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.07f));
        }

        private void CreateModeButton(string label, string desc, GameMode mode,
            float yCenter, Transform parent)
        {
            float btnHeight = 0.09f;
            float descHeight = 0.03f;

            // Button background
            var btnObj = UIFactory.CreateImage($"Btn_{label}", parent,
                new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetAnchors(btnObj, new Vector2(0.1f, yCenter),
                new Vector2(0.9f, yCenter + btnHeight));

            // Orange left accent
            var accent = UIFactory.CreateImage("Accent", btnObj.transform,
                new Color(1f, 0.6f, 0));
            UIFactory.SetAnchors(accent, new Vector2(0, 0), new Vector2(0.01f, 1));

            // Button label
            var labelObj = UIFactory.CreateText("Label", btnObj.transform,
                label, 20, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(labelObj, new Vector2(0.02f, 0), new Vector2(0.98f, 1));

            // Make clickable
            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.1f, 0.1f, 0.1f);
            colors.highlightedColor = new Color(0.2f, 0.15f, 0.05f);
            colors.pressedColor = new Color(1f, 0.6f, 0);
            colors.selectedColor = new Color(0.15f, 0.12f, 0.05f);
            btn.colors = colors;

            var capturedMode = mode;
            btn.onClick.AddListener(() => {
                lastSelectedMode = capturedMode;
                GameManager.Instance.StartGame(capturedMode);
            });

            // High score badge on right side of button
            int highScore = HighScoreManager.GetHighScore(mode);
            if (highScore > 0)
            {
                string grade = HighScoreManager.GetHighGrade(mode);
                var hsObj = UIFactory.CreateText("HighScore", btnObj.transform,
                    $"{grade}  {highScore}", 12, new Color(1f, 0.85f, 0.2f, 0.7f),
                    FontStyles.Bold, TextAlignmentOptions.Right);
                UIFactory.SetAnchors(hsObj, new Vector2(0.6f, 0), new Vector2(0.96f, 1));
            }

            // Description below button
            var descObj = UIFactory.CreateText($"Desc_{label}", parent,
                desc, 11, new Color(0.5f, 0.5f, 0.5f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(descObj,
                new Vector2(0.1f, yCenter - descHeight),
                new Vector2(0.9f, yCenter));
        }

        private void CreateVehicleButton(string label, VehicleType type, int index,
            Transform parent)
        {
            float xStart = 0.1f + index * 0.27f;
            float xEnd = xStart + 0.25f;

            var btnObj = UIFactory.CreateImage($"Vehicle_{label}", parent,
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetAnchors(btnObj, new Vector2(xStart, 0.62f),
                new Vector2(xEnd, 0.65f));

            vehicleButtonBgs[index] = btnObj.GetComponent<Image>();

            var labelObj = UIFactory.CreateText("Label", btnObj.transform,
                label, 14, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(labelObj, new Vector2(0, 0), new Vector2(1, 1));

            var btn = btnObj.AddComponent<Button>();
            var capturedType = type;
            var capturedIdx = index;
            btn.onClick.AddListener(() => {
                selectedVehicle = capturedType;
                if (GameManager.Instance != null)
                    GameManager.Instance.state.selectedVehicle = capturedType;
                UpdateVehicleHighlight();
            });
        }

        private void UpdateVehicleHighlight()
        {
            Color active = new Color(1f, 0.6f, 0, 0.7f);
            Color inactive = new Color(0.08f, 0.08f, 0.08f);

            for (int i = 0; i < vehicleButtonBgs.Length; i++)
            {
                if (vehicleButtonBgs[i] == null) continue;
                vehicleButtonBgs[i].color = (i == (int)selectedVehicle) ? active : inactive;
            }

            var configs = VehicleConfig.GetDefaults();
            int idx = (int)selectedVehicle;
            if (idx < configs.Length && vehicleLabelText != null)
                vehicleLabelText.text = configs[idx].description;
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
