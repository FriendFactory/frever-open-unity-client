using System;
using System.Threading.Tasks;
using Bridge.Models.Common;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.AssetsManaging.Loaders.DependencyLoading
{
    internal abstract class DependencyLoader<TEntity, TArgs> 
        where TEntity: IEntity where TArgs: LoadAssetArgs<TEntity>
    {
        public abstract bool HasDependenciesToLoad(IAsset asset, TArgs args);
        
        public void Load(TEntity entity, TArgs args, IAsset asset, Action onComplete, Action<string> onFailed, Action onCancelled = null)
        {
            LoadAsync(entity, args, asset,onComplete, onFailed, onCancelled);
        }

        private async void LoadAsync(TEntity entity, TArgs args, IAsset asset, Action onCompleted, Action<string> onFailed, Action onCancelled)
        {
            var result = await LoadDependencies(entity, args, asset);

            if (result.IsCancelled)
            {
                onCancelled?.Invoke();
            }
            else if (result.IsSuccess)
            {
                onCompleted?.Invoke();
            }
            else
            {
                onFailed?.Invoke(result.ErrorMessage);
            }
        }
    
        protected abstract Task<LoadResult> LoadDependencies(TEntity entity, TArgs args, IAsset asset);
    }
    
    internal struct LoadResult
    {
        public bool IsSuccess;
        public bool IsCancelled;
        public bool IsError;
        public string ErrorMessage;
    }
}
