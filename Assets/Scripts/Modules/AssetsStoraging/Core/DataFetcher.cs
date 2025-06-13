using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Models.Common;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Services._7Digital.Models.PlaylistModels;
using JetBrains.Annotations;
using Zenject;
using CharacterSpawnPositionFormation = Bridge.Models.ClientServer.StartPack.Metadata.CharacterSpawnPositionFormation;
using SpawnPositionSpaceSize = Bridge.Models.ClientServer.StartPack.Metadata.SpawnPositionSpaceSize;
using TemplateCategory = Bridge.Models.ClientServer.StartPack.Metadata.TemplateCategory;
using TemplateSubCategory = Bridge.Models.ClientServer.StartPack.Metadata.TemplateSubCategory;
using System.Threading.Tasks;
using System.Threading;
using AppStart;
using Bridge.Models.ClientServer.StartPack;
using Modules.MusicLicenseManaging;

namespace Modules.AssetsStoraging.Core
{
    [UsedImplicitly]
    internal sealed class DataFetcher : IDataFetcher
    {
        private FetchStepData[] _fetchStepDatas = 
        {
            new() { FetchStepType = FetchStepType.MetaData, Index = 0, ProgressWeight = 0.5f },
            new() { FetchStepType = FetchStepType.Files, Index = 1, ProgressWeight = 0.5f }
        };

        private readonly FetcherConfig _fetcherConfig;
        
        private readonly DbDataFetcher _dbFetcher;
        private readonly IBridge _bridge;
        private readonly LocalizationSetup _locSetup;
        
        public event Action<float> FetchProgressed;
        private float _currentProgress;

        private ICollection<TemplateSubCategory> _templateSubCategories;
        private ICollection<CharacterSpawnPositionFormation> _characterSpawnPositionFormations;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnDataFetched;
        public event Action OnStartPackFetched;
        public event Action MetadataPackFetched;
        public event Action OnSeasonFetched;
        public event Action<DefaultUserAssets> OnUserAssetsFetched;
        public event Action<string> OnFetchFailed;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private IEntity[] AvailableAssets { get; set; }
        public DefaultUserAssets DefaultUserAssets => _dbFetcher.DefaultUserAssets;
        public MetadataStartPack MetadataStartPack => _dbFetcher.MetadataStartPack;
        public CurrentSeason CurrentSeason => _dbFetcher.CurrentSeason;
        public long DefaultTemplateId => _dbFetcher.DefaultUserAssets.TemplateId;
        public WardrobeCategoriesForGender[] WardrobeCategoriesForGenders => _dbFetcher.WardrobeCategoriesForGenders;
        public long TemplateIdForOnboarding => _dbFetcher.DefaultUserAssets.OnboardingTemplateId;

        public IReadOnlyCollection<SongInfo> AllSongs => _dbFetcher.AllSongs;
        public ICollection<PlaylistInfo> AllPlaylists => _dbFetcher.Playlists;
        public bool IsFetched { get; private set; }
        public bool IsFetching { get; private set; }
        public bool IsStartPackFetched { get; private set; }
        public bool IsCriticalDataLoaded => _dbFetcher.IsCriticalDataLoaded;
        public bool IsCriticalDataLoadingFailed => _dbFetcher.IsCriticalDataLoadingFailed;
        public string CriticalDataLoadingFailReason => _dbFetcher.CriticalDataLoadingFailReason;
        public bool IsCriticalDataLoading => _dbFetcher.IsCriticalDataLoading;
        public ICollection<TemplateCategory> TemplateCategories => MetadataStartPack.TemplateCategories;

        public ICollection<TemplateSubCategory> TemplateSubCategories
        {
            get
            {
                return _templateSubCategories ?? (_templateSubCategories =
                    MetadataStartPack.TemplateCategories.Where(x=>x.SubCategories!=null).SelectMany(x => x.SubCategories).ToArray());
            }
        }
        
        public ICollection<SpawnPositionSpaceSize> SpawnPositionSpaceSizes => _dbFetcher.MetadataStartPack.SpawnPositionSpaceSizes;

        public ICollection<CharacterSpawnPositionFormation> CharacterSpawnPositionFormations
        {
            get
            {
                return _characterSpawnPositionFormations ?? (_characterSpawnPositionFormations = _dbFetcher
                   .MetadataStartPack.CharacterSpawnPositionFormationTypes
                   .SelectMany(x => x.CharacterSpawnPositionFormations)
                   .ToArray());
            }
        }

