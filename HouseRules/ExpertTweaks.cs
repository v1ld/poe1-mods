using Patchwork.Attributes;

namespace V1ldHouseRules
{
    [ModifiesType]
    class V1ld_GameOption_HouseRules : GameOption
    {
        [ModifiesMember("IsBoolOptionExpert")]
        new public static bool IsBoolOptionExpert(BoolOption c)
        {
            return c == BoolOption.DEATH_IS_NOT_PERMANENT || /* c == BoolOption.AOE_HIGHLIGHTING || */ c == BoolOption.DISPLAY_UNQUALIFIED_INTERACTIONS || c == BoolOption.DISPLAY_INTERACTION_QUALIFIER || c == BoolOption.DISPLAY_PERSONALITY_REPUTATION_INDICATORS || c == BoolOption.DISPLAY_RELATIVE_DEFENSES || c == BoolOption.DISPLAY_QUEST_OBJECTIVE_TITLES || c == BoolOption.DONT_RESTRICT_STASH || c == BoolOption.COMBAT_TOOLTIPS_UI || c == BoolOption.SHOW_TUTORIALS || c == BoolOption.AUTO_LEVEL_COMPANIONS || c == BoolOption.DISABLE_INJURIES;
        }
    }
}