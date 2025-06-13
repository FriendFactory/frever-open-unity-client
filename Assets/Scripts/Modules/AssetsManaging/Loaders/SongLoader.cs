using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class SongLoader : BaseSongAssetLoader<SongInfo, SongLoadArgs>
    {
        public override bool CacheFile => true;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SongLoader(IBridge bridge, AudioSourceManager audioSourceManager) : base(bridge, audioSourceManager)
        {
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void InitAsset()
        {
            var asset = new SongAsset();
            InitSong(asset, Model);
        }
    }
}
