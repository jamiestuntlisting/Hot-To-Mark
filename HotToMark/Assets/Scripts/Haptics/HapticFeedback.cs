using UnityEngine;
using HotToMark.Core;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace HotToMark.Haptics
{
    /// <summary>
    /// Stage 8: Haptic feedback using iOS Core Haptics.
    /// Triggers engine vibration, brake pulse, horn tap, and mark stop feedback.
    /// Falls back to Handheld.Vibrate() on unsupported devices.
    /// </summary>
    public class HapticFeedback : MonoBehaviour
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _TriggerImpactHaptic(int style);

        [DllImport("__Internal")]
        private static extern void _TriggerNotificationHaptic(int type);

        [DllImport("__Internal")]
        private static extern void _TriggerSelectionHaptic();
#endif

        [Header("Settings")]
        [SerializeField] private bool hapticsEnabled = true;
        [SerializeField] private float engineHapticInterval = 0.1f;

        private float lastEngineHapticTime;
        private GameState state;

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }
            if (!hapticsEnabled) return;

            // Continuous engine vibration while driving
            if (state.phase == GamePhase.Driving || state.phase == GamePhase.Reversing)
            {
                float absSpeed = Mathf.Abs(state.speed);
                if ((absSpeed > 5f || state.throttle > 0) &&
                    Time.time - lastEngineHapticTime > engineHapticInterval)
                {
                    TriggerLight();
                    lastEngineHapticTime = Time.time;
                }
            }
        }

        public void TriggerHorn()
        {
            TriggerMedium();
        }

        public void TriggerBrake()
        {
            TriggerHeavy();
        }

        public void TriggerMark()
        {
            // Double buzz for stopping on mark
            TriggerHeavy();
            Invoke(nameof(TriggerHeavy), 0.1f);
            Invoke(nameof(TriggerMedium), 0.25f);
        }

        public void TriggerRumble()
        {
            // Road edge rumble
            TriggerMedium();
            Invoke(nameof(TriggerLight), 0.08f);
            Invoke(nameof(TriggerMedium), 0.16f);
        }

        // ---- Platform abstraction ----

        private void TriggerLight()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(0); // UIImpactFeedbackStyleLight
#elif UNITY_ANDROID && !UNITY_EDITOR
            // Android: short vibration
            Handheld.Vibrate();
#endif
        }

        private void TriggerMedium()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(1); // UIImpactFeedbackStyleMedium
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private void TriggerHeavy()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(2); // UIImpactFeedbackStyleHeavy
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
