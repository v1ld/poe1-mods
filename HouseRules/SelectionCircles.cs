using Patchwork.Attributes;
using System;
using System.Linq;
using UnityEngine;


namespace V1ldHouseRules
{
    [ModifiesType]
    public class V1ld_Faction : Faction
    {
        // Idea from IE Mod
        [ModifiesMember("ShowSelectionCircle")]
        new public bool ShowSelectionCircle(bool elevate)
        {
            if (InGameHUD.Instance == null)
            {
                return false;
            }
            bool flag = DrawSelectionCircle && InGameHUD.Instance.ShowHUD;
            bool flag2 = healthComponent == null || !healthComponent.ShowDead || GameInput.SelectDead;
            bool flag3 = !GameState.Option.GetOption(GameOption.BoolOption.HIDE_CIRCLES) || GameState.Paused;
            bool flag4 = RelationshipToPlayer == Relationship.Hostile || isPartyMember || elevate;
            // Add GameState.Paused as an override here a la IE Mod
            return flag && flag2 && isFowVisible && ((flag3 && flag4) || MousedOver || InGameHUD.Instance.HighlightActive || GameState.Paused);
        }
    }

    // Taken from IE Mod
    [ModifiesType]
    public class V1ld_InGameHUD : InGameHUD
    {
        [ModifiesMember("FetchColors")]
        new public void FetchColors()
        {
            // BG's cyan (64, 255, 255) selection circles for neutral mobs
            FriendlySelectedColorBlind.color = new Color(0.25f, 1f, 1f, 1f);

            FriendlyColor = FriendlySelected.color;
            FoeColor = FoeMaterial.color;
            FriendlyColorColorBlind = FriendlySelectedColorBlind.color;
        }
    }

    // From IE Mod, updated for latest patch code which adds the isDominated argument
    [ModifiesType]
    public class V1ld_SelectionCircle : SelectionCircle
    {

        [ModifiesMember("SetMaterial")]
        new public void SetMaterial(bool isFoe, bool isSelected, bool isStealthed, bool isDominated)
        {
            if (base.renderer == null)
            {
                return;
            }
            if (!isFoe && !isStealthed && !InGameHUD.Instance.UseColorBlindSettings)
            {
                var partyMemberAIComponent = m_Owner.GetComponent<PartyMemberAI>();
                var isPartyMember = partyMemberAIComponent?.enabled == true;
                if (!isPartyMember)
                {
                    // colorblind material for friendlies happens to be a nice azure. The non-selected material is an ugly navy.
                    // selected colorblind material for non-stealthed friendlies.
                    m_selectedMaterial = InGameHUD.Instance.CircleMaterials.Get(true, true, false, false, false);
                    // non-selected colorblind material for non-stealthed friendlies
                    m_Circle.sharedMaterial = InGameHUD.Instance.CircleMaterials.Get(true, true, true, false, false);
                    OnColorChanged(m_Circle.sharedMaterial.color);
                    if (this.OnSharedMaterialChanged != null)
                    {
                        this.OnSharedMaterialChanged(m_Circle.sharedMaterial);
                    }
                    return;
                }
                // if party member, just get the regular circle.
            }

            // original SetMaterial
            m_selectedMaterial = InGameHUD.Instance.CircleMaterials.Get(!isFoe, InGameHUD.Instance.UseColorBlindSettings, selected: true, isStealthed, isDominated);
            m_Circle.sharedMaterial = InGameHUD.Instance.CircleMaterials.Get(!isFoe, InGameHUD.Instance.UseColorBlindSettings, isSelected, isStealthed, isDominated);
            OnColorChanged(m_Circle.sharedMaterial.color);
            if (OnSharedMaterialChanged != null)
            {
                OnSharedMaterialChanged(m_Circle.sharedMaterial);
            }
        }

    }
}