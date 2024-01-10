using Patchwork.Attributes;
using System.Collections.Generic;

namespace V1ldJournalHideCompletedQuests
{
    [ModifiesType]
    class V1ldGameStateJHCQ : GameState
    {
        [NewMember]
        public static bool s_v1ldJournalShowCompleted;
    }

    [ModifiesType("CommandLine")]
    public static class V1ld_CommandLineJHCQ
    {
        [NewMember]
        public static void JournalShowCompleted()
        {
            V1ldGameStateJHCQ.s_v1ldJournalShowCompleted = !V1ldGameStateJHCQ.s_v1ldJournalShowCompleted;
            Console.AddMessage($"Journal completed quests: {(V1ldGameStateJHCQ.s_v1ldJournalShowCompleted ? "shown" : "hidden")}");
        }
    }

    [ModifiesType]
    class V1ld_JournalTreeListQuests : JournalTreeListQuests
    {
        public V1ld_JournalTreeListQuests(QuestType questType) : base(questType)
        {
        }

        [ModifiesMember("LoadTreeListChildren")]
        new public void LoadTreeListChildren(UITreeListItem intoItem)
        {
            QuestManager questManager = QuestManager.Instance;
            List<Quest> incompleteQuests = questManager.GetIncompleteQuests(m_QuestType);
            incompleteQuests.Sort((Quest x, Quest y) => questManager.GetQuestStartTime(y).CompareTo(questManager.GetQuestStartTime(x)));
            List<Quest> completeQuests = questManager.GetCompleteQuests(m_QuestType);
            completeQuests.Sort(delegate (Quest x, Quest y)
            {
                EternityDateTime questStartTime = questManager.GetQuestStartTime(x);
                EternityDateTime questStartTime2 = questManager.GetQuestStartTime(y);
                return questStartTime2.CompareTo(questStartTime);
            });
            if (incompleteQuests.Count > 0 || completeQuests.Count > 0)
            {
                foreach (Quest item in incompleteQuests)
                {
                    intoItem.AddChild(item);
                }
                if (V1ldGameStateJHCQ.s_v1ldJournalShowCompleted)
                {
                    foreach (Quest item2 in completeQuests)
                    {
                        UITreeListItem uITreeListItem = intoItem.AddChild(item2);
                        uITreeListItem.SetVisualDisabled(state: true);
                    }
                }
                return;
            }
            intoItem.AddChild(GetNoChildrenString());
        }
    }
}