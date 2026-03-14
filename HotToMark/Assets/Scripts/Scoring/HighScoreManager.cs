using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Scoring
{
    /// <summary>
    /// Persistent local high scores using PlayerPrefs.
    /// Stores top score and top grade for each game mode.
    /// </summary>
    public static class HighScoreManager
    {
        private static string KeyFor(GameMode mode) => $"HighScore_{mode}";
        private static string GradeKeyFor(GameMode mode) => $"HighGrade_{mode}";
        private static string PlaysKeyFor(GameMode mode) => $"PlayCount_{mode}";

        public static int GetHighScore(GameMode mode)
        {
            return PlayerPrefs.GetInt(KeyFor(mode), 0);
        }

        public static string GetHighGrade(GameMode mode)
        {
            return PlayerPrefs.GetString(GradeKeyFor(mode), "-");
        }

        public static int GetPlayCount(GameMode mode)
        {
            return PlayerPrefs.GetInt(PlaysKeyFor(mode), 0);
        }

        public static bool SaveScore(GameMode mode, int score)
        {
            // Increment play count
            int plays = GetPlayCount(mode) + 1;
            PlayerPrefs.SetInt(PlaysKeyFor(mode), plays);

            bool isNewHigh = score > GetHighScore(mode);
            if (isNewHigh)
            {
                PlayerPrefs.SetInt(KeyFor(mode), score);

                // Calculate and store grade
                string grade = "F";
                if (score >= 95) grade = "A+";
                else if (score >= 85) grade = "A";
                else if (score >= 75) grade = "B+";
                else if (score >= 65) grade = "B";
                else if (score >= 55) grade = "C";
                else if (score >= 40) grade = "D";
                PlayerPrefs.SetString(GradeKeyFor(mode), grade);

                PlayerPrefs.Save();
            }

            return isNewHigh;
        }

        public static void ClearAll()
        {
            foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
            {
                PlayerPrefs.DeleteKey(KeyFor(mode));
                PlayerPrefs.DeleteKey(GradeKeyFor(mode));
                PlayerPrefs.DeleteKey(PlaysKeyFor(mode));
            }
            PlayerPrefs.Save();
        }
    }
}
