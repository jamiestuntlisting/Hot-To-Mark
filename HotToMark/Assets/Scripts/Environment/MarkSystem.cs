using UnityEngine;
using TMPro;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Environment
{
    /// <summary>
    /// Stage 3: The Mark (target) and the "ONE" return mark.
    /// Places red tape on the road, shows MARK label, handles threshold color changes.
    /// Also manages checkpoint marker for Exact MPH mode.
    /// </summary>
    public class MarkSystem : MonoBehaviour
    {
        [Header("Mark Visuals")]
        [SerializeField] private float markWidth = 8f;      // road width
        [SerializeField] private float markThickness = 0.1f;
        [SerializeField] private Color markColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color markNearColor = new Color(1f, 0.1f, 0.1f);
        [SerializeField] private Color oneColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color checkpointColor = new Color(1f, 0.7f, 0f);

        [Header("Label")]
        [SerializeField] private float labelHeight = 2f;
        [SerializeField] private float labelFontSize = 3f;

        private GameState state;
        private float worldScale = 0.3f;

        // Runtime objects
        private GameObject markStripe;
        private GameObject markLabel;
        private GameObject oneStripe;
        private GameObject oneLabel;
        private GameObject checkpointStripe;
        private GameObject checkpointLabel;
        private TextMeshPro markTMP;
        private TextMeshPro oneTMP;
        private TextMeshPro checkpointTMP;

        void Start()
        {
            CreateMarkObjects();
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }
            UpdateMarkVisibility();
            UpdateThresholdColor();
        }

        public void SetupMark(float distanceFeet)
        {
            // Ensure state is available
            if (state == null && GameManager.Instance != null)
                state = GameManager.Instance.state;

            float worldZ = distanceFeet * worldScale;

            // Position the mark stripe
            if (markStripe != null)
            {
                markStripe.transform.localPosition = new Vector3(0, 0.03f, worldZ);
                markStripe.SetActive(true);
            }
            if (markLabel != null)
            {
                markLabel.transform.localPosition = new Vector3(0, labelHeight, worldZ);
                markLabel.SetActive(true);
            }

            // Position ONE at start
            if (oneStripe != null)
            {
                oneStripe.transform.localPosition = new Vector3(0, 0.03f, 0);
                oneStripe.SetActive(false); // shown only during reverse
            }
            if (oneLabel != null)
            {
                oneLabel.transform.localPosition = new Vector3(0, labelHeight, 0);
                oneLabel.SetActive(false);
            }

            // Checkpoint for Exact MPH
            if (state != null && state.mode == GameMode.ExactMPH)
            {
                float cpZ = state.checkpointDistance * worldScale;
                if (checkpointStripe != null)
                {
                    checkpointStripe.transform.localPosition = new Vector3(0, 0.025f, cpZ);
                    checkpointStripe.SetActive(true);
                }
                if (checkpointLabel != null)
                {
                    checkpointLabel.transform.localPosition = new Vector3(0, labelHeight * 0.8f, cpZ);
                    checkpointLabel.SetActive(true);
                }
            }
            else
            {
                if (checkpointStripe != null) checkpointStripe.SetActive(false);
                if (checkpointLabel != null) checkpointLabel.SetActive(false);
            }
        }

        private void UpdateMarkVisibility()
        {
            bool driving = state.phase == GamePhase.Driving
                        || state.phase == GamePhase.StoppedOnMark
                        || state.phase == GamePhase.Honking;

            bool reversing = state.phase == GamePhase.Reversing;

            if (markStripe != null) markStripe.SetActive(driving);
            if (markLabel != null) markLabel.SetActive(driving);
            if (oneStripe != null) oneStripe.SetActive(reversing);
            if (oneLabel != null) oneLabel.SetActive(reversing);

            // Hide checkpoint after passing
            if (state.checkpointPassed)
            {
                if (checkpointStripe != null) checkpointStripe.SetActive(false);
                if (checkpointLabel != null) checkpointLabel.SetActive(false);
            }
        }

        private void UpdateThresholdColor()
        {
            if (markStripe == null || state.phase != GamePhase.Driving) return;

            float distToMark = state.markDistance - state.posY;
            float threshold = state.markDistance * GameState.MARK_THRESHOLD_PCT;
            bool nearMark = Mathf.Abs(distToMark) < threshold;

            var renderer = markStripe.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = nearMark ? markNearColor : markColor;
        }

        private void CreateMarkObjects()
        {
            // Red MARK stripe
            markStripe = CreateStripe("MarkStripe", markColor);
            markLabel = CreateLabel("MarkLabel", "MARK", markColor);
            markTMP = markLabel.GetComponent<TextMeshPro>();

            // Green ONE stripe
            oneStripe = CreateStripe("OneStripe", oneColor);
            oneLabel = CreateLabel("OneLabel", "ONE", oneColor);
            oneTMP = oneLabel.GetComponent<TextMeshPro>();

            // Orange CHECKPOINT stripe
            checkpointStripe = CreateStripe("CheckpointStripe", checkpointColor);
            checkpointStripe.transform.localScale = new Vector3(markWidth, 0.02f, markThickness * 0.7f);
            checkpointLabel = CreateLabel("CheckpointLabel", "CHECKPOINT", checkpointColor);
            checkpointTMP = checkpointLabel.GetComponent<TextMeshPro>();
            if (checkpointTMP != null) checkpointTMP.fontSize = labelFontSize * 0.7f;

            // All start hidden
            markStripe.SetActive(false);
            markLabel.SetActive(false);
            oneStripe.SetActive(false);
            oneLabel.SetActive(false);
            checkpointStripe.SetActive(false);
            checkpointLabel.SetActive(false);
        }

        private GameObject CreateStripe(string name, Color color)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = name;
            stripe.transform.SetParent(transform);
            stripe.transform.localScale = new Vector3(markWidth, 0.02f, markThickness);

            var renderer = stripe.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = color;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", color * 0.5f);

            Destroy(stripe.GetComponent<Collider>());
            return stripe;
        }

        private GameObject CreateLabel(string name, string text, Color color)
        {
            var labelObj = new GameObject(name);
            labelObj.transform.SetParent(transform);

            var tmp = labelObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = labelFontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            // Billboard: always face camera
            var billboard = labelObj.AddComponent<BillboardLabel>();

            return labelObj;
        }
    }

    /// <summary>
    /// Simple billboard behavior to keep labels facing the camera.
    /// </summary>
    public class BillboardLabel : MonoBehaviour
    {
        private UnityEngine.Camera cachedCam;

        void LateUpdate()
        {
            if (cachedCam == null) cachedCam = UnityEngine.Camera.main;
            if (cachedCam != null)
            {
                transform.LookAt(cachedCam.transform);
                transform.Rotate(0, 180, 0);
            }
        }
    }
}
