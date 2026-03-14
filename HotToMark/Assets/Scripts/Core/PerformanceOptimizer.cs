using UnityEngine;

namespace HotToMark.Core
{
    /// <summary>
    /// Stage 8: iOS performance optimization.
    /// Manages LOD, draw call batching, and frame rate targets.
    /// Monitors performance and dynamically adjusts quality.
    /// </summary>
    public class PerformanceOptimizer : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private int targetFrameRate = 60;

        [Header("Adaptive Quality")]
        [SerializeField] private bool adaptiveQuality = true;
        [SerializeField] private float lowFPSThreshold = 45f;
        [SerializeField] private float highFPSThreshold = 55f;

        [Header("Shadow Control")]
        [SerializeField] private float maxShadowDistance = 60f;
        [SerializeField] private float minShadowDistance = 20f;

        [Header("LOD")]
        [SerializeField] private float lodBias = 1.0f;

        private float[] fpsHistory = new float[30];
        private int fpsIndex;
        private float updateTimer;
        private int currentQualityLevel;

        void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // iOS specific optimizations
#if UNITY_IOS
            QualitySettings.antiAliasing = 2; // 2x MSAA, good balance
            QualitySettings.shadows = ShadowQuality.HardOnly; // start with hard shadows
            QualitySettings.shadowDistance = maxShadowDistance;
            QualitySettings.lodBias = lodBias;

            // Enable static batching
            // Dynamic batching handled by URP automatically
#endif
        }

        void Update()
        {
            if (!adaptiveQuality) return;

            // Track FPS
            float fps = 1f / Time.unscaledDeltaTime;
            fpsHistory[fpsIndex] = fps;
            fpsIndex = (fpsIndex + 1) % fpsHistory.Length;

            updateTimer += Time.unscaledDeltaTime;
            if (updateTimer < 2f) return; // check every 2 seconds
            updateTimer = 0;

            float avgFPS = GetAverageFPS();
            AdjustQuality(avgFPS);
        }

        private float GetAverageFPS()
        {
            float sum = 0;
            int count = 0;
            for (int i = 0; i < fpsHistory.Length; i++)
            {
                if (fpsHistory[i] > 0)
                {
                    sum += fpsHistory[i];
                    count++;
                }
            }
            return count > 0 ? sum / count : 60f;
        }

        private void AdjustQuality(float avgFPS)
        {
            if (avgFPS < lowFPSThreshold && currentQualityLevel > 0)
            {
                // Reduce quality
                currentQualityLevel--;
                ApplyQualityLevel(currentQualityLevel);
            }
            else if (avgFPS > highFPSThreshold && currentQualityLevel < 2)
            {
                // Increase quality
                currentQualityLevel++;
                ApplyQualityLevel(currentQualityLevel);
            }
        }

        private void ApplyQualityLevel(int level)
        {
            switch (level)
            {
                case 0: // Low
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.shadowDistance = minShadowDistance;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.lodBias = 0.5f;
                    break;

                case 1: // Medium
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowDistance = (minShadowDistance + maxShadowDistance) / 2f;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.lodBias = 1.0f;
                    break;

                case 2: // High
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowDistance = maxShadowDistance;
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.lodBias = 1.5f;
                    break;
            }
        }
    }
}
