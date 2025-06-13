using System;
using System.Linq;
using System.Threading.Tasks;
using AppStart;
using Bridge;
using Common;
using Common.BridgeAdapter;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.Crew;
using Modules.EditorsCommon;
using Modules.FeaturesOpening;
using Modules.FreverUMA;
using Modules.InAppPurchasing;
using Modules.LevelManaging.Editing.Templates;
using Modules.MusicLicenseManaging;
using Modules.SocialActions;
using Modules.UniverseManaging;
using Modules.WardrobeManaging;
using Modules.WatermarkManagement;
using Navigation.Core;
using Newtonsoft.Json;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.Templates;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Loading
{
    internal sealed class StartupLoadingPage : GenericPage<LoadingPageArgs>
    {
        [Inject] private SocialActionsManager _socialActionsManager;
        [Inject] private IBridge _bridge;
        [Inject] private IDataFetcher _fetcher;
        [Inject] private IBlockedAccountsManager _blockAccountManager;
        [Inject] private IAppFeaturesManager _appFeaturesManager;
        [Inject] private IIAPManager _iapManager;
        [Inject] private FollowersManager _followersManager;
        [Inject] private ITemplatesContainer _templatesContainer;
        [Inject] private IEditorSettingsProvider _editorSettingsProvider;
        [Inject] private CharacterManager _characterManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private PopupManagerHelper _popupHelper;
        [Inject] private TemplatesLoader _templatesLoader;
        [Inject] private IMovementTypeThumbnailsProvider _movementTypeThumbnailsProvider;
        [Inject] private MusicLicenseValidator _musicLicenseValidator;
        [Inject] private ILevelService _levelService;
        [Inject] private CrewService _crewService;
        [Inject] private CharacterThumbnailCacheAutoClearProcess _characterThumbnailCacheAutoClearProcess;
        [Inject] private LocalizationSetup _localizationSetup;
        [Inject] private ICharacterEditor _characterEditor;
        [Inject] private IUniverseManager _universeManager;
        [Inject] private IWatermarkService _watermarkService;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.StartupLoadingPage;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {
            
        }

        protected override void OnDisplayStart(LoadingPageArgs args)
        {
            base.OnDisplayStart(args);
            
            ReleaseRedundantResources();

            _fetcher.OnFetchFailed += OnFetchFailed;
            _fetcher.Fetch();
            
            FetchUserData();
            StartFetchingNotCriticalData();
            InitializeManagers();
            _characterThumbnailCacheAutoClearProcess.Run();
            AppEntryContext.OnLoadingStarted?.Invoke();
            WaitUntilCriticalDataIsFetched();
        }

        private async void WaitUntilCriticalDataIsFetched()
        {
            if (!_fetcher.IsCriticalDataLoaded && !_fetcher.IsCriticalDataLoading)
            {
                _fetcher.LoadCriticalData();
            }
            while (!_fetcher.IsCriticalDataLoaded)
            {
                await Task.Delay(25);
                if (_fetcher.IsCriticalDataLoadingFailed)
                {
                    OnFetchFailed(_fetcher.CriticalDataLoadingFailReason);
                }
            }
            PreFetchDataForLevelCreation();
            _movementTypeThumbnailsProvider.FetchMovementTypeThumbnails();
            if (!_localizationSetup.IsLocalizationDataSetup)
            {
                _localizationSetup.ApplyLastCacheLocalizationDataImmediate();
            }

            PrefectUniversesThumbnails();
            PrefetchMainCharacterThumbnails();
            _watermarkService.FetchWaterMarks();
            _characterEditor.Init(_fetcher.MetadataStartPack.WardrobeCategories.SelectMany(x=>x.SubCategories).ToArray());
            WardrobeCategoryData.SetEnvironment(_bridge.Environment);
            OpenPageArgs.OnDataFetchedAction?.Invoke();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            AppEntryContext.OnLoadingDone?.Invoke();
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InitializeManagers()
        {
            _socialActionsManager.Initialize(default);
            _appFeaturesManager.Initialize();
            _iapManager.Initialize();
            _followersManager.PrefetchDataForLocalUser();
        }
        
        private void OnFetchFailed(string errorMessage)
        {
            var serverError = JsonConvert.DeserializeObject<ServerError>(errorMessage);

            if (serverError.ErrorCode == "AccountBlocked")
            {
                _popupHelper.ShowAlertPopup(
                    "Your account have been blocked due to that you have been violating our Terms of Use”. " + 
                    "You may file an appeal for this decision by clicking here",
                    "Account blocked",
                    "Appeal",
                    () => Application.OpenURL("https://www.frever.com/appeal"));
            }
        }

        private void StartFetchingNotCriticalData()
        {
            _crewService.PrefetchBackgrounds();
            _appFeaturesManager.Initialized -= StartFetchingNotCriticalData;
            RandomBackground.PrefetchImages(_bridge);
        }

        private void PreFetchDataForLevelCreation()
        {
            PrefetchDefaultTemplate();
            PrefetchLevelCreationSettings();
            PrefetchMainCharacter();
            _templatesLoader.FetchPersonalisedTemplates(Constants.Templates.TEMPLATE_PAGE_SIZE);
            _levelService.FetchLevelForVideoMessage();
        }

        private async void PrefetchDefaultTemplate()
        {
            await _templatesContainer.FetchFromServer(_fetcher.DefaultTemplateId);
        }

        private async void PrefetchLevelCreationSettings()
        {
            await _editorSettingsProvider.FetchDefaultEditorSettings();
        }

        private async void PrefetchMainCharacter()
        {
            if (_bridge.Profile.MainCharacterId == null) return;
            var characterId = _bridge.Profile.MainCharacterId.Value;
            _characterManager.SetCharacterSilent(characterId);
            await _characterManager.FetchCharacterAsync(characterId);
        }

        private async void PrefectUniversesThumbnails()
        {
            var universes = _fetcher.MetadataStartPack.Universes;
            foreach (var universe in universes)
            {
                foreach (var fileInfo in universe.Files)
                {
                    if (fileInfo.Resolution != null)
                    {
                        await _bridge.FetchThumbnailAsync(universe, (Resolution)fileInfo.Resolution);
                    }
                }
            }
        }
        
        private async void PrefetchMainCharacterThumbnails()
        {
            var raceMainCharacters = _characterManager.RaceMainCharacters.ToDictionary(x=> x.Key, x => x.Value); //copied to prevent errors if it will be modified while fecthing
            foreach (var characterPair in raceMainCharacters)
            {
                var character = _characterManager.UserCharacters.First(character => character.Id == characterPair.Value);
                foreach (var fileInfo in character.Files)
                {
                    if (fileInfo.Resolution != null)
                    {
                        await _bridge.FetchThumbnailAsync(character, (Resolution)fileInfo.Resolution);
                    }
                }
            }
        }

        private async void FetchUserData()
        {
            await _blockAccountManager.Initialize();
        }

        private void ReleaseRedundantResources()
        {
            _bridge.DeleteTempFiles();
        }
    }
}