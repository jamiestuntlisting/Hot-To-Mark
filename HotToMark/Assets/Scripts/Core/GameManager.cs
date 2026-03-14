using UnityEngine;
using HotToMark.Vehicle;
using HotToMark.Environment;
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
        }

        public void StartGame(GameMode mode)
        {
            state.Reset();
            state.phase = GamePhase.Driving;
            state.mode = mode;
            state.markDistance = Random.Range(GameState.MARK_MIN, GameState.MARK_MAX);
            state.targetMPH = Random.Range(15, 35);
            state.checkpointDistance = state.markDistance * 0.6f;
            state.takeStartTime = Time.time;

            if (mainMenu != null) mainMenu.Hide();
            if (resultsScreen != null) resultsScreen.Hide();
            if (hud != null) hud.Show();
            if (pipCamera != null) pipCamera.SetActive(true);
            if (markSystem != null) markSystem.SetupMark(state.markDistance);
            if (engineAudio != null) engineAudio.StartEngine();
            if (crewManager != null) crewManager.SpawnCrew();

            // "Action!" voice call (Stage 8.5)
            if (engineAudio != null) engineAudio.PlayVoiceAction();
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
        }

        /// <summary>
        /// Called by HornSystem after 2 honks + delay.
        /// </summary>
        public void OnShiftToReverse()
        {
            state.gear = Gear.Reverse;
            state.phase = GamePhase.Reversing;
            state.reverseStartTime = Time.time;
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
            state.smoothnessScore = Mathf.Max(0, 100f - state.jerkAccum * 2f);

            state.phase = GamePhase.Results;

            if (engineAudio != null) engineAudio.StopEngine();
            if (hud != null) hud.Hide();
            if (pipCamera != null) pipCamera.SetActive(false);

            ScoreResult result = scoreManager.CalculateScore(state);
            if (resultsScreen != null) resultsScreen.Show(result, state);
        }
    }
}
