using UnityEngine;
using HotToMark.Core;
using HotToMark.Scoring;

namespace HotToMark.Audio
{
    /// <summary>
    /// F-6: Director gives verbal feedback after key moments.
    /// "That was perfect!", "Too fast!", "Tighter to the mark!", etc.
    /// Plays context-aware quips based on performance.
    /// </summary>
    public class DirectorFeedback : MonoBehaviour
    {
        [Header("Volume")]
        [SerializeField] private float feedbackVolume = 0.65f;
        [SerializeField] private float delayAfterCut = 1.5f;

        private AudioSource source;
        private GameState state;

        // Cached clips per feedback line
        private AudioClip[] markFeedbackClips;
        private AudioClip[] reverseFeedbackClips;
        private AudioClip clipPerfect;
        private AudioClip clipGreat;
        private AudioClip clipGood;
        private AudioClip clipNotBad;
        private AudioClip clipAgain;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.volume = feedbackVolume;

            GenerateAllClips();
        }

        void Update()
        {
            if (state == null && GameManager.Instance != null)
                state = GameManager.Instance.state;
        }

        /// <summary>
        /// Called when car stops on mark. Director reacts to accuracy.
        /// </summary>
        public void OnMarkStop(float accuracy)
        {
            AudioClip clip;
            if (accuracy >= 90f)
                clip = clipPerfect;     // "That's perfect!"
            else if (accuracy >= 70f)
                clip = clipGreat;       // "Great, great!"
            else if (accuracy >= 40f)
                clip = clipGood;        // "OK, that works"
            else
                clip = clipAgain;       // "Let's go again"

            if (clip != null)
                source.PlayOneShot(clip, feedbackVolume);
        }

        /// <summary>
        /// Called from results screen with full score context.
        /// Plays after a delay so it doesn't overlap "Cut!".
        /// </summary>
        public void OnTakeComplete(ScoreResult result)
        {
            AudioClip clip;
            if (result.totalScore >= 90)
                clip = clipPerfect;
            else if (result.totalScore >= 75)
                clip = clipGreat;
            else if (result.totalScore >= 55)
                clip = clipNotBad;
            else
                clip = clipAgain;

            if (clip != null)
                StartCoroutine(PlayDelayed(clip, delayAfterCut));
        }

        private System.Collections.IEnumerator PlayDelayed(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.PlayOneShot(clip, feedbackVolume);
        }

        // ---- Procedural Voice Generation ----

        private void GenerateAllClips()
        {
            int sr = AudioSettings.outputSampleRate;

            // Different director lines with distinct pitch/cadence
            clipPerfect = GenerateDirectorLine(sr, DirectorLine.Perfect);
            clipGreat = GenerateDirectorLine(sr, DirectorLine.Great);
            clipGood = GenerateDirectorLine(sr, DirectorLine.Good);
            clipNotBad = GenerateDirectorLine(sr, DirectorLine.NotBad);
            clipAgain = GenerateDirectorLine(sr, DirectorLine.Again);
        }

        private enum DirectorLine { Perfect, Great, Good, NotBad, Again }

