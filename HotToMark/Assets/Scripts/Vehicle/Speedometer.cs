using UnityEngine;
using TMPro;
using HotToMark.Core;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// Stage 2: Dashboard speedometer — drives the analog needle from SpeedometerBuilder
    /// and manages digital readout, gear indicator, and distance display.
    /// Auto-discovers SpeedometerBuilder's needle and creates its own digital overlays.
    /// </summary>
    public class Speedometer : MonoBehaviour
    {
        [Header("Needle angles")]
        [SerializeField] private float minAngle = 225f;   // 0 MPH position (matching SpeedometerBuilder)
        [SerializeField] private float maxAngle = -45f;    // 60 MPH position
        [SerializeField] private float needleSmoothing = 8f;

        [Header("Colors")]
        [SerializeField] private Color driveColor = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color reverseColor = new Color(1f, 0.2f, 0.2f);

        private GameState state;
        private Transform needle;
        private TextMeshPro digitalMPH;
        private TextMeshPro gearText;
        private TextMeshPro distanceText;
        private float currentNeedleAngle;

        void Start()
        {
            state = GameManager.Instance.state;
            currentNeedleAngle = minAngle;

            FindNeedle();
            BuildDigitalDisplays();
        }

        private void FindNeedle()
        {
            // Find the SpeedometerBuilder's needle in a sibling or child
            var builder = GetComponentInParent<CockpitBuilder>();
            if (builder != null)
            {
                var speedoBuilder = builder.GetComponentInChildren<SpeedometerBuilder>();
                if (speedoBuilder != null && speedoBuilder.needleTransform != null)
                {
                    needle = speedoBuilder.needleTransform;
                }
            }

            // Fallback: search by name
            if (needle == null)
            {
                var needleRoot = GameObject.Find("NeedleRoot");
                if (needleRoot != null) needle = needleRoot.transform;
            }
        }

        private void BuildDigitalDisplays()
        {
            // Digital MPH display on dashboard
            digitalMPH = Create3DText("DigitalMPH", new Vector3(0, 0.56f, 0.72f),
                "0 MPH", 1.2f, Color.green);

            // Gear indicator
            gearText = Create3DText("GearIndicator", new Vector3(-0.25f, 0.56f, 0.72f),
                "D", 1.5f, driveColor);

            // Distance display
            distanceText = Create3DText("DistanceDisplay", new Vector3(0.25f, 0.56f, 0.72f),
                "0.0 ft", 0.8f, new Color(0.7f, 0.7f, 0.7f));
        }

        private TextMeshPro Create3DText(string name, Vector3 localPos,
            string text, float fontSize, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform.parent); // parent is Cockpit
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = Quaternion.Euler(25, 0, 0); // tilt to match dash

            var tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.08f);

            return tmp;
        }

        void Update()
        {
            if (state == null) return;

            float absSpeed = Mathf.Abs(state.speed);

            // Drive analog needle
            float targetAngle = Mathf.Lerp(minAngle, maxAngle,
                Mathf.Clamp01(absSpeed / GameState.MAX_FORWARD_MPH));
            currentNeedleAngle = Mathf.Lerp(currentNeedleAngle, targetAngle,
                Time.deltaTime * needleSmoothing);

            if (needle != null)
                needle.localRotation = Quaternion.Euler(0, 0, currentNeedleAngle);

            // Digital readout
            if (digitalMPH != null)
            {
                digitalMPH.text = $"{absSpeed:F0} MPH";
                // Color shifts with speed
                digitalMPH.color = absSpeed > 50 ? Color.red :
                                   absSpeed > 30 ? new Color(1f, 0.65f, 0) : Color.green;
            }

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
