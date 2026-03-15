using UnityEngine;
using HotToMark.Core;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif

namespace HotToMark.Scoring
{
    /// <summary>
    /// F-4: Game Center integration for leaderboards.
    /// Authenticates the player and reports scores per game mode.
    /// Falls back gracefully when Game Center is unavailable.
    /// </summary>
    public class GameCenterManager : MonoBehaviour
    {
        public static GameCenterManager Instance { get; private set; }

        public bool IsAuthenticated { get; private set; }

        // Leaderboard IDs — must match App Store Connect configuration
        private const string LB_STANDARD = "com.stuntgames.hottomark.standard";
        private const string LB_SPEEDRUN = "com.stuntgames.hottomark.speedrun";
        private const string LB_SMOOTH = "com.stuntgames.hottomark.smooth";
        private const string LB_EXACTMPH = "com.stuntgames.hottomark.exactmph";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            Authenticate();
        }

        /// <summary>
        /// Authenticate with Game Center. Shows sign-in UI on first launch.
        /// </summary>
        public void Authenticate()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Social.localUser.Authenticate((bool success) => {
                IsAuthenticated = success;
                if (success)
                    Debug.Log("[HotToMark] Game Center authenticated: " + Social.localUser.userName);
                else
                    Debug.Log("[HotToMark] Game Center authentication failed (offline or declined)");
            });
#else
            Debug.Log("[HotToMark] Game Center not available (not iOS device)");
            IsAuthenticated = false;
#endif
        }

        /// <summary>
        /// Report a score to the appropriate leaderboard for the given mode.
        /// </summary>
        public void ReportScore(GameMode mode, int score)
        {
            if (!IsAuthenticated) return;

            string leaderboardId = GetLeaderboardId(mode);
            if (string.IsNullOrEmpty(leaderboardId)) return;

#if UNITY_IOS && !UNITY_EDITOR
            Social.ReportScore(score, leaderboardId, (bool success) => {
                if (success)
                    Debug.Log($"[HotToMark] Score {score} reported to {leaderboardId}");
                else
                    Debug.LogWarning($"[HotToMark] Failed to report score to {leaderboardId}");
            });
#endif
        }

        /// <summary>
        /// Show the Game Center leaderboard UI for a specific mode.
        /// </summary>
        public void ShowLeaderboard(GameMode mode)
        {
            if (!IsAuthenticated)
            {
                Debug.Log("[HotToMark] Cannot show leaderboard — not authenticated");
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            GameCenterPlatform.ShowLeaderboardUI(GetLeaderboardId(mode), UnityEngine.SocialPlatforms.TimeScope.AllTime);
#else
            Social.ShowLeaderboardUI();
#endif
        }

        /// <summary>
        /// Show all leaderboards.
        /// </summary>
        public void ShowAllLeaderboards()
        {
            if (!IsAuthenticated) return;
            Social.ShowLeaderboardUI();
        }

        private string GetLeaderboardId(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Standard: return LB_STANDARD;
                case GameMode.SpeedRun: return LB_SPEEDRUN;
                case GameMode.SmoothOperator: return LB_SMOOTH;
                case GameMode.ExactMPH: return LB_EXACTMPH;
                default: return LB_STANDARD;
            }
        }
    }
}
