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
    /// </summary>
    public class HornSystem : MonoBehaviour
    {
        [SerializeField] private AudioClip hornClip;

        private GameState state;
        private AudioSource audioSource;

        void Start()
        {
            state = GameManager.Instance.state;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Called when player taps horn (touch UI or keyboard Space).
        /// </summary>
        public void Honk()
        {
            if (state.phase != GamePhase.StoppedOnMark && state.phase != GamePhase.Honking)
                return;

            state.honkCount++;
            state.phase = GamePhase.Honking;

            // Play horn audio
            if (hornClip != null)
            {
                audioSource.PlayOneShot(hornClip);
            }
            else
            {
                // Fallback: use EngineAudioController's generated horn
                var engineAudio = GameManager.Instance.engineAudio;
                if (engineAudio != null) engineAudio.PlayHorn();
            }

            // Haptic feedback
            var haptics = GameManager.Instance.haptics;
            if (haptics != null) haptics.TriggerHorn();

            // After required honks, shift to reverse
            if (state.honkCount >= GameState.HONKS_REQUIRED)
            {
                Invoke(nameof(ShiftToReverse), GameState.REVERSE_SHIFT_DELAY);
            }
        }

        private void ShiftToReverse()
        {
            if (state.phase == GamePhase.Honking)
            {
                GameManager.Instance.OnShiftToReverse();
            }
        }
    }
}
