using System.Collections.Generic;
using System.Linq;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    using Extensions; // DWC-2022.2 upgrade: getting ambiguous warnings with this using statement so moving it here for scope
    internal sealed class VideoCache: IVideoListCacheControl
    {
        private readonly List<Video> _cache = new List<Video>();
        private readonly IBlockedAccountsManager _blockedAccounts;

        private IEnumerable<Video> NonBlockedCachedVideo =>
            _cache.Where(x => !_blockedAccounts.IsUserBlocked(x.GroupId));

        public bool IsEmpty => !NonBlockedCachedVideo.Any();
        public IEnumerable<Video> AvailableVideos => NonBlockedCachedVideo;
        
        public VideoCache(IBlockedAccountsManager blockedAccounts)
        {
            _blockedAccounts = blockedAccounts;
        }

        public void AddToTheTop(params Video[] videos)
        {
            Insert(0, videos);
        }

        public void Insert(int index, params Video[] videos)
        {
            _cache.InsertRange(index, videos);
        }
        
        public void AddToTheEnd(Video[] videos)
        {
            _cache.AddRange(videos);
        }

        public bool HasInCache(long videoId)
        {
            return _cache.Any(x => x.Id == videoId);
        }
        
        public Video[] GetVideoBefore(long videoId, int count)
        {
            return NonBlockedCachedVideo.TakeWhile(x => x.Id != videoId).TakeLast(count).ToArray();
        }

        public Video[] GetVideoAfter(long videoId, int takeNextCount)
        {
            return NonBlockedCachedVideo.SkipWhile(x => x.Id != videoId).Skip(1).Take(takeNextCount).ToArray();
        }

        public Video GetVideo(long videoId)
        {
            return _cache.FirstOrDefault(x => x.Id == videoId);
        }

        public Video GetFirstVideo()
        {
            return _cache.FirstOrDefault();
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void ClearVideosBefore(long videoId, bool includingTarget = true)
        {
            var newCache = _cache.SkipWhile(x => x.Id != videoId).Skip(includingTarget ? 1 : 0).ToArray();
            _cache.Clear();
            _cache.AddRange(newCache);
        }

        public void UpdateVideo(Video video)
        {
            if (!HasInCache(video.Id)) return;
            var videoToUpdate = GetVideo(video.Id);
            var index = _cache.IndexOf(videoToUpdate);
            _cache[index] = video;
        }
        
        public void RemoveVideo(long videoId)
        {
            if (!HasInCache(videoId)) return;
            var videoToRemove = GetVideo(videoId);
            _cache.Remove(videoToRemove);
        }

        public void UpdateTemplateName(string previousName, string newName)
        {
            var videosToUpdate = _cache.Where(x => x.MainTemplate != null && x.MainTemplate.Title == previousName);
            foreach (var video in videosToUpdate)
            {
                video.MainTemplate.Title = newName;
            }
        }
    }
}