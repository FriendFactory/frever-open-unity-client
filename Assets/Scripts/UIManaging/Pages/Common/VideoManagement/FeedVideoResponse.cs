using Bridge.Models.VideoServer;

namespace UIManaging.Pages.Common.VideoManagement
{
    public struct FeedVideoResponse
    {
        public Video[] Video;
        public string ErrorMessage;
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage) && !IsCanceled;
        public bool IsCanceled;
        public bool IsError => !IsSuccess && !IsCanceled;
    }
}