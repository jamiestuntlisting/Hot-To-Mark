using UnityEngine;
using TMPro;
using HotToMark.Core;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// Stage 2: Dashboard speedometer — analog needle + digital readout.
    /// Attach to the instrument cluster GameObject on the dashboard.
    /// </summary>
    public class Speedometer : MonoBehaviour
    {
        [Header("Needle")]
        [SerializeField] private Transform needle;
        [SerializeField] private float minAngle = 135f;   // angle at 0 mph
        [SerializeField] private float maxAngle = -135f;   // angle at 60 mph
        [SerializeField] private float needleSmoothing = 8f;

        [Header("Digital Display")]
        [SerializeField] private TextMeshProUGUI digitalMPH;

        [Header("Gear Indicator")]
        [SerializeField] private TextMeshProUGUI gearText;
        [SerializeField] private Color driveColor = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color reverseColor = new Color(1f, 0.2f, 0.2f);

        [Header("Distance Display")]
        [SerializeField] private TextMeshProUGUI distanceText;

        private GameState state;
        private float currentNeedleAngle;

        void Start()
        {
            state = GameManager.Instance.state;
            currentNeedleAngle = minAngle;
        }

        void Update()
        {
            if (state == null) return;

            float absSpeed = Mathf.Abs(state.speed);

            // Analog needle
            float targetAngle = Mathf.Lerp(minAngle, maxAngle,
                Mathf.Clamp01(absSpeed / GameState.MAX_FORWARD_MPH));
            currentNeedleAngle = Mathf.Lerp(currentNeedleAngle, targetAngle,
                Time.deltaTime * needleSmoothing);

            if (needle != null)
                needle.localRotation = Quaternion.Euler(0, 0, currentNeedleAngle);

            // Digital readout
            if (digitalMPH != null)
                digitalMPH.text = $"{absSpeed:F0} MPH";

            // Gear indicator
            if (gearText != null)
            {
                bool isReverse = state.gear == Gear.Reverse;
                gearText.text = isReverse ? "R" : "D";
                gearText.color = isReverse ? reverseColor : driveColor;
            }

            // Distance
            if (distanceText != null)
                distanceText.text = $"{Mathf.Abs(state.posY):F1} ft";
        }
    }
}
