using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Environment
{
    /// <summary>
    /// F-7: Weather and time-of-day system.
    /// Modifies lighting, fog, and visual effects based on weather condition.
    /// Can spawn rain particles and adjust sky appearance.
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        [Header("Current Weather")]
        public WeatherCondition condition = WeatherCondition.Clear;

        private GameState state;
        private ParticleSystem rainParticles;
        private Light sun;

        void Start()
        {
            sun = FindAnyObjectByType<Light>();
        }

        void Update()
        {
            if (state == null && GameManager.Instance != null)
                state = GameManager.Instance.state;
        }

        /// <summary>
        /// Apply a weather condition. Call at game start.
        /// </summary>
        public void ApplyWeather(WeatherCondition weather)
        {
            condition = weather;

            // Clean up existing rain
            if (rainParticles != null)
            {
                Destroy(rainParticles.gameObject);
                rainParticles = null;
            }

            switch (weather)
            {
                case WeatherCondition.Clear:
                    ApplyClear();
                    break;
                case WeatherCondition.Overcast:
                    ApplyOvercast();
                    break;
                case WeatherCondition.Rain:
                    ApplyRain();
                    break;
                case WeatherCondition.GoldenHour:
                    ApplyGoldenHour();
                    break;
                case WeatherCondition.Night:
                    ApplyNight();
                    break;
            }
        }

        private void ApplyClear()
        {
            RenderSettings.fog = false;
            if (sun != null)
            {
                sun.intensity = 1.2f;
                sun.color = new Color(1f, 0.95f, 0.85f);
                sun.transform.rotation = Quaternion.Euler(45, -30, 0);
            }
        }

        private void ApplyOvercast()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.6f, 0.62f, 0.65f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 20f;
            RenderSettings.fogEndDistance = 120f;

            if (sun != null)
            {
                sun.intensity = 0.6f;
                sun.color = new Color(0.85f, 0.85f, 0.9f);
                sun.shadows = LightShadows.None; // overcast = diffuse light
            }

            RenderSettings.ambientSkyColor = new Color(0.5f, 0.52f, 0.58f);
            RenderSettings.ambientEquatorColor = new Color(0.48f, 0.48f, 0.52f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.36f, 0.38f);

            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_SkyTint", new Color(0.55f, 0.58f, 0.65f));
                RenderSettings.skybox.SetFloat("_Exposure", 0.8f);
            }
        }

        private void ApplyRain()
        {
            // Start with overcast settings
            ApplyOvercast();

            // Darken further
            if (sun != null) sun.intensity = 0.4f;
            RenderSettings.fogEndDistance = 80f;

            if (RenderSettings.skybox != null)
                RenderSettings.skybox.SetFloat("_Exposure", 0.6f);

            // Spawn rain particles
            var rainObj = new GameObject("Rain");
            rainObj.transform.position = new Vector3(0, 15f, 10f);

            rainParticles = rainObj.AddComponent<ParticleSystem>();
            var main = rainParticles.main;
            main.maxParticles = 500;
            main.startLifetime = 1.5f;
            main.startSpeed = 12f;
            main.startSize = 0.03f;
            main.startColor = new Color(0.7f, 0.75f, 0.85f, 0.5f);
            main.gravityModifier = 1.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var emission = rainParticles.emission;
            emission.rateOverTime = 300f;

            var shape = rainParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 0.1f, 30f);

            // Use default particle material
            var renderer = rainObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = new Color(0.7f, 0.75f, 0.85f, 0.4f);
        }

        private void ApplyGoldenHour()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.85f, 0.6f, 0.3f, 0.3f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 200f;

            if (sun != null)
            {
                sun.intensity = 1.4f;
                sun.color = new Color(1f, 0.7f, 0.35f); // warm golden
                sun.transform.rotation = Quaternion.Euler(12, -60, 0); // low angle
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.8f;
            }

            RenderSettings.ambientSkyColor = new Color(0.8f, 0.55f, 0.3f);
            RenderSettings.ambientEquatorColor = new Color(0.7f, 0.5f, 0.3f);
            RenderSettings.ambientGroundColor = new Color(0.4f, 0.3f, 0.2f);

            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_SkyTint", new Color(0.8f, 0.5f, 0.25f));
                RenderSettings.skybox.SetColor("_GroundColor", new Color(0.5f, 0.35f, 0.2f));
                RenderSettings.skybox.SetFloat("_Exposure", 1.3f);
            }
        }

        private void ApplyNight()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.1f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 60f;

            if (sun != null)
            {
                sun.intensity = 0.15f;
                sun.color = new Color(0.4f, 0.45f, 0.6f); // moonlight
                sun.transform.rotation = Quaternion.Euler(60, -30, 0);
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.3f;
            }

            RenderSettings.ambientSkyColor = new Color(0.06f, 0.06f, 0.12f);
            RenderSettings.ambientEquatorColor = new Color(0.05f, 0.05f, 0.08f);
            RenderSettings.ambientGroundColor = new Color(0.03f, 0.03f, 0.05f);

            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_SkyTint", new Color(0.05f, 0.05f, 0.15f));
                RenderSettings.skybox.SetColor("_GroundColor", new Color(0.03f, 0.03f, 0.05f));
                RenderSettings.skybox.SetFloat("_Exposure", 0.3f);
            }

            // Add headlight effect
            SpawnHeadlights();
        }

        private void SpawnHeadlights()
        {
            var headlightObj = new GameObject("Headlights");
            headlightObj.transform.position = new Vector3(0, 0.8f, 1f);

            var light1 = new GameObject("HeadlightL");
            light1.transform.SetParent(headlightObj.transform);
            light1.transform.localPosition = new Vector3(-0.6f, 0, 0);
            var l1 = light1.AddComponent<Light>();
            l1.type = LightType.Spot;
            l1.color = new Color(1f, 0.95f, 0.85f);
            l1.intensity = 2f;
            l1.range = 30f;
            l1.spotAngle = 60f;
            l1.transform.rotation = Quaternion.Euler(15, 0, 0);

            var light2 = new GameObject("HeadlightR");
            light2.transform.SetParent(headlightObj.transform);
            light2.transform.localPosition = new Vector3(0.6f, 0, 0);
            var l2 = light2.AddComponent<Light>();
            l2.type = LightType.Spot;
            l2.color = new Color(1f, 0.95f, 0.85f);
            l2.intensity = 2f;
            l2.range = 30f;
            l2.spotAngle = 60f;
            l2.transform.rotation = Quaternion.Euler(15, 0, 0);
        }
    }

    public enum WeatherCondition
    {
        Clear,
        Overcast,
        Rain,
        GoldenHour,
        Night
    }
}
