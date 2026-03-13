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

        private GameState state;
        private GameObject roadSurface;
        private GameObject[] centerDashes;
        private GameObject leftEdgeLine;
        private GameObject rightEdgeLine;
        private float worldScale = 0.3f;

        void Start()
        {
            state = GameManager.Instance.state;
            BuildRoad();
            BuildCenterLine();
            BuildEdgeLines();
            BuildGround();
        }

        void Update()
        {
            if (state == null) return;
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
            var leftGrass = GameObject.CreatePrimitive(PrimitiveType.Quad);
            leftGrass.name = "LeftGrass";
            leftGrass.transform.SetParent(transform);
            leftGrass.transform.localRotation = Quaternion.Euler(90, 0, 0);
            leftGrass.transform.localPosition = new Vector3(-roadWidth / 2f - 25f, 0, roadLength * worldScale / 2f);
            leftGrass.transform.localScale = new Vector3(50f, roadLength * worldScale, 1);
            if (grassMaterial != null)
                leftGrass.GetComponent<Renderer>().material = grassMaterial;
            else
                SetDefaultColor(leftGrass, new Color(0.35f, 0.54f, 0.23f));
            Destroy(leftGrass.GetComponent<Collider>());

            // Right grass
            var rightGrass = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rightGrass.name = "RightGrass";
            rightGrass.transform.SetParent(transform);
            rightGrass.transform.localRotation = Quaternion.Euler(90, 0, 0);
            rightGrass.transform.localPosition = new Vector3(roadWidth / 2f + 25f, 0, roadLength * worldScale / 2f);
            rightGrass.transform.localScale = new Vector3(50f, roadLength * worldScale, 1);
            if (grassMaterial != null)
                rightGrass.GetComponent<Renderer>().material = grassMaterial;
            else
                SetDefaultColor(rightGrass, new Color(0.35f, 0.54f, 0.23f));
            Destroy(rightGrass.GetComponent<Collider>());
        }

        private void SetDefaultColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = color;
            }
        }
    }
}
