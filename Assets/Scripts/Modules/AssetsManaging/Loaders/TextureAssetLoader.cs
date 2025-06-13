using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.Common;
using UnityEngine;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.AssetsManaging.Loaders
{
    internal abstract class TextureAssetLoader<TFileEntity, TArgs> : FileAssetLoader<TFileEntity, TArgs>
        where TFileEntity : IFilesAttachedEntity
        where TArgs : LoadAssetArgs<TFileEntity>
    {
        protected TextureAssetLoader(IBridge bridge) : base(bridge)
        {
        }
        
        protected sealed override void InitAsset()
        {
            Asset = CreateView(Target as Texture2D);
        }

        protected abstract IAsset CreateView(Texture2D texture);
        
        protected override async Task Download(CancellationToken cancellationToken = default)
        {
            var result = await Bridge.GetAssetAsync(Model, true, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            
            if (result.IsSuccess)
            {
                OnFileLoaded(result.Object);
            }
            else if (result.IsError)
            {
                OnFailed(GetErrorResult(result.ErrorMessage));
            }
        }
    }
}