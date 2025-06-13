using System.Threading.Tasks;
using Bridge.Models.Common;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.AssetsManaging.Loaders.DependencyLoading
{
    /// <summary>
    /// If asset does not have other dependencies
    /// </summary>
    internal sealed class NoDependencyLoader<TEntity, TArgs>: DependencyLoader<TEntity, TArgs> 
        where TEntity: IEntity where TArgs: LoadAssetArgs<TEntity>
    {
        public override bool HasDependenciesToLoad(IAsset asset, TArgs args)
        {
            return false;
        }

        protected override Task<LoadResult> LoadDependencies(TEntity entity, TArgs args, IAsset asset)
        {
            return Task.FromResult(new LoadResult {IsSuccess = true});
        }
    }
}