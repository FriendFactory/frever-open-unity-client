using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AppStart;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Models.Common;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.StartPack;
using Bridge.Results;
using Bridge.Services._7Digital.Models.PlaylistModels;
using Modules.MusicLicenseManaging;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.AssetsStoraging.Core
{
    internal sealed class DbDataFetcher
    {
        private readonly IBridge _bridge;
        private readonly CriticalDataFetcher _criticalDataFetcher;
        private readonly MusicLicenseValidator _musicLicenseValidator;

        private FetchingState _state;
        private int _waitingForDownloading;
        private int _totalWaitingObjectsAmount;
        private bool _seasonFetched;
        
        private Action<float> _onProgress;
        private Action _onCompleted;
        private Action<string> _onFailed;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsCriticalDataLoaded => _criticalDataFetcher.IsFetched;
        public bool IsCriticalDataLoadingFailed => _criticalDataFetcher.IsFetchingFailed;
        public string CriticalDataLoadingFailReason => _criticalDataFetcher.ErrorMessage;
        public Dictionary<Type, List<IEntity>> Entities { get; private set; }
        public DefaultUserAssets DefaultUserAssets => _criticalDataFetcher.DefaultUserAssets;
        public MetadataStartPack MetadataStartPack => _criticalDataFetcher.MetadataStartPack;
        public WardrobeCategoriesForGender[] WardrobeCategoriesForGenders => _criticalDataFetcher.WardrobeCategoriesForGenders;
        public CurrentSeason CurrentSeason { get; private set; }
        public PlaylistInfo[] Playlists { get; private set; }
        public SongInfo[] AllSongs { get; private set; }

        private bool AreStartPacksLoaded => DefaultUserAssets != null && MetadataStartPack != null && WardrobeCategoriesForGenders != null;
        public bool IsCriticalDataLoading => _criticalDataFetcher.IsFetching;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action MetaDataStartPackFetched;
        public event Action UserAssetsDataFetched;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DbDataFetcher(IBridge serverBridge, MusicLicenseValidator musicLicenseValidator, CriticalDataFetcher criticalDataFetcher)
        {
            _bridge = serverBridge;
            _musicLicenseValidator = musicLicenseValidator;
            _criticalDataFetcher = criticalDataFetcher;
            _criticalDataFetcher.MetadataStartPackFetched += () => MetaDataStartPackFetched?.Invoke();
            _criticalDataFetcher.DefaultUserAssetsFetched += () => UserAssetsDataFetched?.Invoke();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Prefetch(Action onCompleted, Action<float> onProgress, Action<string> onFailed)
        {
            if (_state == FetchingState.FetchingStarted) return;

            _state = FetchingState.FetchingStarted;

            _onCompleted = onCompleted;
            _onProgress = onProgress;
            _onFailed = onFailed;

            _waitingForDownloading = 0;
            _totalWaitingObjectsAmount = 0;

            Entities = new Dictionary<Type, List<IEntity>>();
            
            PrefetchCharacters();
            PrefetchDataStartPacks();
            PrefetchCurrentSeason();
        }

        public async Task FetchSongsAsync()
        {
            var songsResult = await _bridge.GetSongsAsync(1500, 0);
            AllSongs = songsResult.Models;
            
            if (!_musicLicenseValidator.IsPremiumSoundsEnabled) return;

            var result = await _bridge.GetExternalPlaylists(null, 0, 10);
            Playlists = result.Models;
        }

        public async Task ReFetchCurrentSeason()
        {
            _seasonFetched = false;
            var resp = await _bridge.GetCurrentSeason();
            
            if (resp.HttpStatusCode == 403)
            {
                _seasonFetched = true;
                return;
            }
            
            if (resp.IsError)
            {
                throw new Exception($"Failed to load CurrentSeason. Reason: {resp.ErrorMessage}");
            }

            CurrentSeason = resp.Model;
            _seasonFetched = true;
        }

        public void ResetData()
        {
            _criticalDataFetcher.ResetDefaultUserAssets();
        }
        
        public void LoadCriticalData()
        {
            _criticalDataFetcher.Fetch();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void PrefetchCharacters()
        {
            OnFetchingOneTypeEntitiesStarted();

            while (DefaultUserAssets == null)
            {
                await Task.Delay(25);
            }
            
            var userCharacters = DefaultUserAssets.UserCharacters;
            OnModelFetched(userCharacters ?? new List<CharacterInfo>());
        }

        private async void PrefetchAssets<TModel>(Task<ArrayResult<TModel>> getRequest) where TModel:  class, IEntity
        {
            OnFetchingOneTypeEntitiesStarted();
            
            var resp = await getRequest;
            if (!resp.IsSuccess)
            {
                OnFetchFailed(resp.ErrorMessage); 
                return;
            }

            OnModelFetched(resp.Models);
        }

        private async Task PrefetchAssetsAsync<TModel>(Task<ArrayResult<TModel>> getRequest)
            where TModel : class, IEntity
        {
            OnFetchingOneTypeEntitiesStarted();
            
            var resp = await getRequest;
            if (!resp.IsSuccess)
            {
                OnFetchFailed(resp.ErrorMessage); 
                return;
            }

            OnModelFetched(resp.Models);
        }
        
        private void OnFetchingOneTypeEntitiesStarted()
        {
            _waitingForDownloading++;
            _totalWaitingObjectsAmount++;
        }

        private void OnModelFetched<T>(IReadOnlyCollection<T> models) where T : class, IEntity
        {
            if (models == null || models.Count == 0)
            {
                Debug.LogWarning("Prefetch not existed data " + typeof(T).Name);
                OnModelFetchFinished();
                return;
            }

            CreateNewListForEntity<T>();
            var newModels = GetNonAlreadyExistingModels(models);
            Entities[typeof(T)].AddRange(newModels);

            OnModelFetchFinished();
            GC.Collect();
        }

        private void CreateNewListForEntity<T>() where T : class, IEntity
        {
            if (!Entities.ContainsKey(typeof(T)))
            {
                Entities.Add(typeof(T), new List<IEntity>());
            }
        }

        private IEnumerable<T> GetNonAlreadyExistingModels<T>(IReadOnlyCollection<T> models) where T : class, IEntity
        {
            var result = new List<T>();
            var alreadyAddedModels = Entities[typeof(T)];
            
            foreach (var model in models)
            {
                var existingAsset = alreadyAddedModels.FirstOrDefault(asset => asset.Id == model.Id);

                if (existingAsset == null)
                {
                    result.Add(model);
                }
            }

            return result;
        }
        
        private void OnModelFetchFinished()
        {
            _waitingForDownloading--;
            CheckState();
        }

        private void OnFetchFailed(string message)
        {
            _waitingForDownloading--;
            CheckState();
            Debug.LogWarning("Fetching error: " + message);
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private void CheckState()
        {
            var progress =(_totalWaitingObjectsAmount - _waitingForDownloading) / (float) _totalWaitingObjectsAmount;

            _onProgress?.Invoke(progress);

            if (_state == FetchingState.FetchingStarted && _waitingForDownloading == 0)
            {
                _state = FetchingState.Completed;
                OnAssetsFetchingCompleted();
            }
        }

        private async void OnAssetsFetchingCompleted()
        {
            while (!AreStartPacksLoaded || !_seasonFetched)
            {
                await Task.Delay(25);
            }
            OnCompleted();
        }

        private void PrefetchDataStartPacks()
        {
           if (_criticalDataFetcher.IsFetched || _criticalDataFetcher.IsFetching) return;
           if (_criticalDataFetcher.IsFetchingFailed)
           {
               _onFailed?.Invoke("Failed to load critical data");
               return;
           }
           _criticalDataFetcher.Fetch();
        }

        private async void PrefetchCurrentSeason()
        {
            await ReFetchCurrentSeason();
        }

        private void OnCompleted()
        {
            _onCompleted?.Invoke();
        }
    }
}