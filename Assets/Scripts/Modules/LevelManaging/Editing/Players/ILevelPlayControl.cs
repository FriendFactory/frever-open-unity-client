using System;
using Models;
using UnityEngine;

namespace Modules.LevelManaging.Editing.Players
{
    /// <summary>
    /// Responsible for playing Level preview
    /// </summary>
    internal interface ILevelPlayControl
    {
        Level TargetLevel { get; }
        
        event Action LevelPreviewStarted; 
        event Action LevelPreviewEnded;
        event Action LevelPreviewCancelled;
        event Action PlayingEventSwitched;
        event Action LevelPiecePlayingCompleted;
        event Action LevelPiecePlayingCancelled;
        event Action NextLevelPiecePlayingStarted;
        event Action<Camera> CameraChanged; 
        event Action<float> Tick;
        
        Camera CurrentCamera { get; }

        void PlayLevelPreview(Level level, int firstEventIndex, MemoryConsumingMode memoryConsumingMode, RenderTexture texture, Action onEventStarted, Action onCompleted);
        void CancelLevelPreview();
    }
}