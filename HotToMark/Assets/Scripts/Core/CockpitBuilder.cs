using UnityEngine;

namespace HotToMark.Core
{
    /// <summary>
    /// Stage 1: Procedurally builds the 3D cockpit interior at runtime.
    /// Creates dashboard, steering wheel, windshield frame, A-pillars,
    /// side windows, vents, and instrument cluster.
    /// Attach to the cockpit root GameObject.
    /// </summary>
    public class CockpitBuilder : MonoBehaviour
    {
        [Header("Materials (optional — uses defaults if null)")]
        [SerializeField] private Material dashboardMaterial;
        [SerializeField] private Material steeringWheelMaterial;
        [SerializeField] private Material windshieldMaterial;
        [SerializeField] private Material pillarMaterial;
        [SerializeField] private Material seatMaterial;

        [Header("Built References (auto-populated)")]
        public Transform steeringWheelTransform;
        public Transform leftHandTransform;
        public Transform rightHandTransform;

        private static readonly Color dashColor = new Color(0.12f, 0.12f, 0.12f);
        private static readonly Color pillarColor = new Color(0.1f, 0.1f, 0.1f);
        private static readonly Color wheelColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color chromeColor = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color glassColor = new Color(0.5f, 0.7f, 0.85f, 0.15f);
        private static readonly Color skinColor = new Color(0.83f, 0.68f, 0.55f);
        private static readonly Color ventColor = new Color(0.06f, 0.06f, 0.06f);

        public void Build()
        {
            BuildDashboard();
            BuildSteeringWheel();
            BuildAPillars();
            BuildSideWindows();
            BuildWindshield();
            BuildVents();
            BuildRearInterior();
            BuildRoof();
            BuildDoorPanels();
        }

        private void BuildDashboard()
        {
            // Main dashboard surface
            var dash = CreatePart("Dashboard", PrimitiveType.Cube,
                new Vector3(0, 0.55f, 0.7f),
                new Vector3(2.2f, 0.15f, 0.6f),
                dashboardMaterial, dashColor);

            // Dashboard top
            var dashTop = CreatePart("DashboardTop", PrimitiveType.Cube,
                new Vector3(0, 0.63f, 0.55f),
                new Vector3(2.2f, 0.02f, 0.3f),
                dashboardMaterial, dashColor * 1.2f);

            // Instrument cluster housing
            var cluster = CreatePart("InstrumentCluster", PrimitiveType.Cylinder,
                new Vector3(0, 0.62f, 0.65f),
                new Vector3(0.35f, 0.02f, 0.35f),
                dashboardMaterial, new Color(0.05f, 0.05f, 0.05f));
            cluster.transform.localRotation = Quaternion.Euler(90, 0, 0);

            // Lower dash / knee panel
            var lowerDash = CreatePart("LowerDash", PrimitiveType.Cube,
                new Vector3(0, 0.35f, 0.65f),
                new Vector3(2.0f, 0.35f, 0.1f),
                dashboardMaterial, dashColor * 0.8f);
        }

        private void BuildSteeringWheel()
        {
            // Wheel hub (parent for rotation)
            var wheelRoot = new GameObject("SteeringWheelRoot");
            wheelRoot.transform.SetParent(transform);
            wheelRoot.transform.localPosition = new Vector3(0, 0.6f, 0.45f);
            wheelRoot.transform.localRotation = Quaternion.Euler(25, 0, 0);
            steeringWheelTransform = wheelRoot.transform;

            // Rim (torus approximated with a flattened cylinder)
            var rim = CreatePart("WheelRim", PrimitiveType.Cylinder,
                Vector3.zero, new Vector3(0.32f, 0.012f, 0.32f),
                steeringWheelMaterial, wheelColor, wheelRoot.transform);

            // Hub center
            var hub = CreatePart("WheelHub", PrimitiveType.Cylinder,
                Vector3.zero, new Vector3(0.08f, 0.015f, 0.08f),
                steeringWheelMaterial, new Color(0.18f, 0.18f, 0.18f), wheelRoot.transform);

            // Spokes (3-spoke design)
            float spokeLength = 0.12f;
            float[] spokeAngles = { 0, 120, 240 };
            foreach (float angle in spokeAngles)
            {
                float rad = angle * Mathf.Deg2Rad;
                var spoke = CreatePart($"Spoke_{angle}", PrimitiveType.Cube,
                    new Vector3(Mathf.Sin(rad) * spokeLength / 2f, 0,
                                Mathf.Cos(rad) * spokeLength / 2f),
                    new Vector3(0.025f, 0.01f, spokeLength),
                    steeringWheelMaterial, chromeColor, wheelRoot.transform);
                spoke.transform.localRotation = Quaternion.Euler(0, angle, 0);
            }

            // Hands
            leftHandTransform = BuildHand("LeftHand", wheelRoot.transform, 180f);
            rightHandTransform = BuildHand("RightHand", wheelRoot.transform, 0f);
        }

