using UnityEngine;
using TMPro;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// Stage 2: Builds a 3D analog speedometer dial procedurally.
    /// Creates the dial face, tick marks, MPH numbers, and needle.
    /// Attach to a child of the CockpitBuilder's instrument cluster.
    /// </summary>
    public class SpeedometerBuilder : MonoBehaviour
    {
        [Header("Dial")]
        [SerializeField] private float dialRadius = 0.12f;
        [SerializeField] private float minAngleDeg = 225f; // 0 MPH position (7 o'clock)
        [SerializeField] private float maxAngleDeg = -45f;  // 60 MPH position (5 o'clock)
        [SerializeField] private int maxMPH = 60;
        [SerializeField] private int majorTickInterval = 10;
        [SerializeField] private int minorTickInterval = 5;

        [Header("Colors")]
        [SerializeField] private Color dialFaceColor = new Color(0.02f, 0.02f, 0.02f);
        [SerializeField] private Color tickColor = Color.white;
        [SerializeField] private Color needleColor = new Color(1f, 0.2f, 0.1f);
        [SerializeField] private Color numberColor = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color redZoneColor = new Color(1f, 0.15f, 0.1f, 0.4f);

        // Public so Speedometer can reference it
        [HideInInspector] public Transform needleTransform;

        public void Build()
        {
            BuildDialFace();
            BuildTicks();
            BuildNumbers();
            BuildNeedle();
            BuildRedZone();
            BuildCenterCap();
        }

        private void BuildDialFace()
        {
            var face = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            face.name = "DialFace";
            face.transform.SetParent(transform);
            face.transform.localPosition = Vector3.zero;
            face.transform.localRotation = Quaternion.Euler(90, 0, 0);
            face.transform.localScale = new Vector3(dialRadius * 2f, 0.002f, dialRadius * 2f);
            SetColor(face, dialFaceColor);
            DestroyCollider(face);

            // Chrome bezel ring
            var bezel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bezel.name = "Bezel";
            bezel.transform.SetParent(transform);
            bezel.transform.localPosition = new Vector3(0, 0, -0.001f);
            bezel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            bezel.transform.localScale = new Vector3(dialRadius * 2.1f, 0.003f, dialRadius * 2.1f);
            SetColor(bezel, new Color(0.45f, 0.45f, 0.45f));
            DestroyCollider(bezel);
        }

        private void BuildTicks()
        {
            for (int mph = 0; mph <= maxMPH; mph += minorTickInterval)
            {
                bool major = mph % majorTickInterval == 0;
                float angle = MphToAngle(mph);
                float rad = angle * Mathf.Deg2Rad;

                float tickLength = major ? 0.018f : 0.01f;
                float tickWidth = major ? 0.003f : 0.002f;
                float innerR = dialRadius - 0.025f;

                var tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tick.name = $"Tick_{mph}";
                tick.transform.SetParent(transform);

                float midR = innerR - tickLength / 2f;
                tick.transform.localPosition = new Vector3(
                    Mathf.Cos(rad) * midR,
                    Mathf.Sin(rad) * midR,
                    -0.002f);
                tick.transform.localRotation = Quaternion.Euler(0, 0, angle);
                tick.transform.localScale = new Vector3(tickWidth, tickLength, 0.001f);
                SetColor(tick, tickColor);
                DestroyCollider(tick);
            }
        }

        private void BuildNumbers()
        {
            for (int mph = 0; mph <= maxMPH; mph += majorTickInterval)
            {
                float angle = MphToAngle(mph);
                float rad = angle * Mathf.Deg2Rad;
                float numR = dialRadius - 0.045f;

                var numObj = new GameObject($"Num_{mph}");
                numObj.transform.SetParent(transform);
                numObj.transform.localPosition = new Vector3(
                    Mathf.Cos(rad) * numR,
                    Mathf.Sin(rad) * numR,
                    -0.003f);

                var tmp = numObj.AddComponent<TextMeshPro>();
                tmp.text = mph.ToString();
                tmp.fontSize = 0.3f;
                tmp.color = numberColor;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.rectTransform.sizeDelta = new Vector2(0.04f, 0.02f);
            }
        }

        private void BuildNeedle()
        {
            var needleRoot = new GameObject("NeedleRoot");
            needleRoot.transform.SetParent(transform);
            needleRoot.transform.localPosition = new Vector3(0, 0, -0.004f);
            needleTransform = needleRoot.transform;

            // Needle body
            var needle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            needle.name = "Needle";
            needle.transform.SetParent(needleRoot.transform);
            needle.transform.localPosition = new Vector3(0, dialRadius * 0.35f, 0);
            needle.transform.localScale = new Vector3(0.003f, dialRadius * 0.7f, 0.001f);
            SetColor(needle, needleColor);
            DestroyCollider(needle);

            // Counterweight (small opposite end)
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counterweight";
            counter.transform.SetParent(needleRoot.transform);
            counter.transform.localPosition = new Vector3(0, -dialRadius * 0.08f, 0);
            counter.transform.localScale = new Vector3(0.005f, dialRadius * 0.15f, 0.001f);
            SetColor(counter, needleColor * 0.7f);
            DestroyCollider(counter);
        }

        private void BuildRedZone()
        {
            // Red zone from 50-60 MPH (arc indicator)
            for (int mph = 50; mph <= 60; mph += 2)
            {
                float angle = MphToAngle(mph);
                float rad = angle * Mathf.Deg2Rad;
                float r = dialRadius - 0.01f;

                var dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dot.name = $"RedZone_{mph}";
                dot.transform.SetParent(transform);
                dot.transform.localPosition = new Vector3(
                    Mathf.Cos(rad) * r,
                    Mathf.Sin(rad) * r,
                    -0.002f);
                dot.transform.localScale = new Vector3(0.005f, 0.005f, 0.001f);
                SetColor(dot, redZoneColor);
                DestroyCollider(dot);
            }
        }

        private void BuildCenterCap()
        {
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "CenterCap";
            cap.transform.SetParent(transform);
            cap.transform.localPosition = new Vector3(0, 0, -0.005f);
            cap.transform.localScale = Vector3.one * 0.012f;
            SetColor(cap, new Color(0.4f, 0.4f, 0.4f)); // chrome cap
            DestroyCollider(cap);
        }

        private float MphToAngle(int mph)
        {
            float t = (float)mph / maxMPH;
            return Mathf.Lerp(minAngleDeg, maxAngleDeg, t);
        }

        private void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = new Material(Shader.Find("Standard"));
                r.material.color = color;
            }
        }

        private void DestroyCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }
    }
}
