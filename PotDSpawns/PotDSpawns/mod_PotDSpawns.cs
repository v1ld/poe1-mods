using Patchwork.Attributes;
using UnityEngine;

namespace PotDSpawns
{
    [ModifiesType]
    public class v1ld_Encounter : Encounter
    {
        [ModifiesMember("DoesSpawnAppear")]
        new public bool DoesSpawnAppear(DifficultySettings settings)
        {
            if (settings.RequiresAnyOf != 0 && !DifficultyScaling.Instance.IsAnyScalerActive(settings.RequiresAnyOf))
            {
                return false;
            }
            GameDifficulty difficulty = GameDifficulty.Normal;
            if (GameState.Instance.CurrentNextMap != null)
            {
                difficulty = GameState.Instance.CurrentNextMap.StoryTimeSpawnSetting;
            }
            else
            {
                Debug.LogWarning("CurrentNextMap was null in encounter spawning.", this);
            }
            GameDifficulty savedDifficulty = SavedDifficulty;
            if (savedDifficulty == GameDifficulty.StoryTime && settings.AppearsInBaseDifficulty(difficulty))
            {
                return true;
            }
            switch (savedDifficulty)
            {
                case GameDifficulty.Normal:
                case GameDifficulty.Hard:
                case GameDifficulty.PathOfTheDamned:
                    return true;
                case GameDifficulty.Easy:
                    if (settings.Easy)
                    {
                        return true;
                    }
                    break;
            }
            if (savedDifficulty == GameDifficulty.Normal && settings.Normal)
            {
                return true;
            }
            if (savedDifficulty == GameDifficulty.Hard && settings.Hard)
            {
                return true;
            }
            return false;
        }
    }
}