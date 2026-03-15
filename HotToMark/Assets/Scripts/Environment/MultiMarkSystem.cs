using UnityEngine;
using System.Collections.Generic;
using HotToMark.Core;
using HotToMark.Vehicle;

namespace HotToMark.Environment
{
    /// <summary>
    /// F-8: Multi-Mark Takes — defines a sequence of marks the player must
    /// hit in order during a single take. Each mark has a position and
    /// optional speed requirement. The system feeds back accuracy per mark
    /// and computes an aggregate score.
    /// </summary>
    public class MultiMarkSystem : MonoBehaviour
    {
        public List<MarkTarget> marks = new List<MarkTarget>();
        public int currentMarkIndex;
        public bool allMarksComplete;
        public float aggregateAccuracy;

        private GameState state;
        private List<GameObject> markVisuals = new List<GameObject>();

        void Update()
        {
            if (state == null && GameManager.Instance != null)
                state = GameManager.Instance.state;
            if (state == null) return;

            if (state.phase == GamePhase.Driving && marks.Count > 1)
                CheckCurrentMark();
        }

        /// <summary>
        /// Set up a multi-mark sequence. Pass distances in feet.
        /// </summary>
        public void SetupMarks(float[] distances)
        {
            ClearVisuals();
            marks.Clear();
            currentMarkIndex = 0;
            allMarksComplete = false;
            aggregateAccuracy = 0;

            for (int i = 0; i < distances.Length; i++)
            {
                marks.Add(new MarkTarget
                {
                    distance = distances[i],
                    label = i == distances.Length - 1 ? "FINAL MARK" : $"MARK {i + 1}",
                    hit = false,
                    accuracy = 0
                });
                SpawnMarkVisual(distances[i], i, i == distances.Length - 1);
            }
        }

        /// <summary>
        /// Set up marks from a preset pattern.
        /// </summary>
        public void SetupPattern(MultiMarkPattern pattern, float totalDistance)
        {
            switch (pattern)
            {
                case MultiMarkPattern.TwoMarks:
                    SetupMarks(new float[] {
                        totalDistance * 0.5f,
                        totalDistance
                    });
                    break;

                case MultiMarkPattern.ThreeMarks:
                    SetupMarks(new float[] {
                        totalDistance * 0.3f,
                        totalDistance * 0.65f,
                        totalDistance
                    });
                    break;

                case MultiMarkPattern.SlowFast:
                    // First mark close, second far
                    SetupMarks(new float[] {
                        totalDistance * 0.25f,
                        totalDistance
                    });
                    break;

                case MultiMarkPattern.StaggeredFour:
                    SetupMarks(new float[] {
                        totalDistance * 0.2f,
                        totalDistance * 0.45f,
                        totalDistance * 0.7f,
                        totalDistance
                    });
                    break;

                default: // Single
                    SetupMarks(new float[] { totalDistance });
                    break;
            }
        }

        private void CheckCurrentMark()
        {
            if (allMarksComplete) return;
            if (currentMarkIndex >= marks.Count) return;

            var mark = marks[currentMarkIndex];
            float distFromMark = Mathf.Abs(state.posY - mark.distance);
            float threshold = mark.distance * 0.05f; // 5% tolerance

            // Player must slow down near the mark
            bool nearMark = state.posY >= mark.distance - threshold;
            bool slowEnough = Mathf.Abs(state.speed) < 2f;

            if (nearMark && slowEnough)
            {
                float accuracy = Mathf.Max(0, 100f - (distFromMark / threshold) * 100f);
                mark.accuracy = accuracy;
                mark.hit = true;
                marks[currentMarkIndex] = mark;

                // Visual feedback — flash the mark
                if (currentMarkIndex < markVisuals.Count)
                    HighlightMark(markVisuals[currentMarkIndex], accuracy);

                currentMarkIndex++;

                if (currentMarkIndex >= marks.Count)
                {
                    allMarksComplete = true;
                    CalculateAggregate();
                }
            }
            // If player drives past without stopping, mark as missed
            else if (state.posY > mark.distance + threshold * 2 && !mark.hit)
            {
                mark.accuracy = 0;
                mark.hit = true;
                marks[currentMarkIndex] = mark;
                currentMarkIndex++;

                if (currentMarkIndex >= marks.Count)
                {
                    allMarksComplete = true;
                    CalculateAggregate();
                }
            }
        }

        private void CalculateAggregate()
        {
            float total = 0;
            foreach (var m in marks) total += m.accuracy;
            aggregateAccuracy = marks.Count > 0 ? total / marks.Count : 0;
        }

        private void SpawnMarkVisual(float distance, int index, bool isFinal)
        {
            float worldScale = 0.3f;
            float zPos = distance * worldScale;

            // Cross-road tape mark
            var markObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            markObj.name = $"MultiMark_{index}";
            markObj.transform.SetParent(transform);
            markObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            markObj.transform.localPosition = new Vector3(0, 0.025f, zPos);
            markObj.transform.localScale = new Vector3(8f, 0.15f, 1);

            var renderer = markObj.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            Color markColor = isFinal
                ? new Color(1f, 0.2f, 0.2f)        // Red final mark
                : new Color(1f, 0.6f, 0, 0.8f);     // Orange intermediate
            renderer.material.color = markColor;

            Destroy(markObj.GetComponent<Collider>());
            markVisuals.Add(markObj);

            // Label above mark
            // (In a full implementation, this would be a world-space TextMesh.
            //  For now we use a small cube as a visual indicator.)
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.name = $"MarkIndicator_{index}";
            indicator.transform.SetParent(markObj.transform);
            indicator.transform.localPosition = new Vector3(0, 0, 50f); // above mark
            indicator.transform.localScale = new Vector3(0.05f, 0.05f, 3f);
            var indRenderer = indicator.GetComponent<Renderer>();
            indRenderer.material = new Material(Shader.Find("Standard"));
            indRenderer.material.color = markColor;
            Destroy(indicator.GetComponent<Collider>());
        }

        private void HighlightMark(GameObject visual, float accuracy)
        {
            if (visual == null) return;
            var renderer = visual.GetComponent<Renderer>();
            if (renderer == null) return;

            // Green for good, yellow for ok, red for miss
            Color hitColor;
            if (accuracy >= 80) hitColor = new Color(0.2f, 1f, 0.3f);
            else if (accuracy >= 40) hitColor = new Color(1f, 0.8f, 0.2f);
            else hitColor = new Color(1f, 0.3f, 0.3f);

            renderer.material.color = hitColor;
        }

        private void ClearVisuals()
        {
            foreach (var v in markVisuals)
                if (v != null) Destroy(v);
            markVisuals.Clear();
        }

        void OnDestroy()
        {
            ClearVisuals();
        }
    }

    [System.Serializable]
    public struct MarkTarget
    {
        public float distance;
        public string label;
        public bool hit;
        public float accuracy;
    }

    public enum MultiMarkPattern
    {
        Single,
        TwoMarks,
        ThreeMarks,
        SlowFast,
        StaggeredFour
    }
}
