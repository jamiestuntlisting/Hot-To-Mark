using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.Scoring;

namespace HotToMark.UI
{
    /// <summary>
    /// Stage 4: Results screen shown after completing a take.
    /// Self-builds its own UI hierarchy. Displays total score, letter grade,
    /// breakdown, penalties, and stats.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        private GameObject resultsPanel;

        // Dynamic text elements (set during Build, updated during Show)
        private TextMeshProUGUI headerText;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI gradeText;
        private TextMeshProUGUI markAccuracyLine;
        private TextMeshProUGUI returnToOneLine;
        private TextMeshProUGUI modeScoreLine;
        private TextMeshProUGUI statsText;
        private Transform penaltyContainer;
        private Button playAgainButton;
        private Button backToMenuButton;

        private GameState lastState;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            resultsPanel = UIFactory.CreatePanel("ResultsPanel", transform,
                new Color(0.03f, 0.03f, 0.03f, 0.94f));

            // Orange top bar
            var topBar = UIFactory.CreateImage("TopBar", resultsPanel.transform,
                new Color(1f, 0.6f, 0, 0.9f));
            UIFactory.SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 6));

            // Header
            var headerObj = UIFactory.CreateText("Header", resultsPanel.transform,
                "TAKE COMPLETE", 36, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(headerObj, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.96f));
            headerText = headerObj.GetComponent<TextMeshProUGUI>();

            // Score
            var scoreObj = UIFactory.CreateText("Score", resultsPanel.transform,
                "0/100", 52, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(scoreObj, new Vector2(0.1f, 0.76f), new Vector2(0.55f, 0.88f));
            scoreText = scoreObj.GetComponent<TextMeshProUGUI>();

            // Grade
            var gradeObj = UIFactory.CreateText("Grade", resultsPanel.transform,
                "F", 72, Color.red, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(gradeObj, new Vector2(0.55f, 0.76f), new Vector2(0.9f, 0.88f));
            gradeText = gradeObj.GetComponent<TextMeshProUGUI>();

            // Divider
            var divider = UIFactory.CreateImage("Divider", resultsPanel.transform,
                new Color(1f, 0.6f, 0, 0.3f));
            UIFactory.SetAnchors(divider, new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.745f));

            // Breakdown section header
            var breakdownLabel = UIFactory.CreateText("BreakdownLabel", resultsPanel.transform,
                "SCORE BREAKDOWN", 14, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(breakdownLabel, new Vector2(0.08f, 0.68f), new Vector2(0.9f, 0.73f));

            // Mark accuracy line
            var markObj = UIFactory.CreateText("MarkAccuracy", resultsPanel.transform,
                "", 16, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(markObj, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.68f));
            markAccuracyLine = markObj.GetComponent<TextMeshProUGUI>();
            markAccuracyLine.richText = true;

            // Return to one line
            var returnObj = UIFactory.CreateText("ReturnToOne", resultsPanel.transform,
                "", 16, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(returnObj, new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.62f));
            returnToOneLine = returnObj.GetComponent<TextMeshProUGUI>();
            returnToOneLine.richText = true;

            // Mode score line
            var modeObj = UIFactory.CreateText("ModeScore", resultsPanel.transform,
                "", 16, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(modeObj, new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.56f));
            modeScoreLine = modeObj.GetComponent<TextMeshProUGUI>();
            modeScoreLine.richText = true;

            // Penalties container
            var penaltiesObj = new GameObject("PenaltyContainer");
            penaltiesObj.transform.SetParent(resultsPanel.transform, false);
            var penaltyRect = penaltiesObj.AddComponent<RectTransform>();
            UIFactory.SetAnchors(penaltiesObj, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.48f));
            penaltyContainer = penaltiesObj.transform;

            // Add a vertical layout group for penalties
            var vlg = penaltiesObj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            var csf = penaltiesObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Stats bar
            var statsBg = UIFactory.CreateImage("StatsBg", resultsPanel.transform,
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetAnchors(statsBg, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.28f));

            var statsObj = UIFactory.CreateText("Stats", statsBg.transform,
                "", 12, new Color(0.7f, 0.7f, 0.7f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(statsObj, new Vector2(0, 0), new Vector2(1, 1));
            statsText = statsObj.GetComponent<TextMeshProUGUI>();

            // Buttons
            var playAgainObj = CreateResultButton("Play Again", resultsPanel.transform,
                new Vector2(0.08f, 0.08f), new Vector2(0.48f, 0.17f),
                new Color(1f, 0.6f, 0));
            playAgainButton = playAgainObj.GetComponent<Button>();
            playAgainButton.onClick.AddListener(OnPlayAgain);

            var menuBtnObj = CreateResultButton("Back to Menu", resultsPanel.transform,
                new Vector2(0.52f, 0.08f), new Vector2(0.92f, 0.17f),
                new Color(0.3f, 0.3f, 0.3f));
            backToMenuButton = menuBtnObj.GetComponent<Button>();
            backToMenuButton.onClick.AddListener(OnBackToMenu);

            resultsPanel.SetActive(false);
        }

        private GameObject CreateResultButton(string label, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
        {
            var btnObj = UIFactory.CreateImage($"Btn_{label}", parent, bgColor);
            UIFactory.SetAnchors(btnObj, anchorMin, anchorMax);

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            var textObj = UIFactory.CreateText("Label", btnObj.transform,
                label, 18, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(textObj, new Vector2(0, 0), new Vector2(1, 1));

            return btnObj;
        }

        public void Show(ScoreResult result, GameState state)
        {
            lastState = state;
            if (resultsPanel != null) resultsPanel.SetActive(true);

            headerText.text = "TAKE COMPLETE";
            scoreText.text = $"{result.totalScore}/100";
            gradeText.text = result.grade;
            gradeText.color = GradeColor(result.grade);

            string missed = result.markAccuracyPct <= 0 ? " <color=#FF5555>MISSED!</color>" : "";
            markAccuracyLine.text = $"Mark Accuracy: {result.markAccuracyPct:F1}%  —  " +
                $"<b>{result.markPoints}/40 pts</b>{missed}";

            returnToOneLine.text = $"Return to One: {result.returnAccuracyPct:F1}%  —  " +
                $"<b>{result.returnPoints}/20 pts</b>";

            modeScoreLine.text = $"{result.modeLabel}:  <b>{result.modePoints}/40 pts</b>";

            // Clear and rebuild penalties
            foreach (Transform child in penaltyContainer)
                Destroy(child.gameObject);

            foreach (var penalty in result.penalties)
            {
                var lineObj = new GameObject("Penalty");
                lineObj.transform.SetParent(penaltyContainer, false);
                var le = lineObj.AddComponent<LayoutElement>();
                le.preferredHeight = 22;

                var tmp = lineObj.AddComponent<TextMeshProUGUI>();
                tmp.text = $"  -{penalty.points}:  {penalty.description}";
                tmp.fontSize = 14;
                tmp.color = new Color(1f, 0.33f, 0.33f);
            }

            statsText.text = $"Top Speed: {result.topSpeed:F1} mph   |   " +
                $"Total Time: {result.totalTime:F1}s   |   " +
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
