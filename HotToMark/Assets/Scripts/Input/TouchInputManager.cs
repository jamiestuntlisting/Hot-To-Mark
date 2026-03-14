using UnityEngine;
using UnityEngine.InputSystem;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Input
{
    /// <summary>
    /// Stage 1: Touch control manager for iOS.
    /// Handles gas/brake pedal zones, steering (tilt or swipe), and horn tap.
    /// Also supports keyboard input for development/testing.
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        [Header("Steering Mode")]
        [SerializeField] private SteeringMode steeringMode = SteeringMode.Tilt;

        [Header("Tilt Sensitivity")]
        [SerializeField] private float tiltDeadzone = 0.05f;
        [SerializeField] private float tiltSensitivity = 3f;

        [Header("Swipe Sensitivity")]
        [SerializeField] private float swipeSensitivity = 0.01f;

        [Header("Touch Zones (screen proportion 0-1)")]
        [Tooltip("Right side of screen = gas")]
        [SerializeField] private float gasBrakeZoneSplit = 0.5f;

        private CarController carController;
        private HornSystem hornSystem;

        private GameState state;
        private float swipeSteerValue;
        private bool initialized;

        public enum SteeringMode { Tilt, Swipe }

        void Start()
        {
            UnityEngine.Input.gyro.enabled = true;
        }

        private void EnsureInit()
        {
            if (initialized) return;
            if (GameManager.Instance == null) return;

            state = GameManager.Instance.state;
            carController = GameManager.Instance.carController;
            hornSystem = GameManager.Instance.hornSystem;
            initialized = true;
        }

        void Update()
        {
            EnsureInit();
            if (state == null) return;

            // Pause toggle (Escape key)
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GameManager.Instance.TogglePause();
                return;
            }

            if (state.phase == GamePhase.Menu || state.phase == GamePhase.Results
                || state.phase == GamePhase.Paused)
                return;

            HandleKeyboardInput();
            HandleTouchInput();
            HandleSteering();
        }

        // ---- Keyboard (development) ----
        private void HandleKeyboardInput()
        {
            // Throttle / brake
            state.throttle = (Keyboard.current != null &&
                (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed))
                ? 1f : 0f;

            state.brake = (Keyboard.current != null &&
                (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed))
                ? 1f : 0f;

            // Keyboard steering
            float kbSteer = 0;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    kbSteer = -1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    kbSteer = 1f;
            }

            if (kbSteer != 0 && carController != null)
                carController.SetSteeringInput(kbSteer);

            // Horn
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (hornSystem != null) hornSystem.Honk();
            }
        }

        // ---- Touch ----
        private void HandleTouchInput()
        {
            if (Touchscreen.current == null) return;

            // Reset each frame — accumulate from active touches
            float touchThrottle = 0;
            float touchBrake = 0;

            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed) continue;

                Vector2 pos = touch.position.ReadValue();
                float screenX = pos.x / Screen.width;
                float screenY = pos.y / Screen.height;

                // Bottom half = pedals
                if (screenY < 0.4f)
                {
                    if (screenX > gasBrakeZoneSplit)
                        touchThrottle = 1f; // Right side = gas
                    else
                        touchBrake = 1f;     // Left side = brake
                }

                // Tap on center-top area = horn
                if (touch.press.wasPressedThisFrame &&
                    screenX > 0.35f && screenX < 0.65f &&
                    screenY > 0.3f && screenY < 0.6f)
                {
                    if (hornSystem != null) hornSystem.Honk();
                }
            }

            // Merge: touch overrides keyboard if active
            if (touchThrottle > 0) state.throttle = touchThrottle;
            if (touchBrake > 0) state.brake = touchBrake;
        }

        private void HandleSteering()
        {
            if (steeringMode == SteeringMode.Tilt)
            {
                HandleTiltSteering();
            }
            else
            {
                HandleSwipeSteering();
            }
        }

        // ---- Tilt-to-steer (accelerometer) ----
        private void HandleTiltSteering()
        {
            // Use accelerometer for tilt steering
            Vector3 accel = UnityEngine.Input.acceleration;
            float tilt = accel.x; // left/right tilt

            if (Mathf.Abs(tilt) < tiltDeadzone) tilt = 0;
            float steerInput = Mathf.Clamp(tilt * tiltSensitivity, -1f, 1f);

            if (carController != null)
                carController.SetSteeringInput(steerInput);
        }

        // ---- Drag-to-steer (swipe on wheel area) ----
        private void HandleSwipeSteering()
        {
            if (Touchscreen.current == null) return;

            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed) continue;

                Vector2 pos = touch.position.ReadValue();
                float screenY = pos.y / Screen.height;

                // Only use touches in the wheel area (bottom-center)
                if (screenY < 0.4f)
                {
                    Vector2 delta = touch.delta.ReadValue();
                    swipeSteerValue += delta.x * swipeSensitivity;
                    swipeSteerValue = Mathf.Clamp(swipeSteerValue, -1f, 1f);
                }
            }

            // Decay toward center when not touching
            swipeSteerValue *= 0.95f;

            if (carController != null)
                carController.SetSteeringInput(swipeSteerValue);
        }
    }
}
