using System.Threading;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Common.Abstract;
using Extensions;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public class SoundThumbnail: BaseContextPanel<IPlayableMusic>
    {
        [SerializeField] private RawImage _thumbnailTarget;
        
        [Inject] private MusicDownloadHelper _musicDownloadHelper;
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;

        private CancellationTokenSource _cancellationTokenSource;
        private Texture2D _thumbnail;

        protected override void OnInitialized()
        {
            HideThumbnail();
            DownloadThumbnail();
        }

        protected override void BeforeCleanUp()
        {
            Cancel();

            if (_thumbnail)
            {
                Destroy(_thumbnail);
                _thumbnail = null;
            }
        }

        private void DownloadThumbnail()
        {
            Cancel();
            
            _cancellationTokenSource = new CancellationTokenSource();

            switch (ContextData)
            {
                case SongInfo song: 
                    _musicDownloadHelper.DownloadThumbnail(song, Resolution._128x128, OnThumbnailLoaded, OnThumbnailLoadingFailed, _cancellationTokenSource.Token);
                    break;
                case UserSoundFullInfo userSound:
                    _characterThumbnailsDownloader.GetCharacterThumbnailByUserGroupId(userSound.Owner.Id, Resolution._128x128, OnThumbnailLoaded, OnThumbnailLoadingFailed, _cancellationTokenSource.Token);
                    break;
                case ExternalTrackInfo externalSong:
                    _musicDownloadHelper.DownloadThumbnail(externalSong, OnThumbnailLoaded, OnThumbnailLoadingFailed, _cancellationTokenSource.Token);
                    break;
                case FavouriteMusicInfo favoriteSound:
                        _musicDownloadHelper.DownloadFavoriteSoundThumbnail(favoriteSound, OnThumbnailLoaded, OnThumbnailLoadingFailed, _cancellationTokenSource.Token);
                    break;
            }
        }

        private void OnThumbnailLoaded(Texture2D thumbnail)
        {
            _thumbnail = thumbnail;
            SetTexture(thumbnail);
        }
        
        private void OnThumbnailLoadingFailed(string error)
        {
            HideThumbnail();
        }
        
        private void SetTexture(Texture texture)
        {
            _thumbnailTarget.SetActive(true);
            _thumbnailTarget.color = Color.white;
            _thumbnailTarget.texture = texture;
        }

        private void HideThumbnail()
        {
            _thumbnailTarget.color = Color.white.SetAlpha(0f);
        }

        private void Cancel()
        {
            if (_cancellationTokenSource == null) return;
            
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
        }
    }
}