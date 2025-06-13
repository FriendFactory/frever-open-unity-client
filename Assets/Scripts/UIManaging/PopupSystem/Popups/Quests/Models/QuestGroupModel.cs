using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Onboarding;
using Bridge.Models.ClientServer.UserActivity;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;

namespace UIManaging.PopupSystem.Popups.Quests.Models
{
    public class QuestGroupModel: IQuestGroupModel
    {
        public IReadOnlyList<IQuestModel> QuestModels { get; }

        public QuestGroupModel(OnboardingQuestGroup questGroup, IDictionary<string, bool> isActive, IDictionary<long, OnboardingQuestProgress> progress, Action<string> goToQuestTargetAction)
        {
            QuestModels = questGroup.Quests
                                    .Where(quest => progress.ContainsKey(quest.Id))
                                    .Select(quest => new QuestModel(quest, isActive[quest.QuestType], progress[quest.Id], goToQuestTargetAction))
                                    .ToList();
        }
    }
}