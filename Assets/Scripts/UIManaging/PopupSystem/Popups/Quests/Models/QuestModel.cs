using System;
using Bridge.Models.ClientServer.Onboarding;
using Bridge.Models.ClientServer.UserActivity;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;

namespace UIManaging.PopupSystem.Popups.Quests.Models
{
    public class QuestModel: IQuestModel
    {
        public string QuestType { get; }
        public bool IsActive { get; }
        public string Title { get; }
        public string Description { get; }
        public int CurrentParamValue { get; }
        public int MaxParamValue { get; }
        public Action GoToQuestTargetAction { get; }

        public QuestModel(OnboardingQuest quest, bool isActive, OnboardingQuestProgress progress, Action<string> goToQuestTargetAction)
        {
            QuestType = quest.QuestType;
            IsActive = isActive;
            Title = quest.Title;
            Description = quest.Description;
            CurrentParamValue = progress.CurrentProgress;
            MaxParamValue = progress.ToComplete;
            GoToQuestTargetAction = () => goToQuestTargetAction?.Invoke(QuestType);
        }
    }
}