using Patchwork.Attributes;
using System;
using System.IO;
using UnityEngine;

// IE Mod's autosave only even n minutes feature
namespace V1ldHouseRules
{
    [ModifiesType]
    public class V1ld_GameState : GameState
    {
        [NewMember]
        public static DateTime LastAutoSaveTime;

        [NewMember]
        public static void AutosaveIfAllowed()
        {
            var now = DateTime.Now;
            var minutes = 10f;
            if ((minutes >= 0) && ((now - LastAutoSaveTime).TotalMinutes >= minutes))
            {
                LastAutoSaveTime = now;
                Autosave();
            }
        }

        [ModifiesMember("ChangeLevel")]
        new public static void ChangeLevel(MapData map)
        {
            // v1ld: from IEMod
            AutosaveIfAllowed();

            try
            {
                PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
                foreach (PartyMemberAI partyMemberAI in partyMembers)
                {
                    if (!(partyMemberAI == null))
                    {
                        if (partyMemberAI.StateManager != null)
                        {
                            partyMemberAI.StateManager.CurrentState?.StopMover();
                            partyMemberAI.StateManager.AbortStateStack();
                        }
                        Stealth component = partyMemberAI.GetComponent<Stealth>();
                        if ((bool)component)
                        {
                            component.ClearAllSuspicion();
                        }
                    }
                }
                StartPoint.s_ChosenStartPoint = null;
                BeginLevelUnload(map.SceneName);
                ConditionalToggleManager.Instance.ResetBetweenSceneLoads();
                PersistenceManager.SaveGame();
                FogOfWar.Save();
                string levelFilePath = PersistenceManager.GetLevelFilePath(map.SceneName);
                IsRestoredLevel = File.Exists(levelFilePath);
                if ((bool)GameUtilities.Instance)
                {
                    bool loadFromSaveFile = false;
                    GameUtilities.Instance.StartCoroutine(GameResources.LoadScene(map.SceneName, loadFromSaveFile));
                }
                Instance.CurrentNextMap = map;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        [ModifiesMember("FinalizeLevelLoad")]
        new public void FinalizeLevelLoad()
        {
            try
            {
                if (CurrentMap != null && !CurrentMap.HasBeenVisited && (bool)BonusXpManager.Instance && CurrentMap.GivesExplorationXp)
                {
                    CurrentMap.HasBeenVisited = true;
                    int num = 0;
                    if (BonusXpManager.Instance != null)
                    {
                        num = BonusXpManager.Instance.MapExplorationXp;
                    }
                    Console.AddMessage("[" + NGUITools.EncodeColor(Color.yellow) + "]" + Console.Format(GUIUtils.GetTextWithLinks(1633), CurrentMap.DisplayName, num * PartyHelper.NumPartyMembers));
                    PartyHelper.AssignXPToParty(num, printMessage: false);
                }
                if (GameState.OnLevelLoaded != null)
                {
                    GameState.OnLevelLoaded(Application.loadedLevelName, EventArgs.Empty);
                }
                if (NewGame)
                {
                    if (Difficulty == GameDifficulty.Easy)
                    {
                        Option.AutoPause.SetSlowEvent(AutoPauseOptions.PauseEvent.CombatStart, isActive: true);
                    }
                    HasNotifiedPX2Installation = true;
                }
                ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnLevelLoaded);
                IsLoading = false;
                /* v1ld: Autosave happens before level load now
                if (s_playerCharacter != null && !LoadedGame && !NewGame && NumSceneLoads > 0)
                {
                    if ((bool)FogOfWar.Instance)
                    {
                        FogOfWar.Instance.WaitForFogUpdate();
                    }
                    Autosave();
                }
                */
                NewGame = false;
                if (CurrentMap != null && CouldAccessStashOnLastMap != CurrentMap.GetCanAccessStash() && !Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH))
                {
                    if (CurrentMap.GetCanAccessStash())
                    {
                        UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1565), Color.white);
                    }
                    else
                    {
                        UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1566), Color.white);
                    }
                }
                if (NumSceneLoads == 0 && AnalyticsManager.Instance != null && (bool)s_playerCharacter)
                {
                    Guid sessionID = s_playerCharacter.GetComponent<Player>().SessionID;
                    AnalyticsManager.Instance.GameSessionBegin(sessionID, NewGame, LoadedGame);
                }
                NumSceneLoads++;
                FatigueCamera.CreateCamera();
                GammaCamera.CreateCamera();
                WinCursor.Clip(state: true);
                if (CurrentMap != null)
                {
                    TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.ENTERED_MAP);
                    trigger.Map = CurrentMap.SceneName;
                    TutorialManager.STriggerTutorialsOfType(trigger);
                }
                if (CurrentMap != null && CurrentMap.IsValidOnMap("px1"))
                {
                    HasEnteredPX1 = true;
                    if (GameGlobalVariables.HasStartedPX2())
                    {
                        HasEnteredPX2 = true;
                    }
                }
                if (CurrentMap != null)
                {
                    AnalyticsManager.Instance.MapEnter(Guid.Empty, CurrentMap.ToString());
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ReturnToMainMenuFromError();
            }
            if (!RetroactiveSpellMasteryChecked)
            {
                for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
                {
                    if (!(PartyMemberAI.PartyMembers[i] == null))
                    {
                        CharacterStats component = PartyMemberAI.PartyMembers[i].GetComponent<CharacterStats>();
                        if ((bool)component && component.MaxMasteredAbilitiesAllowed() > component.GetNumMasteredAbilities())
                        {
                            UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(2252), GUIUtils.GetText(2303));
                            break;
                        }
                    }
                }
                RetroactiveSpellMasteryChecked = true;
            }
            if (GameUtilities.HasPX2() && LoadedGame)
            {
                if (GameGlobalVariables.HasFinishedPX1())
                {
                    QuestManager.Instance.StartPX2Umbrella();
                }
                else if (!HasNotifiedPX2Installation)
                {
                    UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, string.Empty, GUIUtils.GetText(2438));
                    HasNotifiedPX2Installation = true;
                }
            }
        }
    }
}
