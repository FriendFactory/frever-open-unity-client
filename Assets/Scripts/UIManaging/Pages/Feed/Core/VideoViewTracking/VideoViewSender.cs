using System;
using Bridge;
using Bridge.VideoServer;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.Ui.Feed
{
    
    public sealed class VideoViewSender
    {
        [Inject] private IBridge _bridge;

        public async void Send(VideoView[] videoViews, Action onFail = null)
        {
            try
            {
                if (videoViews.Length == 0) return;

                var sendVideoViews = await _bridge.SendViewsData(videoViews);
                if (sendVideoViews.IsError)
                {
                    onFail?.Invoke();
                    Debug.LogError($"Failed to send user video views. Reason: {sendVideoViews.ErrorMessage}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
