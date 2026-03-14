using UnityEngine;
using HotToMark.Core;

namespace HotToMark.Scoring
{
    /// <summary>
    /// Stage 7: Configuration for each challenge mode.
    /// Provides display names, descriptions, and scoring parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "HotToMark/GameModeConfig")]
    public class GameModeConfig : ScriptableObject
    {
        public GameMode mode;
        public string displayName;
        [TextArea] public string description;

        public static GameModeConfig[] GetDefaults()
        {
            return new GameModeConfig[]
            {
                CreateConfig(GameMode.Standard, "Standard Take",
                    "Drive to the mark, stop accurately, reverse to one."),
                CreateConfig(GameMode.SpeedRun, "Speed Run",
                    "Complete the entire take as fast as possible."),
                CreateConfig(GameMode.SmoothOperator, "Smooth Operator",
                    "Minimize jerky inputs. Be cinematic."),
                CreateConfig(GameMode.ExactMPH, "Exact MPH",
                    "Hit exactly the target speed at the checkpoint."),
            };
        }

        private static GameModeConfig CreateConfig(GameMode mode, string name, string desc)
        {
            var config = ScriptableObject.CreateInstance<GameModeConfig>();
            config.mode = mode;
            config.displayName = name;
            config.description = desc;
            return config;
        }
    }
}
