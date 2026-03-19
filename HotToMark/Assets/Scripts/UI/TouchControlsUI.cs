using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Visual pedal buttons for gas and brake.
    /// Shaped like foot pedals at the bottom of the screen.
    /// Horn is handled by tapping the steering wheel in the 3D cockpit view.
    /// </summary>
    public class TouchControlsUI : MonoBehaviour
    {
        private Image gasPedal;
        private Image brakePedal;
        private Image gasPedalFace;
        private Image brakePedalFace;

        private readonly Color gasActiveColor = new Color(0.15f, 0.7f, 0.15f, 0.9f);
        private readonly Color gasInactiveColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        private readonly Color brakeActiveColor = new Color(0.7f, 0.15f, 0.15f, 0.9f);
        private readonly Color brakeInactiveColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);

        private readonly Color gasFaceActive = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        private readonly Color gasFaceInactive = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        private readonly Color brakeFaceActive = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        private readonly Color brakeFaceInactive = new Color(0.15f, 0.15f, 0.15f, 0.7f);

        private GameState state;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // --- Brake pedal (left side) ---
            // Pedal base/housing — dark rectangle
            var brakeBase = UIFactory.CreateImage("BrakePedalBase", transform, new Color(0.04f, 0.04f, 0.04f, 0.8f));
            UIFactory.SetAnchors(brakeBase, new Vector2(0.03f, 0.02f), new Vector2(0.22f, 0.30f));

            // Pedal face — the part you press (narrower at top, wider at bottom = pedal shape)
            var brakeFace = UIFactory.CreateImage("BrakePedalFace", brakeBase.transform, brakeFaceInactive);
            var brakeFaceRect = brakeFace.GetComponent<RectTransform>();
            brakeFaceRect.anchorMin = new Vector2(0.1f, 0.08f);
            brakeFaceRect.anchorMax = new Vector2(0.9f, 0.92f);
            brakeFaceRect.offsetMin = Vector2.zero;
            brakeFaceRect.offsetMax = Vector2.zero;
            brakePedalFace = brakeFace.GetComponent<Image>();

            // Pedal surface texture — horizontal grip lines
            for (int i = 0; i < 4; i++)
            {
                float y = 0.2f + i * 0.18f;
                var grip = UIFactory.CreateImage($"BrakeGrip_{i}", brakeFace.transform,
                    new Color(0.3f, 0.3f, 0.3f, 0.5f));
                var gripRect = grip.GetComponent<RectTransform>();
                gripRect.anchorMin = new Vector2(0.15f, y);
                gripRect.anchorMax = new Vector2(0.85f, y + 0.06f);
                gripRect.offsetMin = Vector2.zero;
                gripRect.offsetMax = Vector2.zero;
            }

            // Pedal label
            var brakeLabel = UIFactory.CreateText("BrakeLabel", brakeFace.transform,
                "BRAKE", 30, new Color(1f, 0.4f, 0.4f, 0.8f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(brakeLabel, new Vector2(0, 0), new Vector2(1, 0.3f));

            brakePedal = brakeBase.GetComponent<Image>();

            // Touch zone for brake (larger than visual, covers left half of bottom)
            var brakeTouchZone = UIFactory.CreateImage("BrakeTouchZone", transform,
                new Color(0, 0, 0, 0));
            UIFactory.SetAnchors(brakeTouchZone, new Vector2(0, 0), new Vector2(0.5f, 0.35f));

            // --- Gas pedal (right side) ---
            var gasBase = UIFactory.CreateImage("GasPedalBase", transform, new Color(0.04f, 0.04f, 0.04f, 0.8f));
            UIFactory.SetAnchors(gasBase, new Vector2(0.78f, 0.02f), new Vector2(0.97f, 0.30f));

            // Pedal face
            var gasFace = UIFactory.CreateImage("GasPedalFace", gasBase.transform, gasFaceInactive);
            var gasFaceRect = gasFace.GetComponent<RectTransform>();
            gasFaceRect.anchorMin = new Vector2(0.1f, 0.08f);
            gasFaceRect.anchorMax = new Vector2(0.9f, 0.92f);
            gasFaceRect.offsetMin = Vector2.zero;
            gasFaceRect.offsetMax = Vector2.zero;
            gasPedalFace = gasFace.GetComponent<Image>();

            // Pedal surface texture — horizontal grip lines
            for (int i = 0; i < 4; i++)
            {
                float y = 0.2f + i * 0.18f;
                var grip = UIFactory.CreateImage($"GasGrip_{i}", gasFace.transform,
                    new Color(0.3f, 0.3f, 0.3f, 0.5f));
                var gripRect = grip.GetComponent<RectTransform>();
                gripRect.anchorMin = new Vector2(0.15f, y);
                gripRect.anchorMax = new Vector2(0.85f, y + 0.06f);
                gripRect.offsetMin = Vector2.zero;
                gripRect.offsetMax = Vector2.zero;
            }

            // Pedal label
            var gasLabel = UIFactory.CreateText("GasLabel", gasFace.transform,
                "GAS", 30, new Color(0.4f, 1f, 0.4f, 0.8f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(gasLabel, new Vector2(0, 0), new Vector2(1, 0.3f));

            gasPedal = gasBase.GetComponent<Image>();

            // Touch zone for gas (larger than visual, covers right half of bottom)
            var gasTouchZone = UIFactory.CreateImage("GasTouchZone", transform,
                new Color(0, 0, 0, 0));
            UIFactory.SetAnchors(gasTouchZone, new Vector2(0.5f, 0), new Vector2(1, 0.35f));
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }

            // Animate pedal colors based on input
            bool gasActive = state.throttle > 0;
            bool brakeActive = state.brake > 0;

            gasPedalFace.color = gasActive ? gasFaceActive : gasFaceInactive;
            brakePedalFace.color = brakeActive ? brakeFaceActive : brakeFaceInactive;
        }
    }
}
