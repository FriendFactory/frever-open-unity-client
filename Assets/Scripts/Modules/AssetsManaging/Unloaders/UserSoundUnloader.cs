using System;
using Bridge.Models.ClientServer.Assets;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class UserSoundUnloader : BaseAudioUnloader<UserSoundFullInfo>
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            var song = asset as UserSoundAsset;
            UnloadSong(song, onSuccess);
        }
    }
}
