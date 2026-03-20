using UnityEngine;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Camera
{
    /// <summary>
    /// First-person cockpit camera.
    /// Positioned at driver's eye level, follows the car rigidly.
    /// Subtle acceleration lean, gentle side sway, minimal vertical bob.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CockpitCamera : MonoBehaviour
    {
        [Header("Eye Position (relative to car)")]
        [SerializeField] private Vector3 eyeOffset = new Vector3(0, 1.15f, 0.2f);

        [Header("Motion - Vertical Bob (very subtle)")]
        [SerializeField] private float speedBobAmount = 0.0004f;
        [SerializeField] private float speedBobFrequency = 2.5f;

        [Header("Motion - Acceleration Lean")]
        [SerializeField] private float accelLeanAmount = 0.03f;
        [SerializeField] private float accelLeanSmoothing = 3f;

        [Header("Motion - Side Sway")]
        [SerializeField] private float sideSwayAmount = 0.02f;
        [SerializeField] private float sideSwayFrequency = 0.7f;

        [Header("FOV")]
        [SerializeField] private float baseFOV = 65f;
        [SerializeField] private float maxFOVIncrease = 8f;

        private GameState state;
        private CarController car;
        private UnityEngine.Camera cam;
        private float bobTimer;
        private float swayTimer;
        private float smoothedAccelLean;
        private float lastSpeed;

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

            // Rigidly follow car position
            Vector3 carWorldPos = CarController.FeetToWorldPosition(state.posY, state.posX);
            Vector3 targetPos = carWorldPos + eyeOffset;

            float absSpeed = Mathf.Abs(state.speed);
            float dt = Time.deltaTime;

            // Acceleration lean — shift backward when accelerating, forward when braking
            float speedDelta = state.speed - lastSpeed;
            lastSpeed = state.speed;
            float targetLean = -speedDelta * accelLeanAmount * 60f; // scale for frame independence
            smoothedAccelLean = Mathf.Lerp(smoothedAccelLean, targetLean, dt * accelLeanSmoothing);
            targetPos.z += smoothedAccelLean;
            // Subtle vertical component — head dips slightly under acceleration
            targetPos.y += smoothedAccelLean * 0.3f;

            // Very subtle vertical bob (road texture feel, not bouncy)
            if (absSpeed > 3f)
            {
                bobTimer += dt * speedBobFrequency * Mathf.Sqrt(absSpeed / 20f);
                targetPos.y += Mathf.Sin(bobTimer) * speedBobAmount * absSpeed;
            }

            // Gentle side-to-side sway (natural body movement)
            if (absSpeed > 2f)
            {
                swayTimer += dt * sideSwayFrequency;
                targetPos.x += Mathf.Sin(swayTimer) * sideSwayAmount;
            }

            // Steering head turn — slight lateral shift
            targetPos.x += state.steering * 0.08f;

            transform.position = targetPos;

            // Look forward (or backward when reversing)
            Vector3 lookDir = state.gear == Gear.Reverse ? Vector3.back : Vector3.forward;
            lookDir.x += Mathf.Clamp(state.steering * 0.08f, -0.12f, 0.12f);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDir), dt * 8f);

            // Dynamic FOV based on speed
            float fovTarget = baseFOV + (absSpeed / GameState.MAX_FORWARD_MPH) * maxFOVIncrease;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovTarget, dt * 3f);
        }
    }
}
