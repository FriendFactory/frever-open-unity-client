using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Common;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UnityEngine;
using Modules.CharacterManagement;
using Modules.LevelManaging.Assets;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using Zenject;
using CameraAnimationTemplate = Bridge.Models.ClientServer.StartPack.Metadata.CameraAnimationTemplate;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal abstract class LevelEditorUiBase: MonoBehaviour
    {
        [SerializeField] private GameObject _editorUiPanel;
        
        [Inject] protected ILevelManager LevelManager;
        [Inject] protected OutfitsManager OutfitsManager;
        [Inject] protected IBridge Bridge;
        [Inject] private IDataFetcher _dataFetcher;
        
        [Inject] private SetLocationPaginationLoader.Factory _setLocationFactory;
        [Inject] private BodyAnimationPaginationLoader.Factory _bodyAnimationFactory;
        [Inject] private CameraFilterPaginationLoader.Factory _cameraFilterFactory;
        [Inject] private VfxPaginationLoader.Factory _vfxFactory;
        [Inject] protected TemplateCameraAnimationSpeedUpdater TemplateCameraAnimationSpeedUpdater;
        [Inject] protected ICameraTemplatesManager CameraTemplatesManager;
        [Inject] protected AmplitudeAssetEventLogger AmplitudeAssetEventLogger;
        [Inject] protected StopWatchProvider StopWatchProvider;
        
        protected SpawnPositionAssetSelector SpawnPositionAssetSelector;
        protected SetLocationAssetSelector SetLocationAssetSelector;
        protected BodyAnimationAssetSelector BodyAnimationsAssetSelector;
        protected CameraFilterVariantAssetSelector CameraFilterVariantAssetSelector;
        protected CameraFilterAssetSelector CameraFilterAssetSelector;
        protected AssetSelectorsHolder CameraFiltersHolder;
        protected VfxAssetSelector VFXAssetSelector;
        protected VoiceFilterAssetSelector VoiceFilterAssetSelector;
        protected CameraAssetSelector CameraAssetSelector;
        protected OutfitAssetSelector OutfitAssetSelector;
        protected Stopwatch StopWatch;
        protected abstract BaseEditorPageModel PageModel { get; }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected MetadataStartPack MetaData => _dataFetcher.MetadataStartPack;

        protected Universe Universe => PageModel.Universe;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void SetupSelectedItems(Event targetEvent, bool silent = true)
        {
            RefreshSelectedSetLocation(targetEvent, silent);
            RefreshSelectedOutfit(targetEvent, silent);
            RefreshSelectedCameraFilters(targetEvent, silent);
            RefreshSelectedVfx(targetEvent, silent);
            RefreshSelectedVoiceFilter(targetEvent, silent);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            LevelManager.AssetUpdateStarted += OnAssetUpdatingBegan;
            LevelManager.AssetUpdateCompleted += OnAssetChanged;
            LevelManager.CharacterSpawned += OnCharacterSpawned;
            LevelManager.CharacterDestroyed += ResetBodyAnimationListCache;
        }

        protected virtual void OnDestroy()
        {
            LevelManager.AssetUpdateStarted -= OnAssetUpdatingBegan;
            LevelManager.AssetUpdateCompleted -= OnAssetChanged;
            LevelManager.CharacterSpawned -= OnCharacterSpawned;
            LevelManager.CharacterDestroyed -= ResetBodyAnimationListCache;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void RefreshSelectedCameraAnimationTemplate(Event @event, bool silent = true)
        {
            CameraAssetSelector.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
            TemplateCameraAnimationSpeedUpdater.UpdateSpeedUIStateWithEvent(@event);
        }

        protected void RefreshSelectedBodyAnimation(Event @event, bool silent = true)
        {
            BodyAnimationsAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
        }

        protected void RefreshSelectedOutfit(Event @event, bool silent = true)
        {
            OutfitAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
        }

        protected ICategory[] GetVoiceFilterCategories()
        {
            return _dataFetcher.MetadataStartPack.VoiceFilterCategories
                        .OrderBy(category => category.SortOrder).Cast<ICategory>().ToArray();
        }
        
        protected void SetupSetLocations(Action<AssetSelectionItemModel> onSetLocationSelected, bool allowPhoto = true, bool allowVideo = true, long? taskId = null, IList<long> categoryIds = null, bool showMyAssetsCategory = true)
        {
            var categories = _dataFetcher.MetadataStartPack.SetLocationCategories
                                         .Where(category => categoryIds.IsNullOrEmpty() || categoryIds.Contains(category.Id))
                                         .OrderBy(category => category.SortOrder).Cast<ICategory>().ToArray();
            
            var myAssetsLoader = _setLocationFactory.CreateMyAssetsLoader(() => SetLocationAssetSelector, Universe.Id ,null, taskId);
            myAssetsLoader.OnPageLoadedEvent += UpdateSpawnPointsView;
            SetLocationAssetSelector = new SetLocationAssetSelector(new PaginatedAssetSelectorParameters<SetLocationPaginationLoader, SetLocationFullInfo> {
                DisplayName = "Locations", 
                Categories = categories, 
                LoaderCreator = categoryId =>
                {
                    var loader = _setLocationFactory.CreateCategoryLoader(categoryId, () => SetLocationAssetSelector, Universe.Id ,taskId);
                    loader.OnPageLoadedEvent += UpdateSpawnPointsView;
                    return loader;
                }, 
                SearchLoaderCreator = filter =>
                {
                    var loader = _setLocationFactory.CreateSearchLoader(filter, () => SetLocationAssetSelector, Universe.Id,null, taskId);
                    loader.OnPageLoadedEvent += UpdateSpawnPointsView;
                    return loader;
                }, 
                MyAssetsLoader = myAssetsLoader,
                ShowMyAssetsCategory = showMyAssetsCategory
            }, allowPhoto, allowVideo);
            SetLocationAssetSelector.OnSelectedItemChangedEvent += onSetLocationSelected;
            SetLocationAssetSelector.SubSelector = SpawnPositionAssetSelector;
        }
       
        protected void SetupSpawnPointsView(Action<AssetSelectionItemModel> onSpawnPointSelected)
        {
            SpawnPositionAssetSelector = new SpawnPositionAssetSelector("Spawn Points");
            SpawnPositionAssetSelector.OnSelectedItemChangedEvent += onSpawnPointSelected;
        }

        protected void UpdateSpawnPointsView(ICollection<SetLocationFullInfo> setLocations)
        {
            //initially selected items need to be collected from event.
            const int categoryId = 1;

            if (setLocations.Count == 0)
            {
                return;
            }
            
            var selectableItems = new List<AssetSelectionSpawnPositionModel>();
            var index = 0;

            foreach (var location in setLocations)
            {
                foreach (var spawnPosition in location.GetSpawnPositions())
                {
                    var newSelectableItem = new AssetSelectionSpawnPositionModel(index++, Resolution._128x128,
                        spawnPosition, categoryId, location.Id, location.Name);
                    selectableItems.Add(newSelectableItem);
                }
            }
            
            SpawnPositionAssetSelector.AddItems(selectableItems, true);
        }

        protected void SetupBodyAnimationsView(Action<AssetSelectionItemModel> onSelected, long? stickToCategoryId = null, bool isPoses = false, long? taskId = null)
        {
            var categories = GetBodyAnimationCategories();
            BodyAnimationsAssetSelector = new BodyAnimationAssetSelector(new PaginatedAssetSelectorParameters<BodyAnimationPaginationLoader, BodyAnimationInfo> 
            {
                DisplayName = "Animations", 
                Categories = categories, 
                LoaderCreator = categoryId => _bodyAnimationFactory.CreateCategoryLoader(categoryId, () => BodyAnimationsAssetSelector, Universe.Id, taskId),
                SearchLoaderCreator = filter => _bodyAnimationFactory.CreateSearchLoader(filter, () => BodyAnimationsAssetSelector, Universe.Id, stickToCategoryId, taskId), 
                MyAssetsLoader = _bodyAnimationFactory.CreateMyAssetsLoader(() => BodyAnimationsAssetSelector, Universe.Id, stickToCategoryId, taskId),
                RecommendedLoader = _bodyAnimationFactory.CreateRecommendedLoader(() => BodyAnimationsAssetSelector, Universe.Id, stickToCategoryId, taskId),
                ShowMyAssetsCategory = !isPoses,
                ShowRecommendedCategory = true
            }, !isPoses);
            BodyAnimationsAssetSelector.OnSelectedItemChangedEvent += onSelected;
            BodyAnimationsAssetSelector.OnUpdateRequestValues += UpdateRequestValues;
        }

        protected abstract ICategory[] GetBodyAnimationCategories();
        
        private void UpdateRequestValues()
        {
            BodyAnimationsAssetSelector.CharacterCount = LevelManager.TargetEvent.GetCharactersCount();
            var genderId = LevelManager.TargetEvent.GetCharacters().First().GenderId;
            var race = MetaData.GetRaceByGenderId(genderId);
            BodyAnimationsAssetSelector.RaceId = race.Id;
            
            var spawnPosition = LevelManager.TargetEvent.GetTargetSpawnPosition();
            var allMovementTypes = spawnPosition.GetAllSupportedMovementTypes();
            
            BodyAnimationsAssetSelector.MainMovementTypeId = spawnPosition.MovementTypeId;
            BodyAnimationsAssetSelector.AllMovementTypeIds = allMovementTypes;
        }

        protected void SetupCameraFiltersVariants(Action<AssetSelectionItemModel> onFilterVariantSelected)
        {
            CameraFilterVariantAssetSelector = new CameraFilterVariantAssetSelector("Filter Variants");
            CameraFilterVariantAssetSelector.OnSelectedItemChangedEvent += onFilterVariantSelected;
        }

        protected void UpdateCameraFiltersVariants(IEnumerable<CameraFilterInfo> cameraFilters)
        {
            const int categoryId = 1;
            var selectableItems = new List<AssetSelectionCameraFilterVariantModel>();
            var index = 0;

            foreach (var filter in cameraFilters)
            {
                foreach (var variant in filter.CameraFilterVariants)
                {
                    var newSelectableItem = new AssetSelectionCameraFilterVariantModel(index++, Resolution._128x128,
                        variant, categoryId, filter, filter.Name);
                    selectableItems.Add(newSelectableItem);
                }
            }
            
            CameraFilterVariantAssetSelector.AddItems(selectableItems, true);
        }
        
        protected void SetupCameraFilters(Action<AssetSelectionItemModel> onCameraFilterSelected, long? taskId = null, IList<long> categoryIds = null)
        {
            var categories = _dataFetcher.MetadataStartPack.CameraFilterCategories
                                         .Where(category => categoryIds == null || categoryIds.Contains(category.Id))
                                         .OrderBy(category => category.SortOrder).Cast<ICategory>().ToArray();

            var myAssetsLoader = _cameraFilterFactory.CreateMyAssetsLoader(() => CameraFilterAssetSelector, Universe.Id,null, taskId);
            myAssetsLoader.OnPageLoadedEvent += UpdateCameraFiltersVariants;
            CameraFilterAssetSelector = new CameraFilterAssetSelector(new PaginatedAssetSelectorParameters<CameraFilterPaginationLoader, CameraFilterInfo> 
            {
                DisplayName = "Camera Filters", 
                Categories = categories, 
                LoaderCreator = categoryId =>
                {
                    var loader = _cameraFilterFactory.CreateCategoryLoader(categoryId, () => CameraFilterAssetSelector, Universe.Id, taskId);
                    loader.OnPageLoadedEvent += UpdateCameraFiltersVariants;
                    return loader;
                },
                SearchLoaderCreator = filter =>
                {
                    var loader = _cameraFilterFactory.CreateSearchLoader(filter, () => CameraFilterAssetSelector, Universe.Id, null, taskId);
                    loader.OnPageLoadedEvent += UpdateCameraFiltersVariants;
                    return loader;
                }, 
                MyAssetsLoader = myAssetsLoader
            });
            CameraFilterAssetSelector.OnSelectedItemChangedEvent += onCameraFilterSelected;
            CameraFiltersHolder = new AssetSelectorsHolder(new MainAssetSelectorModel[] { CameraFilterAssetSelector });
            CameraFilterAssetSelector.SubSelector = CameraFilterVariantAssetSelector;
        }
        
        protected void SetupVfxView(Action<AssetSelectionItemModel> onVfxSelected, long? taskId = null, IList<long> categoryIds = null)
        {
            var categories = _dataFetcher.MetadataStartPack.VfxCategories
                                         .Where(category => categoryIds.IsNullOrEmpty() || categoryIds.Contains(category.Id))
                                         .OrderBy(category => category.SortOrder).Cast<ICategory>().ToArray();
            VFXAssetSelector = new VfxAssetSelector(new PaginatedAssetSelectorParameters<VfxPaginationLoader, VfxInfo>
            {
                DisplayName = "VFX", 
                Categories = categories, 
                LoaderCreator = categoryId => _vfxFactory.CreateCategoryLoader(categoryId, () => VFXAssetSelector, Universe.Id, taskId), 
                SearchLoaderCreator = filter => _vfxFactory.CreateSearchLoader(filter, () => VFXAssetSelector, Universe.Id, null, taskId),
                MyAssetsLoader = _vfxFactory.CreateMyAssetsLoader(() => VFXAssetSelector, Universe.Id,null, taskId)
            }); 
            VFXAssetSelector.OnSelectedItemChangedEvent += onVfxSelected;
        }
        
        protected void OnCameraFilterAssetSelected(AssetSelectionItemModel cameraFilter)
        {
            if (cameraFilter.IsSelected)
            {
                CameraFilterVariantAssetSelector.SetCurrentCameraFilterId(cameraFilter.ItemId);

                if (cameraFilter.ItemId != CameraFilterVariantAssetSelector.AssetSelectionHandler.SelectedModels.FirstOrDefault()?.ParentAssetId)
                {
                    var filter = cameraFilter.RepresentedObject as CameraFilterInfo;
                    var defaultVariantId = filter.CameraFilterVariants.First().Id;
                    CameraFilterVariantAssetSelector.SetSelectedItems(new[] { defaultVariantId });
                }
                
                AmplitudeAssetEventLogger.LogSelectedCameraFilterAmplitudeEvent(cameraFilter.ItemId);
            }
            else
            {
                CameraFilterVariantAssetSelector.AssetSelectionHandler.UnselectAllSelectedItems();
                CameraFilterVariantAssetSelector.SetSelectedItems();
                CameraFilterVariantAssetSelector.ShowNoItems();
                CameraFilterVariantAssetSelector.SetCurrentCameraFilterId(0);
            }
        }
        
        protected void OnCameraFilterVariantSelected(AssetSelectionItemModel filterVariantModel)
        {
            var variant = filterVariantModel.RepresentedObject as CameraFilterVariantInfo;
            var cameraFilter = ((AssetSelectionCameraFilterVariantModel)filterVariantModel).CameraFilter;
            CameraFilterVariantAssetSelector.LockItems();

            if (filterVariantModel.IsSelected)
            {
                LevelManager.ChangeCameraFilter(cameraFilter, variant.Id, OnCompleted);
                AmplitudeAssetEventLogger.LogSelectedCameraFilterVariantAmplitudeEvent(filterVariantModel.ItemId);
                
                if (!CameraFilterAssetSelector.AssetSelectionHandler.IsItemAlreadySelected(filterVariantModel.ParentAssetId, filterVariantModel.CategoryId))
                {
                    var parentAsset = CameraFilterAssetSelector.GetItemsToShow().FirstOrDefault(item => item.ItemId == filterVariantModel.ParentAssetId);
                    parentAsset?.SetIsSelected(true);
                }
            }
            else
            {
                LevelManager.RemoveCameraFilter(()=>OnCompleted(null));
            }
            
            void OnCompleted(IAsset asset)
            {
                CameraFilterVariantAssetSelector.UnlockItems();
            }
        }

        protected void SetupSelectorScrollPositions(AssetSelectionViewManager manager)
        {
            manager.SetSelectedItemScrollPositions(BodyAnimationsAssetSelector);
            manager.SetSelectedItemScrollPositions(VFXAssetSelector);
            manager.SetSelectedItemScrollPositions(VoiceFilterAssetSelector);
            manager.SetSelectedItemScrollPositions(SetLocationAssetSelector);
            manager.SetSelectedItemScrollPositions(CameraAssetSelector);
            manager.SetSelectedItemScrollPositions(OutfitAssetSelector);
            manager.SetSelectedItemScrollPositions(CameraFilterAssetSelector);
        }
        
        protected void OnTargetCharacterSequenceNumberChanged()
        {
            RefreshSelectedBodyAnimation(LevelManager.TargetEvent);
            RefreshSelectedOutfit(LevelManager.TargetEvent);
            OnFocusTargetChanged();
        }
        
        protected void OnCharactersSwapped()
        {
            RefreshSelectedBodyAnimation(LevelManager.TargetEvent);
        }
        
        protected void DisableNotSuitableCategoriesForTargetSpawnPoint()
        {
            var spawnPosition = LevelManager.TargetEvent.GetTargetSpawnPosition();
            var allMovementTypes = spawnPosition.GetAllSupportedMovementTypes();
            var characterCount = LevelManager.TargetEvent.GetCharactersCount();
            var bodyAnimCategoriesIds = GetBodyAnimationCategories().Cast<BodyAnimationCategory>()
                                                                    .Where(category => category.CharacterCountMovementTypes.ContainsKey(characterCount) &&
                                                                               category.CharacterCountMovementTypes[characterCount].Any(moveTypeId => allMovementTypes.Contains(moveTypeId)))
                                                                    .Select(x=>x.Id).ToArray();
            foreach (var tabModel in BodyAnimationsAssetSelector.TabsManagerArgs.Tabs)
            {
                tabModel.Enabled = tabModel.Index == AssetCategoryTabsManagerArgs.RECOMMENDED_TAB_INDEX ||
                                   bodyAnimCategoriesIds.Contains(tabModel.Index);
            }
        }
        
        protected virtual void SetupCameraAnimationsView(CameraAnimationTemplate[] templates)
        {
            CameraTemplatesManager.SetupCameraAnimationTemplates(GetAllCameraTemplates().ToArray(), templates.First().Id);

            var allCategories = _dataFetcher.MetadataStartPack.CameraCategories;
            var categories = allCategories
                            .Where(x=> x.CameraAnimationTemplates.Any(templates.Contains))
                            .OrderBy(category => category.SortOrder).Cast<ICategory>().ToArray();

            CameraAssetSelector = new CameraAssetSelector("Camera", categories, MetaData, templates);
            CameraAssetSelector.OnSelectedItemChangedEvent += OnCameraAnimationSelected;
            CameraAssetSelector.OnSelectedItemSilentChangedEvent += OnCameraAnimationSelectedSilent;
            //CameraAssetSelector.OnInterruptDownload += InterruptDownload;
        }

        protected IEnumerable<CameraAnimationTemplate> GetAllCameraTemplates()
        {
            return _dataFetcher.MetadataStartPack.CameraCategories
                               .Where(category => category.CameraAnimationTemplates != null)
                               .SelectMany(category => category.CameraAnimationTemplates.Select(template => template))
                               .Distinct();
        }

        protected IEnumerable<CameraAnimationTemplate> GetCameraTemplates(long[] ids)
        {
            return GetAllCameraTemplates().Where(x => ids.Contains(x.Id));
        }

        protected void OnCameraAnimationSelectedSilent(AssetSelectionItemModel cameraAnimation)
        {
            if (!cameraAnimation.IsSelected) return;

            var cameraAnimationTemplate = cameraAnimation.RepresentedObject as CameraAnimationTemplate;
            OnChangeCameraAnimation(cameraAnimationTemplate);
        }
        
        protected void OnCameraAnimationSelected(AssetSelectionItemModel cameraAnimation)
        {
            if (!cameraAnimation.IsSelected) return;
            
            var cameraAnimationTemplate = cameraAnimation.RepresentedObject as CameraAnimationTemplate;
            OnChangeCameraAnimation(cameraAnimationTemplate, OnComplete);

            void OnComplete()
            {
                LevelManager.StopCameraAnimation();
                LevelManager.PlayTeaserClip(cameraAnimationTemplate.Id);
            }
        }
        
        protected void OnChangeCameraAnimation(CameraAnimationTemplate cameraAnimationTemplate, Action onComplete = null)
        {
            StopWatch = StopWatchProvider.GetStopWatch();
            StopWatch.Restart();
            
            LevelManager.Change(cameraAnimationTemplate, OnComplete);
            
            void OnComplete(IAsset asset)
            {
                var cameraController = LevelManager.TargetEvent.GetCameraController();
                if (cameraController != null)
                {
                    cameraController.CameraAnimationTemplateId = cameraAnimationTemplate.Id;
                }
                onComplete?.Invoke();
                LogSelectedCameraCameraTemplateAmplitudeEvent(cameraAnimationTemplate.Id);
            }
        }

        protected async void OnOutfitSelected(AssetSelectionItemModel outfitModel)
        {
            PageModel.OnOutfitChangingBegun();
            await ChangeOutfit(outfitModel);
        }

        protected void StopAndDisposeStopWatch()
        {
            StopWatch.Stop();
            StopWatchProvider.Dispose(StopWatch);
        }
        
        protected void ResetBodyAnimationListCache()
        {
            BodyAnimationsAssetSelector.ResetCache();
        }

        protected void RefreshSelectedSetLocation()
        {
            SetLocationAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, LevelManager.TargetEvent, _dataFetcher);
            SpawnPositionAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, LevelManager.TargetEvent, _dataFetcher);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RefreshSelectedVoiceFilter(Event @event, bool silent = true)
        {
            VoiceFilterAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, @event, silent: silent);
        }

        private void RefreshSelectedSetLocation(Event @event, bool silent = true)
        {
            if (SetLocationAssetSelector != null)
            {
                SetLocationAssetSelector.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
            }
            
            SpawnPositionAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
        }
        
        private void RefreshSelectedVfx(Event @event, bool silent = true)
        {
            VFXAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
        }
        
        private void RefreshSelectedCameraFilters(Event @event, bool silent = true)
        {
            if (CameraFilterAssetSelector != null)
            {
                CameraFilterAssetSelector.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
            }
            
            CameraFilterVariantAssetSelector?.SetSelectedItemsAsInEvent(LevelManager, @event, _dataFetcher, silent);
        }
        
        private void OnFocusTargetChanged()
        {
            RefreshSelectedOutfit(LevelManager.TargetEvent);
        }

        protected BodyAnimationCategory GetBodyAnimationCategory(long? stickToCategoryId)
        {
            return GetAllBodyAnimationCategories().First(x=>x.Id == stickToCategoryId);
        }

        protected IEnumerable<BodyAnimationCategory> GetAllBodyAnimationCategories()
        {
            return _dataFetcher.MetadataStartPack.BodyAnimationCategories
                               .OrderBy(category => category.SortOrder);
        }
        
        private void LogSelectedCameraCameraTemplateAmplitudeEvent(long id)
        {
            if (StopWatch == null) return;
            
            AmplitudeAssetEventLogger.LogSelectedCameraTemplateAmplitudeEvent(id, StopWatch.ElapsedMilliseconds.ToSecondsClamped());
            StopAndDisposeStopWatch();
        }
        
        private async Task ChangeOutfit(AssetSelectionItemModel outfitModel)
        {
            OutfitAssetSelector.LockItems(); 
            
            if (outfitModel.IsSelected)
            {
                var outfitFullInfo = await OutfitsManager.GetFullOutfit(outfitModel.RepresentedObject as OutfitShortInfo);
                await LevelManager.ChangeOutfit(outfitFullInfo);
            }
            else
            {
                await LevelManager.RemoveOutfit();
            }
            
            outfitModel.ApplyModel();
            OutfitAssetSelector.UnlockItems();
        }
        
        private void OnAssetChanged(DbModelType assetType)
        {
            if (assetType == DbModelType.BodyAnimation 
             && BodyAnimationsAssetSelector.AssetSelectionHandler.SelectedModels.FirstOrDefault()?.ItemId 
                != BodyAnimationsAssetSelector.GetCurrentBodyAnimationId(LevelManager, LevelManager.TargetEvent))
            {
                RefreshSelectedBodyAnimation(LevelManager.TargetEvent);
            }

            if (assetType == DbModelType.SetLocation && !LevelManager.IsLoadingAssetsOfType(DbModelType.SetLocation))
            {
                SpawnPositionAssetSelector.UnlockItems();
            }
        }

        private void OnCharacterSpawned(ICharacterAsset characterAsset)
        {
            ResetBodyAnimationListCache();
        }
        
        private void OnAssetUpdatingBegan(DbModelType assetType, long id)
        {
            switch (assetType)
            {
                case DbModelType.SetLocation:
                    SpawnPositionAssetSelector.LockItems();
                    return;
                default: return;
            }
        }
    }
}