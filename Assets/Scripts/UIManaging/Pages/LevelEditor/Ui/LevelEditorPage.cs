using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AppsFlyerSDK;
using Bridge;
using Bridge.Models;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.Common;
using Common;
using Extensions;
using Modules.Amplitude;
using Modules.AppsFlyerManaging;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions;
using Modules.CharacterManagement;
using Modules.EditorsCommon;
using Modules.FaceAndVoice.Face.Facade;
using Modules.FreverUMA;
using Modules.FreverUMA.ViewManagement;
using Modules.InputHandling;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.LevelSaving;
using Modules.LevelManaging.Editing.RecordingModeSelection;
using Modules.LevelManaging.Editing.Templates;
using Modules.WardrobeManaging;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.InputFields;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.CacheManaging;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow;
using UIManaging.Pages.LevelEditor.Ui.Exit;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UIManaging.Pages.LevelEditor.Ui.RecordingButton;
using UnityEngine;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Debug = UnityEngine.Debug;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;


namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class LevelEditorPage : GenericPage<LevelEditorArgs>
    {
        [SerializeField] private LevelDurationProgressUI levelDurationProgressUI;
        [SerializeField] private SongSelectedPreviewManager _songSelectedPreviewManager;
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private LevelEditorStateListenersInitializer _sceneStateListenersInitializer;
        [SerializeField] private AudioRecordingPanel _audioRecordingPanel;
        [SerializeField] private LevelEditorPageWardrobeHelper _wardrobeHelper;
        [SerializeField] private EditingStepsFlowController _editingStepsFlowController;
        
        [Inject] private AmplitudeAssetEventLogger _amplitudeAssetEventLogger;
        [Inject] private ILevelAssetsUnCompressingService _levelAssetsUnCompressingService;
        [Inject] private StopWatchProvider _stopWatchProvider;
        [Inject] private EventRecordingService _eventRecordingService;
        [Inject] private LevelEditorFeaturesSetup _editorFeaturesSetup;
        [Inject] private ILevelManager _levelManager;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private IInputManager _inputManager;
        [Inject] private LevelEditorPageModel _pageModel;
        [Inject] private AvatarHelper _avatarHelper;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private ICameraInputController _cameraInputController;
        [Inject] private ICameraTemplatesManager _cameraTemplatesManager;
        [Inject] private ITemplatesContainer _templatesContainer;
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private IDefaultThumbnailService _defaultThumbnailService;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private IArSessionManager _arSessionManager;
        [Inject] private LevelEditorPageCacheControl _levelEditorPageCacheControl;
        [Inject] private IMovementTypeThumbnailsProvider _movementTypeThumbnailsProvider;
        [Inject] private AudioRecordingStateController _audioRecordingStateController;
        [Inject] private IBridge _bridge;
        [Inject] private IWardrobeStore _wardrobeStore;
        [Inject] private IExitButtonClickHandler[] _clickHandlers;

        private Stopwatch _stopWatch;
        private LevelEditorPageUI _pageUI;
        private bool _loadingCharacterModelForSpawning;
        private bool _closingWardrobePanel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.LevelEditor;
        private MetadataStartPack MetadataStartPack => _dataFetcher.MetadataStartPack;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _pageModel.CharacterItemClicked += SwitchCharacterState;
            _pageModel.SongSelectionOpened += StopMusicPreview;
            _pageModel.OutfitButtonPressed += OnOutfitButtonPressed;
            _pageModel.CreateNewOutfitRequested += OnCreationNewOutfitRequested;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _pageModel.CharacterItemClicked -= SwitchCharacterState;
            _pageModel.SongSelectionOpened -= StopMusicPreview;
            _pageModel.OutfitButtonPressed -= OnOutfitButtonPressed;
            _pageModel.CreateNewOutfitRequested -= OnCreationNewOutfitRequested;
        }

        //---------------------------------------------------------------------
        // Page
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override async void OnDisplayStart(LevelEditorArgs args)
        {
            base.OnDisplayStart(args);
            
            // TODO: temporary solution, need to spend more time in PageManager internals to come up with solution
            if (IsDisplayed) return;
            
            _pageModel.OpeningPageArgs = args.Clone() as LevelEditorArgs;
            var isNewLevel = args.LevelData == null || args.LevelData.IsEmpty();
            var gender = GetAnyUsedInLevelGender(args, isNewLevel);
            _levelManager.Initialize(_dataFetcher.MetadataStartPack);
            _levelManager.SetupCameraFocusAnimationCurve(gender);
            _pageModel.Universe = MetadataStartPack.GetUniverseByGenderId(gender);
            _pageUI = GetComponent<LevelEditorPageUI>();
            _levelManager.AllowReducingCharactersQuality = true;

            _editorFeaturesSetup.Setup(args.Settings);
            _sceneStateListenersInitializer.Initialize();
            _inputBlocker.Block();
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            UpdateUserAssetsAndBalance();
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            
            AppsFlyer.sendEvent(AppsFlyerConstants.CREATE_VIDEO, null);

            _levelManager.EventPreviewStarted += DisableGestures;
            _levelManager.EventPreviewCompleted += EnableGestures;
            _levelManager.TemplateApplyingCompleted += OnEventTemplateApplied;
            _levelManager.EventSaved += OnEventSaved;
            _levelManager.EventDeleted += OnEventDeleted;
            _levelManager.EventDeletionStarted += OnEventDeletionStarted;
            _levelManager.PreviewCancelled += EnableGestures;
            _cameraInputController.CameraModificationStarted += OnCameraModificationStarted;
            _cameraInputController.AutoActivateOnCameraSystemActivation = true;
            _cameraInputController.Activate(true);
            _inputManager.Enable(true);
            _levelManager.SetLocationChangeFinished += OnSetLocationLoaded;
            _levelManager.SpawnFormationChanged += OnSpawnFormationChanged;
            _levelManager.ShufflingBegun += _inputBlocker.Block;
            _levelManager.ShufflingDone += _inputBlocker.UnBlock;
            _levelManager.ShufflingFailed += _inputBlocker.UnBlock;
            _levelManager.OnTrySavingEmptyOutfit += OnTrySavingEmptyOutfit;
            _pageModel.ExitRequested += Exit;
            _pageModel.EnterVideoMessageEditorRequested += args.OnCreateVideoMessageRequested;
            _pageModel.UploadVideoRequested += OnNonLevelVideoUploadRequested;
            _pageModel.StartOverRequested += OnStartOverRequested;
            
            var level = isNewLevel ? _levelManager.CreateLevelInstance() : args.LevelData;
            level.LevelTypeId = ServerConstants.LevelType.STANDARD;
            _levelManager.CurrentLevel = level;

            var originalLevel = isNewLevel ? level.Clone() : _pageModel.OpeningPageArgs.LevelData;
            _pageModel.OriginalEditorLevel = originalLevel;

            SetupEventIndicators(args);
            _eventRecordingService.Activate();
            _levelManager.EventForRecordingSetup += AdjustCameraPosition;

            _pageUI.OnDisplayStart(args);
            _pageModel.LoadingOverlayHidden += OnLoadingOverlayHidden;
            
            var targetEvent = args.DraftEventData.Event ?? level.GetLastEvent();

            if (isNewLevel)
            {
                _pageUI.SetupDefaultRecordingMode();
                _levelManager.SetMusicVolume(99);
                _levelManager.SetVoiceVolume(99);
                SetupFirstEventForNewLevel(args);
            }
            else
            {
                var recordingMode = targetEvent.HasVoiceTrack()
                    ? RecordingMode.Story
                    : RecordingMode.LipSync;
                _levelManager.ChangeRecordingMode(recordingMode);
                var totalRecorded = _levelManager.LevelDurationSeconds / _levelManager.MaxLevelDurationSec;
                levelDurationProgressUI.SetProgress(totalRecorded);

                SetupNextEventForRecording(args, targetEvent);
            }

            SetupFaceTracking();

            // TODO: move to separate component and perform initialization based on PageManger events?
            SetupAudioRecordingPanel();
            _songSelectionController.SongApplied += OnSongSelected;

            FetchTemplates();
            
            _pageModel.ExitButtonClicked += OnExitRequested;
            _pageModel.MoveToPostRecordEditorClicked += DisableGestures;
            _pageModel.ExitCancelled += EnableGestures;
            _levelManager.RecordingStarted += OnRecordingBegin;
            _levelManager.RecordingCancelled += OnRecordingStopped;
            _levelManager.RecordingEnded += OnRecordingStopped;
            
            await _defaultThumbnailService.Init();
            _movementTypeThumbnailsProvider.FetchMovementTypeThumbnails();

            _wardrobeHelper.GenderId = gender;
            _wardrobeHelper.Initialize();
            
            if (args.ShowDressingStep)
            {
                await _levelManager.LoadAndKeepEditingAnimations();
            }
        }

        private static long GetAnyUsedInLevelGender(LevelEditorArgs args, bool isNewLevel)
        {
            return isNewLevel
                ? args.ReplaceCharactersData.First().ReplaceByCharacter.GenderId
                : args.LevelData.GetFirstEvent().GetCharacters().First().GenderId;
        }

        private void OnTrySavingEmptyOutfit()
        {
            _pageUI.ShowEmptyOutfitSnackbar();
        }

        private void SetupEventIndicators(LevelEditorArgs args)
        {
            levelDurationProgressUI.SetupEventIndicators();
            if (args.NewEventsDeletionOnly)
            {
                levelDurationProgressUI.PaleAllEvents();
            }
            else
            {
                levelDurationProgressUI.UnPaleAllEvents();
            }
        }

        private void SetupNextEventForRecording(LevelEditorArgs args, Event targetEvent)
        {
            if (args.DraftEventData.Event != null)
            {
                _levelManager.PlayEvent(PlayMode.PreRecording, args.DraftEventData.Event, OnDraftEventLoaded);
                return;
            }
            
            if (args.Template != null)
            {
                SetupNewEventBasedOnTemplate(args.Template, args.ReplaceCharactersData);
            }
            else
            {
                _levelManager.PlayEvent(PlayMode.PreRecording, targetEvent, OnLevelLastEventLoaded);
            }
        }

        private void SetupFirstEventForNewLevel(LevelEditorArgs args)
        {
            if (args.DraftEventData.Event == null)
            {
                CreateNewEvent(args.Template, args.ReplaceCharactersData);
                _levelManager.CurrentLevel.SchoolTaskId = args.TaskId;
            }
            else
            {
                _levelManager.SetTargetEvent(args.DraftEventData.Event, OnDraftEventLoaded, PlayMode.PreRecording);
            }

            if (args.TemplateHashtagInfo != null)
            {
            #if ADVANCEDINPUTFIELD_TEXTMESHPRO
                _levelManager.CurrentLevel.Description =
                    $"{AdvancedInputFieldUtils.GetHashtagBindingData(args.TemplateHashtagInfo).codePoint} ";
            #else
                _levelManager.CurrentLevel.Description = $"#{args.TemplateHashtagInfo.Name} ";
            #endif
            }
        }

        private void OnNonLevelVideoUploadRequested(NonLeveVideoData videoData)
        {
            OpenPageArgs.OnOpenVideoUploadPageRequested?.Invoke(videoData);
        }

        private void CreateNewEvent(TemplateInfo template, ReplaceCharacterData[] charactersToApply)
        {
            _levelManager.TemplateApplyingCompleted += OnNewLevelBasedOnTemplateSetup;
            SetupNewEventBasedOnTemplate(template, charactersToApply);
        }

        private void DisableGestures()
        {
            _inputManager.Enable(false);
        }

        private void OnDraftEventLoaded()
        {
            _cameraTemplatesManager.SetStartFrameForTemplates(OpenPageArgs.DraftEventData.CameraPosition);
            _cameraSystem.Simulate(_cameraTemplatesManager.CurrentTemplateClip,0);
        }
        
        private void OnLevelLastEventLoaded()
        {
            _levelManager.PutCameraOnLastCameraAnimationFrame();
            _cameraSystem.Simulate(_cameraTemplatesManager.CurrentTemplateClip,0);
            _levelManager.PrepareNewEventBasedOnTarget();
            _levelManager.PlayEvent(PlayMode.PreRecording, _levelManager.TargetEvent);
        }

        private void OnSetLocationLoaded(ISetLocationAsset setLocation)
        {
            _levelEditorPageCacheControl.TryToClearCache();
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            if (_editingStepsFlowController.IsInitialized)
            {
                _editingStepsFlowController.CleanUp();
            }

            _editingStepsFlowController.ExitRequested -= OnExitRequested;
            
            PrepareForClosing();
            onComplete?.Invoke();
        }

        private void PrepareForClosing()
        {
            CleanUpInternal();
            _levelManager.SetFaceTracking(false);
            
            _avatarHelper.UnloadAllUmaBundles();
            _levelManager.EventSaved -= OnEventSaved;
            _levelManager.EventForRecordingSetup -= AdjustCameraPosition;
            _levelManager.TemplateApplyingCompleted -= OnEventTemplateApplied;
            _songSelectionController.SongApplied -= OnSongSelected;
            _levelManager.SpawnFormationChanged -= OnSpawnFormationChanged;
            _levelManager.RecordingStarted -= OnRecordingBegin;
            _levelManager.RecordingCancelled -= OnRecordingStopped;
            _levelManager.RecordingEnded -= OnRecordingStopped;
            _levelManager.ShufflingBegun -= _inputBlocker.Block;
            _levelManager.ShufflingDone -= _inputBlocker.UnBlock;
            _levelManager.ShufflingFailed -= _inputBlocker.UnBlock;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void EnableGestures()
        {
            _inputManager.Enable(true);
        }

        private void SetupCamera(ICharacterAsset characterAsset)
        {
            var characterController = _levelManager.TargetEvent.GetCharacterControllerByCharacterId(characterAsset.Id);
            if (characterController?.ControllerSequenceNumber != 0) return;
            _cameraSystem.SetTargets(characterAsset.LookAtBoneGameObject, characterAsset.GameObject);
        }

        private async void SwitchCharacterState(object character)
        {
            if (character is not CharacterFullInfo characterFullInfo)
            {
                if (character is not CharacterInfo characterInfo)
                {
                    Debug.LogError($"Unknown object type {character}, needs to be CharacterInfo or CharacterFullInfo");
                    return;
                }

                _loadingCharacterModelForSpawning = true;
                characterFullInfo = await _characterManager.GetCharacterAsync(characterInfo.Id);
                _loadingCharacterModelForSpawning = false;
            }
            
            if (_levelManager.TargetEvent.CharacterController.Any(controller => controller.Character.Id == characterFullInfo.Id))
            {
                DespawnCharacter(characterFullInfo);
            }
            else
            {
                SpawnCharacter(characterFullInfo);
            }
        }

        private void SpawnCharacter(CharacterFullInfo character)
        {
            _stopWatch = _stopWatchProvider.GetStopWatch();
            _stopWatch.Restart();
            _levelManager.SpawnCharacter(character, OnCharacterSpawned);

            void OnCharacterSpawned(ICharacterAsset characterAsset)
            {
                _amplitudeAssetEventLogger.LogAddedCharacterAmplitudeEvent(character.Id, _stopWatch.ElapsedMilliseconds.ToSecondsClamped(), characterAsset.View.ViewType == ViewType.Baked);
                _stopWatch.Stop();
                _stopWatchProvider.Dispose(_stopWatch);
                SetupCamera(characterAsset);
                _pageModel.ConfirmCharacterLoaded();
            }
        }

        private void DespawnCharacter(CharacterFullInfo character)
        {
            _levelManager.DestroyCharacter(character);
            _pageModel.ConfirmCharacterLoaded();
            _amplitudeAssetEventLogger.LogRemovedCharacterAmplitudeEvent(character.Id);
        }

        private void CleanUpInternal()
        {
            _levelManager.EventPreviewStarted -= DisableGestures;
            _pageModel.ExitButtonClicked -= OnExitRequested;
            _pageModel.ExitCancelled -= EnableGestures;
            _pageModel.MoveToPostRecordEditorClicked -= DisableGestures;
            _pageModel.ExitRequested -= Exit;
            _pageModel.EnterVideoMessageEditorRequested -= OpenPageArgs.OnCreateVideoMessageRequested;
            _pageModel.StartOverRequested -= OnStartOverRequested;

            _levelManager.EventPreviewCompleted -= EnableGestures;
            _levelManager.PreviewCancelled -= EnableGestures;
            _levelManager.SetLocationChangeFinished -= OnSetLocationLoaded;
            _cameraInputController.CameraModificationStarted -= OnCameraModificationStarted;
            _levelManager.EventPreviewStarted -= DisableArSessionIfFaceTrackingEnabled;
            _levelManager.EventPreviewCompleted -= EnableArSessionIfFaceTrackingEnabled;
            _levelManager.EventDeleted -= OnEventDeleted;
            _levelManager.EventDeletionStarted -= OnEventDeletionStarted;
            
            _levelManager.CleanUp();
            _eventRecordingService.Deactivate();
            _pageUI.OnHidingBegin();
            _songSelectedPreviewManager.StopPreview();
            _levelEditorPageCacheControl.TryToClearCache();
            _movementTypeThumbnailsProvider.Cleanup();
            _audioRecordingPanel.CleanUp();
            _wardrobeHelper.CleanUp();
            _levelManager.ReleaseEditingAnimations();
        }

        private void Exit(bool savedToDraft)
        {
            _arSessionManager.SetARActive(false);
            _levelManager.UseSameFaceFx = false;
            OpenPageArgs.OnExitRequested.Invoke(new LevelEditorExitArgs
            {
                SavedToDraft = savedToDraft
            });
        }

        private async void DecompressTargetEventBundles()
        {
            await _levelAssetsUnCompressingService.UnCompressHeavyBundles(_levelManager.TargetEvent);
        }

        private async void OnEditorLoaded()
        {
            _inputBlocker.UnBlock();
            _pageModel.SetSwitchTargetCharacterId(_levelManager.TargetEvent.GetTargetCharacterController().CharacterId);
            OpenPageArgs.OnLevelEditorLoaded?.Invoke();
            
            base.OnDisplayStart(OpenPageArgs);
            
            _cameraSystem.CameraUpdated += RememberCameraStartPosition;
            if (OpenPageArgs.Music != null)
            {
                _levelManager.ChangeSong(OpenPageArgs.Music);
            }

            _editingStepsFlowController.ExitRequested += OnExitRequested;
            var steps = new List<LevelEditorState> { LevelEditorState.Default };
            if (OpenPageArgs.ShowTemplateCreationStep)
            {
                steps.Add(LevelEditorState.TemplateSetup);
            }

            if (OpenPageArgs.ShowDressingStep)
            {
                steps.Add(LevelEditorState.Dressing);
            }
            
            var initialStep = GetInitialStep();
            var model = new EditingFlowModel(steps.ToArray(), initialStep);
            await _editingStepsFlowController.InitializeAsync(model);
        }

        private LevelEditorState GetInitialStep()
        {
            if (!_levelManager.CurrentLevel.IsEmpty())
            {
                return LevelEditorState.Default;
            }
            
            if (OpenPageArgs.ShowTemplateCreationStep)
            {
                return LevelEditorState.TemplateSetup;
            }

            return OpenPageArgs.ShowDressingStep ? LevelEditorState.Dressing : LevelEditorState.Default;
        }

        private void OnEventSaved()
        {
            DecompressTargetEventBundles();
            var recordedEventLastFrame = _levelManager.GetPreviousEventCameraAnimationLastFrame();
            _cameraTemplatesManager.SetStartFrameForTemplates(recordedEventLastFrame);
            
            _levelManager.PreventUnloadingUsedLicensedSongs();
            _levelManager.ReleaseNotUsedLicensedSongs();
        }

        private void RememberCameraStartPosition()
        {
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
            _cameraSystem.CameraUpdated -= RememberCameraStartPosition;
        }

        private void OnCameraModificationStarted()
        {
             _cameraSystem.StopAnimation();
        }
        
        private void AdjustCameraPosition()
        {
            var previousEventLastFrame = _levelManager.GetPreviousEventCameraAnimationLastFrame();
            if(HasUserChangedCameraState(previousEventLastFrame, _cameraTemplatesManager.TemplatesStartFrame))
                return;
            
            ForceCameraToPreviousEventLastFrame(previousEventLastFrame);
        }

        private void ForceCameraToPreviousEventLastFrame(CameraAnimationFrame previousEventLastFrame)
        {
            //camera depends on character position, so we need to force character to proper position as well
            _levelManager.Simulate(0, PlayMode.Recording, DbModelType.BodyAnimation);
            _cameraSystem.Simulate(previousEventLastFrame);
        }
        
        private bool HasUserChangedCameraState(CameraAnimationFrame originalPosition, CameraAnimationFrame actualPosition)
        {
            if (originalPosition == null) return true;
            return !originalPosition.HasEqualCinemachineValues(actualPosition) || !originalPosition.HasEqualPostProcessingValues(actualPosition);
        }

        private void OnEventTemplateApplied()
        {
            _levelManager.UnloadNotUsedByTargetEventAssets();
            _levelManager.PlayEvent(PlayMode.PreRecording);
        }
        
        private async void FetchTemplates()
        {
            var templateIds = new List<long>();
            templateIds.AddRange(_levelManager.CurrentLevel.GetUsedTemplates());
            templateIds.Add(_dataFetcher.DefaultTemplateId);
            await _templatesContainer.FetchFromServer(templateIds.ToArray());
        }
        
        private void EnableArSessionIfFaceTrackingEnabled()
        {
            if(!_levelManager.IsFaceRecordingEnabled) return;
            _levelManager.SetFaceTracking(true);
        }

        private void DisableArSessionIfFaceTrackingEnabled()
        {
            if(!_levelManager.IsFaceRecordingEnabled) return;
            _levelManager.SetFaceTracking(false);
        }
        
        private void SetupFaceTracking()
        {
            _levelManager.EventPreviewStarted += DisableArSessionIfFaceTrackingEnabled;
            _levelManager.EventPreviewCompleted += EnableArSessionIfFaceTrackingEnabled;
        }
        
        private void OnSongSelected(IPlayableMusic selectedSong, int activationCue)
        {
            _levelManager.ChangeSong(selectedSong, activationCue, x =>
            {
                _levelManager.ReleaseNotUsedLicensedSongs();
            });
        }

        private void StopMusicPreview()
        {
            _songSelectedPreviewManager.StopPreview();
        }

        private void OnSpawnFormationChanged(long? obj)
        {
            UpdateCameraTemplatesStartPosition();
        }

        private void UpdateCameraTemplatesStartPosition()
        {
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
            _cameraTemplatesManager.UpdateStartPositionForTemplate(_cameraTemplatesManager.CurrentTemplateClip);
        }
        
        private void OnLoadingOverlayHidden()
        {
            _pageModel.LoadingOverlayHidden -= OnLoadingOverlayHidden;
            
            _inputBlocker.UnBlock();
            
            OnEditorLoaded();
        }

        private void SetupNewEventBasedOnTemplate(TemplateInfo template, ReplaceCharacterData[] characters)
        {
            var args = new ApplyingTemplateArgs
            {
                Template = template,
                ReplaceCharactersData = characters
            };
            _levelManager.CreateFreshEventBasedOnTemplate(args);
        }
        
        private void OnNewLevelBasedOnTemplateSetup()
        {
            _levelManager.TemplateApplyingCompleted -= OnNewLevelBasedOnTemplateSetup;
            if (!OpenPageArgs.LinkTemplateToEvent)
            {
                _levelManager.UnlinkEventFromTemplate();
            }
            
            _cameraSystem.SetLookAt(true);
            _levelManager.TargetEvent.GetCameraController().LookAtIndex = 1;
            _cameraSystem.SetFollow(true);
            _levelManager.TargetEvent.GetCameraController().FollowAll = true;
        }

        private void OnStartOverRequested()
        {
            OpenPageArgs.OnStartOverRequested?.Invoke();
        }
        
        private void OnEventDeleted()
        {
            levelDurationProgressUI.StopDeleteLastEventAnimation();
            if (_levelManager.IsLevelEmpty)
            {
                PreparationsForEmptyLevel();
            }
            else
            {
                PreparationsForNotEmptyLevel();
            }

            _levelManager.UnloadNotUsedByTargetEventAssets();
        }
        
        private void OnEventDeletionStarted()
        {
            levelDurationProgressUI.RemoveEventIndicator(_levelManager.GetLastEvent().LevelSequence);
        }
        
        private void PreparationsForEmptyLevel()
        {
            levelDurationProgressUI.SetProgress(0);
            _levelManager.PlayEvent(_levelManager.CurrentPlayMode);
        }

        private void PreparationsForNotEmptyLevel()
        {
            var totalRecorded = _levelManager.LevelDurationSeconds / _levelManager.MaxLevelDurationSec;
            levelDurationProgressUI.SetProgress(totalRecorded);
        }
        
        private async Task UpdateUserAssetsAndBalance()
        {
            await _userData.UpdatePurchasedAssetsInfo();
            await _userData.UpdateBalance();
        }

        private void OnOutfitButtonPressed()
        {
            _pageModel.OpenOutfitPanel();
        }
        
        private void OnCreationNewOutfitRequested(long categoryId, long? subcategoryId)
        {
            if (_loadingCharacterModelForSpawning) return;
            _pageModel.ChangeState(LevelEditorState.PurchasableAssetSelection);
            _wardrobeHelper.ShowWardrobe(categoryId, subcategoryId);
        }
        
        private void OnRecordingBegin()
        {
            _pageModel.ChangeState(LevelEditorState.Recording);
        }
        
        private void OnRecordingStopped()
        {
            _pageModel.ChangeState(LevelEditorState.Default);
        }

        private void SetupAudioRecordingPanel()
        {
            IPlayableMusic music;
            int activationCue;
            var replaceOriginalMusic = OpenPageArgs.Music != null;
            if (replaceOriginalMusic)
            {
                music = OpenPageArgs.Music;
                activationCue = 0;
            }
            else
            {
                music = _levelManager.TargetEvent?.GetMusic();
                var musicController = _levelManager.TargetEvent?.GetMusicController();
                activationCue = musicController?.ActivationCue ?? 0;
            }
            var trigger = music != null ? AudioRecordingTrigger.SelectMusic : AudioRecordingTrigger.ActiveVoice;

            _audioRecordingPanel.Initialize();
            _audioRecordingStateController.FireAsync(trigger);
            
            if (music != null)
            {
                _songSelectionController.IsSelectionForEventRecording = true;
                _songSelectionController.ApplySong(music, activationCue, true);
            }
        }

        private async void OnExitRequested()
        {
            DisableGestures();

            _pageModel.ShowLoadingOverlay();

            var clickHandler = _clickHandlers.First(x => x.ExitButtonBehaviour == _pageModel.OpeningPageArgs.ExitButtonBehaviour);
            await clickHandler.HandleClickAsync();

            _pageModel.HideLoadingOverlay();
        }
    }
}