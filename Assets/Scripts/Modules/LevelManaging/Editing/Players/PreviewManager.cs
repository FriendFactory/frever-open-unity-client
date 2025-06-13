using System;
using System.Linq;
using System.Threading;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.LevelManaging.Editing.Players
{
    [UsedImplicitly]
    internal sealed class PreviewManager : IPreviewManager
    {
        public PlayMode PlayMode => PlayingType == PlayingType.SingleEvent?  _eventPlayControl.CurrentPlayMode : PlayMode.Preview;
        public PlayingType PlayingType { get; private set; }

        private readonly ILevelPlayControl _levelPlayControl;
        private readonly IEventPlayControl _eventPlayControl;
        private readonly IPlayersManager _playersManager;
        private readonly IAssetManager _assetManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly EventAssetsProvider _eventAssetsProvider;

        private PreviewCleanMode _cleanMode;

        public event Action EventStarted;
        public event Action NextLevelPiecePlayingStarted;
        public event Action LevelPiecePlayingCompleted;
        public event Action PreviewStarted;
        public event Action PlayingEventSwitched;

        public PreviewManager(ILevelPlayControl levelPlayControl, IEventPlayControl eventPlayControl,
            IAssetManager assetManager, EventAssetsProvider eventAssetsProvider, IPlayersManager playersManager,
            ICameraSystem cameraSystem)
        {
            _levelPlayControl = levelPlayControl;
            _eventPlayControl = eventPlayControl;
            _assetManager = assetManager;

            _levelPlayControl.LevelPreviewStarted += OnPreviewStarted;
            _eventPlayControl.EventStarted += OnEventStarted;
            _levelPlayControl.PlayingEventSwitched += OnPlayingEventSwitched;
            _levelPlayControl.NextLevelPiecePlayingStarted += OnLevelPiecePlayingStarted;
            _levelPlayControl.LevelPiecePlayingCompleted += OnLevelPiecePlayingCompleted;
            _eventAssetsProvider = eventAssetsProvider;
            _playersManager = playersManager;
            _cameraSystem = cameraSystem;
        }

        public void PlayLevelPreview(Level levelData, int firstEventIndex, MemoryConsumingMode mode, PreviewCleanMode cleanMode, RenderTexture outputTexture, Action onEventStarted,
            Action onCompleted)
        {
            StopEventPlayMode();
            
            PlayingType = PlayingType.MultipleEvents;
            _cleanMode = cleanMode;

            _levelPlayControl.PlayLevelPreview(levelData, firstEventIndex, mode, outputTexture, onEventStarted, ()=>
            {
                ClearMemory(cleanMode, levelData.Event.ToArray());
                onCompleted?.Invoke();
            });
        }

        public void CancelLevelPreview(PreviewCleanMode cleanMode)
        {
            _levelPlayControl.CancelLevelPreview();
            ClearMemory(cleanMode, _levelPlayControl.TargetLevel.Event.ToArray());
        }

        public void ChangePlayMode(PlayMode playMode)
        {
            _eventPlayControl.ChangePlayMode(playMode);
        }

        public void PlayEvent(PlayMode mode, Event eventData, PreviewCleanMode cleanMode ,Action onEventLoaded = null, Action onEventPlayed = null, CancellationToken cancellationToken = default)
        {
            PlayingType = PlayingType.SingleEvent;
            _cleanMode = cleanMode;
            
            _eventPlayControl.PlayEvent(mode, eventData,()=>
            {
                onEventLoaded?.Invoke();
                OnEventStarted();
                if(mode == PlayMode.Preview || mode == PlayMode.PreviewWithCameraTemplate)
                    OnPreviewStarted();
            }, ()=>
            {
                ClearMemory(cleanMode, eventData);
                onEventPlayed?.Invoke();
            }, cancellationToken);
        }
        
        private void ClearMemory(PreviewCleanMode cleanMode, params Event[] playedEvents)
        {
            switch (cleanMode)
            {
                case PreviewCleanMode.KeepAll:
                    //do nothing
                    break;
                case PreviewCleanMode.KeepFirstEvent:
                    var firstEvent = playedEvents.OrderBy(x => x.LevelSequence).First();
                    UnloadAssets(firstEvent);
                    break;
                case PreviewCleanMode.KeepLastEvent:
                    var lastEvent = playedEvents.OrderBy(x => x.LevelSequence).Last();
                    UnloadAssets(lastEvent);
                    break;
                case PreviewCleanMode.ReleaseAll:
                    _cameraSystem.ForgetCamera();
                    _cameraSystem.Enable(false);
                    _assetManager.UnloadAll();
                    CollectGarbage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UnloadAssets(Event keepAssetsEvent)
        {
            var assetsFromEvent = _eventAssetsProvider.GetLoadedAssets(keepAssetsEvent, _playersManager.PlayableTypes.ToArray());
            _assetManager.UnloadAllExceptFor(assetsFromEvent);
        }

        private void CollectGarbage()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public void CancelTargetEventPreview()
        {
            _eventPlayControl.CancelEventPreview();
            ClearMemory(_cleanMode, _eventPlayControl.TargetEvent);
        }

        public void PauseEventPlayMode()
        {
            _eventPlayControl.PauseCurrentPlayMode();
        }

        public void ResumeEventPlayMode()
        {
            _eventPlayControl.ResumeCurrentPlayMode();
        }

        public void StopEventPlayMode()
        {
            _eventPlayControl.StopCurrentPlayMode();
        }
        
        public void PauseAudio()
        {
            _eventPlayControl.PauseAudio();
        }

        public void PauseCaption(long captionId)
        {
            _eventPlayControl.PauseCaption(captionId);
        }

        public void ResumeCaption(long captionId)
        {
            _eventPlayControl.ResumeCaption(captionId);
        }

        public void RefreshPlayers(Event ev, params DbModelType[] types)
        {
            foreach (var type in types)
            {
                _eventPlayControl.RefreshPlayers(type, ev);
            }
        }

        public void CleanUp()
        {
            _eventPlayControl.CleanUp();
        }

        public void Simulate(float time, params DbModelType[] assetsToSimulate)
        {
            _eventPlayControl.Simulate(time, assetsToSimulate);
        }

        private void OnEventStarted()
        {
            EventStarted?.Invoke();
        }
        
        private void OnPreviewStarted()
        {
            PreviewStarted?.Invoke();
        }
        
        private void OnPlayingEventSwitched()
        {
            PlayingEventSwitched?.Invoke();
        }
        
        private void OnLevelPiecePlayingStarted()
        {
            NextLevelPiecePlayingStarted?.Invoke();
        }
        
        private void OnLevelPiecePlayingCompleted()
        {
            LevelPiecePlayingCompleted?.Invoke();
        }
    }
}