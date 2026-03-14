using UnityEngine;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Camera
{
    /// <summary>
    /// Stage 8: Rearview mirror using a rear-facing camera + RenderTexture.
    /// Shows actual road behind the car in real-time.
    /// </summary>
    public class RearviewMirror : MonoBehaviour
    {
        [Header("Mirror Camera")]
        [SerializeField] private Vector3 mirrorCameraOffset = new Vector3(0, 1.2f, -0.1f);
        [SerializeField] private float mirrorFOV = 50f;

        [Header("Render Texture")]
        [SerializeField] private RenderTexture mirrorRT;
        [SerializeField] private int renderWidth = 256;
        [SerializeField] private int renderHeight = 96;

        [Header("Display")]
        [SerializeField] private Renderer mirrorRenderer;

        private UnityEngine.Camera mirrorCamera;
        private GameState state;

        void Awake()
        {
            if (mirrorRT == null)
            {
                mirrorRT = new RenderTexture(renderWidth, renderHeight, 16);
                mirrorRT.name = "RearviewMirror_RT";
            }

            // Create rear-facing camera
            var camObj = new GameObject("RearviewCamera");
            camObj.transform.SetParent(transform);
            mirrorCamera = camObj.AddComponent<UnityEngine.Camera>();
            mirrorCamera.targetTexture = mirrorRT;
            mirrorCamera.fieldOfView = mirrorFOV;
            mirrorCamera.transform.localPosition = mirrorCameraOffset;
            mirrorCamera.transform.localRotation = Quaternion.Euler(0, 180, 0); // look backward
            mirrorCamera.depth = -2; // render before main camera
        }

        void LateUpdate()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }

            // Follow the car
            Vector3 carPos = CarController.FeetToWorldPosition(state.posY, state.posX);
            mirrorCamera.transform.position = carPos + mirrorCameraOffset;
        }
    }
}
