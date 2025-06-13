using Bridge.Models.ClientServer.Assets;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal sealed class UserSoundsPanel : UserSoundsPanelBase<UserSoundFullInfo, PlayableUserSoundModel,
        UserSoundsListModel, UserSoundItem, UserSoundsList> 
    {
        public void AddUserSound(UserSoundFullInfo userSound)
        {
            ContextData.Insert(0, userSound);
            
            _emptyListPanel.SetActive(false);

            if (ContextData.Models.Count == 1)
            {
                OnFirstPageLoaded();
            }
            
            _userSoundsList.ReloadData();
        }

        public void ReplaceOrAddUserSound(UserSoundFullInfo userSound)
        {
            ContextData.ReplaceOrAdd(userSound);
            
            _userSoundsList.ReloadData();
        }
    }
}