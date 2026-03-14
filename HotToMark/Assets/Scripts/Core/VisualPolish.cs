using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Core
{
    /// <summary>
    /// Stage 8: Visual polish — windshield reflections, dashboard material details,
    /// dynamic shadows, and post-processing effects.
    /// Runs once at scene build and continuously for animated effects.
    /// </summary>
    public class VisualPolish : MonoBehaviour
    {
        [Header("Windshield")]
        [SerializeField] private float reflectionIntensity = 0.15f;
        [SerializeField] private float dirtOpacity = 0.05f;

        [Header("Dashboard")]
        [SerializeField] private float leatherSmoothness = 0.3f;
        [SerializeField] private float plasticSmoothness = 0.6f;
        [SerializeField] private float chromeSmoothness = 0.9f;

        [Header("Animated Effects")]
        [SerializeField] private Light dashboardIndicatorLight;
        [SerializeField] private float indicatorBlinkRate = 1.5f;

        private GameState state;

        void Start()
        {
            ApplyMaterialPolish();
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }
            UpdateAnimatedEffects();
        }

        /// <summary>
        /// Apply material properties to all cockpit parts for visual realism.
        /// Called once at startup.
        /// </summary>
        public void ApplyMaterialPolish()
        {
            // Find and upgrade dashboard materials
            ApplyMaterialToNamed("Dashboard", CreateLeatherMaterial());
            ApplyMaterialToNamed("DashboardTop", CreatePlasticMaterial(new Color(0.15f, 0.15f, 0.15f)));
            ApplyMaterialToNamed("LowerDash", CreatePlasticMaterial(new Color(0.1f, 0.1f, 0.1f)));

            // Chrome accents
            ApplyMaterialToNamed("Bezel", CreateChromeMaterial());
            ApplyMaterialToNamed("CenterCap", CreateChromeMaterial());

            // Steering wheel — soft rubber
            ApplyMaterialToNamed("WheelRim", CreateRubberMaterial());
            ApplyMaterialToNamed("WheelHub", CreatePlasticMaterial(new Color(0.18f, 0.18f, 0.18f)));

            // Apply windshield tint with subtle reflections
            ApplyWindshieldMaterial();
        }

        private void ApplyMaterialToNamed(string objectName, Material mat)
        {
            var obj = GameObject.Find(objectName);
            if (obj == null) return;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = mat;
        }

        private Material CreateLeatherMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.12f, 0.1f, 0.09f); // dark brown-black leather
            mat.SetFloat("_Smoothness", leatherSmoothness);
            mat.SetFloat("_Metallic", 0.0f);
            // Subtle normal for leather grain (would need a normal map texture)
            return mat;
        }

        private Material CreatePlasticMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", plasticSmoothness);
            mat.SetFloat("_Metallic", 0.0f);
            return mat;
        }

        private Material CreateChromeMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.7f, 0.7f, 0.72f);
            mat.SetFloat("_Smoothness", chromeSmoothness);
            mat.SetFloat("_Metallic", 0.85f);
            return mat;
        }

        private Material CreateRubberMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.15f, 0.15f, 0.15f);
            mat.SetFloat("_Smoothness", 0.15f);
            mat.SetFloat("_Metallic", 0.0f);
            return mat;
        }

        private void ApplyWindshieldMaterial()
        {
            var windshield = GameObject.Find("Windshield");
            if (windshield == null) return;

            var renderer = windshield.GetComponent<Renderer>();
            if (renderer == null) return;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.6f, 0.8f, 0.95f, 0.04f + dirtOpacity);

            // Make transparent
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            // Reflections
            mat.SetFloat("_Smoothness", 0.95f); // glass is very smooth
            mat.SetFloat("_Metallic", reflectionIntensity);

            renderer.material = mat;
        }

        private void UpdateAnimatedEffects()
        {
            if (state == null) return;

            // Blink dashboard warning light when in reverse too long
            if (state.phase == GamePhase.Reversing && dashboardIndicatorLight != null)
            {
                float elapsed = Time.time - state.reverseStartTime;
                if (elapsed > GameState.HURRY_TIME)
                {
                    dashboardIndicatorLight.enabled =
                        Mathf.Sin(Time.time * indicatorBlinkRate * Mathf.PI * 2) > 0;
                }
            }
        }
    }
}
