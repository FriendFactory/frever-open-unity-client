using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using JetBrains.Annotations;
using Modules.MusicCacheManaging;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.Helpers
{
    [UsedImplicitly]
    public sealed class MusicDownloadHelper
    {
        private const string MUSIC_IS_NOT_AVAILABLE_ERROR_MESSAGE = "Unauthorized";
        private const string EXTERNAL_TRACK_IS_NOT_AVAILABLE_MESSAGE = "Sorry, this music is not available in your country";
        
        private readonly IBridge _bridge;
        private readonly ILicensedMusicProvider _licensedMusicManager;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly CharacterThumbnailsDownloader _characterThumbnailsDownloader;

        public MusicDownloadHelper(IBridge bridge, ILicensedMusicProvider licensedMusicManager, SnackBarHelper snackBarHelper, CharacterThumbnailsDownloader characterThumbnailsDownloader)
        {
            _bridge = bridge;
            _licensedMusicManager = licensedMusicManager;
            _snackBarHelper = snackBarHelper;
            _characterThumbnailsDownloader = characterThumbnailsDownloader;
        }

        public async void DownloadMusic(IFilesAttachedEntity song, Action<AudioClip> onSuccess = null, Action<string> onFail = null, CancellationToken cancellationToken = default)
        {
            var result = await _bridge.GetAssetAsync(song, cancellationToken: cancellationToken);

            if (result.IsRequestCanceled)
            {
                Debug.LogWarning($"[{GetType().Name}] Song download has canceled # id: {song.Id}");
                return;
            }


            if (result.IsSuccess)
            {
                onSuccess?.Invoke((AudioClip)result.Object);
            }
            else
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }
        
        public async void DownloadMusic(long id, Action<AudioClip> onSuccess = null, Action<string> onFail = null, CancellationToken cancellationToken = default)
        {
            var result = await _licensedMusicManager.GetExternalTrackClip(id, cancellationToken);
            if (result.RequestCancelled) return;
            
            if (result.AudioClip != null)
            {
                onSuccess?.Invoke(result.AudioClip);
            }
            else
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }

        public void KeepInCache(long externalTrackId, AudioClip audioClip)
        {
            _licensedMusicManager.KeepClipInMemoryCache(externalTrackId, audioClip);
        }

        public async void DownloadThumbnail(IFilesAttachedEntity song, Resolution resolution, Action<Texture2D> onSuccess = null, Action<string> onFail = null, CancellationToken cancellationToken = default)
        {
            var result = await _bridge.GetThumbnailAsync(song, resolution, cancellationToken:cancellationToken);

            if (result.IsRequestCanceled)
            {
                Debug.LogWarning($"[{GetType().Name}] Thumbnail loading has canceled # song id: {song.Id}");
                return;
            }

            if (result.IsSuccess)
            {
                onSuccess?.Invoke((Texture2D)result.Object);
            }
            else
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }
        
        public async void DownloadThumbnail(ExternalTrackInfo trackInfo, Action<Texture2D> onSuccess = null, Action<string> onFail = null, CancellationToken cancellationToken = default)
        {
            if (trackInfo == null) return;
            
            if (trackInfo.ThumbnailUrl == null)
            {
                var thumbnailUrl = await GetExternalTrackThumbnailUrl(trackInfo.Id, cancellationToken);
                
                if (string.IsNullOrEmpty(thumbnailUrl)) return;
                
                trackInfo.ThumbnailUrl = thumbnailUrl;
            }
            
            var result  = await _bridge.DownloadExternalTrackThumbnail(trackInfo.ThumbnailUrl, cancellationToken);

            if (result.IsRequestCanceled)
            {
                Debug.LogWarning($"[{GetType().Name}] External track thumbnail download has canceled # id: {trackInfo.Id}");
                return;
            }
            
            if (result.IsSuccess)
            {
                onSuccess?.Invoke(result.Model);
            }
            else if (result.IsError)
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }

        public void DownloadFavoriteSoundThumbnail(FavouriteMusicInfo favouriteSound, Action<Texture2D> onSuccess = null, Action<string> onFail = null, CancellationToken cancellationToken = default)
        {
            switch (favouriteSound.Type)
            {
                case SoundType.Song:
                    var soundAsset = favouriteSound.GetSoundAsset();
                    DownloadThumbnail(soundAsset, Resolution._128x128, onSuccess, onFail, cancellationToken);
                    break;
                case SoundType.ExternalSong:
                    DownloadExternalTrackThumbnailById(favouriteSound.Id);
                    break;
                case SoundType.UserSound:
                    _characterThumbnailsDownloader.GetCharacterThumbnailByUserGroupId(favouriteSound.Owner.Id, Resolution._128x128, onSuccess, onFail, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            async void DownloadExternalTrackThumbnailById(long id)
            {
                try
                {
                    var externalTrackResult = await _bridge.GetTrackDetails(id, cancellationToken);

                    if (externalTrackResult.IsError)
                    {
                        onFail?.Invoke(externalTrackResult.ErrorMessage);
                        return;
                    }
                    
                    if (externalTrackResult.IsRequestCanceled) return;

                    var externalTrack = externalTrackResult.Model;

                    DownloadThumbnail(externalTrack, onSuccess, onFail, cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        
        public Task<bool> CheckIfMusicAvailableAsync(Event @event, bool showSnackBar = true)
        {
            var externalTrackId = @event?.GetExternalTrackId();

            return CheckIfMusicAvailableAsync(externalTrackId, showSnackBar);
        }
        
        public async Task<bool> CheckIfMusicAvailableAsync(long? externalTrackId, bool showSnackBar = true)
        {
            if (!externalTrackId.HasValue) return true;
            
            var trackRequest = await _bridge.GetTrackDetails(externalTrackId.Value);

            if (showSnackBar && trackRequest.IsError && trackRequest.ErrorMessage.Equals(MUSIC_IS_NOT_AVAILABLE_ERROR_MESSAGE))
            {
                _snackBarHelper.ShowFailSnackBar(EXTERNAL_TRACK_IS_NOT_AVAILABLE_MESSAGE);
                return false;
            }
            
            return trackRequest.IsSuccess;
        }
        
        public async Task<bool> CheckIfMusicAvailableForLevelAsync(long levelId, bool showSnackBar = true)
        {
            var level = await _bridge.GetLevel(levelId);

            if (!level.IsSuccess) return false;
            if (level.Model.ExternalSongs.IsNullOrEmpty()) return true;
            
            var requests =
                level.Model.ExternalSongs.Select(track => CheckIfMusicAvailableAsync(track.ExternalTrackId, showSnackBar));

            var results = await Task.WhenAll(requests);
            return results.All(isAvailable => isAvailable == true);
        }

        private async Task<string> GetExternalTrackThumbnailUrl(long externalTrackId, CancellationToken token)
        {
            var resp = await _bridge.GetTrackDetails(externalTrackId, token);
            if (resp.IsError)
            {
                Debug.LogError(resp.ErrorMessage);
            }

            return resp.Model?.ThumbnailUrl;
        }
    }
}
