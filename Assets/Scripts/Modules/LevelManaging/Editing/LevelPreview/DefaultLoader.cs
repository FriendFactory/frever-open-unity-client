using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Extensions;
using Modules.AssetsManaging;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    /// <summary>
    /// This loader cover almost all cases, where we have 1 entity = 1 asset
    /// The only character asset can have the same character asset with different outfits, so basically we need to
    /// few character views(meshes) for the same character. That's why we have another implementation for character loader
    /// </summary>
    internal abstract class DefaultLoader<T, TArgs> : LoaderBase<T, TArgs>
        where T : IEntity where TArgs : LoadAssetArgs<T>
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected abstract TArgs[] Args { get; }
        protected List<T> AssetsToLoad { get; private set; } = new List<T>();

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        protected DefaultLoader(IAssetManager assetManager): base(assetManager)
        {
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Prepare(ICollection<Event> events)
        {
            AssetsToLoad.Clear();
            
            CollectAssetsToLoad(events);
            
            AssetsToLoad = SortAssetsToLoad(AssetsToLoad);
            LoadedAssets.Clear();
            AssetsToLoadRemaining = AssetsToLoad.Count;
        }

        public override void Run()
        {
            LoadAssets();
            IsRunning = true;
        }
        
        public override async Task RunAsync()
        {
            Run();
            while (!IsFinished)
            {
                await Task.Delay(33);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract ICollection<T> ExtractAssetModels(Event @event);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void LoadAssets()
        {
            CheckIfHaveAssetsToLoad();

            CancellationToken = new CancellationTokenSource();
            
            for (var i = 0; i < AssetsToLoad.Count; i++)
            {
                var args = GetArgs(i);
                if(args != null) args.CancellationToken = CancellationToken.Token;
                
                LoadAsset(AssetsToLoad[i], args);
            }
        }

        private TArgs GetArgs(int characterIndex)
        {
            if (Args == null || Args.Length == 0) return null;
            //try to take argument per asset. if argument is only 1 or < assets count: take first
            return Args.Length > characterIndex ? Args[characterIndex] : Args.First();
        }
        
        private List<T> SortAssetsToLoad(List<T> assetsToLoad)
        {
            return assetsToLoad.Unique();
        }

        private void CollectAssetsToLoad(ICollection<Event> events)
        {
            foreach (var eventData in events)
            {
                var assetModels = ExtractAssetModels(eventData);

                CollectNotYetLoadedAssets(assetModels);
            }
        }

        private void CollectNotYetLoadedAssets(ICollection<T> assetModels)
        {
            foreach (var assetModel in assetModels)
            {
                if (AssetManager.IsAssetLoaded(assetModel)) continue;
                AssetsToLoad.Add(assetModel);
            }
        }
    }
}