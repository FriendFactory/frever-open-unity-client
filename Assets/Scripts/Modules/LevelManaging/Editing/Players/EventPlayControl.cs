using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.EventPlaying;
using Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms;
using static Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players
{
    [UsedImplicitly]
    internal sealed class EventPlayControl : IEventPlayControl
    {
        private readonly IEventEditor _eventEditor;
        private readonly IAssetTypesProvider _assetTypesProvider;
        private readonly Dictionary<PlayMode, EventPlayingAlgorithm> _playingAlgorithms;
        private Action _onEventStarted;

        private Action _onPreviewCompleted;
        private Action _onTargetEventCompletedCallback;
        private Action _onTargetEventStartedCallback;

        private PlayMode _previousMode;
        private DbModelType[] _allAssetTypes;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private DbModelType[] AllAssetTypes => _allAssetTypes ?? (_allAssetTypes = _assetTypesProvider.AssetTypes.ToArray());

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        internal EventPlayControl(IEventEditor eventEditor, IAssetTypesProvider assetTypesProvider,
            AudioSourceManager audioManager, AvatarHelper avatarHelper, PreRecordingPlayingAlgorithm preRecordAlgorithm,
            PreviewPlayingAlgorithm previewAlgorithm, RecordingPlayingAlgorithm recordingAlgorithm,
            OverrideCameraAnimationByTemplateEventPreview overrideCameraAnimationByTemplatePlayAlgorithm, 
            StayOnFirstFramePlayingAlgorithm stayOnFirstFramePlayingAlgorithm, LoopEventPreviewPlayingAlgorithm loopEventPreviewPlayingAlgorithm)
        {
            _eventEditor = eventEditor;
            _assetTypesProvider = assetTypesProvider;

            _playingAlgorithms = new Dictionary<PlayMode, EventPlayingAlgorithm>
            {
                {PreRecording, preRecordAlgorithm},
                {Preview, previewAlgorithm},
                {Recording, recordingAlgorithm},
                {PreviewWithCameraTemplate, overrideCameraAnimationByTemplatePlayAlgorithm},
                {StayOnFirstFrame, stayOnFirstFramePlayingAlgorithm},
                {PreviewLoop, loopEventPreviewPlayingAlgorithm}
            };

            SetCurrentPlayAlgorithm();
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action EventStarted;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public PlayMode CurrentPlayMode { get; private set; }
        public Event TargetEvent { get; private set; }
        private EventPlayingAlgorithm CurrentPlayingAlgorithm { get; set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ChangePlayMode(PlayMode playMode)
        {
            ChangeCurrentPlayMode(playMode);
        }

        public void PlayEvent(PlayMode playMode, Event eventData, Action onEventLoaded = null,
            Action onEventPlayed = null, CancellationToken cancellationToken = default)
        {
            StopCurrentPlayMode();
            
            TargetEvent = eventData;
            _playingAlgorithms[playMode].EventStarted -= OnEventStarted;
            _playingAlgorithms[playMode].EventStarted += OnEventStarted;

            _onPreviewCompleted = onEventPlayed;
            
            _playingAlgorithms[playMode].EventDone += OnEventDone;
            _eventEditor.SetTargetEvent(eventData, Play, playMode != Preview, cancellationToken);

            void Play()
            {
                onEventLoaded?.Invoke();
                ChangeCurrentPlayMode(playMode);
                PlayEvent();
                EventStarted?.Invoke();
            }
        }

        public void CancelEventPreview()
        {
            _playingAlgorithms[Preview].EventStarted -= OnTargetEventPreviewStarted;
            _playingAlgorithms[CurrentPlayMode].EventDone -= OnTargetEventPreviewCompleted;
            _playingAlgorithms[CurrentPlayMode].Cancel();
            PlayEvent(_previousMode, TargetEvent);
        }

        public void PauseCurrentPlayMode()
        {
            CurrentPlayingAlgorithm.Pause();
        }

        public void ResumeCurrentPlayMode()
        {
            CurrentPlayingAlgorithm.Resume();
        }

        public void StopCurrentPlayMode()
        {
            if(!CurrentPlayingAlgorithm.IsRunning) return;
            
            CurrentPlayingAlgorithm.EventDone -= OnCompleted;
            CurrentPlayingAlgorithm.Stop();
        }

        public void RefreshPlayers(DbModelType targetType, Event ev)
        {
            CurrentPlayingAlgorithm.RefreshAssetPlayers(targetType, ev);
        }

        public void CleanUp()
        {
            CurrentPlayingAlgorithm.RemoveAllAssetPlayers();
            _onEventStarted = null;
            _onPreviewCompleted = null;
            _onTargetEventCompletedCallback = null;
            _onTargetEventStartedCallback = null;
        }

        public void PauseAudio()
        {
            CurrentPlayingAlgorithm.PauseAudio();
        }

        public void PauseCaption(long captionId)
        {
            CurrentPlayingAlgorithm.PauseCaption(captionId);
        }

        public void ResumeCaption(long captionId)
        {
            CurrentPlayingAlgorithm.ResumeCaption(captionId);
        }

        public void Simulate(float time, params DbModelType[] assetsToSimulate)
        {
            if (CurrentPlayingAlgorithm.IsRunning)
            {
                CurrentPlayingAlgorithm.Stop();
            }
            CurrentPlayingAlgorithm.Init(TargetEvent);
            if (assetsToSimulate.IsNullOrEmpty())
            {
                assetsToSimulate = AllAssetTypes;
            }
            CurrentPlayingAlgorithm.Simulate(time, assetsToSimulate);
        }

        private void OnEventDone()
        {
            CurrentPlayingAlgorithm.EventDone -= OnEventDone;
            _onPreviewCompleted?.Invoke();
            _onPreviewCompleted = null;
        }

        private void OnTargetEventPreviewStarted()
        {
            _playingAlgorithms[Preview].EventStarted -= OnTargetEventPreviewStarted;
            _onTargetEventStartedCallback?.Invoke();
        }

        private void OnTargetEventPreviewCompleted()
        {
            _playingAlgorithms[Preview].EventStarted -= OnTargetEventPreviewStarted;
            _playingAlgorithms[Preview].EventDone -= OnTargetEventPreviewCompleted;
            _onTargetEventCompletedCallback?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ChangeCurrentPlayMode(PlayMode mode, bool stopCurrent = true)
        {
            if (CurrentPlayMode == mode) return;

            if (stopCurrent) StopCurrentPlayMode();

            _previousMode = CurrentPlayMode;
            CurrentPlayMode = mode;
            SetCurrentPlayAlgorithm();
        }

        private void PlayEvent()
        {
            CurrentPlayingAlgorithm.Init(TargetEvent);
            CurrentPlayingAlgorithm.Play();
            EventStarted?.Invoke();
        }

        private void OnEventStarted()
        {
            _onEventStarted?.Invoke();
            CurrentPlayingAlgorithm.EventStarted -= OnEventStarted;
            EventStarted?.Invoke();
        }

        private void OnCompleted()
        {
            CurrentPlayingAlgorithm.EventDone -= OnCompleted;
            CurrentPlayingAlgorithm.Stop();

            ChangeCurrentPlayMode(_previousMode, false);

            _onPreviewCompleted?.Invoke();
            _onPreviewCompleted = null;
        }

        private void SetCurrentPlayAlgorithm()
        {
            CurrentPlayingAlgorithm = _playingAlgorithms[CurrentPlayMode];
        }
    }
}