using UnityEngine;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Camera
{
    /// <summary>
    /// Stage 1: First-person cockpit camera.
    /// Positioned at the driver's eye level, looking through the windshield.
    /// Follows the car's lateral movement and adds subtle motion.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CockpitCamera : MonoBehaviour
    {
        [Header("Eye Position (relative to car)")]
        [SerializeField] private Vector3 eyeOffset = new Vector3(0, 1.15f, 0.2f);

        [Header("Motion")]
        [SerializeField] private float lateralShiftAmount = 0.15f;
        [SerializeField] private float speedBobAmount = 0.002f;
        [SerializeField] private float speedBobFrequency = 3f;

        [Header("Interior Bounds (keep driver inside car)")]
        [SerializeField] private float maxLateralOffset = 0.35f;
        [SerializeField] private float maxVerticalOffset = 0.15f;
        [SerializeField] private float maxForwardOffset = 0.25f;

        [Header("FOV")]
        [SerializeField] private float baseFOV = 65f;
        [SerializeField] private float maxFOVIncrease = 8f;

        private GameState state;
        private CarController car;
        private UnityEngine.Camera cam;
        private float bobTimer;

        void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
            cam.fieldOfView = baseFOV;
        }

        void LateUpdate()
        {
            if (state == null || car == null)
            {
                if (GameManager.Instance != null)
                {
                    state = GameManager.Instance.state;
                    car = GameManager.Instance.carController;
                }
                return;
            }

            // Rigidly follow car position — camera is inside the car
            Vector3 carWorldPos = CarController.FeetToWorldPosition(state.posY, state.posX);
            Vector3 targetPos = carWorldPos + eyeOffset;

            // Subtle lateral shift from steering
            targetPos.x += state.steering * lateralShiftAmount;

            // Speed bob (road vibration feel)
            float absSpeed = Mathf.Abs(state.speed);
            if (absSpeed > 2f)
            {
                bobTimer += Time.deltaTime * speedBobFrequency * (absSpeed / 30f);
                targetPos.y += Mathf.Sin(bobTimer) * speedBobAmount * absSpeed;
            }

            // Lock camera directly to car — no lerp delay
            transform.position = targetPos;

            // Look forward (or backward when reversing)
            Vector3 lookDir = state.gear == Gear.Reverse ? Vector3.back : Vector3.forward;
            lookDir.x += Mathf.Clamp(state.steering * 0.1f, -0.15f, 0.15f);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDir), Time.deltaTime * 8f);

            // Dynamic FOV based on speed
            float fovTarget = baseFOV + (absSpeed / GameState.MAX_FORWARD_MPH) * maxFOVIncrease;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovTarget, Time.deltaTime * 3f);
        }
    }
}
