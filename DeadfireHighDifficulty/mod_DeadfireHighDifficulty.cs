using Patchwork.Attributes;

namespace DeadfireHighDifficulty
{
    [ModifiesType]
    class V1ldCharacterStats : CharacterStats
    {
        [ModifiesMember("get_DifficultyStatBonus")]
        public float get_DifficultyStatBonus()
        {
            if (!IsPartyMember && GameState.Instance != null)
            {
                if (GameState.Instance.IsDifficultyPotd)
                {
                    return 15f;
                }
                if (GameState.Instance.Difficulty == GameDifficulty.Hard)
                {
                    return 8f;
                }
            }
            return 0f;
        }

        [ModifiesMember("get_DifficultyHealthStaminaMult")]
        public float get_DifficultyHealthStaminaMult()
        {
            if (!IsPartyMember && !HasFactionSwapEffect() && GameState.Instance != null)
            {
                if (GameState.Instance.IsDifficultyPotd)
                {
                    return 1.25f;
                }
                if (GameState.Instance.Difficulty == GameDifficulty.Hard)
                {
                    return 1.125f;
                }
                if (GameState.Instance.IsDifficultyStoryTime)
                {
                    return 0.5f;
                }
                return 1f;
            }
            return 1f;
        }
    }
}