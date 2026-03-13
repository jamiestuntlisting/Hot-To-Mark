using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;

namespace HotToMark.UI
{
    /// <summary>
    /// Stage 1: Visual touch control overlays for iOS.
    /// Gas pedal zone (right), brake pedal zone (left), horn button (center).
    /// These are visual indicators — actual input is handled by TouchInputManager.
    /// </summary>
    public class TouchControlsUI : MonoBehaviour
    {
        [Header("Pedal Zones")]
        [SerializeField] private Image gasPedalZone;
        [SerializeField] private Image brakePedalZone;
        [SerializeField] private TextMeshProUGUI gasLabel;
        [SerializeField] private TextMeshProUGUI brakeLabel;

        [Header("Horn Button")]
        [SerializeField] private Button hornButton;
        [SerializeField] private Image hornImage;

        [Header("Feedback Colors")]
        [SerializeField] private Color gasActiveColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        [SerializeField] private Color gasInactiveColor = new Color(0.2f, 0.8f, 0.2f, 0.15f);
        [SerializeField] private Color brakeActiveColor = new Color(0.8f, 0.2f, 0.2f, 0.4f);
        [SerializeField] private Color brakeInactiveColor = new Color(0.8f, 0.2f, 0.2f, 0.15f);

        private GameState state;

        void Start()
        {
            state = GameManager.Instance.state;

            if (hornButton != null)
            {
                hornButton.onClick.AddListener(() =>
                {
                    var horn = GameManager.Instance.hornSystem;
                    if (horn != null) horn.Honk();
                });
            }
        }

        void Update()
        {
            if (state == null) return;

            // Visual feedback on pedal zones
            if (gasPedalZone != null)
                gasPedalZone.color = state.throttle > 0 ? gasActiveColor : gasInactiveColor;

            if (brakePedalZone != null)
                brakePedalZone.color = state.brake > 0 ? brakeActiveColor : brakeInactiveColor;
        }
    }
}
