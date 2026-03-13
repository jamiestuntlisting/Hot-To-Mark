using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// Stage 1: Steering wheel visual — rotates based on steering input.
    /// 3-spoke design with horn button. Hands follow the wheel.
    /// Attach to the steering wheel GameObject in the cockpit.
    /// </summary>
    public class SteeringWheel : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float maxRotation = 45f;

        [Header("Hand Transforms")]
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        [Header("Hand Offsets (local to wheel center)")]
        [SerializeField] private float handRadius = 0.15f;

        private GameState state;

        void Start()
        {
            state = GameManager.Instance.state;
        }

        void Update()
        {
            if (state == null) return;

            // Rotate the wheel around its local Z axis
            float angle = state.wheelAngle;
            transform.localRotation = Quaternion.Euler(0, 0, -angle);

            // Position hands at 9 and 3 o'clock relative to wheel center
            UpdateHandPosition(leftHand, 180f + angle);  // 9 o'clock
            UpdateHandPosition(rightHand, 0f + angle);   // 3 o'clock
        }

        private void UpdateHandPosition(Transform hand, float angleDeg)
        {
            if (hand == null) return;

            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(
                Mathf.Cos(rad) * handRadius,
                Mathf.Sin(rad) * handRadius,
                -0.02f // slightly in front of wheel
            );
            hand.localPosition = localPos;
            hand.localRotation = Quaternion.Euler(0, 0, -angleDeg);
        }
    }
}
