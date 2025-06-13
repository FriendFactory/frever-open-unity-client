using System.Linq;
using Bridge.Models.Common;
using Extensions;
using Modules.LevelManaging.Assets;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    internal sealed partial class LevelManager
    {
        private readonly EventAssetsProvider _eventAssetProvider; 
        
        public bool IsLoadingAssets()
        {
            return _assetManager.IsLoadingAssets();
        }

        public bool IsLoadingAssetsOfType(DbModelType assetType)
        {
            return _assetManager.IsLoadingAssetsOfType(assetType);
        }

        public bool IsAssetLoaded(IEntity entity)
        {
            return _assetManager.IsAssetLoaded(entity);
        }

        public ISetLocationAsset GetTargetEventSetLocationAsset()
        {
            var targetId = TargetEvent.GetSetLocation().Id;
            return _assetManager.GetAllLoadedAssets<ISetLocationAsset>().FirstOrDefault(asset => asset.Id == targetId);
        }
        
        public ISetLocationAsset GetCurrentActiveSetLocationAsset()
        {
            return _assetManager.GetActiveAssets<ISetLocationAsset>().FirstOrDefault();
        }

        public ICameraAnimationAsset GetCurrentCameraAnimationAsset()
        {
            var animId = TargetEvent.GetCameraAnimationId();
            return !animId.HasValue 
                ? null : 
                _assetManager.GetAllLoadedAssets<ICameraAnimationAsset>().FirstOrDefault(x=>x.Id == animId.Value);
        }

        public ICharacterAsset[] GetCurrentCharactersAssets()
        {
            return _eventAssetProvider.GetLoadedAssets(TargetEvent, DbModelType.Character).Cast<ICharacterAsset>().ToArray();
        }

        public IVfxAsset GetCurrentVfxAsset()
        {
            return _assetManager.GetActiveAssets<IVfxAsset>().FirstOrDefault();
        }

        public IAudioAsset GetCurrentAudioAsset()
        {
            return _assetManager.GetActiveAssets<IAudioAsset>().FirstOrDefault();
        }

        public ICaptionAsset[] GetCurrentCaptionAssets()
        {
            return _eventAssetProvider.GetLoadedAssets(TargetEvent, DbModelType.Caption).Cast<ICaptionAsset>().ToArray();
        }
    }
}