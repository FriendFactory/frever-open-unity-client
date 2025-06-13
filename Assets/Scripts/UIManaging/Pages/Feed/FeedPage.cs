using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Coffee.UIExtensions;
using DG.Tweening;
using Extensions;
using Modules.Amplitude;
using Modules.Crew;
using Modules.InputHandling;
using Modules.QuestManaging;
using Modules.VideoStreaming.Feed;
using Navigation.Args.Feed;
using Navigation.Core;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common;
using UIManaging.Localization;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.FavoriteSounds;
using UIManaging.Pages.Feed.Core;
using UIManaging.Pages.Feed.Core.VideoViewTracking;
using UIManaging.Pages.Feed.Ui.Feed;
using UIManaging.Pages.VideosBasedOnTemplatePage;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Feed
{
    internal class FeedPage : GenericPage<BaseFeedArgs>
    {
        private const float RAYCAST_TARGET_ALPHA = 0.5f;
        
        private static bool _isEnteringTipShown = false;
        
        [SerializeField] private FeedManagerView _feedManagerView;
        [SerializeField] private GameObject _loadingVideosGameObject;
        [SerializeField] private GameObject _navigationBar;
        [SerializeField] private GameObject _bottomBar;
        [SerializeField] private GameObject _videoProgressBar;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _refreshCurrentTabButton;
        [SerializeField] private Button _notificationButton;
        [SerializeField] private Button _discoveryButton;
        [SerializeField] private TabsManagerView _tabsManager;
        [SerializeField] private VideoViewAutoSender _videoViewAutoSender;
        [SerializeField] private UIParticle _confettiParticles;
        [SerializeField] private QuestButton _questButton;
        [SerializeField] private SlideInOutBehaviour _slideInOutBehaviour;
        [SerializeField] private Image _raycastBlocker;
        
        [SerializeField] private UseMessageTemplateButton _useMessageTemplateButton;
        [SerializeField] private UseTemplateButton _useTemplateButton;
        [SerializeField] private UseTemplateButton _startChallengeButton;
        [SerializeField] private TMP_Text _startChallengeButtonText;
        [SerializeField] private UseForRemixButton _useForRemixButton;
        [SerializeField] private UseFavoriteSoundButton _useFavoriteSoundButton;
        [SerializeField] private FeedAddCommentPanel _addCommentPanel;
        
        [Header("For Me")]
        [SerializeField] private Button _forMeNotificationButton;
        [SerializeField] private Button _forMeDiscoveryButton;
        [SerializeField] private TabsManagerView _forMeTabsManager;

        [Inject] private FollowersManager _followersManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private VideoViewTracker _videoViewTracker;
        [Inject] private IProfilesProvider _profilesProvider;
        [Inject] private IInputManager _inputManager;
        [Inject] private IBridge _bridge;
        [Inject] private IQuestManager _questManager;
        [Inject] private FeedPopupHelper _feedPopupHelper;
        [Inject] private CrewService _crewService;
        [Inject] private FeedLocalization _localization;

        private FeedModelsDownloader _feedModelsDownloader;
        private FeedTabModel[] _tabModels;
        private bool _isForMeVariant;
        private TabsManagerView _activeTabsManagerView;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.Feed;

        [SuppressMessage("ReSharper", "InvertIf")]
        protected override BaseFeedArgs BackToPageArgs
        {
            get
            {
                var backToFeedArgs = (BaseFeedArgs) OpenPageArgs.Clone();

                if (_feedManagerView.ContextData != null && _feedManagerView.ContextData.FeedVideoModels.Any())
                {
                    var modelIndex = _feedManagerView.VideoModelIndex;
                    modelIndex = Mathf.Min(modelIndex, _feedManagerView.ContextData.FeedVideoModels.Count - 1);
                    var videoId = _feedManagerView.ContextData.FeedVideoModels[modelIndex].Video.Id;
                    backToFeedArgs.SetIdOfFirstVideoToShow(videoId);
                }

                return backToFeedArgs;
            }
        }

        internal VideoListType CurrentVideoType => OpenPageArgs.VideoListType;
        
        //---------------------------------------------------------------------
        // Events 
        //---------------------------------------------------------------------

        public event Action<VideoListType> TabSelectionStarted;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _backButton.interactable = true;
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _refreshCurrentTabButton.onClick.AddListener(OnRefreshCurrentTabButtonClicked);
            _useForRemixButton.ButtonClicked += _feedManagerView.PauseCurrentVideo;
            
            _questButton.Button.onClick.AddListener(OnQuestButton);
            _questButton.HidingBegin += OnQuestHidingBegin;
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _refreshCurrentTabButton.onClick.RemoveListener(OnRefreshCurrentTabButtonClicked);
            _useForRemixButton.ButtonClicked -= _feedManagerView.PauseCurrentVideo;

            if (OpenPageArgs.BlockScrolling)
            {
                _inputManager.Enable(true);
            }
            
            _questButton.Button.onClick.RemoveListener(OnQuestButton);
            _questButton.HidingBegin -= OnQuestHidingBegin;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _feedManagerView.OnFirstVideoLoadedEvent -= OnFirstVideoLoaded;
            _feedManagerView.OnRefreshDragPerformedEvent -= RefreshFeed;
            _feedManagerView.OnVideoDeleted -= OpenPageArgs.OnVideoDeleted;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _feedModelsDownloader = new FeedModelsDownloader(_feedManagerView);
        }

        protected override void OnDisplayStart(BaseFeedArgs args)
        {
            var endSeasonPopupIsShowing = _feedPopupHelper.TryShowEndedSeasonPopup();
            var seasonPopupIsShowing = !endSeasonPopupIsShowing && _feedPopupHelper.TryShowSeasonStartPopup(ForceShowTip); // The popup won't be shown if EndSeasonPopup is displayed!
            args.ShowHintsOnDisplay = !seasonPopupIsShowing && args.ShowHintsOnDisplay; // call before base.OnDisplayStart to affect hints behaviour
            
            base.OnDisplayStart(args);
            
            DisableBottomButtons();
            SetupInputManager(args);
            ForMeSetup();
            InitializeTabs();
            ShowLoadingIcon(true);
            _profilesProvider.ResetCache();
            _feedModelsDownloader.OnDisplayStart(args);

            _feedManagerView.BlockScrolling = args.BlockScrolling;
            _feedManagerView.ShouldRefreshOnVideoDeleted = OpenPageArgs.ShouldRefreshOnVideoDeleted();
            
            _feedManagerView.OnFirstVideoLoadedEvent += OnFirstVideoLoaded;
            _feedManagerView.OnRefreshDragPerformedEvent += RefreshFeed;
            _feedManagerView.OnVideoDeleted += OpenPageArgs.OnVideoDeleted;
            _feedManagerView.OnVideoStartedPlaying += OnVideoStartedPlaying;

            Manager.PageSwitchingBegan += OnMovingFromPageStarted;

            _activeTabsManagerView.gameObject.SetActive(args.ShouldShowTabs());
            _navigationBar.SetActive(args.ShouldShowNavigationBar());
            _videoProgressBar.SetActive(args.ShouldShowNavigationBar());
            
            //_bottomBar.SetActive(OpenPageArgs.ShowVideoDescription);
  
            _addCommentPanel.SetActive(false);
            
            if (OpenPageArgs.ShouldShowNavigationBar())
            {
                _navigationBar.SetActive(true);
            }

            var showBackButton = !args.ShouldShowNavigationBar() && args.ShowBackButton;
            _backButton.gameObject.SetActive(showBackButton);
            _backButton.interactable = showBackButton;

            _questButton.SetActive(args.ShowQuestsButton);
            if (args.ShowQuestsButton)
            {
                _questManager.UpdateQuestData();
            }

            if (!endSeasonPopupIsShowing && !seasonPopupIsShowing && !_feedPopupHelper.IsPopupDisplayed(ForceShowTip))
            {
                ForceShowTip();
            }

            _slideInOutBehaviour.Show();
            _raycastBlocker.SetActive(false);

            PrefetchLocalUserData();
            args.PageLoaded?.Invoke();

            void ForceShowTip()
            {
                if (!_isEnteringTipShown && !_questManager.IsQuestCompleted(QuestType.CREATE_VIDEO))
                {
                    _questManager.GoToQuestTarget(QuestType.FEED_SWIPE);
                    _isEnteringTipShown = true;
                }
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            if (_feedManagerView != null)
            {
                _feedManagerView.OnFirstVideoLoadedEvent -= OnFirstVideoLoaded;
                _feedManagerView.OnRefreshDragPerformedEvent -= RefreshFeed;
                _feedManagerView.OnVideoDeleted -= OpenPageArgs.OnVideoDeleted;
                _feedManagerView.OnVideoStartedPlaying -= OnVideoStartedPlaying;
            }
            
            Manager.PageSwitchingBegan -= OnMovingFromPageStarted;

            _feedModelsDownloader.OnHidingBegin();
            _videoViewAutoSender.Send();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnQuestButton()
        {
            _raycastBlocker.SetActive(true);
            DOTween.To(t => _raycastBlocker.SetAlpha(t), 0, RAYCAST_TARGET_ALPHA, 0.4f).SetEase(Ease.InOutQuad);
            _slideInOutBehaviour.SlideOut();
            _feedManagerView.PauseCurrentVideo();
        }

        private void OnQuestHidingBegin()
        {
            if (_questManager.IsComplete)
            {
                _questButton.Button.interactable = false;
            }
            
            DOTween.To(t => _raycastBlocker.SetAlpha(t), RAYCAST_TARGET_ALPHA, 0, 0.4f).SetEase(Ease.InOutQuad)
                   .OnComplete(() => _raycastBlocker.SetActive(false));
            
            _slideInOutBehaviour.SlideIn(() =>
            {
                _feedManagerView.ResumeCurrentVideo();

                if (!_questManager.IsComplete)
                {
                    return;
                }
                
                _questButton.QuestCompleteAnimation();
            });
        }
        
        private void ForMeSetup()
        {
            _isForMeVariant = AmplitudeManager.IsForMeFeatureEnabled();

            _activeTabsManagerView = _isForMeVariant ? _forMeTabsManager : _tabsManager;
            _activeTabsManagerView.SetActive(true);

            if (_isForMeVariant)
            {
                _notificationButton.SetActive(false);
                _discoveryButton.SetActive(false);
                _tabsManager.SetActive(false);
                _forMeNotificationButton.gameObject.SetActive(OpenPageArgs.ShouldShowNotificationButton());
                _forMeDiscoveryButton.gameObject.SetActive(OpenPageArgs.ShouldShowDiscoveryButton());

                _tabModels = new[]
                {
                    new FeedTabModel(0, _localization.FeedTabForMe, VideoListType.ForMe),
                    new FeedTabModel(1,  _localization.FeedTabFollowing, VideoListType.Following),
                    new FeedTabModel(2,  _localization.FeedTabFriends, VideoListType.Friends)
                };
            }
            else
            {
                _forMeNotificationButton.SetActive(false);
                _forMeDiscoveryButton.SetActive(false);
                _forMeTabsManager.SetActive(false);
                _notificationButton.gameObject.SetActive(OpenPageArgs.ShouldShowNotificationButton());
                _discoveryButton.gameObject.SetActive(OpenPageArgs.ShouldShowDiscoveryButton());

                _tabModels = new[]
                {
                    new FeedTabModel(0, _localization.FeedTabFeatured, VideoListType.Featured),
                    new FeedTabModel(1, _localization.FeedTabNew, VideoListType.New, true, false),
                    new FeedTabModel(2, _localization.FeedTabFollowing, VideoListType.Following),
                    new FeedTabModel(3, _localization.FeedTabFriends, VideoListType.Friends)
                };
            }
        }

        private void GetListOfVideos(long? videoId = null)
        {
            if (IsDestroyed) return;
            
            ShowLoadingIcon(true);
            _feedModelsDownloader.GetListOfVideos(videoId, OnVideosDownloaded);
        }

        private void OnVideosDownloaded(IReadOnlyCollection<Video> videos)
        {
            if (_feedModelsDownloader.CancelledRequest) return;
            
            SetVideos(videos);

            if (videos.Count == 0)
            {
                ShowLoadingIcon(false);
            }
        }

        private void InitializeTabs()
        {
            var selectedTab = GetTab(OpenPageArgs.VideoListType);
            
            // not sure why we initialize tabs even for Feed w/o tabs, but here is a fast way to skip the initialization 
            if (selectedTab == null) return;

            if (_activeTabsManagerView.TabsManagerArgs != null)
            {
                var selectedTabIndex = _activeTabsManagerView.TabsManagerArgs.SelectedTabIndex;
                selectedTab = GetTab(selectedTabIndex);
            }

            OpenPageArgs.SetVideoListType(selectedTab.Type);

            var tabsManagerArgs = new TabsManagerArgs(_tabModels, selectedTab.Index);
            _activeTabsManagerView.Init(tabsManagerArgs);
            _activeTabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _activeTabsManagerView.TabSelectionStarted -= OnTabSelectionStarted;
            _activeTabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
            _activeTabsManagerView.TabSelectionStarted += OnTabSelectionStarted;
        }

        private void OnTabSelectionStarted(int tabIndex)
        {
            var selectedVideoType = GetTab(tabIndex).Type;
            var selectedSameTab = selectedVideoType == CurrentVideoType;
            var currentVideoIndex = _feedManagerView.VideoModelIndex;
            
            switch (CurrentVideoType)
            {
            case VideoListType.ForMe:
            case VideoListType.New:
                if (selectedSameTab)
                {
                    var nextVideoIndex = currentVideoIndex + 1;
                    currentVideoIndex = nextVideoIndex;
                    var lastSeenVideoId = _videoViewTracker.GetLastSeenVideoId(CurrentVideoType, true);
                    _videoManager.ClearAllVideosBefore(CurrentVideoType, lastSeenVideoId);
                }

                break;
            default:
                _videoManager.ForgetPreviousVideos(CurrentVideoType);
                break;
            }
                
            long lastShownVideoId = 0;
            var feedVideoModels = _feedManagerView.ContextData?.FeedVideoModels;
            if (feedVideoModels?.Count > 0)
            {
                lastShownVideoId = feedVideoModels[Mathf.Min(currentVideoIndex, feedVideoModels.Count - 1)].Video.Id;
            }

            OpenPageArgs.SetLastShownForVideoListType(CurrentVideoType, lastShownVideoId);
            
            TabSelectionStarted?.Invoke(selectedVideoType);
        }

        private void OnTabSelectionCompleted(int tabIndex)
        {
            SendFeedTabSelectedAmplitudeEvent(tabIndex);
            
            var feedType = (VideoListType)tabIndex;
            switch (feedType)
            {
                case VideoListType.Following:
                case VideoListType.Friends:
                    OnTabSelected(tabIndex);
                    break;
                case VideoListType.Featured:
                case VideoListType.New:
                case VideoListType.ForMe:
                    OnTabSelected(tabIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void OnTabSelected(int tabIndex)
        {
            var selectedTab = GetTab(tabIndex);
            var currentTab = GetTab(OpenPageArgs.VideoListType);
            var isSameTab = tabIndex == currentTab.Index;
            if (!isSameTab)
            {
                _feedManagerView.CloseAllVideos();
            }
            OpenPageArgs.SetVideoListType(selectedTab.Type);
            var idOfFirstVideoToShow = isSameTab ? null : OpenPageArgs.GetLastShownVideoListType(selectedTab.Type);
            OpenPageArgs.SetIdOfFirstVideoToShow(idOfFirstVideoToShow);
            GetListOfVideos(idOfFirstVideoToShow);
        }

        private void SetVideos(IReadOnlyCollection<Video> videos)
        {
            var feedVideoModels = new List<FeedVideoModel>(videos.Count);

            long taskId = 0;

            var forMeOnGeneralFeed = OpenPageArgs is GeneralFeedArgs && CurrentVideoType == VideoListType.ForMe;
            var isPostedDateAndViewsAmountActive = !forMeOnGeneralFeed;

            if (OpenPageArgs is TaskFeedArgs taskFeedArgs)
            {
                taskId = taskFeedArgs.TaskId;
                isPostedDateAndViewsAmountActive = false;
            }
            
            foreach (var video in videos)
            {
                var feedVideoModelArgs = new FeedVideoModelArgs
                {
                    Video = video,
                    FileLocation = MediaPathType.AbsolutePathOrURL,
                    CanUseAsTemplate = OpenPageArgs.CanUseVideosAsTemplate,
                    ShowBasedOnTemplateButton = OpenPageArgs.ShouldShowUseBasedOnTemplateButton(),
                    HashtagInfo = (OpenPageArgs as HashtagFeedArgs)?.HashtagInfo,
                    TaskId = taskId,
                    IsNavBarActive = OpenPageArgs.ShouldShowNavigationBar(),
                    FollowButtonClick = OnFollowButtonClick,
                    IsPostedDateAndViewsAmountActive = isPostedDateAndViewsAmountActive,
                    OnJoinTemplateClick = OpenPageArgs.OnJoinTemplateClick,
                    ScaleMode = OpenPageArgs.ScaleMode,
                    ShowActionsBar = OpenPageArgs.ShowActionsBar,
                    ShowVideoDescription = OpenPageArgs.ShowVideoDescription,
                    ShowChallengeButtonInDescription = OpenPageArgs.VideoListType != VideoListType.Task,
                    Sound = (OpenPageArgs as VideosBasedOnSoundFeedArgs)?.Sound,
                };

                if (OpenPageArgs.JoinedChallenge != null)
                {
                    feedVideoModelArgs.JoinTaskButtonClick = OnJoinChallengeClick;
                }
                
                var feedVideoModel = new FeedVideoModel(feedVideoModelArgs);
                feedVideoModels.Add(feedVideoModel);
            }

            var idOfFirstVideoToShow = OpenPageArgs.IdOfFirstVideoToShow;
            var targetVideo = videos.FirstOrDefault(v => v.Id == idOfFirstVideoToShow);

            if (OpenPageArgs.FindVideoWithRemix && (targetVideo == null || !targetVideo.IsRemixable))
            {
                targetVideo = videos.FirstOrDefault(v => v.IsRemixable);
            }

            idOfFirstVideoToShow = targetVideo?.Id;
            var aspectMode = OpenPageArgs.ShowVideoDescription
                ? AspectRatioFitter.AspectMode.EnvelopeParent
                : AspectRatioFitter.AspectMode.FitInParent;
            var feedManagerArgs = new FeedManagerArgs(feedVideoModels, idOfFirstVideoToShow, isPostedDateAndViewsAmountActive, OpenPageArgs);
            _feedManagerView.Initialize(feedManagerArgs);
            //_feedManagerView.SetAspectRatioFitterMode(aspectMode);
        }

        private void OnFirstVideoLoaded()
        {
            ShowLoadingIcon(false);
            OpenPageArgs.FirstVideoLoaded?.Invoke();
        }

        private void ShowLoadingIcon(bool visible)
        {
            _refreshCurrentTabButton.interactable = !visible;
            _loadingVideosGameObject.SetActive(visible);
        }
        
        private void RefreshFeed()
        {
            switch (CurrentVideoType)
            {
                case VideoListType.ForMe:
                case VideoListType.New:
                    var lastSeenVideoId = _videoViewTracker.GetLastSeenVideoId(CurrentVideoType, true);
                    _videoManager.ClearAllVideosBefore(CurrentVideoType, lastSeenVideoId);
                    break;
                default:
                    _videoManager.ForgetPreviousVideos(CurrentVideoType);
                    break;
                
            }
            
            OpenPageArgs.SetLastShownForVideoListType(OpenPageArgs.VideoListType, 0);
            OpenPageArgs.SetIdOfFirstVideoToShow(null);
            GetListOfVideos();
        }

        private void OnRefreshCurrentTabButtonClicked()
        {
            RefreshFeed();
        }

        private void OnBackButtonClicked()
        {
            _backButton.interactable = false;
            Manager.MoveBack();
        }

        private void OnMovingFromPageStarted(PageId? current, PageData next)
        {
            if (!current.HasValue || current.Value != Id) return;
            _feedManagerView.CloseAllVideos();
        }

        private void SendFeedTabSelectedAmplitudeEvent(int tabIndex)
        {
            var feedTabMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.FEED_TAB_NAME] = _tabModels[tabIndex].Name
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.FEED_TAB_SELECTED, feedTabMetaData);
        }

        private string GetCurrentTabName()
        {
            return OpenPageArgs.ShouldShowTabs() ? GetTab(OpenPageArgs.VideoListType).Name : string.Empty;
        }

        private FeedTabModel GetTab(VideoListType type)
        {
            return _tabModels.FirstOrDefault(x => x.Type == type);
        }
        
        private FeedTabModel GetTab(int index)
        {
            return _tabModels.FirstOrDefault(x => x.Index == index);
        }
        
        private async void PrefetchLocalUserData()
        {
            await Task.Yield();
            await Task.Yield();
            await Task.WhenAll(_followersManager.PrefetchDataForLocalUser(),
                               _crewService.RefreshCrewDataAsync(default));
            GetListOfVideos(OpenPageArgs.IdOfFirstVideoToShow);
        }

        private void SetupInputManager(BaseFeedArgs args)
        {
            if(!args.BlockScrolling) return;
            
            _inputManager.Enable(false);
        }

        private void OnFollowButtonClick()
        {
            OpenPageArgs.FollowedAccount?.Invoke();
        }

        private void OnJoinChallengeClick()
        {
            OpenPageArgs.JoinedChallenge?.Invoke();
            
            if (!OpenPageArgs.ShowActionsBar)
            {
                _feedManagerView.StopPlayingVideos();
            }
        }

        private void OnVideoStartedPlaying(FeedVideoModel model)
        {
            SetupBottomPanel(model);
            
            var video = model.Video;
            
            if (video == null
             || !video.IsVotingTask 
             || video.BattleResult == null 
             || video.BattleResult.Place > 3)
            {
                return;
            }
            
            var key = $"confetti_{_bridge.Profile.GroupId}_{video.Id}";
            var playerPref = PlayerPrefs.GetInt(key, 0);
            
            if (playerPref == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                _confettiParticles.Play();
            }
        }

        private void SetupBottomPanel(FeedVideoModel model)
        {
            PrepareTemplateButtons(model);

            _addCommentPanel.SetActive(!OpenPageArgs.ShouldShowNavigationBar()
                                          && !_useTemplateButton.gameObject.activeSelf
                                          && !_startChallengeButton.gameObject.activeSelf
                                          && !_useForRemixButton.gameObject.activeSelf 
                                          && !_useFavoriteSoundButton.gameObject.activeSelf
                                          && !_useMessageTemplateButton.gameObject.activeSelf);
            
            _addCommentPanel.Initialize(new FeedAddCommentPanelModel(model.Video.AllowComment, _feedManagerView.OpenCommentsWithKeyboard));
        }

        private void DisableBottomButtons()
        {
           _useTemplateButton.gameObject.SetActive(false);
           _startChallengeButton.gameObject.SetActive(false);
           _useForRemixButton.gameObject.SetActive(false); 
           _useFavoriteSoundButton.gameObject.SetActive(false);
           _useMessageTemplateButton.gameObject.SetActive(false);
        }

        private void PrepareTemplateButtons(FeedVideoModel model)
        {
            PrepareUseThisTemplateButton(model);
            PrepareStartChallengeButton(model);
            PrepareRemixButton(model);
            PrepareUseFavoriteSoundButton(model);
            PrepareUseMessageButton(model);
        }

        private void PrepareUseMessageButton(FeedVideoModel model)
        {
            var isDisplayed = !OpenPageArgs.ShouldShowNavigationBar() && model.Video.IsPublishedAsMessage();
            _useMessageTemplateButton.SetActive(isDisplayed);
            if (!isDisplayed) return;
            _useMessageTemplateButton.Initialize(model.Video);
        }

        private void PrepareUseFavoriteSoundButton(FeedVideoModel model)
        {
            var showUseSoundButton = model.OpenedWithSound != null;

            if (!showUseSoundButton)
            {
                _useFavoriteSoundButton.SetActive(false);
                return;
            }
            
            _useFavoriteSoundButton.SetActive(true);
            _useFavoriteSoundButton.Initialize(model.OpenedWithSound);
        }
        
        private void PrepareUseThisTemplateButton(FeedVideoModel model)
        {
            _useTemplateButton.gameObject.SetActive(false);
            var showHashtagButton = model.OpenedWithHashtag != null;
            var showTemplateButton = !model.ShowBasedOnTemplateButton && model.Video.MainTemplate != null;
            if (!showHashtagButton && !showTemplateButton)
            {
                _useTemplateButton.SetActive(false);
                return;
            }

            _useTemplateButton.SetActive(true);
            
            if (showHashtagButton)
            {
                _useTemplateButton.Setup(model.OpenedWithHashtag);
            }
            else
            {
                _useTemplateButton.Setup(model.Video.MainTemplate.Id);
            }
        }
        
        private async void PrepareStartChallengeButton(FeedVideoModel model)
        {
            var taskId = model.Video.IsVotingTask ? model.Video.TaskId : model.OpenedWithTask;
            if (taskId == 0 || OpenPageArgs.VideoListType != VideoListType.Task)
            {
                _startChallengeButton.SetActive(false);
                return;
            }

            _startChallengeButton.SetActive(true);
            
            var result = await _bridge.GetTaskFullInfoAsync(taskId);
            if (result.IsError)
            {
                Debug.LogError($"Failed to retrieve task info for task {taskId}");
            }

            if (result.IsSuccess)
            {
                var isTaskFeed = model.OpenedWithTask != 0;
                _startChallengeButton.Setup(result.Model, model.Video.GroupId == _bridge.Profile.GroupId, isTaskFeed, model.JoinTaskButtonClick);
            }
        }
        
        private void PrepareRemixButton(FeedVideoModel model)
        {
            if (!model.Video.IsRemixable)
            {
                _useForRemixButton.gameObject.SetActive(false);
                return;
            }
            
            _useForRemixButton.gameObject.SetActive(model.CanUseForRemix);
            
            if (model.CanUseForRemix)
            {
                _useForRemixButton.Initialize(model);
            }
        }

    }
}