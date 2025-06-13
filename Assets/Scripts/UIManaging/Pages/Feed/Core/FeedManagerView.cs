using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge.Models.VideoServer;
using Bridge;
using Bridge.VideoServer;
using Common;
using Common.Publishers;
using Modules.AssetsStoraging.Core;
using Modules.VideoStreaming.Feed;
using Navigation.Args;
using Navigation.Core;
using RenderHeads.Media.AVProVideo;
using UIManaging.Localization;
using UIManaging.Pages.Comments;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.EditTemplate;
using UIManaging.Pages.Feed.Core.VideoViewTracking;
using UIManaging.Pages.Feed.Dialogs;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.TaggedUsers;
using UIManaging.PopupSystem.Popups.VideoRatingReward;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Debug = UnityEngine.Debug;

#if TV_BUILD
using Settings;
#endif

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class FeedManagerView : BaseContextDataView<FeedManagerArgs>
    {
        private const int VIDEO_MODELS_MAX_COUNT = 200;
        private const int VIDEOS_BEFORE_TRIGGER_ALMOST_REACHED_BOTTOM_EVENT = 3;

        [SerializeField] private CanvasGroup body;
        [SerializeField] private FeedScrollView feedScroll;
        [SerializeField] private FeedVideoView _feedVideoViewPrefab;
        [SerializeField] private CommentsView _commentsView;
        [SerializeField] private FeedVideoOptions feedVideoOptions;
        [SerializeField] private GameObject noVideosMessageGameObject;
        [SerializeField] private VideoLoadingIndicator _videoLoadingIndicator;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private StopWatchProvider _stopWatchProvider;
        [Inject] private IPublishVideoHelper _publishHelper;
        [Inject] private FeedLoadingQueue _loadingQueue;
        [Inject] private VideoViewTracker _videoViewTracker;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private FeedLocalization _localization;

        private FeedScrollController _feedScrollController;
        private FeedVideoView _targetVideoView;
        private bool _isFirstVideoLoaded;
        private Stopwatch _stopWatch;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int VideoModelIndex { get; private set; }

        public bool ShouldRefreshOnVideoDeleted { get; set; }

        public bool BlockScrolling
        {
            set => _feedScrollController.BlockScrolling = value;
        }

        private IReadOnlyList<FeedVideoView> FeedVideoViews => feedScroll.ContextData?.VideoViews;

        public FeedVideoView TargetVideoView => _targetVideoView;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnFirstVideoLoadedEvent;
        public event Action OnRefreshDragPerformedEvent;
        public event Action OnVideoDeleted;
        public event Action OnScrolledToTop;
        public event Action OnScrolledAlmostToBottom;
        public event Action OnScrolledNextOnBottom;
        public event Action OnScroll;
        public event Action OnVideoPublished;
        public event Action<FeedVideoModel> OnVideoStartedPlaying;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            ShowBody(false);
            _loadingQueue.VideoReachedQueue += InitializeVideoStreaming;
            _feedScrollController = new FeedScrollController(_feedVideoViewPrefab);
        }

        private void OnEnable()
        {
            ShowNoVideosMessage(false);
            feedVideoOptions.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            ShowBody(false);
            _loadingQueue.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _loadingQueue.VideoReachedQueue -= InitializeVideoStreaming;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void CloseAllVideos()
        {
            if (FeedVideoViews == null) return;

            foreach (var videoView in FeedVideoViews)
            {
                if (videoView != null) videoView.CloseMedia();
            }
            
            feedScroll.CleanUp();
        }
        
        public void StopPlayingVideos()
        {
            foreach (var videoView in FeedVideoViews)
            {
                if (videoView == null || !videoView.IsPlaying && !videoView.IsVideoLoading)
                {
                    continue;
                }
                
                videoView.RewindAndStop();
            }
        }

        public void PauseCurrentVideo()
        {
            if (_targetVideoView == null)
            {
                return;
            }
            
            _targetVideoView.StopMediaPlayer();
        }
        
        public void ResumeCurrentVideo()
        {
            if (_targetVideoView == null)
            {
                return;
            }
            
            _targetVideoView.StartMediaPlayer();
        }
        
        public void OpenComments()
        {
            _commentsView.Open();
        }
        
        public void OpenCommentsWithKeyboard()
        {
            _commentsView.Open(true);
        }
        
        public void SetAspectRatioFitterMode(AspectRatioFitter.AspectMode aspectMode)
        {
            foreach (var feedVideoView in FeedVideoViews)
            {
                feedVideoView.SetAspectRatioFitterMode(aspectMode);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            var indexToShow = 0;

            if (ContextData.IdOfFirstVideoToShow > 0)
            {
                indexToShow = ContextData.IndexOfFirstVideoToShow;
            }

            VideoModelIndex = indexToShow;

            var feedVideoModels = ContextData.FeedVideoModels;

            if (feedVideoModels.Count == 0)
            {
                ShowBody(true);
            }
            
            _feedScrollController.Setup(feedVideoModels.Count, indexToShow);
            
            feedScroll.Initialize(_feedScrollController);

            for (var i = 0; i < FeedVideoViews.Count; i++)
            {
                var videoView = FeedVideoViews[i];
                if (videoView == null)
                {
                    continue;
                }
                
                videoView.gameObject.name = $"FeedVideoView{i}";
            }

            _isFirstVideoLoaded = false;
            
            SubscribeToEvents();
            RefreshNoVideosMessageActive();
            InitializeFirstVideos();
        }

        //---------------------------------------------------------------------
        // Internal
        //---------------------------------------------------------------------

        private void InitializeVideoStreaming(FeedLoadingItem item)
        {
            if (IsDestroyed || !gameObject.activeInHierarchy) return;
            
            item.Model.IsVideoAvailable = true;
            item.View.InitializeVideoAvailabilityState();
            
            if (item.AutoPlay)
            {
                if (_targetVideoView != null) _targetVideoView.CloseMedia();
                SetupTargetVideoView();
            }
                
            item.View.DownloadVideo(item.AutoPlay);
        }

        internal void AddVideos(IEnumerable<Video> videos, bool toTheEnd = true)
        {
            var currentModels = ContextData.FeedVideoModels;
            var newVideoModels = GetVideoModels(videos.ToArray());
            var uniqueNewVideoModels = newVideoModels.Where(model => currentModels.All(x => x.Video.Id != model.Video.Id)).ToArray();

            if (toTheEnd)
            {
                RegisterBelowVideosToLoadingQueue(uniqueNewVideoModels);
                currentModels.AddRange(uniqueNewVideoModels);
                LimitFeedFromAbove(currentModels);
            }
            else
            {
                RegisterAboveVideosToLoadingQueue(uniqueNewVideoModels);
                currentModels.InsertRange(0, uniqueNewVideoModels);
                LimitFeedFromBelow(currentModels);

                VideoModelIndex += uniqueNewVideoModels.Length;
                feedScroll.ContextData.Index += uniqueNewVideoModels.Length;
            }
            feedScroll.ContextData.Amount = currentModels.Count;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void LimitFeedFromAbove(List<FeedVideoModel> models)
        {
            var currentCount = models.Count;
            if (currentCount <= VIDEO_MODELS_MAX_COUNT) return;

            var countDiff = currentCount - VIDEO_MODELS_MAX_COUNT;
            models.RemoveRange(0, countDiff);

            VideoModelIndex -= countDiff;
            feedScroll.ContextData.Index -= countDiff;
        }

        private static void LimitFeedFromBelow(List<FeedVideoModel> models)
        {
            var currentCount = models.Count;
            if (currentCount <= VIDEO_MODELS_MAX_COUNT) return;

            var countDiff = currentCount - VIDEO_MODELS_MAX_COUNT;
            models.RemoveRange(currentCount - countDiff - 1, countDiff);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void OnVideoFinishedPlaying()
        {
            #if TV_BUILD
                var isLastVideo = VideoModelIndex >= ContextData.FeedVideoModels.Count - 1;

                if(isLastVideo)
                {
                    OnRefreshDragPerformedEvent?.Invoke();
                }
                else
                {
                    feedScroll.ScrollDown();
                }
            #endif
        }

        private void OnRefreshDragPerformed()
        {
            OnRefreshDragPerformedEvent?.Invoke();
        }

        private void RefreshTargetVideoKPI()
        {
            RefreshVideoKPIInCache(RefreshVideoKPI);
        }

        private void RefreshVideoKPIInCache(Action<Video> onComplete)
        {
            if (_targetVideoView == null)
            {
                return;
            }
            
            _videoManager.RefreshVideoKPIInCache(_targetVideoView.VideoId, onComplete);
        }

        private void RefreshVideoPrivacy(long videoId, VideoAccess access)
        {
            if (_targetVideoView.IsDestroyed || _targetVideoView.VideoId != videoId) return;
            
            _targetVideoView.RefreshVideoPrivacy(access);
        }

        private void RefreshVideoKPI(Video video)
        {
            if (_targetVideoView == null)
            {
                return;
            }
            
            _targetVideoView.RefreshKPIValues(video.KPI);
        }

        private async void DeleteVideo(long videoId)
        {
            var loadingPopupConfig = new InformationPopupConfiguration
            {
                PopupType = PopupType.Loading, Title = _localization.VideoDeleteLoadingTitle
            };
            _popupManager.SetupPopup(loadingPopupConfig);
            _popupManager.ShowPopup(loadingPopupConfig.PopupType);

            await _videoManager.DeleteVideo(videoId);

            OnVideoDeleted?.Invoke();
            if (ShouldRefreshOnVideoDeleted)
            {
                OnRefreshDragPerformed();
            }
            
            _snackBarHelper.ShowInformationSnackBar(_localization.VideoDeletedSnackbarMessage);

            _popupManager.ClosePopupByType(PopupType.Loading);
        }

        private void OnOpenProfileButtonClicked(long userGroupId, string nickname)
        {
            OnDataForRemoteUserDownloaded(userGroupId, nickname);
        }

        private void OnDataForRemoteUserDownloaded(long userGroupId, string nickname)
        {
            var feedPage = _pageManager.CurrentPage as FeedPage;
            if (feedPage == null) return;

            var userProfileArgs = new UserProfileArgs(userGroupId, nickname);
            feedPage.OpenPageArgs.SetLastShownForVideoListType(feedPage.OpenPageArgs.VideoListType, ContextData.FeedVideoModels[VideoModelIndex].Video.Id);

            _pageManager.MoveNext(PageId.UserProfile, userProfileArgs);
        }

        private void RefreshNoVideosMessageActive()
        {
            var hasVideos = ContextData != null && ContextData.FeedVideoModels.Count > 0;
            ShowNoVideosMessage(!hasVideos && (ContextData == null 
                                            || ContextData.FeedType != VideoListType.Friends 
                                            && ContextData.FeedType != VideoListType.Following));
        }

        private void ShowNoVideosMessage(bool visible)
        {
            noVideosMessageGameObject.SetActive(visible);

            if (FeedVideoViews == null) return;

            foreach (var view in FeedVideoViews)
            {
                if (view == null)
                {
                    continue;
                }
                
                view.gameObject.SetActive(!visible);
            }
        }

        private void OnMoreButtonClicked(FeedVideoModel model)
        {
            feedVideoOptions.gameObject.SetActive(true);
            feedVideoOptions.Initialize(model);
        }

        private void OpenTaggedUsersView(List<TaggedGroup> taggedGroups)
        {
            _popupManager.SetupPopup(new TaggedUsersPopupConfiguration(taggedGroups.ToArray()));
            _popupManager.ShowPopup(PopupType.TaggedUsers, true);
        }

        private void OnViewScrolledDown()
        {
            if (VideoModelIndex >= ContextData.FeedVideoModels.Count)
            {
                var last = FeedVideoViews.Last();
                if (last != null)
                {
                    last.CloseMedia();
                }

                return;
            }
            
            VideoModelIndex += 1;

            var videoIndexToPreload = VideoModelIndex + FeedScrollController.LAST_POSITION - FeedScrollController.VISIBLE_POSITION;
            
            if (VideoModelIndex < ContextData.FeedVideoModels.Count)
            {
                if (videoIndexToPreload < ContextData.FeedVideoModels.Count)
                {
                    var last = FeedVideoViews.Last();

                    if (last != null)
                    {
                        var item = new FeedLoadingItem(last, ContextData.FeedVideoModels[videoIndexToPreload]);
                        _loadingQueue.AddLastToLoadingQueue(item, true);
                    }
                }
            } 
            StopPreviousFeedItem();

            OnPageScrolled();
            
            var almostAtBottom = videoIndexToPreload == ContextData.FeedVideoModels.Count - VIDEOS_BEFORE_TRIGGER_ALMOST_REACHED_BOTTOM_EVENT;
            if (almostAtBottom)
            {
                OnScrolledAlmostToBottom?.Invoke();
            }
        }

        private void StopPreviousFeedItem()
        {
            if (FeedVideoViews[FeedScrollController.VISIBLE_POSITION - 1] != null)
            {
                FeedVideoViews[FeedScrollController.VISIBLE_POSITION - 1].RewindAndStop();
            }
        }

        private void OnViewScrolledUp()
        {
            if (VideoModelIndex <= 0)
            {
                if (FeedVideoViews[0] != null)
                {
                    FeedVideoViews[0].CloseMedia();
                }
                
                return;
            }

            VideoModelIndex -= 1;

            var videoIndexToPreload = VideoModelIndex - FeedScrollController.VISIBLE_POSITION;
            if (videoIndexToPreload >= 0 && FeedVideoViews[0] != null)
            {
                var item = new FeedLoadingItem(FeedVideoViews[0], ContextData.FeedVideoModels[videoIndexToPreload]);
                _loadingQueue.AddLastToLoadingQueue(item, true);
            }

            if (FeedScrollController.VISIBLE_POSITION + 1 < FeedVideoViews.Count 
                    && FeedVideoViews[FeedScrollController.VISIBLE_POSITION + 1] != null)
            {
                FeedVideoViews[FeedScrollController.VISIBLE_POSITION + 1].RewindAndStop();
            }
            
            OnPageScrolled();

            var hasScrolledToTop = videoIndexToPreload == 0;
            if (hasScrolledToTop)
            {
                OnScrolledToTop?.Invoke();
            }
        }

        private void OnPageScrolled()
        {
            if (VideoModelIndex >= ContextData.FeedVideoModels.Count)
            {
                _videoLoadingIndicator.Switch(false);
                OnScroll?.Invoke();
                return;
            }
            
            SetupTargetVideoView();

            if (_targetVideoView == null)
            {
                _videoLoadingIndicator.Switch(false);
                OnScroll?.Invoke();
                return;
            }
            
            _videoLoadingIndicator.Switch(!_targetVideoView.IsReady);
            if (!_targetVideoView.IsReady)
            {
                _targetVideoView.OnVideoReadyToPlayEvent += OnVideoReady;
                _targetVideoView.OnVideoNotAvailableEvent += OnVideoReady;
            }
            _targetVideoView.PlayVideo();   
            OnScroll?.Invoke();
            OnVideoStartedPlaying?.Invoke(_targetVideoView.ContextData);

            void OnVideoReady(FeedVideoView view)
            {
                view.OnVideoReadyToPlayEvent -= OnVideoReady;
                _targetVideoView.OnVideoNotAvailableEvent -= OnVideoReady;
                
                var isStillShown = _targetVideoView == view;
                if (isStillShown)
                {
                    _videoLoadingIndicator.Switch(false);
                }
            }
        }

        private void SetupTargetVideoView()
        {
            _stopWatch = _stopWatchProvider.GetStopWatch();
            _stopWatch.Restart();

            if (_targetVideoView) _targetVideoView.IsTarget = false;
            _targetVideoView = FeedVideoViews[FeedScrollController.VISIBLE_POSITION];

            if (_targetVideoView == null)
            {
                return;
            }

            _targetVideoView.IsTarget = true;
            
            _targetVideoView.TaggedUsersButtonSubscribe(OpenTaggedUsersView);
            _targetVideoView.AddCommentsButtonOnClickListener(OpenComments);
            
            var videoId = _targetVideoView.ContextData.Video.Id;
            _commentsView.VideoId = videoId;

            RefreshTargetVideoKPI();
            _targetVideoView.RefreshVideoPrivacy(_targetVideoView.ContextData.Video.Access);
            AddVideoToViewTracker(videoId);
            
            if (ContextData.CommentInfo != null)
            {
                _commentsView.OpenWithCommentOnTop(ContextData.CommentInfo);
            }

            if (!_targetVideoView.IsOwner)
            {
                _targetVideoView.HideRewardBadge();
                return;
            }

            ShowRewardBadge(_targetVideoView);
            ShowRewardPopupIfNecessary(_targetVideoView);
        }
        
        private bool IsModelIndexValid(int index)
        {
            return index >= 0 && index < ContextData.FeedVideoModels.Count;
        }

        private void InitializeFirstVideos()
        {
            _loadingQueue.Clear();
            
            // preventing case when user redirects to Notification page immediately after display start
            if (!gameObject.activeInHierarchy) return;

            // Do nothing if there is no videos from backend
            if (!ContextData.FeedVideoModels.Any()) return;

            // First visible video
            FeedLoadingItem item;
            if (FeedVideoViews[FeedScrollController.VISIBLE_POSITION] != null)
            {
                item = new FeedLoadingItem(FeedVideoViews[FeedScrollController.VISIBLE_POSITION], ContextData.FeedVideoModels[VideoModelIndex], true);
                _loadingQueue.AddLastToLoadingQueue(item);
            }  

            // Rest of videos in feed
            for (var i = 0; i < FeedVideoViews.Count; i++)
            {
                if (i == FeedScrollController.VISIBLE_POSITION || FeedVideoViews[i] == null) continue;

                var videoModelIndex = VideoModelIndex + i - FeedScrollController.VISIBLE_POSITION;
                
                if (!IsModelIndexValid(videoModelIndex))
                {
                    FeedVideoViews[i].CloseMedia();
                    continue;
                }

                item = new FeedLoadingItem(FeedVideoViews[i], ContextData.FeedVideoModels[videoModelIndex]);
                _loadingQueue.AddLastToLoadingQueue(item, true);
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            feedScroll.CleanUp();
            UnsubscribeFromEvents();
        }

        private void OnFirstVideoViewItemInitialized(FeedVideoView view)
        {
            ShowBody(true);
            if (_isFirstVideoLoaded) return;

            _isFirstVideoLoaded = true;
            OnFirstVideoLoadedEvent?.Invoke();
            OnVideoStartedPlaying?.Invoke(view.ContextData);
        }

        private void ShowBody(bool value)
        {
            body.alpha = value ? 1f : 0f;
        }

        private void SubscribeToEvents()
        {
            UnSubscribeVideoViewEvents();
            SubscribeVideoViewEvents();
            
            feedScroll.OnViewScrolledUpEvent += OnViewScrolledUp;
            feedScroll.OnViewScrolledDownEvent += OnViewScrolledDown;
            feedScroll.OnScrolledNextOnBottom += ScrolledNextOnBottom;
            feedScroll.ViewRecycled += OnViewRecycled;
            feedScroll.OnRefreshDragPerformedEvent += OnRefreshDragPerformed;
            feedVideoOptions.OnDeletionConfirmedEvent += DeleteVideo;
            feedVideoOptions.OnPrivacyChangedEvent += RefreshVideoPrivacy;
            feedVideoOptions.OnEditTemplateButton += OnEditTemplateButtonClicked;
            _commentsView.OnCommentPublished += RefreshTargetVideoKPI;
            _commentsView.OnOpen += RefreshTargetVideoKPI;
            _publishHelper.VideoPublished += VideoPublished;
        }

        private void SubscribeVideoViewEvents()
        {
            if (FeedVideoViews == null) return;
            
            for (var index = 0; index < FeedVideoViews.Count; index++)
            {
                var view = FeedVideoViews[index];

                if (view == null)
                {
                    continue;
                }
                    
                if (index == FeedScrollController.VISIBLE_POSITION)
                {
                    view.OnVideoReadyToPlayEvent += OnFirstVideoViewItemInitialized;
                    view.OnVideoNotAvailableEvent += OnFirstVideoViewItemInitialized;
                }

                view.OnMoreButtonClickedEvent += OnMoreButtonClicked;
                view.OnOpenProfileButtonClickedEvent += OnOpenProfileButtonClicked;
                view.OnTemplateDisabledButton += OnEditTemplateButtonClicked;

                #if TV_BUILD
                    if (AppSettings.FeedAutoScroll)
                    {
                        view.OnVideoFinishedPlayingEvent += OnVideoFinishedPlaying;
                    }
                #endif
            }
        }

        private void OnViewRecycled(bool isTop)
        {
            var elementToRemove = FeedVideoViews[isTop ? 0 : FeedVideoViews.Count - 1];

            if (elementToRemove == null)
            {
                return;
            }
            
            _loadingQueue.RemoveFromQueue(elementToRemove);
        }

        private void VideoPublished(Video video)
        {
            if (!_pageManager.IsCurrentPage(PageId.Feed)) return;
            
            StopPlayingVideos();
            
            OnVideoPublished?.Invoke();
        }

        private void UnsubscribeFromEvents()
        {
            feedScroll.OnViewScrolledUpEvent -= OnViewScrolledUp;
            feedScroll.OnViewScrolledDownEvent -= OnViewScrolledDown;
            feedScroll.OnScrolledNextOnBottom -= ScrolledNextOnBottom;
            feedScroll.OnRefreshDragPerformedEvent -= OnRefreshDragPerformed;
            feedVideoOptions.OnDeletionConfirmedEvent -= DeleteVideo;
            feedVideoOptions.OnEditTemplateButton -= OnEditTemplateButtonClicked;
            feedScroll.ViewRecycled -= OnViewRecycled;
            _commentsView.OnCommentPublished -= RefreshTargetVideoKPI;
            _commentsView.OnOpen -= RefreshTargetVideoKPI;
            _publishHelper.VideoPublished -= VideoPublished;
        }

        private void UnSubscribeVideoViewEvents()
        {
            if (FeedVideoViews == null) return;
            
            foreach (var view in FeedVideoViews)
            {
                if (view == null)
                {
                    continue;
                }
                    
                view.OnVideoReadyToPlayEvent -= OnFirstVideoViewItemInitialized;
                view.OnVideoNotAvailableEvent -= OnFirstVideoViewItemInitialized;
                view.OnMoreButtonClickedEvent -= OnMoreButtonClicked;
                view.OnOpenProfileButtonClickedEvent -= OnOpenProfileButtonClicked;
                view.OnVideoFinishedPlayingEvent -= OnVideoFinishedPlaying;
                view.OnTemplateDisabledButton -= OnEditTemplateButtonClicked;
            }
        }

        private void AddVideoToViewTracker(long videoId)
        {
            _videoViewTracker.Add(videoId, ContextData.FeedType, ContextData.FeedName);
        }

        private List<FeedVideoModel> GetVideoModels(Video[] videos)
        {
            var feedVideoModels = new List<FeedVideoModel>(videos.Length);
            feedVideoModels.AddRange(videos.Select(GetVideoModelArgs).Select(feedVideoModelArgs => new FeedVideoModel(feedVideoModelArgs)));

            return feedVideoModels;
        }

        private void RegisterBelowVideosToLoadingQueue(FeedVideoModel[] newVideos)
        {
            foreach (var videoModel in newVideos)
            {
                var emptyView = FeedVideoViews.FirstOrDefault(x => x.ContextData == null);
                if (emptyView == null) break;

                var item = new FeedLoadingItem(emptyView, videoModel);
                _loadingQueue.AddLastToLoadingQueue(item, true);
            }
        }
        
        private void RegisterAboveVideosToLoadingQueue(FeedVideoModel[] newVideos)
        {
            foreach (var videoModel in newVideos)
            {
                var emptyView = FeedVideoViews.LastOrDefault(x => x.ContextData == null);
                if (emptyView == null) break;

                var item = new FeedLoadingItem(emptyView, videoModel);
                _loadingQueue.AddFirstToLoadingQueue(item, true);
            }
        }

        private FeedVideoModelArgs GetVideoModelArgs(Video video)
        {
            long taskId = 0;
            if (ContextData.FeedType == VideoListType.Task)
            {
                taskId = video.TaskId;
            }
                
            return new FeedVideoModelArgs
            {
                Video = video,
                FileLocation = MediaPathType.AbsolutePathOrURL,
                CanUseAsTemplate = ContextData.CanUseVideosAsTemplate,
                ShowBasedOnTemplateButton = ContextData.ShowBasedOnTemplateButton,
                IsPostedDateAndViewsAmountActive = ContextData.ShowViews,
                IsNavBarActive = ContextData.IsNavBarActive,
                HashtagInfo = ContextData.HashtagInfo,
                TaskId = taskId,
                ShowActionsBar = ContextData.ShowActionsBar,
                ShowVideoDescription = ContextData.ShowVideoDescription,
                ShowChallengeButtonInDescription = ContextData.FeedType != VideoListType.Task,
                ScaleMode = ContextData.ScaleMode
            };
        }

        private void ScrolledNextOnBottom()
        {
            OnScrolledNextOnBottom?.Invoke();
        }

        private void OnEditTemplateButtonClicked()
        {
            var video = _targetVideoView.ContextData.Video;
            if (!video.TemplateFromVideo.AllowFeature)
            {
                var reason = GetTemplateGenerationUnavailableReason(video);
                _snackBarHelper.ShowInformationSnackBar(reason);
                return;
            }
            
            if (_targetVideoView.ContextData.Video.GeneratedTemplateId.HasValue)
            {
                if (_targetVideoView.ContextData.VideoAccess == VideoAccess.Public)
                {
                    OpenTemplatePage();
                }
                else
                {
                    OpenPrivacyRequiredPopup();
                }
                return;
            }

            OpenGenerateTemplatePage();
        }

        private async Task<bool> ChangeVideoPrivacy(VideoAccess access)
        {
            var response = await _bridge.ChangePrivacyAsync(_targetVideoView.ContextData.Video.Id, new UpdateVideoAccessRequest
            {
                Access = access
            });

            _videoManager.RefreshVideoKPIInCache(_targetVideoView.ContextData.Video.Id, default);
            
            if (!response.IsSuccess)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.TemplatePrivacyUpdateFailedSnackbarMessage);
            }
            
            _snackBarHelper.ShowInformationSnackBar(_localization.TemplatePrivacyUpdateSuccessSnackbarMessage);

            return response.IsSuccess;
        }
        
         private void OpenPrivacyRequiredPopup()
        {
            var config = new DialogDarkPopupConfiguration()
            {
                Title = _localization.TemplateChangePrivacyPopupTitle,
                Description = _localization.TemplateChangePrivacyPopupDescription,
                YesButtonText = _localization.TemplateChangePrivacyPopupConfirmButton,
                NoButtonText = _localization.TemplateChangePrivacyPopupCancelButton,
                OnYes = PrepareVideoForTemplate,
                PopupType = PopupType.DialogDark
            };
            
            _popupManager.PushPopupToQueue(config);
        }

        private async void PrepareVideoForTemplate()
        {
            var changePrivacyResult = await ChangeVideoPrivacy(VideoAccess.Public);
            
            if(!changePrivacyResult) return;
            
            if (_targetVideoView.ContextData.Video.GeneratedTemplateId.HasValue)
            {
                OpenTemplatePage();
            }
            else
            {
                OpenGenerateTemplatePage();
            }
        }
        
        private void OpenGenerateTemplatePage()
        {
            var editTemplatePageArgs = new EditTemplateFeedPageArgs
            {
                NameUpdatedCallback = OnGenerateTemplateConfirmed,
                SetVideoPublicCallback = SetVideoPublicCallback,
                TemplateName = _targetVideoView.ContextData.Video.MainTemplate?.Title ?? string.Empty,
                IsVideoPublic = _targetVideoView.ContextData.Video.Access == VideoAccess.Public,
                IsTemplateCreationUnlocked = true
            };

            _pageManager.MoveNext(PageId.EditTemplateFeed, editTemplatePageArgs);
        }

        private async void SetVideoPublicCallback()
        {
            await ChangeVideoPrivacy(VideoAccess.Public);
        }

        private async void OpenTemplatePage()
        {
            if (_targetVideoView.ContextData.Video.GeneratedTemplateId == null)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.TemplateNotFoundSnackbarMessage);
                return;
            }
            
            var response = await _bridge.GetEventTemplate(_targetVideoView.ContextData.Video.GeneratedTemplateId.Value);

            if (response.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.TemplateNotAvailableSnackbarMessage);
                return;
            }

            var pageArgs = new VideosBasedOnTemplatePageArgs
            {
                TemplateInfo = response.Model,
                TemplateName = response.Model.Title,
                UsageCount = response.Model.UsageCount
            };

            _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, pageArgs);
        }

        private async void OnGenerateTemplateConfirmed(bool generateTemplate, string templateName)
        {
            if (!generateTemplate) return;
            
            if (_targetVideoView.ContextData.VideoAccess != VideoAccess.Public)
            {
                var privacyResult = await _bridge.ChangePrivacyAsync(_targetVideoView.VideoId, new UpdateVideoAccessRequest()
                {
                    Access = VideoAccess.Public
                });

                if (!privacyResult.IsSuccess)
                {
                    _snackBarHelper.ShowInformationSnackBar(_localization.TemplatePrivacyUpdateFailedSnackbarMessage);
                    return;
                }
            }
            
            var result = await _bridge.GenerateTemplate(_targetVideoView.ContextData.Video.Id, templateName);
            
            if (result.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar(result.ErrorMessage);
                return;
            }
            
            _snackBarHelper.ShowInformationSnackBar(_localization.TemplateGenerationSuccessSnackbarMessage);
            
            var args = new VideosBasedOnTemplatePageArgs()
            {
                TemplateName = result.Model.Title,
                TemplateInfo = result.Model
            };

            _videoManager.RefreshVideoKPIInCache(_targetVideoView.ContextData.Video.Id);
            
            _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, args);
        }
        
        private string GetTemplateGenerationUnavailableReason(Video video)
        {
            var restrictionReason = video.TemplateFromVideo.RestrictionReason; 
            if (!restrictionReason.HasValue)
            {
                return _localization.TemplateGenerationFailedSnackbarMessage;
            }
            var reasons = _dataFetcher.MetadataStartPack.TemplateFromVideoRestrictionReasons;
            var reasonType = restrictionReason.Value;
            return reasons[reasonType];
        }

        private void ShowRewardBadge(FeedVideoView targetVideoView)
        {
            targetVideoView.ShowRewardBadge();
        }

        private void ShowRewardPopupIfNecessary(FeedVideoView targetVideoView)
        {
            if (targetVideoView.Video.RatingResult is not { IsRewardAvailable: true }) return;
            
            var popupConfig = new VideoRatingRewardPopupConfiguration(targetVideoView.Video.Id, targetVideoView.Video.RatingResult); 
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(popupConfig.PopupType);
        }
    }
}