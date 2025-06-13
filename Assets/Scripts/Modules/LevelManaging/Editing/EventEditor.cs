using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraSystemCore;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UnityEngine;
using Zenject;
using CharacterController = Models.CharacterController;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using CharacterSpawnPositionFormation = Bridge.Models.ClientServer.StartPack.Metadata.CharacterSpawnPositionFormation;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Models.ClientServer.Level.Shuffle;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LocalStorage;


namespace Modules.LevelManaging.Editing
{
    /// <summary>
    /// This object provides all API for event/record editing
    /// </summary>
    [UsedImplicitly]
    internal sealed class EventEditor : IEventEditor
    {
        private readonly IAssetManager _assetManager;
        private readonly EventSetupAlgorithm _eventSetupAlgorithm;
        private readonly IVoiceFilterController _voiceFilterController;
        private readonly SetLocationChangingAlgorithm _setLocationChangingAlgorithm;
        private readonly BodyAnimationForNewCharacterLoader _bodyAnimationForNewCharacterLoader;
        private readonly CharacterSpawnPointChangingAlgorithm _spawnPositionChangingAlgorithm;
        private readonly VfxChanger _vfxChanger;
        private readonly CharactersChanger _charactersChanger;
        private readonly BodyAnimationChanger _bodyAnimationChanger;
        private readonly VoiceTrackChanger _voiceTrackChanger;
        private readonly FaceAnimationChanger _faceAnimationChanger;
        private readonly CameraAnimationChanger _cameraAnimationChanger;
        private readonly OutfitChanger _outfitChanger;
        private readonly AudioChanger _audioChanger;
        private readonly CameraFilterVariantChanger _cameraFilterVariantChanger;
        private readonly CharacterSpawnFormationChanger _characterSpawnFormationChanger;
        private readonly CameraAnimationTemplateChanger _cameraAnimationTemplateChanger;
        private readonly ITemplatesContainer _templatesContainer;
        private readonly BaseChanger[] _assetChangers;
        private readonly NotUsedAssetsUnloader _notUsedAssetsUnloader;
        private readonly CharacterRemovingAlgorithm _characterRemovingAlgorithm;
        private readonly DefaultBodyAnimationForSpawnPositionLoader _defaultBodyAnimationForSpawnPositionLoader;
        private readonly IBodyAnimationSelector<ChangingSpawnPositionContext> _bodyAnimationForSpawnPositionChangingSelector;
        private readonly IBodyAnimationSelector<ShuffleContext> _bodyAnimationForShuffleSelector;

        private readonly IEventTemplateManager _templateManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;
        private readonly ISpawnFormationProvider _spawnFormationProvider;
        private readonly IMetadataProvider _metadataProvider;

        private bool _useSameFaceFx;
        private readonly AvatarHelper _avatarHelper;
        private int _lastNonGroupTargetCharacterSequenceNumber;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<DbModelType> Updated;
        public event Action<DbModelType> AssetUpdatingFailed;
        public event Action<DbModelType, long> AssetStartedUpdating;
        public event Action<long?> SpawnFormationSetup;
        public event Action<long?> SpawnFormationChanged;
        
        public event Action<ISetLocationAsset> SetLocationChangeFinished;
        public event Action UseSameFaceFxChanged;
        public event Action EventLoadingStarted;
        public event Action EventLoadingComplete;
        public event Action CharactersOutfitsUpdatingBegan;
        public event Action<Event> TargetEventChanged;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Event TargetEvent { get; private set; }
        public bool IsChangingAsset { get; private set; }

        public bool IsChangingOutfit { get; private set; }

        public int TargetCharacterSequenceNumber
        {
            get
            {
                if (TargetEvent == null) return -1;
                return TargetEvent.TargetCharacterSequenceNumber;
            }
            set
            {
                if (TargetEvent == null) return;
                TargetEvent.TargetCharacterSequenceNumber = value;
                LastNonGroupTargetCharacterSequenceNumber = value;
            }
        }

        public int LastNonGroupTargetCharacterSequenceNumber
        {
            get => _lastNonGroupTargetCharacterSequenceNumber;
            set
            {
                if (value >= 0)
                {
                    _lastNonGroupTargetCharacterSequenceNumber = value;
                }
            }
        }

        public int EditingCharacterSequenceNumber { get; set; }
        
        private bool IsGroupSelected => EditingCharacterSequenceNumber == -1;

        public ICharacterAsset TargetCharacterAsset => TargetCharacterController == null ? null : GetCharacterAssetIfLoaded(TargetCharacterController.CharacterId);

        private ICharacterAsset LastNonGroupTargetCharacterAsset => LastNonGroupCharacterController == null ? null : GetCharacterAssetIfLoaded(LastNonGroupCharacterController.CharacterId);

        public CharacterController TargetCharacterController => TargetEvent.GetCharacterController(TargetCharacterSequenceNumber);

        private CharacterController LastNonGroupCharacterController => TargetEvent.GetCharacterController(LastNonGroupTargetCharacterSequenceNumber);

        private ICollection<CharacterController> TargetCharacterControllers
        {
            get
            {
                return IsGroupSelected
                    ? TargetEvent.CharacterController
                    : new[] {TargetEvent.GetCharacterController(EditingCharacterSequenceNumber)};
            }
        }
        
        public bool UseSameBodyAnimation { get; set; }

