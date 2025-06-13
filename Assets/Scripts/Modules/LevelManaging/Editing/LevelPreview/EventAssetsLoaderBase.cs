using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.ExternalPackages.AsynAwaitUtility;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.FreverUMA;
using UnityEngine;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal abstract class EventAssetsLoaderBase
    {
        private static int _loadingCounter;
        
        protected readonly IAssetManager AssetManager;
        protected readonly List<IAsset> LoadedAssets;
        protected readonly ReusedAssetsAlgorithm ReusedAssetsAlgorithm;
        protected readonly AvatarHelper AvatarHelper;

        private Action<List<IAsset>> _onComplete;
        private Event[] _events;

        private readonly IConcreteAssetTypeLoader[] _loaders;
        private readonly Dictionary<long, bool> _loadingProcessCancelState = new();
        private Action _onCancel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected EventAssetsLoaderBase(IAssetManager assetManager, AvatarHelper avatarHelper, ReusedAssetsAlgorithm reusedAssetsAlgorithm, 
                                        IConcreteAssetTypeLoader[] levelPreviewAwaitableAssetLoaders, IConcreteAssetTypeLoader[] levelPreviewAssetLoaders)
        {
            AssetManager = assetManager;
            LoadedAssets = new List<IAsset>();
            ReusedAssetsAlgorithm = reusedAssetsAlgorithm;
            _loaders = levelPreviewAssetLoaders;
            AvatarHelper = avatarHelper;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void CancelLoadingAssets()
        {
            foreach (var loadingId in _loadingProcessCancelState.Keys.ToArray())
            {
                _loadingProcessCancelState[loadingId] = true;
            }
            
            foreach (var loader in _loaders)
            {
                loader.Cancel();
            }
            CleanupLoaders();
            _onCancel?.Invoke();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void LoadAssetsInternal(Event[] events, bool unloadAllNotUsedAssets, Action<List<IAsset>> onComplete, Action onCancel = null)
        {
            _events = events;
            _onComplete = onComplete;
            _onCancel =  onCancel;
            LoadedAssets.Clear();
            
            RegisterAlreadyLoadedAssets();
           
            if (unloadAllNotUsedAssets)
            {
                AssetManager.UnloadAllExceptFor(LoadedAssets);
            }
            
            Initialize(events);
            
            if (AllAssetsAlreadyLoaded())
            {
                OnCompleted();
            }
            else
            {
                var loadingId = _loadingCounter++;
                _loadingProcessCancelState.Add(loadingId, false);
                RunLoaders(loadingId);
            }
        }

        protected virtual void OnCompleted()
        {
            CleanupLoaders();
            _onComplete(LoadedAssets);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnLoaderFinished(IAsset[] asset)
        {
            LoadedAssets.AddRange(asset);
            CheckIfCompleted();
        }

        private void CheckIfCompleted()
        {
            if (_loaders.Any(x =>!x.IsFinished)) return;
            OnCompleted();
        }

        private void RegisterAlreadyLoadedAssets()
        {
            var reused = ReusedAssetsAlgorithm.GetAlreadyLoadedAssetsUsedBy(_events);
            StoreAlreadyLoadedAssets(reused);
        }

        private void StoreAlreadyLoadedAssets(IAsset[] reused)
        {
            LoadedAssets.AddRange(reused);
        }

        private void Initialize(ICollection<Event> events)
        {
            foreach (var loader in _loaders)
            {
                loader.Prepare(events);
            }
        }

        private async void RunLoaders(long processId)
        {
            SubscribeLoaders();

            try
            {
                var characterLoader = _loaders.First(x => x.Type == DbModelType.Character);
                if (characterLoader.HasAssetsToLoad)
                {
                    await characterLoader.RunAsync();
                    await CleanupMemory();
                }
                
                if (IsProcessCancelled(processId))
                {
                    OnProcessCancelled(processId);
                    return;
                }

                var setLocationLoader = _loaders.First(x => x.Type == DbModelType.SetLocation);
                if (setLocationLoader.HasAssetsToLoad)
                {
                    await setLocationLoader.RunAsync();
                    await CleanupMemory();
                }

                if (IsProcessCancelled(processId))
                {
                    OnProcessCancelled(processId);
                    return;
                }

                var remainingLoaders = _loaders.Where(x => !x.IsFinished);
                foreach (var loader in remainingLoaders)
                {
                    if(!loader.HasAssetsToLoad) continue;
                    loader.Run();
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    OnProcessCancelled(processId);
                    return;
                }
                Debug.LogError(e);
            }
        }

        private void SubscribeLoaders()
        {
            foreach (var loader in _loaders)
            {
                loader.Finished += OnLoaderFinished;
            }
        }

        private void CleanupLoaders()
        {
            foreach (var loader in _loaders)
            {
                loader.Finished -= OnLoaderFinished;
            } 
        }
        
        private bool AllAssetsAlreadyLoaded()
        {
            return _loaders.All(x=>!x.HasAssetsToLoad);
        }

        private async Task CleanupMemory()
        {
            await Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private bool IsProcessCancelled(long processId)
        {
            return _loadingProcessCancelState[processId];
        }

        private void OnProcessCancelled(long processId)
        {
            if (_loadingProcessCancelState.ContainsKey(processId))
            {
                _loadingProcessCancelState.Remove(processId);
            }
        }
    }
}