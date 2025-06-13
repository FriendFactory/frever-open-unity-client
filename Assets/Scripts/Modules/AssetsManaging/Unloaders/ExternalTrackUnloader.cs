using System;
using Bridge.Services._7Digital.Models.TrackModels;
using Modules.LevelManaging.Assets;
using Modules.MusicCacheManaging;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class ExternalTrackUnloader :  BaseAudioUnloader<ExternalTrackInfo>
    {
        private readonly ILicensedMusicProvider _licensedMusicProvider;

        public ExternalTrackUnloader(ILicensedMusicProvider licensedMusicProvider)
        {
            _licensedMusicProvider = licensedMusicProvider;
        }

        public override void Unload(IAsset asset, Action onSuccess = null)
        {
            if (ShouldUnloadFile(asset.Id))
            {
                var song = asset as ExternalTrackAsset;
                UnloadSong(song, onSuccess);
            }
            else
            {
                onSuccess?.Invoke();
            }
        }

        private bool ShouldUnloadFile(long id)
        {
            return !_licensedMusicProvider.IsKeptByCache(id);
        }
    }
}