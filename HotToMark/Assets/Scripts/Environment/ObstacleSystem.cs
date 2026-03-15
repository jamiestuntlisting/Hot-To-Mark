using UnityEngine;
using System.Collections.Generic;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Environment
{
    /// <summary>
    /// F-10: Obstacle Avoidance — spawns cones, crew members, equipment, and
    /// other obstacles along the road. The player must steer around them.
    /// Hitting obstacles incurs penalties. Obstacles are spawned procedurally
    /// based on difficulty.
    /// </summary>
    public class ObstacleSystem : MonoBehaviour
    {
        public int obstaclesHit;
        public int totalObstacles;
        public float penaltyPerHit = 3f;

        private GameState state;
        private List<Obstacle> obstacles = new List<Obstacle>();
        private float worldScale = 0.3f;
        private float roadWidth = 8f;
        private float checkRadius = 0.8f;

        void Update()
        {
            if (state == null && GameManager.Instance != null)
                state = GameManager.Instance.state;
            if (state == null) return;

            if (state.phase == GamePhase.Driving || state.phase == GamePhase.Reversing)
                CheckCollisions();
        }

        /// <summary>
        /// Spawn obstacles along the road based on difficulty (0 = none, 1 = easy, 3 = hard).
        /// </summary>
        public void SpawnObstacles(int difficulty, float markDistance)
        {
            ClearObstacles();
            obstaclesHit = 0;

            if (difficulty <= 0) return;

            int count = difficulty * 3 + Random.Range(0, 3);
            float minZ = 30f;  // don't spawn too close to start
            float maxZ = markDistance * worldScale - 10f;

            for (int i = 0; i < count; i++)
            {
                float z = Mathf.Lerp(minZ, maxZ, (float)(i + 1) / (count + 1));
                z += Random.Range(-3f, 3f);

                float halfRoad = roadWidth / 2f - 1f;
                float x = Random.Range(-halfRoad, halfRoad);

                ObstacleType type = PickObstacleType(difficulty);
                SpawnObstacle(type, x, z);
            }

            totalObstacles = obstacles.Count;
        }

        private ObstacleType PickObstacleType(int difficulty)
        {
            float r = Random.value;
            if (difficulty <= 1)
            {
                // Easy: mostly cones
                if (r < 0.7f) return ObstacleType.Cone;
                return ObstacleType.AppleBox;
            }
            else if (difficulty <= 2)
            {
                // Medium: mix
                if (r < 0.4f) return ObstacleType.Cone;
                if (r < 0.65f) return ObstacleType.CFlag;
                if (r < 0.85f) return ObstacleType.AppleBox;
                return ObstacleType.LightStand;
            }
            else
            {
                // Hard: more crew and equipment
                if (r < 0.25f) return ObstacleType.Cone;
                if (r < 0.45f) return ObstacleType.CFlag;
                if (r < 0.6f) return ObstacleType.LightStand;
                if (r < 0.8f) return ObstacleType.AppleBox;
                return ObstacleType.CrewMember;
            }
        }

        private void SpawnObstacle(ObstacleType type, float x, float z)
        {
            GameObject obj;
            Color color;
            Vector3 scale;

            switch (type)
            {
                case ObstacleType.Cone:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    scale = new Vector3(0.3f, 0.35f, 0.3f);
                    color = new Color(1f, 0.4f, 0);  // orange cone
                    break;

                case ObstacleType.CFlag:
                    // C-stand flag (tall thin rectangle)
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    scale = new Vector3(0.08f, 1.5f, 0.6f);
                    color = new Color(0.15f, 0.15f, 0.15f); // black flag
                    break;

                case ObstacleType.AppleBox:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    scale = new Vector3(0.5f, 0.2f, 0.35f);
                    color = new Color(0.6f, 0.45f, 0.25f);  // wooden
                    break;

                case ObstacleType.LightStand:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    scale = new Vector3(0.15f, 1.2f, 0.15f);
                    color = new Color(0.3f, 0.3f, 0.3f);     // metal gray
                    break;

                case ObstacleType.CrewMember:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    scale = new Vector3(0.3f, 0.8f, 0.3f);
                    color = new Color(0.2f, 0.35f, 0.2f);    // green vest
                    break;

                default:
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    scale = new Vector3(0.3f, 0.3f, 0.3f);
                    color = Color.red;
                    break;
            }

            obj.name = $"Obstacle_{type}_{obstacles.Count}";
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(x, scale.y / 2f, z);
            obj.transform.localScale = scale;

            var renderer = obj.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;

            // Remove physics collider (we do manual distance check)
            Destroy(obj.GetComponent<Collider>());

            obstacles.Add(new Obstacle
            {
                visual = obj,
                type = type,
                worldX = x,
                worldZ = z,
                hit = false,
                radius = Mathf.Max(scale.x, scale.z) * 0.5f + 0.2f
            });
        }

        private void CheckCollisions()
        {
            float carZ = state.posY * worldScale;
            float carX = state.posX * 3f; // match CarController lateral scaling

            foreach (var obs in obstacles)
            {
                if (obs.hit) continue;
                if (obs.visual == null) continue;

                float dx = carX - obs.worldX;
                float dz = carZ - obs.worldZ;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist < obs.radius + checkRadius)
                {
                    obs.hit = true;
                    obstaclesHit++;

                    // Visual feedback: knock it over
                    KnockOver(obs);

                    // Haptic feedback
                    var haptics = GameManager.Instance?.haptics;
                    if (haptics != null) haptics.TriggerRumble();
                }
            }
        }

        private void KnockOver(Obstacle obs)
        {
            if (obs.visual == null) return;

            // Tilt the obstacle
            float knockDir = state.steering > 0 ? 1f : -1f;
            obs.visual.transform.localRotation = Quaternion.Euler(
                Random.Range(60, 90) * knockDir,
                Random.Range(-30, 30),
                Random.Range(-20, 20));

            // Shift it aside
            obs.visual.transform.localPosition += new Vector3(
                knockDir * 0.5f, -0.1f, 0.2f);

            // Dim it
            var renderer = obs.visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color *= 0.5f;
        }

        public void ClearObstacles()
        {
            foreach (var obs in obstacles)
                if (obs.visual != null) Destroy(obs.visual);
            obstacles.Clear();
            totalObstacles = 0;
            obstaclesHit = 0;
        }

        void OnDestroy()
        {
            ClearObstacles();
        }

        /// <summary>
        /// Get penalty points from hitting obstacles.
        /// </summary>
        public int GetPenaltyPoints()
        {
            return Mathf.RoundToInt(obstaclesHit * penaltyPerHit);
        }
    }

    public class Obstacle
    {
        public GameObject visual;
        public ObstacleType type;
        public float worldX;
        public float worldZ;
        public bool hit;
        public float radius;
    }

    public enum ObstacleType
    {
        Cone,
        CFlag,        // C-stand flag
        AppleBox,     // wooden box
        LightStand,   // tall light
        CrewMember    // person with green vest
    }
}
