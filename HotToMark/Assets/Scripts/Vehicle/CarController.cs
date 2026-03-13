using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// Stage 2 + 4: Vehicle physics — acceleration, braking, drag, friction, steering.
    /// Handles forward driving and reverse. Detects mark stop and return to one.
    /// </summary>
    public class CarController : MonoBehaviour
    {
        [Header("Physics Tuning")]
        [SerializeField] private float accelForce = 35f;    // mph/s at full throttle
        [SerializeField] private float brakeForce = 55f;    // mph/s at full brake
        [SerializeField] private float dragCoeff = 0.4f;
        [SerializeField] private float rollingFriction = 1.8f;
        [SerializeField] private float steerSmoothing = 6f;
        [SerializeField] private float steerSensitivity = 0.85f;
        [SerializeField] private float lateralSpeedFactor = 0.003f;

        private GameState state;
        private const float MPH_TO_FPS = 1.467f; // mph to feet/s

        void Start()
        {
            state = GameManager.Instance.state;
        }

        void Update()
        {
            if (state.phase == GamePhase.Menu || state.phase == GamePhase.Results)
                return;

            float dt = Time.deltaTime;
            UpdateSmoothnessTracking(dt);

            if (state.phase == GamePhase.StoppedOnMark || state.phase == GamePhase.Honking)
            {
                // Car is frozen on the mark
                state.speed *= 0.92f;
                if (Mathf.Abs(state.speed) < 0.1f) state.speed = 0;
                return;
            }

            UpdateForces(dt);
            UpdatePosition(dt);
            TrackStats();
            CheckCheckpoint();
            CheckMarkStop();
            CheckReturnToOne();
        }

        private void UpdateSmoothnessTracking(float dt)
        {
            float throttleJerk = Mathf.Abs(state.throttle - state.lastThrottle);
            float steerJerk = Mathf.Abs(state.steering - state.lastSteering);
            state.jerkAccum += (throttleJerk + steerJerk * 0.5f) * dt * 60f;
            state.lastThrottle = state.throttle;
            state.lastSteering = state.steering;
        }

        private void UpdateForces(float dt)
        {
            bool isReverse = state.gear == Gear.Reverse;

            if (isReverse)
            {
                state.speed -= state.throttle * accelForce * dt;
                if (state.speed > 0) state.speed = 0;
                state.speed += state.brake * brakeForce * dt;
                if (state.speed > 0) state.speed = 0;

                if (state.speed < 0)
                {
                    state.speed += (Mathf.Abs(state.speed) * dragCoeff + rollingFriction) * dt;
                    if (state.speed > 0) state.speed = 0;
                }
            }
            else
            {
                state.speed += state.throttle * accelForce * dt;
                state.speed -= state.brake * brakeForce * dt;

                if (state.speed > 0)
                {
                    state.speed -= (state.speed * dragCoeff + rollingFriction) * dt;
                }
                if (state.speed < 0) state.speed = 0;
            }

            // Clamp to speed limits
            state.speed = Mathf.Clamp(state.speed,
                -GameState.MAX_REVERSE_MPH, GameState.MAX_FORWARD_MPH);
        }

        private void UpdatePosition(float dt)
        {
            float feetPerSec = state.speed * MPH_TO_FPS;
            state.posY += feetPerSec * dt;

            // Lateral movement from steering
            state.posX += state.steering * Mathf.Abs(state.speed) * lateralSpeedFactor * dt * 60f;
            state.posX = Mathf.Clamp(state.posX, -1f, 1f);

            // Update the actual transform position for 3D objects
            Vector3 worldPos = FeetToWorldPosition(state.posY, state.posX);
            transform.position = worldPos;
        }

        private void TrackStats()
        {
            if (state.speed > state.maxSpeedHit)
                state.maxSpeedHit = state.speed;

            if (state.gear == Gear.Reverse && Mathf.Abs(state.speed) > state.reverseMaxSpeed)
                state.reverseMaxSpeed = Mathf.Abs(state.speed);
        }

        private void CheckCheckpoint()
        {
            if (state.gear == Gear.Reverse) return;
            if (state.checkpointPassed) return;

            if (state.posY >= state.checkpointDistance)
            {
                state.checkpointPassed = true;
                state.speedAtCheckpoint = state.speed;
                state.exactMPHAccuracy = Mathf.Max(0,
                    100f - Mathf.Abs(state.speed - state.targetMPH) * 5f);
            }
        }

        private void CheckMarkStop()
        {
            if (state.phase != GamePhase.Driving) return;

            float distFromMark = Mathf.Abs(state.posY - state.markDistance);
            float threshold = state.markDistance * GameState.MARK_THRESHOLD_PCT;

            // Normal stop near mark
            if (state.posY >= state.markDistance - threshold
                && Mathf.Abs(state.speed) < 0.5f
                && state.speed >= 0)
            {
                float accuracy = Mathf.Max(0, 100f - (distFromMark / threshold) * 100f);
                GameManager.Instance.OnStoppedOnMark(accuracy);
            }
            // Overshoot stop
            else if (state.posY > state.markDistance + GameState.OVERSHOOT_LIMIT
                     && Mathf.Abs(state.speed) < 0.5f)
            {
                float accuracy = Mathf.Max(0, 100f - (distFromMark / threshold) * 100f);
                GameManager.Instance.OnStoppedOnMark(accuracy);
            }
        }

        private void CheckReturnToOne()
        {
            if (state.phase != GamePhase.Reversing) return;

            if (state.posY <= GameState.RETURN_TOLERANCE && Mathf.Abs(state.speed) < 0.5f)
            {
                float returnAccuracy = Mathf.Max(0, 100f - Mathf.Abs(state.posY) * 10f);
                GameManager.Instance.OnReturnedToOne(returnAccuracy);
            }
        }

        /// <summary>
        /// Apply steering input (called by TouchInputManager or keyboard input).
        /// </summary>
        public void SetSteeringInput(float input)
        {
            float dt = Time.deltaTime;
            state.steering += (input * steerSensitivity - state.steering) * dt * steerSmoothing;
            state.wheelAngle = state.steering * 45f;
        }

        /// <summary>
        /// Convert game-feet position to Unity world coordinates.
        /// 1 foot = 0.3048 meters, but we use a scale factor for visual clarity.
        /// </summary>
        public static Vector3 FeetToWorldPosition(float feetForward, float lateralOffset)
        {
            float worldScale = 0.3f; // 1 foot ≈ 0.3 Unity units for a good visual scale
            return new Vector3(lateralOffset * 3f, 0, feetForward * worldScale);
        }
    }
}
