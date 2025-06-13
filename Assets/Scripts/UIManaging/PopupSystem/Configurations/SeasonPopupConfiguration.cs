using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class SeasonPopupConfiguration : PopupConfiguration
    {
        public Action ExploreSeason { get; set; }
        public Action OnExploreSeasonIgnored { get; set; }

        public SeasonPopupConfiguration() : base(PopupType.SeasonPopup, null)
        {
        }
    }
}
