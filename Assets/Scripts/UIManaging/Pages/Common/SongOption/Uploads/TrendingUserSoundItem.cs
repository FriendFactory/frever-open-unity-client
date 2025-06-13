using System.Threading;
using Bridge.Models.Common;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    public class TrendingUserSoundItem: PlayableItemBase<PlayableTrendingUserSoundModel>
    {
        [SerializeField] private Texture2D _defaultThumbnail;
        
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        
        protected override IPlayableMusic Music => ContextData.TrendingUserSound;
        
        protected override void DownloadThumbnail()
        {
            var groupId = ContextData.TrendingUserSound.Owner.Id;

            CancellationTokenSource = new CancellationTokenSource();
            
            DownloadThumbnailByUserGroupId(groupId, Resolution._128x128, CancellationTokenSource.Token);
        }
        
        private void DownloadThumbnailByUserGroupId(long userGroupId, Resolution resolution, CancellationToken token)
        {
            _characterThumbnailsDownloader.GetCharacterThumbnailByUserGroupId(userGroupId, resolution, OnThumbnailLoaded, OnThumbnailLoadingFailed, token);
        }

        protected override void OnThumbnailLoadingFailed(string error)
        {
            base.OnThumbnailLoadingFailed(error);

            _thumbnailImage.texture = _defaultThumbnail;
        }
    }
}