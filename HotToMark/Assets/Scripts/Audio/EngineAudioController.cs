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
        [SerializeField] private AudioClip rollingVoiceClip;
        [SerializeField] private AudioClip speedVoiceClip;
        [SerializeField] private AudioClip actionVoiceClip;
        [SerializeField] private AudioClip cutVoiceClip;
        [SerializeField] private AudioClip backToOneVoiceClip;

        private AudioSource engineSource;
        private AudioSource sfxSource;
        private GameState state;
        private float lastScreechTime;
        private bool engineRunning;

        // Cached procedural clips
        private AudioClip generatedIdleClip;
        private AudioClip generatedHornClip;
        private AudioClip generatedScreechClip;
        private AudioClip generatedRollingClip;
        private AudioClip generatedSpeedClip;
        private AudioClip generatedActionClip;
        private AudioClip generatedCutClip;
        private AudioClip generatedBackToOneClip;

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

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }
            if (!engineRunning) return;

            UpdateEnginePitch();
            CheckTireScreech();
        }

        private void GenerateAllClips()
        {
            int sr = AudioSettings.outputSampleRate;
            generatedIdleClip = GenerateEngineIdle(sr);
            generatedHornClip = GenerateHorn(sr);
            generatedScreechClip = GenerateTireScreech(sr);
            generatedRollingClip = GenerateVoiceCall(sr, "rolling");
            generatedSpeedClip = GenerateVoiceCall(sr, "speed");
            generatedActionClip = GenerateVoiceCall(sr, "action");
            generatedCutClip = GenerateVoiceCall(sr, "cut");
            generatedBackToOneClip = GenerateVoiceCall(sr, "backtoone");
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

        public void PlayVoiceRolling()
        {
            var clip = rollingVoiceClip != null ? rollingVoiceClip : generatedRollingClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.7f);
        }

        public void PlayVoiceSpeed()
        {
            var clip = speedVoiceClip != null ? speedVoiceClip : generatedSpeedClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.7f);
        }

        public void PlayVoiceAction()
        {
            var clip = actionVoiceClip != null ? actionVoiceClip : generatedActionClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.8f);
        }

        public void PlayVoiceCut()
        {
            var clip = cutVoiceClip != null ? cutVoiceClip : generatedCutClip;
            if (clip != null) sfxSource.PlayOneShot(clip, 0.7f);
        }

        public void PlayVoiceBackToOne()
        {
            var clip = backToOneVoiceClip != null ? backToOneVoiceClip : generatedBackToOneClip;
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
            float duration;
            float f0;
            string clipName;

            switch (callType)
            {
                case "rolling":  duration = 0.65f; f0 = 130f; clipName = "VoiceRolling"; break;
                case "speed":    duration = 0.5f;  f0 = 145f; clipName = "VoiceSpeed"; break;
                case "action":   duration = 0.7f;  f0 = 140f; clipName = "VoiceAction"; break;
                case "cut":      duration = 0.45f; f0 = 160f; clipName = "VoiceCut"; break;
                case "backtoone": duration = 0.8f; f0 = 135f; clipName = "VoiceBackToOne"; break;
                default:         duration = 0.5f;  f0 = 140f; clipName = "Voice"; break;
            }

            int length = (int)(sr * duration);
            var clip = AudioClip.Create(clipName, length, 1, sr, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float tNorm = t / duration;

                // Envelope per call type
                float env = GetVoiceEnvelope(callType, t, tNorm, duration);

                // Glottal pulse train (vocal folds)
                float pitch = f0 * (1f + 0.08f * Mathf.Sin(2f * Mathf.PI * 5f * t));
                float glottal = 0;
                for (int h = 1; h <= 8; h++)
                {
                    float amplitude = 1f / h;
                    glottal += Mathf.Sin(2f * Mathf.PI * pitch * h * t) * amplitude;
                }
                glottal *= 0.3f;

                // Formant shaping per call type
                float formant = GetVoiceFormant(callType, t, tNorm);

                // Aspirated noise (breathiness)
                float breath = Random.Range(-0.05f, 0.05f) * env;

                data[i] = (glottal + formant + breath) * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private float GetVoiceEnvelope(string callType, float t, float tNorm, float duration)
        {
            switch (callType)
            {
                case "rolling":
                    // "Ro-lling!": two syllables, second held longer
                    return (Gaussian(tNorm, 0.2f, 0.1f) * 0.7f +
                            Gaussian(tNorm, 0.6f, 0.22f) * 1f) * 0.8f;

                case "speed":
                    // "Speed!": single sustained syllable with punch
                    return Mathf.Clamp01(t / 0.02f) *
                           Mathf.Clamp01((duration - t) / 0.08f) * 0.9f;

                case "action":
                    // "Ac-tion!": two syllables
                    return (Gaussian(tNorm, 0.2f, 0.12f) * 0.6f +
                            Gaussian(tNorm, 0.6f, 0.2f) * 1f) * 0.8f;

                case "cut":
                    // "Cut!": single sharp syllable
                    return Mathf.Clamp01(t / 0.02f) * Mathf.Exp(-t * 4f) * 1.5f;

                case "backtoone":
                    // "Back-to-one!": three syllables
                    return (Gaussian(tNorm, 0.15f, 0.08f) * 0.7f +
                            Gaussian(tNorm, 0.4f, 0.06f) * 0.5f +
                            Gaussian(tNorm, 0.7f, 0.15f) * 1f) * 0.8f;

                default:
                    return Mathf.Clamp01(t / 0.02f) * Mathf.Exp(-t * 3f);
            }
        }

        private float GetVoiceFormant(string callType, float t, float tNorm)
        {
            float formant;
            switch (callType)
            {
                case "rolling":
                    // /ɹ/ -> /oʊ/ -> /l/ -> /ɪ/ -> /ŋ/
                    float rf1 = Mathf.Lerp(400, 300, tNorm);
                    float rf2 = Mathf.Lerp(1200, 900, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * rf1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * rf2 * t) * 0.2f;
                    // /ɹ/ start roughness
                    if (t < 0.05f) formant += Random.Range(-0.15f, 0.15f);
                    return formant;

                case "speed":
                    // /s/ -> /p/ -> /iː/ -> /d/
                    float sf1 = Mathf.Lerp(270, 400, tNorm);
                    float sf2 = Mathf.Lerp(2300, 2000, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * sf1 * t) * 0.35f +
                              Mathf.Sin(2f * Mathf.PI * sf2 * t) * 0.25f;
                    // /s/ sibilant at start
                    if (t < 0.06f) formant += Random.Range(-0.3f, 0.3f) * (1f - t / 0.06f);
                    // /d/ burst at end
                    if (tNorm > 0.85f) formant += Random.Range(-0.1f, 0.1f);
                    return formant;

                case "action":
                    // /æ/ -> /ʃ/ -> /ə/ -> /n/
                    float af1 = Mathf.Lerp(730, 490, tNorm);
                    float af2 = Mathf.Lerp(1090, 1350, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * af1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * af2 * t) * 0.2f;
                    return formant;

                case "cut":
                    // /k/ -> /ʌ/ -> /t/
                    float cf1 = Mathf.Lerp(600, 300, tNorm);
                    float cf2 = Mathf.Lerp(1000, 800, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * cf1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * cf2 * t) * 0.2f;
                    if (t < 0.03f) formant += Random.Range(-0.3f, 0.3f);
                    if (tNorm > 0.8f)
                        formant += Random.Range(-0.15f, 0.15f) *
                            Mathf.Clamp01((tNorm - 0.8f) / 0.1f);
                    return formant;

                case "backtoone":
                    // /b/ -> /æ/ -> /k/ -> /t/ -> /ə/ -> /w/ -> /ʌ/ -> /n/
                    float bf1 = Mathf.Lerp(700, 600, tNorm);
                    float bf2 = Mathf.Lerp(1100, 1000, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * bf1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * bf2 * t) * 0.2f;
                    // /b/ voiced stop
                    if (t < 0.02f) formant += Random.Range(-0.2f, 0.2f);
                    // /k/ burst mid-word
                    if (tNorm > 0.3f && tNorm < 0.35f)
                        formant += Random.Range(-0.15f, 0.15f);
                    return formant;

                default:
                    return 0f;
            }
        }

        private static float Gaussian(float x, float mean, float sigma)
        {
            float d = (x - mean) / sigma;
            return Mathf.Exp(-0.5f * d * d);
        }
    }
}
