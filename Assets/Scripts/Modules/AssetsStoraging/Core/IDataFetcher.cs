using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.StartPack;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Services._7Digital.Models.PlaylistModels;
using CharacterSpawnPositionFormation = Bridge.Models.ClientServer.StartPack.Metadata.CharacterSpawnPositionFormation;
using SpawnPositionSpaceSize = Bridge.Models.ClientServer.StartPack.Metadata.SpawnPositionSpaceSize;
using TemplateCategory = Bridge.Models.ClientServer.StartPack.Metadata.TemplateCategory;
using TemplateSubCategory = Bridge.Models.ClientServer.StartPack.Metadata.TemplateSubCategory;

namespace Modules.AssetsStoraging.Core
{
    public interface IDataFetcher: ISeasonProvider, IMetadataProvider
    {
        event Action OnDataFetched;
        event Action OnStartPackFetched;
        event Action OnSeasonFetched;
        event Action<float> FetchProgressed;
        event Action<DefaultUserAssets> OnUserAssetsFetched;
        event Action MetadataPackFetched;
        event Action<string> OnFetchFailed;

        DefaultUserAssets DefaultUserAssets { get; }
        bool IsFetched { get; }
        bool IsFetching { get; }
        bool IsStartPackFetched { get; }
        bool IsCriticalDataLoaded { get; }
        bool IsCriticalDataLoadingFailed { get; }
        string CriticalDataLoadingFailReason{ get; }
        bool IsCriticalDataLoading { get; }

        void Refetch();
        void Fetch();
        Task FetchSeason();
        void LoadCriticalData();
        Task FetchLocalization();
        void FetchUserAssets();
        void ResetData();
        Task<WardrobeFullInfo[]> GetHighlightingWardrobes(int genderId, string[] names, CancellationToken token = default);
        Task FetchSongsAsync();
    }

    public interface ISeasonProvider
    {
        CurrentSeason CurrentSeason { get; }
    }

    public interface IMetadataProvider
    {
        MetadataStartPack MetadataStartPack { get; }
        long DefaultTemplateId { get; }
        WardrobeCategoriesForGender[] WardrobeCategoriesForGenders { get; }
        ICollection<TemplateCategory> TemplateCategories { get; }
        ICollection<TemplateSubCategory> TemplateSubCategories { get; }
        ICollection<SpawnPositionSpaceSize> SpawnPositionSpaceSizes { get; }
        ICollection<CharacterSpawnPositionFormation> CharacterSpawnPositionFormations { get; }
        ICollection<VoiceFilterFullInfo> VoiceFilters { get; }
        ICollection<WardrobeCategory> WardrobeCategories { get; }
        IReadOnlyCollection<SongInfo> AllSongs { get; }
        ICollection<PlaylistInfo> AllPlaylists { get; }
    }
}