        public bool UseSameFaceFx
        {
            get => _useSameFaceFx;
            set
            {
                if(_useSameFaceFx == value) return;
                _useSameFaceFx = value;
                
                if (!_useSameFaceFx)
                {
                    UseSameFaceFxChanged?.Invoke();
                    return;
                }
                
                var targetCharacterController = TargetCharacterAsset == null
                    ? TargetEvent.GetTargetCharacterController()
                    : TargetEvent.GetCharacterController(TargetCharacterSequenceNumber);
                
                var targetFaceController = targetCharacterController.CharacterControllerFaceVoice.First();
                var targetAnimation = targetFaceController.FaceAnimation;
                var targetVoice = targetFaceController.VoiceTrack;
                ChangeFaceExpression(targetAnimation, targetVoice);
                UseSameFaceFxChanged?.Invoke();
            }
        }

        private CharacterController EditingCharacterController => TargetEvent?.GetCharacterController(EditingCharacterSequenceNumber);

        private ICharacterAsset EditingCharacterAsset => EditingCharacterController == null ? null : GetCharacterAssetIfLoaded(EditingCharacterController.CharacterId);

        private bool HasDefaultSpawnFormation => TargetEvent.CharacterSpawnPositionFormationId ==
                                                 _spawnFormationProvider.GetDefaultSpawnFormationId(TargetEvent);
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public EventEditor(IAssetManager assetManager, IVoiceFilterController voiceFilterController, 
            AvatarHelper avatarHelper, EventSetupAlgorithm eventSetupAlgorithm,
            CharacterSpawnFormationChanger characterSpawnFormationChanger, SetLocationChangingAlgorithm setLocationChangingAlgorithm, VfxChanger vfxChanger,
            AudioChanger audioChanger, OutfitChanger outfitChanger, CharactersChanger charactersChanger, CharacterSpawnPointChangingAlgorithm characterSpawnPointChangingAlgorithm,
            BodyAnimationChanger bodyAnimationChanger, VoiceTrackChanger voiceTrackChanger, FaceAnimationChanger faceAnimationChanger, 
            CameraFilterVariantChanger cameraFilterVariantChanger, CameraAnimationChanger cameraAnimationChanger, IEventTemplateManager templateManager, 
            ICameraSystem cameraSystem, ITemplatesContainer templatesContainer, ICameraTemplatesManager cameraTemplatesManager, ISpawnFormationProvider spawnFormationProvider,
            NotUsedAssetsUnloader notUsedAssetsUnloader, CameraAnimationTemplateChanger cameraAnimationTemplateChanger, 
            BodyAnimationForNewCharacterLoader bodyAnimationForNewCharacterLoader, CharacterRemovingAlgorithm characterRemovingAlgorithm,
            DefaultBodyAnimationForSpawnPositionLoader defaultBodyAnimationForSpawnPositionLoader, IBodyAnimationSelector<ChangingSpawnPositionContext> bodyAnimationForSpawnPositionChangingSelector,
            IBodyAnimationSelector<ShuffleContext> bodyAnimationForShuffleSelector, IMetadataProvider metadataProvider)
        {
            _assetManager = assetManager;
            _avatarHelper = avatarHelper;
            _voiceFilterController = voiceFilterController;
            _eventSetupAlgorithm = eventSetupAlgorithm;
            _templateManager = templateManager;
            _characterSpawnFormationChanger = characterSpawnFormationChanger;
            _setLocationChangingAlgorithm = setLocationChangingAlgorithm;
            _vfxChanger = vfxChanger;
            _audioChanger = audioChanger;
            _outfitChanger = outfitChanger;
            _charactersChanger = charactersChanger;
            _spawnPositionChangingAlgorithm = characterSpawnPointChangingAlgorithm;
            _bodyAnimationChanger = bodyAnimationChanger;
            _voiceTrackChanger = voiceTrackChanger;
            _faceAnimationChanger =  faceAnimationChanger;
            _cameraFilterVariantChanger = cameraFilterVariantChanger;
            _cameraAnimationChanger = cameraAnimationChanger;
            _cameraSystem = cameraSystem;
            _templatesContainer = templatesContainer;
            _cameraTemplatesManager = cameraTemplatesManager;
            _spawnFormationProvider = spawnFormationProvider;
            _notUsedAssetsUnloader = notUsedAssetsUnloader;
            _cameraAnimationTemplateChanger = cameraAnimationTemplateChanger;
            _bodyAnimationForNewCharacterLoader = bodyAnimationForNewCharacterLoader;
            _characterRemovingAlgorithm = characterRemovingAlgorithm;
            _defaultBodyAnimationForSpawnPositionLoader = defaultBodyAnimationForSpawnPositionLoader;
            _bodyAnimationForSpawnPositionChangingSelector = bodyAnimationForSpawnPositionChangingSelector;
            _bodyAnimationForShuffleSelector = bodyAnimationForShuffleSelector;
            _metadataProvider = metadataProvider;

            _assetChangers = new BaseChanger[]
            {
                _setLocationChangingAlgorithm, _spawnPositionChangingAlgorithm, _vfxChanger, _charactersChanger, _bodyAnimationChanger,
                _voiceTrackChanger, _faceAnimationChanger, _cameraAnimationChanger, _outfitChanger,
                _cameraFilterVariantChanger, _audioChanger, _cameraAnimationTemplateChanger
            };

            SubscribeAssetChangerEvents();

            _eventSetupAlgorithm.EventLoadingStarted += OnEventSetupStarted;
            _eventSetupAlgorithm.EventLoadingComplete += OnEventSetupComplete;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetTargetEvent(Event targetEvent, Action onCompleted, bool refocusCamera, CancellationToken cancellationToken = default)
        {
            TargetEvent = targetEvent;

            TargetEventChanged?.Invoke(TargetEvent);

            _eventSetupAlgorithm.SetupEvent(targetEvent, OnEventLoaded, refocusCamera);
            _setLocationChangingAlgorithm.Reset();
            
            void OnEventLoaded()
            {
                TargetCharacterSequenceNumber = targetEvent.TargetCharacterSequenceNumber;
                ClampEditingCharacterNumberToAvailableRange(targetEvent);
                onCompleted?.Invoke();
            }
        }

        public async Task CreateFreshEventBasedOnTemplate(ApplyingTemplateArgs args)
        {
            var templatesEvent = await _templatesContainer.GetTemplateEvent(args.Template.Id);
            
            var freshEvent = _templateManager.CreateFreshEventBasedOnTemplate(templatesEvent, args.ReplaceCharactersData);
            freshEvent.TemplateId = args.Template.Id;
            
            SetTargetEvent(freshEvent, OnEventAssetsLoaded, true);
           
            void OnEventAssetsLoaded()
            {
                SetCameraPositionFromTemplate(templatesEvent, args.OnEventSetupCallback);
            }
        }

        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        public void Change<T>(T next, Action<IAsset> onCompleted = null, Action onCancelled = null,
            bool unloadPrevious = true, long subAssetId = -1L) where T : class, IEntity
        {
            var modelType = next.GetModelType();
            switch (modelType)
            {
                case DbModelType.BodyAnimation:
                    throw new InvalidOperationException("Generic method is obsolete and does not support changing body animation. " +
                                                        $"Please use {nameof(IEventEditor.ChangeBodyAnimation)} instead");
                case DbModelType.CameraAnimation:
                    ChangeCameraAnimation(next as CameraAnimationFullInfo, onCompleted);
                    break;
                case DbModelType.CameraAnimationTemplate:
                    _cameraAnimationTemplateChanger.Run(next as CameraAnimationTemplate, onCompleted);
                    break;
                case DbModelType.SetLocation:
                    throw new InvalidOperationException("Generic method is obsolete and does not support changing set location. " +
                                                        $"Please use {nameof(IEventEditor.ChangeSetLocation)} instead");
                case DbModelType.CharacterSpawnPosition:
                    throw new InvalidOperationException("Generic method is obsolete and does not support changing spawn position. " +
                                                        $"Please use {nameof(IEventEditor.ChangeCharacterSpawnPosition)} instead");
                case DbModelType.Vfx:
                    ChangeVfx(next as VfxInfo, onCompleted);
                    break;
                case DbModelType.VoiceFilter:
                    ChangeVoiceFilter(next as VoiceFilterFullInfo, onCompleted, Debug.LogError);
                    break;
                case DbModelType.SpawnFormation:
                    throw new InvalidOperationException("Generic method is obsolete and does not support changing spawn formation. " +
                                                        $"Please use {nameof(IEventEditor.ApplyCharacterSpawnFormation)} instead");
                case DbModelType.Outfit:
                    throw new InvalidEnumArgumentException("Generic method is obsolete and does not support changing outfit. " +
                                                           $"Please use {nameof(IEventEditor.ChangeOutfit)} instead");
                case DbModelType.CameraFilterVariant:
                case DbModelType.CameraFilter:
                    throw new InvalidOperationException("Generic method is obsolete and does not support changing camera filter. " +
                                                        $"Please use {nameof(IEventEditor.ChangeCameraFilter)} instead");
                case DbModelType.Song:
                case DbModelType.UserSound:
                case DbModelType.ExternalTrack:
                    ChangeAudio(next as IPlayableMusic, onCompleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeSetLocation(SetLocationFullInfo nextLocation, long? nextSpawnPointId, SetLocationChanged onCompleted, bool allowChangingAnimations, bool recenterCamera)
        {
            var spawnPositions = nextLocation.CharacterSpawnPosition;
            if (!nextSpawnPointId.HasValue || spawnPositions.All(spawnPosition => spawnPosition.Id != nextSpawnPointId))
            {
                nextSpawnPointId = GetRecommendedSpawnPosition(nextLocation) ?? GetDefaultSpawnPosition(nextLocation);
            }
            
            _bodyAnimationForSpawnPositionChangingSelector.Setup(new ChangingSpawnPositionContext
            {
                Event = TargetEvent,
                PreviousSpawnPointSetLocation = TargetEvent.GetSetLocation(),
                NextSpawnPointSetLocation = nextLocation,
                NextSpawnPositionId = nextSpawnPointId.Value
            });
            
            ChangeSetLocationInternal(nextLocation, nextSpawnPointId, onCompleted, allowChangingAnimations, _bodyAnimationForSpawnPositionChangingSelector, recenterCamera);
        }

        public async void Shuffle(ShuffleAssets assetTypesFlag, EventShuffleResult shuffleResult, 
            SetLocationFullInfo[] setLocations, BodyAnimationInfo[] bodyAnimations, SetLocationChanged callback)
        {
            var setLocationShuffle = (assetTypesFlag & ShuffleAssets.SetLocation) == ShuffleAssets.SetLocation;
            var bodyAnimationShuffle = (assetTypesFlag & ShuffleAssets.BodyAnimation) == ShuffleAssets.BodyAnimation;
            
            _bodyAnimationForShuffleSelector.Setup(new ShuffleContext
            {
                ShuffleResult = shuffleResult,
                TargetEvent = TargetEvent
            });
            
            ISetLocationAsset readyLocationAsset = null;

            if (setLocationShuffle)
            {
                readyLocationAsset = await ShuffleSetLocation(shuffleResult, setLocations);
            }

            if (bodyAnimationShuffle)
            {
                await ShuffleBodyAnimations(shuffleResult, bodyAnimations);
            }

            var changedTypes = new List<DbModelType>();
            if (setLocationShuffle) changedTypes.Add(DbModelType.SetLocation);
            if (bodyAnimationShuffle) changedTypes.Add(DbModelType.BodyAnimation);
            
            callback?.Invoke(readyLocationAsset, changedTypes.ToArray());
        }

        public async void ChangeCharacterSpawnPosition(CharacterSpawnPositionInfo spawnPosition, bool allowChangingAnimations, Action<DbModelType[]> callback)
        {
            if (spawnPosition.Id == TargetEvent.CharacterSpawnPositionId)
            {
                callback?.Invoke(Array.Empty<DbModelType>());
                return;
            }

            var onChanged = new Action<DbModelType[]>(x =>
            {
                _notUsedAssetsUnloader.UnloadNotUsedAssets(TargetEvent);
                callback?.Invoke(x);
            });
            
            _bodyAnimationForSpawnPositionChangingSelector.Setup(new ChangingSpawnPositionContext
            {
                Event = TargetEvent,
                PreviousSpawnPointSetLocation = TargetEvent.GetSetLocation(),
                NextSpawnPointSetLocation = TargetEvent.GetSetLocation(),
                NextSpawnPositionId = spawnPosition.Id
            });
            
            _spawnPositionChangingAlgorithm.SetBodyAnimationSelector(_bodyAnimationForSpawnPositionChangingSelector);
            var currentSpawnPos = TargetEvent.GetSetLocation().GetSpawnPosition(TargetEvent.CharacterSpawnPositionId);
            var keepFormation = ShouldKeepSpawnFormationOnChangingSpawnPoint(currentSpawnPos, spawnPosition);
            await _spawnPositionChangingAlgorithm.RunAsync(spawnPosition, TargetEvent, allowChangingAnimations, keepFormation, onChanged);
            if (!keepFormation)
            {
                SetDefaultSpawnFormation();
            }
        }

        public async void ChangeBodyAnimation(BodyAnimationInfo bodyAnimation, Action callback)
        {
            try
            {
                await ChangeBodyAnimation(bodyAnimation, TargetCharacterControllers, callback);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void ChangeCameraFilter(CameraFilterInfo cameraFilter, long variantId, Action<IAsset> onCompleted = null)
        {
            if (cameraFilter == null)
            {
                TargetEvent.RemoveCameraFilter();
            }
            else
            {
                TargetEvent.SetCameraFilter(cameraFilter, variantId);
            }
            
            _cameraFilterVariantChanger.Run(cameraFilter, variantId, onCompleted);
        }

        public void RemoveCameraFilter(Action onRemoved)
        {
            _cameraFilterVariantChanger.Run(null, -1, x=>
            {
                TargetEvent.RemoveCameraFilter();
                onRemoved?.Invoke();
            });
        }

        public void Cleanup()
        {
            foreach (var assetChanger in _assetChangers)
            {
                assetChanger.CancelAll();
            }
            ResetEventLoadingCallback();
        }

        public void ResetEventLoadingCallback()
        {
            _eventSetupAlgorithm.ResetOnCompletedCallback();
        }

        public void ResetEditingEvent()
        {
            var sourceCopy = TargetEvent.Clone();
            sourceCopy.Id = 0;
            sourceCopy.LevelSequence = 0;
            sourceCopy.Files = null;

            // Character

            foreach (var character in sourceCopy.CharacterController)
            {
                character.Id = 0;
                character.EventId = 0;

                // Body Animation

                var bodyAnimationCollection = character.CharacterControllerBodyAnimation;
                var bodyAnimation = bodyAnimationCollection.FirstOrDefault();

                bodyAnimationCollection.Clear();

                if (bodyAnimation != null)
                {
                    bodyAnimation.Id = 0;
                    bodyAnimation.CharacterControllerId = 0;
                    bodyAnimation.ActivationCue = bodyAnimation.EndCue;
                    bodyAnimationCollection.Add(bodyAnimation);
                }

                // Face & Voice

                var faceVoiceCollection = character.CharacterControllerFaceVoice;
                var faceVoice = faceVoiceCollection.FirstOrDefault();
                faceVoiceCollection.Clear();
                if (faceVoice != null)
                {
                    faceVoice.Id = 0;
                    faceVoice.VoiceTrackId = null;
                    faceVoice.FaceAnimationId = null;
                    faceVoice.CharacterControllerId = 0;
                    faceVoice.FaceAnimation = null;
                    faceVoice.VoiceTrack = null;
                    faceVoiceCollection.Add(faceVoice);
                }
            }

            // Music

            var musicCollection = sourceCopy.MusicController;
            var music = musicCollection.FirstOrDefault();
            if (music != null)
            {
                music.Id = 0;
                music.EventId = 0;
                musicCollection.Add(music);
            }

            // SetLocation

            var setLocationCollection = sourceCopy.SetLocationController;
            var setLocation = setLocationCollection.FirstOrDefault();
            if (setLocation != null)
            {
                setLocation.Id = 0;
                setLocation.EventId = 0;
                setLocationCollection.Add(setLocation);
                setLocation.VideoActivationCue = setLocation.VideoEndCue;
            }

            // VFX

            var vfxCollection = sourceCopy.VfxController;
            var vfx = vfxCollection.FirstOrDefault();
            if (vfx != null)
            {
                vfx.Id = 0;
                vfx.EventId = 0;
                vfxCollection.Add(vfx);
            }
            
            // Camera
            
            var cameraController = sourceCopy.GetCameraController();
            if (cameraController != null)
            {
                cameraController.Id = 0;
                cameraController.EventId = 0;
                cameraController.CameraAnimation = null;
                cameraController.CameraAnimationId = 0;
                sourceCopy.SetCameraController(cameraController);
            }
            
            // Camera Filter

            var cameraFilterController = sourceCopy.GetCameraFilterController();
            if (cameraFilterController != null)
            {
                cameraFilterController.Id = 0;
            }

            //Caption

            var captions = sourceCopy.Caption;
            if (captions != null)
            {
                foreach (var caption in captions)
                {
                    caption.Id = LocalStorageManager.GetNextLocalId(nameof(CaptionFullInfo));
                }
            }

            SetTargetEvent(sourceCopy, null, false);
        }
        
        public void ReplaceCharacter(CharacterFullInfo oldCharacter, CharacterFullInfo newCharacter, bool unloadOld, Action<ICharacterAsset> onSuccess)
        {
            _charactersChanger.ReplaceCharacter(oldCharacter, newCharacter, TargetEvent, unloadOld, OnCharacterReplaced);

            void OnCharacterReplaced(ICharacterAsset characterAsset)
            {
                onSuccess?.Invoke(characterAsset);
            }
        }

        public async void SpawnCharacter(CharacterFullInfo character, Action<ICharacterAsset> onSuccess)
        {
            if (TargetEvent.GetCharactersCount() >= Constants.CHARACTERS_IN_EVENT_MAX) return;
            
            await _bodyAnimationForNewCharacterLoader.StartFetchingBodyAnimationForNewSpawnedCharacter(TargetEvent);
            _charactersChanger.SpawnCharacter(character, TargetEvent, OnCharacterSpawned);
            
            async void OnCharacterSpawned(ICharacterAsset characterAsset)
            {
                SetDefaultSpawnFormation();
                
                var controller = TargetEvent.GetCharacterControllerByCharacterId(character.Id);
                while (_bodyAnimationForNewCharacterLoader.IsLoading)
                {
                    await Task.Delay(33);
                }

                var unApplyMultiCharacterAnimation = TargetEvent.HasSetupMultiCharacterAnimation();
                if (unApplyMultiCharacterAnimation)
                {
                    await _defaultBodyAnimationForSpawnPositionLoader.ApplyDefaultBodyAnimationForAllCharacters(TargetEvent);
                }
                else
                {
                    var bodyAnim = _bodyAnimationForNewCharacterLoader.LoadedAnimation;
                    await ChangeBodyAnimation(bodyAnim, new[] { controller });
                }

                if (ShouldUseSpawnFormation())
                {
                    _characterSpawnFormationChanger.Run(TargetEvent.CharacterSpawnPositionFormationId, TargetEvent);
                }
                
                characterAsset.SetActive(true);
                onSuccess?.Invoke(characterAsset);
            }
        }

        private bool ShouldUseSpawnFormation()
        {
            var spawnPosition = TargetEvent.GetTargetSpawnPosition();
            if (!spawnPosition.HasGroup()) return true;
            if (!spawnPosition.AllowUsingSubSpawnPositions) return true;
            if (TargetEvent.GetCharactersCount() == 1) return false;
            return TargetEvent.CharacterController.HasSameSpawnPosition();
        }
        
        private void SetDefaultSpawnFormation()
        {
            TargetEvent.CharacterSpawnPositionFormationId = _spawnFormationProvider.GetDefaultSpawnFormationId(TargetEvent);
        }

        public void DestroyCharacter(CharacterFullInfo target, Action onSuccess)
        {
            _characterRemovingAlgorithm.Run(TargetEvent, target, onSuccess);
        }

        public void UnloadFaceAndVoice()
        {
            _assetManager.UnloadAll(DbModelType.FaceAnimation);
            _assetManager.UnloadAll(DbModelType.VoiceTrack);
        }

        public void SetFilterValue(float value)
        {
            var filterController = TargetEvent.GetCameraFilterController();
            if (filterController == null) return;
            filterController.CameraFilterValue = value.ToHecto();

            var assets = _assetManager.GetActiveAssets<ICameraFilterVariantAsset>();
            if (assets == null) return;

            var filterAsset = assets.FirstOrDefault();
            filterAsset?.SetIntensity(value);
        }

        public void SetLevelSequenceNumber(int number)
        {
            TargetEvent.LevelSequence = number;
        }

        public void ChangeCameraAnimation(CameraAnimationFullInfo next, string animationText)
        {
            var currentAnim = TargetEvent.GetCameraAnimation();
            _cameraAnimationChanger.Unload(currentAnim);
            _cameraAnimationChanger.Run(next, animationText);
        }

        public Task ChangeOutfit(OutfitFullInfo outfitFullInfo)
        {
            var activeCharacterAssets = _assetManager.GetActiveAssets<ICharacterAsset>();
            var characterAssets = EditingCharacterSequenceNumber < 0
                ? activeCharacterAssets
                : new[] { EditingCharacterAsset };

            return SetOutfitForCharacters(characterAssets, outfitFullInfo);
        }

        public Task RemoveOutfit()
        {
            return ChangeOutfit(null);
        }

        public void RefreshCharactersOnSpawnPosition()
        {
            if (TargetEvent.AreCharactersOnTheSameSpawnPosition())
            {
                var currentFormationId = TargetEvent.CharacterSpawnPositionFormationId;
                _characterSpawnFormationChanger.Run(currentFormationId, TargetEvent);
            }
            else
            {
                var setLocation = _assetManager.GetAllLoadedAssets<ISetLocationAsset>()
                                               .First(x => x.Id == TargetEvent.GetSetLocationId());
                var characterAssets = _assetManager.GetAllLoadedAssets<ICharacterAsset>();
                foreach (var cc in TargetEvent.CharacterController)
                {
                    var characterAsset = characterAssets.First(x => x.Id == cc.CharacterId);
                    var spawnPosition = setLocation.RepresentedModel.GetSpawnPosition(cc.CharacterSpawnPositionId);
                    setLocation.Attach(spawnPosition, characterAsset);
                }
            }
        }

        public void ApplySetLocationBackground(PhotoFullInfo photo, Action onApplied)
        {
            UnloadSetLocationBackgroundAssets();
            OnAssetStartedUpdating(photo.GetModelType(), photo.Id);
            
            TargetEvent.ResetSetLocationAttachedMedia();
            TargetEvent.SetPhoto(photo);

            _assetManager.Load(photo, asset =>
            {
                var player = GetSetLocationPhotoVideoPlayer();
                player.ShowTexture((asset as IPhotoAsset).Texture);
                onApplied?.Invoke();
                OnAssetUpdated(photo.GetModelType());
            });
        }

        public void ApplySetLocationBackground(SetLocationBackground background, Action onApplied)
        {
            UnloadSetLocationBackgroundAssets();
            OnAssetStartedUpdating(background.GetModelType(), background.Id);
            
            TargetEvent.ResetSetLocationAttachedMedia();
            TargetEvent.SetFreverBackground(background);

            _assetManager.Load(background, asset =>
            {
                var player = GetSetLocationPhotoVideoPlayer();
                player.ShowTexture((asset as ISetLocationBackgroundAsset).Texture);
                onApplied?.Invoke();
                OnAssetUpdated(background.GetModelType());
            });
        }

        public void ApplySetLocationBackground(VideoClipFullInfo videoClip)
        {
            UnloadSetLocationBackgroundAssets();
            OnAssetStartedUpdating(videoClip.GetModelType(), videoClip.Id);
            
            TargetEvent.ResetSetLocationAttachedMedia();
            TargetEvent.SetVideo(videoClip);
            
            var path =
                videoClip.Files.First().FilePath ??
                _assetManager.GetActiveAssets<IVideoClipAsset>()?.FirstOrDefault()?.FilePath;
                
            if (File.Exists(path))
            {
                var player = GetSetLocationPhotoVideoPlayer();
                player.PlayVideo(path);
            }
            else
            {
                Debug.LogError($"Failed applying set location background. Reason: video {videoClip.Id} does not exist at {path}");
            }
            
            OnAssetUpdated(videoClip.GetModelType());
        }

        public void ResetSetLocationBackground()
        {
            TargetEvent.ResetSetLocationAttachedMedia();

            var setLocation = TargetEvent.GetSetLocation();
            if (setLocation.AllowPhoto || setLocation.AllowVideo)
            {
                var player = GetSetLocationPhotoVideoPlayer();
                player.PlayDefaultVideo();
            }
        }
        
        public void ApplyCharacterSpawnFormation(CharacterSpawnPositionFormation nextFormation)
        {
            var isDifferent = TargetEvent.CharacterSpawnPositionFormationId != nextFormation.Id;
            var isAlreadyApplied = TargetEvent.CharacterController.GroupBy(x => x.CharacterSpawnPositionId).Count() == 1;
            if (!isDifferent && isAlreadyApplied) return;
            
            _characterSpawnFormationChanger.Run(nextFormation.Id, TargetEvent);
            
            TargetEvent.CharacterSpawnPositionFormationId = nextFormation.Id;
            OnSpawnFormationChanged();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ChangeVfx(VfxInfo nextVfx, Action<IAsset> onCompleted)
        {
            var vfxControllerModel = new VfxController();
            vfxControllerModel.SetVfx(nextVfx);
            TargetEvent.VfxController = new List<VfxController> {vfxControllerModel};
            _vfxChanger.Run(nextVfx, TargetEvent.CurrentCharacterSpawnPosition(), LastNonGroupTargetCharacterAsset, onCompleted);
        }

        private void ChangeCameraAnimation(CameraAnimationFullInfo nextCamera, Action<IAsset> onCompleted)
        {
            var current = TargetEvent.GetCameraAnimation();
            _cameraAnimationChanger.Unload(current);
            _cameraAnimationChanger.Run(nextCamera, onCompleted);
        }

        private void ChangeAudio(IPlayableMusic nextSong, Action<IAsset> onCompleted = null)
        {
            var previousAudio = _assetManager.GetActiveAssets<IAudioAsset>().FirstOrDefault();
            if (previousAudio?.Entity == nextSong)
            {
                onCompleted?.Invoke(previousAudio);
                return;
            }
            var musicController = TargetEvent.GetMusicController();
            var audioVolume = musicController?.LevelSoundVolume / 100f ?? Constants.LevelDefaults.AUDIO_SOURCE_VOLUME_DEFAULT;
            _audioChanger.Run(nextSong, previousAudio, audioVolume, onCompleted);
        }

        private async Task SetOutfitForCharacters(ICharacterAsset[] characterAssets, OutfitFullInfo outfit)
        {
            IsChangingOutfit = true;

            CharactersOutfitsUpdatingBegan?.Invoke();
            
            var tasks = new List<Task>();
            foreach (var characterAsset in characterAssets)
            {
                tasks.Add(_outfitChanger.Run(characterAsset, outfit));
            }

            await Task.WhenAll(tasks);

            var updatedCharactersIds = characterAssets.Select(x => x.Id).ToArray();
            var controllers = TargetEvent.CharacterController.GetControllersByCharacterId(updatedCharactersIds);
            controllers.SetOutfitForAllControllers(outfit);
            
            IsChangingOutfit = false;
            _avatarHelper.UnloadAllUmaBundles();
        }

        private void OnAssetStartedUpdating(DbModelType updated, long id)
        {
            IsChangingAsset = true;
            AssetStartedUpdating?.Invoke(updated, id);
        }
        
        private void OnAssetUpdated(DbModelType updated)
        {
            IsChangingAsset = false;
            Updated?.Invoke(updated);
        }
        
        private void OnAssetUpdatingFailed(DbModelType updated)
        {
            IsChangingAsset = false;
            AssetUpdatingFailed?.Invoke(updated);
        }

        private async Task ChangeBodyAnimation(BodyAnimationInfo animation, ICollection<CharacterController> characterControllers, Action onCompleted = null)
        {
            var args = new List<BodyAnimLoadArgs>
            {
                new()
                {
                    BodyAnimation = animation,
                    CharacterController = characterControllers
                }
            };
            var spawnFormationBefore = TargetEvent.CharacterSpawnPositionFormationId;
            await _bodyAnimationChanger.Run(TargetEvent, args);
            if (spawnFormationBefore != TargetEvent.CharacterSpawnPositionFormationId)
            {
                OnSpawnFormationChanged();
            }
            _notUsedAssetsUnloader.UnloadNotUsedAssets(DbModelType.BodyAnimation, TargetEvent);
            onCompleted?.Invoke();
        }

        private void ChangeFaceExpression(FaceAnimationFullInfo nextFaceAnim, VoiceTrackFullInfo voice)
        {
            CharacterController[] targetCharacterControllers = null;

            if (UseSameFaceFx)
            {
                targetCharacterControllers = TargetEvent.CharacterController.ToArray();
            }
            else
            {
                var targetCharacter = TargetEvent.GetTargetCharacterController();
                
                if (targetCharacter != null)
                {
                    targetCharacterControllers = new []{targetCharacter};
                }
            }

            if (targetCharacterControllers == null || targetCharacterControllers.Length == 0)
            {
                Debug.LogError("There is no character to apply new face expression");
                return;
            }
            
            foreach (var characterController in targetCharacterControllers)
            {
                var faceVoiceController = characterController.CharacterControllerFaceVoice.First();
                faceVoiceController.FaceAnimation = nextFaceAnim;
                faceVoiceController.FaceAnimationId = nextFaceAnim?.Id;
                faceVoiceController.VoiceTrack = null;
                faceVoiceController.VoiceTrackId = null;
            }
            
            var characterAssets = UseSameFaceFx ? _assetManager.GetActiveAssets<ICharacterAsset>() : new [] {TargetCharacterAsset};
            if (characterAssets.Any() && nextFaceAnim != null)
            {
                _faceAnimationChanger.Run(nextFaceAnim);
            }

            var voiceCharacterController = targetCharacterControllers.First();
            voiceCharacterController.CharacterControllerFaceVoice.First().VoiceTrack = voice;
            voiceCharacterController.CharacterControllerFaceVoice.First().VoiceTrackId = voice?.Id;

            if (voice != null)
            {
                _voiceTrackChanger?.Run(voice);
            }
        }

        private void ChangeVoiceFilter(VoiceFilterFullInfo voiceFilter, Action<IAsset> onCompleted, Action<string> onFail)
        {
            void OnSuccess()
            {
                onCompleted?.Invoke(null);
            }

            _voiceFilterController.Change(voiceFilter, OnSuccess, onFail);
            
            foreach (var controller in TargetEvent.CharacterController)
            {
                var characterFaceVoice = controller.CharacterControllerFaceVoice.First();
                characterFaceVoice.VoiceFilterId = voiceFilter.Id;
                characterFaceVoice.VoiceFilter = voiceFilter;
            }
        }

        private void OnEventSetupStarted()
        {
            EventLoadingStarted?.Invoke();
        }

        private void OnEventSetupComplete()
        {
            EventLoadingComplete?.Invoke();
        }

        private ICharacterAsset GetCharacterAssetIfLoaded(long characterId)
        {
            return _assetManager.GetActiveAssetOfType<ICharacterAsset>(characterId);
        }

        private void SubscribeAssetChangerEvents()
        {
            foreach (var assetChanger in _assetChangers)
            {
                assetChanger.AssetStartedUpdating += OnAssetStartedUpdating;
                assetChanger.AssetUpdatingFailed += OnAssetUpdatingFailed;
                assetChanger.AssetUpdated += OnAssetUpdated;
            }

            _voiceFilterController.VoiceFilterChanged += OnAssetUpdated;
        }

        private void SetCameraPositionFromTemplate(Event template, Action onCameraSetup)
        {
            var cameraAnimation = template.GetCameraAnimation();
            _assetManager.Load(cameraAnimation, PutCameraOnFirstFrame);

            void PutCameraOnFirstFrame(IAsset cameraAnimationAsset)
            {
                var clip = (cameraAnimationAsset as ICameraAnimationAsset).Clip;
                var firstFrame = clip.GetFrame(0);
                _cameraTemplatesManager.SetStartFrameForTemplates(firstFrame);
                _cameraSystem.Simulate(firstFrame);

                onCameraSetup?.Invoke();
            }
        }
        
        private void ClampEditingCharacterNumberToAvailableRange(Event targetEvent)
        {
            //allow to have EditingCharacterSequenceNumber as -1 if event has few characters
            if(targetEvent.CharacterController.Count > 1 && EditingCharacterSequenceNumber == -1) return;
            
            var containsCharacterWithEditingIndex =
                targetEvent.CharacterController.Any(x => x.ControllerSequenceNumber == EditingCharacterSequenceNumber);
            
            if (containsCharacterWithEditingIndex) return;
            
            EditingCharacterSequenceNumber = TargetCharacterSequenceNumber;
        }

        private UserPhotoVideoPlayer GetSetLocationPhotoVideoPlayer()
        {
            var setLocation = _assetManager.GetAllLoadedAssets<ISetLocationAsset>().First(x => x.Id == TargetEvent.GetSetLocationId());
            return setLocation.MediaPlayer;
        }
        
        private void UnloadSetLocationBackgroundAssets()
        {
            _assetManager.UnloadAll(DbModelType.UserPhoto);
            _assetManager.UnloadAll(DbModelType.VideoClip);
            _assetManager.UnloadAll(DbModelType.SetLocationBackground);
        }
        
        private long? GetRecommendedSpawnPosition(SetLocationFullInfo nextLocation)
        {
            long? movementTypeId;
            var currentBodyAnimation = TargetEvent.GetTargetCharacterController().GetBodyAnimation();
            if (currentBodyAnimation.MovementTypeId.HasValue)
            {
                movementTypeId = currentBodyAnimation.MovementTypeId;
            }
            else //fallback while we don't have set movement type for all animations
            {
                var previousSpawnPoint =
                    TargetEvent.GetSetLocation().GetSpawnPosition(TargetEvent.CharacterSpawnPositionId);
                movementTypeId = previousSpawnPoint.MovementTypeId;
            }

            return nextLocation.GetSpawnPositions().Where(x => x.AvailableForSelection)
                               .Where(x => x.MovementTypeId == movementTypeId)
                               .OrderByDescending(x => x.IsDefault)
                               .FirstOrDefault()?.Id;
        }

        private long? GetDefaultSpawnPosition(SetLocationFullInfo nextLocation)
        {
            return nextLocation.GetDefaultSpawnPosition().Id;
        }
        
        private void OnSpawnFormationChanged()
        {
            SpawnFormationSetup?.Invoke(TargetEvent.CharacterSpawnPositionFormationId);
            SpawnFormationChanged?.Invoke(TargetEvent.CharacterSpawnPositionFormationId);
        }

        private void ChangeSetLocationInternal(SetLocationFullInfo nextLocation, long? nextSpawnPointId, SetLocationChanged onCompleted, bool allowChangingAnimations, IBodyAnimationSelector bodyAnimationSelector, bool recenterCamera)
        {
            var spawnPositions = nextLocation.GetSpawnPositions();

            if (spawnPositions.Count == 0)
            {
                throw new InvalidOperationException($"SetLocation {nextLocation.Name} has no spawn positions");
            }

            _setLocationChangingAlgorithm.SetBodyAnimationSelector(bodyAnimationSelector);
            var nextSpawnPos = spawnPositions.First(x => x.Id == nextSpawnPointId);
            var currentSpawnPos = TargetEvent.GetSetLocation().GetSpawnPosition(TargetEvent.CharacterSpawnPositionId);
            var keepFormation = ShouldKeepSpawnFormationOnChangingSpawnPoint(currentSpawnPos, nextSpawnPos);
            _setLocationChangingAlgorithm.Run(TargetEvent, nextLocation, nextSpawnPointId.Value, allowChangingAnimations, keepFormation, OnSetLocationChanged);
            
            void OnSetLocationChanged(ISetLocationAsset setLocationAsset, DbModelType[] alsoUpdatedTypes)
            {
                _notUsedAssetsUnloader.UnloadNotUsedAssets(TargetEvent);
                if (!keepFormation)
                {
                    SetDefaultSpawnFormation();
                }
                onCompleted?.Invoke(setLocationAsset, alsoUpdatedTypes);
                SetLocationChangeFinished?.Invoke(setLocationAsset);
            }
        }

        private bool ShouldKeepSpawnFormationOnChangingSpawnPoint(CharacterSpawnPositionInfo previousSpawnPos, CharacterSpawnPositionInfo nextSpawnPos)
        {
            if (!nextSpawnPos.MovementTypeId.HasValue) return true;
            var nextSpawnPosMovementType = GetMovementType(nextSpawnPos.MovementTypeId.Value);
            if (!nextSpawnPosMovementType.SupportFormation) return false;
            if (!previousSpawnPos.MovementTypeId.HasValue) return true;
            var previousSpawnPosMovementType = GetMovementType(previousSpawnPos.MovementTypeId.Value);
            if (previousSpawnPosMovementType == nextSpawnPosMovementType && HasDefaultSpawnFormation)
            {
                return false;
            }
            return previousSpawnPosMovementType.SupportFormation;
        }

        private MovementType GetMovementType(long id)
        {
            return _metadataProvider.MetadataStartPack.MovementTypes.First(x=>x.Id == id);
        }
        
        private async Task<ISetLocationAsset> ShuffleSetLocation(EventShuffleResult shuffleResult, SetLocationFullInfo[] setLocations)
        {
            var setLocation = setLocations.FirstOrDefault(sl => sl.Id == shuffleResult.SetLocationId);
            var nextSpawnPositionId = shuffleResult.Characters.First().CharacterSpawnPositionId;

            var isDone = false;
            ISetLocationAsset loadedSetLocation = null;

            if (shuffleResult.SetLocationId == TargetEvent.GetSetLocationId())
            {
                var model = TargetEvent.GetSetLocation()
                                       .GetSpawnPosition((shuffleResult.Characters.First().CharacterSpawnPositionId));
                ChangeCharacterSpawnPosition(model, false, (modelTypes) =>
                {
                    OnSetLocationChanged(_assetManager.GetActiveAssets<ISetLocationAsset>().First());
                });
            }
            else
            {
                ChangeSetLocationInternal(setLocation, nextSpawnPositionId,  (setLocationAsset, otherChangedAssetTypes) =>
                {
                    OnSetLocationChanged(setLocationAsset);
                }, false, _bodyAnimationForShuffleSelector, false);
            }
            
            void OnSetLocationChanged(ISetLocationAsset locationAsset)
            {
                isDone = true;
                loadedSetLocation = locationAsset;
            }

            while (!isDone)
            {
                await Task.Delay(100);
            }

            return loadedSetLocation;
        }
        
        private async Task ShuffleBodyAnimations(EventShuffleResult shuffleResult, BodyAnimationInfo[] bodyAnimations)
        {
            foreach (var character in shuffleResult.Characters)
            {
                var bodyAnimation = bodyAnimations.FirstOrDefault(ba => ba.Id == character.BodyAnimationId);
            
                await ChangeBodyAnimation(bodyAnimation, TargetEvent.CharacterController
                                                                         .Where(controller => controller.CharacterId == character.CharacterId).ToList());
            }
        }
    }
}