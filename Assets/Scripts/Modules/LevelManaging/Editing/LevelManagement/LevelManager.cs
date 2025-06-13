using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.Common;
using Bridge;
using Bridge.AssetManagerServer;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Models.ClientServer.Level.Shuffle;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Services._7Digital.Models.TrackModels;
using Common;
using Common.Permissions;
using Configs;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.FaceAndVoice.Face.Facade;
using Modules.FaceAndVoice.Face.Recording.Core;
using Modules.FaceAndVoice.Voice.Recording.Core;
using Modules.FreverUMA;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.CameraManaging;
using Modules.LevelManaging.Editing.CameraManaging.CameraSettingsManaging;
using Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation;
using Modules.LevelManaging.Editing.EventRecording;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging;
using Modules.LevelManaging.Editing.EventSaving;
using Modules.LevelManaging.Editing.LevelSaving;
using Modules.LevelManaging.Editing.Players;
using Modules.LevelManaging.Editing.RecordingModeSelection;
using Modules.LevelManaging.Editing.Templates;
using Modules.LevelManaging.Editing.ThumbnailCreator;
using Modules.LocalStorage;
using Modules.MusicCacheManaging;
using Modules.TempSaves.Manager;
using Modules.WatermarkManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using CharacterController = Models.CharacterController;
using CharacterSpawnPositionFormation = Bridge.Models.ClientServer.StartPack.Metadata.CharacterSpawnPositionFormation;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;
using SpawnPositionSpaceSize = Bridge.Models.ClientServer.StartPack.Metadata.SpawnPositionSpaceSize;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    [UsedImplicitly]
    internal sealed partial class LevelManager : ILevelManager
    {
        private readonly IContextControl _context;
        private readonly IEventEditor _eventEditor;
        private readonly IAssetManager _assetManager;
        private readonly IBridge _bridge;
        private readonly IPreviewManager _previewManager;
        private readonly EventDeleter _eventDeleter;
        private readonly EventSaver _eventSaver;
        private readonly LevelSaver _levelSaver;
        private readonly CameraFocusManager _cameraFocusManager;
        private readonly LocalUserDataHolder _dataHolder;
        private readonly WatermarkControl _watermarkControl;
        private readonly IWatermarkService _watermarkService;
        
        private readonly IFaceAnimRecorder _faceAnimRecorder;
        private readonly ICameraSystem _cameraSystem;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly NotUsedAssetsUnloader _notUsedAssetsUnloader;
        private readonly CameraSettingProvider _cameraSettingProvider;
        private readonly TempFileManager _tempFileManager;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;
        private readonly CharacterSpawnFormationChanger _characterSpawnFormationChanger;
        private readonly ActivationCueManager _activationCueManager;
        private readonly CameraSpawnFormationControl _cameraSpawnFormationControl;
        private readonly MusicApplier _musicApplier;
        private readonly IPermissionsHelper _permissionsService;
        private readonly ITemplateProvider _templateProvider;
        private readonly ILicensedMusicUsageManager _licensedMusicManager;
        private readonly ILicensedMusicProvider _licensedMusicProvider;
        private readonly ICameraFocusAnimationCurveProvider _cameraFocusAnimationCurveProvider;
        private MetadataStartPack _metadataStartPack;

        private Level _currentLevel;

        private CancellationTokenSource _cameraAnimationCancellationSource;
        private CancellationTokenSource _eventLoadingCancellationSource;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action EventDeletionStarted;
        public event Action EventDeleted;
        public event Action TemplateApplyingStarted;
        public event Action TemplateApplyingCompleted;
        public event Action<ApplyingTemplateArgs> TemplateApplied;
        public event Action<DbModelType, long> StartUpdatingAsset;
        public event Action<DbModelType, long> StopUpdatingAsset;
        public event Action<IEntity> AssetLoaded;
        public event Action<DbModelType, long> AssetUpdateStarted;
        public event Action<DbModelType> AssetUpdateCompleted;
        public event Action<DbModelType> AssetUpdateFailed;
        public event Action<DbModelType> AssetUpdateCancelled;
        public event Action CharactersPositionsSwapped;
        public event Action<long?> SpawnFormationSetup;
        public event Action<long?> SpawnFormationChanged;
        public event Action<ISetLocationAsset> SetLocationChangeFinished;
        public event Action BodyAnimationChanged;
        public event Action SpawnPositionChanged;
        public event Action CharacterSpawnStarted;
        public event Action<ICharacterAsset> CharacterSpawned;
        public event Action<CharacterFullInfo, CharacterFullInfo> CharacterReplacementStarted;
        public event Action<ICharacterAsset> CharacterReplaced;
        public event Action CharacterDestroyed;
        public event Action TargetCharacterSequenceNumberChanged;
        public event Action EditingCharacterSequenceNumberChanged;
        public event Action SongChanged;
        public event Action<Event> TargetEventChanged;
        public event Action CurrentLevelChanged;
        public event Action CharactersOutfitsUpdatingBegan;
        public event Action CharactersOutfitsUpdated;
        public event Action RequestPlayerCenterFaceStarted;
        public event Action RequestPlayerCenterFaceFinished;
        public event Action RequestPlayerNeedsBetterLightingStarted;
        public event Action RequestPlayerNeedsBetterLightingFinished;
        public event Action ShufflingBegun;
        public event Action ShufflingFailed;
        public event Action PhotoOnSetLocationChanged;
        public event Action ShufflingDone;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float LevelDurationSeconds => CalculateEventsDuration();
        public Event TargetEvent => _eventEditor.TargetEvent;
        public bool IsChangingAsset => _eventEditor.IsChangingAsset;
        public bool IsLevelEmpty => CurrentLevel.IsEmpty();
        public bool IsDeletingEvent { get; private set; }
        public bool IsReplacingCharacter { get; private set; }
        public bool IsChangingOutfit => _eventEditor.IsChangingOutfit;

        public int TargetCharacterSequenceNumber
        {
            get => _eventEditor.TargetCharacterSequenceNumber;
            set
            {
                if (TargetCharacterSequenceNumber == value) return;
                
                _eventEditor.TargetCharacterSequenceNumber = value;
                SetEditingCharacterSequenceNumberSilent(value);
                RefreshGroupFocusLevelEditorSettings();
                TargetCharacterSequenceNumberChanged?.Invoke();
            }
        }

        public int EditingCharacterSequenceNumber
        {
            get => _eventEditor.EditingCharacterSequenceNumber;
            set
            {
                _eventEditor.EditingCharacterSequenceNumber = value;
                EditingCharacterSequenceNumberChanged?.Invoke();
            }

        }

        public ICharacterAsset TargetCharacterAsset => _eventEditor.TargetCharacterAsset;

        public ICharacterAsset EditingTargetCharacterAsset => _assetManager.GetAllLoadedAssets<ICharacterAsset>()
                                                                           .FirstOrDefault(
                                                                                x => x.Id == EditingCharacterController
                                                                                   .CharacterId);
        public CharacterController TargetCharacterController => _eventEditor.TargetCharacterController;
        public CharacterController EditingCharacterController => TargetEvent?.GetCharacterController(EditingCharacterSequenceNumber);
        
        public Level CurrentLevel
        {
            get => _currentLevel;
            set
            {
                _currentLevel = value;
                _context.SetCurrentLevel(_currentLevel);
                OrderEventsByLevelSequence();
                LinkNewModelsToAlreadyLoadedEditableAssets(value);
                CurrentLevelChanged?.Invoke();
            }
        }

        public float MaxLevelDurationSec => _dataHolder.IsStarCreator ? Constants.LevelDefaults.MAX_LEVEL_DURATION_STAR : Constants.LevelDefaults.MAX_LEVEL_DURATION;
        public float MinEventDurationMs => Constants.LevelDefaults.MIN_EVENT_DURATION_MS;
        public bool IsSongSelected => TargetEvent?.MusicController != null && TargetEvent.MusicController.Any();

        public bool UseSameBodyAnimation
        {
            get => _eventEditor.UseSameBodyAnimation;
            set => _eventEditor.UseSameBodyAnimation = value;
        }

        public int VoiceVolume
        {
            get
            {
                var characterController = TargetEvent?.GetFirstCharacterController();
                return characterController?.CharacterControllerFaceVoice.First().VoiceSoundVolume ?? 0;
            }
        }

        public int MusicVolume
        {
            get
            {
                var musicController = TargetEvent?.GetMusicController();
                return musicController?.LevelSoundVolume ?? 0;
            }
        }

        public bool IsShuffling { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public LevelManager(IBridge bridge, IAssetManager assetManager, ICameraSystem cameraSystem,
            ARFaceManager faceManager, IFaceAnimRecorder faceAnimRecorder, IVoiceRecorder voiceRecorder,
            IArSessionManager sessionManager, IEventEditor eventEditor, EventSaver eventSaver,
            AmplitudeManager amplitudeManager, IPreviewManager previewManager, EventRecorder eventRecorder,
            NotUsedAssetsUnloader notUsedAssetsUnloader, CameraFocusManager cameraFocusManager,
            CameraSettingProvider cameraSettingProvider, TempFileManager tempFileManager, ICameraTemplatesManager cameraTemplatesManager, 
            CharacterSpawnFormationChanger characterSpawnFormationChanger, ActivationCueManager activationCueManager,
            LevelSaver levelSaver, CameraSpawnFormationControl cameraSpawnFormationControl, MusicApplier musicApplier,
            IContextControl context, PreviousEventLastCameraFrameProvider previousEventLastCameraFrameProvider, 
            IPermissionsHelper permissionsService, EventThumbnailsCreatorManager eventThumbnailsCreator, 
            EventAssetsProvider eventAssetProvider, ITemplateProvider templateProvider, LocalUserDataHolder dataHolder, 
            ILicensedMusicUsageManager licensedMusicManager, ILicensedMusicProvider licensedMusicProvider,
            ICharacterEditor characterEditor, CharacterViewContainer characterViewContainer, CharacterManagerConfig characterManagerConfig,
            WatermarkControl watermarkControl, IWatermarkService watermarkService, ICameraFocusAnimationCurveProvider cameraFocusAnimationCurveProvider, AvatarHelper avatarHelper)
        {
            _bridge = bridge;
            _assetManager = assetManager;
            _amplitudeManager = amplitudeManager;
            _previousEventLastCameraFrameProvider = previousEventLastCameraFrameProvider;
            _permissionsService = permissionsService;
            _dataHolder = dataHolder;
            _licensedMusicManager = licensedMusicManager;
            _licensedMusicProvider = licensedMusicProvider;
            _watermarkControl = watermarkControl;
            _watermarkService = watermarkService;
            _cameraFocusAnimationCurveProvider = cameraFocusAnimationCurveProvider;

            _eventDeleter = new EventDeleter(previewManager, assetManager, amplitudeManager);

            _arSessionManager = sessionManager;
            _arFaceManager = faceManager;
            _cameraSystem = cameraSystem;
            _faceAnimRecorder = faceAnimRecorder;
            _cameraFocusManager = cameraFocusManager;
            _cameraSettingProvider = cameraSettingProvider;
            _tempFileManager = tempFileManager;
            _cameraTemplatesManager = cameraTemplatesManager;
            _activationCueManager = activationCueManager;
            _characterSpawnFormationChanger = characterSpawnFormationChanger;
            _cameraSpawnFormationControl = cameraSpawnFormationControl;
            _context = context;
            _eventThumbnailsCreator = eventThumbnailsCreator;
            _eventAssetProvider = eventAssetProvider;
            _templateProvider = templateProvider;
            _characterEditor = characterEditor;
            _avatarHelper = avatarHelper;

            InitializeRecorder(voiceRecorder, eventRecorder);
            InitializeFaceTracker();

            _eventEditor = eventEditor;
            _eventEditor.Updated += OnAssetUpdated;
            _eventEditor.AssetUpdatingFailed += OnAssetUpdatingFailed;
            _eventEditor.AssetStartedUpdating += OnAssetStartedUpdating;

            _eventEditor.TargetEventChanged += OnTargetEventChanged;
            _eventEditor.EventLoadingStarted += OnEventLoadingStarted;
            _eventEditor.EventLoadingComplete += OnEventLoadingComplete;
            _eventEditor.UseSameFaceFxChanged += OnUseSameFaceFxChanged;
            _eventEditor.CharactersOutfitsUpdatingBegan += OnCharacterOutfitUpdatingBegan;
            _eventEditor.SpawnFormationSetup += id => SpawnFormationSetup?.Invoke(id);
            _eventEditor.SpawnFormationChanged += id => SpawnFormationChanged?.Invoke(id);

            _eventSaver = eventSaver;
            _levelSaver = levelSaver;
            _characterManagerConfig = characterManagerConfig;

            _previewManager = previewManager;
            _previewManager.PlayingEventSwitched += () => PlayingEventSwitched?.Invoke();
            _assetManager.StartUpdatingAsset += (type, id) => StartUpdatingAsset?.Invoke(type, id);
            _assetManager.StopUpdatingAsset += (type, id) => StopUpdatingAsset?.Invoke(type, id);
            _assetManager.AssetLoaded += x=> AssetLoaded?.Invoke(x);
            _assetManager.AssetLoadingCancelled += x=> AssetUpdateCancelled?.Invoke(x.GetModelType());
            _notUsedAssetsUnloader = notUsedAssetsUnloader;
            SpawnFormationChanged += x => SetupCameraOnDefaultPosition();
            TargetCharacterSequenceNumberChanged += SetupCameraOnDefaultPosition;
            _musicApplier = musicApplier;
            _characterViewContainer = characterViewContainer;
            InitializeLevelPlayer();
            _licensedMusicProvider.Initialize();
        }

        partial void InitializeRecorder(IVoiceRecorder voiceRecorder, EventRecorder eventRecorder);
        partial void InitializeFaceTracker();
        partial void InitializeLevelPlayer();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public Event GetLastEvent()
        {
            return CurrentLevel.GetLastEvent();
        }

        public void PrepareNewEventBasedOnTarget()
        {
            _eventEditor.ResetEditingEvent();
            SetTargetEventSequenceAsItWasNextInCurrentLevel();
            EditingCharacterSequenceNumber = _eventEditor.TargetCharacterSequenceNumber;
            SetupMusicForNewEvent();
            SetupRecordingMode();
            RefreshGroupFocusLevelEditorSettings();
            UnloadNotUsedByTargetEventAssets();
        }

        private void SetupMusicForNewEvent()
        {
            var ev = _eventEditor.TargetEvent;
            if (!ev.HasMusic()) return;
            var music = ev.GetMusic();
            var endCue = ev.GetMusicController().EndCue;
            var hasEnoughSoundForNextEvent = music.Duration > endCue + Constants.LevelDefaults.MIN_EVENT_DURATION_MS;
            if (!hasEnoughSoundForNextEvent)
            {
                ev.RemoveMusic();
            }
        }

        public async void CreateFreshEventBasedOnTemplate(TemplateInfo info, CharacterFullInfo[] characters)
        {
            var templateEvent = await GetTemplateEvent(info);
            var templateCharacterIds = templateEvent.GetUniqueCharacterIds();
            var replaceData = new ReplaceCharacterData[templateCharacterIds.Length];
            for (var i = 0; i < templateCharacterIds.Length; i++)
            {
                replaceData[i] = new ReplaceCharacterData(templateCharacterIds[i], characters[i]);
            }

            var templateArgs = new ApplyingTemplateArgs
            {
                Template = info,
                ReplaceCharactersData = replaceData
            };

            CreateFreshEventBasedOnTemplate(templateArgs);
        }

        public void UnlinkEventFromTemplate()
        {
            TargetEvent.TemplateId = null;
        }

        public async void CreateFreshEventBasedOnTemplate(ApplyingTemplateArgs args)
        {
            TemplateApplyingStarted?.Invoke();
            
            if (args.OnEventSetupCallback == null)
            {
                args.OnEventSetupCallback = OnEventSetup;
            }
            else
            {
                args.OnEventSetupCallback += OnEventSetup;
            }
            
            async void OnEventSetup()
            {
                UnloadNotUsedByTargetEventAssets();
                var templatesEvent = await _templateProvider.GetTemplateEvent(args.Template.Id);
                SetupFaceTrackingBasedOnTemplate(templatesEvent);
                SetupRecordingMode();
                EditingCharacterSequenceNumber = _eventEditor.TargetCharacterSequenceNumber;
                TemplateApplyingCompleted?.Invoke();
                TemplateApplied?.Invoke(args);
            }
            
            await _eventEditor.CreateFreshEventBasedOnTemplate(args);
            SetTargetEventSequenceAsItWasNextInCurrentLevel();
        }

        public void SetTargetEvent(Event @event, Action onEventLoaded, PlayMode playMode)
        {
            //todo: check why those 2 methods behave exact the same
            PlayEvent(playMode, @event, onEventLoaded);
        }

        public void RefreshTargetEventAssets()
        {
            PlayEvent(PlayMode.StayOnFirstFrame, TargetEvent);
        }

        public void Initialize(MetadataStartPack metadataStartPack)
        {
            _metadataStartPack = metadataStartPack;
            _cameraSettingProvider.Initialize(metadataStartPack.SpawnPositionSpaceSizes);
        }

        public void CancelLoading()
        {
            _eventLoadingCancellationSource?.Cancel();
            CleanUp();
        }

        public void ClearTempFiles()
        {
            LocalStorageManager.DeleteFiles();
            _tempFileManager.RemoveTempFile(Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
            _cameraSystem.ClearCachedFilesAsync();
            _bridge.CleanCacheFromConvertedFilesAsync();
        }

        public void ReplaceEvent(Event @event)
        {
            var newEvents = new List<Event>(CurrentLevel.Event);
            for (var i = 0; i < CurrentLevel.Event.Count; i++)
            {
                var isTargetEvent = CurrentLevel.Event.ElementAt(i).Id == @event.Id;
                if (isTargetEvent)
                {
                    newEvents[i] = @event;
                }
                else
                {
                    newEvents[i] = CurrentLevel.Event.ElementAt(i);
                }
            }

            CurrentLevel.Event = newEvents;
        }

        public async Task DeleteLastEvent()
        {
            IsDeletingEvent = true;
            EventDeletionStarted?.Invoke();
            var deletedCameraAnim = await GetDeletingCameraAnimationClip();
            var previousEvent = _eventDeleter.DeleteLastEvent(CurrentLevel.Event);
            OnEventDeleted(previousEvent, deletedCameraAnim);
            CreateNewLevelIfNeeded();
        }

        public void DeleteEvent(long eventId)
        {
            EventDeletionStarted?.Invoke();
            _eventDeleter.DeleteEvent(eventId, _currentLevel);
            EventDeleted?.Invoke();
        }

        public Level CreateLevelInstance()
        {
            return new Level
            {
                LevelTemplateId = 1,
                GroupId = _bridge.Profile.GroupId,
                OriginalGroupId = _bridge.Profile.GroupId,
                LanguageId = 1,
                VerticalCategoryId = 1,
                Event = new List<Event>()
            };
        }

        public void SaveLevel(Level level, Action<Level> onSaved, Action<string> onFailure)
        {
            _levelSaver.SaveLevel(level, onSaved, onFailure);
        }

        public void SaveRecordedEvent()
        {
            _activationCueManager.SetupEndCues(TargetEvent);
            
            _eventSaver.SaveEvent(CurrentLevel, TargetEvent, CurrentRecordingMode);
            OnRecordedEventSaved();
            _tempFileManager.SaveDataLocally(CurrentLevel, Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
        }
        
        public void SaveEditingEvent()
        {
            _tempFileManager.SaveDataLocally(CurrentLevel, Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
            EventSaved?.Invoke();
        }

        public void SetVoiceVolume(int volume)
        {
            if (TargetEvent == null) return;

            foreach (var characterController in TargetEvent.CharacterController)
            {
                var faceVoiceController = characterController?.CharacterControllerFaceVoice.FirstOrDefault();
                if (faceVoiceController == null) continue;

                faceVoiceController.VoiceSoundVolume = volume;
            }
            
            _previewManager.RefreshPlayers(TargetEvent, DbModelType.VoiceTrack);
        }

        public void SetMusicVolume(int volume)
        {
            var musicController = TargetEvent?.GetMusicController();
            if (musicController != null)
                musicController.LevelSoundVolume = volume;
            _previewManager.RefreshPlayers(TargetEvent, DbModelType.Song, DbModelType.UserSound);
        }
        
        public float GetMusicVolume()
        {
            var musicController = TargetEvent?.GetMusicController();
            return musicController?.LevelSoundVolume.ToHecto() ?? Constants.LevelDefaults.AUDIO_SOURCE_VOLUME_DEFAULT;
        }

        public int GetMusicActivationCue()
        {
            return _activationCueManager.GetMusicActivationCue(TargetEvent, CurrentLevel);
        }

        public void ApplySongStartingFromTargetEvent(IPlayableMusic playableMusic, int activationCue, Action onApplied)
        {
            string reason = null;
            if (playableMusic != null && !CanUseForReplacing(playableMusic, activationCue, ref reason))
            {
                throw new InvalidOperationException(reason);
            }
            _musicApplier.ApplyMusicToLevel(CurrentLevel, playableMusic, TargetEvent.Id, activationCue);
            if (playableMusic != null)
            {         
                _assetManager.Load(playableMusic, onCompleted: x=>onApplied?.Invoke());
            }
        }

        public bool CanUseForRecording(IPlayableMusic song, ref string reason)
        {
            if (!song.IsLicensed()) return true;
            return _licensedMusicManager.CanUseForNewRecording(CurrentLevel, song.Id, ref reason);
        }

        public float GetAllowedDurationForNextRecordingInSec(long externalSongId)
        {
            var activationCue = GetMusicActivationCue();
            return _licensedMusicManager.GetAllowedDurationForNextRecordingInSec(CurrentLevel, externalSongId, activationCue);
        }

        public bool CanUseForReplacing(IPlayableMusic song, int activationCue, ref string reason)
        {
            if (!song.IsLicensed()) return true;
            var canUseForReplacingInTargetEvent = _licensedMusicManager.CanUseForReplacing(CurrentLevel, TargetEvent.Id, song.Id, activationCue, TargetEvent.GetExternalTrackId(), ref reason);
            if (!canUseForReplacingInTargetEvent) return false;
            
            var licensedSongsAfterReplacingCount = _musicApplier.GetExternalTracksCountAfterReplacing(CurrentLevel, song as ExternalTrackInfo, TargetEvent.Id,
                                                               activationCue);
            var isCountInLimitRange = licensedSongsAfterReplacingCount <= _licensedMusicManager.MusicCountPerLevelLimit;
            if (!isCountInLimitRange)
            {
                reason = $"Can't change song. Video cannot contain more than {_licensedMusicManager.MusicCountPerLevelLimit} licensed songs";
            }
            return isCountInLimitRange;
        }

        public void ChangeSong(IPlayableMusic nextAudio, int activationCue = 0, Action<IAsset> onCompleted = null)
        {
            _musicApplier.ApplyMusicToEvent(TargetEvent, nextAudio, activationCue);
            if (nextAudio == null)
            {
                onCompleted?.Invoke(null);
                return;
            }

            _eventEditor.Change(nextAudio, onCompleted);

            SongChanged?.Invoke();
        }

        public void ChangeRecordingMode(RecordingMode mode)
        {
            CurrentRecordingMode = mode;
        }

        public void ChangeCameraAnimation(CameraAnimationFullInfo next, string animationString)
        {
            _eventEditor.ChangeCameraAnimation(next, animationString);
        }

        public void ChangeBodyAnimation(BodyAnimationInfo bodyAnimation, Action callback)
        {
            void OnChanged()
            {
                RefreshAssetsOnScene(DbModelType.BodyAnimation);
                callback?.Invoke();
                BodyAnimationChanged?.Invoke();
            }

            _eventEditor.ChangeBodyAnimation(bodyAnimation, OnChanged);
        }

        public void ApplyFormation(CharacterSpawnPositionFormation formation)
        {
            ApplyFormationInternal(formation);
            LogAmplitudeCharacterFormationChanged(formation.Id);
        }

        public void ChangeCameraFilter(CameraFilterInfo cameraFilter, long variantId, Action<IAsset> onCompleted = null)
        {
            _eventEditor.ChangeCameraFilter(cameraFilter, variantId, onCompleted);
        }

        public void RemoveCameraFilter(Action onRemoved)
        {
            _eventEditor.RemoveCameraFilter(onRemoved);
        }

        public void ApplySetLocationBackground(PhotoFullInfo photo)
        {
            _eventEditor.ApplySetLocationBackground(photo, () =>
            {
                RefreshAssetsOnScene(DbModelType.UserPhoto);
                PhotoOnSetLocationChanged?.Invoke();
            });
        }

        public void ApplySetLocationBackground(SetLocationBackground background)
        {
            _eventEditor.ApplySetLocationBackground(background, () => RefreshAssetsOnScene(DbModelType.SetLocationBackground));
        }

        public void ApplySetLocationBackground(VideoClipFullInfo videoClip)
        {
            _eventEditor.ApplySetLocationBackground(videoClip);
        }

        public void ResetSetLocationBackground()
        {
            _eventEditor.ResetSetLocationBackground();
        }

        public void ApplyRenderTextureToSetLocationCamera(RenderTexture renderTexture)
        {
            var camera = GetActiveCamera();
            camera.targetTexture = renderTexture;
            camera.ApplyAspectRatioFromRenderTextureImmediate();
            RefreshAssetsOnScene(DbModelType.Caption);
        }

        public void AddCaption(CaptionFullInfo caption)
        {
            TargetEvent.Caption.Add(caption);
            _assetManager.Load(caption,asset =>
            {
                var setLocation = GetTargetEventSetLocationAsset();
                setLocation.Attach(TargetEvent.CharacterSpawnPositionId, asset as ICaptionAsset);
                RefreshAssetsOnScene(DbModelType.Caption);
            });
        }

        public void RemoveCaption(long captionId)
        {
            var captionToRemove = TargetEvent.Caption?.FirstOrDefault(x => x.Id == captionId);
            if (captionToRemove != null)
            {
                TargetEvent.Caption.Remove(captionToRemove);
                RefreshAssetsOnScene(DbModelType.Caption);
            }
        }

        public void RefreshCaption(CaptionFullInfo caption)
        {
            var captionAsset = _assetManager.GetAllLoadedAssets(DbModelType.Caption).FirstOrDefault(x => x.Id == caption.Id) as ICaptionAsset;
            captionAsset?.ForceRefresh();
        }

        public void SwapCharacters()
        {
            SwapSequenceNumbers();
            
            _eventEditor.RefreshCharactersOnSpawnPosition();
            RefreshCameraFocusTargetGameObject();
            UpdateFaceManagerSettings();
            LogAmplitudeSwapCharacters();
            CharactersPositionsSwapped?.Invoke();
        }

        public async void Shuffle(ShuffleModel shuffleModel, Action onCompleted)
        {
            IsShuffling = true;
            ShufflingBegun?.Invoke();
            
            var setup = await GetShufflingSetup(shuffleModel.Assets, TargetEvent);
            if (setup == null)
            {
                IsShuffling = false;
                ShufflingFailed?.Invoke();
                return;
            }
            
            _eventEditor.Shuffle(shuffleModel.Assets, setup.Events.First(), setup.SetLocations, setup.BodyAnimations, OnCompleted);
            
            void OnCompleted(ISetLocationAsset asset, DbModelType[] otherChangedAssetType)
            {
                LogShuffleEvent(AmplitudeEventConstants.EventNames.SHUFFLE_BUTTON);
                
                RefreshAssetsOnScene(DbModelType.SetLocation);
                if (!otherChangedAssetType.IsNullOrEmpty())
                {
                    foreach (var assetType in otherChangedAssetType)
                    {
                        RefreshAssetsOnScene(assetType);
                    }
                }

                IsShuffling = false;
                onCompleted?.Invoke();
                ShufflingDone?.Invoke();
            }
        }
        
        public async void ShuffleAI(ShuffleModel shuffleModel, Action onCompleted)
        {
            var prompt = ((AIShuffleModel)shuffleModel).Prompt;

            if (string.IsNullOrEmpty(prompt))
            {
                Shuffle(shuffleModel, onCompleted);
                return;
            }
            
            IsShuffling = true;
            ShufflingBegun?.Invoke();
            
            var setup = await GetShufflingSetupAI(shuffleModel.Assets, prompt, TargetEvent);
            
            if (setup == null)
            {
                IsShuffling = false;
                ShufflingFailed?.Invoke();
                return;
            }

            _eventEditor.Shuffle(shuffleModel.Assets, setup.Events.First(), setup.SetLocations, setup.BodyAnimations, OnCompleted);
            
            void OnCompleted(ISetLocationAsset asset, DbModelType[] otherChangedAssetType)
            {
                LogShuffleEvent(AmplitudeEventConstants.EventNames.AI_GENERATE_BUTTON, prompt);
                
                RefreshAssetsOnScene(DbModelType.SetLocation);
                if (!otherChangedAssetType.IsNullOrEmpty())
                {
                    foreach (var assetType in otherChangedAssetType)
                    {
                        RefreshAssetsOnScene(assetType);
                    }
                }

                IsShuffling = false;
                onCompleted?.Invoke();
                ShufflingDone?.Invoke();
            }
        }
        
        private InputEventInfo FormInputEventInfo(Event @event)
        {
            var inputEventInfo = new InputEventInfo
            {
                Id = @event.Id,
                SetLocationId = @event.GetSetLocationId(),
                Characters = @event.CharacterController.Select(FormCharacterInputInfo).ToArray()
            };

            return inputEventInfo;

            CharacterInputInfo FormCharacterInputInfo(CharacterController characterController)
            {
                var characterInputInfo = new CharacterInputInfo
                {
                    CharacterId = characterController.CharacterId,
                    CharacterSpawnPositionId = characterController.CharacterSpawnPositionId,
                    BodyAnimationId = characterController.GetBodyAnimationId()
                };

                return characterInputInfo;
            }
        }

        public void CleanUp()
        {
            _previewManager.CleanUp();
            _cameraSystem.CleanUp();
            _cameraSystem.Enable(false);
            _eventEditor.Cleanup();
            _previousEventLastCameraFrameProvider.ClearCache();
            _cameraAnimationCancellationSource = null;
        }

        public void ReplaceCharacter(CharacterFullInfo oldCharacter, CharacterFullInfo newCharacter, bool unloadOld,
                                     Action<ICharacterAsset> onSuccess)
        {
            IsReplacingCharacter = true;
            CharacterReplacementStarted?.Invoke(oldCharacter, newCharacter);

            var characterController = TargetEvent.GetCharacterControllerByCharacterId(oldCharacter.Id);
            characterController.SetOutfit(null);

            _eventEditor.ReplaceCharacter(oldCharacter, newCharacter, unloadOld, asset =>
            {
                IsReplacingCharacter = false;
                onSuccess?.Invoke(asset);
                CharacterReplaced?.Invoke(asset);
            });
        }

        public void SpawnCharacter(CharacterFullInfo character, Action<ICharacterAsset> onSuccess)
        {
            CharacterSpawnStarted?.Invoke();
            _eventEditor.SpawnCharacter(character, asset =>
            {
                if (TargetEvent.CharactersCount() > 1)
                {
                    TargetCharacterSequenceNumber = -1;
                    EditingCharacterSequenceNumber = -1;
                }
                
                RefreshAssetsOnScene(DbModelType.Character);
                UpdateFaceManagerSettings();
                onSuccess?.Invoke(asset);
                CharacterSpawned?.Invoke(asset);
            });
        }

        public void DestroyCharacter(CharacterFullInfo target, Action onSuccess = null)
        {
            _eventEditor.DestroyCharacter(target, () =>
            {
                TargetEvent.UpdateCharacterSequenceNumbers();
                _eventEditor.LastNonGroupTargetCharacterSequenceNumber = TargetEvent.TargetCharacterSequenceNumber;
                RefreshAssetsOnScene(DbModelType.Character);
                RefreshAssetsOnScene(DbModelType.BodyAnimation);
                onSuccess?.Invoke();
                UpdateFaceManagerSettings();
                CharacterDestroyed?.Invoke();
            });
        }

        public void UnloadFaceAndVoice()
        {
            _eventEditor.UnloadFaceAndVoice();
            RefreshAssetsOnScene(DbModelType.FaceAnimation);
            RefreshAssetsOnScene(DbModelType.VoiceTrack);
        }

        public void UnloadNotUsedByLevelAssets(Level level = null)
        {
            var targetLevel = level ?? _currentLevel;
            _notUsedAssetsUnloader.UnloadNotUsedAssets(targetLevel.Event.ToArray());
        }
        
        public void UnloadNotUsedByTargetEventAssets()
        {
            _notUsedAssetsUnloader.UnloadNotUsedAssets(TargetEvent);
        }

        public void UnloadNotUsedByEventsAssets(params Event[] events)
        {
            _notUsedAssetsUnloader.UnloadNotUsedAssets(events);
        }

        public void UnloadAllAssets()
        {
            _assetManager.UnloadAll();
        }

        public void UnloadAsset(IAsset asset)
        {
            _assetManager.Unload(asset);
        }

        public void UnloadAllAssets(params IAsset[] except)
        {
            _assetManager.UnloadAllExceptFor(except);
        }

        public void DeactivateAllAssets()
        {
            _assetManager.DeactivateAll();
        }

        public void CancelLoadingCurrentAssets()
        {
            _assetManager.CancelLoadingCurrentAssets();
        }

        public void Change<T>(T next, Action<IAsset> onCompleted = null, Action onCancelled = null, bool unloadPrevious = true, long subAssetId = -1L) where T : class, IEntity
        {
            Action<IAsset> callback = asset =>
            {
                RefreshAssetsOnScene(next.GetModelType());
            };
            if (onCompleted != null)
            {
                callback += onCompleted;
            }
            
            _eventEditor.Change(next, callback, onCancelled, unloadPrevious, subAssetId);
        }

        public void ChangeSetLocation(SetLocationFullInfo setLocation, Action<ISetLocationAsset, DbModelType[]> onCompleted = null,
            Action onCancelled = null, long? spawnPositionId = null, bool allowChangingAnimations = true, bool recenterCamera = false)
        {
            SetLocationChanged callback = (asset, otherChangedAssetType) =>
            {
                RefreshAssetsOnScene(DbModelType.SetLocation);
                if (!otherChangedAssetType.IsNullOrEmpty())
                {
                    foreach (var assetType in otherChangedAssetType)
                    {
                        RefreshAssetsOnScene(assetType);
                    }
                }
                
                OnSetLocationChangeFinished(asset);
            };
            if (onCompleted != null)
            {
                callback = ((x,y) => onCompleted(x, y)) + callback;
            }

            _eventEditor.ChangeSetLocation(setLocation, spawnPositionId, callback, allowChangingAnimations, recenterCamera);
        }
        
        public void ChangeCharacterSpawnPosition(CharacterSpawnPositionInfo spawnPosition, bool allowChangingAnimations)
        {
            _eventEditor.ChangeCharacterSpawnPosition(spawnPosition, allowChangingAnimations,otherLoadedAssetTypes =>
            {
                RefreshAssetsOnScene(DbModelType.SetLocation);
                foreach (var assetType in otherLoadedAssetTypes)
                {
                    RefreshAssetsOnScene(assetType);
                }
                SpawnPositionChanged?.Invoke();
            });
        }

        public async Task<bool> IsLevelModified(Level original, Level current)
        {
            var comparisonTask = Task.Run(() =>
            {
                var diff = new DifferenceDeepUpdateReq<Level>(original, current);
                diff.CompareObjects();
                return diff.HasDataToUpdate;
            });

            return await comparisonTask;
        }
        
        public void SetFilterValue(float value)
        {
            _eventEditor.SetFilterValue(value);
        }

        public void SaveDayNightControllerValues()
        {
            var setLocation = GetTargetEventSetLocationAsset();
          
            if(!setLocation.IsDayTimeControlSupported) return;

            var dayNightController = setLocation.DayNightController;
            var timeOfDay = dayNightController.Time.ToMilliseconds();
            var timelapseSpeed = dayNightController.Speed.ToMilli();
            
            var setLocationController = TargetEvent.GetSetLocationController();
            setLocationController.TimeOfDay = timeOfDay;
            setLocationController.TimelapseSpeed = timelapseSpeed;
        }

        public void RemoveAllVfx()
        {
            if(TargetEvent.GetVfxController() == null) return;
            TargetEvent.RemoveVfx();

            var activeVfx = _assetManager.GetActiveAssets<IVfxAsset>();
            if (!activeVfx.Any()) return;

            foreach (var vfxAsset in activeVfx)
            {
                _assetManager.Unload(vfxAsset);
            }
        }

        public void PreventUnloadingUsedLicensedSongs()
        {
            _licensedMusicProvider.ClearAutoKeepInCacheRegister();
            
            IEnumerable<long> usedSongs = CurrentLevel.GetUsedLicensedSongsIds();
            if (TargetEvent.HasExternalTrack())
            {
                usedSongs = usedSongs.Append(TargetEvent.GetExternalTrackId().Value).Distinct();
            }
            var loadedSongsAssets = _assetManager.GetAllLoadedAssets(DbModelType.ExternalTrack);
            foreach (var usedSong in usedSongs)
            {
                if (_licensedMusicProvider.IsKeptByCache(usedSong)) continue;
                var loadedSongAsset = loadedSongsAssets.FirstOrDefault(x => x.Id == usedSong);
                if (loadedSongAsset == null)
                {
                    _licensedMusicProvider.KeepInCacheWhenLoaded(usedSong);
                    continue;
                }
                var audioClip = (loadedSongAsset as IExternalTrackAsset).Clip;
                _licensedMusicProvider.KeepClipInMemoryCache(usedSong, audioClip);
            }
        }

        public void ReleaseNotUsedLicensedSongs()
        {
            IEnumerable<long> usedSongs = CurrentLevel.GetUsedLicensedSongsIds();
            if (TargetEvent.HasExternalTrack())
            {
                usedSongs = usedSongs.Append(TargetEvent.GetExternalTrackId().Value).Distinct();
            }
            var cachedSongs = _licensedMusicProvider.KeptInMemoryClipIds;
            var notUsedCachedSongs = cachedSongs.Where(x => !usedSongs.Contains(x)).ToArray();
            UnloadExternalTrackAssets(notUsedCachedSongs);
            ReleaseAudioClipsFromInMemoryCache(notUsedCachedSongs);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
       
        private void CreateNewLevelIfNeeded()
        {
            if (CurrentLevel.Event.Count == 0)
            {
                CurrentLevel = CreateLevelInstance();
            }
        }
        
        private void OnEventDeleted(Event nextTargetEvent, RecordedCameraAnimationClip deletedCameraAnim)
        {
            PlayEvent(CurrentPlayMode, nextTargetEvent, OnSuccess);

            void OnSuccess()
            {
                IsDeletingEvent = false;
                UseSameFaceFx = TargetEvent.IsGroupFocus();
                var startFrame = deletedCameraAnim.GetFrame(0);
                _cameraTemplatesManager.SetStartFrameForTemplates(startFrame);
                _cameraSystem.Simulate(deletedCameraAnim, 0);
                TargetEvent.SetMusicEndCue(0);
                PrepareNewEventBasedOnTarget();
                UnloadNotUsedByTargetEventAssets();
                EventDeleted?.Invoke();
            }
        }
        
        private async Task<RecordedCameraAnimationClip> GetDeletingCameraAnimationClip()
        {
            _cameraAnimationCancellationSource?.Cancel();
            var cameraAnimModel = _currentLevel.Event.Last().GetCameraAnimation();
            var loaded = false;
            ICameraAnimationAsset cameraAnimationAsset = null;
            _cameraAnimationCancellationSource = new CancellationTokenSource();
            var loadArgs = new CameraAnimLoadArgs
            {
                DeactivateOnLoad = true,
                CancellationToken = _cameraAnimationCancellationSource.Token
            };
            _assetManager.Load(cameraAnimModel, loadArgs,
                               x =>
                               {
                                   _cameraAnimationCancellationSource = null;
                                   cameraAnimationAsset = x as ICameraAnimationAsset;
                                   loaded = true;
                               }, 
                               message =>
                               {
                                   _cameraAnimationCancellationSource = null;
                                   Debug.LogWarning(message);
                               });

            while (!loaded)
            {
                await Task.Delay(100);
            }

            return cameraAnimationAsset.Clip;
        }

        private void UpdateLevel()
        {
            CurrentLevel = _eventSaver.CurrentLevel;
        }

        private void PlayerNeedsToCenterFaceStart()
        {
            RequestPlayerCenterFaceStarted?.Invoke();
        }

        private void OnPlayerNeedsToCenterFaceFinished()
        {
            RequestPlayerCenterFaceFinished?.Invoke();
        }
        
        private void PlayerNeedsBetterLightingStarted()
        {
            RequestPlayerNeedsBetterLightingStarted?.Invoke();
        }

        private void OnPlayerNeedsBetterLightingFinished()
        {
            RequestPlayerNeedsBetterLightingFinished?.Invoke();
        }

        private void OnAssetStartedUpdating(DbModelType type, long id)
        {
            AssetUpdateStarted?.Invoke(type, id);
        }
        
        private void OnAssetUpdated(DbModelType type)
        {
            AssetUpdateCompleted?.Invoke(type);
        }

        private void OnAssetUpdatingFailed(DbModelType type)
        {
            AssetUpdateFailed?.Invoke(type);
        }

        private float CalculateEventsDuration()
        {
            return CurrentLevel.GetEventsDurationSec();
        }

        private void OnRecordedEventSaved()
        {
            UnloadFaceAndVoice();
            UpdateLevel();
            LogAmplitudeEventSaved();
            PrepareNewEventBasedOnTarget();
            EventSaved?.Invoke();
        }

        private void OnTargetEventChanged(Event @event)
        {
            TargetEventChanged?.Invoke(@event);
        }
        
        private void OnEventLoadingStarted()
        {
            EventLoadingStarted?.Invoke();
        }

        private void OnEventLoadingComplete()
        {
            RefreshFaceTrackingTarget();
            EventLoadingCompleted?.Invoke();
        }

        private void OnSetLocationChangeFinished(ISetLocationAsset setLocation)
        {
            SetLocationChangeFinished?.Invoke(setLocation);
        }

        private void OnCharactersOutfitsUpdated()
        {
            UnloadNotUsedByTargetEventOutfits();
            CharactersOutfitsUpdated?.Invoke();
        }

        private void OnCharacterOutfitUpdatingBegan()
        {
            CharactersOutfitsUpdatingBegan?.Invoke();
        }

        private void SwapSequenceNumbers()
        {
            var characterControllers = TargetEvent.GetOrderedCharacterControllers();
            var sequenceNumbers = characterControllers.Select(x => x.ControllerSequenceNumber).ToArray();
            var spawnPositions = characterControllers.Select(x => x.CharacterSpawnPositionId).ToArray();
            for (var i = 0; i < characterControllers.Length; i++)
            {
                var cc = characterControllers[i];
                cc.ControllerSequenceNumber = i < characterControllers.Length - 1
                    ? sequenceNumbers[i + 1]
                    : sequenceNumbers[0];
                cc.CharacterSpawnPositionId = i < characterControllers.Length - 1
                    ? spawnPositions[i + 1]
                    : spawnPositions[0];
            }

            TargetEvent.CharacterController = characterControllers.OrderBy(x => x.ControllerSequenceNumber).ToList();
        }

        private void OrderEventsByLevelSequence()
        {
            if (CurrentLevel.Event == null) return;
            CurrentLevel.Event = CurrentLevel.Event.OrderBy(x => x.LevelSequence).ToList();
        }

        private void UnloadNotUsedByTargetEventOutfits()
        {
            _notUsedAssetsUnloader.UnloadNotUsedOutfits(TargetEvent);
        }

        private void SetEditingCharacterSequenceNumberSilent(int number)
        {
            _eventEditor.EditingCharacterSequenceNumber = number;
        }

        private ICharacterAsset GetTargetCharacterOrFirstLoaded()
        {
            if (TargetCharacterAsset != null) return TargetCharacterAsset;
            var characterAssets = _assetManager.GetAllLoadedAssets<ICharacterAsset>();
            return characterAssets.FirstOrDefault();
        }

        private void SetupFaceTrackingBasedOnTemplate(Event template)
        {
            var hasFaceAnim = template.GetFaceAnimations().Any();
            UseSameFaceFx = hasFaceAnim && template.IsGroupFocus();
            
            var hasCameraPermission = _permissionsService.HasPermission(PermissionTarget.Camera);
            SetFaceRecording(hasCameraPermission && hasFaceAnim);
        }

        private void RefreshFaceTrackingOnCharacters(ARFace arFace)
        {
            DeactivateFaceTrackingOnAllCharacters();

            if (IsFaceRecordingEnabled && IsFaceTrackingEnabled)
            {
                EnableFaceTrackingOnTargetCharacters(arFace);
            }
        }

        private void ActivateFaceTrackingOnAllCharacters(ARFace arFace)
        {
            foreach (var character in GetCurrentCharactersAssets())
            {
                character.StartMirroringTrackedUserFace(arFace);
                // Doing every character seems odd...  since we only have one face... so are we calling Swift code under the hood for each character?
                character.RequestPlayerCenterFaceStarted += PlayerNeedsToCenterFaceStart;
                character.RequestPlayerCenterFaceFinished += OnPlayerNeedsToCenterFaceFinished;
                character.RequestPlayerNeedsBetterLightingStarted += PlayerNeedsBetterLightingStarted;
                character.RequestPlayerNeedsBetterLightingFinished += OnPlayerNeedsBetterLightingFinished;
            }
        }

        private void DeactivateFaceTrackingOnAllCharacters()
        {
            foreach (var character in GetCurrentCharactersAssets())
            {
                character.StopMirroringTrackedUserFace();
                character.RequestPlayerCenterFaceStarted -= PlayerNeedsToCenterFaceStart;
                character.RequestPlayerCenterFaceFinished -= OnPlayerNeedsToCenterFaceFinished;
                character.RequestPlayerNeedsBetterLightingStarted -= PlayerNeedsBetterLightingStarted;
                character.RequestPlayerNeedsBetterLightingFinished -= OnPlayerNeedsBetterLightingFinished;
            }
        }

        private void EnableFaceTrackingOnTargetCharacters(ARFace arFace)
        {
            var hasLoadedCharacterAssets = _assetManager.GetAllLoadedAssets<ICharacterAsset>().Any();
            if (!hasLoadedCharacterAssets) return;

            if (UseSameFaceFx)
            {
                ActivateFaceTrackingOnAllCharacters(arFace);
            }
            else
            {
                var character = GetTargetCharacterOrFirstLoaded();
                character.StartMirroringTrackedUserFace(arFace);
                // Doing every character seems odd...  since we only have one face... so are we calling Swifit code under the hood for each cahracter?
                character.RequestPlayerCenterFaceStarted += PlayerNeedsToCenterFaceStart;
                character.RequestPlayerCenterFaceFinished += OnPlayerNeedsToCenterFaceFinished;
                character.RequestPlayerNeedsBetterLightingStarted += PlayerNeedsBetterLightingStarted;
                character.RequestPlayerNeedsBetterLightingFinished += OnPlayerNeedsBetterLightingFinished;
            }
        }
        
        private void SetupCameraOnDefaultPosition()
        {
            var formationId = TargetEvent.CharacterSpawnPositionFormationId;
            if (!formationId.HasValue) return;
            
            _cameraSpawnFormationControl.PutCameraOnDefaultPosition(formationId.Value, _eventEditor.TargetCharacterSequenceNumber);
        }

        private void SetTargetEventSequenceAsItWasNextInCurrentLevel()
        {
            var nextEventNumber = GetSequenceNumberForNextEvent();
            _eventEditor.SetLevelSequenceNumber(nextEventNumber);
        }
        
        private void RefreshGroupFocusLevelEditorSettings()
        {
            UseSameBodyAnimation = TargetEvent.IsGroupFocus();
            UseSameFaceFx = TargetEvent.IsGroupFocus();
            UpdateFaceManagerSettings();
        }

        private void LogAmplitudeEventSaved()
        {
            var eventCreatedMetaData =  new Dictionary<string, object>
            {
                {AmplitudeEventConstants.EventProperties.LEVEL_ID, CurrentLevel.Id}
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CREATE_EVENT, eventCreatedMetaData);
        }
        
        private void LogAmplitudeSwapCharacters()
        {
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CHARACTERS_SWAPPED_POSITION);
        }
        
        private void LogAmplitudeCharacterFormationChanged(long id)
        {
            var formationChangedMetaData =  new Dictionary<string, object> { {AmplitudeEventConstants.EventProperties.FORMATION_ID, id}};
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTERS_FORMATION_CHANGED, formationChangedMetaData);
        }

        private Task<Event> GetTemplateEvent(TemplateInfo template)
        {
            return _templateProvider.GetTemplateEvent(template.Id);
        }
        
        private void RefreshCameraFocusTargetGameObject()
        {
            _cameraFocusManager.UpdateCameraFocus(TargetEvent, false);
        }
        
        private async Task<LevelShuffleResult> GetShufflingSetup(ShuffleAssets assetTypesFlag, Event targetEvent)
        {
            var levelShuffleInput = new LevelShuffleInput
            {
                CharacterCount = targetEvent.GetCharactersCount(),
                ShuffleAssets = assetTypesFlag,
                Events = new [] { FormInputEventInfo(targetEvent) }
            };

            var result = await _bridge.GetShuffledLevel(levelShuffleInput);
            if (!result.IsError) return result.Model;
            
            Debug.LogError($"Failed to shuffle event. Reason: {result.ErrorMessage}");
            return null;
        }
        
        private async Task<LevelShuffleResult> GetShufflingSetupAI(ShuffleAssets assetTypesFlag, string prompt, Event targetEvent)
        {
            var levelShuffleInput = new LevelShuffleInputAI
            {
                CharacterCount = targetEvent.GetCharactersCount(),
                ShuffleAssets = assetTypesFlag,
                Events = new [] { FormInputEventInfo(targetEvent) },
                Text = prompt
            };

            var result = await _bridge.GetShuffledLevelAI(levelShuffleInput);
            if (!result.IsError) return result.Model;
            
            Debug.LogError($"Failed to AI shuffle event. Reason: {result.ErrorMessage}");
            return null;
        }

        private void LogShuffleEvent(string eventName, string prompt = null)
        {
            var targetEvent = _eventEditor.TargetEvent;
            
            var setId = targetEvent.SetLocationController.First().SetLocation.Id;
            var characterIds = new List<long>();
            var animationIds = new List<long>();
            var spawnIds = new List<long>();
            
            foreach (var characterController in targetEvent.CharacterController)
            {
                characterIds.Add(characterController.Character.Id);
                animationIds.Add(characterController.CharacterControllerBodyAnimation.First().BodyAnimationId);
                spawnIds.Add(characterController.CharacterSpawnPositionId);
            }

            var shuffleMetaData = new Dictionary<string, object>()
            {
                [AmplitudeEventConstants.EventProperties.SET_LOCATION_ID] = setId,
                [AmplitudeEventConstants.EventProperties.CHARACTER_IDS] = characterIds.ToArray(),
                [AmplitudeEventConstants.EventProperties.BODY_ANIMATION_IDS] = animationIds.ToArray(),
                [AmplitudeEventConstants.EventProperties.SPAWN_POSITION_IDS] = spawnIds.ToArray()
            };

            if (prompt != null)
            {
                shuffleMetaData.Add(AmplitudeEventConstants.EventProperties.PROMPT, prompt);
            }
            
            _amplitudeManager.LogEventWithEventProperties(eventName, shuffleMetaData);
        }
        
        private void UnloadExternalTrackAssets(IEnumerable<long> notUsedCachedSongs)
        {
            var externalTracks = _assetManager.GetAllLoadedAssets(DbModelType.ExternalTrack);
            foreach (var asset in externalTracks.Where(x => notUsedCachedSongs.Contains(x.Id)))
            {
                _assetManager.Unload(asset);
            }
        }
        
        private void ReleaseAudioClipsFromInMemoryCache(long[] notUsedCachedSongs)
        {
            foreach (var notUsedCachedSong in notUsedCachedSongs)
            {
                _licensedMusicProvider.RemoveFromInMemoryCache(notUsedCachedSong);
                _licensedMusicProvider.DontKeepAfterLoading(notUsedCachedSong);
            }
        }

        private void LinkNewModelsToAlreadyLoadedEditableAssets(Level level)
        {
            //currently only caption asset can be edit by user
            var captionModels = level.Event.Where(x => x.Caption != null).SelectMany(x => x.Caption).ToArray();
            var captionAssets = _assetManager.GetAllLoadedAssets<ICaptionAsset>();
            foreach (var asset in captionAssets)
            {
                var correspondedModel = captionModels.FirstOrDefault(x => x.Id == asset.Id);
                if (correspondedModel == null || ReferenceEquals(correspondedModel, asset.RepresentedModel)) continue;
                asset.RefreshModel(correspondedModel);
            }
        }
        
        private void ApplyFormationInternal(CharacterSpawnPositionFormation formation)
        {
            _eventEditor.ApplyCharacterSpawnFormation(formation);
        }

        public void SetupCameraFocusAnimationCurve(long? genderId = null)
        {
            if (!genderId.HasValue)
            {
                if (CurrentLevel.IsEmpty() && TargetEvent == null) return;
                genderId = !CurrentLevel.IsEmpty() ? CurrentLevel.GetFirstEvent().GetCharacters().First().GenderId : TargetEvent.GetCharacters().First().GenderId;
            }
            
            var race = _metadataStartPack.GetRaceByGenderId(genderId.Value);
            var animCurve = _cameraFocusAnimationCurveProvider.GetAnimationCurve(race.Id);
            _cameraSystem.SetFocusPointAdjustmentCurve(animCurve);
        }
    }
}