        private Transform BuildHand(string name, Transform parent, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            float handRadius = 0.15f;

            var hand = new GameObject(name);
            hand.transform.SetParent(parent);
            hand.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * handRadius,
                Mathf.Sin(rad) * handRadius,
                -0.02f);

            // Palm
            var palm = CreatePart("Palm", PrimitiveType.Sphere,
                Vector3.zero, new Vector3(0.06f, 0.04f, 0.07f),
                null, skinColor, hand.transform);

            // Fingers
            for (int i = -1; i <= 1; i++)
            {
                CreatePart($"Finger_{i}", PrimitiveType.Capsule,
                    new Vector3(i * 0.015f, 0.025f, 0),
                    new Vector3(0.015f, 0.02f, 0.015f),
                    null, skinColor * 0.9f, hand.transform);
            }

            // Wrist
            CreatePart("Wrist", PrimitiveType.Capsule,
                new Vector3(0, -0.04f, 0),
                new Vector3(0.03f, 0.04f, 0.03f),
                null, skinColor, hand.transform);

            return hand.transform;
        }

        private void BuildAPillars()
        {
            // Left A-pillar
            CreatePart("LeftAPillar", PrimitiveType.Cube,
                new Vector3(-1.0f, 0.9f, 0.6f),
                new Vector3(0.06f, 1.2f, 0.06f),
                pillarMaterial, pillarColor);

            // Right A-pillar
            CreatePart("RightAPillar", PrimitiveType.Cube,
                new Vector3(1.0f, 0.9f, 0.6f),
                new Vector3(0.06f, 1.2f, 0.06f),
                pillarMaterial, pillarColor);
        }

        private void BuildSideWindows()
        {
            // Left window frame (sill — bottom edge of window)
            var leftSill = CreatePart("LeftWindowSill", PrimitiveType.Cube,
                new Vector3(-0.98f, 0.88f, 0.15f),
                new Vector3(0.06f, 0.03f, 0.8f),
                pillarMaterial, pillarColor);

            // Left glass — positioned between door panel top and roof
            var leftGlass = CreatePart("LeftWindowGlass", PrimitiveType.Quad,
                new Vector3(-0.96f, 1.08f, 0.15f),
                new Vector3(0.75f, 0.4f, 1),
                windshieldMaterial, glassColor);
            leftGlass.transform.localRotation = Quaternion.Euler(0, 90, 0);
            MakeTransparent(leftGlass);

            // Right window frame (sill)
            var rightSill = CreatePart("RightWindowSill", PrimitiveType.Cube,
                new Vector3(0.98f, 0.88f, 0.15f),
                new Vector3(0.06f, 0.03f, 0.8f),
                pillarMaterial, pillarColor);

            // Right glass
            var rightGlass = CreatePart("RightWindowGlass", PrimitiveType.Quad,
                new Vector3(0.96f, 1.08f, 0.15f),
                new Vector3(0.75f, 0.4f, 1),
                windshieldMaterial, glassColor);
            rightGlass.transform.localRotation = Quaternion.Euler(0, -90, 0);
            MakeTransparent(rightGlass);
        }

        private void BuildWindshield()
        {
            // Windshield glass with tint
            var windshield = CreatePart("Windshield", PrimitiveType.Quad,
                new Vector3(0, 1.1f, 1.0f),
                new Vector3(1.8f, 0.8f, 1),
                windshieldMaterial, new Color(0.6f, 0.8f, 0.95f, 0.06f));
            windshield.transform.localRotation = Quaternion.Euler(15, 0, 0);
            MakeTransparent(windshield);
        }

        private void BuildVents()
        {
            // Left vent
            CreatePart("LeftVent", PrimitiveType.Cube,
                new Vector3(-0.55f, 0.6f, 0.68f),
                new Vector3(0.15f, 0.05f, 0.04f),
                null, ventColor);

            // Right vent
            CreatePart("RightVent", PrimitiveType.Cube,
                new Vector3(0.55f, 0.6f, 0.68f),
                new Vector3(0.15f, 0.05f, 0.04f),
                null, ventColor);

            // Center vent
            CreatePart("CenterVent", PrimitiveType.Cube,
                new Vector3(0, 0.6f, 0.72f),
                new Vector3(0.12f, 0.04f, 0.03f),
                null, ventColor);
        }

