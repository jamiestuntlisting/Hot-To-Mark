using UnityEngine;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Stages 3-7: HUD overlay showing contextual game info.
    /// Adapts display based on current phase and game mode.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject hudPanel;

        [Header("Mode Label")]
        [SerializeField] private TextMeshProUGUI modeLabelText;

        [Header("Driving Phase")]
        [SerializeField] private GameObject drivingGroup;
        [SerializeField] private TextMeshProUGUI distToMarkText;

        [Header("Exact MPH HUD")]
        [SerializeField] private GameObject exactMPHGroup;
        [SerializeField] private TextMeshProUGUI targetMPHText;
        [SerializeField] private TextMeshProUGUI checkpointDistText;
        [SerializeField] private TextMeshProUGUI checkpointResultText;

        [Header("Stopped on Mark Phase")]
        [SerializeField] private GameObject stoppedGroup;
        [SerializeField] private TextMeshProUGUI markAccuracyText;
        [SerializeField] private TextMeshProUGUI honkInstructionText;
        [SerializeField] private TextMeshProUGUI honkCountText;

        [Header("Reversing Phase")]
        [SerializeField] private GameObject reversingGroup;
        [SerializeField] private TextMeshProUGUI distToStartText;
        [SerializeField] private TextMeshProUGUI reverseTimeText;
        [SerializeField] private TextMeshProUGUI hurryText;
        [SerializeField] private TextMeshProUGUI reverseMaxSpeedText;

        private GameState state;

        void Start()
        {
            state = GameManager.Instance.state;
        }

        void Update()
        {
            if (state == null || state.phase == GamePhase.Menu || state.phase == GamePhase.Results)
                return;

            UpdateModeLabel();

            // Toggle phase groups
            bool driving = state.phase == GamePhase.Driving;
            bool stopped = state.phase == GamePhase.StoppedOnMark || state.phase == GamePhase.Honking;
            bool reversing = state.phase == GamePhase.Reversing;

            if (drivingGroup != null) drivingGroup.SetActive(driving);
            if (stoppedGroup != null) stoppedGroup.SetActive(stopped);
            if (reversingGroup != null) reversingGroup.SetActive(reversing);

            if (driving) UpdateDrivingHUD();
            else if (stopped) UpdateStoppedHUD();
            else if (reversing) UpdateReversingHUD();
        }

        private void UpdateModeLabel()
        {
            if (modeLabelText == null) return;
            switch (state.mode)
            {
                case GameMode.Standard:      modeLabelText.text = "STANDARD TAKE"; break;
                case GameMode.SpeedRun:      modeLabelText.text = "SPEED RUN"; break;
                case GameMode.SmoothOperator: modeLabelText.text = "SMOOTH OPERATOR"; break;
                case GameMode.ExactMPH:      modeLabelText.text = "EXACT MPH"; break;
            }
        }

        private void UpdateDrivingHUD()
        {
            float distToMark = state.markDistance - state.posY;

            if (distToMarkText != null)
            {
                distToMarkText.text = $"Distance to Mark: {Mathf.Max(0, distToMark):F1} ft";
                distToMarkText.color = distToMark < 20 ? Color.red :
                                       distToMark < 50 ? new Color(1f, 0.65f, 0) : Color.green;
            }

            // Exact MPH specific
            bool isExact = state.mode == GameMode.ExactMPH;
            if (exactMPHGroup != null) exactMPHGroup.SetActive(isExact);

            if (isExact)
            {
                if (targetMPHText != null)
                    targetMPHText.text = $"Target: {state.targetMPH} MPH at checkpoint";

                if (!state.checkpointPassed)
                {
                    float distToCP = state.checkpointDistance - state.posY;
                    if (checkpointDistText != null)
                        checkpointDistText.text = $"Checkpoint in: {Mathf.Max(0, distToCP):F0} ft";
                    if (checkpointResultText != null)
                        checkpointResultText.gameObject.SetActive(false);
                }
                else
                {
                    if (checkpointDistText != null)
                        checkpointDistText.gameObject.SetActive(false);
                    if (checkpointResultText != null)
                    {
                        checkpointResultText.gameObject.SetActive(true);
                        checkpointResultText.text = $"Checkpoint: {state.speedAtCheckpoint:F1} MPH " +
                            $"(accuracy: {state.exactMPHAccuracy:F0}%)";
                        checkpointResultText.color = Color.green;
                    }
                }
            }
        }

        private void UpdateStoppedHUD()
        {
            if (markAccuracyText != null)
            {
                if (state.markAccuracy > 0)
                {
                    markAccuracyText.text = $"ON MARK! {state.markAccuracy:F1}%";
                    markAccuracyText.color = Color.yellow;
                }
                else
                {
                    markAccuracyText.text = "MISSED!";
                    markAccuracyText.color = Color.red;
                }
            }

            if (honkInstructionText != null)
                honkInstructionText.text = "Honk horn TWICE to begin reverse";

            if (honkCountText != null)
                honkCountText.text = $"Honks: {state.honkCount}/{GameState.HONKS_REQUIRED}";
        }

        private void UpdateReversingHUD()
        {
            if (distToStartText != null)
                distToStartText.text = $"Distance to Start: {state.posY:F1} ft";

            float elapsed = Time.time - state.reverseStartTime;

            if (reverseTimeText != null)
                reverseTimeText.text = $"Time: {elapsed:F1}s";

            if (hurryText != null)
            {
                hurryText.gameObject.SetActive(elapsed > GameState.HURRY_TIME);
                hurryText.text = "HURRY!";
            }

            if (reverseMaxSpeedText != null)
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
