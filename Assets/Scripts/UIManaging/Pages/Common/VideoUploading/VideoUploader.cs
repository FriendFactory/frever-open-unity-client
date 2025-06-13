using System;
using System.Collections.Generic;
using Bridge;
using Bridge.VideoServer;
using Common;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoUploading
{
    [UsedImplicitly]
    internal sealed class VideoUploader : IVideoUploader
    {
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<long> OnVideoUploadedEvent;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UploadLevelVideo(DeployLevelVideoReq deployData, Action<long> onSuccess = null)
        {
            UploadLevelVideoAsync(deployData, onSuccess);
        }

        public void UploadNonLevelVideo(DeployNonLevelVideoReq deployData, Action<long> onSuccess = null, Action onFail = null)
        {
            UploadNonLevelVideoAsync(deployData, onSuccess, onFail);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void UploadLevelVideoAsync(DeployLevelVideoReq deployVideoData, Action<long> onSuccess = null)
        {
            var uploadVideoResult = await _bridge.UploadLevelVideoAsync(deployVideoData);

            if (uploadVideoResult.IsSuccess)
            {
                onSuccess?.Invoke(uploadVideoResult.VideoId);
                OnVideoUploadedEvent?.Invoke(uploadVideoResult.VideoId);
            }
            else
            {
                Debug.LogError($"Failed to upload video (Level ID: {deployVideoData.LevelId}). Reason: {uploadVideoResult.ErrorData.ErrorMessage}");
            }
        }
        
        private async void UploadNonLevelVideoAsync(DeployNonLevelVideoReq deployVideoData, Action<long> onSuccess = null, Action onFail = null)
        {
            var uploadVideoResult = await _bridge.UploadNonLevelVideoAsync(deployVideoData);

            if (uploadVideoResult.IsSuccess)
            {
                SendCopyRightNoticeEvent(false);
                onSuccess?.Invoke(uploadVideoResult.VideoId);
                OnVideoUploadedEvent?.Invoke(uploadVideoResult.VideoId);
            }
            else
            {
                OnNonLevelVideoDeployingFailed(uploadVideoResult.ErrorData.ErrorMessage);
                onFail?.Invoke();
            }
        }

        private void OnNonLevelVideoDeployingFailed(string errorMessage)
        {
            var isCopyRight = errorMessage.Contains(Constants.ErrorMessage.COPYRIGHT_ERROR_IDENTIFIER);
            var isModeration = errorMessage.Contains(Constants.ErrorMessage.MODERATION_FAILED_ERROR_CODE);
            SendCopyRightNoticeEvent(isCopyRight);
            
            if (isCopyRight)
            {
                _snackBarHelper.ShowInformationSnackBar("Sorry, you are not allowed to upload this video due to potential copyright infringement.", 5);
            }
            else if (isModeration)
            {
                _snackBarHelper.ShowInformationSnackBar("This video contains inappropriate content", 5);
            }
            else
            {
                Debug.LogError($"Failed to upload video. Reason: {errorMessage}");
            }
        }
        
        private void SendCopyRightNoticeEvent(bool isCopyRight)
        {
            var metaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.COPYRIGHT_MATCH] = isCopyRight,
                [AmplitudeEventConstants.EventProperties.MEDIA_TYPE] = "Video"
            };

            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.COPYRIGHT_NOTICE, metaData);
        }
    }
}