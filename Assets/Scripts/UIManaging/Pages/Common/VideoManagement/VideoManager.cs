using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Level;
using Bridge.Models.VideoServer;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.NotificationServer;
using Bridge.Results;
using Bridge.VideoServer;
using Common;
using Common.UserNotifications;
using JetBrains.Annotations;
using Models;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;
using Event = Models.Event;
using NewLikeOnVideoNotification = Bridge.NotificationServer.NewLikeOnVideoNotification;
using Object = UnityEngine.Object;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    public sealed class VideoManager
    {
        public event Action<long, long> LikeCountChanged;
        
        [Inject] private FeedVideoLikesService _feedVideoLikesService;
        
        private readonly IBridge _bridge;
        private readonly VideoProvider _videoProvider;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly IVideoUploadingListener[] _videoUploadingListeners;
        private readonly IVideoPrivacyChangingListener[] _videoPrivacyChangingListeners;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        internal VideoManager(IBridge bridge, SnackBarHelper snackBarHelper, VideoProvider videoProvider, IVideoUploadingListener[] videoUploadingListeners, IVideoPrivacyChangingListener[] videoPrivacyChangingListeners, INotificationEventSource notificationEventSource)
        {
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
            _videoProvider = videoProvider;
            _videoUploadingListeners = videoUploadingListeners;
            _videoPrivacyChangingListeners = videoPrivacyChangingListeners;
            notificationEventSource.NewNotificationReceived += OnNotificationReceived;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void GetThumbnailForLevel(Level level, Resolution targetResolution,
            Action<Texture2D> onThumbnailDownloaded, CancellationToken cancellationToken = default)
        {
            if (level.Event != null && level.Event.Count > 1)
            {
                GetThumbnailForEvent(level.Event.ElementAt(0), targetResolution, onThumbnailDownloaded, cancellationToken);
                return;
            }

            if (level.Event?.Count > 0)
            {
                var firstEvent =  level.Event.ElementAt(0);
                GetThumbnailForEvent(firstEvent, targetResolution, onThumbnailDownloaded, cancellationToken);
                return;
            }

            var result = await _bridge.GetLevelThumbnailInfo(level.Id, cancellationToken);

            if (result.IsSuccess)
            {
                GetThumbnail(result.Model, targetResolution, onThumbnailDownloaded, cancellationToken);
            }
            else
            {
                Debug.LogWarning(result.ErrorMessage);
            }
        }

        public async void GetThumbnailForEvent(IEventInfo @event, Resolution resolution,
            Action<Texture2D> onThumbnailDownloaded, CancellationToken cancellationToken = default)
        {
            if (@event.Files.Count == 0) return;
            var fileInfo = @event.Files.FirstOrDefault(x => x.Resolution == resolution) ?? @event.Files.Last();
            var thumbnailResult = await _bridge.GetEventThumbnailAsync(@event.Id, fileInfo, true, cancellationToken);
            HandleThumbnailFileResponse(onThumbnailDownloaded, thumbnailResult);
        }
        
        public async void GetVideosForLocalUser(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            var args = new VideoLoadArgs { TargetVideoId = videoId, TakeNext = takeNextCount, TakePrevious = takePreviousCount, CancellationToken = cancellationToken, VideoType = VideoListType.Profile};
            await LoadVideoListAsync(args, onSuccess, onFail);
        }
        
        public Task<FeedVideoResponse> GetVideosForLocalUser(long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            var args = new VideoLoadArgs { TargetVideoId = videoId, TakeNext = takeNextCount, TakePrevious = takePreviousCount, CancellationToken = cancellationToken, VideoType = VideoListType.Profile};
            return _videoProvider.GetVideosAsync(args);
        }

        public void GetVideoForUser(long videoId, Action<string> onFail, Action<Video> onSuccess = null, CancellationToken cancellationToken = default)
        {
            GetVideoInternal(videoId, onSuccess, onFail, cancellationToken);
        }

        public void GetVideosForRemoteUser(Action<Video[]> onSuccess, Action<string> onFail, long groupId,
            long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            var getVideoListTask = _bridge.GetPublicVideoForAccount(groupId, videoId, takeNextCount, takePreviousCount, cancellationToken);
            HandleVideoListTask(getVideoListTask, onSuccess, onFail, cancellationToken);
        }

        public async void GetFeedVideos(Action<Video[]> onSuccess, Action<string> onFail, VideoListType videoType, long? videoId, int takeNextCount,
                                  int takePreviousCount, CancellationToken cancellationToken)
        {
            var args = new VideoLoadArgs
            {
                VideoType = videoType,
                TargetVideoId = videoId,
                TakeNext = takeNextCount,
                TakePrevious = takePreviousCount,
                CancellationToken = cancellationToken
            };

            await LoadVideoListAsync(args, onSuccess, onFail);
        }
        
        public Task<Video[]> GetFeedVideosAsync(Action<string> onFail, VideoListType videoType, long? videoId, int takeNextCount,
            int takePreviousCount, CancellationToken cancellationToken)
        {
            var args = new VideoLoadArgs
            {
                VideoType = videoType,
                TargetVideoId = videoId,
                TakeNext = takeNextCount,
                TakePrevious = takePreviousCount,
                CancellationToken = cancellationToken
            };

            return LoadVideoListAsync(args, onFail:onFail);
        }

        public void GetVideosForTaggedUser(Action<Video[]> onSuccess, Action<string> onFail, long groupId, long? targetVideo, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            var getVideoListTask = _bridge.GetUserTaggedVideoListAsync(groupId, targetVideo, takeNextCount, takePreviousCount, cancellationToken);
            HandleVideoListTask(getVideoListTask, onSuccess, onFail, cancellationToken);
        }
        
        public async void GetHashtagVideos(long hashtagId, Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount, CancellationToken cancellationToken = default)
        {
            var args = CreateHastHashtagVideoLoadArgs(hashtagId, videoId, takeNextCount, takePreviousCount, cancellationToken);
            await LoadVideoListAsync(args, onSuccess, onFail);
        }
        
        public Task<FeedVideoResponse> GetHashtagVideos(long hashtagId, long? videoId, int takeNextCount, int takePreviousCount, CancellationToken cancellationToken = default)
        {
            var args = CreateHastHashtagVideoLoadArgs(hashtagId, videoId, takeNextCount, takePreviousCount, cancellationToken);
            return _videoProvider.GetVideosAsync(args);
        }

        public async void GetRemixVideoListAsync(Action<Video[]> onSuccess, Action<string> onFail, long videoId,  long? targetVideoId, int takeNextCount, int takePreviousCount, CancellationToken token)
        {
            var response = await _bridge.GetRemixVideoListAsync(videoId, targetVideoId, takeNextCount, takePreviousCount, token);
            
            if (response.IsError)
            {
                Debug.LogError($"Failed to get videos. Reason: {response.ErrorMessage}");
                onFail?.Invoke(response.ErrorMessage);
            }

            if (response.IsSuccess)
            {
                onSuccess?.Invoke(response.Models);
            }
        }

        public void ForgetPreviousVideos(params VideoListType[] targets)
        {
            _videoProvider.ClearCache(targets);
        }

        public void ClearAllVideosBefore(VideoListType target, long videoId, bool includingTarget = true)
        {
            _videoProvider.ClearAllVideosBefore(target, videoId, includingTarget);
        }

        public Task<FeedVideoResponse> GetVideoForTemplate(long templateId, long? videoId, int takeNextCount, int takePrevious, CancellationToken cancellationToken = default)
        {
            var args = new TemplateVideoLoadArgs
            {
                TemplateId = templateId,
                TargetVideoId = videoId,
                TakeNext = takeNextCount,
                TakePrevious = takePrevious,
                CancellationToken = cancellationToken
            };

            return _videoProvider.GetVideosAsync(args);
        }
        
        public Task<FeedVideoResponse> GetVideoForTask(long taskId, long? videoId, int takeNextCount, int takePrevious, CancellationToken cancellationToken = default)
        {
            var args = new TaskVideoLoaderArgs
            {
                TaskId = taskId,
                TargetVideoId = videoId,
                TakeNext = takeNextCount,
                TakePrevious = takePrevious,
                CancellationToken = cancellationToken
            };

            return _videoProvider.GetVideosAsync(args);
        }

        public async Task<Video[]> GetUserVideoForTasks(long? videoId, long userGroupId, int takeNext,
                                         int takePrevious, CancellationToken cancellationToken,
                                         Action<Video[]> onSuccess = null, Action<string> onFail = null)
        {
            var args = new UserTaskVideoLoaderArgs
            {
                TargetVideoId = videoId,
                UserGroupId = userGroupId,
                TakeNext = takeNext,
                TakePrevious = takePrevious,
                CancellationToken = cancellationToken,
            };

            return await LoadVideoListAsync(args, onSuccess, onFail);
        }
        
        public async Task DeleteVideo(long videoId)
        {
            var videoDeletingResult = await _bridge.DeleteVideo(videoId);
            
            if (videoDeletingResult.IsError)
            {
                Debug.LogError($"Failed to delete {nameof(Video)} with {nameof(Video.Id)}={videoId}. Reason: {videoDeletingResult.ErrorMessage}");
            }
            _videoProvider.RemoveVideoInCache(videoId);
        }
        
        public void ChangeVideoPrivacy(Video video, VideoAccess access, long[] taggedFriendIds, Action onSuccess = null, Action onFailure = null)
        {
            ChangeVideoPrivacyAsync(video, access, taggedFriendIds, onSuccess, onFailure);
        }
        
        public void RefreshVideoKPIInCache(long videoId, Action<Video> onComplete = null)
        {
            GetVideoInternal(videoId, OnVideoDownloaded);

            void OnVideoDownloaded(Video video)
            {
                _videoProvider.UpdateVideoKpi(video.Id, video.KPI);
                onComplete?.Invoke(video);
            }
        }

        public async void GetTaskVideos(long taskId, long? targetVideoKey, int takeNextCount, int takePreviousCount,
                                  Action<Video[]> onSuccess, Action<string> onFail,
                                  CancellationToken cancellationToken = default)
        {
            var args = CreateFeedTaskVideoLoadArgs(taskId, targetVideoKey, takeNextCount, takePreviousCount,
                                               cancellationToken);
            var response = await _videoProvider.GetVideosAsync(args);
            if (response.IsSuccess)
            {
                onSuccess?.Invoke(response.Video);
            }
            else
            {
                Debug.LogWarning(response.ErrorMessage);
                onFail?.Invoke(response.ErrorMessage);
            }
        }
        
        public Task<FeedVideoResponse> GetTaskGridVideo(long taskId, long? videoId, string targetVideoKey ,int takeNextCount, int takePrevious, CancellationToken cancellationToken = default)
        {
            var args = new TaskVideoLoaderArgs()
            {
                TaskId= taskId,
                TargetVideoKey = targetVideoKey,  
                TargetVideoId = videoId,
                TakeNext = takeNextCount,
                TakePrevious = takePrevious, 
                CancellationToken = cancellationToken
            };

            return _videoProvider.GetVideosAsync(args);
        }

        public void OnNewVideoUploaded(Video video)
        {
            foreach (var newVideoUploadingListener in _videoUploadingListeners)
            {
                newVideoUploadingListener.OnVideoUploaded(video);
            }
        }
        
        public async void GetVideosForSound(SoundType soundType, long soundId, Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount, CancellationToken cancellationToken = default)
        {
            var args = new SoundVideoLoaderArgs(soundType, soundId)
            {
                TargetVideoId = videoId, 
                TakeNext = takeNextCount,
                TakePrevious = takePreviousCount,
                CancellationToken = cancellationToken,
            };
            
            await LoadVideoListAsync(args, onSuccess, onFail);
        }

        public Task<FeedVideoResponse> GetVideosForSound(SoundType soundType, long soundId, long? videoId, int takeNext, int takePrevious, CancellationToken cancellationToken = default)
        {
            var args = new SoundVideoLoaderArgs(soundType, soundId)
            {
                TargetVideoId = videoId, 
                TakeNext = takeNext,
                TakePrevious = takePrevious,
                CancellationToken = cancellationToken,
            };

            return _videoProvider.GetVideosAsync(args);
        }

        public bool IsFollowingRecommended(Video video)
        {
            return video.IsFollowRecommended
                   || _feedVideoLikesService.GetLocalGivenLikesCount(video.GroupId) >= Constants.Feed.LIKES_GIVEN_TO_RECOMMEND_FOLLOWING;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static async void HandleVideoListTask(Task<EntitiesResult<Video>> getVideosTask,
                                                      Action<Video[]> onSuccess, Action<string> onFail, CancellationToken cancellationToken)
        {
            var result = await getVideosTask;
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (result == null || result.IsError)
            {
                var error = result?.ErrorMessage;
                Debug.LogError($"Failed to get videos. Reason: {error}");
                onFail?.Invoke(error);
            }
            else if(!result.IsRequestCanceled)
            {
                onSuccess?.Invoke(result.Models);
            }
        }

        private async void GetVideoInternal(long videoId, Action<Video> onSuccess, Action<string> onFail = null, CancellationToken cancellationToken = default)
        {
            var getVideoTask = await _bridge.GetVideoAsync(videoId, cancellationToken);

            if (getVideoTask.IsError)
            {
                if (onFail == null)
                {
                    //Debug.LogError($"Failed to get video. Reason: {getVideoTask.ErrorMessage}");
                }

                onFail?.Invoke(getVideoTask.ErrorMessage);
            }

            if (getVideoTask.IsSuccess)
            {
                onSuccess?.Invoke(getVideoTask.ResultObject);
            }
        }

        public async void LikeVideo(long videoId, long groupId, bool like, Action onSuccess = null)
        {
            var task = like ? _bridge.LikeVideoAsync(videoId) : _bridge.UnlikeVideoAsync(videoId);

            await task;

            if (task.Result.IsError)
            {
                DisplayLikeErrorMessage();
            }

            if (!task.Result.IsSuccess) return;

            _feedVideoLikesService.AddLocalGivenLikesCount(groupId, like);

            LikeCountChanged?.Invoke(videoId, groupId);
            onSuccess?.Invoke();
        }

        private void DisplayLikeErrorMessage()
        {
            _snackBarHelper.ShowInformationSnackBar("Video was deleted or set to private. Can't like", 2);
        }
        
        private async void GetThumbnail(LevelShortInfo levelShortInfo, Resolution resolution,
                                               Action<Texture2D> onThumbnailDownloaded, CancellationToken cancellationToken = default)
        {
            var thumbnailResult = await _bridge.GetThumbnailAsync(levelShortInfo, resolution, true, cancellationToken);
            HandleThumbnailFileResponse(onThumbnailDownloaded, thumbnailResult);
        }
        
        private static void HandleThumbnailFileResponse(Action<Texture2D> onThumbnailDownloaded, GetAssetResult thumbnailResult)
        {
            if(thumbnailResult.IsRequestCanceled) return;
            
            if (thumbnailResult.IsSuccess)
            {
                if (onThumbnailDownloaded != null)
                {
                    onThumbnailDownloaded((Texture2D) thumbnailResult.Object);
                }
                else
                {
                    Object.Destroy((Object) thumbnailResult.Object);
                }
            }

            if (thumbnailResult.IsError)
            {
                Debug.LogError($"Failed to download thumbnail for {nameof(Event)}. Reason: {thumbnailResult.ErrorMessage}");
            }
        }
        
        private async Task<Video[]> LoadVideoListAsync(VideoLoadArgs args, Action<Video[]> onSuccess = null, Action<string> onFail = null)
        {
            var feedResp = await _videoProvider.GetVideosAsync(args);
            
            if (args.CancellationToken.IsCancellationRequested)
            {
                return null;
            }
            
            if (feedResp.IsSuccess)
            {
                onSuccess?.Invoke(feedResp.Video);
                return feedResp.Video;
            }
            
            if (!feedResp.IsCanceled)
            {
                onFail?.Invoke(feedResp.ErrorMessage);
            }

            return null;
        }
        
        private static HashtagVideoLoadArgs CreateHastHashtagVideoLoadArgs(long hashtagId, long? videoId, int takeNextCount,
                                                                           int takePreviousCount, CancellationToken cancellationToken)
        {
            var args = new HashtagVideoLoadArgs
            {
                HashtagId = hashtagId,
                TargetVideoId = videoId,
                TakeNext = takeNextCount,
                TakePrevious = takePreviousCount,
                CancellationToken = cancellationToken
            };
            return args;
        }
        
        private async void ChangeVideoPrivacyAsync(Video video, VideoAccess access, long[] taggedFriendIds, Action onSuccess, Action onFailure)
        {
            var changeVideoPrivacyTask = await _bridge.ChangePrivacyAsync(video.Id, new UpdateVideoAccessRequest
            {
                Access = access,
                TaggedFriendIds = taggedFriendIds
            });

            if (changeVideoPrivacyTask.IsSuccess)
            {
                video.Access = access;
                OnVideoChangedChanged(video);
                onSuccess?.Invoke();
            }
            
            if (changeVideoPrivacyTask.IsError)
            {
                Debug.LogError($"Failed to changed privacy of {nameof(Video)} with {nameof(Video.Id)} {video.Id}. Reason: " + changeVideoPrivacyTask.ErrorMessage);
                onFailure?.Invoke();
            }
        }

        private void OnVideoChangedChanged(Video video)
        {
            _videoProvider.UpdateVideoPrivacyInCache(video.Id, video.Access);
            foreach (var videoPrivacyChangingListener in _videoPrivacyChangingListeners)
            {
                videoPrivacyChangingListener.OnVideoPrivacyChanged(video);
            }
        }

        private static TaskVideoLoaderArgs CreateFeedTaskVideoLoadArgs(long taskId, long? targetVideoId, int takeNextCount,
                                                          int takePreviousCount, CancellationToken cancellationToken)
        {
            return new TaskVideoLoaderArgs
            {
                TaskId = taskId,
                TargetVideoId = targetVideoId,
                TakeNext = takeNextCount,
                TakePrevious = takePreviousCount,
                CancellationToken = cancellationToken
            };
        }
        
        private void OnNotificationReceived(NotificationBase notification)
        {
            switch (notification.NotificationType)
            {
                case NotificationType.NewLikeOnVideo:
                    RefreshKpiForVideo((notification as NewLikeOnVideoNotification).LikedVideo);
                    break;
                case NotificationType.NewCommentOnVideo:
                    RefreshKpiForVideo((notification as NewCommentOnVideoNotification).CommentedVideo);
                    break;
                case NotificationType.NewMentionOnVideo:
                    RefreshKpiForVideo((notification as NewMentionOnVideoNotification).MentionedVideo);
                    break;
                case NotificationType.YourVideoRemixed:
                    RefreshKpiForVideo((notification as YourVideoRemixedNotification).RemixedFromVideo);
                    break;
            }

            void RefreshKpiForVideo(VideoInfo video)
            {
                if(video == null) return;
                RefreshVideoKPIInCache(video.Id);
            }
        }
    }
}