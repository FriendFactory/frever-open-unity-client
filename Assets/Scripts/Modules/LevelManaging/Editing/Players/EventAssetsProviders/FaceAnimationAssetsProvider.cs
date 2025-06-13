using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class FaceAnimationAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.FaceAnimation;
        
        public FaceAnimationAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var faceAnimIds = ev.GetFaceAnimations().Select(x=>x.Id);
            var loadedFaceAnimations = AssetManager.GetAllLoadedAssets(DbModelType.FaceAnimation);
            return loadedFaceAnimations.Where(x => faceAnimIds.Contains(x.Id)).ToArray();
        }
    }
}