using UnityEngine;
using HotToMark.Core;
using HotToMark.Audio;
using HotToMark.Haptics;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// Stage 4: Horn honking system.
    /// Player must honk twice before reversing. Tracks honk count,
    /// triggers audio, and auto-shifts to reverse after delay.
    /// Includes timeout to prevent stuck state if player doesn't honk.
    /// </summary>
    public class HornSystem : MonoBehaviour
    {
        [SerializeField] private AudioClip hornClip;

        private GameState state;
        private AudioSource audioSource;
        private bool initialized;
        private bool reverseShiftPending;
        private float honkTimeoutTimer;
        private const float HONK_TIMEOUT = 15f;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void EnsureInit()
        {
            if (initialized) return;
            if (GameManager.Instance == null) return;
            state = GameManager.Instance.state;
            initialized = true;
        }

        void Update()
        {
            EnsureInit();
            if (state == null) return;

            // Track timeout while waiting for honks
            if (state.phase == GamePhase.StoppedOnMark)
            {
                honkTimeoutTimer += Time.deltaTime;
                if (honkTimeoutTimer >= HONK_TIMEOUT)
                {
                    // Auto-advance: player took too long, penalize and shift to reverse
                    state.honkPenalty = true;
                    state.phase = GamePhase.Honking;
                    reverseShiftPending = true;
                    Invoke(nameof(ShiftToReverse), GameState.REVERSE_SHIFT_DELAY);
                }
            }
        }

        /// <summary>
        /// Called when player taps horn (touch UI or keyboard Space).
        /// </summary>
        public void Honk()
        {
            EnsureInit();
            if (state == null) return;
            if (state.phase != GamePhase.StoppedOnMark && state.phase != GamePhase.Honking)
                return;

            state.honkCount++;
            state.phase = GamePhase.Honking;
            honkTimeoutTimer = 0; // Reset timeout on honk

            // Play horn audio
            if (hornClip != null)
            {
                audioSource.PlayOneShot(hornClip);
            }
            else
            {
                var engineAudio = GameManager.Instance.engineAudio;
                if (engineAudio != null) engineAudio.PlayHorn();
            }

            // Haptic feedback
            var haptics = GameManager.Instance.haptics;
            if (haptics != null) haptics.TriggerHorn();

            // After required honks, shift to reverse (prevent duplicate invokes)
            if (state.honkCount >= GameState.HONKS_REQUIRED && !reverseShiftPending)
            {
                reverseShiftPending = true;
                Invoke(nameof(ShiftToReverse), GameState.REVERSE_SHIFT_DELAY);
            }
        }

        private void ShiftToReverse()
        {
            reverseShiftPending = false;
            if (state != null && (state.phase == GamePhase.Honking || state.phase == GamePhase.StoppedOnMark))
            {
                GameManager.Instance?.OnShiftToReverse();
            }
        }

        /// <summary>
        /// Reset state for new game.
        /// </summary>
        public void ResetHorn()
        {
            reverseShiftPending = false;
            honkTimeoutTimer = 0;
            CancelInvoke(nameof(ShiftToReverse));
        }
    }
}
