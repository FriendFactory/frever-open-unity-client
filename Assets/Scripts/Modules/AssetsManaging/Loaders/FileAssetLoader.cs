using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Bridge;

namespace Modules.AssetsManaging.Loaders
{
    internal abstract class FileAssetLoader<TFileEntity, TArgs> : AssetLoader<TFileEntity, TArgs>
        where TFileEntity: IFilesAttachedEntity
        where TArgs: LoadAssetArgs<TFileEntity>
    {
        public virtual bool CacheFile => true;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected FileAssetLoader(IBridge bridge) : base(bridge)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task LoadAsset(TArgs args)
        {
            return Download(args.CancellationToken);
        }

        protected virtual async Task Download(CancellationToken cancellationToken = default)
        {
            var result = await Bridge.GetAssetAsync(Model, CacheFile, cancellationToken);

            if (IsCancellationRequested)
            {
                OnCancelled();
                return;
            }
            
            if (result.IsSuccess)
            {
                OnFileLoaded(result.Object);
            }
            else
            {
                OnFailed(GetErrorResult(result.ErrorMessage));
            }
        }
        
        protected void OnFileLoaded(object target)
        {
            Target = target;
            InitAsset();
            OnCompleted(GetSuccessResult());
        }
    }
}