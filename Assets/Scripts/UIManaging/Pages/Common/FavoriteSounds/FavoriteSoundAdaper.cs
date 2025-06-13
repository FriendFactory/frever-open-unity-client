using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Results;
using Bridge.Services._7Digital.Models.TrackModels;
using UnityEngine;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public sealed class FavoriteSoundAdapter: IDisposable
    {
        public FavouriteMusicInfo FavoriteSound { get; }
        public IPlayableMusic TargetSound { get; private set; }
        
        private readonly IBridge _bridge;
        
        private CancellationTokenSource _cancellationTokenSource;
        
        public FavoriteSoundAdapter(IBridge bridge, FavouriteMusicInfo favoriteSound)
        {
            _bridge = bridge;
            FavoriteSound = favoriteSound ?? throw new ArgumentNullException();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        public async Task GetTargetSoundAsync()
        {
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            
            TargetSound = await GetSoundAssetAsync(FavoriteSound, _cancellationTokenSource.Token);
        }
        
        private async Task<IPlayableMusic> GetSoundAssetAsync(FavouriteMusicInfo favoriteSound, CancellationToken token)
        {
            switch (favoriteSound.Type)
            {
                case SoundType.Song:
                    return await GetSoundAsync<SongInfo>(favoriteSound, token, _bridge.GetSongAsync);
                case SoundType.UserSound:
                    return await GetSoundAsync<UserSoundFullInfo>(favoriteSound, token, _bridge.GetUserSoundAsync);
                case SoundType.ExternalSong:
                    return await GetSoundAsync<ExternalTrackInfo>(favoriteSound, token, _bridge.GetTrackDetails);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private async Task<TModel> GetSoundAsync<TModel>(FavouriteMusicInfo favoriteSound, CancellationToken token,
            Func<long, CancellationToken, Task<Result<TModel>>> getSoundFunc) where TModel : IPlayableMusic
        {
            var id = favoriteSound.Id;
            var soundResult = await getSoundFunc(id, token);
            if (soundResult.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get sound of type {favoriteSound.Type} with id: {id} # {soundResult.ErrorMessage}");
                return default;
            }

            return soundResult.Model;
        }

        private void Cancel()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }
    }
}