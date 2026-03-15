using UnityEngine;

namespace HotToMark.Environment
{
    /// <summary>
    /// F-2: Filming location configuration — defines visual theme for different sets.
    /// Each set has unique road, ground, sky, and prop colors.
    /// </summary>
    [CreateAssetMenu(fileName = "SetConfig", menuName = "HotToMark/SetConfig")]
    public class SetConfig : ScriptableObject
    {
        public string displayName;
        public string description;
        public SetType type;

        [Header("Road")]
        public Color roadColor = new Color(0.24f, 0.24f, 0.24f);
        public Color centerLineColor = new Color(0.93f, 0.78f, 0f);
        public Color edgeLineColor = Color.white;

        [Header("Ground")]
        public Color groundColor = new Color(0.35f, 0.54f, 0.23f);

        [Header("Sky")]
        public Color skyTint = new Color(0.4f, 0.6f, 0.9f);
        public Color groundTint = new Color(0.35f, 0.5f, 0.25f);
        public float skyExposure = 1.2f;

        [Header("Lighting")]
        public Color sunColor = new Color(1f, 0.95f, 0.85f);
        public float sunIntensity = 1.2f;
        public Vector3 sunAngle = new Vector3(45, -30, 0);
        public Color ambientSky = new Color(0.6f, 0.7f, 0.9f);
        public Color ambientEquator = new Color(0.5f, 0.55f, 0.6f);
        public Color ambientGround = new Color(0.3f, 0.35f, 0.25f);

        [Header("Props")]
        public bool hasTrees = true;
        public bool hasBarriers = false;
        public bool hasBuildingSilhouettes = false;

        public static SetConfig[] GetDefaults()
        {
            return new SetConfig[]
            {
                CreateBacklot(),
                CreateDesertHighway(),
                CreateCityStreet(),
                CreateParkingStructure()
            };
        }

        private static SetConfig CreateBacklot()
        {
            var config = CreateInstance<SetConfig>();
            config.displayName = "Studio Backlot";
            config.description = "Classic film lot. Green grass, blue sky.";
            config.type = SetType.Backlot;
            // All defaults are fine for the classic look
            config.hasTrees = true;
            return config;
        }

        private static SetConfig CreateDesertHighway()
        {
            var config = CreateInstance<SetConfig>();
            config.displayName = "Desert Highway";
            config.description = "Scorching heat. Endless road.";
            config.type = SetType.Desert;
            config.roadColor = new Color(0.3f, 0.28f, 0.25f);  // faded asphalt
            config.centerLineColor = new Color(0.9f, 0.75f, 0.1f);
            config.groundColor = new Color(0.72f, 0.6f, 0.38f); // sand
            config.skyTint = new Color(0.55f, 0.7f, 0.95f);
            config.groundTint = new Color(0.65f, 0.55f, 0.35f);
            config.skyExposure = 1.5f;
            config.sunColor = new Color(1f, 0.9f, 0.7f);        // hot sun
            config.sunIntensity = 1.5f;
            config.sunAngle = new Vector3(70, -20, 0);           // high noon
            config.ambientSky = new Color(0.75f, 0.7f, 0.6f);
            config.ambientEquator = new Color(0.6f, 0.55f, 0.45f);
            config.ambientGround = new Color(0.5f, 0.45f, 0.3f);
            config.hasTrees = false;
            return config;
        }

        private static SetConfig CreateCityStreet()
        {
            var config = CreateInstance<SetConfig>();
            config.displayName = "City Street";
            config.description = "Downtown. Buildings on both sides.";
            config.type = SetType.City;
            config.roadColor = new Color(0.2f, 0.2f, 0.22f);    // darker asphalt
            config.centerLineColor = Color.white;                 // white dashes
            config.edgeLineColor = new Color(0.9f, 0.9f, 0);     // yellow curb
            config.groundColor = new Color(0.4f, 0.4f, 0.42f);   // sidewalk gray
            config.skyTint = new Color(0.5f, 0.55f, 0.7f);       // hazy
            config.groundTint = new Color(0.4f, 0.4f, 0.42f);
            config.skyExposure = 1.0f;
            config.sunIntensity = 0.9f;
            config.sunAngle = new Vector3(35, -45, 0);
            config.ambientSky = new Color(0.5f, 0.55f, 0.65f);
            config.ambientEquator = new Color(0.45f, 0.45f, 0.5f);
            config.ambientGround = new Color(0.35f, 0.35f, 0.38f);
            config.hasTrees = false;
            config.hasBuildingSilhouettes = true;
            return config;
        }

        private static SetConfig CreateParkingStructure()
        {
            var config = CreateInstance<SetConfig>();
            config.displayName = "Parking Structure";
            config.description = "Tight space. Concrete everywhere.";
            config.type = SetType.ParkingStructure;
            config.roadColor = new Color(0.35f, 0.35f, 0.33f);   // concrete
            config.centerLineColor = Color.white;
            config.edgeLineColor = new Color(1f, 0.8f, 0);        // painted yellow
            config.groundColor = new Color(0.32f, 0.32f, 0.3f);
            config.skyTint = new Color(0.4f, 0.4f, 0.45f);        // overcast/indoor
            config.groundTint = new Color(0.35f, 0.35f, 0.35f);
            config.skyExposure = 0.6f;                              // dim
            config.sunColor = new Color(0.95f, 0.93f, 0.85f);
            config.sunIntensity = 0.6f;
            config.sunAngle = new Vector3(80, 0, 0);               // overhead fluorescent
            config.ambientSky = new Color(0.4f, 0.4f, 0.42f);
            config.ambientEquator = new Color(0.38f, 0.38f, 0.4f);
            config.ambientGround = new Color(0.3f, 0.3f, 0.3f);
            config.hasTrees = false;
            config.hasBarriers = true;
            return config;
        }
    }

    public enum SetType
    {
        Backlot,
        Desert,
        City,
        ParkingStructure
    }
}
