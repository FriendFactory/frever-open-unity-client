using System;
using Bridge.Models.Common;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Loaders.DependencyLoading;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging
{
    // provides necessary services/data for different assets loading/unloading
    internal abstract class AssetLoadProfile<TEntity,TArgs> : IAssetLoadProfile, ILoaderProvider<TEntity,TArgs>, IUnloaderProvider, IDefaultLoadArgsProvider<TEntity>
        where TEntity: IEntity where TArgs: LoadAssetArgs<TEntity>
    {
        public abstract AssetLoader<TEntity, TArgs> GetAssetLoader();
        public abstract AssetUnloader GetUnloader();
        public virtual LoadAssetArgs<TEntity> GetDefaultLoadArgs() => Activator.CreateInstance<TArgs>();
        public virtual DependencyLoader<TEntity, TArgs> GetDependencyLoader() => new NoDependencyLoader<TEntity, TArgs>();
    }

    internal interface IAssetLoadProfile
    {
    }
    
    internal interface ILoaderProvider<TEntity, TArgs>
        where TEntity : IEntity where TArgs : LoadAssetArgs<TEntity>
    {
        AssetLoader<TEntity, TArgs> GetAssetLoader();
        DependencyLoader<TEntity, TArgs> GetDependencyLoader();
    }
    
    internal interface IUnloaderProvider
    { 
        AssetUnloader GetUnloader();
    }

    internal interface IDefaultLoadArgsProvider<TEntity> where TEntity : IEntity
    { 
        LoadAssetArgs<TEntity> GetDefaultLoadArgs();
    }
}