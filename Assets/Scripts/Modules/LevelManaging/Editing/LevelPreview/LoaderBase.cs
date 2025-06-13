using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Extensions;
using Modules.AssetsManaging;
using Settings;
using UnityEngine;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    internal abstract class LoaderBase<T, TArgs> : IConcreteAssetTypeLoader
        where T : IEntity where TArgs : LoadAssetArgs<T>
    {
        protected readonly IAssetManager AssetManager;

        protected CancellationTokenSource CancellationToken;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected int AssetsToLoadRemaining { get; set; }
        protected List<IAsset> LoadedAssets { get; } = new List<IAsset>();
        protected bool OptimizeMemory => AppSettings.UseOptimizedMemory;
        public bool IsFinished => AssetsToLoadRemaining == 0;
        public bool HasAssetsToLoad => AssetsToLoadRemaining > 0;
        protected bool IsRunning { get; set; }
        public abstract DbModelType Type { get; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<IAsset[]> Finished;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        public abstract void Prepare(ICollection<Event> events);

        public abstract void Run();

        public abstract Task RunAsync();
        
        public void Cancel()
        {
            if (!IsRunning) return;
            
            CancellationToken.Cancel();
            IsRunning = false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected LoaderBase(IAssetManager assetManager)
        {
            AssetManager = assetManager;
        }

        protected void CheckIfHaveAssetsToLoad()
        {
            if (!HasAssetsToLoad)
            {
                throw new InvalidOperationException($"{GetType().Name}: There is no assets to load");
            }
        }
        
        protected void LoadAsset(T entity, TArgs args)
        {
            AssetManager.Load(entity, args, OnAssetLoaded, OnFail);
        }
        
        protected void OnAssetLoaded(IAsset asset)
        {
            AssetsToLoadRemaining--;
            LoadedAssets.Add(asset);
            if (AssetsToLoadRemaining > 0) return;
            OnFinished();
        }
        
        protected async Task LoadAssetAsync(T entity, TArgs args)
        {
            var isLoaded = false;
            IAsset loadedAsset = null;
            AssetManager.Load(entity, args, OnLoaded, OnFail);
            
            void OnLoaded(IAsset asset)
            {
                isLoaded = true;
                loadedAsset = asset;   
            }

            while (!isLoaded)
            {
                await Task.Delay(20, args.CancellationToken);
            }

            OnAssetLoaded(loadedAsset);
        }
        
        protected async Task WaitForAllAssetsLoaded(CancellationToken token = default)
        {
            while (AssetsToLoadRemaining > 0)
            {
                await Task.Delay(33, token);
            }
        }
        
        protected void OnFail(string msg)
        {
            Debug.LogError(msg);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnFinished()
        {
            IsRunning = false;
            Finished?.Invoke(LoadedAssets.ToArray());
        }
    }
}