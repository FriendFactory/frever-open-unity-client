using System.Collections.Generic;

namespace UIManaging.PopupSystem.Popups.Quests.Interfaces
{
    public interface IQuestGroupModel
    {
        IReadOnlyList<IQuestModel> QuestModels { get; }
    }
}