using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Audio
{
    /// <summary>
    /// Stage 8: Engine audio system with RPM-based pitch.
    /// Handles idle, acceleration, deceleration sounds.
    /// All audio is generated procedurally as fallback when no AudioClips are assigned.
    /// Includes horn, tire screech, and "Action!"/"Cut!" voice calls.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EngineAudioController : MonoBehaviour
    {
        [Header("Engine Audio")]
        [SerializeField] private AudioClip engineIdleClip;
        [SerializeField] private float idlePitch = 0.6f;
        [SerializeField] private float maxPitch = 2.5f;
        [SerializeField] private float idleVolume = 0.15f;
        [SerializeField] private float maxVolume = 0.5f;

        [Header("Horn")]
        [SerializeField] private AudioClip hornClip;

        [Header("Tire Screech")]
        [SerializeField] private AudioClip tireScreechClip;
        [SerializeField] private float screechSpeedThreshold = 15f;
        [SerializeField] private float screechCooldown = 0.3f;

        [Header("Voice Calls")]
        [SerializeField] private AudioClip actionVoiceClip;
        [SerializeField] private AudioClip cutVoiceClip;

        private AudioSource engineSource;
        private AudioSource sfxSource;
        private GameState state;
        private float lastScreechTime;
        private bool engineRunning;

        // Cached procedural clips
        private AudioClip generatedIdleClip;
        private AudioClip generatedHornClip;
        private AudioClip generatedScreechClip;
        private AudioClip generatedActionClip;
        private AudioClip generatedCutClip;

        void Awake()
        {
            engineSource = GetComponent<AudioSource>();
            engineSource.loop = true;
            engineSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            GenerateAllClips();
        }

        void Start()
        {
            state = GameManager.Instance.state;
        }

        void Update()
        {
            if (!engineRunning || state == null) return;

            UpdateEnginePitch();
            CheckTireScreech();
        }

        private void GenerateAllClips()
        {
            int sr = AudioSettings.outputSampleRate;
            generatedIdleClip = GenerateEngineIdle(sr);
            generatedHornClip = GenerateHorn(sr);
            generatedScreechClip = GenerateTireScreech(sr);
            generatedActionClip = GenerateVoiceCall(sr, "action");
            generatedCutClip = GenerateVoiceCall(sr, "cut");
        }

        private void UpdateEnginePitch()
        {
            float absSpeed = Mathf.Abs(state.speed);
            float speedNorm = absSpeed / GameState.MAX_FORWARD_MPH;
            float throttleBoost = state.throttle * 0.3f;

            float targetPitch = Mathf.Lerp(idlePitch, maxPitch, speedNorm + throttleBoost);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * 5f);

            float targetVol = Mathf.Lerp(idleVolume, maxVolume,
                speedNorm * 0.7f + state.throttle * 0.3f);
            engineSource.volume = Mathf.Lerp(engineSource.volume, targetVol, Time.deltaTime * 5f);
        }

        private void CheckTireScreech()
        {
            if (state.brake > 0.5f && Mathf.Abs(state.speed) > screechSpeedThreshold)
            {
                if (Time.time - lastScreechTime > screechCooldown)
                {
                    PlayTireScreech();
                    lastScreechTime = Time.time;
                }
            }
        }

        public void StartEngine()
        {
            engineRunning = true;

            var clip = engineIdleClip != null ? engineIdleClip : generatedIdleClip;
            if (clip != null)
            {
                engineSource.clip = clip;
                engineSource.pitch = idlePitch;
                engineSource.volume = idleVolume;
                engineSource.Play();
            }
        }

        public void StopEngine()
        {
            engineRunning = false;
            engineSource.Stop();
        }

        public void PlayHorn()
        {
            var clip = hornClip != null ? hornClip : generatedHornClip;
            sfxSource.PlayOneShot(clip, 0.6f);
        }

        public void PlayTireScreech()
        {
            var clip = tireScreechClip != null ? tireScreechClip : generatedScreechClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.4f);
        }

        public void PlayVoiceAction()
        {
            var clip = actionVoiceClip != null ? actionVoiceClip : generatedActionClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.7f);
        }

        public void PlayVoiceCut()
        {
            var clip = cutVoiceClip != null ? cutVoiceClip : generatedCutClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.7f);
        }

        // ---- Procedural Audio Generation ----

        /// <summary>
        /// V8 engine idle: fundamental ~35Hz with harmonics, subtle rumble variation.
        /// </summary>
        private AudioClip GenerateEngineIdle(int sr)
        {
            float duration = 2f;
            int length = (int)(sr * duration);
            var clip = AudioClip.Create("EngineIdle", length, 1, sr, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float loopT = (float)i / length;

                // Cylinder firing frequency ~35Hz
                float f1 = Mathf.Sin(2f * Mathf.PI * 35f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 70f * t) * 0.5f;
                float f3 = Mathf.Sin(2f * Mathf.PI * 105f * t) * 0.3f;
                float f4 = Mathf.Sin(2f * Mathf.PI * 140f * t) * 0.2f;
                float f5 = Mathf.Sin(2f * Mathf.PI * 210f * t) * 0.1f;

                // Subtle RPM variation (cam lope)
                float lope = 1f + 0.15f * Mathf.Sin(2f * Mathf.PI * 2.5f * t);

                // Mechanical noise
                float noise = (Random.value - 0.5f) * 0.06f;

                // Seamless loop envelope
                float env = Mathf.SmoothStep(0, 1, Mathf.Min(loopT * 20f, (1f - loopT) * 20f));

                data[i] = (f1 + f2 + f3 + f4 + f5 + noise) * lope * env * 0.2f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Dual-tone car horn: ~380 Hz + ~507 Hz, 0.35 second burst.
        /// </summary>
        private AudioClip GenerateHorn(int sr)
        {
            float duration = 0.35f;
            int length = (int)(sr * duration);
            var clip = AudioClip.Create("Horn", length, 1, sr, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float attack = Mathf.Clamp01(t / 0.015f);
                float decay = Mathf.Clamp01((duration - t) / 0.05f);
                float env = attack * decay;

                float tone1 = Mathf.Sin(2f * Mathf.PI * 380f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 507f * t);
                float tone3 = Mathf.Sin(2f * Mathf.PI * 634f * t) * 0.15f;

                data[i] = (tone1 * 0.45f + tone2 * 0.35f + tone3) * env * 0.5f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Tire screech: filtered noise with resonant frequency sweep.
        /// </summary>
        private AudioClip GenerateTireScreech(int sr)
        {
            float duration = 0.6f;
            int length = (int)(sr * duration);
            var clip = AudioClip.Create("TireScreech", length, 1, sr, false);
            float[] data = new float[length];

            float prev = 0;
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Clamp01(t / 0.02f) * Mathf.Clamp01((duration - t) / 0.15f);

                // White noise, band-filtered
                float noise = Random.Range(-1f, 1f);

                // Resonant frequency sweeps down (tire losing grip)
                float freq = Mathf.Lerp(3200f, 1800f, t / duration);
                float resonance = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f;

                // Simple low-pass
                float alpha = 0.15f;
                float filtered = prev + alpha * (noise - prev);
                prev = filtered;

                data[i] = (filtered * 0.7f + resonance) * env * 0.35f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Generates a synthesized voice-like call ("Action!" or "Cut!").
        /// Uses formant synthesis with vowel shaping to approximate speech.
        /// </summary>
        private AudioClip GenerateVoiceCall(int sr, string callType)
        {
            bool isAction = callType == "action";
            float duration = isAction ? 0.7f : 0.45f;
            int length = (int)(sr * duration);
            var clip = AudioClip.Create(isAction ? "VoiceAction" : "VoiceCut",
                length, 1, sr, false);
            float[] data = new float[length];

            // Fundamental vocal frequency (male voice ~120Hz)
            float f0 = isAction ? 140f : 160f;

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float tNorm = t / duration;

                // Envelope: sharp attack, sustain, decay
                float env;
                if (isAction)
                {
                    // "Ac-tion!": two syllables
                    float syl1 = Gaussian(tNorm, 0.2f, 0.12f);
                    float syl2 = Gaussian(tNorm, 0.6f, 0.2f);
                    env = (syl1 * 0.6f + syl2 * 1f) * 0.8f;
                }
                else
                {
                    // "Cut!": single sharp syllable
                    env = Mathf.Clamp01(t / 0.02f) * Mathf.Exp(-t * 4f) * 1.5f;
                }

                // Glottal pulse train (vocal folds)
                float pitch = f0 * (1f + 0.08f * Mathf.Sin(2f * Mathf.PI * 5f * t));
                float glottal = 0;
                for (int h = 1; h <= 8; h++)
                {
                    float amplitude = 1f / h;
                    glottal += Mathf.Sin(2f * Mathf.PI * pitch * h * t) * amplitude;
                }
                glottal *= 0.3f;

                // Formant shaping (vowel resonances)
                float formant;
                if (isAction)
                {
                    // /æ/ -> /ʃ/ -> /ə/ -> /n/
                    float f1 = Mathf.Lerp(730, 490, tNorm); // first formant
                    float f2 = Mathf.Lerp(1090, 1350, tNorm); // second formant
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.2f;
                }
                else
                {
                    // /k/ -> /ʌ/ -> /t/
                    float f1 = Mathf.Lerp(600, 300, tNorm);
                    float f2 = Mathf.Lerp(1000, 800, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.2f;

                    // /k/ consonant burst at start
                    if (t < 0.03f)
                        formant += Random.Range(-0.3f, 0.3f);
                    // /t/ consonant burst at end
                    if (tNorm > 0.8f)
                        formant += Random.Range(-0.15f, 0.15f) *
                            Mathf.Clamp01((tNorm - 0.8f) / 0.1f);
                }

                // Aspirated noise (breathiness)
                float breath = Random.Range(-0.05f, 0.05f) * env;

                data[i] = (glottal + formant + breath) * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static float Gaussian(float x, float mean, float sigma)
        {
            float d = (x - mean) / sigma;
            return Mathf.Exp(-0.5f * d * d);
        }
    }
}
