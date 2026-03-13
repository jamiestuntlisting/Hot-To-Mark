using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Vehicle;
using HotToMark.Environment;
using HotToMark.Camera;
using HotToMark.UI;
using HotToMark.Scoring;
using HotToMark.Audio;
using HotToMark.Haptics;
using HotToMark.Input;

namespace HotToMark.Core
{
    /// <summary>
    /// Bootstrap script that builds the entire scene at runtime.
    /// Attach this to an empty GameObject in a blank scene.
    /// It creates all game objects, wires references, and starts the game.
    ///
    /// This allows the game to run from a single empty scene without
    /// requiring pre-built prefabs or complex scene hierarchy.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        void Awake()
        {
            BuildScene();
        }

        private void BuildScene()
        {
            // ---- Create GameState ----
            var gameState = ScriptableObject.CreateInstance<GameState>();

            // ---- Root objects ----
            var carRoot = new GameObject("Car");
            var environmentRoot = new GameObject("Environment");
            var uiRoot = CreateUIRoot();
            var audioRoot = new GameObject("Audio");

            // ---- Car + Physics (Stage 1+2) ----
            var carController = carRoot.AddComponent<CarController>();

            // ---- Cockpit (Stage 1) ----
            var cockpitObj = new GameObject("Cockpit");
            cockpitObj.transform.SetParent(carRoot.transform);
            var cockpitBuilder = cockpitObj.AddComponent<CockpitBuilder>();
            cockpitBuilder.Build();

            // Steering wheel component
            if (cockpitBuilder.steeringWheelTransform != null)
            {
                var steeringWheel = cockpitBuilder.steeringWheelTransform
                    .gameObject.AddComponent<SteeringWheel>();
            }

            // Speedometer
            var speedoObj = new GameObject("Speedometer");
            speedoObj.transform.SetParent(cockpitObj.transform);
            speedoObj.AddComponent<Speedometer>();

            // ---- Main Camera (Stage 1) ----
            var mainCamObj = new GameObject("MainCamera");
            mainCamObj.tag = "MainCamera";
            var mainCam = mainCamObj.AddComponent<UnityEngine.Camera>();
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 65;
            mainCam.nearClipPlane = 0.1f;
            mainCam.farClipPlane = 500f;
            var cockpitCam = mainCamObj.AddComponent<CockpitCamera>();

            // Audio listener on main camera
            mainCamObj.AddComponent<AudioListener>();

            // ---- Road (Stage 2) ----
            var roadObj = new GameObject("Road");
            roadObj.transform.SetParent(environmentRoot.transform);
            roadObj.AddComponent<RoadManager>();

            // ---- Mark System (Stage 3) ----
            var markObj = new GameObject("MarkSystem");
            markObj.transform.SetParent(environmentRoot.transform);
            var markSystem = markObj.AddComponent<MarkSystem>();

            // ---- Horn (Stage 4) ----
            var hornObj = new GameObject("Horn");
            hornObj.transform.SetParent(carRoot.transform);
            var hornSystem = hornObj.AddComponent<HornSystem>();

            // ---- Score Manager (Stage 4+7) ----
            var scoreObj = new GameObject("ScoreManager");
            var scoreManager = scoreObj.AddComponent<ScoreManager>();

            // ---- BTS Crew (Stage 5) ----
            var crewObj = new GameObject("BTSCrew");
            crewObj.transform.SetParent(environmentRoot.transform);
            var crewManager = crewObj.AddComponent<CrewManager>();

            // ---- PIP Camera (Stage 6) ----
            var pipObj = new GameObject("PIPCamera");
            var pipController = pipObj.AddComponent<PIPCameraController>();

            // ---- Rearview Mirror (Stage 8) ----
            var mirrorObj = new GameObject("RearviewMirror");
            mirrorObj.transform.SetParent(cockpitObj.transform);
            mirrorObj.AddComponent<RearviewMirror>();

            // ---- Audio (Stage 8) ----
            audioRoot.AddComponent<AudioSource>();
            var engineAudio = audioRoot.AddComponent<EngineAudioController>();

            // Ambient film set audio generator (procedural)
            var ambientObj = new GameObject("AmbientAudio");
            ambientObj.transform.SetParent(audioRoot.transform);
            ambientObj.AddComponent<AmbientAudioGenerator>();

            // ---- Haptics (Stage 8) ----
            var hapticObj = new GameObject("Haptics");
            var haptics = hapticObj.AddComponent<HapticFeedback>();

            // ---- Input (Stage 1) ----
            var inputObj = new GameObject("InputManager");
            var inputManager = inputObj.AddComponent<TouchInputManager>();

            // ---- UI (Stage 4+7) ----
            var mainMenu = BuildMainMenuUI(uiRoot.transform);
            var resultsScreen = BuildResultsUI(uiRoot.transform);
            var hud = BuildHUDUI(uiRoot.transform);
            var touchControls = BuildTouchControlsUI(uiRoot.transform);

            // ---- GameManager (wires everything) ----
            var gmObj = new GameObject("GameManager");
            var gm = gmObj.AddComponent<GameManager>();
            gm.state = gameState;
            gm.carController = carController;
            gm.markSystem = markSystem;
            gm.hornSystem = hornSystem;
            gm.scoreManager = scoreManager;
            gm.mainMenu = mainMenu;
            gm.resultsScreen = resultsScreen;
            gm.hud = hud;
            gm.engineAudio = engineAudio;
            gm.haptics = haptics;
            gm.crewManager = crewManager;
            gm.pipCamera = pipController;

            // ---- Procedural Speedometer Dial (Stage 2) ----
            var speedoDialObj = new GameObject("SpeedometerDial");
            speedoDialObj.transform.SetParent(cockpitObj.transform);
            speedoDialObj.transform.localPosition = new Vector3(0, 0.62f, 0.65f);
            speedoDialObj.transform.localScale = Vector3.one * 0.8f;
            var speedoBuilder = speedoDialObj.AddComponent<SpeedometerBuilder>();
            speedoBuilder.Build();

            // ---- Visual Polish (Stage 8) ----
            var polishObj = new GameObject("VisualPolish");
            polishObj.AddComponent<VisualPolish>();

            // ---- Performance Optimizer (Stage 8) ----
            var perfObj = new GameObject("PerformanceOptimizer");
            perfObj.AddComponent<PerformanceOptimizer>();

            // ---- Skybox ----
            SetupSkybox();

            // ---- Lighting ----
            SetupLighting();
        }

