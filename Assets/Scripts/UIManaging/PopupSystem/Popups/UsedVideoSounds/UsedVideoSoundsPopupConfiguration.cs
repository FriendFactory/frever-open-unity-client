using System.Collections.Generic;
using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups.UsedVideoSounds
{
    public sealed class UsedVideoSoundsPopupConfiguration : PopupConfiguration
    {
        public BaseEnhancedScroller<UsedSoundItemModel> SoundsListModel { get; set; }
        
        public UsedVideoSoundsPopupConfiguration(IList<UsedSoundItemModel> sounds) : base(PopupType.UsedVideoSounds, null, null)
        {
            SoundsListModel = new BaseEnhancedScroller<UsedSoundItemModel>(sounds);
        }
    }
}