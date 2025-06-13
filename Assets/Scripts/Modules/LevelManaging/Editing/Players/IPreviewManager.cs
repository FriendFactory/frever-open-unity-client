using System;
using System.Threading;
using Extensions;
using Models;
using UnityEngine;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.LevelManaging.Editing.Players
{
    internal interface IPreviewManager
    {
        event Action PreviewStarted; 
        event Action PlayingEventSwitched;
        event Action EventStarted;
        event Action LevelPiecePlayingCompleted;
        event Action NextLevelPiecePlayingStarted;

        PlayingType PlayingType { get; }
        
        void PlayLevelPreview(Level levelData, int firstEventIndex, MemoryConsumingMode mode, PreviewCleanMode cleanMode, RenderTexture outputTexture, Action onEventStarted, Action onCompleted);
        void CancelLevelPreview(PreviewCleanMode cleanMode);

        PlayMode PlayMode { get; }

        void ChangePlayMode(PlayMode playMode);
        void PlayEvent(PlayMode mode, Event eventData, PreviewCleanMode cleanMode, Action onEventLoaded = null, Action onEventPlayed = null, CancellationToken cancellationToken = default);
        void CancelTargetEventPreview();
        void PauseEventPlayMode();
        void ResumeEventPlayMode();
        void StopEventPlayMode();
        void PauseAudio();
        void PauseCaption(long captionId);
        void ResumeCaption(long captionId);
        void RefreshPlayers(Event ev, params DbModelType[] types);
        void CleanUp();
        void Simulate(float time, params DbModelType[] assetsToSimulate);
    }
}