        private AudioClip GenerateDirectorLine(int sr, DirectorLine line)
        {
            float duration;
            float f0;       // vocal pitch
            float energy;   // excitement level
            string name;

            switch (line)
            {
                case DirectorLine.Perfect:
                    duration = 0.9f; f0 = 155f; energy = 1.0f; name = "DirPerfect"; break;
                case DirectorLine.Great:
                    duration = 0.6f; f0 = 150f; energy = 0.85f; name = "DirGreat"; break;
                case DirectorLine.Good:
                    duration = 0.55f; f0 = 140f; energy = 0.6f; name = "DirGood"; break;
                case DirectorLine.NotBad:
                    duration = 0.7f; f0 = 135f; energy = 0.5f; name = "DirNotBad"; break;
                default: // Again
                    duration = 0.8f; f0 = 125f; energy = 0.4f; name = "DirAgain"; break;
            }

            int length = (int)(sr * duration);
            var clip = AudioClip.Create(name, length, 1, sr, false);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sr;
                float tNorm = t / duration;

                // Envelope varies by excitement
                float env = GetDirectorEnvelope(line, tNorm, energy);

                // Pitch rises with excitement
                float pitch = f0 * (1f + energy * 0.15f * Mathf.Sin(2f * Mathf.PI * 4f * t));

                // Glottal pulse train
                float glottal = 0;
                for (int h = 1; h <= 6; h++)
                {
                    glottal += Mathf.Sin(2f * Mathf.PI * pitch * h * t) / h;
                }
                glottal *= 0.25f;

                // Formant shaping per line
                float formant = GetDirectorFormant(line, t, tNorm);

                // More breath for excited lines
                float breath = Random.Range(-1f, 1f) * 0.04f * energy * env;

                data[i] = (glottal + formant + breath) * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private float GetDirectorEnvelope(DirectorLine line, float tNorm, float energy)
        {
            switch (line)
            {
                case DirectorLine.Perfect:
                    // "That's per-fect!": three syllables, rising energy
                    return (Gaussian(tNorm, 0.15f, 0.08f) * 0.5f +
                            Gaussian(tNorm, 0.45f, 0.1f) * 0.8f +
                            Gaussian(tNorm, 0.75f, 0.12f) * 1.0f) * energy;

                case DirectorLine.Great:
                    // "Great!": single strong syllable
                    return Gaussian(tNorm, 0.4f, 0.25f) * energy;

                case DirectorLine.Good:
                    // "Good": single steady syllable
                    return Gaussian(tNorm, 0.45f, 0.3f) * energy;

                case DirectorLine.NotBad:
                    // "Not bad": two syllables, low key
                    return (Gaussian(tNorm, 0.25f, 0.12f) * 0.6f +
                            Gaussian(tNorm, 0.65f, 0.15f) * 0.8f) * energy;

                default: // Again
                    // "Let's go a-gain": three syllables, descending
                    return (Gaussian(tNorm, 0.15f, 0.08f) * 0.7f +
                            Gaussian(tNorm, 0.45f, 0.1f) * 0.5f +
                            Gaussian(tNorm, 0.75f, 0.13f) * 0.9f) * energy;
            }
        }

        private float GetDirectorFormant(DirectorLine line, float t, float tNorm)
        {
            float f1, f2;
            float formant;

            switch (line)
            {
                case DirectorLine.Perfect:
                    // /ð/ -> /æ/ -> /p/ -> /ɜː/ -> /f/ -> /ɛ/ -> /k/ -> /t/
                    f1 = Mathf.Lerp(700, 500, tNorm);
                    f2 = Mathf.Lerp(1100, 1400, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.35f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.2f;
                    if (tNorm > 0.35f && tNorm < 0.4f) formant += Random.Range(-0.1f, 0.1f);
                    return formant;

                case DirectorLine.Great:
                    // /g/ -> /ɹ/ -> /eɪ/ -> /t/
                    f1 = Mathf.Lerp(500, 550, tNorm);
                    f2 = Mathf.Lerp(1800, 1900, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.2f;
                    if (t < 0.04f) formant += Random.Range(-0.2f, 0.2f);
                    if (tNorm > 0.85f) formant += Random.Range(-0.1f, 0.1f);
                    return formant;

                case DirectorLine.Good:
                    // /g/ -> /ʊ/ -> /d/
                    f1 = Mathf.Lerp(300, 350, tNorm);
                    f2 = Mathf.Lerp(950, 1000, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.15f;
                    if (t < 0.03f) formant += Random.Range(-0.2f, 0.2f);
                    return formant;

                case DirectorLine.NotBad:
                    // /n/ -> /ɒ/ -> /t/ -> /b/ -> /æ/ -> /d/
                    f1 = Mathf.Lerp(600, 700, tNorm);
                    f2 = Mathf.Lerp(1000, 1100, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.35f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.2f;
                    if (tNorm > 0.4f && tNorm < 0.45f) formant += Random.Range(-0.15f, 0.15f);
                    return formant;

                default: // Again
                    // /l/ -> /ɛ/ -> /t/ -> /s/ -> /g/ -> /oʊ/ -> /ə/ -> /g/ -> /ɛ/ -> /n/
                    f1 = Mathf.Lerp(500, 600, tNorm);
                    f2 = Mathf.Lerp(1400, 1200, tNorm);
                    formant = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.35f +
                              Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.2f;
                    if (tNorm > 0.2f && tNorm < 0.25f)
                        formant += Random.Range(-0.2f, 0.2f); // /s/ sibilant
                    return formant;
            }
        }

        private static float Gaussian(float x, float mean, float sigma)
        {
            float d = (x - mean) / sigma;
            return Mathf.Exp(-0.5f * d * d);
        }
    }
}
