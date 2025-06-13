using System;
using System.Collections.Generic;
using Bridge.Models.Common;
using Extensions;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.AssetsManaging
{
    public interface IAssetManager: IAssetTypesProvider
    {
        event Action<DbModelType, long> StartUpdatingAsset;
        event Action<DbModelType, long> StopUpdatingAsset;
        event Action<IEntity> AssetLoaded;
        event Action<IEntity> AssetLoadingCancelled; 
        event Action<IEntity> AssetUnloaded;

        bool IsAssetLoaded(IEntity entity);
        bool IsLoadingAssets();
        bool IsLoadingAssetsOfType(DbModelType assetType);
        bool IsCharacterLoaded(long characterId, long? outfitId);
        bool IsCharacterMeshReady(long characterId, long? outfitId);

        T[] GetActiveAssets<T>() where T : IAsset;
        T GetActiveAssetOfType<T>(long id) where T : IAsset;
        IAsset[] GetAllActiveAssets();
        IAsset[] GetAllLoadedAssets();
        IAsset[] GetAllLoadedAssets(DbModelType assetType);
        T[] GetAllLoadedAssets<T>();
        
        void Load<TEntity, TArgs>(TEntity modelData, TArgs args = null, Action<IAsset> onCompleted = null, Action<string> onFailed = null, Action onCancelled = null)
            where TEntity : IEntity where TArgs: LoadAssetArgs<TEntity>;

        void Load<TEntity>(TEntity modelData, Action<IAsset> onCompleted = null, Action<string> onFailed = null, Action onCancelled = null)
            where TEntity : IEntity;

        void Unload(IAsset target, Action onCompleted = null);
        void Unload<TEntity>(TEntity entity, Action onCompleted = null) where TEntity: IEntity;
        void UnloadAll(DbModelType target, params IAsset[] targetExceptions);
        void UnloadAllExceptFor(params IAsset[] targetExceptions);
        void UnloadAllExceptFor(ICollection<IAsset> targetExceptions);
        void UnloadAll();

        void CancelLoadingCurrentAssets();
        
        void DeactivateAllExceptFor(params IAsset[] targetExceptions);
        void DeactivateAll();
    }
}