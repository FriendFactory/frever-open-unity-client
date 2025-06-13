using Bridge.Models.ClientServer.Level.Full;
using JetBrains.Annotations;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging.LoadingProfiles
{
    [UsedImplicitly]
    internal sealed class CaptionLoadProfile: AssetLoadProfile<CaptionFullInfo, CaptionLoadArgs>
    {
        private readonly CaptionLoader.Factory _captionFactory;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CaptionLoadProfile(CaptionLoader.Factory captionFactory)
        {
            _captionFactory = captionFactory;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override AssetLoader<CaptionFullInfo, CaptionLoadArgs> GetAssetLoader()
        {
            return _captionFactory.Create();
        }

        public override AssetUnloader GetUnloader()
        {
            return new CaptionUnloader();
        }
    }
}