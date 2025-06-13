using System;
using Bridge.Models.ClientServer.Assets;
using Modules.LevelManaging.Assets;


namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class AudioUnloader : BaseAudioUnloader<SongInfo>
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            var song = asset as SongAsset;
            UnloadSong(song, onSuccess);
        }
    }
}