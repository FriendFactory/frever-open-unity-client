using System.Collections.Generic;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsListModel
    {
        public List<SeasonRewardsItemModel> Items { get;}
        public int CurrentLevelIndex { get; }

        public SeasonRewardsListModel(List<SeasonRewardsItemModel> items, int currentLevelIndex)
        {
            Items = items;
            CurrentLevelIndex = currentLevelIndex;
        }
    }
}