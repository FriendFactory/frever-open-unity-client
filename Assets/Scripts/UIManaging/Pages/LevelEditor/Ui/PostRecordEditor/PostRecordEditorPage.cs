using System;
using System.Diagnostics;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions;
using Modules.CharacterManagement;
using Modules.FreverUMA;
using Modules.FreverUMA.ViewManagement;
using Modules.InputHandling;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.LevelSaving;
using Modules.LevelManaging.Editing.Players;
using Modules.LevelManaging.Editing.Templates;
using Modules.LevelViewPort;
using Modules.WatermarkManagement;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UnityEngine;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Debug = UnityEngine.Debug;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class PostRecordEditorPage : GenericPage<PostRecordEditorArgs>
    {
        [SerializeField] private PostRecordEditorStateListenersInitializer _sceneStateListenersInitializer;
        [SerializeField] private LevelViewPort _levelViewPort;
        [SerializeField] private RectTransform _levelViewUiHolder;

        [Inject] private EventThumbnailCapture _eventThumbnailCapture;
        [Inject] private AmplitudeAssetEventLogger _amplitudeAssetEventLogger;
        [Inject] private StopWatchProvider _stopWatchProvider;
        [Inject] private CharacterManager _characterManager;
        [Inject] private PostRecordEditorFeaturesSetup _featuresSetup;
        [Inject] private ICameraInputController _cameraInputController;
        [Inject] private ILevelManager _levelManager;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private IInputManager _inputManager;
        [Inject] private AvatarHelper _avatarHelper;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private ICameraTemplatesManager _cameraTemplatesManager;
        [Inject] private ITemplatesContainer _templatesContainer;
        [Inject] private IDefaultThumbnailService _defaultThumbnailService;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private PostRecordEditorPageModel _pageModel;
        [Inject] private ILevelAssetsUnCompressingService _levelAssetsUnCompressingService;
        [Inject] private IMovementTypeThumbnailsProvider _movementTypeThumbnailsProvider;
        [Inject] private IWatermarkService _watermarkService;
        [Inject] private ICameraFocusAnimationCurveProvider _cameraFocusAnimationCurveProvider;
        

        private PostRecordEditorPageUI _pageUI;
        private Stopwatch _stopWatch;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.PostRecordEditor;
        private MetadataStartPack MetadataStartPack => _dataFetcher.MetadataStartPack;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _pageUI = GetComponent<PostRecordEditorPageUI>();
            
            _pageModel.CharacterItemClicked += SwitchCharacterState;
            _pageModel.PostRecordEditorOpened += StopMusicPreview;
            _pageModel.SongSelectionOpened += StopMusicPreview;
            _pageModel.OutfitButtonPressed += OnOutfitButtonPressed;
            _pageModel.CreateNewOutfitRequested += OnCreationNewOutfitRequested;
            _pageModel.CreateNewEventRequested += OnCreationNewEventRequested;
            _pageModel.AssetSelectionViewOpened += OnAssetSelectionOpened;
            _pageModel.AssetSelectionViewClosed += OnAssetSelectionClosed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _pageModel.CharacterItemClicked -= SwitchCharacterState;
            _pageModel.PostRecordEditorOpened -= StopMusicPreview;
            _pageModel.SongSelectionOpened -= StopMusicPreview;
            _pageModel.OutfitButtonPressed -= OnOutfitButtonPressed;
            _pageModel.CreateNewOutfitRequested -= OnCreationNewOutfitRequested;
            _pageModel.CreateNewEventRequested -= OnCreationNewEventRequested;
            _pageModel.AssetSelectionViewOpened -= OnAssetSelectionOpened;
            _pageModel.AssetSelectionViewClosed -= OnAssetSelectionClosed;
        }

        //---------------------------------------------------------------------
        // Page
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
     
        }

        protected override void OnDisplayStart(PostRecordEditorArgs args)
        {
            // TODO: temporary solution, need to spend more time in PageManager internals to come up with solution
            if (IsDisplayed)
            {
                base.OnDisplayStart(args);
                return;
            }
            
            UpdateUserAssetsAndBalance();
            SetupCameraFocusCurve();
            
            _levelManager.AllowReducingCharactersQuality = true;
            _featuresSetup.Setup(args.Settings);
            _sceneStateListenersInitializer.Initialize();
            _pageModel.OnMovingForward = args.OnMoveForwardRequested;
            _pageModel.OnMovingBack = args.OnMovingBackRequested;
            _pageModel.CheckIfLevelWasModifiedBeforeExit = args.CheckIfLevelWasModifiedBeforeExit;
            _cameraInputController.AutoActivateOnCameraSystemActivation = true;
            _pageModel.Universe = MetadataStartPack.GetUniverseByGenderId(args.LevelData.GetFirstEvent().GetCharacters().First().GenderId);
            
            _levelManager.Initialize(_dataFetcher.MetadataStartPack);
            _levelManager.EventStarted += OnEditorLoaded;

            _levelManager.LevelPreviewStarted += OnLevelPreviewStarted;
            _levelManager.LevelPreviewCompleted += OnPreviewDone;
            _levelManager.EventPreviewCompleted += OnPreviewDone;
            _levelManager.TemplateApplyingCompleted += OnEventTemplateApplied;
            _levelManager.EventSaved += OnEventSaved;
            _levelManager.PreviewCancelled += OnPreviewDone;

            var level = args.LevelData;
            _levelManager.CurrentLevel = level;

            _levelManager.EventForRecordingSetup += AdjustCameraPosition;
            _pageUI.OnDisplayStart(args);

            if (args.IsPreviewMode)
            {
                _pageModel.ClosePostRecordEditor();
                SetupForPreview();
            }
            else
            {
                var targetEvent = level.Event.First(x => x.LevelSequence == OpenPageArgs.OpeningState.TargetEventSequenceNumber);
                _levelManager.PlayEvent(PlayMode.StayOnFirstFrame, targetEvent, OnEventLoaded);

                void OnEventLoaded()
                {
                    _pageModel.OpenPostRecordEditor();
                    _levelManager.SetupCameraFocusAnimationCurve();
                    if (args.DefaultOpenedAssetSelector.HasValue)
                    {
                        _pageUI.ForceOpenAssetSelector(args.DefaultOpenedAssetSelector.Value);
                    }
                    else
                    {
                        _pageModel.ChangeState(PostRecordEditorState.Default);
                    }
                    
                    args.OnLoadingCompleted?.Invoke();
                }
            }
            
            _inputManager.Enable(false);

            SetupFaceTracking();
            FetchUsedTemplates();
            
            _defaultThumbnailService.Init();
            FetchDefaultEventTemplate();
            _movementTypeThumbnailsProvider.FetchMovementTypeThumbnails();
            _levelViewPort.Init();
            _levelViewPort.RectTransform.CopyProperties(_levelViewUiHolder);
            var ip = _dataFetcher.MetadataStartPack.GetIntellectualPropertyForLevel(level);
            _watermarkService.FetchWaterMark(ip);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            CleanUpInternal();
            _levelManager.SetFaceTracking(false);
            _inputManager.Enable(true);
            
            _avatarHelper.UnloadAllUmaBundles();
            DetachRenderTexture();
            _levelManager.EventSaved -= OnEventSaved;
            _levelManager.EventForRecordingSetup -= AdjustCameraPosition;
            _levelManager.TemplateApplyingCompleted -= OnEventTemplateApplied;
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupForPreview()
        {
            _eventThumbnailCapture.RefreshThumbnailsDuringNextLevelPreview();
            _levelManager.LevelPreviewCompleted += OnLevelPreviewCompleted;
            _levelManager.PreviewCancelled += OnLevelPreviewCompleted;

            if (OpenPageArgs.DecompressBundlesAfterPreview)
            {
                _levelManager.LevelPreviewCompleted += DecompressBundles;

                void DecompressBundles()
                {
                    _levelManager.LevelPreviewCompleted -= DecompressBundles;
                    DecompressLevelBundles();
                }
            }

            _levelManager.SetFaceTracking(false);

            _levelManager.PlayLevelPreview(MemoryConsumingMode.UseFullMemory, PreviewCleanMode.KeepAll, _levelViewPort.RenderTexture);
        }
        
        private void OnLevelPreviewStarted()
        {
            _pageModel.ChangeState(PostRecordEditorState.Preview);
        }

        private void OnPreviewDone()
        {
            // Workaround for UI blinking when previewing from the "publish" page
            if (OpenPageArgs.IsPreviewMode && !OpenPageArgs.DecompressBundlesAfterPreview) return;
            // Workaround for consequent event previews when changing members
            if (_pageModel.CurrentState == PostRecordEditorState.AssetSelection) return;

            _pageModel.ChangeState(PostRecordEditorState.Default);
        }

        private void SetupCamera(ICharacterAsset characterAsset)
        {
            var characterController = _levelManager.TargetEvent.GetCharacterControllerByCharacterId(characterAsset.Id);
            if (characterController?.ControllerSequenceNumber != 0) return;
            _cameraSystem.SetTargets(characterAsset.LookAtBoneGameObject, characterAsset.GameObject);
        }

        private async void SwitchCharacterState(object character)
        {
            var characterFullInfo = character as CharacterFullInfo;

            if (characterFullInfo == null)
            {
                var characterInfo = character as CharacterInfo;

                if (characterInfo == null)
                {
                    Debug.LogError($"Unknown object type {character}, needs to be CharacterInfo or CharacterFullInfo");
                    return;
                }
                
                characterFullInfo = await _characterManager.GetCharacterAsync(characterInfo.Id);
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

        private void OnLevelPreviewCompleted()
        {
            _pageModel.ShowPageLoadingOverlay();
            _levelManager.LevelPreviewCompleted -= OnLevelPreviewCompleted;
            _levelManager.PreviewCancelled -= OnLevelPreviewCompleted;
            OpenPageArgs.OnPreviewCompleted?.Invoke();
        }

        private void CleanUpInternal()
        {
            _pageModel.ClosePostRecordEditor();
            _levelManager.LevelPreviewStarted -= OnLevelPreviewStarted;
            _levelManager.LevelPreviewCompleted -= OnPreviewDone;
            _levelManager.EventPreviewCompleted -= OnPreviewDone;
            _levelManager.PreviewCancelled -= OnPreviewDone;
            _levelManager.EventPreviewStarted -= DisableArSessionIfFaceTrackingEnabled;
            _levelManager.EventPreviewCompleted -= EnableArSessionIfFaceTrackingEnabled;
            _pageModel.PostRecordEventsTimelineModel.Cleanup();
            _pageModel.PreviewEventsTimelineModel.Cleanup();
            _levelManager.CleanUp();
            _pageUI.OnHidingBegin();
            StopMusicPreview();
        }

        private async void DecompressTargetEventBundles()
        {
            await _levelAssetsUnCompressingService.UnCompressHeavyBundles(_levelManager.TargetEvent);
        }

        private async void DecompressLevelBundles()
        {
            await _levelAssetsUnCompressingService.UnCompressHeavyBundles(_levelManager.CurrentLevel);
        }
        
        private void OnEditorLoaded()
        {
            _pageModel.SetSwitchTargetCharacterId(_levelManager.TargetEvent.GetTargetCharacterController().CharacterId);
            
            //todo: clarify why it's should be called only after level editor loaded, seems like work around
            base.OnDisplayStart(OpenPageArgs); // Call only after editor loaded
            if (!OpenPageArgs.IsPreviewMode)
            {
                _pageUI.SetupSelectedItems(_levelManager.TargetEvent);
                _pageUI.SetupSelectorScrollPositions();
                _cameraSystem.CameraUpdated += RememberCameraStartPosition;
            }
            
            _levelManager.EventStarted -= OnEditorLoaded;
            _levelManager.PreventUnloadingUsedLicensedSongs();
        }

        private void OnEventSaved()
        {
            DecompressTargetEventBundles();
        }

        private void RememberCameraStartPosition()
        {
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
            _cameraSystem.CameraUpdated -= RememberCameraStartPosition;
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
            _pageUI.SetupSelectedItems(_levelManager.TargetEvent);
            _pageUI.SetupSelectorScrollPositions();
        }
        
        private async void FetchUsedTemplates()
        {
            var usedTemplateIds = _levelManager.CurrentLevel.GetUsedTemplates();
            await _templatesContainer.FetchFromServer(usedTemplateIds);
        }
        
        private void EnableArSessionIfFaceTrackingEnabled()
        {
            if (!_levelManager.IsFaceRecordingEnabled) return;
            _levelManager.SetFaceTracking(true);
        }

        private void DisableArSessionIfFaceTrackingEnabled()
        {
            if (!_levelManager.IsFaceRecordingEnabled) return;
            _levelManager.SetFaceTracking(false);
        }
        
        private void SetupFaceTracking()
        {
            _levelManager.SetFaceTracking(false);
        }

        private void StopMusicPreview()
        {
            _levelManager.StopAudio();
        }
        
        private async void UpdateUserAssetsAndBalance()
        {
            await _userData.UpdatePurchasedAssetsInfo();
            await _userData.UpdateBalance();
        }
        
        private void OnCreationNewOutfitRequested(long categoryId, long? subcategoryId)
        {
            _levelManager.StopCurrentPlayMode();

            var characterController = _levelManager.EditingCharacterController;
            var data = new PiPOutfitCreationRequestData
            {
                TargetCharacter = characterController.Character,
                CurrentOutfit = characterController.Outfit,
                TargetEventSequenceNumber = _levelManager.TargetEvent.LevelSequence
            };
            OpenPageArgs.OnOutfitCreationRequested?.Invoke(data);
        }

        private void OnCreationNewEventRequested()
        {
            var data = new CreateNewEventRequestData
            {
                TemplateId = OpenPageArgs.Settings.EventCreationSettings.TemplateId
            };
            OpenPageArgs.OnNewEventCreationRequested.Invoke(data);
        }
        
        private void OnOutfitButtonPressed()
        {
            _pageModel.OpenOutfitPanel();
        } 
        
        private void FetchDefaultEventTemplate()
        {
            _templatesContainer.FetchFromServer(_dataFetcher.DefaultUserAssets.TemplateId);
        }

        private void OnAssetSelectionOpened()
        {
            OpenPageArgs.OnAssetSelectionOpened?.Invoke(_pageModel.CurrentAssetType);
        }

        private void OnAssetSelectionClosed()
        {
            OpenPageArgs.OnAssetSelectionClosed?.Invoke(_pageModel.CurrentAssetType);
            _movementTypeThumbnailsProvider.Cleanup();
        }
        
        private void DetachRenderTexture()
        {
            var cam = _levelManager.GetCurrentEventCamera();
            if (cam == null) return;
            cam.targetTexture = null;
        }
        
        private void SetupCameraFocusCurve()
        {
            var anyUsedGender = OpenPageArgs.LevelData.GetFirstEvent().GetCharacters().First().GenderId;
            var race = MetadataStartPack.GetRaceByGenderId(anyUsedGender);
            var animationCurve = _cameraFocusAnimationCurveProvider.GetAnimationCurve(race.Id);
            _cameraSystem.SetFocusPointAdjustmentCurve(animationCurve);
        }
    }
}