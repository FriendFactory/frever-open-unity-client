using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal sealed class UserSoundsList : BaseEnhancedScrollerView<UserSoundItem, PlayableUserSoundModel>
    {
        public void ReloadData()
        {
            _scroller.ReloadData();
        }
    }
}