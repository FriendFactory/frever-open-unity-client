using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class PhotoAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.UserPhoto;
        
        public PhotoAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var photo = ev.GetPhoto();
            if (photo == null) return Empty;

            var allPhotos = AssetManager.GetAllLoadedAssets(DbModelType.UserPhoto);
            return allPhotos.Where(x => x.Id == photo.Id).ToArray();
        }
    }
}