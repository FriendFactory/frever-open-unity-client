using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Bridge.VideoServer;
using Common.Publishers;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.Notifications;
using Navigation.Args;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Common.VideoUploading;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.PublishSuccess;
using UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    [UsedImplicitly]
    public sealed class PublishVideoHelper : IPublishVideoHelper
    {
        private readonly IBridge _bridge;
        private readonly VideoManager _videoManager;
        private readonly PopupManager _popupManager;
        private readonly IVideoUploader _videoUploader;
        private readonly IChatService _chatService;
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly SnackBarHelper _snackBarHelper;
        
        private Level _level;
        private long _publishedVideoId;
        private VideoAccess _videoAccess;
        private string _generateTemplateName;
        private Action<long> _onVideoPublished;
        private bool _showPublishedSuccessPopup = true;
        private bool _publishedVideoFromTask;
        private bool _isTaskVideo;
        private string _prefKey;

        [Inject] private Localization.PublishPageLocalization _localization;
        [Inject] private NotificationsPermissionHandler _notificationsPermissionHandler;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action VideoUploaded;
        public event Action<Video> VideoPublished;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsPublishing { private set; get; }
        public long? PublishedLevelId => _level?.Id;
        public bool IsPublishedFromTask => _publishedVideoFromTask;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public PublishVideoHelper(IBridge bridge, VideoManager videoManager, PopupManager popupManager,
             IVideoUploader videoUploader, IChatService chatService, LocalUserDataHolder localUserDataHolder, SnackBarHelper snackBarHelper)
        {
            _bridge = bridge;
            _videoManager = videoManager;
            _popupManager = popupManager;
            _videoUploader = videoUploader;
            _chatService = chatService;
            _localUserDataHolder = localUserDataHolder;
            _snackBarHelper = snackBarHelper;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UploadLevelVideo(DeployLevelVideoReq request, Level level, bool isTaskVideo,
            Action<long> onComplete, bool showPublishedSuccessPopup = true)
        {
            _onVideoPublished = onComplete;
            IsPublishing = true;
            _videoAccess = request.Access ?? VideoAccess.Private;
            _generateTemplateName = request.GenerateTemplate ? request.GenerateTemplateWithName : string.Empty;
            _level = level;
            _showPublishedSuccessPopup = showPublishedSuccessPopup;
            _publishedVideoFromTask = _level.SchoolTaskId != null;
            _isTaskVideo = isTaskVideo;
            
            _videoUploader.UploadLevelVideo(request, OnComplete);

            void OnComplete(long videoId)
            {
                VideoUploaded?.Invoke();
                _publishedVideoId = videoId;
                WaitForVideoUrl(videoId, OnLevelVideoUploaded, isTaskVideo);
            }
        }
        
        public void UploadNonLevelVideo(DeployNonLevelVideoReq request, Action<long> onComplete, Action onFail)
        {
            _onVideoPublished = onComplete;
            IsPublishing = true;
            _videoAccess = request.Access ?? VideoAccess.Private;
            onFail += ()=> _popupManager.ClosePopupByType(PopupType.VideoUploading);

            ShowVideoUploadingPopup();
            _videoUploader.UploadNonLevelVideo(request, OnComplete, onFail);
            
            void OnComplete(long videoId)
            {
                _publishedVideoId = videoId;
                WaitForVideoUrl(videoId, OnNonLevelVideoUploaded, false);
            }
        }

        public async Task ShareVideoToChats(long videoId, MessagePublishInfo messagePublishInfo)
        {
            var sendMessageModel = messagePublishInfo;
            var shouldSendVideoAsMessage = sendMessageModel != null &&
                                           (!sendMessageModel.ShareDestination.Chats.IsNullOrEmpty() ||
                                            !sendMessageModel.ShareDestination.Users.IsNullOrEmpty());
            
            if (!shouldSendVideoAsMessage) return;

            var shareDestinations = sendMessageModel.ShareDestination;
            
            var sendModel = new SendMessageToGroupsModel
            {
                ChatIds = shareDestinations.Chats?.Select(x=>x.Id).ToArray(),
                GroupIds = shareDestinations.Users?.Select(x=>x.MainGroupId).ToArray(),
                Text = messagePublishInfo.MessageText,
                VideoId = videoId
            };
            var res = await _chatService.PostMessage(sendModel);
            if (res.IsError)
            {
                Debug.LogError($"Failed to post message: {res.ErrorMessage}");
            }
        }

        public async void ShowPublishSuccessPopup(Video video)
        {
            if (_popupManager.IsPopupOpen(PopupType.PublishSuccess)) return;
                
            var creatorScoreModel = await GetCreatorScoreModelAsync();
            var videoSharingModel = await GetVideoSharingInfoAsync(video);
            _popupManager.SetupPopup(new PublishSuccessPopupConfiguration(video, creatorScoreModel, videoSharingModel, _generateTemplateName));
            _popupManager.ShowPopup(PopupType.PublishSuccess);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void WaitForVideoUrl(long videoId, Action<string, bool> onSuccess, bool isTaskVideo)
        {
            var downloadingVideoUrl = true;
            while (downloadingVideoUrl)
            {
                await Task.Delay(1000);
                
                var videoUrlResp = await _bridge.GetVideoFileUlr(videoId);
                if (!videoUrlResp.IsSuccess) continue;
                
                onSuccess.Invoke(videoUrlResp.Url, isTaskVideo);
                downloadingVideoUrl = false;
            }
        }

        private void OnLevelVideoUploaded(string videoUrl, bool isTaskVideo)
        {
            IsPublishing = false;

            _popupManager.ClosePopupByType(PopupType.VideoUploadingCountdown);
            OnVideoPublishFinished();
        }

        private void OnNonLevelVideoUploaded(string videoUrl, bool isTaskVideo)
        {
            IsPublishing = false;
            
            _popupManager.ClosePopupByType(PopupType.VideoUploading);
            OnVideoPublishFinished();
        }

        private async void OnVideoPublishFinished()
        {
            var videoResp = await _bridge.GetVideoAsync(_publishedVideoId);
            if (videoResp.IsError)
            {
                Debug.LogError($"Failed to load new uploaded video {_publishedVideoId}");
                return;
            }

            var video = videoResp.ResultObject;
            _videoManager.OnNewVideoUploaded(video);
            _onVideoPublished?.Invoke(video.Id);
            
            if (_videoAccess == VideoAccess.Private || _isTaskVideo)
            {
                var message = _isTaskVideo ? _localization.VideoPublishedTaskSnackbarMessage : _localization.VideoPublishedPrivateSnackbarMessage;
                _snackBarHelper.ShowSuccessDarkSnackBar(message);
            }
            else if (_showPublishedSuccessPopup)
            {
                ShowPublishSuccessPopup(video);
            }

            VideoPublished?.Invoke(video);
        }

        private void ShowVideoUploadingPopup()
        {
            var configuration = new VideoUploadingPopupConfiguration()
            {
                PopupType = PopupType.VideoUploading,
            };
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }
        
        private async Task<CreatorScoreModel> GetCreatorScoreModelAsync()
        {
            // not sure if we need more than 2,147,483,647 for creator score...
            var lastCreatorScore = (int)_localUserDataHolder.LevelingProgress.CreatorScore;

            await _localUserDataHolder.RefreshUserInfoAsync();
            
            var creatorScoreBadge = _localUserDataHolder.LevelingProgress.CreatorScoreBadge;
            var creatorScore = (int)_localUserDataHolder.LevelingProgress.CreatorScore;
            var scoreProgress = creatorScore - lastCreatorScore;

            return new CreatorScoreModel(creatorScoreBadge, creatorScore, scoreProgress);
        }

        private async Task<VideoSharingModel> GetVideoSharingInfoAsync(Video video)
        {
            var result = await _bridge.GetVideoSharingInfo(video.Id);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get video sharing info # {result.ErrorMessage}");
                return null;
            }

            return new VideoSharingModel(video, result.Model);
        }
    }
}