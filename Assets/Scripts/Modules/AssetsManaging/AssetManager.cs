using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AssetBundleLoaders;
using Bridge.Models.Common;
using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Recording.Core;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.AssetsManaging
{
    [UsedImplicitly]
    internal sealed class AssetManager : IAssetManager
    {
        private readonly List<IAsset> _loadedAssets = new List<IAsset>();
        private readonly AssetServicesProvider _assetServicesProvider;
        private readonly Dictionary<long, LoadingEntityEntry> _loadingEntities = new Dictionary<long, LoadingEntityEntry>();
        private readonly CharacterViewContainer _characterViewContainer;
        
        //cached
        private readonly MethodInfo _mainLoadMethod;
        private readonly MethodInfo _getDefaultArgsMethod;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<DbModelType, long> StartUpdatingAsset;
        public event Action<DbModelType, long> StopUpdatingAsset;
        public event Action<IEntity> AssetLoaded;
        public event Action<IEntity> AssetLoadingCancelled;
        public event Action<IEntity> AssetUnloaded;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public IReadOnlyCollection<DbModelType> AssetTypes => _assetServicesProvider.SupportedTypes;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public AssetManager(IBridge bridge, AudioSourceManager audioSourceManager, IAssetBundleLoader assetBundleLoader,
            UncompressedBundlesManager uncompressedBundlesManager, FaceAnimationConverter faceAnimationConverter,
            FaceBlendShapeMap faceBlendShapeMap, AssetServicesProvider assetServicesProvider, CharacterViewContainer characterViewContainer)
        {
            _assetServicesProvider = assetServicesProvider;
            _characterViewContainer = characterViewContainer;
            _mainLoadMethod = GetType().GetMethods()
                .Where(x => x.Name == nameof(Load)).First(x => x.GetGenericArguments().Length == 2);
            _getDefaultArgsMethod = _assetServicesProvider.GetType().GetMethod(nameof(_assetServicesProvider.GetDefaultLoadArgs));
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Load<TEntity, TArgs>(TEntity entity, TArgs loadAssetArgs, Action<IAsset> onCompleted, Action<string> onFailed, Action onCancelled) 
            where TEntity : IEntity where TArgs: LoadAssetArgs<TEntity>
        {
            StartUpdatingAsset?.Invoke(entity.GetModelType(), entity.Id);

            if (loadAssetArgs == null) loadAssetArgs = _assetServicesProvider.GetDefaultLoadArgs<TEntity>() as TArgs;
            
            var asset = FindAsset(_loadedAssets, entity);

            if (asset != null)
            {
                OnAssetLoaded(asset, entity, loadAssetArgs, onCompleted);
            }
            else
            {
                if (IsLoadingNow(entity))
                {
                    WaitUntilLoaded(entity, loadAssetArgs, onCompleted, onFailed);
                }
                else
                {
                    LoadFromBackend(entity,loadAssetArgs, onCompleted, onFailed, onCancelled);
                }
            }
        }

        public void Load<TEntity>(TEntity modelData, Action<IAsset> onCompleted = null, Action<string> onFailed = null, Action onCancelled = null)
            where TEntity : IEntity
        {
            var defaultArgs = _getDefaultArgsMethod.MakeGenericMethod(modelData.GetType()).Invoke(_assetServicesProvider, Array.Empty<object>());
            _mainLoadMethod.MakeGenericMethod(modelData.GetType(), defaultArgs.GetType()).Invoke(this,
                new[] {modelData, defaultArgs, onCompleted, onFailed, onCancelled});
        }

        public void Unload<TEntity>(TEntity entity, Action onCompleted = null) where TEntity : IEntity
        {
            var asset = FindAsset(_loadedAssets, entity);
            if (asset == null)
            {
                Debug.LogWarning($"Trying to unload asset which is not loaded. Asset: {entity.GetModelType()}, id: {entity.Id}");
                onCompleted?.Invoke();
            }
            Unload(asset, onCompleted);
        }

        public void DeactivateAllExceptFor(params IAsset[] targetExceptions)
        {
            var activeAssets = _loadedAssets.Where(x => x.IsActive);
            foreach (var activeAsset in activeAssets)
            {
                if (targetExceptions != null)
                {
                    var containsInException = targetExceptions.Any(x => x.Equals(activeAsset));
                    if (containsInException) continue;
                }
                
                activeAsset.SetActive(false);
            }
        }

        public void DeactivateAll()
        {
            foreach (var asset in _loadedAssets)
            {
                asset.SetActive(false);
            }
        }

        public void UnloadAll(DbModelType target, params IAsset[] targetExceptions)
        {
            var removeTargets = _loadedAssets.Where(x =>
                x.AssetType == target &&
                targetExceptions?.FirstOrDefault(y => y.AssetType == x.AssetType && y.Id == x.Id) == null).ToArray();

            Unload(removeTargets);
        }

        public void UnloadAllExceptFor(params IAsset[] targetExceptions)
        {
            UnloadExcept(targetExceptions);
        }

        public void UnloadAllExceptFor(ICollection<IAsset> targetExceptions)
        {
            UnloadExcept(targetExceptions);
        }

        public void UnloadAll()
        {
            var assetTypes = _loadedAssets.Select(x => x.AssetType).Distinct().ToArray();
            foreach (var asset in assetTypes)
            {
                UnloadAll(asset);
            }
        }

        public T[] GetActiveAssets<T>() where T : IAsset
        {
            var activeAssets = _loadedAssets.Where(x => x.IsActive && x is T);
            return !activeAssets.Any() ? Array.Empty<T>() : activeAssets.Cast<T>().ToArray();
        }

        public T GetActiveAssetOfType<T>(long id) where T : IAsset
        {
            return (T)_loadedAssets.FirstOrDefault(x => x.IsActive && x.Id == id && x is T);
        }

        public IAsset[] GetAllActiveAssets()
        {
            return _loadedAssets.Where(x => x.IsActive).ToArray();
        }

        public IAsset[] GetAllLoadedAssets()
        {
            return _loadedAssets.ToArray();
        }

        public IAsset[] GetAllLoadedAssets(DbModelType assetType)
        {
            return _loadedAssets.Where(x => x.AssetType == assetType).ToArray();
        }

        public T[] GetAllLoadedAssets<T>()
        {
            return _loadedAssets.OfType<T>().ToArray();
        }

        public bool IsAssetLoaded(IEntity entity)
        {
            return _loadedAssets.Where(x => x.AssetType == entity.GetModelType()).Any(asset => asset.Id == entity.Id);
        }
        
        public bool IsLoadingAssets()
        {
            return _loadingEntities.Any();
        }
        
        public bool IsLoadingAssetsOfType(DbModelType type)
        {
            return _loadingEntities.Any(x => x.Value.Entity.GetModelType() == type);
        }

        public bool IsCharacterLoaded(long characterId, long? outfitId)
        {
            return GetAllLoadedAssets<ICharacterAsset>()
               .FirstOrDefault(x => x.Id == characterId && x.OutfitId.Compare(outfitId)) != null;
        }

        public bool IsCharacterMeshReady(long characterId, long? outfitId)
        {
            return _characterViewContainer.HasPreparedView(characterId, outfitId);
        }

        [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
        public void CancelLoadingCurrentAssets()
        {
            var entries = _loadingEntities.Values.ToArray();
            
            foreach (var entry in entries)
            {
                entry.CancelLoading();
                AssetLoadingCancelled?.Invoke(entry.Entity);
            }

            _loadingEntities.Clear();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private IAsset FindAsset<T>(IEnumerable<IAsset> assets, T entity) where T : IEntity
        {
            var modelType = entity.GetModelType();
            return assets.Where(asset => asset.AssetType == modelType && asset.Entity.Compare(entity))
                .OrderBy(x => x.IsActive).FirstOrDefault();
        }

        private bool IsLoadingNow<TEntity>(TEntity entity) where TEntity : IEntity
        {
            return _loadingEntities.Values.Any(x=> x.Entity.Id == entity.Id && x.Entity.GetModelType() == entity.GetModelType());
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private async void WaitUntilLoaded<TEntity, TArgs>(TEntity entity, TArgs args, Action<IAsset> onCompleted, Action<string> onFailed)
            where TEntity : IEntity where TArgs: LoadAssetArgs<TEntity>
        {
            IAsset currentAsset;
            bool isLoadingNow;

            var timeout = 10000;

            do
            {
                isLoadingNow = IsLoadingNow(entity);
                currentAsset = FindAsset(_loadedAssets, entity);

                if (currentAsset != null)
                {
                    OnAssetLoaded(currentAsset, entity, args, onCompleted);
                    return;
                }

                await Task.Delay(100);

                timeout -= 100;
                if (timeout > 0) continue;

                onFailed?.Invoke($"Asset [{entity.Id} {entity.GetModelType()}] loading timeout.");
                return;

            } while (isLoadingNow && currentAsset == null);
        }

        private async void LoadFromBackend<TEntity, TArgs>(TEntity entity, TArgs args,
            Action<IAsset> onCompleted, Action<string> onFailed, Action onCancelled)
            where TEntity : IEntity where TArgs: LoadAssetArgs<TEntity>
        {
            await LoadAsset(entity, args, OnLoadingComplete, OnLoadingFailed, OnLoadingCancelled);

            void OnLoadingComplete(AssetResult result)
            {
                UnRegisterFromLoadingAssets();
                _loadedAssets.Add(result.Asset);
                OnAssetLoaded(result.Asset, entity, args, onCompleted);
                AssetLoaded?.Invoke(entity);
            }

            void OnLoadingCancelled()
            {
                UnRegisterFromLoadingAssets();
                AssetLoadingCancelled?.Invoke(entity);
                onCancelled?.Invoke();
            }
            
            void OnLoadingFailed(AssetResult result)
            {
                UnRegisterFromLoadingAssets();
                onFailed?.Invoke(result.ErrorMessage);
            }

            void UnRegisterFromLoadingAssets()
            {
                if (!_loadingEntities.TryGetValue(entity.Id, out var entry)) return;
                
                entry.Dispose();
                _loadingEntities.Remove(entity.Id);
            }
        }

        private async Task LoadAsset<TEntity, TArgs>(TEntity entity, TArgs args, Action<AssetResult> onComplete, Action<AssetResult> onFail, Action onCancelled)
            where TEntity : IEntity where TArgs: LoadAssetArgs<TEntity>
        {
            if (!_loadingEntities.TryGetValue(entity.Id, out var loadingEntry))
            {
                loadingEntry = new LoadingEntityEntry(entity, args.CancellationToken);
                _loadingEntities.Add(entity.Id, loadingEntry);
            }
            
            args.CancellationToken = loadingEntry.CancellationToken;
            var loader = _assetServicesProvider.GetAssetLoader<TEntity, TArgs>();
            
            try
            {
                await loader.Load(entity, args, onComplete, onFail, onCancelled);
            }
            catch (OperationCanceledException)
            {
                onCancelled?.Invoke();
            }
        }

        public void Unload(IAsset target, Action onCompleted = null)
        {
            target.SetActive(false);
            
            var unloader = _assetServicesProvider.GeAssetUnloader(target.AssetType);
            target.PrepareForUnloading();
            unloader.Unload(target);
            _loadedAssets.Remove(target);
            onCompleted?.Invoke();
            AssetUnloaded?.Invoke(target.Entity);
        }

        private void Unload(IEnumerable<IAsset> assets)
        {
            foreach (var asset in assets)
            {
                Unload(asset);
            }
        }

        private void OnAssetLoaded<TEntity, TArgs>(IAsset asset, TEntity entity, TArgs args, Action<IAsset> onCompleted)
            where TEntity : IEntity where TArgs : LoadAssetArgs<TEntity>
        {
            var dependencyLoader = _assetServicesProvider.GetDependencyLoader<TEntity, TArgs>();
            if (dependencyLoader.HasDependenciesToLoad(asset, args))
            {
                dependencyLoader.Load(entity, args, asset, OnLoadedCompletely, Debug.LogError);
            }
            else
            {
                OnLoadedCompletely();
            }
            
            void OnLoadedCompletely()
            {
                if(args.CancellationToken.IsCancellationRequested) return;
                asset.SetActive(!args.DeactivateOnLoad);
                onCompleted?.Invoke(asset);
                StopUpdatingAsset?.Invoke(asset.AssetType, asset.Id);
            }
        }
        
        private void UnloadExcept(ICollection<IAsset> targetExceptions)
        {
            var removeTargets = _loadedAssets.Where(x => !targetExceptions.Contains(x)).ToArray();
            Unload(removeTargets);
        }
    }

    public class LoadingEntityEntry : IDisposable
    {
        public IEntity Entity { get; }
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public LoadingEntityEntry(IEntity entity, CancellationToken cancellationToken)
        {
            Entity = entity;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public void CancelLoading()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}