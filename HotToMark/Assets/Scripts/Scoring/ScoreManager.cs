using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Scoring
{
    /// <summary>
    /// Stage 4 + 7: Calculates final score from game state.
    /// Handles all four challenge modes with mode-specific scoring.
    /// Score = Mark Accuracy (40) + Return to One (20) + Mode Performance (40) - Penalties
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public ScoreResult CalculateScore(GameState state)
        {
            var result = new ScoreResult();

            // Mark accuracy → up to 40 pts
            result.markAccuracyPct = state.markAccuracy;
            result.markPoints = Mathf.RoundToInt(state.markAccuracy * 0.4f);

            // Return to one → up to 20 pts
            result.returnAccuracyPct = state.reverseAccuracy;
            result.returnPoints = Mathf.RoundToInt(state.reverseAccuracy * 0.2f);

            // Mode-specific → up to 40 pts
            CalculateModeScore(state, result);

            // Penalties
            if (state.honkPenalty)
            {
                result.penalties.Add(new Penalty("Failed to honk twice before reversing",
                    GameState.PENALTY_POINTS));
            }
            if (state.reverseTooSlow)
            {
                result.penalties.Add(new Penalty(
                    $"Took too long to reverse (>{GameState.PENALTY_SLOW_TIME}s)",
                    GameState.PENALTY_POINTS));
            }
            if (state.reverseTooFast)
            {
                result.penalties.Add(new Penalty(
                    $"Reversed too fast ({state.reverseMaxSpeed:F1} mph > {GameState.PENALTY_FAST_MPH})",
                    GameState.PENALTY_POINTS));
            }

            int totalPenalties = 0;
            foreach (var p in result.penalties)
                totalPenalties += p.points;

            result.totalScore = Mathf.Max(0,
                result.markPoints + result.returnPoints + result.modePoints - totalPenalties);
            result.grade = CalculateGrade(result.totalScore);

            // Stats
            result.topSpeed = state.maxSpeedHit;
            result.totalTime = state.takeTime;
            result.markDistance = state.markDistance;

            return result;
        }

        private void CalculateModeScore(GameState state, ScoreResult result)
        {
            switch (state.mode)
            {
                case GameMode.SpeedRun:
                    result.modePoints = Mathf.Max(0,
                        Mathf.RoundToInt(40f - state.takeTime * 1.5f));
                    result.modeLabel = $"Speed Bonus ({state.takeTime:F1}s)";
                    break;

                case GameMode.SmoothOperator:
                    result.modePoints = Mathf.RoundToInt(state.smoothnessScore * 0.4f);
                    result.modeLabel = $"Smoothness: {state.smoothnessScore:F0}%";
                    break;

                case GameMode.ExactMPH:
                    result.modePoints = Mathf.RoundToInt(
                        (state.exactMPHAccuracy > 0 ? state.exactMPHAccuracy : 0) * 0.4f);
                    result.modeLabel = $"Exact MPH (target {state.targetMPH}, " +
                                       $"hit {state.speedAtCheckpoint:F1})";
                    break;

                default: // Standard
                    int timePts = Mathf.Max(0, Mathf.RoundToInt(20f - state.takeTime * 0.8f));
                    int smoothPts = Mathf.RoundToInt(state.smoothnessScore * 0.2f);
                    result.modePoints = timePts + smoothPts;
                    result.modeLabel = "Performance (time + smoothness)";
                    break;
            }
        }

        private string CalculateGrade(int score)
        {
            if (score >= 95) return "A+";
            if (score >= 85) return "A";
            if (score >= 75) return "B+";
            if (score >= 65) return "B";
            if (score >= 55) return "C";
            if (score >= 40) return "D";
            return "F";
        }
    }

    [System.Serializable]
    public class ScoreResult
    {
        public int totalScore;
        public string grade;

        public float markAccuracyPct;
        public int markPoints;        // out of 40
        public float returnAccuracyPct;
        public int returnPoints;      // out of 20
        public int modePoints;        // out of 40
        public string modeLabel;

        public System.Collections.Generic.List<Penalty> penalties
            = new System.Collections.Generic.List<Penalty>();

        public float topSpeed;
        public float totalTime;
        public float markDistance;
    }

    [System.Serializable]
    public struct Penalty
    {
        public string description;
        public int points;

        public Penalty(string desc, int pts)
        {
            description = desc;
            points = pts;
        }
    }
}
