using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Environment
{
    /// <summary>
    /// Stage 2: Generates and scrolls the road environment.
    /// Creates road surface, lane markings, grass, and skybox at runtime.
    /// Uses object pooling for dashed center lines.
    /// </summary>
    public class RoadManager : MonoBehaviour
    {
        [Header("Road Dimensions")]
        [SerializeField] private float roadWidth = 8f;         // meters
        [SerializeField] private float roadLength = 200f;       // visible meters
        [SerializeField] private int roadSegments = 40;

        [Header("Materials")]
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Material centerLineMaterial;
        [SerializeField] private Material edgeLineMaterial;

        [Header("Center Line")]
        [SerializeField] private float dashLength = 3f;
        [SerializeField] private float gapLength = 4f;
        [SerializeField] private float lineWidth = 0.12f;
        [SerializeField] private int dashPoolSize = 30;

        [Header("Set")]
        public SetConfig activeSet;

        private GameState state;
        private GameObject roadSurface;
        private GameObject[] centerDashes;
        private GameObject leftEdgeLine;
        private GameObject rightEdgeLine;
        private GameObject leftGround;
        private GameObject rightGround;
        private GameObject[] props;
        private float worldScale = 0.3f;

        void Start()
        {
            BuildRoad();
            BuildCenterLine();
            BuildEdgeLines();
            BuildGround();
        }

        void Update()
        {
            if (state == null)
            {
                if (GameManager.Instance != null)
                    state = GameManager.Instance.state;
                return;
            }
            UpdateCenterLineDashes();
        }

        private void BuildRoad()
        {
            roadSurface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            roadSurface.name = "RoadSurface";
            roadSurface.transform.SetParent(transform);
            roadSurface.transform.localRotation = Quaternion.Euler(90, 0, 0);
            roadSurface.transform.localPosition = new Vector3(0, 0.01f, roadLength * worldScale / 2f);
            roadSurface.transform.localScale = new Vector3(roadWidth, roadLength * worldScale, 1);

            if (roadMaterial != null)
                roadSurface.GetComponent<Renderer>().material = roadMaterial;
            else
                SetDefaultColor(roadSurface, new Color(0.24f, 0.24f, 0.24f));

            // Disable collider on visual-only quad
            Destroy(roadSurface.GetComponent<Collider>());
        }

        private void BuildCenterLine()
        {
            centerDashes = new GameObject[dashPoolSize];
            for (int i = 0; i < dashPoolSize; i++)
            {
                var dash = GameObject.CreatePrimitive(PrimitiveType.Quad);
                dash.name = $"CenterDash_{i}";
                dash.transform.SetParent(transform);
                dash.transform.localRotation = Quaternion.Euler(90, 0, 0);
                dash.transform.localScale = new Vector3(lineWidth, dashLength * worldScale, 1);

                if (centerLineMaterial != null)
                    dash.GetComponent<Renderer>().material = centerLineMaterial;
                else
                    SetDefaultColor(dash, new Color(0.93f, 0.78f, 0));

                Destroy(dash.GetComponent<Collider>());
                centerDashes[i] = dash;
            }
        }

        private void UpdateCenterLineDashes()
        {
            float scrollOffset = state.posY * worldScale;
            float totalDashCycle = dashLength + gapLength;
            float startZ = scrollOffset - (scrollOffset % (totalDashCycle * worldScale));

            for (int i = 0; i < dashPoolSize; i++)
            {
                float z = startZ + i * totalDashCycle * worldScale - scrollOffset;
                centerDashes[i].transform.localPosition = new Vector3(0, 0.02f, z + dashLength * worldScale / 2f);

                // Hide dashes that are behind the camera
                bool visible = z > -5f && z < roadLength * worldScale;
                centerDashes[i].SetActive(visible);
            }
        }

        private void BuildEdgeLines()
        {
            float halfRoad = roadWidth / 2f;

            leftEdgeLine = CreateEdgeLine("LeftEdge", -halfRoad);
            rightEdgeLine = CreateEdgeLine("RightEdge", halfRoad);
        }

        private GameObject CreateEdgeLine(string name, float xPos)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.name = name;
            line.transform.SetParent(transform);
            line.transform.localRotation = Quaternion.Euler(90, 0, 0);
            line.transform.localPosition = new Vector3(xPos, 0.02f, roadLength * worldScale / 2f);
            line.transform.localScale = new Vector3(lineWidth * 1.5f, roadLength * worldScale, 1);

            if (edgeLineMaterial != null)
                line.GetComponent<Renderer>().material = edgeLineMaterial;
            else
                SetDefaultColor(line, Color.white);

            Destroy(line.GetComponent<Collider>());
            return line;
        }

        private void BuildGround()
        {
            // Left grass
            leftGround = GameObject.CreatePrimitive(PrimitiveType.Quad);
            leftGround.name = "LeftGrass";
            leftGround.transform.SetParent(transform);
            leftGround.transform.localRotation = Quaternion.Euler(90, 0, 0);
            leftGround.transform.localPosition = new Vector3(-roadWidth / 2f - 25f, 0, roadLength * worldScale / 2f);
            leftGround.transform.localScale = new Vector3(50f, roadLength * worldScale, 1);
            if (grassMaterial != null)
                leftGround.GetComponent<Renderer>().material = grassMaterial;
            else
                SetDefaultColor(leftGround, new Color(0.35f, 0.54f, 0.23f));
            Destroy(leftGround.GetComponent<Collider>());

            // Right grass
            rightGround = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rightGround.name = "RightGrass";
            rightGround.transform.SetParent(transform);
            rightGround.transform.localRotation = Quaternion.Euler(90, 0, 0);
            rightGround.transform.localPosition = new Vector3(roadWidth / 2f + 25f, 0, roadLength * worldScale / 2f);
            rightGround.transform.localScale = new Vector3(50f, roadLength * worldScale, 1);
            if (grassMaterial != null)
                rightGround.GetComponent<Renderer>().material = grassMaterial;
            else
                SetDefaultColor(rightGround, new Color(0.35f, 0.54f, 0.23f));
            Destroy(rightGround.GetComponent<Collider>());
        }

        /// <summary>
        /// Apply a set configuration, changing road/ground/sky colors and spawning props.
        /// </summary>
        public void ApplySet(SetConfig config)
        {
            if (config == null) return;
            activeSet = config;

            // Road
            SetColor(roadSurface, config.roadColor);

            // Center line dashes
            if (centerDashes != null)
            {
                foreach (var dash in centerDashes)
                    SetColor(dash, config.centerLineColor);
            }

            // Edge lines
            SetColor(leftEdgeLine, config.edgeLineColor);
            SetColor(rightEdgeLine, config.edgeLineColor);

            // Ground
            SetColor(leftGround, config.groundColor);
            SetColor(rightGround, config.groundColor);

            // Skybox
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_SkyTint", config.skyTint);
                RenderSettings.skybox.SetColor("_GroundColor", config.groundTint);
                RenderSettings.skybox.SetFloat("_Exposure", config.skyExposure);
            }

            // Lighting
            var sun = FindAnyObjectByType<Light>();
            if (sun != null)
            {
                sun.color = config.sunColor;
                sun.intensity = config.sunIntensity;
                sun.transform.rotation = Quaternion.Euler(config.sunAngle);
            }
            RenderSettings.ambientSkyColor = config.ambientSky;
            RenderSettings.ambientEquatorColor = config.ambientEquator;
            RenderSettings.ambientGroundColor = config.ambientGround;

            // Clean old props
            ClearProps();

            // Spawn set-specific props
            if (config.hasTrees) SpawnTrees();
            if (config.hasBuildingSilhouettes) SpawnBuildings();
            if (config.hasBarriers) SpawnBarriers();
        }

        private void ClearProps()
        {
            if (props != null)
            {
                foreach (var p in props)
                    if (p != null) Destroy(p);
            }
            props = null;
        }

        private void SpawnTrees()
        {
            var treeList = new System.Collections.Generic.List<GameObject>();
            float halfRoad = roadWidth / 2f;

            for (int i = 0; i < 16; i++)
            {
                float side = (i % 2 == 0) ? -1f : 1f;
                float xPos = side * (halfRoad + 8f + Random.Range(0f, 10f));
                float zPos = i * 8f * worldScale;

                // Trunk
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = $"Tree_{i}";
                trunk.transform.SetParent(transform);
                trunk.transform.localPosition = new Vector3(xPos, 1.5f, zPos);
                trunk.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
                SetColor(trunk, new Color(0.35f, 0.25f, 0.15f));
                Destroy(trunk.GetComponent<Collider>());

                // Canopy
                var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.name = $"Canopy_{i}";
                canopy.transform.SetParent(trunk.transform);
                canopy.transform.localPosition = new Vector3(0, 1.1f, 0);
                canopy.transform.localScale = new Vector3(4f, 2.5f, 4f);
                SetColor(canopy, new Color(0.2f + Random.Range(0f, 0.1f),
                    0.45f + Random.Range(0f, 0.15f), 0.15f));
                Destroy(canopy.GetComponent<Collider>());

                treeList.Add(trunk);
            }
            props = treeList.ToArray();
        }

        private void SpawnBuildings()
        {
            var buildingList = new System.Collections.Generic.List<GameObject>();
            float halfRoad = roadWidth / 2f;

            for (int i = 0; i < 12; i++)
            {
                float side = (i % 2 == 0) ? -1f : 1f;
                float xPos = side * (halfRoad + 6f + Random.Range(0f, 4f));
                float zPos = i * 10f * worldScale;
                float height = Random.Range(4f, 12f);
                float width = Random.Range(3f, 6f);

                var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{i}";
                building.transform.SetParent(transform);
                building.transform.localPosition = new Vector3(xPos, height / 2f, zPos);
                building.transform.localScale = new Vector3(width, height, width * 0.8f);

                float shade = Random.Range(0.25f, 0.45f);
                SetColor(building, new Color(shade, shade, shade + 0.02f));
                Destroy(building.GetComponent<Collider>());

                // Random window glow
                if (Random.value > 0.5f)
                {
                    var window = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    window.transform.SetParent(building.transform);
                    window.transform.localPosition = new Vector3(side * -0.51f, 0.2f, 0);
                    window.transform.localRotation = Quaternion.Euler(0, side > 0 ? 90 : -90, 0);
                    window.transform.localScale = new Vector3(0.6f, 0.3f, 1f);
                    var windowMat = new Material(Shader.Find("Standard"));
                    windowMat.color = new Color(0.8f, 0.75f, 0.5f);
                    windowMat.SetColor("_EmissionColor", new Color(0.4f, 0.35f, 0.2f));
                    windowMat.EnableKeyword("_EMISSION");
                    window.GetComponent<Renderer>().material = windowMat;
                    Destroy(window.GetComponent<Collider>());
                }

                buildingList.Add(building);
            }
            props = buildingList.ToArray();
        }

        private void SpawnBarriers()
        {
            var barrierList = new System.Collections.Generic.List<GameObject>();
            float halfRoad = roadWidth / 2f;

            for (int i = 0; i < 20; i++)
            {
                float side = (i % 2 == 0) ? -1f : 1f;
                float xPos = side * (halfRoad + 0.5f);
                float zPos = i * 5f * worldScale;

                var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                barrier.name = $"Barrier_{i}";
                barrier.transform.SetParent(transform);
                barrier.transform.localPosition = new Vector3(xPos, 0.4f, zPos);
                barrier.transform.localScale = new Vector3(0.3f, 0.8f, 1.5f);
                SetColor(barrier, new Color(0.85f, 0.85f, 0.2f)); // yellow jersey barrier
                Destroy(barrier.GetComponent<Collider>());

                barrierList.Add(barrier);
            }
            props = barrierList.ToArray();
        }

        private void SetColor(GameObject go, Color color)
        {
            if (go == null) return;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = color;
            }
        }

        private void SetDefaultColor(GameObject go, Color color)
        {
            SetColor(go, color);
        }
    }
}
