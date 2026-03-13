using UnityEngine;
using TMPro;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Camera
{
    /// <summary>
    /// Stage 6: Picture-in-Picture camera view.
    /// Shows the film camera's wide-angle perspective of the stunt.
    /// Renders to a RenderTexture displayed in the upper-right UI.
    /// </summary>
    public class PIPCameraController : MonoBehaviour
    {
        [Header("Film Camera Position")]
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 2.5f, 28f);
        [SerializeField] private float fieldOfView = 40f;

        [Header("Render Texture")]
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private int renderWidth = 420;
        [SerializeField] private int renderHeight = 280;

        [Header("UI Elements")]
        [SerializeField] private GameObject pipPanel;
        [SerializeField] private UnityEngine.UI.RawImage pipDisplay;
        [SerializeField] private TextMeshProUGUI timecodeText;
        [SerializeField] private GameObject recDot;
        [SerializeField] private TextMeshProUGUI cameraLabel;

        [Header("REC Blink")]
        [SerializeField] private float blinkInterval = 0.5f;

        private UnityEngine.Camera pipCamera;
        private GameState state;
        private float blinkTimer;
        private bool recVisible = true;

        void Awake()
        {
            // Create render texture if not assigned
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(renderWidth, renderHeight, 16);
                renderTexture.name = "PIP_RT";
            }

            // Create or configure the PIP camera
            pipCamera = GetComponentInChildren<UnityEngine.Camera>();
            if (pipCamera == null)
            {
                var camObj = new GameObject("PIPCamera");
                camObj.transform.SetParent(transform);
                pipCamera = camObj.AddComponent<UnityEngine.Camera>();
            }

            pipCamera.targetTexture = renderTexture;
            pipCamera.fieldOfView = fieldOfView;
            pipCamera.transform.localPosition = cameraPosition;
            pipCamera.transform.localRotation = Quaternion.Euler(5, 180, 0); // looking back at the action

            // Wire up the display
            if (pipDisplay != null)
                pipDisplay.texture = renderTexture;
        }

        void Start()
        {
            state = GameManager.Instance.state;

            if (cameraLabel != null)
                cameraLabel.text = "CAM A - WIDE";
        }

        void Update()
        {
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

            // Point film camera at the mark position
            float markWorldZ = state.markDistance * 0.3f;
            pipCamera.transform.localPosition = new Vector3(0, 2.5f, markWorldZ + 5f);
        }

        public void SetActive(bool active)
        {
            if (pipPanel != null) pipPanel.SetActive(active);
            if (pipCamera != null) pipCamera.enabled = active;
        }
    }
}
