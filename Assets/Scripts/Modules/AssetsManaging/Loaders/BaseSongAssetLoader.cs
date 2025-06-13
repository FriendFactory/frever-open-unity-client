using Bridge.Models.Common;
using Bridge;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal abstract class BaseSongAssetLoader<TEntity, TArgs> : FileAssetLoader<TEntity, TArgs> 
        where TEntity: IFilesAttachedEntity, IPlayableMusic 
        where TArgs: LoadAssetArgs<TEntity>
    {
        private readonly AudioSourceManager _audioSourceManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BaseSongAssetLoader(IBridge bridge, AudioSourceManager audioSourceManager) : base(bridge)
        {
            _audioSourceManager = audioSourceManager;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void InitSong(BaseMusicAsset<TEntity> view, TEntity songData)
        {
            var audioClip = Target as AudioClip;
            view.Init(songData, _audioSourceManager.SongAudioSource, audioClip);
            Asset = view;
        }
    }
}