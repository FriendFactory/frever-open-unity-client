using Bridge.Models.VideoServer;

namespace UIManaging.Pages.Common.VideoManagement
{
    internal interface IVideoPrivacyChangingListener
    {
        void OnVideoPrivacyChanged(Video video);
    }
}