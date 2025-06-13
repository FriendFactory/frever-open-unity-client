using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Common;
using Extensions;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.RecordingModeSelection;
using Modules.LevelViewPort;
using Navigation.Args;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.EditingPage;
using UIManaging.Pages.LevelEditor.Ui.AssetButtons;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using UIManaging.Pages.LevelEditor.Ui.AssetUIManagers;
using UIManaging.Pages.LevelEditor.Ui.Characters;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.LevelEditor.Ui.Preview;
using UIManaging.Pages.LevelEditor.Ui.RecordingButton;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UIManaging.Pages.LevelEditor.Ui.SwitchingEditors;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class LevelEditorPageUI : LevelEditorUiBase
    {
        [SerializeField] private LevelEditorAssetSelectionManager assetSelectionViewManager;
        [SerializeField] private DeleteLastEventUIManager _deleteLastEventUiManager;
        [SerializeField] private EditingPageLoading _pageLoading;
        [SerializeField] private CharactersControlPanel _charactersControlPanel;
        [SerializeField] private CharacterOverlaysPanel _characterOverlaysPanel;
        [SerializeField] private EventRecordingUIManager _eventRecordingUIManager;
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private GameObject _levelEditorUI;
        [SerializeField] private PreviewButtonsHolder _previewButtonsHolder;
        [SerializeField] private CameraAnimationSpeedAssetView _cameraAnimationSpeedAssetView;
        [SerializeField] private UploadingPanel _uploadingPanel;
        [SerializeField] private LastEventPreviewButton _lastEventPreviewButton;
        [SerializeField] private SwitchEditorOptionsPanel _switchEditorOptionsPanel;
        [SerializeField] private Button _openSwitchSelectionButton;
        [SerializeField] private LevelViewPort _levelViewPort;
        [SerializeField] private RectTransform _buttonsContainer;
        
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private CameraAnimationGenerator _cameraAnimationGenerator;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private ICharactersUIManager _charactersUIManager;
        [Inject] private ISetLocationSelectionFeatureControl _setLocationFeatureControl;
        [Inject] private IVoiceFilterFeatureControl _voiceFilterFeatureControl;
        [Inject] private AudioSourceManager _audioSourceManager;
        [Inject] private IOutfitFeatureControl _outfitFeatureControl;
        [Inject] private ICameraAnimationFeatureControl _cameraAnimationFeatureControl;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private MusicSelectionPageModel _musicSelectionPageModel;

        private LevelEditorArgs _args;
        private AssetSelectorsHolder _vfxsHolder;
        private SetLocationAssetSelectionHolder _setLocationsHolder;
        private BodyAnimationAssetSelectionHolder _bodyAnimationsHolder;
        private AssetCameraSelectorsHolder _cameraAnimationsHolder;
        private AssetSelectorsHolder _voiceFilterHolder;
        private BaseCharactersUIHandler _addCharactersUiHandler;
        private AssetSelectorsHolder _outfitHolder;
        private AssetSelectorModel[] _assetsSelectionModels;

        private bool _isNewLevel;
        private bool _isMusicPanelOpen;

        protected override BaseEditorPageModel PageModel => _levelEditorPageModel;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            LevelManager.TargetCharacterSequenceNumberChanged += OnTargetCharacterSequenceNumberChanged;
            LevelManager.EditingCharacterSequenceNumberChanged += OnEditingCharacterSequenceNumberChanged;
            LevelManager.CharactersPositionsSwapped += OnCharactersSwapped;

            _levelEditorPageModel.SetupSongSelection(_musicSelectionPageModel);
            _levelEditorPageModel.BodyAnimationsButtonClicked += OnBodyAnimationsButtonClicked;
            _levelEditorPageModel.VfxButtonClicked += OnVfxButtonClicked;
            _levelEditorPageModel.SetLocationsButtonClicked += OnSetLocationsButtonClicked;
            _levelEditorPageModel.CameraButtonClicked += OnCameraButtonClicked;
            _levelEditorPageModel.VoiceButtonClicked += OnVoiceFilterClicked;
            _levelEditorPageModel.OutfitPanelOpened += OnOutfitPanelOpened;
            _levelEditorPageModel.ShowLoadingIconOverlay += ShowLoadingOverlay;
            _levelEditorPageModel.HideLoadingIconOverlay += HideLoadingOverlay;
            _levelEditorPageModel.ExitRequested += OnExit;
            
            LevelManager.LevelPreviewStarted += OnEventPreviewStarted;
            LevelManager.LevelPiecePlayingCompleted += OnPreviewOnePiecePlayed;
            LevelManager.NextLevelPiecePlayingStarted += OnNextPreviewPieceStarted;
            LevelManager.LevelPreviewCompleted += DisplayLevelEditorUi;
            LevelManager.EventPreviewCompleted += DisplayLevelEditorUi;
            LevelManager.PreviewCancelled += DisplayLevelEditorUi;
            LevelManager.TemplateApplyingCompleted += OnTemplateApplyingFinished;
            LevelManager.RequestPlayerCenterFaceStarted += RequestPlayerCenterFaceStarted;
            LevelManager.RequestPlayerCenterFaceFinished += RequestPlayerCenterFaceFinished;
            LevelManager.RequestPlayerNeedsBetterLightingStarted += RequestPlayerNeedsBetterLightingStarted;
            LevelManager.RequestPlayerNeedsBetterLightingFinished += RequestPlayerNeedsBetterLightingFinished;
            LevelManager.ShufflingDone += OnShufflingCompleted;
            LevelManager.SetLocationChangeFinished += _uploadingPanel.OnSetLocationChanged;

            _musicSelectionPageModel.PageOpened += OnSongSelectionOpened;
            _musicSelectionPageModel.PageCloseRequested += OnSongSelectionCloseRequested;
            _openSwitchSelectionButton.onClick.AddListener(_switchEditorOptionsPanel.Show);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelManager.TargetCharacterSequenceNumberChanged -= OnTargetCharacterSequenceNumberChanged;
            LevelManager.EditingCharacterSequenceNumberChanged -= OnEditingCharacterSequenceNumberChanged;
            LevelManager.CharactersPositionsSwapped -= OnCharactersSwapped;

            _levelEditorPageModel.BodyAnimationsButtonClicked -= OnBodyAnimationsButtonClicked;
            _levelEditorPageModel.VfxButtonClicked -= OnVfxButtonClicked;
            _levelEditorPageModel.SetLocationsButtonClicked -= OnSetLocationsButtonClicked;
            _levelEditorPageModel.CameraButtonClicked -= OnCameraButtonClicked;
            _levelEditorPageModel.VoiceButtonClicked -= OnVoiceFilterClicked;
            _levelEditorPageModel.OutfitPanelOpened -= OnOutfitPanelOpened;
            _levelEditorPageModel.ShowLoadingIconOverlay -= ShowLoadingOverlay;
            _levelEditorPageModel.HideLoadingIconOverlay -= HideLoadingOverlay;
            _levelEditorPageModel.ExitRequested -= OnExit;
            
            LevelManager.LevelPreviewStarted -= OnEventPreviewStarted;
            LevelManager.LevelPiecePlayingCompleted -= OnPreviewOnePiecePlayed;
            LevelManager.NextLevelPiecePlayingStarted -= OnNextPreviewPieceStarted;
            LevelManager.LevelPreviewCompleted -= DisplayLevelEditorUi;
            LevelManager.EventPreviewCompleted -= DisplayLevelEditorUi;
            LevelManager.PreviewCancelled -= DisplayLevelEditorUi;
            LevelManager.TemplateApplyingCompleted -= OnTemplateApplyingFinished;
            BodyAnimationsAssetSelector.OnSelectedItemChangedEvent -= OnBodyAnimationSelected;
            VFXAssetSelector.OnSelectedItemChangedEvent -= OnVfxSelected;
            VoiceFilterAssetSelector.OnSelectedItemChangedEvent -= OnVoiceFilterSelected;
            SetLocationAssetSelector.OnSelectedItemChangedEvent -= OnSetLocationSelected;
            SpawnPositionAssetSelector.OnSelectedItemChangedEvent -= OnSpawnPointSelected;
            OutfitAssetSelector.OnSelectedItemChangedEvent -= OnOutfitSelected;
            CameraFilterAssetSelector.OnSelectedItemChangedEvent -= OnCameraFilterAssetSelected;
            CameraFilterVariantAssetSelector.OnSelectedItemChangedEvent -= OnCameraFilterVariantSelected;
            UnSubscribeCameraAssetSelectionManagerModel();
            LevelManager.RequestPlayerCenterFaceStarted -= RequestPlayerCenterFaceStarted;
            LevelManager.RequestPlayerCenterFaceFinished -= RequestPlayerCenterFaceFinished;
            LevelManager.RequestPlayerNeedsBetterLightingStarted -= RequestPlayerNeedsBetterLightingStarted;
            LevelManager.RequestPlayerNeedsBetterLightingFinished -= RequestPlayerNeedsBetterLightingFinished;  
            LevelManager.SetLocationChangeFinished -= _uploadingPanel.OnSetLocationChanged;
            LevelManager.ShufflingDone -= OnShufflingCompleted;
            
            _musicSelectionPageModel.PageOpened -= OnSongSelectionOpened;
            _musicSelectionPageModel.PageCloseRequested -= OnSongSelectionCloseRequested;
            _openSwitchSelectionButton.onClick.RemoveListener(_switchEditorOptionsPanel.Show);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnDisplayStart(LevelEditorArgs args)
        {
            _args = args;
            LevelManager.EventStarted += OnEditorLoaded;
            SetupCharacterUi();
            SetupCharactersOverlay();

            _cameraSystem.Initialize();
            TemplateCameraAnimationSpeedUpdater.Initialize(_cameraAnimationSpeedAssetView);

            HideLoadingOverlay();
            ConfigureAndShowLoadingPage(args);

            _isNewLevel = args.LevelData == null || !args.LevelData.Event.Any();

            _deleteLastEventUiManager.Initialize();

            _eventRecordingUIManager.Initialize();
            var cameraTemplates = _cameraAnimationFeatureControl.AllowToUseAllTemplates
                ? GetAllCameraTemplates()
                : GetCameraTemplates(_cameraAnimationFeatureControl.TemplateIds);
            SetupCameraAnimationsView(cameraTemplates.ToArray());

            var vfxCategories = args.Settings.VfxSettings.Categories;
            SetupVfxView(OnVfxSelected, args.TaskId, vfxCategories);
            
            SetupVoiceFilters();
            
            SetupBodyAnimationsView(OnBodyAnimationSelected, taskId:_args.TaskId);
            SetupSpawnPointsView(OnSpawnPointSelected);

            var setLocationCategories = args.Settings.SetLocationSettings.Categories;
            SetupSetLocations(OnSetLocationSelected, _setLocationFeatureControl.AllowPhoto, _setLocationFeatureControl.AllowVideo, categoryIds: setLocationCategories, taskId:_args.TaskId);
            var anyUsedGender = _isNewLevel? args.ReplaceCharactersData.First().ReplaceByCharacter.GenderId : args.LevelData.GetFirstEvent().GetCharacters().First().GenderId;//important only race of the gender, that is why we can pick any
            SetupOutfits(anyUsedGender);
            SetupCameraFiltersVariants(OnCameraFilterVariantSelected);
            SetupCameraFilters(OnCameraFilterAssetSelected);
            SetupAssetSelectionModelsList();

            _vfxsHolder = new AssetSelectorsHolder(new MainAssetSelectorModel[] {VFXAssetSelector});
            _cameraAnimationsHolder = new AssetCameraSelectorsHolder(new[] {CameraAssetSelector}, _cameraAnimationGenerator);
            _bodyAnimationsHolder = new BodyAnimationAssetSelectionHolder(new[] {BodyAnimationsAssetSelector});
            _setLocationsHolder = new SetLocationAssetSelectionHolder(new MainAssetSelectorModel[] {SetLocationAssetSelector});
            _voiceFilterHolder = new AssetSelectorsHolder(new[] {VoiceFilterAssetSelector});

            LevelManager.EventDeleted += OnEventDeleted;
            LevelManager.EventDeletionStarted += LockAssetSelectionItems;
            LevelManager.AssetUpdateFailed += OnAssetUpdatingFailed;
            LevelManager.EventStarted += SetInitialSelectedItems;

            _previewButtonsHolder.Init(true);
            _lastEventPreviewButton.Init();
            _switchEditorOptionsPanel.Init();

            _openSwitchSelectionButton.SetActive(false);
            _levelViewPort.Init();
            _buttonsContainer.position = _levelViewPort.transform.position;
        }

        public void OnHidingBegin()
        {
            DetachRenderTextureFromCamera();
            _addCharactersUiHandler.CleanUp();
            _deleteLastEventUiManager.CleanUp();
            _lastEventPreviewButton.CleanUp();
            TemplateCameraAnimationSpeedUpdater.CleanUp();
            _characterOverlaysPanel.CleanUp();
            _charactersControlPanel.CleanUp();

            LevelManager.EventStarted -= OnEditorLoaded;
            LevelManager.EventStarted -= SetInitialSelectedItems;
            LevelManager.EventDeletionStarted -= LockAssetSelectionItems;
            LevelManager.EventDeleted -= OnEventDeleted;
            _charactersControlPanel.OpenCharactersViewButtonClicked -= DisplayAddCharactersView;
            _charactersUIManager.OnDisplayCharacterSelector -= ShowAssetSelectionManagerView;
            LevelManager.AssetUpdateFailed -= OnAssetUpdatingFailed;

            HideAssetLoadingSnackBarIfOpened();
            RequestPlayerCenterFaceFinished();
        }

        private void DetachRenderTextureFromCamera()
        {
            var cam = LevelManager.GetCurrentEventCamera();
            if (cam == null) return;
            cam.targetTexture = null;
        }

        public void SetupDefaultRecordingMode()
        {
            LevelManager.ChangeRecordingMode(RecordingMode.Story);
        }

        public override void SetupSelectedItems(Event targetEvent, bool silent = true)
        {
            base.SetupSelectedItems(targetEvent, silent);
         
            RefreshSelectedBodyAnimation(targetEvent, silent);
            RefreshSelectedCameraAnimationTemplate(targetEvent, silent);
            SetRenderTexture();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override ICategory[] GetBodyAnimationCategories()
        {
            var categoryIds = _args.Settings.BodyAnimationSettings.Categories;
            return GetAllBodyAnimationCategories().Where(x => categoryIds.IsNullOrEmpty() || categoryIds.Contains(x.Id)).Cast<ICategory>().ToArray();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetupSelectorScrollPositions()
        {
            SetupSelectorScrollPositions(assetSelectionViewManager);
        }

        private void ConfigureAndShowLoadingPage(LevelEditorArgs args)
        {
            // Check whether it is a "Back" navigation
            if (string.IsNullOrEmpty(args.NavigationMessage)) return; 
            
            _pageLoading.Configure(Constants.NavigationMessages.DEFAULT_LEVEL_EDITOR_MESSAGE);
            _pageLoading.ShowDark(args.NavigationMessage, true ,args.CancelLoadingAction);
        }

        private void OnEditingCharacterSequenceNumberChanged()
        {
            RefreshSelectedBodyAnimation(LevelManager.TargetEvent);
            RefreshSelectedOutfit(LevelManager.TargetEvent);
        }

        private void OnEditorLoaded()
        {
            LevelManager.EventStarted -= OnEditorLoaded;

            HidePageLoadingOverlay();          
            
            SetupSelectedItems(LevelManager.TargetEvent);

            var selectInitialAssets = !_isNewLevel
                                   || LevelManager.CurrentLevel.RemixedFromLevelId != null
                                   || LevelManager.TargetEvent.TemplateId != null;
            
            if (selectInitialAssets)
            {
                SetupSelectorScrollPositions();
            }

            if (_args.ShowTaskInfo)
            {
                _popupManagerHelper.ShowTaskInfoPopup(_args.TaskFullInfo);
            }
            
            _uploadingPanel.RefreshState(false);
            SetRenderTexture();
        }

        private void OnEventPreviewStarted()
        {
            SetRenderTexture();
            _levelEditorPageModel.ChangeState(LevelEditorState.Preview);
        }

        private void DisplayLevelEditorUi()
        {
            _levelEditorPageModel.ChangeState(LevelEditorState.Default);
        }
        
        private void SetupVoiceFilters()
        {
            VoiceFilterAssetSelector = new VoiceFilterAssetSelector("Voice filters", GetVoiceFilterCategories(), MetaData, _audioSourceManager, () => _voiceFilterFeatureControl.IsFeatureEnabled);
            VoiceFilterAssetSelector.OnSelectedItemChangedEvent += OnVoiceFilterSelected;
        }

        private async void SetupOutfits(long genderId)
        {
            OutfitAssetSelector = new OutfitAssetSelector("Outfit", _outfitFeatureControl.CreationOutfitAllowed);
            var outfits = await OutfitsManager.GetOutfitShortInfoList(500, 0, SaveOutfitMethod.Manual, genderId);
            var autoSavedOutfits = await OutfitsManager.GetOutfitShortInfoList(500, 0, SaveOutfitMethod.Automatic, genderId);
            outfits = outfits.Concat(autoSavedOutfits);
            var index = 0;
            var selectableItems = outfits.Select(x => new AssetSelectionOutfitModel(index++, Resolution._128x128, x)).ToArray();           
            OutfitAssetSelector.OnSelectedItemChangedEvent += OnOutfitSelected;
            OutfitAssetSelector.AddItems(selectableItems, true);
            
            _outfitHolder = new AssetSelectorsHolder(new[] {OutfitAssetSelector});
        }

        private void OnBodyAnimationSelected(AssetSelectionItemModel bodyAnimation)
        {
            if (!bodyAnimation.IsSelected) return;

            StopWatch = StopWatchProvider.GetStopWatch();
            StopWatch.Restart();
            
            LevelManager.ChangeBodyAnimation(bodyAnimation.RepresentedObject as BodyAnimationInfo, OnBodyAnimationChanged);

            void OnBodyAnimationChanged()
            {
                if(!_levelEditorPageModel.IsPostRecordEditorOpened) _cameraSystem.RecenterFocus();
                LogSelectedBodyAnimationAmplitudeEvent(bodyAnimation.ItemId);

                var vfxBundle = VFXAssetSelector.AssetSelectionHandler
                                                .SelectedModels.FirstOrDefault( itemModel => itemModel.IsSelected && (itemModel.RepresentedObject as VfxInfo)?.BodyAnimationAndVfx != null);
                
                if (vfxBundle == null) return;
                
                vfxBundle.SetIsSelected(false);
            }
        }
        
        private void OnCameraAnimationSelectedPostRecording(AssetSelectionItemModel cameraAnimation)
        {
            if (!cameraAnimation.IsSelected) return;

            LevelManager.StopCameraAnimation();
            LevelManager.PutCameraOnFirstCameraAnimationFrame();
            
            var cameraAnimationTemplate = cameraAnimation.RepresentedObject as CameraAnimationTemplate;
            OnChangeCameraAnimation(cameraAnimationTemplate);
        }

        private void OnSpawnPointSelected(AssetSelectionItemModel characterSpawnPosition)
        {
            if (!characterSpawnPosition.IsSelected) return;

            AmplitudeAssetEventLogger.LogSelectedSpawnPositionAmplitudeEvent(characterSpawnPosition.ItemId);
            
            var setLocation = LevelManager.TargetEvent.GetSetLocation();
            var parentAsset = SetLocationAssetSelector.Models.FirstOrDefault(item => item.ItemId == characterSpawnPosition.ParentAssetId);

            if (!SetLocationAssetSelector.AssetSelectionHandler?.IsItemAlreadySelected(parentAsset.ItemId, parentAsset.CategoryId) ?? true)
            {
                parentAsset.SetIsSelected(true);
                return;
            }

            var isParentAssetLoaded = setLocation.Id == parentAsset.ItemId;
            if (!isParentAssetLoaded) return;

            var spawnPosition = setLocation.GetSpawnPositions()
                .First(spawnPos => spawnPos.Id == characterSpawnPosition.ItemId);

            LevelManager.ChangeCharacterSpawnPosition(spawnPosition, true);
        }

        private void OnVoiceFilterSelected(AssetSelectionItemModel voiceFilterItemModel)
        {
            if (!voiceFilterItemModel.IsSelected) return;

            var voiceFilter = voiceFilterItemModel.RepresentedObject as VoiceFilterFullInfo;
            LevelManager.Change(voiceFilter);
            _levelEditorPageModel.OnVoiceFilterClicked(voiceFilter);
            AmplitudeAssetEventLogger.LogSelectedVoiceFilterAmplitudeEvent(voiceFilter.Id);
        }

        private void OnVfxSelected(AssetSelectionItemModel vfxAnimation)
        {
            if (!vfxAnimation.IsSelected)
            {
                LevelManager.RemoveAllVfx();
            }
            else
            {
                VFXAssetSelector.LockItems();
                var vfxInfo = vfxAnimation.RepresentedObject as VfxInfo;
                if (vfxInfo?.BodyAnimationAndVfx != null)
                {
                    var bodyAnimation = vfxInfo.BodyAnimationAndVfx.BodyAnimation;
                    LevelManager.ChangeBodyAnimation(bodyAnimation, OnBodyAnimationChanged);
                }
                else
                {
                    LevelManager.Change(vfxAnimation.RepresentedObject as VfxInfo, OnCompleted);
                }
            }

            void OnCompleted(IAsset asset)
            {
                AmplitudeAssetEventLogger.LogSelectedVfxAmplitudeEvent(vfxAnimation.ItemId);
                
                VFXAssetSelector.UnlockItems();
            }
            
            void OnBodyAnimationChanged()
            {
                if(!_levelEditorPageModel.IsPostRecordEditorOpened) _cameraSystem.RecenterFocus();

                LevelManager.Change(vfxAnimation.RepresentedObject as VfxInfo, OnCompleted);
            }
        }
        
        private void OnSetLocationSelected(AssetSelectionItemModel setLocation)
        {
            if (!setLocation.IsSelected) return;
            HideAssetLoadingSnackBarIfOpened();
            ShowAssetLoadingSnackBar();
            
            var location = setLocation.RepresentedObject as SetLocationFullInfo;
            var selectedSpawnPositionModel = SpawnPositionAssetSelector.AssetSelectionHandler.SelectedModels.FirstOrDefault();
            
            StopWatch = StopWatchProvider.GetStopWatch();
            StopWatch.Restart();
            
            LevelManager.ChangeSetLocation(location, OnCompleted, OnCancelled, spawnPositionId:selectedSpawnPositionModel?.ItemId);

            void OnCompleted(ISetLocationAsset asset, DbModelType[] otherChangedAssetTypes)
            {
                HideAssetLoadingSnackBarIfOpened();
                SpawnPositionAssetSelector.SetCurrentSetLocationId(setLocation.ItemId);
                setLocation.ApplyModel();

                var spawnPositionId = LevelManager.TargetEvent.GetTargetSpawnPosition().Id;
                SpawnPositionAssetSelector.SetSelectedItems(new[] { spawnPositionId }, silent:true);

                //_uploadingPanel.RefreshState(true);
                SetRenderTexture();
                LogSelectedSetLocationAmplitudeEvent(asset);
                
                if (!otherChangedAssetTypes.Contains(DbModelType.BodyAnimation)) return;
                
                var vfxBundle = VFXAssetSelector.AssetSelectionHandler
                                                .SelectedModels.FirstOrDefault( itemModel => itemModel.IsSelected && (itemModel.RepresentedObject as VfxInfo)?.BodyAnimationAndVfx != null);
                
                vfxBundle?.SetIsSelected(false);
            }
            
            void OnCancelled()
            {
                HideAssetLoadingSnackBarIfOpened();
            }
        }

        private void OnEnable()
        {
            assetSelectionViewManager.Closed += OnAssetSelectionViewClosed;
            _levelEditorPageModel.CameraFilterClicked += OnCameraCameraFilterButtonClicked;
        }

        private void OnDisable()
        {
            assetSelectionViewManager.Closed -= OnAssetSelectionViewClosed;
            _levelEditorPageModel.CameraFilterClicked -= OnCameraCameraFilterButtonClicked;
        }

        private void OnAssetSelectionViewClosed()
        {
            _levelEditorPageModel.ReturnToPrevState();
        }

        private void OnBodyAnimationsButtonClicked()
        {
            DisableNotSuitableCategoriesForTargetSpawnPoint();

            ShowAssetSelectionManagerView(_bodyAnimationsHolder, true);
        }

        private void OnVfxButtonClicked()
        {
            ShowAssetSelectionManagerView(_vfxsHolder, true);
        }

        private void OnSetLocationsButtonClicked()
        {
            ShowAssetSelectionManagerView(_setLocationsHolder, true);
        }

        private void OnCameraButtonClicked()
        {
            ShowAssetSelectionManagerView(_cameraAnimationsHolder);
        }

        private void OnVoiceFilterClicked()
        {
            ShowAssetSelectionManagerView(_voiceFilterHolder);
        }

        private void OnOutfitPanelOpened()
        {
            RefreshSelectedOutfit(LevelManager.TargetEvent);

            if (_outfitHolder == null) return;
            ShowAssetSelectionManagerView(_outfitHolder);
        }

        private void OnCameraCameraFilterButtonClicked()
        {
            ShowAssetSelectionManagerView(CameraFiltersHolder, true);
        }
        
        private void ShowAssetSelectionManagerView(AssetSelectorsHolder assetSelectorsHolder)
        {
            ShowAssetSelectionManagerView(assetSelectorsHolder, false);
        }

        private void ShowAssetSelectionManagerView(AssetSelectorsHolder assetSelectorsHolder, bool isPurchasable)
        {
            var selectionState = isPurchasable 
                ? LevelEditorState.PurchasableAssetSelection 
                : LevelEditorState.AssetSelection;
            
            _levelEditorPageModel.ChangeState(selectionState);

            assetSelectionViewManager.gameObject.SetActive(true);
            assetSelectionViewManager.Initialize(assetSelectorsHolder);
        }

        private void LockAssetSelectionItems()
        {
            foreach (var model in _assetsSelectionModels)
            {
                model.LockItems();
            }  
        }
        
        private void OnEventDeleted()
        {
            UnlockAssetSelectionItems();
            SetupSelectedItems(LevelManager.TargetEvent);
        }

        private void UnlockAssetSelectionItems()
        {
            foreach (var model in _assetsSelectionModels)
            {
                model.UnlockItems();
            }
        }

        private void UnSubscribeCameraAssetSelectionManagerModel()
        {
            CameraAssetSelector.OnSelectedItemChangedEvent -= OnCameraAnimationSelected;
            CameraAssetSelector.OnSelectedItemSilentChangedEvent -= OnCameraAnimationSelectedSilent;
            CameraAssetSelector.OnSelectedItemChangedEvent -= OnCameraAnimationSelectedPostRecording;
        }

        private void ShowLoadingOverlay(string title)
        {
            _loadingOverlay.Show(title: title);
        }

        private void HideLoadingOverlay()
        {
            _loadingOverlay.Hide();
        }

        private void OnSongSelectionCloseRequested()
        {
            _levelEditorUI.SetActive(true);
            _isMusicPanelOpen = false;
        }

        private void OnSongSelectionOpened()
        {
            _levelEditorUI.SetActive(false);
            _isMusicPanelOpen = true;
            RequestPlayerCenterFaceFinished();
        }
        
        private void OnPreviewOnePiecePlayed()
        {
            _pageLoading.ShowDark();
        }
        
        private void OnNextPreviewPieceStarted()
        {
            HidePageLoadingOverlay();
        }

        private void ShowAssetLoadingSnackBar()
        {
            _snackBarHelper.ShowAssetLoadingSnackBar(float.PositiveInfinity);
        }
        
        private void HideAssetLoadingSnackBarIfOpened()
        {
            _snackBarHelper.HideSnackBar(SnackBarType.AssetLoading);
        }
        
        private void RequestPlayerCenterFaceStarted()
        {
            if (!_isMusicPanelOpen)
            {
                _snackBarHelper.ShowInformationSnackBar("Hold your phone steady and center your face.", float.PositiveInfinity);
            }
        }
        
        private void RequestPlayerCenterFaceFinished()
        {
            _snackBarHelper.HideSnackBar(SnackBarType.Information);
        } 
        
        private void RequestPlayerNeedsBetterLightingStarted()
        {
            if (!_isMusicPanelOpen)
            {
                _snackBarHelper.ShowInformationSnackBar("Please improve your room lighting, for best results have a light pointing toward your face.", float.PositiveInfinity);
            }
        }
        
        private void RequestPlayerNeedsBetterLightingFinished()
        {
            _snackBarHelper.HideSnackBar(SnackBarType.Information);
        }  
        
        private void LogSelectedBodyAnimationAmplitudeEvent(long id)
        {
            if (StopWatch == null) return;
            
            AmplitudeAssetEventLogger.LogSelectedBodyAnimationAmplitudeEvent(id, StopWatch.ElapsedMilliseconds.ToSecondsClamped());
            StopAndDisposeStopWatch();
        }
        private void LogSelectedSetLocationAmplitudeEvent(IAsset asset)
        {
            if (StopWatch == null) return;
            
            AmplitudeAssetEventLogger.LogSelectedSetLocationAmplitudeEvent(asset.Id, StopWatch.ElapsedMilliseconds.ToSecondsClamped());
            StopAndDisposeStopWatch();
        }

        private void SetupCharacterUi()
        {
            _charactersControlPanel.Initialize();
            _charactersUIManager.OnDisplayCharacterSelector += ShowAssetSelectionManagerView;
            _addCharactersUiHandler = _charactersUIManager.GetCharacterUIHandler(CharacterUIHandlerType.AddCharacters);
            _addCharactersUiHandler.Initialize();
            _charactersControlPanel.OpenCharactersViewButtonClicked += DisplayAddCharactersView;
        }

        private void SetupCharactersOverlay()
        {
            _characterOverlaysPanel.Initialize();
        }

        private void OnAssetUpdatingFailed(DbModelType type)
        {
            if(type == DbModelType.SetLocation) HideAssetLoadingSnackBarIfOpened();
        }

        private void OnTemplateApplyingFinished()
        {
            SetupSelectedItems(LevelManager.TargetEvent);
            SetupSelectorScrollPositions();
            HideLoadingOverlay();
        }
        
        private void DisplayAddCharactersView()
        {
            _charactersUIManager.DisplayTargetCharactersUiHandlerView(CharacterUIHandlerType.AddCharacters, Universe.Id);
        }

        private void SetInitialSelectedItems()
        {
            LevelManager.EventStarted -= SetInitialSelectedItems;
            SetupSelectedItems(LevelManager.TargetEvent);
        }

        private void OnExit(bool savedToDraft)
        {
            _pageLoading.ShowDarkOverlay();
        }
        
        private void SetupAssetSelectionModelsList()
        {
            _assetsSelectionModels = new AssetSelectorModel[]
            {
                BodyAnimationsAssetSelector,
                VFXAssetSelector,
                VoiceFilterAssetSelector,
                SetLocationAssetSelector,
                SpawnPositionAssetSelector,
                CameraAssetSelector,
                OutfitAssetSelector,
                CameraFilterAssetSelector,
                CameraFilterVariantAssetSelector
            };
        }

        private void HidePageLoadingOverlay()
        {
            if (_args.HideLoadingPopupRequested != null)
            {
                _args.HideLoadingPopupRequested?.Invoke();
                OnHidden();
                return;
            }

            if (_pageLoading.isActiveAndEnabled)
            {
                _pageLoading.Hide(0.75f, OnHidden);
                return;
            }

            OnHidden();

            void OnHidden()
            {
                _levelEditorPageModel.OnLoadingOverlayHidden();
            }
        }
        
        private void OnShufflingCompleted()
        {
            RefreshSelectedSetLocation();
            SetRenderTexture();
        }
        
        private void SetRenderTexture()
        {
            LevelManager.ApplyRenderTextureToSetLocationCamera(_levelViewPort.RenderTexture);
        }

        public void ShowEmptyOutfitSnackbar()
        {
            _snackBarHelper.ShowInformationSnackBar("Add some clothes to your outfit first");
        }
    }
}