        // ---- UI Builders ----

        private GameObject CreateUIRoot()
        {
            var canvasObj = new GameObject("UICanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution =
                new Vector2(1170, 2532);
            canvasObj.AddComponent<GraphicRaycaster>();
            return canvasObj;
        }

        private MainMenuUI BuildMainMenuUI(Transform parent)
        {
            var panelObj = CreatePanel("MenuPanel", parent,
                new Color(0.03f, 0.03f, 0.03f, 0.94f));

            var menuUI = panelObj.AddComponent<MainMenuUI>();

            // Title
            var title = CreateText("Title", panelObj.transform,
                "HOT TO MARK", 46, new Color(1f, 0.6f, 0));
            title.transform.localPosition = new Vector3(0, 350, 0);
            var titleTMP = title.GetComponent<TextMeshProUGUI>();
            titleTMP.fontStyle = FontStyles.Bold;

            // Subtitle
            var subtitle = CreateText("Subtitle", panelObj.transform,
                "The Stunt Driving Game", 15, new Color(0.75f, 0.75f, 0.75f));
            subtitle.transform.localPosition = new Vector3(0, 290, 0);
            var subTMP = subtitle.GetComponent<TextMeshProUGUI>();
            subTMP.fontStyle = FontStyles.Italic;

            // Mode buttons
            float yStart = 180;
            string[] names = { "Standard Take", "Speed Run", "Smooth Operator", "Exact MPH" };
            string[] descs = {
                "Drive to the mark, stop accurately, reverse to one.",
                "Complete the entire take as fast as possible.",
                "Minimize jerky inputs. Be cinematic.",
                "Hit exactly the target speed at the checkpoint."
            };

            Button[] buttons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var btn = CreateButton(names[i], panelObj.transform,
                    new Vector3(0, yStart - i * 110, 0),
                    new Vector2(400, 55));
                buttons[i] = btn;

                var desc = CreateText($"Desc_{i}", panelObj.transform,
                    descs[i], 11, new Color(0.55f, 0.55f, 0.55f));
                desc.transform.localPosition = new Vector3(0, yStart - i * 110 - 38, 0);
            }

            // Use reflection-free approach: set serialized fields via a helper
            SetMenuUIFields(menuUI, panelObj, title, subtitle, buttons);

            return menuUI;
        }

