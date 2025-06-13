using System;
using System.Threading;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.Players.EventPlaying;

namespace Modules.LevelManaging.Editing.Players
{
    internal interface IEventPlayControl
    { 
        event Action EventStarted;
        
        PlayMode CurrentPlayMode { get; }
        Event TargetEvent { get;}

        void ChangePlayMode(PlayMode playMode);
        void PlayEvent(PlayMode playMode, Event eventData, Action onEventLoaded = null, Action onEventPlayed = null, CancellationToken cancellationToken = default);
        void CancelEventPreview();
        
        void PauseCurrentPlayMode();
        void ResumeCurrentPlayMode();
        void StopCurrentPlayMode();
        void RefreshPlayers(DbModelType modelType, Event ev);
        void CleanUp();
        void PauseAudio();
        void PauseCaption(long captionId);
        void ResumeCaption(long captionId);

        /// <summary>
        /// Puts all assets to state in particular time moment
        /// </summary>
        /// <param name="time">Target time in seconds</param>
        /// <param name="targetTypes">Target asset types to simulate. If list is null or empty, it will simulate all</param>
        void Simulate(float time, params DbModelType[] targetTypes);
    }
}