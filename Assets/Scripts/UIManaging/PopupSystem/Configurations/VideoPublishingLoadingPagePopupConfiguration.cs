using System;
using UnityEngine;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class VideoPublishingLoadingPagePopupConfiguration : BasePageLoadingPopupConfiguration
    {
        public float MaxValue { get; private set; } = 1.0f;
        public float InitialValue { get; private set; } = 0.0f;

        public IVideoRenderingState VideoRenderingState { get; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<float> OnProgressUpdated;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public VideoPublishingLoadingPagePopupConfiguration(string header, string progressBarText, IVideoRenderingState videoRenderingState, Action<object> onClose = null)
            : base(header, progressBarText, onClose, PopupType.VideoPublishingLoadingPage)
        {
            VideoRenderingState = videoRenderingState;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetMaxProgressValue(float maxValue)
        {
            MaxValue = maxValue;
        }
        
        public void UpdateProgress(float value)
        {
            OnProgressUpdated?.Invoke(value);
        }
    }
}