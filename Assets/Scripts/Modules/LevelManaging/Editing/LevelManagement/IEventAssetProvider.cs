using Bridge.Models.Common;
using Extensions;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface IEventAssetProvider
    {
        bool IsLoadingAssets();
        bool IsLoadingAssetsOfType(DbModelType assetType);
        bool IsAssetLoaded(IEntity entity);

        
        ISetLocationAsset GetTargetEventSetLocationAsset(); 
        ISetLocationAsset GetCurrentActiveSetLocationAsset();
        ICameraAnimationAsset GetCurrentCameraAnimationAsset();
        ICharacterAsset[] GetCurrentCharactersAssets();
        IVfxAsset GetCurrentVfxAsset();
        IAudioAsset GetCurrentAudioAsset();
        ICaptionAsset[] GetCurrentCaptionAssets();
    }
}