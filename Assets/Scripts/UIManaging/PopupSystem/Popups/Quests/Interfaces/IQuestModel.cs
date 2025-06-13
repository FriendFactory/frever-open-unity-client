using System;

namespace UIManaging.PopupSystem.Popups.Quests.Interfaces
{
    public interface IQuestModel
    {
        string QuestType { get; }
        bool IsActive { get; }
        string Title { get; }
        string Description { get; }
        int CurrentParamValue { get; }
        int MaxParamValue { get; }

        Action GoToQuestTargetAction { get; }
    }
}