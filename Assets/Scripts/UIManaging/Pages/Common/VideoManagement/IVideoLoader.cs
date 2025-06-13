using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.VideoServer.Models;

namespace UIManaging.Pages.Common.VideoManagement
{
    internal interface IVideoLoader
    {
        VideoListType VideoType { get; }
        bool SupportCache { get; }
        Task<FeedVideoResponse> GetVideoAsync(VideoLoadArgs args);
        void ClearCache();
        void ClearAllBefore(long videoId, bool includingTarget = true);
        void UpdateVideoPrivacyInCache(long videoId, VideoAccess access);
        void UpdateVideoKPIInCache(long videoId, VideoKPI kpi);
        void AddLikeToVideoInCache(long videoId);
        void RemoveVideoInCache(long videoId);
    }

    internal abstract class VideoLoader<TArgs>: IVideoLoader where TArgs:VideoLoadArgs
    {
        protected readonly IVideoBridge Bridge;

        public abstract VideoListType VideoType { get; }
        public abstract bool SupportCache { get; }
        
        protected VideoLoader(IVideoBridge bridge)
        {
            Bridge = bridge;
        }
        
        public Task<FeedVideoResponse> GetVideoAsync(VideoLoadArgs args)
        {
            if (args is TArgs targetTypeArgs)
            {
                return GetVideoAsyncInternal(targetTypeArgs);
            }

            throw new InvalidOperationException(
                $"Wrong argument type for video loader. Video loader: {GetType().Name}. Args type: {args.GetType().Name}");
        }

        public abstract void ClearCache();
        public abstract void ClearAllBefore(long videoId, bool includingTarget = true);
        public abstract void UpdateVideoPrivacyInCache(long videoId, VideoAccess access);
        public abstract void UpdateVideoKPIInCache(long videoId, VideoKPI kpi);

        public abstract void AddLikeToVideoInCache(long videoId);
        public abstract void RemoveVideoInCache(long videoId);

        protected abstract Task<FeedVideoResponse> GetVideoAsyncInternal(TArgs args);
    }
}