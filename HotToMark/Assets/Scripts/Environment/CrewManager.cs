using UnityEngine;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Environment
{
    /// <summary>
    /// Stage 5: BTS Film Crew manager.
    /// Spawns 8 crew members along both sides of the road.
    /// Each crew type has distinct visuals and equipment.
    /// </summary>
    public class CrewManager : MonoBehaviour
    {
        [Header("Crew Configuration")]
        [SerializeField] private float roadHalfWidth = 4.5f;
        [SerializeField] private float crewOffsetFromRoad = 2f;
        [SerializeField] private float visibleRange = 80f;

        private GameState state;
        private CrewMember[] crewMembers;
        private float worldScale = 0.3f;

        // Crew placement data: side (-1=left, 1=right), distance in feet, type
        private static readonly CrewPlacement[] placements = new CrewPlacement[]
        {
            new CrewPlacement(-1, 40,  CrewType.Safety),
            new CrewPlacement( 1, 50,  CrewType.PA),
            new CrewPlacement(-1, 80,  CrewType.Grip),
            new CrewPlacement( 1, 85,  CrewType.CameraOp),
            new CrewPlacement(-1, 150, CrewType.Director),
            new CrewPlacement( 1, 165, CrewType.Grip),
            new CrewPlacement(-1, 225, CrewType.Sound),
            new CrewPlacement( 1, 250, CrewType.CameraOp),
        };

        void Start()
        {
            state = GameManager.Instance.state;
        }

        void Update()
        {
            if (crewMembers == null) return;

            // Show/hide crew based on distance from car
            foreach (var crew in crewMembers)
            {
                if (crew == null || crew.root == null) continue;
                float dist = Mathf.Abs(crew.feetPosition - state.posY);
                crew.root.SetActive(dist < visibleRange);
            }
        }

        public void SpawnCrew()
        {
            DespawnCrew();
            crewMembers = new CrewMember[placements.Length];

            for (int i = 0; i < placements.Length; i++)
            {
                var p = placements[i];
                crewMembers[i] = CreateCrewMember(p, i);
            }
        }

        private void DespawnCrew()
        {
            if (crewMembers != null)
            {
                foreach (var crew in crewMembers)
                    if (crew != null && crew.root != null)
                        Destroy(crew.root);
            }
            crewMembers = null;
        }

        private CrewMember CreateCrewMember(CrewPlacement placement, int index)
        {
            var crew = new CrewMember();
            crew.type = placement.type;
            crew.feetPosition = placement.distanceFeet;

            float xPos = placement.side * (roadHalfWidth + crewOffsetFromRoad);
            float zPos = placement.distanceFeet * worldScale;

            crew.root = new GameObject($"Crew_{placement.type}_{index}");
            crew.root.transform.SetParent(transform);
            crew.root.transform.localPosition = new Vector3(xPos, 0, zPos);

            // Build the crew member visually
            BuildCrewVisual(crew);

            return crew;
        }

        private void BuildCrewVisual(CrewMember crew)
        {
            Color bodyColor = GetCrewColor(crew.type);
            float personHeight = 1.75f;

            // Body (capsule)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(crew.root.transform);
            body.transform.localPosition = new Vector3(0, personHeight * 0.4f, 0);
            body.transform.localScale = new Vector3(0.4f, personHeight * 0.35f, 0.3f);
            SetColor(body, bodyColor);
            Destroy(body.GetComponent<Collider>());

            // Head (sphere)
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(crew.root.transform);
            head.transform.localPosition = new Vector3(0, personHeight * 0.85f, 0);
            head.transform.localScale = Vector3.one * 0.25f;
            SetColor(head, new Color(0.87f, 0.72f, 0.6f)); // skin tone
            Destroy(head.GetComponent<Collider>());

            // Legs
            for (int i = -1; i <= 1; i += 2)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                leg.name = i < 0 ? "LeftLeg" : "RightLeg";
                leg.transform.SetParent(crew.root.transform);
                leg.transform.localPosition = new Vector3(i * 0.1f, personHeight * 0.12f, 0);
                leg.transform.localScale = new Vector3(0.15f, personHeight * 0.18f, 0.15f);
                SetColor(leg, bodyColor * 0.7f);
                Destroy(leg.GetComponent<Collider>());
            }

            // Type-specific equipment
            switch (crew.type)
            {
                case CrewType.CameraOp:
                    BuildCameraEquipment(crew.root.transform, personHeight);
                    break;
                case CrewType.Director:
                    BuildDirectorChair(crew.root.transform, personHeight);
                    break;
                case CrewType.Sound:
                    BuildBoomMic(crew.root.transform, personHeight);
                    break;
                case CrewType.Safety:
                    BuildSafetyVest(crew.root.transform, body, personHeight);
                    break;
                case CrewType.PA:
                    BuildWalkieTalkie(crew.root.transform, personHeight);
                    break;
            }
        }

        private void BuildCameraEquipment(Transform parent, float height)
        {
            // Camera body
            var cam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cam.name = "FilmCamera";
            cam.transform.SetParent(parent);
            cam.transform.localPosition = new Vector3(0.5f, height * 0.6f, 0);
            cam.transform.localScale = new Vector3(0.4f, 0.25f, 0.3f);
            SetColor(cam, new Color(0.12f, 0.12f, 0.12f));
            Destroy(cam.GetComponent<Collider>());

            // Lens
            var lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "Lens";
            lens.transform.SetParent(parent);
            lens.transform.localPosition = new Vector3(0.5f, height * 0.6f, 0.2f);
            lens.transform.localRotation = Quaternion.Euler(90, 0, 0);
            lens.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
            SetColor(lens, new Color(0.2f, 0.2f, 0.35f));
            Destroy(lens.GetComponent<Collider>());

            // Tripod legs
            for (int i = -1; i <= 1; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = $"TripodLeg_{i}";
                leg.transform.SetParent(parent);
                float xOffset = 0.5f + i * 0.15f;
                leg.transform.localPosition = new Vector3(xOffset, height * 0.3f, 0);
                leg.transform.localRotation = Quaternion.Euler(0, 0, i * 8);
                leg.transform.localScale = new Vector3(0.03f, height * 0.3f, 0.03f);
                SetColor(leg, new Color(0.3f, 0.3f, 0.3f));
                Destroy(leg.GetComponent<Collider>());
            }
        }

        private void BuildDirectorChair(Transform parent, float height)
        {
            // Chair seat
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "ChairSeat";
            seat.transform.SetParent(parent);
            seat.transform.localPosition = new Vector3(-0.5f, height * 0.25f, 0);
            seat.transform.localScale = new Vector3(0.5f, 0.05f, 0.4f);
            SetColor(seat, new Color(0.15f, 0.3f, 0.12f)); // dark green canvas
            Destroy(seat.GetComponent<Collider>());

            // Chair back
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "ChairBack";
            back.transform.SetParent(parent);
            back.transform.localPosition = new Vector3(-0.5f, height * 0.45f, -0.18f);
            back.transform.localScale = new Vector3(0.5f, 0.35f, 0.04f);
            SetColor(back, new Color(0.15f, 0.3f, 0.12f));
            Destroy(back.GetComponent<Collider>());

            // Chair legs
            for (int xi = -1; xi <= 1; xi += 2)
            {
                for (int zi = -1; zi <= 1; zi += 2)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "ChairLeg";
                    leg.transform.SetParent(parent);
                    leg.transform.localPosition = new Vector3(
                        -0.5f + xi * 0.2f, height * 0.12f, zi * 0.15f);
                    leg.transform.localScale = new Vector3(0.04f, height * 0.12f, 0.04f);
                    SetColor(leg, new Color(0.4f, 0.25f, 0.1f)); // wood
                    Destroy(leg.GetComponent<Collider>());
                }
            }
        }

        private void BuildBoomMic(Transform parent, float height)
        {
            // Boom pole
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "BoomPole";
            pole.transform.SetParent(parent);
            pole.transform.localPosition = new Vector3(0.3f, height * 0.9f, 0.5f);
            pole.transform.localRotation = Quaternion.Euler(60, 0, 0);
            pole.transform.localScale = new Vector3(0.03f, 1f, 0.03f);
            SetColor(pole, new Color(0.5f, 0.5f, 0.5f));
            Destroy(pole.GetComponent<Collider>());

            // Mic windscreen (fuzzy)
            var mic = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            mic.name = "MicWindscreen";
            mic.transform.SetParent(parent);
            mic.transform.localPosition = new Vector3(0.3f, height * 0.95f, 1.3f);
            mic.transform.localRotation = Quaternion.Euler(90, 0, 0);
            mic.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
            SetColor(mic, new Color(0.35f, 0.35f, 0.35f));
            Destroy(mic.GetComponent<Collider>());
        }

        private void BuildSafetyVest(Transform parent, GameObject body, float height)
        {
            // Reflective vest overlay
            var vest = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vest.name = "SafetyVest";
            vest.transform.SetParent(parent);
            vest.transform.localPosition = new Vector3(0, height * 0.4f, 0);
            vest.transform.localScale = new Vector3(0.42f, height * 0.2f, 0.32f);
            var vestMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            vestMat.color = new Color(1f, 0.5f, 0f);
            vestMat.EnableKeyword("_EMISSION");
            vestMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 0.3f);
            vest.GetComponent<Renderer>().material = vestMat;
            Destroy(vest.GetComponent<Collider>());

            // Reflective stripes
            for (int i = 0; i < 2; i++)
            {
                var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = $"ReflectiveStripe_{i}";
                stripe.transform.SetParent(parent);
                stripe.transform.localPosition = new Vector3(0, height * 0.35f + i * 0.15f, 0.17f);
                stripe.transform.localScale = new Vector3(0.42f, 0.03f, 0.01f);
                var stripeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                stripeMat.color = new Color(1f, 1f, 0f);
                stripeMat.EnableKeyword("_EMISSION");
                stripeMat.SetColor("_EmissionColor", Color.yellow * 0.5f);
                stripe.GetComponent<Renderer>().material = stripeMat;
                Destroy(stripe.GetComponent<Collider>());
            }
        }

        private void BuildWalkieTalkie(Transform parent, float height)
        {
            var walkie = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walkie.name = "WalkieTalkie";
            walkie.transform.SetParent(parent);
            walkie.transform.localPosition = new Vector3(0.2f, height * 0.55f, 0.1f);
            walkie.transform.localScale = new Vector3(0.06f, 0.15f, 0.04f);
            SetColor(walkie, new Color(0.15f, 0.15f, 0.15f));
            Destroy(walkie.GetComponent<Collider>());

            // Antenna
            var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.name = "Antenna";
            antenna.transform.SetParent(parent);
            antenna.transform.localPosition = new Vector3(0.2f, height * 0.68f, 0.1f);
            antenna.transform.localScale = new Vector3(0.015f, 0.06f, 0.015f);
            SetColor(antenna, new Color(0.2f, 0.2f, 0.2f));
            Destroy(antenna.GetComponent<Collider>());
        }

        private Color GetCrewColor(CrewType type)
        {
            switch (type)
            {
                case CrewType.Grip:     return new Color(0.33f, 0.33f, 0.33f);
                case CrewType.CameraOp: return new Color(0.2f, 0.2f, 0.2f);
                case CrewType.Director: return new Color(0.48f, 0.19f, 0.13f);
                case CrewType.Sound:    return new Color(0.19f, 0.31f, 0.5f);
                case CrewType.PA:       return new Color(0.31f, 0.5f, 0.19f);
                case CrewType.Safety:   return new Color(0.93f, 0.47f, 0f);
                default:                return Color.gray;
            }
        }

        private void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = color;
            }
        }
    }

    public enum CrewType
    {
        Grip, CameraOp, Director, Sound, PA, Safety
    }

    public class CrewMember
    {
        public CrewType type;
        public float feetPosition;
        public GameObject root;
    }

    public struct CrewPlacement
    {
        public int side;
        public float distanceFeet;
        public CrewType type;

        public CrewPlacement(int side, float dist, CrewType type)
        {
            this.side = side;
            this.distanceFeet = dist;
            this.type = type;
        }
    }
}
