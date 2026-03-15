using UnityEngine;
using HotToMark.Vehicle;

namespace HotToMark.Core
{
    public enum GamePhase
    {
        Menu,
        PreRoll,        // Film protocol: "Rolling!", "Speed!", "Action!"
        Driving,
        StoppedOnMark,
        Honking,
        Reversing,
        Results,
        Paused
    }

    public enum GameMode
    {
        Standard,
        SpeedRun,
        SmoothOperator,
        ExactMPH
    }

    public enum Gear
    {
        Drive,
        Reverse
    }

    /// <summary>
    /// Central game state — all mutable data lives here.
    /// ScoreManager, CarController, etc. read and write to this.
    /// </summary>
    [CreateAssetMenu(fileName = "GameState", menuName = "HotToMark/GameState")]
    public class GameState : ScriptableObject
    {
        [Header("Phase")]
        public GamePhase phase = GamePhase.Menu;
        public GameMode mode = GameMode.Standard;

        [Header("Vehicle")]
        public float speed;            // mph (negative = reversing)
        public float posX;             // lateral offset (-1 to 1)
        public float posY;             // distance traveled in feet
        public float steering;         // -1 to 1
        public float throttle;         // 0 to 1
        public float brake;            // 0 to 1
        public Gear gear = Gear.Drive;
        public float wheelAngle;       // degrees, max ±45

        [Header("Mark")]
        public float markDistance;     // feet to the mark (150-350)

        [Header("Scoring")]
        public float markAccuracy;
        public int honkCount;
        public bool honkPenalty;
        public float reverseStartTime;
        public bool reverseTooSlow;
        public bool reverseTooFast;
        public float reverseMaxSpeed;
        public float reverseAccuracy;
        public float maxSpeedHit;
        public float takeTime;
        public float takeStartTime;

        [Header("Smoothness")]
        public float lastThrottle;
        public float lastSteering;
        public float jerkAccum;
        public float smoothnessScore;

        [Header("Exact MPH")]
        public float targetMPH;
        public float exactMPHAccuracy;
        public bool checkpointPassed;
        public float checkpointDistance;
        public float speedAtCheckpoint;

        [Header("Take")]
        public int takeNumber;             // current take number (persists across rounds)
        public VehicleType selectedVehicle = VehicleType.Sedan;

        [Header("Pause")]
        public GamePhase phaseBeforePause;

        [Header("Constants")]
        public const float MAX_FORWARD_MPH = 60f;
        public const float MAX_REVERSE_MPH = 25f;
        public const float MARK_MIN = 150f;
        public const float MARK_MAX = 350f;
        public const float MARK_THRESHOLD_PCT = 0.05f;
        public const int HONKS_REQUIRED = 2;
        public const float REVERSE_SHIFT_DELAY = 0.5f;
        public const float HURRY_TIME = 15f;
        public const float PENALTY_SLOW_TIME = 20f;
        public const float PENALTY_FAST_MPH = 15f;
        public const int PENALTY_POINTS = 10;
        public const float RETURN_TOLERANCE = 2f;
        public const float OVERSHOOT_LIMIT = 20f;

        public void Reset()
        {
            phase = GamePhase.Menu;
            speed = 0; posX = 0; posY = 0;
            steering = 0; throttle = 0; brake = 0;
            gear = Gear.Drive;
            wheelAngle = 0;
            markDistance = 250f;
            markAccuracy = 0; honkCount = 0; honkPenalty = false;
            reverseStartTime = 0; reverseTooSlow = false; reverseTooFast = false;
            reverseMaxSpeed = 0; reverseAccuracy = 0;
            maxSpeedHit = 0; takeTime = 0; takeStartTime = 0;
            lastThrottle = 0; lastSteering = 0; jerkAccum = 0; smoothnessScore = 100;
            targetMPH = 0; exactMPHAccuracy = 0; checkpointPassed = false;
            checkpointDistance = 0; speedAtCheckpoint = 0;
            phaseBeforePause = GamePhase.Menu;
        }
    }
}
