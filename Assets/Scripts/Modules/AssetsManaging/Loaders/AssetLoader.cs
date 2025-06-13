using System;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Bridge;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.AssetsManaging.Loaders
{
    internal abstract class AssetLoader<TEntity, TArgs> where TEntity: IEntity where TArgs: LoadAssetArgs<TEntity>
    {
        protected readonly IBridge Bridge;
        protected TEntity Model;
        protected TArgs LoadArgs;
        protected IAsset Asset;
        
        private Action<AssetResult> _onCompleteCallback;
        private Action<AssetResult> _onFailCallback;
        private Action _onCancelledCallback;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected object Target { get; set; }
        protected bool IsCancellationRequested => LoadArgs.CancellationToken.IsCancellationRequested;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected AssetLoader(IBridge bridge)
        {
            Bridge = bridge;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task Load(TEntity data, TArgs args, Action<AssetResult> onComplete, Action<AssetResult> onFail, Action onCancelled)
        {
            Model = data;
            LoadArgs = args;
            _onCompleteCallback = onComplete;
            _onFailCallback = onFail;
            _onCancelledCallback = onCancelled;
            await LoadAsset(args);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract Task LoadAsset(TArgs args);

        protected abstract void InitAsset();
        
        protected AssetResult GetSuccessResult()
        {
            return new AssetResult(Asset);
        }
        
        protected AssetResult GetErrorResult(string message)
        {
            return new AssetResult($"Failed to load asset: {Model}. Reason: {message}");
        }

        protected void OnCompleted(AssetResult result)
        {
            _onCompleteCallback?.Invoke(result);
        }

        protected void OnFailed(AssetResult result)
        {
            ReleaseResources();
            _onFailCallback?.Invoke(result);
        }
        
        protected void OnCancelled()
        {
            ReleaseResources();
            _onCancelledCallback?.Invoke();
        }

        protected virtual void ReleaseResources()
        {
        }
    }
}