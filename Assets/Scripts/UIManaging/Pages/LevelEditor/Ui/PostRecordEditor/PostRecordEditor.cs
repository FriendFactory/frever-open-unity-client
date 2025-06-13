using System;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Common;
using DigitalRubyShared;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSystemCore;
using Modules.InputHandling;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Players;
using Modules.LevelViewPort;
using Modules.LocalStorage;
using Modules.TempSaves;
using Modules.TempSaves.Manager;
using Navigation.Args;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.SongTitlePanel;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedSetLocationTabViews;
using UIManaging.Pages.LevelEditor.Ui.AdvancedSettings;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using UIManaging.Pages.LevelEditor.Ui.AssetUIManagers;
using UIManaging.Pages.LevelEditor.Ui.CacheManaging;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.ConfirmAssetChangesHandlers;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class PostRecordEditor : MonoBehaviour
    {
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private PostRecordAssetSelectionManager assetSelectionViewManager;
        [SerializeField] private Button _backButton;
        [SerializeField] private PointerDownButton _backgroundButton;
        [SerializeField] private CameraAnimationSpeedAssetView _cameraAnimationSpeedAssetView;
        [SerializeField] private TimeOfDayView _timeOfDayView;
        [SerializeField] private CameraFilterValueAssetView _cameraFilterValueAssetView;
        [SerializeField] private AdvancedSettingsCameraView _advancedSettingsCameraView;
        [SerializeField] private AdvancedSettingsSetlocationView _advancedSettingsSetLocationView;
        [SerializeField] private CaptionsPanel.CaptionsPanel _captionsPanel;
        [SerializeField] private EditPageSongTitlePanel _editPageSongTitlePanel;
        [SerializeField] private Selectable[] _blockWhileLoadingAssets;
        [Header("Events Timeline")]
        [SerializeField] private EventTimelinePostRecordingView _eventsTimelineView;
        [SerializeField] private Button _previewButton;
        [SerializeField] private Button _createEventButton;
        [SerializeField] private LevelViewPort _levelViewPort;
        [SerializeField] private CaptionPanelActivator _captionPanelActivator;

        [Inject] private PostRecordEditorPageModel _editorPageModel;
        [Inject] private CaptionProjectionManager _captionProjectionManager;
        private EventTimelineItemModel _selectedEvent;
        [Inject] private ILevelManager _levelManager;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private ICameraTemplatesManager _cameraTemplatesManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private EventThumbnailCapture _eventThumbnailCapture;
        [Inject] AmplitudeManager _amplitudeManager;
        [Inject] private ICharactersUIManager _charactersUIManager;
        [Inject] private TempFileManager _tempFileManager;
        private ConfirmAssetChangesHandlerProvider _confirmAssetChangesHandlerProvider;
        private BaseCharactersUIHandler _switchCharactersUiHandler;
        [Inject] private EventSettingsStateChecker _eventSettingsStateChecker;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PostRecordEditorPageCacheControl _cacheControl;
        [Inject] private IInputManager _inputManager;
        [Inject] private FingersScript _fingersScript;
        [Inject] private IBridge _bridge;
        
        private bool _originalIsFaceRecordingEnabled;
        private bool _playingSingleEventPreview;
        private bool _isSwitchingEvents;
        private bool _isSubscribedToEvents;
        private CameraAnimationFrame _currentTransformBasedCameraState;
        private CancellationTokenSource _waitForDelayTokenSource;

        private AdvancedSettingsView[] _advancedSettingsViews;

        private readonly DbModelType[] _retakeEventThumbnailAssetTypes =
        {
            DbModelType.CameraFilter, DbModelType.CameraFilterVariant, DbModelType.SpawnFormation, DbModelType.Character,
            DbModelType.CharacterSpawnPosition, DbModelType.SetLocation, DbModelType.Outfit, DbModelType.Vfx, DbModelType.Caption,
            DbModelType.BodyAnimation
        };

        private readonly DbModelType[] _regenerateCameraAnimAssetTypes =
        {
            DbModelType.CharacterSpawnPosition, DbModelType.SetLocation, DbModelType.CameraAnimationTemplate,
            DbModelType.BodyAnimation
        };

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private Event TargetEvent => _levelManager.TargetEvent;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ICameraSystem cameraSystem, UncompressedBundlesManager uncompressedBundlesManager,
            CameraAnimationGenerator cameraAnimationGenerator, ICharactersUIManager charactersUIManager)
        {
            _confirmAssetChangesHandlerProvider = new ConfirmAssetChangesHandlerProvider(cameraAnimationGenerator, _editorPageModel, uncompressedBundlesManager, cameraSystem, _levelManager, _eventSettingsStateChecker);
            _advancedSettingsViews = new AdvancedSettingsView[] {_advancedSettingsCameraView, _advancedSettingsSetLocationView};
            _switchCharactersUiHandler = charactersUIManager.GetCharacterUIHandler(CharacterUIHandlerType.SwitchCharacters);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _editorPageModel.PostRecordEditorOpened += OnPostRecordEditorOpened;
            _editorPageModel.PostRecordEditorClosed += OnPostRecordEditorClosed;
            _editorPageModel.PostRecordEditorSubscribe += SubscribeToEvents;
            _editorPageModel.PostRecordEditorUnsubscribe += UnsubscribeFromEvents;
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _backgroundButton.OnPointerDown += OnBackgroundPointerDown;
            _captionsPanel.Closed += OnCaptionPanelClosed;
            _fingersScript.PassThroughObjects.Add(_backgroundButton.gameObject);
        }

        private void OnDisable()
        {
            HideLoadingOverlay();
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _backgroundButton.OnPointerDown -= OnBackgroundPointerDown;
            _captionsPanel.Closed -= OnCaptionPanelClosed;
            _fingersScript.PassThroughObjects.Remove(_backgroundButton.gameObject);
        }

        private void OnDestroy()
        {
            _editorPageModel.PostRecordEditorOpened -= OnPostRecordEditorOpened;
            _editorPageModel.PostRecordEditorClosed -= OnPostRecordEditorClosed;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void OnPostRecordEditorOpened()
        {
            _levelManager.StopAudio();

            ShowLoadingOverlay();
            _backButton.SetActive(_editorPageModel.OnMovingBack != null);
           
            SubscribeToEvents();

            var originalLevel = _levelManager.CurrentLevel.Clone();
            _editorPageModel.OriginalPostRecordLevel = originalLevel;
            _originalIsFaceRecordingEnabled = _levelManager.IsFaceRecordingEnabled;

            _levelManager.SetFaceRecording(false);
            _levelManager.SetFaceTracking(false);
            SetRenderTextureToSetLocationCamera();
           
            await _captionsPanel.Init(GetCameraForActiveSetLocation());
            RefreshCaptionProjections();
            _inputManager.Enable(true, GestureType.Rotation);
            _captionPanelActivator.Enable(true);
        }

        private void OnSongApplied(IPlayableMusic selectedSong, int activationCue)
        {
            var previousEvent = _levelManager.CurrentLevel.GetEventBefore(TargetEvent);
            var shouldMatchWithPreviousEvent = selectedSong !=null && previousEvent != null && previousEvent.HasMusic() &&
                                               previousEvent.HasMusic(selectedSong);
            if (shouldMatchWithPreviousEvent)
            {
                activationCue = previousEvent.GetMusicController().EndCue;
            }

            _levelManager.ApplySongStartingFromTargetEvent(selectedSong, activationCue, () =>
            {
                SaveLevelLocally();
    
                _levelManager.PreventUnloadingUsedLicensedSongs();
                _levelManager.ReleaseNotUsedLicensedSongs();
            });
        }

        private void SaveLevelLocally()
        {
            _levelManager.SaveEditingEvent();
        }

        private void OnAssetStartedUpdating(DbModelType type, long id)
        {
            BlockClosingAssetSelectionView();

            if (type == DbModelType.Outfit)
            {
                SetInteractableForBlockedWhileLoadingUI(false);
            }
            else if (type == DbModelType.SetLocation)
            {
                SetupCameraForFirstFrameOfCurrentTemplateCameraAnimation();
                TryToClearCache();
            }
        }

        private void BlockClosingAssetSelectionView()
        {
            _backgroundButton.enabled = false;
        }

        private void SetInteractableForBlockedWhileLoadingUI(bool value)
        {
            for (int i = 0; i < _blockWhileLoadingAssets.Length; i++)
            {
                _blockWhileLoadingAssets[i].interactable = value;
            }
        }

        private void SubscribeToEvents()
        {
            if (_isSubscribedToEvents) return;

            _isSubscribedToEvents = true;
            
            _levelManager.CharacterReplacementStarted += OnCharacterReplacementStarted;
            _levelManager.CharacterReplaced += OnCharacterReplaced;
            _levelManager.UseSameFaceFxChanged += OnUseSameFaceFxChanged;
            _levelManager.EventDeletionStarted += ShowLoadingOverlay;
            _levelManager.CharactersOutfitsUpdated += OnCharactersOutfitsUpdated;
            _levelManager.LevelPreviewStarted += SetupThumbnailsCapturingDuringPreview;
            _editorPageModel.PostRecordEventsTimelineModel.SelectedEventChanged += OnSelectedEventChanged;
            _previewButton.onClick.AddListener(OnPreviewButtonClicked);
            _createEventButton.onClick.AddListener(OnCreateEventButtonClicked);
            _eventsTimelineView.EventEditButtonClicked += OnEventEditClicked;
            _editorPageModel.VoiceFilterClicked += OnVoiceFilterClicked;
            _editorPageModel.CameraButtonClicked += OnCameraButtonClicked;
            _editorPageModel.CaptionCreationRequested += OnCaptionCreationRequested;
            _editorPageModel.LevelAudioPanelOpened += OnLevelAudioPanelOpened;
            _editorPageModel.CharacterToSwitchClicked += BlockClosingAssetSelectionView;
            _editorPageModel.AssetSelectionViewClosed += OnAssetSelectionProcessFinished;
            _editorPageModel.AssetSelectionViewOpened += OnAssetSelectionViewOpened;
            _editorPageModel.OutfitChangingBegun += BlockClosingAssetSelectionView;
            _cameraAnimationSpeedAssetView.SetOnPointerUpListener(OnCameraAnimationSpeedViewPointerUp);
            _cameraFilterValueAssetView.SetOnPointerUpListener(OnCameraFilterViewPointerUp);
            _timeOfDayView.SetOnPointerDownListener(OnTimeOfDayPointerDown);
            _timeOfDayView.SetOnPointerUpListener(OnTimeOfDayPointerUp);
            _levelManager.AssetUpdateStarted += OnAssetStartedUpdating;
            _levelManager.CharactersPositionsSwapped -= OnCharacterPositionsSwapped;
            _levelManager.CharactersPositionsSwapped += OnCharacterPositionsSwapped;
            _cameraTemplatesManager.TemplateAnimationChanged -= OnCameraTemplateAnimationChanged;
            _cameraTemplatesManager.TemplateAnimationChanged += OnCameraTemplateAnimationChanged;
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;
            _levelManager.BodyAnimationChanged += OnBodyAnimationChanged;
            _levelManager.SpawnPositionChanged += OnSpawnPositionChanged;
            _advancedSettingsCameraView.ConfirmChangesButtonClicked += OnAdvancedCameraSettingsConfirmed;
            _advancedSettingsCameraView.DiscardChangesButtonClicked += OnAdvancedCameraSettingsDiscarded;
            _editorPageModel.SwitchCharacterButtonClicked += DisplaySwitchCharactersView;
            assetSelectionViewManager.Opened += AssetSelectionViewManagerOpened;
            assetSelectionViewManager.Closed += AssetSelectionViewManagerClosed;
            _songSelectionController.SongApplied += OnSongApplied;
            _levelManager.SetLocationChangeFinished += OnSetLocationFinishedLoading;
            _editorPageModel.LevelAudioPanelClosed += OnLevelAudioPanelClosed;
            _captionsPanel.CaptionPanelOpening += OnBeforeStartCaptionEditing;
            _switchCharactersUiHandler.Initialize();
            SubscribeAdvancedSettingsViews();
        }

        private void UnsubscribeFromEvents()
        {
            _isSubscribedToEvents = false;
            _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;
            _levelManager.CharactersPositionsSwapped -= OnCharacterPositionsSwapped;
            _levelManager.CharacterReplacementStarted -= OnCharacterReplacementStarted;;
            _levelManager.CharacterReplaced -= OnCharacterReplaced;
            _levelManager.UseSameFaceFxChanged -= OnUseSameFaceFxChanged;
            _levelManager.EventDeletionStarted -= ShowLoadingOverlay;
            _levelManager.CharactersOutfitsUpdated -= OnCharactersOutfitsUpdated;
            _levelManager.LevelPreviewStarted -= SetupThumbnailsCapturingDuringPreview;
            _editorPageModel.PostRecordEventsTimelineModel.SelectedEventChanged -= OnSelectedEventChanged;
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            _previewButton.onClick.RemoveListener(OnPreviewButtonClicked);
            _createEventButton.onClick.RemoveListener(OnCreateEventButtonClicked);
            _eventsTimelineView.EventEditButtonClicked -= OnEventEditClicked;
            _editorPageModel.VoiceFilterClicked -= OnVoiceFilterClicked;
            _editorPageModel.CameraButtonClicked -= OnCameraButtonClicked;
            _editorPageModel.CaptionCreationRequested -= OnCaptionCreationRequested;
            _editorPageModel.LevelAudioPanelOpened -= OnLevelAudioPanelOpened;
            _editorPageModel.CharacterToSwitchClicked -= BlockClosingAssetSelectionView;
            _editorPageModel.OutfitChangingBegun -= BlockClosingAssetSelectionView;
            _editorPageModel.AssetSelectionViewClosed -= OnAssetSelectionProcessFinished;
            _editorPageModel.AssetSelectionViewOpened -= OnAssetSelectionViewOpened;
            _cameraTemplatesManager.TemplateAnimationChanged -= OnCameraTemplateAnimationChanged;
            _levelManager.AssetUpdateStarted -= OnAssetStartedUpdating;
            _levelManager.BodyAnimationChanged -= OnBodyAnimationChanged;
            _levelManager.SpawnPositionChanged -= OnSpawnPositionChanged;
            _cameraAnimationSpeedAssetView.ResetEvents();
            _cameraFilterValueAssetView.ResetEvents();
            _timeOfDayView.ResetEvents();
            _advancedSettingsCameraView.ConfirmChangesButtonClicked -= OnAdvancedCameraSettingsConfirmed;
            _advancedSettingsCameraView.DiscardChangesButtonClicked -= OnAdvancedCameraSettingsDiscarded;
            _editorPageModel.SwitchCharacterButtonClicked -= DisplaySwitchCharactersView;
            assetSelectionViewManager.Opened -= AssetSelectionViewManagerOpened;
            assetSelectionViewManager.Closed -= AssetSelectionViewManagerClosed;
            _songSelectionController.SongApplied -= OnSongApplied;
            _levelManager.SetLocationChangeFinished -= OnSetLocationFinishedLoading;
            _editorPageModel.LevelAudioPanelClosed -= OnLevelAudioPanelClosed;
            _captionsPanel.CaptionPanelOpening -= OnBeforeStartCaptionEditing;
            _switchCharactersUiHandler.CleanUp();
            UnSubscribeAdvancedSettingsViews();
        }

        private void OnPostRecordEditorClosed()
        {
            UnsubscribeFromEvents();
            Cleanup();
        }

        private void Cleanup()
        {
            _editorPageModel.PostRecordEditorSubscribe -= SubscribeToEvents;
            _editorPageModel.PostRecordEditorUnsubscribe -= UnsubscribeFromEvents;
            _captionsPanel.Cleanup();
        }

        private void OnSpawnFormationChanged(long? formationId)
        {
            TargetEvent.CharacterSpawnPositionFormationId = formationId;
            PlayCurrentEventPreview();
        }

        private void OnLevelAudioPanelClosed()
        {
            SaveLevelLocally();
            SwitchCaptionProjections(true);
        }

        private void OnBackgroundPointerDown(BaseEventData eventData)
        {
            if (assetSelectionViewManager.IsOpened)
            {
                assetSelectionViewManager.OnTapOutsideView();
            }
        }

        private void OnCameraTemplateAnimationChanged(TemplateCameraAnimationClip animationClip)
        {
            PlayCurrentEventPreview(PlayMode.PreviewWithCameraTemplate);
        }

        private void OnChangesConfirmedEventSet()
        {
            _levelManager.PauseEventPlayMode();
            CoroutineSource.Instance.ExecuteAtEndOfFrame(PauseRendering);
            
            var eventViewChanged = IsAppliedChangesRequireThumbnailRefresh();
            if (eventViewChanged)
            {
                RefreshEventViewTexture();
            }
            ResubscribeToFormationChange();
            SaveLevelLocally();

            if (TargetEvent.HasActualThumbnail)
            {
                TargetEvent.HasActualThumbnail = !eventViewChanged;
            }
            RefreshThumbnailForEventIfNeeded();
        }

        private void PauseRendering()
        {
            _cameraSystem.EnableCameraRendering(false);
        }

        private void RefreshEventIcon(Event ev)
        {
            var timeline = _editorPageModel.PostRecordEventsTimelineModel;
            var eventModel = timeline.EventTimelineItemModels.First(x => x.Event.Id == ev.Id);
            eventModel.ReloadThumbnail();
        }
        
        private bool IsAppliedChangesRequireThumbnailRefresh()
        {
            return _eventSettingsStateChecker.HasAnyAssetsChanged(_retakeEventThumbnailAssetTypes);
        }

        private void OnCameraAnimationSpeedViewPointerUp()
        {
            PlayCurrentEventPreview(PlayMode.PreviewWithCameraTemplate);
        }

        private void OnCameraFilterViewPointerUp()
        {
            PlayCurrentEventPreview();
        }

        private void OnTimeOfDayPointerDown()
        {
            _cameraSystem.EnableCameraRendering(true);
        }
        
        private void OnTimeOfDayPointerUp()
        {
            _cameraSystem.EnableCameraRendering(false);
            _levelManager.SaveDayNightControllerValues();
        }

        private void ShowLoadingOverlay()
        {
            _loadingOverlay.Show();
        }

        private void HideLoadingOverlay()
        {
            _loadingOverlay.Hide();
        }

        private void OnAssetUpdated(DbModelType type)
        {
            RefreshBackgroundButtonState();
            
            if (type.IsAudioType())
            {
                _levelManager.StopAudio();
                return;
            }
           
            switch (type)
            {
                case DbModelType.SetLocation:                 
                    _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;//todo: try to remove not clear unsubscribing
                    return;
                case DbModelType.BodyAnimation:
                case DbModelType.CameraAnimation:
                case DbModelType.CameraAnimationTemplate:
                case DbModelType.CharacterSpawnPosition:
                case DbModelType.Outfit: //Do nothing when outfit loaded. PlayCurrentEventPreview will be called after all outfits loaded
                    break; 
                default:
                    PlayCurrentEventPreview();
                    break;
            }
        }

        private void RefreshBackgroundButtonState()
        {
            _backgroundButton.enabled = !_levelManager.IsChangingAsset && !_levelManager.IsChangingOutfit;
        }

        private void OnCharactersOutfitsUpdated()
        {
            RefreshBackgroundButtonState();
            SetInteractableForBlockedWhileLoadingUI(true);
            PlayCurrentEventPreview();
        }

        private void SyncBodyAnimationCueForAllCharacters()
        {
            if (_levelManager.UseSameBodyAnimation || _levelManager.EditingCharacterSequenceNumber == -1)
            {
                var characterControllers = TargetEvent.CharacterController;

                foreach (var cc in characterControllers)
                {
                    cc.CharacterControllerBodyAnimation.First().ActivationCue = 0;
                }
            }
        }

        private void OnCharacterReplacementStarted(CharacterFullInfo from, CharacterFullInfo to)
        {
            BlockClosingAssetSelectionView();
            _levelManager.StopCurrentPlayMode();
            PauseRendering();
        }
        
        private void OnCharacterReplaced(ICharacterAsset characterAsset)
        {
            PlayCurrentEventPreview();
        }

        private void OnUseSameFaceFxChanged()
        {
            var useSameFaceFx = _levelManager.UseSameFaceFx;

            if (useSameFaceFx)
            {
                var targetCharacterController = _levelManager.TargetCharacterController;

                if (targetCharacterController == null)
                {
                    targetCharacterController = TargetEvent.CharacterController.FirstOrDefault(cc =>
                        cc.CharacterControllerFaceVoice?.FirstOrDefault()?.FaceAnimation != null);
                }

                var targetCharacterFaceVoiceController = targetCharacterController?.CharacterControllerFaceVoice.FirstOrDefault();

                if (targetCharacterFaceVoiceController != null)
                {
                    var allCharacterControllers = TargetEvent.CharacterController;

                    foreach (var characterController in allCharacterControllers)
                    {
                        var characterControllerFaceVoice = characterController.CharacterControllerFaceVoice.FirstOrDefault();
                        characterControllerFaceVoice.SetFaceAnimation(targetCharacterFaceVoiceController.FaceAnimation);
                    }
                }
            }
            else
            {
                ResetFaceAnimationToOriginal();
            }

            PlayCurrentEventPreview();
        }

        private void ResetFaceAnimationToOriginal()
        {
            var allCharacterControllers = TargetEvent.CharacterController;
            var originalLevel = _editorPageModel.OriginalPostRecordLevel;
            var originalEvent = originalLevel.Event.FirstOrDefault(ev => ev.Id == TargetEvent.Id);

            foreach (var characterController in allCharacterControllers)
            {
                var characterControllerFaceVoice = characterController.CharacterControllerFaceVoice.FirstOrDefault();
                var originalCharacterController = originalEvent.GetCharacterControllerByControllerId(characterController.Id);
                var originalFaceVoice = originalCharacterController.CharacterControllerFaceVoice.FirstOrDefault();
                characterControllerFaceVoice.SetFaceAnimation(originalFaceVoice.FaceAnimation);
            }

            _levelManager.UseSameFaceFxChanged -= OnUseSameFaceFxChanged;
            _levelManager.UseSameFaceFx = false;
            _levelManager.UseSameFaceFxChanged += OnUseSameFaceFxChanged;
        }

        private void OnSetLocationFinishedLoading(ISetLocationAsset setLocation)
        {
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
            _cameraTemplatesManager.UpdateStartPositionForTemplate(_cameraTemplatesManager.CurrentTemplateClip);
            PlayCurrentEventPreview(PlayMode.PreviewWithCameraTemplate);
        }

        private void OnCharacterPositionsSwapped()
        {
            PlayCurrentEventPreview();
        }

        private void PlayCurrentEventPreview(PlayMode mode = PlayMode.Preview)
        {
            if (_isSwitchingEvents) return;
            _playingSingleEventPreview = true;
            _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;
            var shouldOverrideAnimationTemplate = mode == PlayMode.PreviewWithCameraTemplate;
            _levelManager.PlayEvent(mode, onStartPlay: SetRenderTextureToSetLocationCamera, onEventPlayed: () => RefreshEventViewTexture(shouldOverrideAnimationTemplate));
            _cameraSystem.EnableCameraRendering(true);
        }

        private void ResubscribeToFormationChange()
        {
            _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;
            _levelManager.SpawnFormationChanged += OnSpawnFormationChanged;
        }

        private void RefreshEventViewTexture(bool overrideAnimationWithTemplate = false)
        {
            _cameraSystem.EnableCameraRendering(true);
            var playMode = overrideAnimationWithTemplate ? PlayMode.PreviewWithCameraTemplate : PlayMode.Preview;
            PreparationsOnEventPreviewDone(playMode);
            DisableCameraRenderingAfterFrameIsRendered();
        }

        private void PreparationsOnEventPreviewDone(PlayMode playMode)
        {
            _playingSingleEventPreview = false;
            SetRenderTextureToSetLocationCamera();
            _levelManager.Simulate(0, playMode);
            ResubscribeToFormationChange();
        }

        private void SetRenderTextureToSetLocationCamera()
        {
            _levelManager.ApplyRenderTextureToSetLocationCamera(_levelViewPort.RenderTexture);
        }

        private Camera GetCameraForActiveSetLocation()
        {
            return _levelManager.GetActiveCamera();
        }

        private void BeforeLevelPreview()
        {
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;
            SetRenderTextureToSetLocationCamera();
            
            _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;
            _editorPageModel.PostRecordEventsTimelineModel.SelectedEventChanged -= OnSelectedEventChanged;
            PreviewSubscribing();
        }

        private void OnVoiceFilterClicked(VoiceFilterFullInfo voiceFilter)
        {
            PlayCurrentEventPreview();
        }

        private void RefreshOriginalCameraAnimation()
        {
            var currentEventCameraAnimation = _levelManager.GetCurrentCameraAnimationAsset();
            var clip = currentEventCameraAnimation.Clip;
            _cameraSystem.PutCinemachineToState(clip.GetFrame(0));
        }

        private void OnPreviewButtonClicked()
        {
            _previewButton.interactable = false;
            _createEventButton.interactable = false;

            _loadingOverlay.Show(title:"Preview");
            BeforeLevelPreview();

            var firstEventIndex = _selectedEvent?.Event.LevelSequence - 1 ?? 0;
            _levelManager.PlayLevelPreview(firstEventIndex, MemoryConsumingMode.UseFullMemory, PreviewCleanMode.KeepAll, _levelViewPort.RenderTexture);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.LEVEL_PREVIEW_BUTTON_CLICKED);
        }

        private void OnLevelPreviewFinished()
        {
            ResubscribeToFormationChange();
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            PreviewUnSubscribing();
            _editorPageModel.PostRecordEventsTimelineModel.SelectedEventChanged += OnSelectedEventChanged;

            void OnTargetEventSet()
            {
                PreparePictureForTargetEvent();
                _previewButton.interactable = true;
                _createEventButton.interactable = true;
            }

            SetupSelectedEvent(OnTargetEventSet);
        }

        private void OnCreateEventButtonClicked()
        {
            _previewButton.interactable = false;
            _createEventButton.interactable = false;

            _editorPageModel.RequestCreationOfNewEvent();
        }

        private void SetSelectedEvent(EventTimelineItemModel eventTimelineItemModel)
        {
            if(_selectedEvent == eventTimelineItemModel) return;
            _selectedEvent = eventTimelineItemModel;
            _editorPageModel.OnPostRecordEditorEventSelectionChanged(_selectedEvent.Event);
        }

        private void PreviewSubscribing()
        {
            _levelManager.LevelPreviewStarted += OnLevelLevelPreviewStarted;
            _levelManager.LevelPreviewCompleted += OnLevelPreviewFinished;
            _levelManager.PreviewCancelled += OnLevelPreviewFinished;
        }

        private void PreviewUnSubscribing()
        {
            _levelManager.LevelPreviewStarted -= OnLevelLevelPreviewStarted;
            _levelManager.LevelPreviewCompleted -= OnLevelPreviewFinished;
            _levelManager.PreviewCancelled -= OnLevelPreviewFinished;
        }

        private void OnLevelLevelPreviewStarted()
        {
            HideLoadingOverlay();
            _cameraSystem.EnableCameraRendering(true);
        }

        private void OnSelectedEventChanged()
        {
            SetupSelectedEvent(OnSetupComplete);

            void OnSetupComplete()
            {
                RefreshOriginalCameraAnimation();
                PreparePictureForTargetEvent();
                RefreshThumbnailForEventIfNeeded();
                SetRenderTextureToSetLocationCamera();
                TryToClearCache();
                RefreshCaptionProjections();
            }
        }

        private void TryToClearCache()
        {
            _cacheControl.TryToClearCache();
        }
        
        private void SetupSelectedEvent(Action onTargetEventSet)
        {
            _isSwitchingEvents = true;
            ShowLoadingOverlay();
            var eventTimelineItemModel = _editorPageModel.PostRecordEventsTimelineModel.SelectedEvent;
            SetSelectedEvent(eventTimelineItemModel);
            var ev = eventTimelineItemModel.Event;
            _cameraTemplatesManager.ChangeTemplateAnimation(ev.GetCameraAnimationTemplateId());
            var activeCamera =  _levelManager.GetActiveCamera();
            if (!(activeCamera is null))
            {
                activeCamera.targetTexture = null;
            }
                                            
            _levelManager.SetTargetEvent(ev, ()=>
            {
                SetRenderTextureToSetLocationCamera();
                onTargetEventSet?.Invoke();
            }, PlayMode.StayOnFirstFrame);
            _levelManager.EditingCharacterSequenceNumber = TargetEvent.TargetCharacterSequenceNumber;
        }

        private void PreparePictureForTargetEvent()
        {
            _cameraSystem.EnableCameraRendering(true);
            _isSwitchingEvents = false;
            _levelManager.PauseEventPlayMode();
            _levelManager.Simulate(0, PlayMode.Preview);
            _levelManager.UnloadNotUsedByTargetEventAssets();
            
            var cameraAnimFirstFrame = _levelManager.GetCurrentCameraAnimationFirstFrame();
            _cameraTemplatesManager.SetStartFrameForTemplates(cameraAnimFirstFrame);

            HideLoadingOverlay();
            
            try
            {
                DisableCameraRenderingAfterFrameIsRendered();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Disable camera rendering at end of frame was cancelled.");
            }
        }

        private void RefreshThumbnailForEventIfNeeded()
        {
            _eventThumbnailCapture.RefreshTargetEventThumbnail(RefreshEventIcon);
        }

        private async void OnBackButtonClicked()
        {
            _editorPageModel.ShowLoadingOverlay();

            var original = _editorPageModel.OriginalPostRecordLevel;
            var current = _levelManager.CurrentLevel;

            if (_editorPageModel.CheckIfLevelWasModifiedBeforeExit && await _levelManager.IsLevelModified(original, current))
            {
                _popupManagerHelper.OpenLeavePostRecordEditorPopup(OnErase, OnSaveDraft);
            }
            else
            {
                GoBackToLevelEditor(_levelManager.CurrentLevel);
            }
            
            void OnErase()
            {
                UnloadCameraAnimationIfWasChanged();
                GoBackToLevelEditor(_editorPageModel.OriginalPostRecordLevel);
            }

            _editorPageModel.HideLoadingOverlay();
        }

        private void UnloadCameraAnimationIfWasChanged()
        {
            var cameraAnimAsset = _levelManager.GetCurrentCameraAnimationAsset();
            if (cameraAnimAsset == null) return;
            var originCameraAnimFilePath = _editorPageModel.OriginalPostRecordLevel.Event.Select(x => x.GetCameraAnimation())
                                               .First(x => x.Id == cameraAnimAsset.Id).GetAnimationFilePath();
            var currentFilePath = cameraAnimAsset.RepresentedModel.GetAnimationFilePath();
            if (originCameraAnimFilePath != currentFilePath)
            {
                _levelManager.UnloadAsset(cameraAnimAsset);
            }
        }
        
        private void GoBackToLevelEditor(Level level)
        {
            _waitForDelayTokenSource?.Cancel();
            _levelManager.CleanUp();

            if (_levelManager.IsFaceRecordingEnabled != _originalIsFaceRecordingEnabled)
            {
                _levelManager.SetFaceRecording(_originalIsFaceRecordingEnabled);
            }
            
            var movingBackArgs = new MovingBackData
            {
                LevelData = level,
                OriginalLevelData = _editorPageModel.OriginalPostRecordLevel
            };
            _editorPageModel.OnMovingBack?.Invoke(movingBackArgs);
        }

        private async void OnSaveDraft()
        {
            var currentLevel = _levelManager.CurrentLevel;
            
            await currentLevel.ResetLocalIdsAsync();

            _levelManager.SaveLevel(currentLevel, OnSaved, OnSaveFailed);

            void OnSaved(Level level)
            {
                LocalStorageManager.DeleteFiles();
                _tempFileManager.RemoveTempFile(Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
                _cameraSystem.ClearCachedFilesAsync();
                _levelManager.UnloadAllAssets();
                var movingBackArgs = new MovingBackData
                {
                    LevelData = level,
                    OriginalLevelData = _editorPageModel.OriginalPostRecordLevel,
                    SavedAsDraft = true
                };
                _editorPageModel.OnMovingBack?.Invoke(movingBackArgs);
            }

            void OnSaveFailed(string message)
            {
                if (message.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
                {
                    _snackBarHelper.ShowInformationSnackBar(Constants.ErrorMessage.UNABLE_TO_SAVE_LEVEL_INACCESSIBLE_ASSETS);
                }
                else
                {
                    Debug.LogError(message);
                }
            }
        }

        private void OnEventEditClicked(EventTimelineItemModel eventTimelineItemModel)
        {
            SetSelectedEvent(eventTimelineItemModel);

            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;

            _cameraTemplatesManager.TemplateAnimationChanged -= OnCameraTemplateAnimationChanged;
            _cameraTemplatesManager.TemplateAnimationChanged += OnCameraTemplateAnimationChanged;

            ResubscribeToFormationChange();
        }

        private void AssetSelectionViewManagerOpened()
        {
            _eventsTimelineView.gameObject.SetActive(false);
        }

        private void AssetSelectionViewManagerClosed()
        {
            _eventsTimelineView.gameObject.SetActive(true);
        }

        private void OnBeforeAdvancedSettingsViewDisplayed()
        {
            if (_playingSingleEventPreview)
            {
                _levelManager.StopCurrentPlayMode();
                PreparationsOnEventPreviewDone(PlayMode.PreviewWithCameraTemplate);
            }

            _currentTransformBasedCameraState = _cameraSystem.GetCurrentTransformBasedCameraState();
            _cameraSystem.EnableCameraRendering(true);
        }

        private void DisableCameraRenderingAfterFrameIsRendered()
        {
            if (_waitForDelayTokenSource != null && _waitForDelayTokenSource.IsCancellationRequested) return;
            CoroutineSource.Instance.ExecuteAtEndOfFrame(PauseRendering);
        }
        
        private void OnAdvancedCameraSettingsConfirmed()
        {
            _cameraSystem.EnableCameraRendering(false);
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
            var templateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraTemplatesManager.UpdateStartPositionForTemplate(templateClip);
            PutCameraOnSavedTransformBasedCameraState();
        }
        private void OnAdvancedCameraSettingsDiscarded()
        {
            _cameraSystem.EnableCameraRendering(true);
            PutCameraOnSavedTransformBasedCameraState();
            var templateClip = _cameraTemplatesManager.CurrentTemplateClip;
            var startFrame = templateClip.GetFrame(0);
            _cameraTemplatesManager.SetStartFrameForTemplates(startFrame);
            _cameraSystem.Simulate(templateClip, 0);
            DisableCameraRenderingAfterFrameIsRendered();
        }

        private void PutCameraOnSavedTransformBasedCameraState()
        {
            _cameraSystem.Simulate(_currentTransformBasedCameraState);
        }
        
        private void SetupCameraForFirstFrameOfCurrentTemplateCameraAnimation()
        {
            var originalCameraAnimationAsset = _levelManager.GetCurrentCameraAnimationAsset();
            var camAnimFirstFrame = originalCameraAnimationAsset.Clip.GetFrame(0);
            _cameraTemplatesManager.SetStartFrameForTemplates(camAnimFirstFrame);
            var template = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.SimulateTemplate(template, 0, true);
        }

        private void OnCameraButtonClicked()
        {
            var template = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.Simulate(template, 0);
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        private void OnCaptionCreationRequested()
        {
            _captionsPanel.StartNewCaptionCreation();
        }
        
        private void OnBeforeStartCaptionEditing()
        {
            PlayCurrentEventPreview(PlayMode.StayOnFirstFrame);
            _editPageSongTitlePanel.SetActive(false);
            _eventSettingsStateChecker.StoreSettings(TargetEvent);
            _editorPageModel.ChangeState(PostRecordEditorState.AssetSelection);
        }

        private void SubscribeAdvancedSettingsViews()
        {
            foreach (var view in _advancedSettingsViews)
            {
                view.Shown += OnBeforeAdvancedSettingsViewDisplayed;
            }
        }
        
        private void UnSubscribeAdvancedSettingsViews()
        {
            foreach (var view in _advancedSettingsViews)
            {
                view.Shown -= OnBeforeAdvancedSettingsViewDisplayed;
            }
        }

        private void DisplaySwitchCharactersView()
        {
            _charactersUIManager.DisplayTargetCharactersUiHandlerView(CharacterUIHandlerType.SwitchCharacters, _editorPageModel.Universe.Id);
        }

        private void OnAssetSelectionViewOpened()
        {
            _eventSettingsStateChecker.StoreSettings(_selectedEvent.Event);
            _captionPanelActivator.Enable(false);
            SwitchCaptionProjections(false);
        }

        private void SwitchCaptionProjections(bool isOn)
        {
            var captions = _levelManager.TargetEvent.Caption;
            if (captions.IsNullOrEmpty()) return;
            foreach (var caption in captions)
            {
                _captionProjectionManager.SwitchProjection(caption.Id, isOn);
            }
        }
        
        private void OnAssetSelectionProcessFinished()
        {
            if (_eventSettingsStateChecker.HasAnyAssetsChanged(_regenerateCameraAnimAssetTypes))
            {
                var handler = _confirmAssetChangesHandlerProvider.GetHandler(_editorPageModel.CurrentAssetType);
                handler?.Run();
            }

            _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;
            if (_playingSingleEventPreview) _levelManager.StopCurrentPlayMode();
            _levelManager.SetTargetEvent(_selectedEvent.Event, OnChangesConfirmedEventSet, PlayMode.StayOnFirstFrame);
            
            _captionPanelActivator.Enable(true);
            SwitchCaptionProjections(true);
        }

        private void OnLevelAudioPanelOpened()
        {
            _eventSettingsStateChecker.StoreSettings(TargetEvent);
            SwitchCaptionProjections(false);
        }

        private void OnCaptionPanelClosed()
        {
            _editorPageModel.ChangeState(PostRecordEditorState.Default);
            _editorPageModel.OnCaptionPanelClosed();

            _levelManager.RefreshTargetEventAssets();
            SaveLevelLocally();
            _playingSingleEventPreview = false;
            if (TargetEvent.HasActualThumbnail)
            {
                var isCaptionChanged = _eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.Caption);
                TargetEvent.HasActualThumbnail = !isCaptionChanged;
                if (isCaptionChanged)
                {
                    ForceCaptionAssetsRefresh();//prevents problem when text is not updated because we are keeping component disabled until postprocessing has finished
                }
            }
            RefreshCaptionProjections();
            
            var activeCamera = GetCameraForActiveSetLocation();
            activeCamera.Render(); //force proper rendering with caption, otherwise postprocessing like DoF can be applied to the text
            
            RefreshThumbnailForEventIfNeeded();
            CoroutineSource.Instance.ExecuteAtEndOfFrame(PauseRendering);//allow captions to render
        }

        private void RefreshCaptionProjections()
        {
            _captionProjectionManager.SetupCaptionsProjection(_levelManager.TargetEvent.Caption);
        }

        private void ForceCaptionAssetsRefresh()
        {
            foreach (var captionAsset in _levelManager.GetCurrentCaptionAssets())
            {
                captionAsset.ForceRefresh();
            }
        }

        private void SetupThumbnailsCapturingDuringPreview()
        {
            _eventThumbnailCapture.RefreshThumbnailsDuringNextLevelPreview(RefreshEventIcon);
        }
        
        private void OnBodyAnimationChanged()
        {
            SyncBodyAnimationCueForAllCharacters();
            PlayCurrentEventPreview(PlayMode.PreviewWithCameraTemplate);
        }
        
        private void OnSpawnPositionChanged()
        {
            PlayCurrentEventPreview(PlayMode.PreviewWithCameraTemplate);
        }
    }
}