        private void BuildRearInterior()
        {
            // B-pillars (behind driver, connecting roof to body)
            CreatePart("LeftBPillar", PrimitiveType.Cube,
                new Vector3(-1.0f, 0.9f, -0.4f),
                new Vector3(0.08f, 1.0f, 0.08f),
                pillarMaterial, pillarColor);

            CreatePart("RightBPillar", PrimitiveType.Cube,
                new Vector3(1.0f, 0.9f, -0.4f),
                new Vector3(0.08f, 1.0f, 0.08f),
                pillarMaterial, pillarColor);

            // Driver headrest — small, sits at top of seat back, doesn't block much
            CreatePart("DriverHeadrest", PrimitiveType.Cube,
                new Vector3(0, 1.08f, -0.1f),
                new Vector3(0.18f, 0.14f, 0.06f),
                seatMaterial, dashColor * 1.5f);

            // Driver seat back — narrow so you can see around it
            CreatePart("DriverSeatBack", PrimitiveType.Cube,
                new Vector3(0, 0.72f, -0.1f),
                new Vector3(0.38f, 0.5f, 0.08f),
                seatMaterial, dashColor * 1.3f);

            // Rear seat bench — low, below sightline
            CreatePart("RearSeatBottom", PrimitiveType.Cube,
                new Vector3(0, 0.3f, -0.7f),
                new Vector3(1.4f, 0.15f, 0.4f),
                seatMaterial, dashColor * 1.3f);

            // Rear seat back — shorter, stops well below rear window
            CreatePart("RearSeatBack", PrimitiveType.Cube,
                new Vector3(0, 0.6f, -0.95f),
                new Vector3(1.4f, 0.4f, 0.1f),
                seatMaterial, dashColor * 1.3f);

            // Rear headrests — small, with gaps between them for visibility
            CreatePart("RearHeadrest_L", PrimitiveType.Cube,
                new Vector3(-0.45f, 0.88f, -0.95f),
                new Vector3(0.14f, 0.12f, 0.06f),
                seatMaterial, dashColor * 1.5f);

            CreatePart("RearHeadrest_R", PrimitiveType.Cube,
                new Vector3(0.45f, 0.88f, -0.95f),
                new Vector3(0.14f, 0.12f, 0.06f),
                seatMaterial, dashColor * 1.5f);

            // Rear shelf / package tray — thin strip below rear window
            CreatePart("RearShelf", PrimitiveType.Cube,
                new Vector3(0, 0.88f, -1.15f),
                new Vector3(1.6f, 0.03f, 0.25f),
                dashboardMaterial, dashColor);

            // Rear window — large, so you can see through it when looking back
            var rearWindow = CreatePart("RearWindow", PrimitiveType.Quad,
                new Vector3(0, 1.1f, -1.35f),
                new Vector3(1.4f, 0.45f, 1),
                windshieldMaterial, new Color(0.5f, 0.7f, 0.85f, 0.08f));
            rearWindow.transform.localRotation = Quaternion.Euler(-10, 180, 0);
            MakeTransparent(rearWindow);

            // Rear window frame — thin bar at top
            CreatePart("RearWindowFrame", PrimitiveType.Cube,
                new Vector3(0, 1.32f, -1.3f),
                new Vector3(1.5f, 0.03f, 0.06f),
                pillarMaterial, pillarColor);

            // C-pillars (rear corners of car)
            CreatePart("LeftCPillar", PrimitiveType.Cube,
                new Vector3(-0.8f, 1.1f, -1.15f),
                new Vector3(0.08f, 0.45f, 0.25f),
                pillarMaterial, pillarColor);

            CreatePart("RightCPillar", PrimitiveType.Cube,
                new Vector3(0.8f, 1.1f, -1.15f),
                new Vector3(0.08f, 0.45f, 0.25f),
                pillarMaterial, pillarColor);

            // Rear side windows (small quarter windows between B and C pillars)
            var leftRearGlass = CreatePart("LeftRearWindow", PrimitiveType.Quad,
                new Vector3(-0.98f, 1.0f, -0.8f),
                new Vector3(0.6f, 0.3f, 1),
                windshieldMaterial, glassColor);
            leftRearGlass.transform.localRotation = Quaternion.Euler(0, 90, 0);
            MakeTransparent(leftRearGlass);

            var rightRearGlass = CreatePart("RightRearWindow", PrimitiveType.Quad,
                new Vector3(0.98f, 1.0f, -0.8f),
                new Vector3(0.6f, 0.3f, 1),
                windshieldMaterial, glassColor);
            rightRearGlass.transform.localRotation = Quaternion.Euler(0, -90, 0);
            MakeTransparent(rightRearGlass);
        }

