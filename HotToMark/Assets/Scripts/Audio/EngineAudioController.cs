using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Audio
{
    /// <summary>
    /// Stage 8: Engine audio system with RPM-based pitch.
    /// Handles idle, acceleration, deceleration sounds.
    /// Also manages horn, tire screech, ambient crew audio, and voice calls.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EngineAudioController : MonoBehaviour
    {
        [Header("Engine Audio")]
        [SerializeField] private AudioClip engineIdleClip;
        [SerializeField] private AudioClip engineAccelClip;
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

        [Header("Ambient Film Set")]
        [SerializeField] private AudioClip ambientCrewClip;
        [SerializeField] private float ambientVolume = 0.1f;

        [Header("Voice Calls")]
        [SerializeField] private AudioClip actionVoiceClip;
        [SerializeField] private AudioClip cutVoiceClip;

        private AudioSource engineSource;
        private AudioSource sfxSource;
        private AudioSource ambientSource;
        private GameState state;
        private float lastScreechTime;
        private bool engineRunning;

        void Awake()
        {
            // Engine loop audio source
            engineSource = GetComponent<AudioSource>();
            engineSource.loop = true;
            engineSource.playOnAwake = false;

            // SFX one-shot source
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            // Ambient loop source
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
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

        private void UpdateEnginePitch()
        {
            float absSpeed = Mathf.Abs(state.speed);
            float speedNorm = absSpeed / GameState.MAX_FORWARD_MPH;
            float throttleBoost = state.throttle * 0.3f;

            // Pitch: idle -> max based on speed + throttle
            float targetPitch = Mathf.Lerp(idlePitch, maxPitch, speedNorm + throttleBoost);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * 5f);

            // Volume: louder with speed and throttle
            float targetVol = Mathf.Lerp(idleVolume, maxVolume, speedNorm * 0.7f + state.throttle * 0.3f);
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

            if (engineIdleClip != null)
            {
                engineSource.clip = engineIdleClip;
                engineSource.pitch = idlePitch;
                engineSource.volume = idleVolume;
                engineSource.Play();
            }

            // Start ambient crew audio
            if (ambientCrewClip != null)
            {
                ambientSource.clip = ambientCrewClip;
                ambientSource.volume = ambientVolume;
                ambientSource.Play();
            }
        }

        public void StopEngine()
        {
            engineRunning = false;
            engineSource.Stop();
            ambientSource.Stop();
        }

        public void PlayHorn()
        {
            if (hornClip != null)
                sfxSource.PlayOneShot(hornClip);
            else
                PlayGeneratedHorn();
        }

        public void PlayTireScreech()
        {
            if (tireScreechClip != null)
                sfxSource.PlayOneShot(tireScreechClip, 0.4f);
        }

        public void PlayVoiceAction()
        {
            if (actionVoiceClip != null)
                sfxSource.PlayOneShot(actionVoiceClip, 0.8f);
        }

        public void PlayVoiceCut()
        {
            if (cutVoiceClip != null)
                sfxSource.PlayOneShot(cutVoiceClip, 0.8f);
        }

        /// <summary>
        /// Fallback procedural horn when no AudioClip is assigned.
        /// Generates a short dual-tone horn sound.
        /// </summary>
        private void PlayGeneratedHorn()
        {
            int sampleRate = AudioSettings.outputSampleRate;
            int length = (int)(sampleRate * 0.35f);
            var clip = AudioClip.Create("GeneratedHorn", length, 1, sampleRate, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - t / 0.35f);
                envelope *= Mathf.Clamp01(t / 0.02f); // attack

                // Dual-tone: ~380 Hz + ~507 Hz
                float tone1 = Mathf.Sin(2f * Mathf.PI * 380f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 507f * t);
                data[i] = (tone1 * 0.5f + tone2 * 0.3f) * envelope * 0.5f;
            }

            clip.SetData(data, 0);
            sfxSource.PlayOneShot(clip, 0.6f);
        }
    }
}
