using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Stage 1: Visual touch control overlays for iOS.
    /// Self-builds gas/brake pedal zones and horn button at runtime.
    /// These are visual indicators — actual input is handled by TouchInputManager.
    /// </summary>
    public class TouchControlsUI : MonoBehaviour
    {
        private Image gasPedalZone;
        private Image brakePedalZone;
        private Image hornImage;

        private readonly Color gasActiveColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        private readonly Color gasInactiveColor = new Color(0.2f, 0.8f, 0.2f, 0.12f);
        private readonly Color brakeActiveColor = new Color(0.8f, 0.2f, 0.2f, 0.4f);
        private readonly Color brakeInactiveColor = new Color(0.8f, 0.2f, 0.2f, 0.12f);

        private GameState state;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // Brake zone — left side, bottom (matches TouchInputManager's 50% split)
            var brakeObj = UIFactory.CreateImage("BrakeZone", transform, brakeInactiveColor);
            UIFactory.SetAnchors(brakeObj, new Vector2(0, 0), new Vector2(0.5f, 0.4f));
            brakePedalZone = brakeObj.GetComponent<Image>();

            var brakeLabel = UIFactory.CreateText("BrakeLabel", brakeObj.transform,
                "BRAKE", 14, new Color(1f, 0.3f, 0.3f, 0.6f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(brakeLabel, new Vector2(0, 0), new Vector2(1, 1));

            // Gas zone — right side, bottom (matches TouchInputManager's 50% split)
            var gasObj = UIFactory.CreateImage("GasZone", transform, gasInactiveColor);
            UIFactory.SetAnchors(gasObj, new Vector2(0.5f, 0), new Vector2(1, 0.4f));
            gasPedalZone = gasObj.GetComponent<Image>();

            var gasLabel = UIFactory.CreateText("GasLabel", gasObj.transform,
                "GAS", 14, new Color(0.3f, 1f, 0.3f, 0.6f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(gasLabel, new Vector2(0, 0), new Vector2(1, 1));

            // Horn button — center
            var hornObj = UIFactory.CreateImage("HornButton", transform,
                new Color(0.9f, 0.7f, 0.1f, 0.25f));
            UIFactory.SetAnchors(hornObj, new Vector2(0.38f, 0.35f), new Vector2(0.62f, 0.55f));
            hornImage = hornObj.GetComponent<Image>();

            var hornLabel = UIFactory.CreateText("HornLabel", hornObj.transform,
                "HORN", 16, new Color(1f, 0.85f, 0.2f, 0.7f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(hornLabel, new Vector2(0, 0), new Vector2(1, 1));

            // Horn click handler
            var btn = hornObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.9f, 0.7f, 0.1f, 0.25f);
            colors.pressedColor = new Color(1f, 0.85f, 0.2f, 0.6f);
            btn.colors = colors;
            btn.onClick.AddListener(() =>
            {
                var horn = GameManager.Instance.hornSystem;
                if (horn != null) horn.Honk();
            });
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }

            gasPedalZone.color = state.throttle > 0 ? gasActiveColor : gasInactiveColor;
            brakePedalZone.color = state.brake > 0 ? brakeActiveColor : brakeInactiveColor;
        }
    }
}
