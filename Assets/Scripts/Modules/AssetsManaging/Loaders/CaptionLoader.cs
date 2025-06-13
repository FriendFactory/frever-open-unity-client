using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Level.Full;
using JetBrains.Annotations;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.Caption;
using Zenject;

namespace Modules.AssetsManaging.Loaders
{
    [UsedImplicitly]
    internal sealed class CaptionLoader : AssetLoader<CaptionFullInfo, CaptionLoadArgs>
    {
        private readonly CaptionViewFactory _captionFactory;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CaptionLoader(IBridge bridge, CaptionViewFactory captionFactory) : base(bridge)
        {
            _captionFactory = captionFactory;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task LoadAsset(CaptionLoadArgs args)
        {
            InitAsset();
            OnCompleted(GetSuccessResult());
            return Task.CompletedTask;
        }

        protected override void InitAsset()
        {
            var captionView = _captionFactory.Create();
            var asset = new CaptionAsset();
            asset.Init(Model, captionView);
            Asset = asset;
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        [UsedImplicitly]
        public class Factory : PlaceholderFactory<CaptionLoader>
        {
        }
        
        [UsedImplicitly]
        public class CaptionViewFactory : PlaceholderFactory<CaptionView>
        {
        }
    }
}