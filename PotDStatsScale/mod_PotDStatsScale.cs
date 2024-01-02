using Patchwork.Attributes;

namespace PotDStatsScale
{
    [ModifiesType]
    public class  V1ldCharacterStats : CharacterStats
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

        [ModifiesMember("DifficultyStatBonus")]
        new public float DifficultyStatBonus
        {
            get
            {
                if ((bool)GameState.Instance && GameState.Instance.IsDifficultyPotd && !IsPartyMember)
                {
                    if (Level <= V1ldScaleLevelStart)
                    {
                        return V1ldScaleAccDefMin;
                    }
                    else if (Level >= V1ldScaleLevelEnd)
                    {
                        return V1ldScaleAccDefMax;
                    }
                    else
                    {
                        return V1ldScaleAccDefMin + (V1ldScaleAccDefMax - V1ldScaleAccDefMin) * (Level - V1ldScaleLevelStart) / (V1ldScaleLevelEnd - V1ldScaleLevelStart);
                    }
                }
                return 0f;
            }
        }

        //[ModifiesMember("DifficultyHealthStaminaMult")]
        //new public float DifficultyHealthStaminaMult
        //{
        //    get
        //    {
        //        if (!IsPartyMember && (IsPartyMember || !HasFactionSwapEffect()) && (bool)GameState.Instance)
        //        {
        //            if (GameState.Instance.IsDifficultyPotd)
        //            {
        //                return 1.25f;
        //            }
        //            if (GameState.Instance.IsDifficultyStoryTime)
        //            {
        //                return 0.5f;
        //            }
        //            return 1f;
        //        }
        //        return 1f;
        //    }
        //}
    }
}