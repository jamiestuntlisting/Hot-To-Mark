using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.Scoring;

namespace HotToMark.UI
{
    /// <summary>
    /// Stage 4: Results screen shown after completing a take.
    /// Displays total score, letter grade, breakdown, penalties, and stats.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject resultsPanel;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI gradeText;

        [Header("Breakdown")]
        [SerializeField] private TextMeshProUGUI markAccuracyLine;
        [SerializeField] private TextMeshProUGUI returnToOneLine;
        [SerializeField] private TextMeshProUGUI modeScoreLine;

        [Header("Penalties")]
        [SerializeField] private Transform penaltyContainer;
        [SerializeField] private GameObject penaltyLinePrefab;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backToMenuButton;

        private GameState lastState;

        void Start()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgain);
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenu);
        }

        public void Show(ScoreResult result, GameState state)
        {
            lastState = state;

            if (resultsPanel != null) resultsPanel.SetActive(true);

            // Header
            if (headerText != null)
                headerText.text = "TAKE COMPLETE";

            if (scoreText != null)
                scoreText.text = $"{result.totalScore}/100";

            if (gradeText != null)
            {
                gradeText.text = result.grade;
                gradeText.color = GradeColor(result.grade);
            }

            // Breakdown
            if (markAccuracyLine != null)
            {
                string missed = result.markAccuracyPct <= 0 ? " <color=#FF5555>MISSED!</color>" : "";
                markAccuracyLine.text = $"Mark Accuracy: {result.markAccuracyPct:F1}% — " +
                    $"<b>{result.markPoints}/40 pts</b>{missed}";
            }

            if (returnToOneLine != null)
                returnToOneLine.text = $"Return to One: {result.returnAccuracyPct:F1}% — " +
                    $"<b>{result.returnPoints}/20 pts</b>";

            if (modeScoreLine != null)
                modeScoreLine.text = $"{result.modeLabel}: <b>{result.modePoints}/40 pts</b>";

            // Penalties
            if (penaltyContainer != null)
            {
                // Clear previous penalty lines
                foreach (Transform child in penaltyContainer)
                    Destroy(child.gameObject);

                foreach (var penalty in result.penalties)
                {
                    if (penaltyLinePrefab != null)
                    {
                        var line = Instantiate(penaltyLinePrefab, penaltyContainer);
                        var tmp = line.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.text = $"-{penalty.points}: {penalty.description}";
                            tmp.color = new Color(1f, 0.33f, 0.33f);
                        }
                    }
                }
            }

            // Stats
            if (statsText != null)
                statsText.text = $"Top Speed: {result.topSpeed:F1} mph  |  " +
                    $"Total Time: {result.totalTime:F1}s  |  " +
                    $"Mark: {result.markDistance:F0} ft";
        }

        public void Hide()
        {
            if (resultsPanel != null) resultsPanel.SetActive(false);
        }

        private void OnPlayAgain()
        {
            if (lastState != null)
                GameManager.Instance.StartGame(lastState.mode);
        }

        private void OnBackToMenu()
        {
            GameManager.Instance.ShowMenu();
        }

        private Color GradeColor(string grade)
        {
            switch (grade)
            {
                case "A+": return new Color(0.2f, 1f, 0.2f);
                case "A":  return new Color(0.4f, 1f, 0.4f);
                case "B+": return new Color(0.6f, 0.9f, 0.3f);
                case "B":  return new Color(0.8f, 0.8f, 0.2f);
                case "C":  return new Color(1f, 0.6f, 0.2f);
                case "D":  return new Color(1f, 0.4f, 0.2f);
                default:   return new Color(1f, 0.2f, 0.2f);
            }
        }
    }
}
