using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    /// Auto-runs via RuntimeInitializeOnLoadMethod — no manual scene setup needed.
    /// Can also be attached to a GameObject manually.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        private static bool hasBootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoBootstrap()
        {
            hasBootstrapped = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBootstrapAfterScene()
        {
            Debug.Log("[HotToMark] AutoBootstrapAfterScene called");
            if (hasBootstrapped)
            {
                Debug.Log("[HotToMark] Already bootstrapped, skipping");
                return;
            }
            if (FindAnyObjectByType<SceneBootstrap>() != null)
            {
                Debug.Log("[HotToMark] SceneBootstrap already exists, skipping");
                return;
            }
            if (FindAnyObjectByType<GameManager>() != null)
            {
                Debug.Log("[HotToMark] GameManager already exists, skipping");
                return;
            }

            Debug.Log("[HotToMark] Creating SceneBootstrap...");
            var bootstrapObj = new GameObject("SceneBootstrap");
            bootstrapObj.AddComponent<SceneBootstrap>();
        }

        void Awake()
        {
            Debug.Log("[HotToMark] SceneBootstrap.Awake called");
            if (hasBootstrapped) { Destroy(gameObject); return; }
            hasBootstrapped = true;
            try
            {
                BuildScene();
                Debug.Log("[HotToMark] BuildScene completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HotToMark] BuildScene FAILED: {e}");
            }
        }

        private void BuildScene()
        {
            // ---- Create GameState ----
            var gameState = ScriptableObject.CreateInstance<GameState>();

            // ---- Root objects ----
            var carRoot = new GameObject("Car");
            var environmentRoot = new GameObject("Environment");
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
                cockpitBuilder.steeringWheelTransform
                    .gameObject.AddComponent<SteeringWheel>();
            }

            // Speedometer (data display)
            var speedoObj = new GameObject("Speedometer");
            speedoObj.transform.SetParent(cockpitObj.transform);
            speedoObj.AddComponent<Speedometer>();

            // Procedural speedometer dial (3D gauge)
            var speedoDialObj = new GameObject("SpeedometerDial");
            speedoDialObj.transform.SetParent(cockpitObj.transform);
            speedoDialObj.transform.localPosition = new Vector3(0, 0.62f, 0.65f);
            speedoDialObj.transform.localScale = Vector3.one * 0.8f;
            var speedoBuilder = speedoDialObj.AddComponent<SpeedometerBuilder>();
            speedoBuilder.Build();

            // ---- Main Camera (Stage 1) ----
            var mainCamObj = new GameObject("MainCamera");
            mainCamObj.tag = "MainCamera";
            var mainCam = mainCamObj.AddComponent<UnityEngine.Camera>();
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 65;
            mainCam.nearClipPlane = 0.1f;
            mainCam.farClipPlane = 500f;
            mainCamObj.AddComponent<CockpitCamera>();
            mainCamObj.AddComponent<AudioListener>();

            // ---- Road (Stage 2) ----
            var roadObj = new GameObject("Road");
            roadObj.transform.SetParent(environmentRoot.transform);
            var roadManager = roadObj.AddComponent<RoadManager>();

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

            var ambientObj = new GameObject("AmbientAudio");
            ambientObj.transform.SetParent(audioRoot.transform);
            ambientObj.AddComponent<AmbientAudioGenerator>();

            // Director feedback (F-6)
            var directorObj = new GameObject("DirectorFeedback");
            directorObj.transform.SetParent(audioRoot.transform);
            var directorFeedback = directorObj.AddComponent<DirectorFeedback>();

            // Walkie-talkie comms (F-9)
            var walkieObj = new GameObject("WalkieTalkieComms");
            walkieObj.transform.SetParent(audioRoot.transform);
            walkieObj.AddComponent<WalkieTalkieComms>();

            // ---- Haptics (Stage 8) ----
            var hapticObj = new GameObject("Haptics");
            var haptics = hapticObj.AddComponent<HapticFeedback>();

            // ---- Input (Stage 1) ----
            var inputObj = new GameObject("InputManager");
            inputObj.AddComponent<TouchInputManager>();

            // ---- UI Canvas ----
            var uiRoot = CreateUICanvas();

            // UI components self-build their hierarchies in Awake()
            var menuObj = new GameObject("MainMenu");
            menuObj.transform.SetParent(uiRoot.transform, false);
            menuObj.AddComponent<RectTransform>().SetFullStretch();
            var mainMenu = menuObj.AddComponent<MainMenuUI>();

            var resultsObj = new GameObject("ResultsScreen");
            resultsObj.transform.SetParent(uiRoot.transform, false);
            resultsObj.AddComponent<RectTransform>().SetFullStretch();
            var resultsScreen = resultsObj.AddComponent<ResultsScreenUI>();

            var hudObj = new GameObject("HUD");
            hudObj.transform.SetParent(uiRoot.transform, false);
            hudObj.AddComponent<RectTransform>().SetFullStretch();
            var hud = hudObj.AddComponent<HUDManager>();

            var touchObj = new GameObject("TouchControls");
            touchObj.transform.SetParent(uiRoot.transform, false);
            touchObj.AddComponent<RectTransform>().SetFullStretch();
            touchObj.AddComponent<TouchControlsUI>();

            // ---- PIP Display (overlay on UI canvas) ----
            BuildPIPDisplay(uiRoot.transform, pipController);

            // ---- Career Menu (F-3) ----
            var careerMenuObj = new GameObject("CareerMenu");
            careerMenuObj.transform.SetParent(uiRoot.transform, false);
            careerMenuObj.AddComponent<RectTransform>().SetFullStretch();
            var careerMenu = careerMenuObj.AddComponent<CareerMenuUI>();

            // ---- Pause Menu ----
            var pauseObj = new GameObject("PauseMenu");
            pauseObj.transform.SetParent(uiRoot.transform, false);
            pauseObj.AddComponent<RectTransform>().SetFullStretch();
            var pauseMenu = pauseObj.AddComponent<PauseMenuUI>();

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
            gm.pauseMenu = pauseMenu;
            gm.directorFeedback = directorFeedback;

            // ---- Weather System (F-7) ----
            var weatherObj = new GameObject("WeatherSystem");
            weatherObj.transform.SetParent(environmentRoot.transform);
            var weatherSystem = weatherObj.AddComponent<WeatherSystem>();

            // ---- Multi-Mark System (F-8) ----
            var multiMarkObj = new GameObject("MultiMarkSystem");
            multiMarkObj.transform.SetParent(environmentRoot.transform);
            var multiMarkSystem = multiMarkObj.AddComponent<MultiMarkSystem>();

            // ---- Obstacle System (F-10) ----
            var obstacleObj = new GameObject("ObstacleSystem");
            obstacleObj.transform.SetParent(environmentRoot.transform);
            var obstacleSystem = obstacleObj.AddComponent<ObstacleSystem>();

            // ---- Replay System (F-5) ----
            var replayObj = new GameObject("ReplaySystem");
            var replaySystem = replayObj.AddComponent<ReplaySystem>();

            // ---- Career Manager (F-3) ----
            var careerObj = new GameObject("CareerManager");
            careerObj.AddComponent<CareerManager>();

            // Wire late-created systems to GameManager
            gm.roadManager = roadManager;
            gm.weatherSystem = weatherSystem;
            gm.multiMarkSystem = multiMarkSystem;
            gm.obstacleSystem = obstacleSystem;
            gm.replaySystem = replaySystem;
            gm.careerMenu = careerMenu;

            // ---- Game Center (F-4) ----
            var gcObj = new GameObject("GameCenter");
            gcObj.AddComponent<GameCenterManager>();

            // ---- Visual Polish (Stage 8) ----
            var polishObj = new GameObject("VisualPolish");
            polishObj.AddComponent<VisualPolish>();

            // ---- Performance Optimizer (Stage 8) ----
            var perfObj = new GameObject("PerformanceOptimizer");
            perfObj.AddComponent<PerformanceOptimizer>();

            // ---- Skybox & Lighting ----
            SetupSkybox();
            SetupLighting();
        }

        private GameObject CreateUICanvas()
        {
            var canvasObj = new GameObject("UICanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170, 2532);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem is required for UI button/touch input to work
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventObj = new GameObject("EventSystem");
                eventObj.AddComponent<EventSystem>();
                // Use InputSystemUIInputModule for new Input System, fall back to StandaloneInputModule
                #if ENABLE_INPUT_SYSTEM
                eventObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                #else
                eventObj.AddComponent<StandaloneInputModule>();
                #endif
            }

            return canvasObj;
        }

        private void BuildPIPDisplay(Transform uiParent, PIPCameraController pipController)
        {
            // PIP container — upper-right corner
            var pipPanel = new GameObject("PIPPanel");
            pipPanel.transform.SetParent(uiParent, false);
            var pipRect = pipPanel.AddComponent<RectTransform>();
            pipRect.anchorMin = new Vector2(0.62f, 0.72f);
            pipRect.anchorMax = new Vector2(0.98f, 0.98f);
            pipRect.offsetMin = Vector2.zero;
            pipRect.offsetMax = Vector2.zero;

            // Black border
            var border = pipPanel.AddComponent<Image>();
            border.color = new Color(0.05f, 0.05f, 0.05f);

            // RawImage for the PIP RenderTexture
            var displayObj = new GameObject("PIPDisplay");
            displayObj.transform.SetParent(pipPanel.transform, false);
            var displayRect = displayObj.AddComponent<RectTransform>();
            displayRect.anchorMin = new Vector2(0.02f, 0.05f);
            displayRect.anchorMax = new Vector2(0.98f, 0.85f);
            displayRect.offsetMin = Vector2.zero;
            displayRect.offsetMax = Vector2.zero;
            var rawImg = displayObj.AddComponent<RawImage>();

            // REC dot
            var recObj = UIFactory.CreateImage("RECDot", pipPanel.transform,
                new Color(1f, 0, 0));
            UIFactory.SetAnchors(recObj, new Vector2(0.04f, 0.88f), new Vector2(0.08f, 0.96f));

            var recLabel = UIFactory.CreateText("RECLabel", pipPanel.transform,
                "REC", 8, Color.red, TMPro.FontStyles.Bold, TMPro.TextAlignmentOptions.Left);
            UIFactory.SetAnchors(recLabel, new Vector2(0.1f, 0.86f), new Vector2(0.3f, 0.98f));

            // Camera label
            var camLabel = UIFactory.CreateText("CamLabel", pipPanel.transform,
                "CAM A - WIDE", 7, new Color(0.8f, 0.8f, 0.8f),
                TMPro.FontStyles.Normal, TMPro.TextAlignmentOptions.Right);
            UIFactory.SetAnchors(camLabel, new Vector2(0.5f, 0.86f), new Vector2(0.96f, 0.98f));

            // Timecode at bottom
            var timecodeObj = UIFactory.CreateText("Timecode", pipPanel.transform,
                "00:00:00", 7, new Color(0.9f, 0.9f, 0.9f),
                TMPro.FontStyles.Normal, TMPro.TextAlignmentOptions.Center);
            UIFactory.SetAnchors(timecodeObj, new Vector2(0.2f, 0), new Vector2(0.8f, 0.06f));
        }

        private void SetupSkybox()
        {
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
            var sunObj = new GameObject("Sun");
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;
            sunObj.transform.rotation = Quaternion.Euler(45, -30, 0);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.7f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.55f, 0.6f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.35f, 0.25f);
        }
    }

    /// <summary>
    /// Extension method to set a RectTransform to full-stretch mode.
    /// </summary>
    public static class RectTransformExtensions
    {
        public static void SetFullStretch(this RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
