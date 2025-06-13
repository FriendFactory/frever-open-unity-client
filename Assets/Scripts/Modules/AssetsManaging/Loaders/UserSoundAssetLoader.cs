using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class UserSoundAssetLoader : BaseSongAssetLoader<UserSoundFullInfo, UserSoundLoadArgs>
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public UserSoundAssetLoader(IBridge bridge, AudioSourceManager audioSourceManager) : base(bridge, audioSourceManager)
        {
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void InitAsset()
        {
            var view = new UserSoundAsset();
            InitSong(view, Model);
        }
    }
}
