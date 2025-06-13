using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bridge;
using Common;
using Modules.VideoSharing;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing
{
    internal sealed class VideoSharingPresenter
    {
        private readonly VideoSharingHelper _videoSharingHelper;
        private readonly IBridge _bridge;

        public bool IsInitialized { get; private set; }

        public event Action<bool> LoadingToggled;

        private VideoSharingModel Model { get; set; }
        private VideoSharingPanel View { get; set; }

        public VideoSharingPresenter(VideoSharingHelper videoSharingHelper, IBridge bridge)
        {
            _videoSharingHelper = videoSharingHelper;
            _bridge = bridge;
        }

        public void Initialize(VideoSharingModel model, VideoSharingPanel view)
        {
            Model = model;
            View = view;

            View.ShareToRequested += ShareVideo;
            View.NativeShareRequested += OpenNativeShare;

            IsInitialized = true;
        }

        public void CleanUp()
        {
            View.ShareToRequested -= ShareVideo;
            View.NativeShareRequested -= OpenNativeShare;

            IsInitialized = false;
        }

        private void ShareVideo()
        {
            _videoSharingHelper.Share(Model.Video);
        }

        private async void OpenNativeShare()
        {
            ToggleLoading(true);

            var result = await _videoSharingHelper.ShareLinkNativeAsync(Model.VideoSharingUrl);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Video sharing failed # error: {result.ErrorMessage}");
                ToggleLoading(false);
                return;
            }

            ToggleLoading(false);

            if (Model.SharedOnce || !result.IsShared) return;
            
            if (!await TryClaimVideoShareReward()) return;

            Model.ShareCount += 1;
        }

        private async Task<bool> TryClaimVideoShareReward()
        {
            if (!TryGetVideoGuid(Model.VideoSharingUrl, out var videoGuid))
            {
                Debug.LogError($"[{GetType().Name}] Failed to get video GUID # {Model.VideoSharingUrl}");
                return false;
            }

            var result = await _bridge.ClaimVideoShareReward(videoGuid);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to claim video share reward # {result.ErrorMessage}");
                return false;
            }

            return true;
            
            bool TryGetVideoGuid(string link, out string videoGuid)
            {
                videoGuid = string.Empty;
            
                var videoRegex = new Regex(Constants.Regexes.VIDEO_LINK);
                var match = videoRegex.Match(link);

                if (!match.Success) return false;

                videoGuid = match.Groups[2].Value;

                return true;
            }
        }

        private void ToggleLoading(bool isOn)
        {
            LoadingToggled?.Invoke(isOn);
            View.ToggleLoading(isOn);
        }
    }
}