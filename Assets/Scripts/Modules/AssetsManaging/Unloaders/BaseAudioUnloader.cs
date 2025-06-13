using System;
using Bridge.Models.Common;
using Modules.LevelManaging.Assets;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Unloaders
{
    internal abstract class BaseAudioUnloader<TEntity> : AssetUnloader where TEntity: IPlayableMusic
    {
        protected void UnloadSong(BaseMusicAsset<TEntity> audio, Action onSuccess)
        {
            Object.Destroy(audio.Clip);
            audio.Clip = null;
            audio.CleanUp();
            onSuccess?.Invoke();
        }
    }
}
