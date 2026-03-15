using UnityEngine;
using System.Collections;
using HotToMark.Vehicle;
using HotToMark.Environment;
using HotToMark.Camera;
using HotToMark.Scoring;
using HotToMark.UI;
using HotToMark.Audio;
using HotToMark.Haptics;

namespace HotToMark.Core
{
    /// <summary>
    /// Top-level orchestrator. Wires up all systems and manages phase transitions.
    /// Attach to a root GameObject in the scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("State")]
        public GameState state;

        [Header("References")]
        public CarController carController;
        public MarkSystem markSystem;
        public HornSystem hornSystem;
        public ScoreManager scoreManager;
        public MainMenuUI mainMenu;
        public ResultsScreenUI resultsScreen;
        public HUDManager hud;
        public EngineAudioController engineAudio;
        public HapticFeedback haptics;
        public CrewManager crewManager;
        public PIPCameraController pipCamera;
        public PauseMenuUI pauseMenu;
        public DirectorFeedback directorFeedback;
        public RoadManager roadManager;
        public WeatherSystem weatherSystem;
        public MultiMarkSystem multiMarkSystem;
        public ObstacleSystem obstacleSystem;
        public ReplaySystem replaySystem;
        public CareerMenuUI careerMenu;

        public static GameManager Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            if (state == null)
            {
                state = ScriptableObject.CreateInstance<GameState>();
            }
        }

        void Start()
        {
            // Defer to let SceneBootstrap finish wiring all references
            Invoke(nameof(ShowMenu), 0);
        }

        public void ShowMenu()
        {
            state.phase = GamePhase.Menu;
            if (engineAudio != null) engineAudio.StopEngine();
            if (mainMenu != null) mainMenu.Show();
            if (resultsScreen != null) resultsScreen.Hide();
            if (hud != null) hud.Hide();
            if (pipCamera != null) pipCamera.SetActive(false);
            if (pauseMenu != null) pauseMenu.Hide();
            if (careerMenu != null) careerMenu.Hide();
        }

        public void StartGame(GameMode mode)
        {
            state.Reset();
            state.takeNumber++;
            state.mode = mode;
            state.markDistance = Random.Range(GameState.MARK_MIN, GameState.MARK_MAX);
            state.targetMPH = Random.Range(15, 35);
            state.checkpointDistance = state.markDistance * 0.6f;

            // Apply selected vehicle (F-1)
            if (carController != null)
            {
                var configs = Vehicle.VehicleConfig.GetDefaults();
                int vIdx = (int)state.selectedVehicle;
                if (vIdx < configs.Length)
                    carController.ApplyVehicle(configs[vIdx]);
            }

            // Apply selected set/location (F-2)
            if (roadManager != null)
            {
                var sets = Environment.SetConfig.GetDefaults();
                int sIdx = (int)state.selectedSet;
                if (sIdx < sets.Length)
                    roadManager.ApplySet(sets[sIdx]);
            }

            // Apply weather (F-7)
            if (weatherSystem != null)
                weatherSystem.ApplyWeather(state.selectedWeather);

            // Obstacles (F-10) — spawn based on take number (difficulty ramps up)
            if (obstacleSystem != null)
            {
                int difficulty = Mathf.Min(state.takeNumber / 2, 3);
                obstacleSystem.SpawnObstacles(difficulty, state.markDistance);
            }

            // Multi-mark setup (F-8) — use multi-marks on higher takes
            if (multiMarkSystem != null)
            {
                MultiMarkPattern pattern = MultiMarkPattern.Single;
                if (state.takeNumber >= 6) pattern = MultiMarkPattern.ThreeMarks;
                else if (state.takeNumber >= 3) pattern = MultiMarkPattern.TwoMarks;
                multiMarkSystem.SetupPattern(pattern, state.markDistance);
            }

            // Start recording replay (F-5)
            if (replaySystem != null)
                replaySystem.StartRecording();

            if (mainMenu != null) mainMenu.Hide();
            if (careerMenu != null) careerMenu.Hide();
            if (resultsScreen != null) resultsScreen.Hide();
            if (hud != null) hud.Show();
            if (pipCamera != null) pipCamera.SetActive(true);
            if (markSystem != null) markSystem.SetupMark(state.markDistance);
            if (crewManager != null) crewManager.SpawnCrew();
            if (hornSystem != null) hornSystem.ResetHorn();
            if (pauseMenu != null) pauseMenu.Hide();

            // Film protocol sequence: "Rolling!" -> "Speed!" -> "Action!"
            state.phase = GamePhase.PreRoll;
            StartCoroutine(FilmProtocolSequence());
        }

        private System.Collections.IEnumerator FilmProtocolSequence()
        {
            // "Rolling!" — AD calls to signal cameras are recording
            if (engineAudio != null) engineAudio.PlayVoiceRolling();
            yield return new WaitForSeconds(1.2f);

            // "Speed!" — sound department confirms audio is recording
            if (engineAudio != null) engineAudio.PlayVoiceSpeed();
            yield return new WaitForSeconds(1.0f);

            // Start engine during the pause
            if (engineAudio != null) engineAudio.StartEngine();
            yield return new WaitForSeconds(0.8f);

            // "Action!" — director's call, player can now drive
            if (engineAudio != null) engineAudio.PlayVoiceAction();
            state.phase = GamePhase.Driving;
            state.takeStartTime = Time.time;
        }

        /// <summary>
        /// Called by CarController when car stops on the mark.
        /// </summary>
        public void OnStoppedOnMark(float accuracy)
        {
            state.markAccuracy = accuracy;
            state.speed = 0;
            state.phase = GamePhase.StoppedOnMark;

            if (haptics != null) haptics.TriggerMark();

            // "Cut!" voice call (Stage 8.6)
            if (engineAudio != null) engineAudio.PlayVoiceCut();

            // Director reacts to mark accuracy (F-6)
            if (directorFeedback != null) directorFeedback.OnMarkStop(accuracy);
        }

        /// <summary>
        /// Called by HornSystem after 2 honks + delay.
        /// </summary>
        public void OnShiftToReverse()
        {
            state.gear = Gear.Reverse;
            state.phase = GamePhase.Reversing;
            state.reverseStartTime = Time.time;

            // "Back to one!" — AD tells driver to return to starting position
            if (engineAudio != null) engineAudio.PlayVoiceBackToOne();
        }

        /// <summary>
        /// Called by CarController when car returns to start.
        /// </summary>
        public void OnReturnedToOne(float returnAccuracy)
        {
            state.speed = 0;
            state.reverseAccuracy = returnAccuracy;
            state.takeTime = Time.time - state.takeStartTime;

            float reverseTime = Time.time - state.reverseStartTime;
            state.reverseTooSlow = reverseTime > GameState.PENALTY_SLOW_TIME;
            state.reverseTooFast = state.reverseMaxSpeed > GameState.PENALTY_FAST_MPH;
            state.honkPenalty = state.honkCount < GameState.HONKS_REQUIRED;

            // Smoothness: scale jerk accumulation so typical play scores well
            // Average take accumulates ~20-60 jerk units; map to 0-100
            state.smoothnessScore = Mathf.Clamp(100f - state.jerkAccum * 1.2f, 0f, 100f);

            // Override mark accuracy with multi-mark aggregate if applicable (F-8)
            if (multiMarkSystem != null && multiMarkSystem.marks.Count > 1)
                state.markAccuracy = multiMarkSystem.aggregateAccuracy;

            state.phase = GamePhase.Results;

            // Stop replay recording (F-5)
            if (replaySystem != null) replaySystem.StopRecording();

            if (engineAudio != null) engineAudio.StopEngine();
            if (hud != null) hud.Hide();
            if (pipCamera != null) pipCamera.SetActive(false);

            if (scoreManager != null)
            {
                ScoreResult result = scoreManager.CalculateScore(state);

                // Obstacle penalties (F-10)
                if (obstacleSystem != null && obstacleSystem.obstaclesHit > 0)
                {
                    int obsPenalty = obstacleSystem.GetPenaltyPoints();
                    result.penalties.Add(new Penalty(
                        $"Hit {obstacleSystem.obstaclesHit} obstacle(s)", obsPenalty));
                    result.totalScore = Mathf.Max(0, result.totalScore - obsPenalty);
                }

                // Save high score (local + Game Center)
                HighScoreManager.SaveScore(state.mode, result.totalScore);
                if (GameCenterManager.Instance != null)
                    GameCenterManager.Instance.ReportScore(state.mode, result.totalScore);

                // Career mode scene completion (F-3)
                if (CareerManager.Instance != null)
                    CareerManager.Instance.CompleteScene(result.totalScore);

                // Director feedback on overall performance (F-6)
                if (directorFeedback != null) directorFeedback.OnTakeComplete(result);

                if (resultsScreen != null) resultsScreen.Show(result, state);
            }
        }

        // ---- Pause System ----

        public void TogglePause()
        {
            if (state.phase == GamePhase.Paused)
                ResumeGame();
            else if (state.phase != GamePhase.Menu && state.phase != GamePhase.Results)
                PauseGame();
        }

        public void PauseGame()
        {
            if (state.phase == GamePhase.Menu || state.phase == GamePhase.Results
                || state.phase == GamePhase.Paused || state.phase == GamePhase.PreRoll)
                return;

            state.phaseBeforePause = state.phase;
            state.phase = GamePhase.Paused;
            Time.timeScale = 0;

            if (pauseMenu != null) pauseMenu.Show();
        }

        public void ResumeGame()
        {
            if (state.phase != GamePhase.Paused) return;

            state.phase = state.phaseBeforePause;
            Time.timeScale = 1;

            if (pauseMenu != null) pauseMenu.Hide();
        }

        public void QuitToMenu()
        {
            Time.timeScale = 1;
            ShowMenu();
        }
    }
}
