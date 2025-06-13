using Bridge.Models.Common;
using UIManaging.Pages.Common.SongOption.Uploads;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public class UserSoundItem: PlayableItemBase<PlayableUserSoundModel>
    {
        [SerializeField] private OpenUserSoundSettingsButton _openSoundSettingsButton;
        
        protected override IPlayableMusic Music => ContextData.UserSoundFullInfo;
        
        protected override void DownloadThumbnail()
        {
            _thumbnailImage.texture = ContextData?.Thumbnail;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _openSoundSettingsButton.Initialize(ContextData.UserSoundFullInfo);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _openSoundSettingsButton.CleanUp();
        }
    }
}