        private void SetMenuUIFields(MainMenuUI menuUI, GameObject panel,
            GameObject title, GameObject subtitle, Button[] buttons)
        {
            // Since we can't set [SerializeField] directly at runtime,
            // we use a public init method pattern.
            // The MainMenuUI.Start() sets text from serialized fields,
            // but we've already set text on creation. The buttons need wiring.

            // Wire button click events directly
            GameMode[] modes = {
                GameMode.Standard, GameMode.SpeedRun,
                GameMode.SmoothOperator, GameMode.ExactMPH
            };
            for (int i = 0; i < buttons.Length; i++)
            {
                var mode = modes[i];
                buttons[i].onClick.AddListener(() => GameManager.Instance.StartGame(mode));
            }
        }

        private ResultsScreenUI BuildResultsUI(Transform parent)
        {
            var panelObj = CreatePanel("ResultsPanel", parent,
                new Color(0.03f, 0.03f, 0.03f, 0.92f));
            panelObj.SetActive(false);

            var resultsUI = panelObj.AddComponent<ResultsScreenUI>();
            return resultsUI;
        }

        private HUDManager BuildHUDUI(Transform parent)
        {
            var panelObj = new GameObject("HUDPanel");
            panelObj.transform.SetParent(parent);
            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0.4f, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(15, -15);
            rect.sizeDelta = new Vector2(400, 200);

            var hudUI = panelObj.AddComponent<HUDManager>();
            return hudUI;
        }

        private TouchControlsUI BuildTouchControlsUI(Transform parent)
        {
            var panelObj = new GameObject("TouchControls");
            panelObj.transform.SetParent(parent);
            panelObj.AddComponent<RectTransform>();

            var touchUI = panelObj.AddComponent<TouchControlsUI>();
            return touchUI;
        }

        // ---- UI Helpers ----

        private GameObject CreatePanel(string name, Transform parent, Color bgColor)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = bgColor;

            return panel;
        }

        private GameObject CreateText(string name, Transform parent,
            string content, float fontSize, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 60);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private Button CreateButton(string label, Transform parent,
            Vector3 localPos, Vector2 size)
        {
            var btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.localPosition = localPos;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f);

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.1f, 0.1f, 0.1f);
            colors.highlightedColor = new Color(1f, 0.6f, 0);
            colors.pressedColor = new Color(0.8f, 0.5f, 0);
            btn.colors = colors;

            // Button outline
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.6f, 0);
            outline.effectDistance = new Vector2(2, 2);

            // Label text
            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 17;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }

        // ---- Environment Setup ----

        private void SetupSkybox()
        {
            // Create a procedural skybox material
            var skyMat = new Material(Shader.Find("Skybox/Procedural"));
            if (skyMat != null)
            {
                skyMat.SetColor("_SkyTint", new Color(0.4f, 0.6f, 0.9f));
                skyMat.SetColor("_GroundColor", new Color(0.35f, 0.5f, 0.25f));
                skyMat.SetFloat("_Exposure", 1.2f);
                skyMat.SetFloat("_AtmosphereThickness", 1.0f);
                RenderSettings.skybox = skyMat;
            }
        }

        private void SetupLighting()
        {
            // Directional light (sun)
            var sunObj = new GameObject("Sun");
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;
            sunObj.transform.rotation = Quaternion.Euler(45, -30, 0);

            // Ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.7f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.55f, 0.6f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.35f, 0.25f);
        }
    }
}
