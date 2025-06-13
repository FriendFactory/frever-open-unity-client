using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Common;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CharacterManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players;
using Navigation.Args;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.Common.SongOption.SongTitlePanel;
using UIManaging.Pages.LevelEditor.EditingPage;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using UIManaging.Pages.LevelEditor.Ui.AssetUIManagers;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents;
using UIManaging.Pages.LevelEditor.Ui.Preview;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class PostRecordEditorPageUI : LevelEditorUiBase
    {
        [SerializeField] private SongTitlePanel SongTitlePanel;
        [FormerlySerializedAs("_assetSelectionManagerViewHolder")]
        [SerializeField] private PostRecordAssetSelectionManager assetSelectionViewManager;
        [SerializeField] private EditingPageLoading _pageLoading;
        [SerializeField] private EventTimelinePreviewView eventsTimelineView;
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private GameObject _loadingOverlayTask;

        [SerializeField] private CancelPreviewButton _cancelPreviewButton;
        [SerializeField] private CameraAnimationSpeedAssetView _cameraAnimationSpeedAssetView;
        [SerializeField] private UploadingPanel _uploadingPanel;
        [SerializeField] private PreviewButtonsHolder _previewButtonsHolder;
        [SerializeField] private TaskCheckListInfoButton _taskCheckListInfoButton;
        [SerializeField] private TaskCheckList _taskCheckList;
        
        [Inject] private PostRecordEditorPageModel _pageModel;
        private PostRecordEditorArgs _pageArgs;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private CameraAnimationGenerator _cameraAnimationGenerator;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private ICharactersUIManager _charactersUIManager;
        [Inject] private AudioSourceManager _audioSourceManager;
        [Inject] private IVoiceFilterFeatureControl _voiceFilterFeatureControl;
        [Inject] private ISetLocationSelectionFeatureControl _setLocationFeatureControl;
        [Inject] private IBodyAnimationFeatureControl _bodyAnimationFeatureControl;
        [Inject] private ICaptionFeatureControl _captionFeatureControl;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IOutfitFeatureControl _outfitFeatureControl;
        [Inject] private TaskCheckListController _taskCheckListController;
        [Inject] private ICameraAnimationFeatureControl _cameraAnimationFeatureControl;
        
        private AssetSelectorsHolder _vfxsHolder;
        private SetLocationAssetSelectionHolder _setLocationsHolder;
        private BodyAnimationAssetSelectionHolder _bodyAnimationsHolder;
        private AssetCameraSelectorsHolder _cameraAnimationsHolder;
        private AssetSelectorsHolder _voiceFilterHolder;
        private AssetSelectorsHolder _outfitHolder;
        private MusicSelectionPageModel _musicSelectionPageModel;

        protected override BaseEditorPageModel PageModel => _pageModel;
        private bool IsTask => _pageArgs.TaskFullInfo != null;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(OutfitsManager outfitsManager, AmplitudeAssetEventLogger amplitudeAssetEventLogger, StopWatchProvider stopWatchProvider,
            TemplateCameraAnimationSpeedUpdater templateCameraAnimationSpeedUpdater, ICameraTemplatesManager cameraTemplatesManager, MusicSelectionPageModel musicSelectionPageModel)
        {
            OutfitsManager = outfitsManager;
            AmplitudeAssetEventLogger = amplitudeAssetEventLogger;
            StopWatchProvider = stopWatchProvider;
            TemplateCameraAnimationSpeedUpdater = templateCameraAnimationSpeedUpdater;
            CameraTemplatesManager = cameraTemplatesManager;
            _musicSelectionPageModel = musicSelectionPageModel;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            LevelManager.TargetCharacterSequenceNumberChanged += OnTargetCharacterSequenceNumberChanged;
            LevelManager.EditingCharacterSequenceNumberChanged += OnEditingCharacterSequenceNumberChanged;
            LevelManager.CharactersPositionsSwapped += OnCharactersSwapped;

            _pageModel.SetupSongSelection(_musicSelectionPageModel);
            _pageModel.PostRecordEditorOpened += OnPostRecordingPageOpened;
            _pageModel.PostRecordEditorClosed += OnPostRecordingPageClosed;
            _pageModel.BodyAnimationsButtonClicked += OnBodyAnimationsButtonClicked;
            _pageModel.VfxButtonClicked += OnVfxButtonClicked;
            _pageModel.SetLocationsButtonClicked += OnSetLocationsButtonClicked;
            _pageModel.CameraButtonClicked += OnCameraButtonClicked;
            _pageModel.VoiceButtonClicked += OnVoiceFilterClicked;
            _pageModel.OutfitPanelOpened += OnOutfitPanelOpened;
            _pageModel.PostRecordEditorEventSelectionChanged += OnEventSelectionChanged;
            _pageModel.ShowLoadingIconOverlay += ShowLoadingOverlay;
            _pageModel.ShowPageLoadingOverlayRequested += ShowPageLoadingOverlay;
            _pageModel.HideLoadingIconOverlay += HideLoadingOverlay;
            _pageModel.InformationMessageRequested += ShowInformationMessage;

            LevelManager.LevelPreviewStarted += OnLevelPreviewStarted;
            LevelManager.LevelPiecePlayingCompleted += OnPreviewOnePiecePlayed;
            LevelManager.NextLevelPiecePlayingStarted += OnNextPreviewPieceStarted;
            LevelManager.LevelPreviewCompleted += OnLevelPreviewCompleted;
            LevelManager.EventPreviewCompleted += OnLevelPreviewCompleted;
            LevelManager.PreviewCancelled += OnLevelPreviewCompleted;
            LevelManager.TemplateApplyingStarted += OnTemplateApplyingStarted;
            LevelManager.TemplateApplyingCompleted += OnTemplateApplyingFinished;
            LevelManager.SetLocationChangeFinished += _uploadingPanel.OnSetLocationChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelManager.TargetCharacterSequenceNumberChanged -= OnTargetCharacterSequenceNumberChanged;
            LevelManager.EditingCharacterSequenceNumberChanged -= OnEditingCharacterSequenceNumberChanged;
            LevelManager.CharactersPositionsSwapped -= OnCharactersSwapped;
            
            _pageModel.PostRecordEditorOpened -= OnPostRecordingPageOpened;
            _pageModel.PostRecordEditorClosed -= OnPostRecordingPageClosed;
            _pageModel.BodyAnimationsButtonClicked -= OnBodyAnimationsButtonClicked;
            _pageModel.VfxButtonClicked -= OnVfxButtonClicked;
            _pageModel.SetLocationsButtonClicked -= OnSetLocationsButtonClicked;
            _pageModel.CameraButtonClicked -= OnCameraButtonClicked;
            _pageModel.VoiceButtonClicked -= OnVoiceFilterClicked;
            _pageModel.OutfitPanelOpened -= OnOutfitPanelOpened;
            _pageModel.PostRecordEditorEventSelectionChanged -= OnEventSelectionChanged;
            _pageModel.ShowLoadingIconOverlay -= ShowLoadingOverlay;
            _pageModel.HideLoadingIconOverlay -= HideLoadingOverlay;
            _pageModel.ShowPageLoadingOverlayRequested -= ShowPageLoadingOverlay;
            _pageModel.InformationMessageRequested -= ShowInformationMessage;
            
            LevelManager.LevelPreviewStarted -= OnLevelPreviewStarted;
            LevelManager.LevelPiecePlayingCompleted -= OnPreviewOnePiecePlayed;
            LevelManager.NextLevelPiecePlayingStarted -= OnNextPreviewPieceStarted;
            LevelManager.LevelPreviewCompleted -= OnLevelPreviewCompleted;
            LevelManager.EventPreviewCompleted -= OnLevelPreviewCompleted;
            LevelManager.PreviewCancelled -= OnLevelPreviewCompleted;
            LevelManager.TemplateApplyingStarted -= OnTemplateApplyingStarted;
            LevelManager.TemplateApplyingCompleted -= OnTemplateApplyingFinished;
            LevelManager.SetLocationChangeFinished -= _uploadingPanel.OnSetLocationChanged;

            BodyAnimationsAssetSelector.OnSelectedItemChangedEvent -= OnBodyAnimationSelected;
            VFXAssetSelector.OnSelectedItemChangedEvent -= OnVfxSelected;
            VoiceFilterAssetSelector.OnSelectedItemChangedEvent -= OnVoiceFilterSelected;
            SetLocationAssetSelector.OnSelectedItemChangedEvent -= OnSetLocationSelected;
            SpawnPositionAssetSelector.OnSelectedItemChangedEvent -= OnSpawnPointSelected;
            OutfitAssetSelector.OnSelectedItemChangedEvent -= OnOutfitSelected;
            CameraFilterAssetSelector.OnSelectedItemChangedEvent -= OnCameraFilterAssetSelected;
            CameraFilterVariantAssetSelector.OnSelectedItemChangedEvent -= OnCameraFilterVariantSelected;
            UnSubscribeCameraAssetSelectionManagerModel();
        }

        private void OnEnable()
        {
            _pageModel.CameraFilterClicked += OnCameraCameraFilterButtonClicked;
        }

        private void OnDisable()
        {
            _pageModel.CameraFilterClicked -= OnCameraCameraFilterButtonClicked;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnDisplayStart(PostRecordEditorArgs args)
        {
            _pageArgs = args;
            LevelManager.EventStarted += OnEditorLoaded;
            SetupCharacterUi();
            _cameraSystem.Initialize();
            _previewButtonsHolder.Init(args.EnablePreviewCancellation);
            TemplateCameraAnimationSpeedUpdater.Initialize(_cameraAnimationSpeedAssetView);

            HideLoadingOverlay();
            ConfigureAndShowLoadingPage(args);
            
            var cameraTemplates = _cameraAnimationFeatureControl.AllowToUseAllTemplates
                ? GetAllCameraTemplates()
                : GetCameraTemplates(_cameraAnimationFeatureControl.TemplateIds);
            SetupCameraAnimationsView(cameraTemplates.ToArray());
            var vfxCategories = _pageArgs.Settings.VfxSettings.Categories;
            SetupVfxView(OnVfxSelected, _pageArgs.TaskId, vfxCategories);
            SetupVoiceFilters();
            
            SetupBodyAnimationsView(OnBodyAnimationSelected, taskId: _pageArgs.TaskId);
            SetupSpawnPointsView(OnSpawnPointSelected);
            
            var setLocationCategories = _pageArgs.Settings.SetLocationSettings.Categories;
            SetupSetLocations(OnSetLocationSelected, taskId: _pageArgs.TaskId, categoryIds:setLocationCategories);
            var anyUsedGender = args.LevelData.GetFirstEvent().GetCharacters().First().GenderId;//important only race of the gender, that is why we can pick any
            SetupOutfits(anyUsedGender);
            SetupCameraFiltersVariants(OnCameraFilterVariantSelected);
            SetupCameraFilters(OnCameraFilterAssetSelected, _pageArgs.TaskId);

            _vfxsHolder = new AssetSelectorsHolder(new MainAssetSelectorModel[] {VFXAssetSelector});
            _cameraAnimationsHolder = new AssetCameraSelectorsHolder(new[] {CameraAssetSelector}, _cameraAnimationGenerator);
            _bodyAnimationsHolder = new BodyAnimationAssetSelectionHolder(new[] {BodyAnimationsAssetSelector});
            _setLocationsHolder = new SetLocationAssetSelectionHolder(new MainAssetSelectorModel[] {SetLocationAssetSelector});
            _voiceFilterHolder = new AssetSelectorsHolder(new[] {VoiceFilterAssetSelector});

            LevelManager.EventDeleted += OnEventDeleted;
            LevelManager.AssetUpdateFailed += OnAssetUpdatingFailed;
            
            // TODO: move to MusicSelectionPageArgs
            // var musicPanelSettings = new SongSelectionSettings
            // {
            //     EnabledUserSoundSelection = args.Settings.MusicSettings.UserSoundSettings.AllowUserSounds
            // };
            // _songSelectionMenu.Init(musicPanelSettings);

            SetupTaskCheckList();
        }
        
        public void OnHidingBegin()
        {
            TemplateCameraAnimationSpeedUpdater.CleanUp();
            LevelManager.EventStarted -= OnEditorLoaded;
            LevelManager.EventDeleted -= OnEventDeleted;
            LevelManager.AssetUpdateFailed -= OnAssetUpdatingFailed;
            CleanUpCharacterUi();
            HideAssetLoadingSnackBarIfOpened();
        }

        public void ForceOpenAssetSelector(DbModelType selectorType)
        {
            switch (selectorType)
            {
                case DbModelType.Vfx:
                    _pageModel.OnVfxButtonClicked();
                    break;
                case DbModelType.BodyAnimation:
                    _pageModel.OnBodyAnimationsButtonClicked();
                    break;
                case DbModelType.SetLocation:
                    _pageModel.OnSetLocationsButtonClicked();
                    break;
                case DbModelType.CameraFilter:
                    _pageModel.OnFilterClicked();
                    break;
                default:
                    Debug.LogError("Trying to force open unsupported selector type");
                    break;
            }
        }

        public override void SetupSelectedItems(Event targetEvent, bool silent = true)
        {
            base.SetupSelectedItems(targetEvent, silent);

            var activeBodyAnimation = targetEvent.GetFirstCharacterController().GetBodyAnimation();
            BodyAnimationsAssetSelector.SetSelectedItems(new[] { activeBodyAnimation.Id }, new[] { activeBodyAnimation.BodyAnimationCategoryId }, silent);

            RefreshSelectedCameraAnimationTemplate(targetEvent, silent);
        }
        
        public void SetupSelectorScrollPositions()
        {
            SetupSelectorScrollPositions(assetSelectionViewManager);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected override ICategory[] GetBodyAnimationCategories()
        {
            var categoryIds = _pageArgs.Settings.BodyAnimationSettings.Categories;
            return GetAllBodyAnimationCategories().Where(x => categoryIds.IsNullOrEmpty() || categoryIds.Contains(x.Id)).Cast<ICategory>().ToArray();
        }
        
        private void ConfigureAndShowLoadingPage(PostRecordEditorArgs args)
        {
            if (string.IsNullOrEmpty(args.NavigationMessage)) return;
            
            _pageLoading.Configure(args.NavigationMessage);
            _pageLoading.ShowDark(args.NavigationMessage, onCancel: args.CancelLoadingAction);
        }

        private void OnEditingCharacterSequenceNumberChanged()
        {
            RefreshSelectedBodyAnimation(LevelManager.TargetEvent);
            RefreshSelectedOutfit(LevelManager.TargetEvent);
        }

        private void OnEditorLoaded()
        {
            LevelManager.EventStarted -= OnEditorLoaded;

            _pageArgs.RequestHideLoadingPopup?.Invoke();
            _pageLoading.Hide(0.75f);

            if (_pageArgs.ShowTaskInfo)
            {
                _popupManagerHelper.ShowTaskInfoPopup(_pageArgs.TaskFullInfo);
            }
            
            _uploadingPanel.RefreshState(false);
        }

        private void OnLevelPreviewStarted()
        {
            if (LevelManager.CurrentPlayingType == PlayingType.SingleEvent) return;

            if (!_pageArgs.IsPreviewMode)
            {
                _pageModel.InitializePreviewTimeline();
                eventsTimelineView.Show();
            }

            if (_pageArgs.EnablePreviewCancellation)
            {
                _cancelPreviewButton.Show();
            }
        }

        private void OnLevelPreviewCompleted()
        {
            DisplayLevelEditorUi();
        }
        
        private void DisplayLevelEditorUi()
        {
            HidePreviewEventsTimeline();
        }
        
        private void HidePreviewEventsTimeline()
        {
            if (LevelManager.CurrentPlayingType == PlayingType.SingleEvent) return;
            eventsTimelineView.Hide();
            eventsTimelineView.RemoveEventViews();
            _cancelPreviewButton.Hide();
        }

        private void SetupVoiceFilters()
        {
            VoiceFilterAssetSelector = new VoiceFilterAssetSelector("Voice filters", GetVoiceFilterCategories(), MetaData, _audioSourceManager, () => _voiceFilterFeatureControl.IsVoiceFilterDisablingAllowed)
            {
                PlaySampleOnSelected = false
            };
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
                if(!_pageModel.IsPostRecordEditorOpened) _cameraSystem.RecenterFocus();
                LogSelectedBodyAnimationAmplitudeEvent(bodyAnimation.RepresentedObject);
                
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
            if (!SetLocationAssetSelector.AssetSelectionHandler.IsItemAlreadySelected(parentAsset.ItemId, parentAsset.CategoryId))
            {
                parentAsset.SetIsSelected(true);
                return;
            }
            
            var isParentAssetLoaded = setLocation.Id == parentAsset.ItemId;
            if (!isParentAssetLoaded) return;

            var spawnPosition =  setLocation.GetSpawnPositions()
                .First(spawnPos => spawnPos.Id == characterSpawnPosition.ItemId);

            LevelManager.ChangeCharacterSpawnPosition(spawnPosition, true);
        }

        private void OnVoiceFilterSelected(AssetSelectionItemModel voiceFilterItemModel)
        {
            if (!voiceFilterItemModel.IsSelected) return;

            var voiceFilter = voiceFilterItemModel.RepresentedObject as VoiceFilterFullInfo;
            LevelManager.Change(voiceFilter);
            _pageModel.OnVoiceFilterClicked(voiceFilter);
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
                if(!_pageModel.IsPostRecordEditorOpened) _cameraSystem.RecenterFocus();
                
                LevelManager.Change(vfxAnimation.RepresentedObject as VfxInfo, OnCompleted);
            }
        }
        
        private void OnSetLocationSelected(AssetSelectionItemModel setLocation)
        {
            if (!setLocation.IsSelected) return;
            HideAssetLoadingSnackBarIfOpened();
            ShowAssetLoadingSnackBar();
            
            StopWatch = StopWatchProvider.GetStopWatch();
            StopWatch.Restart();

            var location = setLocation.RepresentedObject as SetLocationFullInfo;
            LevelManager.ChangeSetLocation(location, (x, otherDbModelTypes) => OnSetLocationChangeComplete(x, otherDbModelTypes, setLocation), HideAssetLoadingSnackBarIfOpened);
        }

        private void OnSetLocationChangeComplete(ISetLocationAsset setLocation, DbModelType[] otherDbModelTypes, AssetSelectionItemModel setLocationModel)
        {
            HideAssetLoadingSnackBarIfOpened();
            SpawnPositionAssetSelector.SetCurrentSetLocationId(setLocationModel.ItemId);
            setLocationModel.ApplyModel();

            var spawnPosition = LevelManager.TargetEvent.GetTargetSpawnPosition();
            SpawnPositionAssetSelector.SetSelectedItems(new[] { spawnPosition.Id }, silent: true);

            LogSelectedSetLocationAmplitudeEvent(setLocation);

            _uploadingPanel.RefreshState(true);
            
            if (!otherDbModelTypes.Contains(DbModelType.BodyAnimation)) return;
            
            var vfxBundle = VFXAssetSelector.AssetSelectionHandler
                                            .SelectedModels.FirstOrDefault( itemModel => itemModel.IsSelected && (itemModel.RepresentedObject as VfxInfo)?.BodyAnimationAndVfx != null);

            vfxBundle?.SetIsSelected(false);
        }

        private void OnBodyAnimationsButtonClicked()
        {
            DisableNotSuitableCategoriesForTargetSpawnPoint();
            ShowAssetSelectionManagerView(_bodyAnimationsHolder);
        }

        private void OnVfxButtonClicked()
        {
            ShowAssetSelectionManagerView(_vfxsHolder);
        }

        private void OnSetLocationsButtonClicked()
        {
            ShowAssetSelectionManagerView(_setLocationsHolder);
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
            ShowAssetSelectionManagerView(_outfitHolder);
        }

        private void OnCameraCameraFilterButtonClicked()
        {
            ShowAssetSelectionManagerView(CameraFiltersHolder);
        }

        private void ShowAssetSelectionManagerView(AssetSelectorsHolder assetSelectorsHolder)
        {
            assetSelectionViewManager.gameObject.SetActive(true);
            assetSelectionViewManager.Initialize(assetSelectorsHolder);
        }

        private void OnEventDeleted()
        {
            SetupSelectedItems(LevelManager.TargetEvent);
            SetupSelectorScrollPositions();
            HideLoadingOverlay();
        }

        private void OnPostRecordingPageOpened()
        {
            if (CameraAssetSelector == null) return;

            UnSubscribeCameraAssetSelectionManagerModel();
            CameraAssetSelector.OnSelectedItemChangedEvent += OnCameraAnimationSelectedPostRecording;
        }
        
        private void OnPostRecordingPageClosed()
        {
            if (CameraAssetSelector == null) return;

            UnSubscribeCameraAssetSelectionManagerModel();
            CameraAssetSelector.OnSelectedItemChangedEvent += OnCameraAnimationSelected;
            CameraAssetSelector.OnSelectedItemSilentChangedEvent += OnCameraAnimationSelectedSilent;
        }

        private void UnSubscribeCameraAssetSelectionManagerModel()
        {
            CameraAssetSelector.OnSelectedItemChangedEvent -= OnCameraAnimationSelected;
            CameraAssetSelector.OnSelectedItemSilentChangedEvent-= OnCameraAnimationSelectedSilent;
            CameraAssetSelector.OnSelectedItemChangedEvent -= OnCameraAnimationSelectedPostRecording;
        }

        private void ShowLoadingOverlay(string title)
        {
            _loadingOverlay.Show(title: title);
        }

        private void ShowPageLoadingOverlay()
        {
            _pageLoading.ShowDark(_pageArgs.NavigationMessage, false, true);
        }

        private void HideLoadingOverlay()
        {
            _loadingOverlay.Hide();
        }

        private void OnPreviewOnePiecePlayed()
        {
            _pageLoading.ShowDark();
            if (_cancelPreviewButton.IsShown)
            {
                _cancelPreviewButton.Hide();
            }
        }
        
        private void OnNextPreviewPieceStarted()
        {
            _pageLoading.Hide(0);
            if (_pageArgs.EnablePreviewCancellation)
            {
                _cancelPreviewButton.Show();
            }
        }

        private void ShowAssetLoadingSnackBar()
        {
            _snackBarHelper.ShowAssetLoadingSnackBar(float.PositiveInfinity);
        }

        private void HideAssetLoadingSnackBarIfOpened()
        {
            _snackBarHelper.HideSnackBar(SnackBarType.AssetLoading);
        }
        
        private void LogSelectedBodyAnimationAmplitudeEvent(IEntity asset)
        {
            AmplitudeAssetEventLogger.LogSelectedBodyAnimationAmplitudeEvent(asset.Id, StopWatch.ElapsedMilliseconds.ToSecondsClamped());
            StopAndDisposeStopWatch();
        }
        
        private void LogSelectedSetLocationAmplitudeEvent(IAsset asset)
        {
            AmplitudeAssetEventLogger.LogSelectedSetLocationAmplitudeEvent(asset.Id, StopWatch.ElapsedMilliseconds.ToSecondsClamped());
            StopAndDisposeStopWatch();
        }
        
        private void LogSelectedCameraCameraTemplateAmplitudeEvent(TemplateCameraAnimationClip clip)
        {
            AmplitudeAssetEventLogger.LogSelectedCameraTemplateAmplitudeEvent(clip.Id, StopWatch.ElapsedMilliseconds.ToSecondsClamped());
            StopAndDisposeStopWatch();
            CameraTemplatesManager.TemplateAnimationChanged -= LogSelectedCameraCameraTemplateAmplitudeEvent;
        }

        private void SetupCharacterUi()
        {
            _charactersUIManager.OnDisplayCharacterSelector += ShowAssetSelectionManagerView;
        }

        private void CleanUpCharacterUi()
        {
            _charactersUIManager.OnDisplayCharacterSelector -= ShowAssetSelectionManagerView;
        }
        
        private void OnAssetUpdatingFailed(DbModelType type)
        {
            if(type == DbModelType.SetLocation) HideAssetLoadingSnackBarIfOpened();
        }

        private void OnTemplateApplyingStarted()
        {
            _pageLoading.Show();
        }
        
        private void OnTemplateApplyingFinished()
        {
            _pageLoading.Hide();
        }

        private void OnEventSelectionChanged(Event targetEvent)
        {
            ResetBodyAnimationListCache();
            
            SetupSelectedItems(targetEvent);
            var musicSelectionAllowed = _pageArgs.Settings.MusicSettings.AllowEditing;
            RefreshMusicSelectionButtonState(targetEvent, musicSelectionAllowed);
            SetupSelectorScrollPositions();
            
            if (IsTask)
            {
                _taskCheckList.HideHints();
            }
        }
        
        private void ShowInformationMessage(string message)
        {
            _popupManagerHelper.ShowInformationMessage(message);
        }
       
        private async void SetupTaskCheckList()
        {
            var mandatoryAssetTypes = GetMandatoryTaskAssetTypes();

            if (_pageArgs.ShowTaskInfo)
            {
                _loadingOverlayTask.SetActive(true);
            }

            if (IsTask)
            {
                _taskCheckList.Initialize(_pageArgs.TaskFullInfo);
                
                await _taskCheckListController.Setup(_pageArgs.CheckIfUserMadeEnoughChangesForTask, _pageArgs.TaskFullInfo.Id, mandatoryAssetTypes);
            }

            _taskCheckListInfoButton.SetActive(IsTask);
            _loadingOverlayTask.SetActive(false);
        }

        private DbModelType[] GetMandatoryTaskAssetTypes()
        {
            var assetTypes = new List<DbModelType>();
            if (_setLocationFeatureControl.IsFeatureEnabled)
            {
                assetTypes.Add(DbModelType.SetLocation);
                assetTypes.Add(DbModelType.CharacterSpawnPosition);
            }

            if (_bodyAnimationFeatureControl.IsFeatureEnabled)
            {
                assetTypes.Add(DbModelType.BodyAnimation); 
            }

            if (_captionFeatureControl.IsFeatureEnabled)
            {
                assetTypes.Add(DbModelType.Caption);  
            }
            
            return assetTypes.ToArray();
        }

        private void RefreshMusicSelectionButtonState(Event targetEvent, bool musicSelectionAllowed)
        {
            if(!musicSelectionAllowed) return;
            
            var music = targetEvent.GetMusic();

            if (music != null)
            {
                var controller = targetEvent.GetMusicController();
                SongTitlePanel.SetAppliedSong(music, controller.ActivationCue);
            }
            else
            {
                SongTitlePanel.DisplayNoSongSelectedPanel();
            }
        }
    }
}