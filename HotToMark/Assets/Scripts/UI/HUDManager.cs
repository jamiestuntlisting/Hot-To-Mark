using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.Scoring;

namespace HotToMark.UI
{
    /// <summary>
    /// Stages 3-7: HUD overlay showing contextual game info.
    /// Self-builds its own UI hierarchy at runtime.
    /// Adapts display based on current phase and game mode.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        private GameObject hudPanel;

        // Mode label
        private TextMeshProUGUI modeLabelText;

        // Driving phase
        private GameObject drivingGroup;
        private TextMeshProUGUI distToMarkText;

        // Exact MPH
        private GameObject exactMPHGroup;
        private TextMeshProUGUI targetMPHText;
        private TextMeshProUGUI checkpointDistText;
        private TextMeshProUGUI checkpointResultText;

        // Stopped on mark
        private GameObject stoppedGroup;
        private TextMeshProUGUI markAccuracyText;
        private TextMeshProUGUI honkInstructionText;
        private TextMeshProUGUI honkCountText;

        // Reversing
        private GameObject reversingGroup;
        private TextMeshProUGUI distToStartText;
        private TextMeshProUGUI reverseTimeText;
        private TextMeshProUGUI hurryText;
        private TextMeshProUGUI reverseMaxSpeedText;

        private GameState state;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // HUD panel — semi-transparent, top-left area
            hudPanel = new GameObject("HUDPanel");
            hudPanel.transform.SetParent(transform, false);
            var rect = hudPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0.42f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = hudPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.35f);

            // Vertical layout
            var vlg = hudPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 8, 8);
            vlg.spacing = 4;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperLeft;

            // Mode label (always visible)
            var modeObj = CreateHUDText("ModeLabel", hudPanel.transform, "", 16,
                new Color(1f, 0.6f, 0), FontStyles.Bold, 24);
            modeLabelText = modeObj;

            // ---- Driving Group ----
            drivingGroup = CreateGroup("DrivingGroup", hudPanel.transform);

            distToMarkText = CreateHUDText("DistToMark", drivingGroup.transform,
                "Distance to Mark: --", 18, Color.green, FontStyles.Normal, 26);

            // Exact MPH sub-group
            exactMPHGroup = CreateGroup("ExactMPHGroup", drivingGroup.transform);
            targetMPHText = CreateHUDText("TargetMPH", exactMPHGroup.transform,
                "", 14, new Color(1f, 0.8f, 0), FontStyles.Normal, 20);
            checkpointDistText = CreateHUDText("CheckpointDist", exactMPHGroup.transform,
                "", 14, new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal, 20);
            checkpointResultText = CreateHUDText("CheckpointResult", exactMPHGroup.transform,
                "", 14, Color.green, FontStyles.Normal, 20);
            exactMPHGroup.SetActive(false);

            drivingGroup.SetActive(false);

            // ---- Stopped Group ----
            stoppedGroup = CreateGroup("StoppedGroup", hudPanel.transform);

            markAccuracyText = CreateHUDText("MarkAccuracy", stoppedGroup.transform,
                "ON MARK!", 24, Color.yellow, FontStyles.Bold, 34);
            honkInstructionText = CreateHUDText("HonkInstruction", stoppedGroup.transform,
                "Honk horn TWICE", 14, Color.white, FontStyles.Normal, 20);
            honkCountText = CreateHUDText("HonkCount", stoppedGroup.transform,
                "Honks: 0/2", 16, new Color(1f, 0.6f, 0), FontStyles.Bold, 22);

            stoppedGroup.SetActive(false);

            // ---- Reversing Group ----
            reversingGroup = CreateGroup("ReversingGroup", hudPanel.transform);

            distToStartText = CreateHUDText("DistToStart", reversingGroup.transform,
                "Distance to Start: --", 18, Color.green, FontStyles.Normal, 26);
            reverseTimeText = CreateHUDText("ReverseTime", reversingGroup.transform,
                "Time: 0.0s", 14, Color.white, FontStyles.Normal, 20);
            hurryText = CreateHUDText("Hurry", reversingGroup.transform,
                "HURRY!", 28, Color.red, FontStyles.Bold, 36);
            hurryText.gameObject.SetActive(false);
            reverseMaxSpeedText = CreateHUDText("ReverseMaxSpeed", reversingGroup.transform,
                "", 12, new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal, 18);

            reversingGroup.SetActive(false);

            // Pause button (top-right)
            var pauseBtn = UIFactory.CreateImage("PauseButton", hudPanel.transform,
                new Color(0.2f, 0.2f, 0.2f, 0.5f));
            var pauseRect = pauseBtn.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(1, 1);
            pauseRect.anchorMax = new Vector2(1, 1);
            pauseRect.pivot = new Vector2(1, 1);
            pauseRect.sizeDelta = new Vector2(50, 40);

            var pauseLabel = UIFactory.CreateText("PauseLabel", pauseBtn.transform,
                "||", 18, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(pauseLabel, new Vector2(0, 0), new Vector2(1, 1));

            var btn = pauseBtn.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.PauseGame();
            });
        }

        private GameObject CreateGroup(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();

            var vlg = obj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2;

            var le = obj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            return obj;
        }

        private TextMeshProUGUI CreateHUDText(string name, Transform parent,
            string text, float fontSize, Color color, FontStyles style, float height)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();

            var le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = true;
            tmp.richText = true;

            return tmp;
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }
            if (state.phase == GamePhase.Menu || state.phase == GamePhase.Results
                || state.phase == GamePhase.Paused)
                return;

            UpdateModeLabel();

            bool driving = state.phase == GamePhase.Driving;
            bool stopped = state.phase == GamePhase.StoppedOnMark || state.phase == GamePhase.Honking;
            bool reversing = state.phase == GamePhase.Reversing;

            drivingGroup.SetActive(driving);
            stoppedGroup.SetActive(stopped);
            reversingGroup.SetActive(reversing);

            if (driving) UpdateDrivingHUD();
            else if (stopped) UpdateStoppedHUD();
            else if (reversing) UpdateReversingHUD();
        }

        private void UpdateModeLabel()
        {
            switch (state.mode)
            {
                case GameMode.Standard:       modeLabelText.text = "STANDARD TAKE"; break;
                case GameMode.SpeedRun:       modeLabelText.text = "SPEED RUN"; break;
                case GameMode.SmoothOperator: modeLabelText.text = "SMOOTH OPERATOR"; break;
                case GameMode.ExactMPH:       modeLabelText.text = "EXACT MPH"; break;
            }
        }

        private void UpdateDrivingHUD()
        {
            float distToMark = state.markDistance - state.posY;
            distToMarkText.text = $"Distance to Mark: {Mathf.Max(0, distToMark):F1} ft";
            distToMarkText.color = distToMark < 20 ? Color.red :
                                   distToMark < 50 ? new Color(1f, 0.65f, 0) : Color.green;

            bool isExact = state.mode == GameMode.ExactMPH;
            exactMPHGroup.SetActive(isExact);

            if (isExact)
            {
                targetMPHText.text = $"Target: {state.targetMPH:F0} MPH at checkpoint";

                if (!state.checkpointPassed)
                {
                    float distToCP = state.checkpointDistance - state.posY;
                    checkpointDistText.text = $"Checkpoint in: {Mathf.Max(0, distToCP):F0} ft";
                    checkpointDistText.gameObject.SetActive(true);
                    checkpointResultText.gameObject.SetActive(false);
                }
                else
                {
                    checkpointDistText.gameObject.SetActive(false);
                    checkpointResultText.gameObject.SetActive(true);
                    checkpointResultText.text = $"Hit {state.speedAtCheckpoint:F1} MPH " +
                        $"(accuracy: {state.exactMPHAccuracy:F0}%)";
                    checkpointResultText.color = state.exactMPHAccuracy > 80 ?
                        Color.green : new Color(1f, 0.65f, 0);
                }
            }
        }

        private void UpdateStoppedHUD()
        {
            if (state.markAccuracy > 0)
            {
                markAccuracyText.text = $"ON MARK!  {state.markAccuracy:F1}%";
                markAccuracyText.color = Color.yellow;
            }
            else
            {
                markAccuracyText.text = "MISSED!";
                markAccuracyText.color = Color.red;
            }

            honkInstructionText.text = "Honk horn TWICE to begin reverse";
            honkCountText.text = $"Honks: {state.honkCount}/{GameState.HONKS_REQUIRED}";
        }

        private void UpdateReversingHUD()
        {
            distToStartText.text = $"Distance to Start: {state.posY:F1} ft";
            distToStartText.color = state.posY < 10 ? Color.green :
                                    state.posY < 30 ? new Color(1f, 0.65f, 0) : Color.white;

            float elapsed = Time.time - state.reverseStartTime;
            reverseTimeText.text = $"Time: {elapsed:F1}s";

            hurryText.gameObject.SetActive(elapsed > GameState.HURRY_TIME);
            reverseMaxSpeedText.text = $"Max reverse speed: {state.reverseMaxSpeed:F1} mph";
        }

        public void Show()
        {
            if (hudPanel != null) hudPanel.SetActive(true);
        }

        public void Hide()
        {
            if (hudPanel != null) hudPanel.SetActive(false);
        }
    }
}
