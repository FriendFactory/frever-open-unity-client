using Bridge.Models.VideoServer;

namespace UIManaging.Pages.Common.VideoManagement
{
    internal interface IVideoUploadingListener
    {
        void OnVideoUploaded(Video video);
    }
}