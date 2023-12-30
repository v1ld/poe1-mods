using Patchwork.Attributes;
using System;
using System.IO;
using UnityEngine;

namespace SaveManager
{
    // Sort saves by time instead of Autosaves first, then Quicksave, then your saves
    [ModifiesType]
    public class v1ld_UISaveLoadPlaythroughGroup : UISaveLoadPlaythroughGroup
    {
        [ModifiesMember("CompareSaveFiles")]
        new public int CompareSaveFiles(UISaveLoadSave save1, UISaveLoadSave save2)
        {
            if (save1.IsNew)
                return -1;
            if (save2.IsNew)
                return 1;
            return -DateTime.Compare(save1.MetaData.RealTimestamp, save2.MetaData.RealTimestamp);
        }
    }

    [ModifiesType]
    public class v1ld_GameState : GameState
    {
        [NewMember]
        private int m_QuicksaveCycleNumber;

        // The current quicksave number is tracked in the savefile
        [Persistent]
        [NewMember]
        public int QuicksaveCycleNumber
        {
            [NewMember]
            get
            {
                return m_QuicksaveCycleNumber;
            }
            [NewMember]
            set
            {
                m_QuicksaveCycleNumber = value;
            }
        }

        [NewMember]
        public static int MaxQuicksaves
        {
            [NewMember]
            get { return 10;}
            [NewMember]
            private set { }
        }
    }

    [ModifiesType]
    public class v1ld_SaveGameInfo : SaveGameInfo
    {
        [ModifiesMember("IsQuickSave")]
        new public bool IsQuickSave()
        {
            return Path.GetFileNameWithoutExtension(FileName).Contains(" quicksave");
        }

        [ModifiesMember("GetQuicksaveFileName")]
        new public static string GetQuicksaveFileName()
        {
            // Always return the last quicksave file - preserves the illusion of there being just one quicksave
            return GetSpecialSaveFileName("quicksave_" + ((v1ld_GameState)GameState.Instance).QuicksaveCycleNumber);
        }
    }

    [ModifiesType]
    public class v1ld_InGameHUD : InGameHUD
    {
        [ModifiesMember("Update")]
        new private void Update()
        {
            m_HighlightPulseTween.duration = HighlightTweenDuration;
            switch (m_CursorMode)
            {
                case ExclusiveCursorMode.Inspect:
                    GameCursor.UiCursor = GameCursor.CursorType.Examine;
                    break;
                case ExclusiveCursorMode.Helwax:
                    GameCursor.UiCursor = GameCursor.CursorType.DuplicateItem;
                    break;
            }
            bool highlightActive = HighlightActive;
            if (UIWindowManager.KeyInputAvailable)
            {
                m_HighlightHeld = GameInput.GetControl(MappedControl.HIGHLIGHT_HOLD);
                if (GameInput.GetControlUp(MappedControl.HIGHLIGHT_TOGGLE))
                {
                    m_HighlightToggled = !m_HighlightToggled;
                }
            }
            else
            {
                m_HighlightHeld = false;
            }
            if (!highlightActive && HighlightActive && OnHighlightBegin != null)
            {
                OnHighlightBegin();
            }
            else if (highlightActive && !HighlightActive && OnHighlightEnd != null)
            {
                OnHighlightEnd();
            }
            if (QuicksaveAllowed && GameInput.GetControlUp(MappedControl.QUICKSAVE))
            {
                if (GameState.CannotSaveBecauseInCombat || PartyMemberAI.IsPartyMemberUnconscious())
                {
                    UISystemMessager.Instance.PostMessage(GUIUtils.GetText(713), Color.red);
                }
                else if (GameState.Mode.TrialOfIron)
                {
                    UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1454), Color.red);
                }
                else
                {
                    // v1ld: Increment Quicksave number here as it's the only place where it's saved.
                    // Would have been nice to do it outside such a big function, but there's no cleaner spot.
                    ((v1ld_GameState)GameState.Instance).QuicksaveCycleNumber += 1;
                    ((v1ld_GameState)GameState.Instance).QuicksaveCycleNumber %= v1ld_GameState.MaxQuicksaves;

                    GameResources.SaveGame(SaveGameInfo.GetQuicksaveFileName());
                    UISystemMessager.Instance.PostMessage(GUIUtils.GetText(601), Color.green);
                    Console.AddMessage(GUIUtils.GetTextWithLinks(601), Color.green);
                }
            }
            if (GameInput.GetControlDown(MappedControl.TOGGLE_HUD, handle: true))
            {
                if (HudUserMode == 0)
                {
                    ShowHUD = false;
                }
                HudUserMode++;
                if (HudUserMode > 2)
                {
                    HudUserMode = 0;
                }
                if (HudUserMode == 0)
                {
                    ShowHUD = true;
                }
                if ((bool)GameCursor.Instance)
                {
                    GameCursor.Instance.SetShowCursor(this, HudUserMode != 2);
                }
                if (this.OnHudVisibilityChanged != null)
                {
                    this.OnHudVisibilityChanged(ShowHUD);
                }
            }
            if (GameState.Instance.CheatsEnabled && GameInput.GetKeyDown(MapKey, setHandled: true))
            {
                MapTag = "all";
                UIWorldMapManager.Instance.Toggle();
            }
        }
    }
}