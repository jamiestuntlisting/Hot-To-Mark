using UnityEngine;

namespace HotToMark.Vehicle
{
    /// <summary>
    /// F-1: Vehicle configuration — defines handling characteristics for different cars.
    /// Each vehicle type has unique acceleration, braking, steering, and speed caps.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleConfig", menuName = "HotToMark/VehicleConfig")]
    public class VehicleConfig : ScriptableObject
    {
        public string displayName;
        public string description;
        public VehicleType type;

        [Header("Performance")]
        public float maxForwardMPH = 60f;
        public float maxReverseMPH = 25f;
        public float accelForce = 35f;
        public float brakeForce = 55f;

        [Header("Handling")]
        public float steerSensitivity = 0.85f;
        public float steerSmoothing = 6f;
        public float lateralSpeedFactor = 0.003f;
        public float dragCoeff = 0.4f;
        public float rollingFriction = 1.8f;

        [Header("Appearance")]
        public Color bodyColor = new Color(0.15f, 0.15f, 0.2f);
        public Color interiorColor = new Color(0.12f, 0.1f, 0.09f);
        public float wheelScale = 1f;

        [Header("Audio")]
        public float engineIdlePitch = 0.6f;
        public float engineMaxPitch = 2.5f;
        public float engineBasePitchHz = 35f;  // V8=35, V6=45, 4cyl=55

        public static VehicleConfig[] GetDefaults()
        {
            return new VehicleConfig[]
            {
                CreateSedan(),
                CreateMuscleCar(),
                CreateSUV()
            };
        }

        private static VehicleConfig CreateSedan()
        {
            var config = CreateInstance<VehicleConfig>();
            config.displayName = "Stunt Sedan";
            config.description = "Balanced handling. The industry standard.";
            config.type = VehicleType.Sedan;
            config.maxForwardMPH = 60f;
            config.maxReverseMPH = 25f;
            config.accelForce = 35f;
            config.brakeForce = 55f;
            config.steerSensitivity = 0.85f;
            config.steerSmoothing = 6f;
            config.lateralSpeedFactor = 0.003f;
            config.dragCoeff = 0.4f;
            config.rollingFriction = 1.8f;
            config.bodyColor = new Color(0.15f, 0.15f, 0.2f);
            config.interiorColor = new Color(0.12f, 0.1f, 0.09f);
            config.engineBasePitchHz = 35f;
            return config;
        }

        private static VehicleConfig CreateMuscleCar()
        {
            var config = CreateInstance<VehicleConfig>();
            config.displayName = "Muscle Car";
            config.description = "Fast and loud. Harder to control.";
            config.type = VehicleType.MuscleCar;
            config.maxForwardMPH = 80f;
            config.maxReverseMPH = 30f;
            config.accelForce = 50f;
            config.brakeForce = 45f;       // worse brakes
            config.steerSensitivity = 0.7f; // heavier steering
            config.steerSmoothing = 4f;
            config.lateralSpeedFactor = 0.004f; // more tail-happy
            config.dragCoeff = 0.3f;        // more slippery
            config.rollingFriction = 1.5f;
            config.bodyColor = new Color(0.6f, 0.1f, 0.1f); // red
            config.interiorColor = new Color(0.1f, 0.08f, 0.06f);
            config.engineIdlePitch = 0.5f;
            config.engineMaxPitch = 2.8f;
            config.engineBasePitchHz = 30f;  // deep V8 rumble
            return config;
        }

        private static VehicleConfig CreateSUV()
        {
            var config = CreateInstance<VehicleConfig>();
            config.displayName = "Stunt SUV";
            config.description = "Slow but forgiving. Easy to handle.";
            config.type = VehicleType.SUV;
            config.maxForwardMPH = 50f;
            config.maxReverseMPH = 20f;
            config.accelForce = 25f;        // sluggish
            config.brakeForce = 60f;        // good brakes
            config.steerSensitivity = 1.0f; // responsive
            config.steerSmoothing = 8f;     // very smooth
            config.lateralSpeedFactor = 0.002f; // stable
            config.dragCoeff = 0.5f;        // boxy = more drag
            config.rollingFriction = 2.2f;
            config.bodyColor = new Color(0.2f, 0.2f, 0.22f);
            config.interiorColor = new Color(0.14f, 0.12f, 0.1f);
            config.wheelScale = 1.2f;
            config.engineIdlePitch = 0.55f;
            config.engineMaxPitch = 2.2f;
            config.engineBasePitchHz = 40f;  // V6
            return config;
        }
    }

    public enum VehicleType
    {
        Sedan,
        MuscleCar,
        SUV
    }
}