        private void BuildRoof()
        {
            // Roof (overhead)
            CreatePart("Roof", PrimitiveType.Cube,
                new Vector3(0, 1.4f, -0.2f),
                new Vector3(2.0f, 0.04f, 2.0f),
                pillarMaterial, pillarColor);

            // Headliner (slightly lower, lighter color)
            CreatePart("Headliner", PrimitiveType.Cube,
                new Vector3(0, 1.37f, -0.2f),
                new Vector3(1.85f, 0.02f, 1.85f),
                null, new Color(0.25f, 0.25f, 0.25f));

            // Sun visor (driver)
            CreatePart("SunVisor", PrimitiveType.Cube,
                new Vector3(-0.2f, 1.32f, 0.55f),
                new Vector3(0.3f, 0.01f, 0.18f),
                null, new Color(0.22f, 0.22f, 0.22f));
        }

        private void BuildDoorPanels()
        {
            // Left side wall — solid panel from dashboard to rear, below window line
            // Lower door panel (thick, visible wall)
            CreatePart("LeftDoorPanelLower", PrimitiveType.Cube,
                new Vector3(-0.98f, 0.45f, 0.0f),
                new Vector3(0.08f, 0.6f, 1.6f),
                pillarMaterial, dashColor * 0.9f);

            // Left upper door panel (above armrest, thinner)
            CreatePart("LeftDoorPanelUpper", PrimitiveType.Cube,
                new Vector3(-0.98f, 0.82f, 0.0f),
                new Vector3(0.06f, 0.14f, 1.6f),
                pillarMaterial, dashColor * 0.7f);

            // Left armrest
            CreatePart("LeftArmrest", PrimitiveType.Cube,
                new Vector3(-0.9f, 0.68f, 0.05f),
                new Vector3(0.14f, 0.06f, 0.4f),
                null, dashColor * 1.3f);

            // Left door handle
            CreatePart("LeftDoorHandle", PrimitiveType.Cube,
                new Vector3(-0.9f, 0.72f, 0.15f),
                new Vector3(0.04f, 0.03f, 0.12f),
                null, chromeColor);

            // Right side wall — mirror of left
            CreatePart("RightDoorPanelLower", PrimitiveType.Cube,
                new Vector3(0.98f, 0.45f, 0.0f),
                new Vector3(0.08f, 0.6f, 1.6f),
                pillarMaterial, dashColor * 0.9f);

            CreatePart("RightDoorPanelUpper", PrimitiveType.Cube,
                new Vector3(0.98f, 0.82f, 0.0f),
                new Vector3(0.06f, 0.14f, 1.6f),
                pillarMaterial, dashColor * 0.7f);

            // Right armrest
            CreatePart("RightArmrest", PrimitiveType.Cube,
                new Vector3(0.9f, 0.68f, 0.05f),
                new Vector3(0.14f, 0.06f, 0.4f),
                null, dashColor * 1.3f);

            // Right door handle
            CreatePart("RightDoorHandle", PrimitiveType.Cube,
                new Vector3(0.9f, 0.72f, 0.15f),
                new Vector3(0.04f, 0.03f, 0.12f),
                null, chromeColor);

            // Center console (between front seats)
            CreatePart("CenterConsole", PrimitiveType.Cube,
                new Vector3(0, 0.45f, 0.0f),
                new Vector3(0.22f, 0.22f, 0.5f),
                dashboardMaterial, dashColor);

            // Gear shifter
            CreatePart("GearShifter", PrimitiveType.Capsule,
                new Vector3(0, 0.6f, 0.1f),
                new Vector3(0.03f, 0.06f, 0.03f),
                null, chromeColor);

            // Floor (visible when looking down/back)
            CreatePart("Floor", PrimitiveType.Cube,
                new Vector3(0, 0.12f, -0.2f),
                new Vector3(1.9f, 0.02f, 2.2f),
                null, new Color(0.08f, 0.08f, 0.08f));
        }

        // ---- Helpers ----

        private GameObject CreatePart(string name, PrimitiveType type,
            Vector3 localPos, Vector3 localScale,
            Material mat, Color fallbackColor, Transform parent = null)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent ?? transform);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;

            var renderer = obj.GetComponent<Renderer>();
            if (mat != null)
            {
                renderer.material = mat;
            }
            else
            {
                renderer.material = new Material(
                    Shader.Find("Standard"));
                renderer.material.color = fallbackColor;
            }

            // Remove colliders from cockpit visuals
            var collider = obj.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            return obj;
        }

        private void MakeTransparent(GameObject obj)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;
            var mat = renderer.material;
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }
}
