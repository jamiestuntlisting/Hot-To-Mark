using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.UI;

namespace HotToMark.Camera
{
    /// <summary>
    /// Stage 6: Picture-in-Picture camera view.
    /// Shows the film camera's wide-angle perspective of the stunt.
    /// Self-builds its camera and finds its UI overlay on the canvas.
    /// </summary>
    public class PIPCameraController : MonoBehaviour
    {
        [Header("Film Camera Position")]
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 2.5f, 28f);
        [SerializeField] private float fieldOfView = 40f;

        [Header("Render Texture (4:3)")]
        [SerializeField] private int renderWidth = 400;
        [SerializeField] private int renderHeight = 300;

        [Header("REC Blink")]
        [SerializeField] private float blinkInterval = 0.5f;

        private UnityEngine.Camera pipCamera;
        private RenderTexture renderTexture;
        private GameState state;
        private float blinkTimer;
        private bool recVisible = true;
        private bool initialized;

        // UI references (found at runtime)
        private GameObject pipPanel;
        private RawImage pipDisplay;
        private TextMeshProUGUI timecodeText;
        private GameObject recDot;

        void Awake()
        {
            // Create render texture
            renderTexture = new RenderTexture(renderWidth, renderHeight, 16);
            renderTexture.name = "PIP_RT";

            // Create the film camera
            var camObj = new GameObject("FilmCamera");
            camObj.transform.SetParent(transform);
            pipCamera = camObj.AddComponent<UnityEngine.Camera>();
            pipCamera.targetTexture = renderTexture;
            pipCamera.fieldOfView = fieldOfView;
            pipCamera.transform.localPosition = cameraPosition;
            pipCamera.transform.localRotation = Quaternion.Euler(5, 180, 0);
            pipCamera.depth = -1;
            // Remove auto-added AudioListener (only MainCamera should have one)
            var listener = camObj.GetComponent<AudioListener>();
            if (listener != null) Destroy(listener);
        }

        void Update()
        {
            EnsureInit();
            if (state == null) return;

            // Timecode (MM:SS:FF at 24fps)
            if (timecodeText != null && state.takeStartTime > 0)
            {
                float elapsed = Time.time - state.takeStartTime;
                int mm = (int)(elapsed / 60f);
                int ss = (int)(elapsed % 60f);
                int ff = (int)((elapsed % 1f) * 24f);
                timecodeText.text = $"{mm:D2}:{ss:D2}:{ff:D2}";
            }

            // Blinking REC dot
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0;
                recVisible = !recVisible;
                if (recDot != null) recDot.SetActive(recVisible);
            }

            // Position film camera near the mark
            if (state.markDistance > 0)
            {
                float markWorldZ = state.markDistance * 0.3f;
                pipCamera.transform.localPosition = new Vector3(
                    6f, 2.5f, markWorldZ * 0.8f);
                // Look toward the mark
                pipCamera.transform.LookAt(
                    new Vector3(0, 0.5f, markWorldZ));
            }
        }

        private void EnsureInit()
        {
            if (initialized) return;
            if (GameManager.Instance == null) return;

            state = GameManager.Instance.state;

            // Find UI elements by name (built by SceneBootstrap)
            var panel = GameObject.Find("PIPPanel");
            if (panel != null)
            {
                pipPanel = panel;

                var display = panel.GetComponentInChildren<RawImage>();
                if (display != null)
                {
                    pipDisplay = display;
                    pipDisplay.texture = renderTexture;
                }

                // Find timecode text
                var timecodeObj = FindChildByName(panel.transform, "Timecode");
                if (timecodeObj != null)
                    timecodeText = timecodeObj.GetComponent<TextMeshProUGUI>();

                // Find REC dot
                var recObj = FindChildByName(panel.transform, "RECDot");
                if (recObj != null)
                    recDot = recObj.gameObject;
            }

            initialized = true;
        }

        private Transform FindChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindChildByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        public void SetActive(bool active)
        {
            if (pipPanel != null) pipPanel.SetActive(active);
            if (pipCamera != null) pipCamera.enabled = active;
            gameObject.SetActive(active);
        }
    }
}
