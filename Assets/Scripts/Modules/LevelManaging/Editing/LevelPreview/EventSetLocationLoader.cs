using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Editing.AssetChangers;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventSetLocationLoader :  DefaultLoader<SetLocationFullInfo, SetLocationLoadArgs>
    {
        private readonly ILayerManager _layerManager;
        
        public override DbModelType Type => DbModelType.SetLocation;
        
        protected override SetLocationLoadArgs[] Args => null;

        public EventSetLocationLoader(IAssetManager assetManager, ILayerManager layerManager) : base(assetManager)
        {
            _layerManager = layerManager;
        }

        public override void Run()
        {
            #pragma warning disable 4014
            RunAsync();
            #pragma warning restore 4014
        }

        public override async Task RunAsync()
        {
            IsRunning = true;
            CheckIfHaveAssetsToLoad();
            CancellationToken = new CancellationTokenSource();
            
            foreach (var setLocation in AssetsToLoad)
            {
                var countRemainingToLoad = AssetsToLoad.Count;
                var args = new SetLocationLoadArgs
                {
                    CancellationToken = CancellationToken.Token,
                    PictureInPictureLayerMask = _layerManager.GetCharacterLayers()
                };
                AssetManager.Load(setLocation, args, OnAssetLoaded, OnFail);
                //todo: remove hotfix for FREV-7934
                if (AssetsToLoad.Count == 1) break;
                
                if (!OptimizeMemory) continue;
                // Only load one at the time
                while (countRemainingToLoad <= AssetsToLoadRemaining)
                {
                    await Task.Delay(25);
                }
                GC.Collect();
            }

            await WaitForAllAssetsLoaded();
            
            GC.Collect();
        }

        protected override ICollection<SetLocationFullInfo> ExtractAssetModels(Event @event)
        {
            return new[] {@event.GetSetLocation()};
        }
    }
}
