using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using Extensions;

namespace UIManaging.Pages.Common.VideoManagement
{
    internal interface IVideoListCacheControl
    {
        void UpdateTemplateName(string previousName, string newName);
    }
    
    internal abstract class CachedVideoLoader<T> : VideoLoader<T>, IVideoListCacheControl where T : VideoLoadArgs
    {
        public sealed override bool SupportCache => true;

        protected CachedVideoLoader(IVideoBridge bridge):base(bridge)
        {
        }

        public abstract void UpdateTemplateName(string previousName, string newName);

        protected override Task<FeedVideoResponse> GetVideoAsyncInternal(T args)
        {
            var isRequestingFirstPage = !args.TargetVideoId.HasValue;
            if (isRequestingFirstPage && !GetCache(args).IsEmpty)
            {
                SetupLoadingFromFirstVideoInCache(ref args);
            }
            
            if (args.TargetVideoId.HasValue && GetCache(args).HasInCache(args.TargetVideoId.Value))
            {
                return GetVideoFromCacheAndDownloadMissed(args);
            }
            
            return DownloadAndCache(args);
        }

        protected abstract Task<EntitiesResult<Video>> DownloadFromServerAsync(T args);
        
        protected abstract VideoCache GetCache(T args);
        
        private async Task<FeedVideoResponse> DownloadAndCache(T args)
        {
            var nextVideo = await DownloadFromServerAsync(args);
            if (nextVideo.IsSuccess)
            {
                GetCache(args).AddToTheEnd(nextVideo.Models);
            }
            return ConvertToFeedVideoResponse(nextVideo);
        }

        private async Task<FeedVideoResponse> GetVideoFromCacheAndDownloadMissed(T args)
        {
            var targetVideoId = args.TargetVideoId;
            var cache = GetCache(args);
            var videoBefore = cache.GetVideoBefore(targetVideoId.Value, args.TakePrevious);
            var targetVideo = cache.GetVideo(targetVideoId.Value);
            var missedNextVideoCount = args.TakeNext - 1; //minus 1, because 'take next' includes the target video on the backend
            var videoAfter = cache.GetVideoAfter(targetVideoId.Value, missedNextVideoCount);
            if (videoAfter.Length == missedNextVideoCount)
            {
                return new FeedVideoResponse
                {
                    Video = videoBefore.Concat(new[] {targetVideo}).Concat(videoAfter).ToArray()
                };
            }

            var lastCachedVideo = videoAfter.Any() ? videoAfter.Last() : targetVideo;
            args.TargetVideoKey = lastCachedVideo.Key;
            args.TargetVideoId = lastCachedVideo.Id;
            args.TakeNext -= videoAfter.Length;
            args.TakePrevious = 0;

            var missedVideoResp = await DownloadFromServerAsync(args);
            
            if (missedVideoResp.IsRequestCanceled)
            {
                return new FeedVideoResponse {IsCanceled = true};
            }

            var output = videoBefore.Concat(new[] { targetVideo }).Concat(videoAfter);
            var responseVideos = missedVideoResp.Models;

            var responseContainsLastCachedVideo = !responseVideos.IsNullOrEmpty() && responseVideos.First().Id == lastCachedVideo.Id;
            var missedVideo = responseContainsLastCachedVideo ? responseVideos.Skip(1).ToArray() : responseVideos;

            if (missedVideo == null || missedVideo.Length == 0)
            {
                return new FeedVideoResponse
                {
                    Video = output.ToArray()
                };
            }

            var responseIncludesTargetVideo = missedVideo[0].Id == lastCachedVideo.Id;
            if (responseIncludesTargetVideo)
            {
                missedVideo = missedVideo.Skip(1).ToArray();
            }
            
            cache.AddToTheEnd(missedVideo);
            return new FeedVideoResponse
            {
                Video = output.Concat(missedVideo).ToArray()
            };
        }
        
        private void SetupLoadingFromFirstVideoInCache(ref T args)
        {
            var cache = GetCache(args);
            args.TargetVideoId = cache.GetFirstVideo()?.Id;
        }

        private static FeedVideoResponse ConvertToFeedVideoResponse(EntitiesResult<Video> resp)
        {
            return new FeedVideoResponse
            {
                Video = resp.Models,
                ErrorMessage = resp.ErrorMessage,
                IsCanceled = resp.IsRequestCanceled
            };
        }
    }
}