using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Audio
{
    /// <summary>
    /// F-9: Walkie-talkie communications — realistic film set radio chatter.
    /// Plays random AD/crew radio calls during gameplay for atmosphere.
    /// Each call has a squelch-in, voice, squelch-out pattern.
    /// </summary>
    public class WalkieTalkieComms : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float minInterval = 8f;
        [SerializeField] private float maxInterval = 20f;
        [SerializeField] private float volume = 0.12f;

        private AudioSource source;
        private GameState state;
        private float nextCallTime;

        // Pre-generated radio calls
        private AudioClip[] radioClips;
        private AudioClip squelchIn;
        private AudioClip squelchOut;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.volume = volume;
            source.spatialBlend = 0.5f; // partially spatial

            GenerateClips();
            ScheduleNext();
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }

            // Only play during active gameplay phases
            if (state.phase != GamePhase.PreRoll && state.phase != GamePhase.Driving
                && state.phase != GamePhase.Reversing && state.phase != GamePhase.StoppedOnMark
                && state.phase != GamePhase.Honking)
                return;

            if (Time.time >= nextCallTime && !source.isPlaying)
            {
                PlayRandomCall();
                ScheduleNext();
            }
        }

        private void ScheduleNext()
        {
            nextCallTime = Time.time + Random.Range(minInterval, maxInterval);
        }

        private void PlayRandomCall()
        {
            if (radioClips == null || radioClips.Length == 0) return;
            int idx = Random.Range(0, radioClips.Length);
            source.PlayOneShot(radioClips[idx], volume);
        }

        private void GenerateClips()
        {
            int sr = AudioSettings.outputSampleRate;

            // Generate several distinct radio call patterns
            radioClips = new AudioClip[]
            {
                GenerateRadioCall(sr, RadioCall.Copy),        // "Copy"
                GenerateRadioCall(sr, RadioCall.StandBy),     // "Stand by"
                GenerateRadioCall(sr, RadioCall.GoForIt),     // "Go for it"
                GenerateRadioCall(sr, RadioCall.CheckCheck),  // "Check check"
                GenerateRadioCall(sr, RadioCall.TenFour),     // "10-4"
                GenerateRadioCall(sr, RadioCall.HoldPosition),// "Hold position"
            };
        }

        private enum RadioCall { Copy, StandBy, GoForIt, CheckCheck, TenFour, HoldPosition }

        private AudioClip GenerateRadioCall(int sr, RadioCall call)
        {
            // Structure: squelch-in (0.08s) + voice (varies) + squelch-out (0.06s)
            float voiceDuration;
            float f0;
            string name;

            switch (call)
            {
                case RadioCall.Copy:        voiceDuration = 0.35f; f0 = 145f; name = "RadioCopy"; break;
                case RadioCall.StandBy:     voiceDuration = 0.55f; f0 = 140f; name = "RadioStandBy"; break;
                case RadioCall.GoForIt:     voiceDuration = 0.5f;  f0 = 150f; name = "RadioGoForIt"; break;
                case RadioCall.CheckCheck:  voiceDuration = 0.6f;  f0 = 138f; name = "RadioCheckCheck"; break;
                case RadioCall.TenFour:     voiceDuration = 0.5f;  f0 = 142f; name = "RadioTenFour"; break;
                default:                    voiceDuration = 0.65f; f0 = 135f; name = "RadioHold"; break;
            }

            float squelchDur = 0.14f;
            float totalDuration = squelchDur + voiceDuration + squelchDur;
            int length = (int)(sr * totalDuration);
            var clip = AudioClip.Create(name, length, 1, sr, false);
            float[] data = new float[length];

            int squelchInEnd = (int)(sr * squelchDur * 0.5f);
            int voiceStart = (int)(sr * squelchDur);
            int voiceEnd = voiceStart + (int)(sr * voiceDuration);
            int squelchOutStart = voiceEnd;

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float sample = 0;

                if (i < squelchInEnd)
                {
                    // Squelch-in: radio static burst
                    float env = Mathf.Clamp01((float)i / squelchInEnd);
                    sample = Random.Range(-1f, 1f) * 0.3f * env;
                    sample += Mathf.Sin(2f * Mathf.PI * 2800f * t) * 0.1f * env;
                }
                else if (i >= voiceStart && i < voiceEnd)
                {
                    // Voice with radio filter
                    float vt = (float)(i - voiceStart) / (voiceEnd - voiceStart);
                    float voiceT = (float)(i - voiceStart) / sr;

                    float env = GetRadioEnvelope(call, vt);

                    // Vocal fundamental with radio compression
                    float pitch = f0 * (1f + 0.05f * Mathf.Sin(2f * Mathf.PI * 6f * voiceT));
                    float glottal = 0;
                    for (int h = 1; h <= 5; h++)
                        glottal += Mathf.Sin(2f * Mathf.PI * pitch * h * voiceT) / h;
                    glottal *= 0.2f;

                    // Radio bandpass character (telephone quality)
                    float radioTone = Mathf.Sin(2f * Mathf.PI * 800f * voiceT) * 0.15f;
                    float radioHigh = Mathf.Sin(2f * Mathf.PI * 2200f * voiceT) * 0.05f;

                    // Subtle static underneath
                    float staticNoise = Random.Range(-0.03f, 0.03f);

                    sample = (glottal + radioTone + radioHigh + staticNoise) * env;
                }
                else if (i >= squelchOutStart)
                {
                    // Squelch-out: quick static burst then silence
                    float progress = (float)(i - squelchOutStart) / (length - squelchOutStart);
                    float env = 1f - progress;
                    sample = Random.Range(-1f, 1f) * 0.2f * env * env;
                    sample += Mathf.Sin(2f * Mathf.PI * 3000f * t) * 0.08f * env;
                }

                data[i] = sample;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private float GetRadioEnvelope(RadioCall call, float tNorm)
        {
            switch (call)
            {
                case RadioCall.Copy:
                    // Single syllable
                    return Gaussian(tNorm, 0.45f, 0.25f) * 0.8f;

                case RadioCall.StandBy:
                    // Two syllables
                    return (Gaussian(tNorm, 0.25f, 0.12f) * 0.7f +
                            Gaussian(tNorm, 0.7f, 0.15f) * 0.9f) * 0.8f;

                case RadioCall.GoForIt:
                    // Three syllables
                    return (Gaussian(tNorm, 0.15f, 0.08f) * 0.7f +
                            Gaussian(tNorm, 0.45f, 0.1f) * 0.6f +
                            Gaussian(tNorm, 0.75f, 0.12f) * 0.8f) * 0.8f;

                case RadioCall.CheckCheck:
                    // Two repeated syllables
                    return (Gaussian(tNorm, 0.25f, 0.1f) * 0.8f +
                            Gaussian(tNorm, 0.65f, 0.1f) * 0.8f) * 0.8f;

                case RadioCall.TenFour:
                    // Two syllables, second emphasized
                    return (Gaussian(tNorm, 0.3f, 0.1f) * 0.6f +
                            Gaussian(tNorm, 0.7f, 0.15f) * 0.9f) * 0.8f;

                default: // HoldPosition
                    // Two syllables
                    return (Gaussian(tNorm, 0.2f, 0.1f) * 0.8f +
                            Gaussian(tNorm, 0.6f, 0.2f) * 0.7f) * 0.8f;
            }
        }

        private static float Gaussian(float x, float mean, float sigma)
        {
            float d = (x - mean) / sigma;
            return Mathf.Exp(-0.5f * d * d);
        }
    }
}
