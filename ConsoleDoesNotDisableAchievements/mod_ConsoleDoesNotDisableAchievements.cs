using Patchwork.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace V1ldConsoleDoesNotDisableAchievements
{
    [ModifiesType("CommandLine")]
    public static class V1ld_CommandLine
    {
        [ModifiesMember("IRoll20s")]
        public static void IRoll20s()
        {
            GameState.Instance.CheatsEnabled = !GameState.Instance.CheatsEnabled;
            //if (GameState.Instance.CheatsEnabled && AchievementTracker.Instance != null)
            //{
            //    AchievementTracker.Instance.DisableAchievements = true;
            //}
            if (GameState.Instance.CheatsEnabled)
            {
                Console.AddMessage("Console Enabled - Achievements are still enabled.");
            }
            else
            {
                Console.AddMessage("Console Disabled");
            }
        }
    }
}