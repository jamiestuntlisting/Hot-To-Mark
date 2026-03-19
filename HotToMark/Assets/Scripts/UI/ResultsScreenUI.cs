using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using HotToMark.Core;
using HotToMark.Scoring;
using System.Collections.Generic;

namespace HotToMark.UI
{
    /// <summary>
    /// Results screen shown after completing a take.
    /// Keyboard: Enter/Space to play again, Escape to go back to menu.
    /// Text sizes 3x for readability.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        private GameObject resultsPanel;

        // Dynamic text elements
        private TextMeshProUGUI headerText;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI gradeText;
        private TextMeshProUGUI markAccuracyLine;
        private TextMeshProUGUI returnToOneLine;
        private TextMeshProUGUI modeScoreLine;
        private TextMeshProUGUI statsText;
        private TextMeshProUGUI highScoreText;
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
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 8));

            // Header — 3x (36 -> 108)
            var headerObj = UIFactory.CreateText("Header", resultsPanel.transform,
                "TAKE COMPLETE", 108, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(headerObj, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f));
            headerText = headerObj.GetComponent<TextMeshProUGUI>();

            // Score — 3x (52 -> 156) — keep it reasonable at 80 to fit
            var scoreObj = UIFactory.CreateText("Score", resultsPanel.transform,
                "0/100", 80, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(scoreObj, new Vector2(0.05f, 0.72f), new Vector2(0.55f, 0.85f));
            scoreText = scoreObj.GetComponent<TextMeshProUGUI>();

            // Grade — 3x (72 -> 216) — cap at 120
            var gradeObj = UIFactory.CreateText("Grade", resultsPanel.transform,
                "F", 120, Color.red, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(gradeObj, new Vector2(0.55f, 0.72f), new Vector2(0.95f, 0.85f));
            gradeText = gradeObj.GetComponent<TextMeshProUGUI>();

            // Divider
            var divider = UIFactory.CreateImage("Divider", resultsPanel.transform,
                new Color(1f, 0.6f, 0, 0.3f));
            UIFactory.SetAnchors(divider, new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.705f));

            // Breakdown section header — 3x (14 -> 42)
            var breakdownLabel = UIFactory.CreateText("BreakdownLabel", resultsPanel.transform,
                "SCORE BREAKDOWN", 42, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(breakdownLabel, new Vector2(0.06f, 0.63f), new Vector2(0.94f, 0.70f));

            // Mark accuracy line — 3x (16 -> 48)
            var markObj = UIFactory.CreateText("MarkAccuracy", resultsPanel.transform,
                "", 48, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(markObj, new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.63f));
            markAccuracyLine = markObj.GetComponent<TextMeshProUGUI>();
            markAccuracyLine.richText = true;

            // Return to one line — 3x (16 -> 48)
            var returnObj = UIFactory.CreateText("ReturnToOne", resultsPanel.transform,
                "", 48, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(returnObj, new Vector2(0.06f, 0.47f), new Vector2(0.94f, 0.55f));
            returnToOneLine = returnObj.GetComponent<TextMeshProUGUI>();
            returnToOneLine.richText = true;

            // Mode score line — 3x (16 -> 48)
            var modeObj = UIFactory.CreateText("ModeScore", resultsPanel.transform,
                "", 48, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            UIFactory.SetAnchors(modeObj, new Vector2(0.06f, 0.39f), new Vector2(0.94f, 0.47f));
            modeScoreLine = modeObj.GetComponent<TextMeshProUGUI>();
            modeScoreLine.richText = true;

            // Penalties container
            var penaltiesObj = new GameObject("PenaltyContainer");
            penaltiesObj.transform.SetParent(resultsPanel.transform, false);
            var penaltyRect = penaltiesObj.AddComponent<RectTransform>();
            UIFactory.SetAnchors(penaltiesObj, new Vector2(0.06f, 0.26f), new Vector2(0.94f, 0.39f));
            penaltyContainer = penaltiesObj.transform;

            var vlg = penaltiesObj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;
            var csf = penaltiesObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Stats bar — 3x (12 -> 36)
            var statsBg = UIFactory.CreateImage("StatsBg", resultsPanel.transform,
                new Color(0.08f, 0.08f, 0.08f));
            UIFactory.SetAnchors(statsBg, new Vector2(0.05f, 0.19f), new Vector2(0.95f, 0.26f));

            var statsObj = UIFactory.CreateText("Stats", statsBg.transform,
                "", 36, new Color(0.7f, 0.7f, 0.7f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(statsObj, new Vector2(0, 0), new Vector2(1, 1));
            statsText = statsObj.GetComponent<TextMeshProUGUI>();

            // High score line — 3x (14 -> 42)
            var highScoreObj = UIFactory.CreateText("HighScore", resultsPanel.transform,
                "", 42, new Color(1f, 0.85f, 0.2f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(highScoreObj, new Vector2(0.1f, 0.13f), new Vector2(0.9f, 0.19f));
            highScoreText = highScoreObj.GetComponent<TextMeshProUGUI>();

            // Play Again button — 3x (18 -> 54)
            var playAgainObj = CreateResultButton("Play Again", resultsPanel.transform,
                new Vector2(0.06f, 0.02f), new Vector2(0.48f, 0.12f),
                new Color(1f, 0.6f, 0));
            playAgainButton = playAgainObj.GetComponent<Button>();
            playAgainButton.onClick.AddListener(OnPlayAgain);

            // Back to Menu button — 3x (18 -> 54)
            var menuBtnObj = CreateResultButton("Back to Menu", resultsPanel.transform,
                new Vector2(0.52f, 0.02f), new Vector2(0.94f, 0.12f),
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
                label, 54, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(textObj, new Vector2(0, 0), new Vector2(1, 1));

            return btnObj;
        }

        void Update()
        {
            if (resultsPanel == null || !resultsPanel.activeSelf) return;

            // Keyboard: Enter/Space to play again, Escape to go back to menu
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame
                || kb.spaceKey.wasPressedThisFrame)
            {
                OnPlayAgain();
            }
            else if (kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame)
            {
                OnBackToMenu();
            }
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
            markAccuracyLine.text = $"Mark: {result.markAccuracyPct:F1}%  —  " +
                $"<b>{result.markPoints}/40</b>{missed}";

            returnToOneLine.text = $"Return: {result.returnAccuracyPct:F1}%  —  " +
                $"<b>{result.returnPoints}/20</b>";

            modeScoreLine.text = $"{result.modeLabel}:  <b>{result.modePoints}/40</b>";

            // Clear and rebuild penalties
            foreach (Transform child in penaltyContainer)
                Destroy(child.gameObject);

            foreach (var penalty in result.penalties)
            {
                var lineObj = new GameObject("Penalty");
                lineObj.transform.SetParent(penaltyContainer, false);
                var le = lineObj.AddComponent<LayoutElement>();
                le.preferredHeight = 44;

                var tmp = lineObj.AddComponent<TextMeshProUGUI>();
                tmp.text = $"  -{penalty.points}:  {penalty.description}";
                tmp.fontSize = 42;
                tmp.color = new Color(1f, 0.33f, 0.33f);
            }

            statsText.text = $"Top: {result.topSpeed:F1} mph  |  " +
                $"Time: {result.totalTime:F1}s  |  " +
                $"Mark: {result.markDistance:F0} ft";

            // High score display
            int highScore = HighScoreManager.GetHighScore(state.mode);
            int plays = HighScoreManager.GetPlayCount(state.mode);
            if (result.totalScore >= highScore && plays > 0)
                highScoreText.text = "NEW HIGH SCORE!";
            else if (highScore > 0)
                highScoreText.text = $"Best: {highScore}/100 ({HighScoreManager.GetHighGrade(state.mode)})";
            else
                highScoreText.text = "";
        }

        public void Hide()
        {
            if (resultsPanel != null) resultsPanel.SetActive(false);
        }

        private void OnPlayAgain()
        {
            if (lastState != null && GameManager.Instance != null)
                GameManager.Instance.StartGame(lastState.mode);
        }

        private void OnBackToMenu()
        {
            GameManager.Instance?.ShowMenu();
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
