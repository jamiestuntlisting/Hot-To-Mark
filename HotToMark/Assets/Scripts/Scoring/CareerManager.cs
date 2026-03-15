using UnityEngine;
using System.Collections.Generic;
using HotToMark.Core;
using HotToMark.Environment;

namespace HotToMark.Scoring
{
    /// <summary>
    /// F-3: Career Mode — progression through increasingly difficult scenes.
    /// Tracks which scenes are unlocked, star ratings, and current progress.
    /// Uses PlayerPrefs for persistence.
    /// </summary>
    public class CareerManager : MonoBehaviour
    {
        public static CareerManager Instance { get; private set; }

        public CareerScene[] scenes;
        public int currentSceneIndex;

        private const string PREFS_PREFIX = "career_";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildScenes();
            LoadProgress();
        }

        private void BuildScenes()
        {
            scenes = new CareerScene[]
            {
                // Act 1: Learning the ropes
                new CareerScene("The Audition", "Drive to your mark. Simple.",
                    SetType.Backlot, WeatherCondition.Clear, GameMode.Standard,
                    markRange: new Vector2(100, 150), targetScore: 40),

                new CareerScene("First Day on Set", "A little further this time.",
                    SetType.Backlot, WeatherCondition.Clear, GameMode.Standard,
                    markRange: new Vector2(150, 250), targetScore: 50),

                new CareerScene("Speed Test", "The director wants it fast.",
                    SetType.Backlot, WeatherCondition.Clear, GameMode.SpeedRun,
                    markRange: new Vector2(200, 250), targetScore: 45),

                // Act 2: On location
                new CareerScene("Desert Chase", "Hit the desert. Hit the mark.",
                    SetType.Desert, WeatherCondition.Clear, GameMode.Standard,
                    markRange: new Vector2(200, 300), targetScore: 55),

                new CareerScene("Smooth Criminal", "The director wants it smooth.",
                    SetType.Desert, WeatherCondition.GoldenHour, GameMode.SmoothOperator,
                    markRange: new Vector2(200, 250), targetScore: 50),

                new CareerScene("Rush Hour", "Hit exactly 25 mph at the checkpoint.",
                    SetType.City, WeatherCondition.Overcast, GameMode.ExactMPH,
                    targetMPH: 25, markRange: new Vector2(250, 300), targetScore: 55),

                // Act 3: Difficult conditions
                new CareerScene("Rain Scene", "Wet roads. Stay focused.",
                    SetType.City, WeatherCondition.Rain, GameMode.Standard,
                    markRange: new Vector2(200, 300), targetScore: 55),

                new CareerScene("Golden Hour Rush", "The light is fading. Move fast.",
                    SetType.Desert, WeatherCondition.GoldenHour, GameMode.SpeedRun,
                    markRange: new Vector2(250, 350), targetScore: 55),

                new CareerScene("Night Shoot", "No room for error in the dark.",
                    SetType.City, WeatherCondition.Night, GameMode.SmoothOperator,
                    markRange: new Vector2(200, 300), targetScore: 60),

                // Act 4: The big leagues
                new CareerScene("Parking Garage Chase", "Tight space. Get it right.",
                    SetType.ParkingStructure, WeatherCondition.Clear, GameMode.Standard,
                    markRange: new Vector2(150, 200), targetScore: 65),

                new CareerScene("The Long Take", "350 feet. One shot. Nailed it.",
                    SetType.Backlot, WeatherCondition.Clear, GameMode.Standard,
                    markRange: new Vector2(340, 350), targetScore: 70),

                new CareerScene("Director's Cut", "All skills. Perfect score or nothing.",
                    SetType.City, WeatherCondition.Night, GameMode.Standard,
                    markRange: new Vector2(250, 350), targetScore: 80),
            };
        }

        /// <summary>
        /// Start a career scene by index.
        /// </summary>
        public void StartScene(int index)
        {
            if (index < 0 || index >= scenes.Length) return;
            if (!scenes[index].unlocked) return;

            currentSceneIndex = index;
            var scene = scenes[index];
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Configure game state for this scene
            gm.state.selectedSet = scene.setType;
            gm.state.selectedWeather = scene.weather;

            // Override mark distance range
            float mark = Random.Range(scene.markRange.x, scene.markRange.y);

            // Override target MPH if specified
            if (scene.targetMPH > 0)
                gm.state.targetMPH = scene.targetMPH;

            gm.StartGame(scene.mode);

            // Override the random mark with our scene-specific one
            gm.state.markDistance = mark;
        }

        /// <summary>
        /// Called when a career take finishes. Returns star count earned (0-3).
        /// </summary>
        public int CompleteScene(int score)
        {
            if (currentSceneIndex < 0 || currentSceneIndex >= scenes.Length) return 0;

            var scene = scenes[currentSceneIndex];
            int stars = CalculateStars(score, scene.targetScore);

            if (stars > scene.bestStars)
            {
                scene.bestStars = stars;
                SaveProgress();
            }

            if (score > scene.bestScore)
                scene.bestScore = score;

            // Unlock next scene if at least 1 star
            if (stars >= 1 && currentSceneIndex + 1 < scenes.Length)
            {
                scenes[currentSceneIndex + 1].unlocked = true;
                SaveProgress();
            }

            return stars;
        }

        private int CalculateStars(int score, int target)
        {
            if (score >= target + 20) return 3;   // Well above target
            if (score >= target) return 2;          // Met the target
            if (score >= target - 15) return 1;     // Close enough to pass
            return 0;                                // Failed
        }

        public int TotalStars()
        {
            int total = 0;
            foreach (var s in scenes) total += s.bestStars;
            return total;
        }

        public int MaxStars()
        {
            return scenes.Length * 3;
        }

        private void SaveProgress()
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                PlayerPrefs.SetInt($"{PREFS_PREFIX}unlocked_{i}", scenes[i].unlocked ? 1 : 0);
                PlayerPrefs.SetInt($"{PREFS_PREFIX}stars_{i}", scenes[i].bestStars);
                PlayerPrefs.SetInt($"{PREFS_PREFIX}score_{i}", scenes[i].bestScore);
            }
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            // Scene 0 always unlocked
            scenes[0].unlocked = true;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (PlayerPrefs.HasKey($"{PREFS_PREFIX}unlocked_{i}"))
                    scenes[i].unlocked = PlayerPrefs.GetInt($"{PREFS_PREFIX}unlocked_{i}") == 1;
                scenes[i].bestStars = PlayerPrefs.GetInt($"{PREFS_PREFIX}stars_{i}", 0);
                scenes[i].bestScore = PlayerPrefs.GetInt($"{PREFS_PREFIX}score_{i}", 0);
            }

            // Ensure first is always unlocked
            scenes[0].unlocked = true;
        }
    }

    [System.Serializable]
    public class CareerScene
    {
        public string name;
        public string description;
        public SetType setType;
        public WeatherCondition weather;
        public GameMode mode;
        public Vector2 markRange;
        public float targetMPH;
        public int targetScore;

        public bool unlocked;
        public int bestStars;    // 0-3
        public int bestScore;

        public CareerScene(string name, string description, SetType setType,
            WeatherCondition weather, GameMode mode,
            Vector2 markRange, int targetScore, float targetMPH = 0)
        {
            this.name = name;
            this.description = description;
            this.setType = setType;
            this.weather = weather;
            this.mode = mode;
            this.markRange = markRange;
            this.targetMPH = targetMPH;
            this.targetScore = targetScore;
            this.unlocked = false;
            this.bestStars = 0;
            this.bestScore = 0;
        }
    }
}
