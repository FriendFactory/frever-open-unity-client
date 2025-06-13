using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Extensions;
using UIManaging.Pages.Common.FavoriteSounds;

namespace UIManaging.Pages.Common.VideoDetails
{
    internal sealed class UsedVideoSoundsLoader: IDisposable
    {
        private readonly IBridge _bridge;
        private readonly Video _video;

        private CancellationTokenSource _cancellationTokenSource;

        public UsedVideoSoundsLoader(IBridge bridge, Video video)
        {
            _bridge = bridge;
            _video = video;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        public async Task<List<UsedSoundItemModel>> GetVideoSoundsAsync()
        {
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            
            var songIds = _video.Songs?.Where(song => !song.IsExternal).Select(songInfo => songInfo.Id).ToArray() ?? Array.Empty<long>();
            var userSoundIds = _video.UserSounds?.Select(userSoundInfo => userSoundInfo.Id).ToArray() ?? Array.Empty<long>();
            var externalSongIds = _video.Songs?.Where(songInfo => songInfo.IsExternal).Select(song => song.Id).ToArray() ?? Array.Empty<long>();
            
            var soundsResult = await _bridge.GetSounds(songIds, userSoundIds, externalSongIds, _cancellationTokenSource.Token);
            if (soundsResult.IsError)
            {
                return null;
            }

            if (soundsResult.IsRequestCanceled) return new List<UsedSoundItemModel>();

            var sounds = new List<UsedSoundItemModel>();

            if (soundsResult.Model.Songs?.Length > 0)
            {
                var usedSongs = soundsResult.Model.Songs.Select(song => new UsedSoundItemModel(song, song.IsFavorite, song.UsageCount));
                sounds.AddRange(usedSongs);
            }

            if (soundsResult.Model.UserSounds?.Length > 0)
            {
                var usedUserSounds = soundsResult.Model.UserSounds.Select(userSound => new UsedSoundItemModel(userSound, userSound.IsFavorite, userSound.UsageCount));
                sounds.AddRange(usedUserSounds);
            }

            await GetExternalSongsAsync();

            return sounds;

            async Task GetExternalSongsAsync()
            {
                var availableExternalSongIds = soundsResult.Model.ExternalSongs?.Where(song => song.IsAvailable).Select(song => song.Id).ToList();
                
                if (availableExternalSongIds.IsNullOrEmpty()) return;

                var externalSongsResult = await _bridge.GetBatchTrackDetails(availableExternalSongIds, _cancellationTokenSource.Token);
                
                if (externalSongsResult.IsError)
                {
                    return;
                }
                
                if (externalSongsResult.IsRequestCanceled) return;

                var models = soundsResult.Model?.ExternalSongs?.Zip(externalSongsResult.Models, (shortInfo, info) => new UsedSoundItemModel(info, shortInfo.IsFavorite, shortInfo.UsageCount)).ToList();
                
                if (models.IsNullOrEmpty()) return;

                sounds.AddRange(models);
            }
        }

        private void Cancel()
        {
            if (_cancellationTokenSource == null) return;
            
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
        }
    }
}