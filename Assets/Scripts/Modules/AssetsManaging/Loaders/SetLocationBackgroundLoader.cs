using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class SetLocationBackgroundLoader : TextureAssetLoader<SetLocationBackground, SetLocationBackgroundLoadArgs>
    {
        private readonly ISetLocationBackgroundInMemoryCache _inMemoryCache;
        
        public SetLocationBackgroundLoader(IBridge bridge, ISetLocationBackgroundInMemoryCache inMemoryCache) : base(bridge)
        {
            _inMemoryCache = inMemoryCache;
        }

        protected override IAsset CreateView(Texture2D texture)
        {
            var view = new SetLocationBackgroundAsset();
            view.Init(Model, texture);
            return view;
        }

        protected override async Task Download(CancellationToken cancellationToken = default)
        {
            if (_inMemoryCache.TryGetBackgroundTexture(Model.Id, out var texture))
            {
                OnFileLoaded(texture);
                return;
            }
            
            await base.Download(cancellationToken);

            if (Target != null)
            {
                _inMemoryCache.Add(Model.Id, Target as Texture2D);
            }
        }
    }
}