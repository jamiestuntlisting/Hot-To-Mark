using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.Vehicle;
using HotToMark.Environment;
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

        private SetType selectedSet = SetType.Backlot;
        private TextMeshProUGUI setLabelText;
        private Image[] setButtonBgs = new Image[4];

        private WeatherCondition selectedWeather = WeatherCondition.Clear;
        private Image[] weatherButtonBgs = new Image[5];

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

            // ---- Set/Location Selector (F-2) ----
            var setLabel = UIFactory.CreateText("SetLabel", menuPanel.transform,
                "SELECT LOCATION", 13, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(setLabel, new Vector2(0.1f, 0.545f), new Vector2(0.9f, 0.575f));

            CreateSetButton("Backlot", SetType.Backlot, 0, menuPanel.transform);
            CreateSetButton("Desert", SetType.Desert, 1, menuPanel.transform);
            CreateSetButton("City", SetType.City, 2, menuPanel.transform);
            CreateSetButton("Garage", SetType.ParkingStructure, 3, menuPanel.transform);

            setLabelText = UIFactory.CreateText("SetDesc", menuPanel.transform,
                "Classic film lot. Green grass, blue sky.", 10,
                new Color(0.5f, 0.5f, 0.5f), FontStyles.Italic, TextAlignmentOptions.Center)
                .GetComponent<TextMeshProUGUI>();
            UIFactory.SetAnchors(setLabelText.gameObject,
                new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.52f));

            UpdateSetHighlight();

            // ---- Weather Selector (F-7) ----
            var weatherLabel = UIFactory.CreateText("WeatherLabel", menuPanel.transform,
                "CONDITIONS", 11, new Color(0.7f, 0.7f, 0.7f),
                FontStyles.Bold, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(weatherLabel, new Vector2(0.1f, 0.445f), new Vector2(0.9f, 0.47f));

            CreateWeatherButton("Clear", WeatherCondition.Clear, 0, menuPanel.transform);
            CreateWeatherButton("Overcast", WeatherCondition.Overcast, 1, menuPanel.transform);
            CreateWeatherButton("Rain", WeatherCondition.Rain, 2, menuPanel.transform);
            CreateWeatherButton("Golden", WeatherCondition.GoldenHour, 3, menuPanel.transform);
            CreateWeatherButton("Night", WeatherCondition.Night, 4, menuPanel.transform);

            UpdateWeatherHighlight();

            // Mode buttons
            CreateModeButton("Standard Take",
                "Drive to the mark, stop accurately, reverse to one.",
                GameMode.Standard, 0.38f, menuPanel.transform);

            CreateModeButton("Speed Run",
                "Complete the entire take as fast as possible.",
                GameMode.SpeedRun, 0.29f, menuPanel.transform);

            CreateModeButton("Smooth Operator",
                "Minimize jerky inputs. Be cinematic.",
                GameMode.SmoothOperator, 0.20f, menuPanel.transform);

            CreateModeButton("Exact MPH",
                "Hit exactly the target speed at the checkpoint.",
                GameMode.ExactMPH, 0.11f, menuPanel.transform);

            // Footer
            var footer = UIFactory.CreateText("Footer", menuPanel.transform,
                "Tap a mode to begin your take", 11, new Color(0.45f, 0.45f, 0.45f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(footer, new Vector2(0.1f, 0.01f), new Vector2(0.9f, 0.05f));
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

        private void CreateSetButton(string label, SetType type, int index,
            Transform parent)
        {
            float xStart = 0.07f + index * 0.22f;
            float xEnd = xStart + 0.2f;

            var btnObj = UIFactory.CreateImage($"Set_{label}", parent,
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetAnchors(btnObj, new Vector2(xStart, 0.52f),
                new Vector2(xEnd, 0.545f));

            setButtonBgs[index] = btnObj.GetComponent<Image>();

            var labelObj = UIFactory.CreateText("Label", btnObj.transform,
                label, 12, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(labelObj, new Vector2(0, 0), new Vector2(1, 1));

            var btn = btnObj.AddComponent<Button>();
            var capturedType = type;
            btn.onClick.AddListener(() => {
                selectedSet = capturedType;
                if (GameManager.Instance != null)
                    GameManager.Instance.state.selectedSet = capturedType;
                UpdateSetHighlight();
            });
        }

        private void CreateWeatherButton(string label, WeatherCondition weather, int index,
            Transform parent)
        {
            float xStart = 0.05f + index * 0.185f;
            float xEnd = xStart + 0.17f;

            var btnObj = UIFactory.CreateImage($"Weather_{label}", parent,
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetAnchors(btnObj, new Vector2(xStart, 0.42f),
                new Vector2(xEnd, 0.445f));

            weatherButtonBgs[index] = btnObj.GetComponent<Image>();

            var labelObj = UIFactory.CreateText("Label", btnObj.transform,
                label, 10, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(labelObj, new Vector2(0, 0), new Vector2(1, 1));

            var btn = btnObj.AddComponent<Button>();
            var capturedWeather = weather;
            btn.onClick.AddListener(() => {
                selectedWeather = capturedWeather;
                if (GameManager.Instance != null)
                    GameManager.Instance.state.selectedWeather = capturedWeather;
                UpdateWeatherHighlight();
            });
        }

        private void UpdateWeatherHighlight()
        {
            Color active = new Color(0.4f, 0.6f, 0.9f, 0.7f);  // blue tint for weather
            Color inactive = new Color(0.08f, 0.08f, 0.08f);

            for (int i = 0; i < weatherButtonBgs.Length; i++)
            {
                if (weatherButtonBgs[i] == null) continue;
                weatherButtonBgs[i].color = (i == (int)selectedWeather) ? active : inactive;
            }
        }

        private void UpdateSetHighlight()
        {
            Color active = new Color(1f, 0.6f, 0, 0.7f);
            Color inactive = new Color(0.08f, 0.08f, 0.08f);

            for (int i = 0; i < setButtonBgs.Length; i++)
            {
                if (setButtonBgs[i] == null) continue;
                setButtonBgs[i].color = (i == (int)selectedSet) ? active : inactive;
            }

            var configs = SetConfig.GetDefaults();
            int idx = (int)selectedSet;
            if (idx < configs.Length && setLabelText != null)
                setLabelText.text = configs[idx].description;
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
