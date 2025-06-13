using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class UserSoundAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.UserSound;
        
        public UserSoundAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var userSound = ev.GetUserSound();
            if (userSound == null) return Empty;
            
            var userSounds = AssetManager.GetAllLoadedAssets(DbModelType.UserSound);
            return userSounds.Where(x => x.Id == userSound.Id).ToArray();
        }
    }
}