        public ICollection<VoiceFilterFullInfo> VoiceFilters => MetadataStartPack.VoiceFilters;
        public ICollection<WardrobeCategory> WardrobeCategories => MetadataStartPack.WardrobeCategories;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public DataFetcher(IBridge bridge, FetcherConfig fetcherConfig, MusicLicenseValidator musicLicenseValidator, CriticalDataFetcher criticalDataFetcher, LocalizationSetup localizationSetup)
        {
            _bridge = bridge;
            _fetcherConfig = fetcherConfig;
            _dbFetcher = new DbDataFetcher(bridge, musicLicenseValidator, criticalDataFetcher);
            _locSetup = localizationSetup;
            _dbFetcher.UserAssetsDataFetched += () =>
            {
                OnUserAssetsFetched?.Invoke(_dbFetcher.DefaultUserAssets);
            };
            _dbFetcher.MetaDataStartPackFetched += () =>
            {
                MetadataPackFetched?.Invoke();
            };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Refetch()
        {
            IsFetching = true;
            IsFetched = true;
            _dbFetcher.Prefetch(OnDbFetchComplete, x => OnProgress(x, FetchStepType.MetaData), OnFetchFailed);
        }

        public void Fetch()
        {
            if (IsFetching || IsFetched) return;
            IsFetching = true;

            _dbFetcher.Prefetch(OnDbFetchComplete, x => OnProgress(x, FetchStepType.MetaData), OnFetchFailed);
        }

        public void LoadCriticalData()
        {
            _dbFetcher.LoadCriticalData();
        }

        public async Task FetchLocalization()
        {
            await _locSetup.FetchLocalization();
        }

        public void ResetData()
        {
            _dbFetcher.ResetData();
        }

        public async Task FetchSeason()
        {
            await _dbFetcher.ReFetchCurrentSeason();
            
            OnSeasonFetched?.Invoke();
        }

        public async Task FetchSongsAsync()
        {
            await _dbFetcher.FetchSongsAsync();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        public void FetchUserAssets()
        {
            FetchUserAssetsInternal();
        }

        public async Task<WardrobeFullInfo[]> GetHighlightingWardrobes(int genderId, string[] names, CancellationToken token = default)
        {
            var result = await _bridge.GetWardrobeList(genderId, names, token);
            if (!result.IsSuccess) return default;
            return result.Models;
        }
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnProgress(float loadingStepValue, FetchStepType stepType)
        {
            var previousStepsProgress = _fetchStepDatas.OrderBy(x => x.Index)
                                                       .TakeWhile(x => x.FetchStepType != stepType)
                                                       .Sum(x => x.ProgressWeight);

            var currentStepWeight = _fetchStepDatas.First(x => x.FetchStepType == stepType).ProgressWeight;

            _currentProgress = previousStepsProgress + loadingStepValue * currentStepWeight;
            FetchProgressed?.Invoke(_currentProgress);
        }

        private void OnDbFetchComplete()
        {
            IsFetched = true;
            IsFetching = false;
            OnDataFetched?.Invoke();

            DownloadStartPackAssets();
        }

        private async void DownloadStartPackAssets()
        {
            AvailableAssets = _dbFetcher.Entities.SelectMany(x => x.Value).ToArray();

            var fetchAssetsTask = _bridge.FetchStartPackAssets(_fetcherConfig.MaxConcurrentRequestsCount, x => OnProgress(x, FetchStepType.Files));
            var fetchLocalizationTask = _locSetup.FetchLocalization();

            await Task.WhenAll(fetchAssetsTask, fetchLocalizationTask);
            
            IsStartPackFetched = true;

            OnStartPackFetched?.Invoke();
        }

        private async void FetchUserAssetsInternal()
        {
            if (_dbFetcher.DefaultUserAssets != null)
            {
                return;
            }

            _dbFetcher.LoadCriticalData();
            while (_dbFetcher.IsCriticalDataLoading)
            {
                await Task.Delay(50);
            }
            
            if (_dbFetcher.IsCriticalDataLoadingFailed)
            {
                Debug.LogError(_dbFetcher.CriticalDataLoadingFailReason);
                return;
            }

            OnUserAssetsFetched?.Invoke(_dbFetcher.DefaultUserAssets);
        }
        
        private enum FetchStepType
        {
            MetaData,
            Files
        }
        
        private struct FetchStepData
        {
            public FetchStepType FetchStepType;
            public float ProgressWeight;
            public float Index;
        }
    }
}