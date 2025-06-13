using System;

namespace UIManaging.PopupSystem.Configurations
{
    public interface IVideoRenderingState
    {
        event Action<float> ProgressUpdated;
        float Progress { get; }
        bool IsRendering { get; }
        bool IsFinished { get; }
        bool IsCanceled { get; }
    }
}