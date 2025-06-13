using System;
using Extensions;
using Modules.LevelManaging.Editing.Players;
using UnityEngine;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelPlayer
    {
        event Action EventStarted;
        event Action EventPreviewStarted;
        event Action EventSaved;
        event Action EventLoadingStarted;
        event Action EventLoadingCompleted;
        event Action PlayingEventSwitched;
        event Action LevelPreviewStarted;
        event Action LevelPreviewCompleted;
        event Action EventPreviewCompleted;
        event Action NextLevelPiecePlayingStarted;
        event Action LevelPiecePlayingCompleted;
        event Action PreviewCancelled;
        PlayMode CurrentPlayMode { get; }
        PlayingType CurrentPlayingType { get; }
        float LevelDurationSec { get; }
        bool IsRunningLevelPreview { get; }
        
        void PlayEvent(PlayMode mode, Event targetEvent = null, Action onStartPlay = null, Action onEventPlayed = null);
        void PlayLevelPreview(MemoryConsumingMode previewMode, PreviewCleanMode cleanMode, RenderTexture renderTexture = null);
        void PlayLevelPreview(int firstEventIndex, MemoryConsumingMode previewMode, PreviewCleanMode cleanMode, RenderTexture renderTexture = null);
        void PlayEventPreview(Event targetEvent, Action onStarted = null, Action onCompleted = null);
        void PlayLevelPreviewForRendering(bool isLandscape);
        void PutEventOnFirstFrame(Action onCompleted = null);
        void Simulate(float time, PlayMode? playMode = null, params DbModelType[] assetsToSimulate);
        void StopAudio();
        void PauseCaption(long captionId);
        void ResumeCaption(long captionId);
        void CancelPreview(PreviewCleanMode cleanMode = PreviewCleanMode.KeepAll);
        void PauseEventPlayMode();
        void ResumeEventPlayMode();
        void StopCurrentPlayMode();
    }
}