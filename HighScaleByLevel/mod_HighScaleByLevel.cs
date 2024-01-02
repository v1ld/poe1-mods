using Patchwork.Attributes;
using UnityEngine;

namespace HighScaleByLevel
{
    [ModifiesType]
    class V1ldCharacterStats : CharacterStats
    {
        [NewMember]
        public int V1ldScaleLevelStart
        {
            get { return 1; }
        }

        [NewMember]
        public int V1ldScaleLevelEnd
        {
            get { return 12; }
        }

        [NewMember]
        public int V1ldScaleAccDefMin
        {
            get { return 5; }
        }

        [NewMember]
        public int V1ldScaleAccDefMax
        {
            get { return 15; }
        }

        [NewMember]
        private int V1ldScalingFactorByLevel()
        {
            int scaling;
            if (Level <= V1ldScaleLevelStart)
            {
                scaling = V1ldScaleAccDefMin;
            }
            else if (Level >= V1ldScaleLevelEnd)
            {
                scaling = V1ldScaleAccDefMax;
            }
            else
            {
                scaling = V1ldScaleAccDefMin + (V1ldScaleAccDefMax - V1ldScaleAccDefMin) * (Level - V1ldScaleLevelStart) / (V1ldScaleLevelEnd - V1ldScaleLevelStart);
            }

            return scaling;
        }

        // Can't override DifficultyStatBonus and DifficultyHealthStaminaMult as they're essentially const properties.
        // So wrap the methods where they're used instead.

        [NewMember]
        [DuplicatesBody("CalculateAccuracy")]
        public int CalculateAccuracy_Original(AttackBase attack, GameObject enemy)
        {
            return 0;
        }

        [ModifiesMember("CalculateAccuracy")]
        public int CalculateAccuracy_Wrapper(AttackBase attack, GameObject enemy)
        {
            int accuracy = CalculateAccuracy_Original(attack, enemy);

            if ((bool)GameState.Instance && GameState.Instance.Difficulty == GameDifficulty.Hard && !IsPartyMember)
            {
                accuracy += V1ldScalingFactorByLevel();
            }
            return accuracy;
        }

        [NewMember]
        [DuplicatesBody("CalculateDefense")]
        public int CalculateDefense_Original(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, bool allowRedirect)
        {
            return 0;
        }

        [ModifiesMember("CalculateDefense")]
        public int CalculateDefense_Wrapper(DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, bool allowRedirect)
        {
            int defense = CalculateDefense_Original(defenseType, attack, enemy, isSecondary, allowRedirect);

            if ((bool)GameState.Instance && GameState.Instance.Difficulty == GameDifficulty.Hard && !IsPartyMember)
            {
                defense += V1ldScalingFactorByLevel();
            }
            return defense;
        }
    }
}