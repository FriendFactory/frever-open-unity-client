using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraSystemCore;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelPreview;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.PreviewSplitting;
using UnityEngine;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing.Players
{
    [UsedImplicitly]
    internal sealed class LevelPlayControl: ILevelPlayControl
    {
        private const int TIME_FOR_ASSETS_UNLOADING_MS = 1000;
        
        private readonly IEventEditor _eventEditor;
        private readonly ILevelPreviewAssetsLoader _assetsLoader;
        private readonly IPlayersManager _playersProvider;
        private readonly ICameraControl _cameraControl;
        private readonly EventAssetsProvider _eventAssetsProvider;
        private readonly IAssetManager _assetManager;
        private readonly AssetPlayStateOnEventSwitchingManager _assetPlayStateOnEventSwitchingManager;
        private readonly ReusedAssetsAlgorithm _reusedAssetsAlgorithm;
        
        private PreviewPiece _playingPiece;
        private Event _playingEvent;
        private ICollection<Event> _allEvents;
        private long _firstEventId;
        private readonly PreviewSplitter _previewSplitter;
        private List<IAssetPlayer> _currentEventAssetPlayers = new List<IAssetPlayer>();
        private readonly List<IAssetPlayer> _assetPlayers = new List<IAssetPlayer>();
        private bool _isPlaying;
        private bool _isCancelled;
        private Coroutine _runningCoroutine;
        private MemoryConsumingMode _memoryConsumingMode;
        private RenderTexture _renderTexture;
        private Camera _currentCamera;

        private Action _onPreviewCompleted;
        private Action _onPreviewStarted;
        
        private readonly IReadOnlyDictionary<MemoryConsumingMode, SplitType> _previewModeToSplitType =
            new Dictionary<MemoryConsumingMode, SplitType>
            {
                {MemoryConsumingMode.SafeMemory, SplitType.KeepAssetsInRamFromOneEventMax},
                {MemoryConsumingMode.UseFullMemory, SplitType.KeepAssetAsMuchAsAllowedByRam}
            };

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action LevelPreviewStarted;
        public event Action LevelPreviewEnded;
        public event Action LevelPreviewCancelled;
        public event Action PlayingEventSwitched;
        public event Action LevelPiecePlayingCancelled;
        public event Action NextLevelPiecePlayingStarted;
        public event Action<Camera> CameraChanged;
        public event Action<float> Tick;
        public event Action LevelPiecePlayingCompleted;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private Event[] PreviewEvents => _playingPiece.Events;
        public Level TargetLevel { get; private set; }
        public Camera CurrentCamera
        {
            get => _currentCamera;
            private set
            {
                if (_currentCamera == value) return;
                _currentCamera = value;
                CameraChanged?.Invoke(value);
            } 
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        internal LevelPlayControl(IEventEditor eventEditor, IAssetManager assetManager,
            AvatarHelper avatarHelper, ILevelPreviewAssetsLoader assetsLoader,
            IPlayersManager assetPlayersProvider, EventAssetsProvider eventAssetsProvider, 
            AssetPlayStateOnEventSwitchingManager assetPlayStateOnEventSwitchingManager, PreviewSplitter previewSplitter,
            ICameraControl cameraControl, ReusedAssetsAlgorithm reusedAssetsAlgorithm)
        {
            _eventEditor = eventEditor;
            _assetManager = assetManager;
            _assetsLoader = assetsLoader;
            _playersProvider = assetPlayersProvider;
            _eventAssetsProvider = eventAssetsProvider;
            _assetPlayStateOnEventSwitchingManager = assetPlayStateOnEventSwitchingManager;
            _previewSplitter = previewSplitter;
            _cameraControl = cameraControl;
            _reusedAssetsAlgorithm = reusedAssetsAlgorithm;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void PlayLevelPreview(Level level, int firstEventIndex, MemoryConsumingMode memoryConsumingMode, RenderTexture renderTexture, Action onEventStarted, Action onCompleted)
        {
            _isCancelled = false;
            TargetLevel = level;
            _onPreviewStarted = onEventStarted;
            _onPreviewCompleted = onCompleted;
            _memoryConsumingMode = memoryConsumingMode;
            _renderTexture = renderTexture;
            
            var eventsToPlay = level.Event.Skip(firstEventIndex).ToArray();
            _allEvents = eventsToPlay;
            var splitType = _previewModeToSplitType[memoryConsumingMode];
            var firstPiece = _previewSplitter.GetNextPiece(eventsToPlay, splitType);
            _firstEventId = firstPiece.Events.First().Id;
            PlayLevelPiece(firstPiece);
        }
        
        public void CancelLevelPreview()
        {
            _isCancelled = true;
            _assetsLoader.CancelLoadingAssets();
            
            if (!_isPlaying) return;

            if (_runningCoroutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }

            StopAllPlayers();
            DetachTargetRenderTextureFromCameras();
            CurrentCamera = null;
            Resources.UnloadUnusedAssets();
            GC.Collect();
            _isPlaying = false;
            LevelPreviewCancelled?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlayLevelPiece(PreviewPiece piece)
        {
            _playingPiece = piece;
            _assetsLoader.LoadAssets(piece.Events, true, OnComplete);

            void OnComplete(List<IAsset> assets)
            {
                if (_isCancelled) return;
                _runningCoroutine = CoroutineSource.Instance.StartCoroutine(InitializePreview(assets));
            }
        }

        private IEnumerator InitializePreview(List<IAsset> assets)
        {
            yield return PrewarmShaders(_playingPiece.Events);
            if (_isCancelled) yield break;
            
            PreparePlayers(assets);
            
            yield return RunPreview();
            _runningCoroutine = null;
        }

        private IEnumerator RunPreview()
        {
            _isPlaying = true;
            
            for (var i  = 0; i < PreviewEvents.Length; i++)
            {
                var e = PreviewEvents[i];
                _currentEventAssetPlayers = PreparePlayers(e);
                PrepareCamera();
                StartPlay();

                var isFirstEvent = i == 0;
                var isFirstPiece = _firstEventId == e.Id;
                
                if (isFirstEvent && isFirstPiece)
                {
                    OnPreviewStarted();
                }

                if (isFirstEvent && !isFirstPiece)
                {
                    NextLevelPiecePlayingStarted?.Invoke();
                }
                
                PlayingEventSwitched?.Invoke();
                
                var eventLength = e.Length.ToSeconds();
                yield return WaitEventEnding(eventLength);
                if (_isCancelled)
                {
                    OnPreviewCanceled();
                    yield break;
                }
                
                FinishEventPlaying(e);
                _currentEventAssetPlayers.Clear();
            }
            
            _isPlaying = false;
            OnPiecePreviewFinished();
        }

        private void OnPreviewCanceled()
        {
            _isPlaying = false;
            _currentEventAssetPlayers.Clear();
            LevelPiecePlayingCancelled?.Invoke();
        }

        private IEnumerator WaitEventEnding(float eventLengthInSec)
        {
            while (eventLengthInSec > 0)
            {
                if(_isCancelled) yield break;
                yield return null;
                Tick?.Invoke(Time.deltaTime);
                eventLengthInSec -= Time.deltaTime;
            }
        }
        
        private List<IAssetPlayer> PreparePlayers(Event ev)
        {
            _playingEvent = ev;
            
            _eventEditor.SetTargetEvent(ev, null, false);

             return SetupPlayers(ev);
        }

        private void StartPlay()
        {
            foreach (var player in _currentEventAssetPlayers)
            {
                if(player.IsPlaying) continue;
                
                player.Play();
            }
        }

        private void FinishEventPlaying(Event ev)
        {
            var nextEvent = GetNextEvent();
            if (nextEvent == null)
            {
                FinishLevelPreview();
            }
            else
            {
                StopRequiredPlayers(ev, nextEvent);
            }
        }

        private void FinishLevelPreview()
        {
            foreach (var assetPlayer in _currentEventAssetPlayers)
            {
                assetPlayer.Stop();
            }
        }

        private void StopRequiredPlayers(Event current, Event nextEvent)
        {
            foreach (var assetPlayer in _currentEventAssetPlayers)
            {
                if(!ShouldStopPlayerBetweenEvents(assetPlayer, current, nextEvent)) continue;
                   
                assetPlayer.Stop();
            }
        }

        private bool ShouldStopPlayerBetweenEvents(IAssetPlayer assetPlayer, Event current, Event next)
        {
            return _assetPlayStateOnEventSwitchingManager.ShouldStopBetweenEvents(current, next, assetPlayer);
        }

        private List<IAssetPlayer> SetupPlayers(Event targetEvent)
        {
            var players = GetPlayersForEvent(targetEvent);
            
            foreach (var player in players)
            {
                var setup = _playersProvider.GetSetup(player.TargetType);
                setup.Setup(player, targetEvent);
            }

            return players;
        }

        private List<IAssetPlayer> GetPlayersForEvent(Event ev)
        {
            var eventAssets = _eventAssetsProvider.GetLoadedAssets(ev, _playersProvider.PlayableTypes.ToArray());
            
            var players = new List<IAssetPlayer>();

            foreach (var asset in eventAssets)
            {
                if(!_playersProvider.IsSupported(asset.AssetType)) continue;

                var player = _assetPlayers.FirstOrDefault(x => x.TargetAsset.Equals(asset));
                if (player == null)
                {
                    Debug.LogWarning($"Player not found for: {asset.AssetType} {asset.Id}");
                    continue;
                }
                players.Add(player);
            }
            
            return players;
        }

        private void PreparePlayers(ICollection<IAsset> assets)
        {
            _assetPlayers.Clear();
            foreach (var asset in assets)
            {
                if (!_playersProvider.IsSupported(asset.AssetType)) continue;

                var player = _playersProvider.CreateAssetPlayer(asset);
                _assetPlayers.Add(player);
            }
        }

        private Event GetNextEvent()
        {
            var nextEventSeqNumber = _playingEvent.LevelSequence + 1;
            return GetEvent(nextEventSeqNumber);
        }

        private Event GetEvent(int levelSequence)
        {
            return TargetLevel.Event.FirstOrDefault(x => x.LevelSequence == levelSequence);
        }

        private void OnPreviewStarted()
        {
            _onPreviewStarted?.Invoke();
            LevelPreviewStarted?.Invoke();
        }

        private async void OnPiecePreviewFinished()
        {
            var lastPlayingEvent = PreviewEvents.Last();
            if (lastPlayingEvent.Id == _allEvents.Last().Id)
            {
                OnLevelPreviewFinished();
                return;
            }

            var nextEvent = _allEvents.SkipWhile(x => x.Id != lastPlayingEvent.Id).Skip(1).ToArray();
            var assetsFromNextEvent = _reusedAssetsAlgorithm.GetAlreadyLoadedAssetsUsedBy(nextEvent);
            _assetManager.UnloadAllExceptFor(assetsFromNextEvent);

            StopAllPlayers();
            LevelPiecePlayingCompleted?.Invoke();
                      
            await Task.Delay(TIME_FOR_ASSETS_UNLOADING_MS);
            
            var remainedEventsToPlay = _allEvents.SkipWhile(x => x.Id != lastPlayingEvent.Id).Skip(1).ToArray();
            var splitType = _previewModeToSplitType[_memoryConsumingMode];
            var nextPiece = _previewSplitter.GetNextPiece(remainedEventsToPlay, splitType);
            PlayLevelPiece(nextPiece);
        }

        private void OnLevelPreviewFinished()
        {
            DetachTargetRenderTextureFromCameras();
            CurrentCamera = null;
            _onPreviewCompleted?.Invoke();
            LevelPreviewEnded?.Invoke();
        }

        private void DetachTargetRenderTextureFromCameras()
        {
            foreach (var setLocationAsset in _assetManager.GetAllLoadedAssets<ISetLocationAsset>())
            {
                setLocationAsset.Camera.targetTexture = null;
            }
        }

        private void StopAllPlayers()
        {
            foreach (var player in _assetPlayers)
            {
                if(!player.IsPlaying) continue;
                player.Stop();
            }
        }

        private void PrepareCamera()
        {
            SetupCameraControl();
            _cameraControl.EnableCameraRendering(true);
        }

        private void SetupCameraControl()
        {
            DetachTargetRenderTextureFromCameras();
            var currentSetLocation =
                (ISetLocationAsset)_eventAssetsProvider.GetLoadedAssets(_playingEvent, DbModelType.SetLocation).First();
            _cameraControl.SetCameraComponents(currentSetLocation.Camera, currentSetLocation.CinemachineBrain);
            currentSetLocation.Camera.targetTexture = _renderTexture;
            currentSetLocation.Camera.ApplyAspectRatioFromRenderTextureImmediate();
            CurrentCamera = currentSetLocation.Camera;
        }
        
        private IEnumerator PrewarmShaders(Event[] events)
        {
            foreach (var currentEvent in events)
            {
                var eventAssets = _eventAssetsProvider.GetLoadedAssets(currentEvent, _playersProvider.PlayableTypes.ToArray());

                if (eventAssets.FirstOrDefault(x => x.AssetType == DbModelType.SetLocation) is ISetLocationAsset setLocation)
                {
                    setLocation.Camera.targetTexture = null;
                }

                yield return PrewarmShaders(eventAssets, currentEvent);
            }
        }

        //simulates few frames of event to force rendering of objects -> shaders prewarm
        private IEnumerator PrewarmShaders(IAsset[] eventAssets, Event currentEvent)
        {
            PreparePlayers(eventAssets);
            var eventLength = currentEvent.Length.ToSeconds();
            const int framesToSimulate = 2;
            for (var frameIndex = 0; frameIndex < framesToSimulate; frameIndex++)
            {
                var timeToSimulate = eventLength / framesToSimulate * frameIndex;
                SimulateEventForShadersPrewarm(currentEvent, timeToSimulate);
                yield return null;
            }
        }

        private void SimulateEventForShadersPrewarm(Event targetEvent, float timeToSimulate)
        {
            var players = PreparePlayers(targetEvent);
            foreach (var player in players)
            {
                if(!NeededForShadersPrewarm(player.TargetType)) continue;
                player.Simulate(timeToSimulate);
            }
        }

        private bool NeededForShadersPrewarm(DbModelType modelType)
        {
            return !modelType.IsAudioType() && modelType != DbModelType.FaceAnimation;
        }
    }
}