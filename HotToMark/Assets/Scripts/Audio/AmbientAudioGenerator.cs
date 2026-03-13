using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Audio
{
    /// <summary>
    /// Stage 5/8: Generates ambient film set audio procedurally.
    /// Creates walkie-talkie squelch, crew murmur, and "quiet on set" / "Action!" / "Cut!" calls.
    /// Used as a fallback when no AudioClip assets are available.
    /// </summary>
    public class AmbientAudioGenerator : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource walkieSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Timing")]
        [SerializeField] private float walkieMinInterval = 5f;
        [SerializeField] private float walkieMaxInterval = 15f;
        [SerializeField] private float ambientLoopLength = 4f;

        private float walkieTimer;
        private float nextWalkieTime;
        private GameState state;

        // Cached procedural clips
        private AudioClip ambientMurmurClip;
        private AudioClip walkieSqelchClip;
        private AudioClip engineIdleClip;

        void Awake()
        {
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.volume = 0.06f;
            }

            if (walkieSource == null)
            {
                walkieSource = gameObject.AddComponent<AudioSource>();
                walkieSource.loop = false;
                walkieSource.volume = 0.08f;
                walkieSource.spatialBlend = 0.7f;
            }

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.loop = false;
                voiceSource.volume = 0.3f;
            }

            GenerateClips();
        }

        void Start()
        {
            state = GameManager.Instance.state;
            nextWalkieTime = Random.Range(walkieMinInterval, walkieMaxInterval);
        }

        void Update()
        {
            if (state == null) return;

            if (state.phase == GamePhase.Driving || state.phase == GamePhase.Reversing)
            {
                if (!ambientSource.isPlaying)
                {
                    ambientSource.clip = ambientMurmurClip;
                    ambientSource.Play();
                }

                walkieTimer += Time.deltaTime;
                if (walkieTimer >= nextWalkieTime)
                {
                    walkieSource.PlayOneShot(walkieSqelchClip);
                    walkieTimer = 0;
                    nextWalkieTime = Random.Range(walkieMinInterval, walkieMaxInterval);
                }
            }
            else
            {
                if (ambientSource.isPlaying)
                    ambientSource.Stop();
            }
        }

        private void GenerateClips()
        {
            int sampleRate = AudioSettings.outputSampleRate;

            // Ambient crew murmur: filtered noise
            ambientMurmurClip = GenerateFilteredNoise("AmbientMurmur",
                ambientLoopLength, sampleRate, 200f, 800f);

            // Walkie squelch: short burst of radio static
            walkieSqelchClip = GenerateWalkieSquelch("WalkieSquelch",
                0.4f, sampleRate);

            // Engine idle (used by EngineAudioController as fallback)
            engineIdleClip = GenerateEngineIdle("EngineIdle",
                2f, sampleRate);
        }

        private AudioClip GenerateFilteredNoise(string name, float duration,
            int sampleRate, float lowFreq, float highFreq)
        {
            int length = (int)(sampleRate * duration);
            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            float[] data = new float[length];

            // Simple band-pass filtered noise (murmur-like)
            float prevSample = 0;
            float filterAlpha = lowFreq / sampleRate;
            float filterBeta = 1f - (highFreq / sampleRate);

            for (int i = 0; i < length; i++)
            {
                float noise = Random.Range(-1f, 1f);
                // Low-pass filter
                float filtered = prevSample + filterAlpha * (noise - prevSample);
                prevSample = filtered;

                // Envelope: smooth loop
                float t = (float)i / length;
                float loopEnv = Mathf.Sin(t * Mathf.PI);

                data[i] = filtered * loopEnv * 0.15f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateWalkieSquelch(string name, float duration, int sampleRate)
        {
            int length = (int)(sampleRate * duration);
            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / length;
                // Envelope: quick attack, short sustain, decay
                float env = Mathf.Clamp01(t / 0.02f) *
                           Mathf.Clamp01((1f - t) / 0.1f);

                // Radio static: bandlimited noise with harmonic
                float noise = Random.Range(-1f, 1f);
                float harmonic = Mathf.Sin(2f * Mathf.PI * 2400f * t); // radio tone
                float squelch = Mathf.Sin(2f * Mathf.PI * 3200f * t); // squelch tone

                // Mix: mostly noise, hint of tone
                float mix = noise * 0.6f + harmonic * 0.2f + squelch * 0.1f;
                data[i] = mix * env * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateEngineIdle(string name, float duration, int sampleRate)
        {
            int length = (int)(sampleRate * duration);
            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                float loopT = (float)i / length;
                float loopEnv = 1f - 0.1f * Mathf.Abs(loopT - 0.5f); // subtle pulse

                // V8 idle: fundamental ~35Hz with harmonics
                float fundamental = Mathf.Sin(2f * Mathf.PI * 35f * t);
                float h2 = Mathf.Sin(2f * Mathf.PI * 70f * t) * 0.5f;
                float h3 = Mathf.Sin(2f * Mathf.PI * 105f * t) * 0.3f;
                float h4 = Mathf.Sin(2f * Mathf.PI * 140f * t) * 0.2f;
                float rumble = Random.Range(-0.05f, 0.05f); // mechanical noise

                data[i] = (fundamental + h2 + h3 + h4 + rumble) * loopEnv * 0.2f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Returns the generated engine idle clip for use by EngineAudioController.
        /// </summary>
        public AudioClip GetEngineIdleClip()
        {
            return engineIdleClip;
        }
    }
}
