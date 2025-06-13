using System;
using Bridge.VideoServer;

namespace UIManaging.Pages.Common.VideoUploading
{
    public interface IVideoUploader
    {
        event Action<long> OnVideoUploadedEvent;
        void UploadLevelVideo(DeployLevelVideoReq deployData, Action<long> onSuccess = null);
        void UploadNonLevelVideo(DeployNonLevelVideoReq deployData, Action<long> onSuccess = null, Action onFail = null);
    }
}