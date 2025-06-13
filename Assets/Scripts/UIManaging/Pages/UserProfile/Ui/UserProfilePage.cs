using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.VideoServer;
using Bridge.Services.UserProfile;
using Common.UserBalance;
using DG.Tweening;
using Extensions;
using Modules.FollowRecommendations;
using Modules.QuestManaging;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using Sirenix.OdinInspector;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.FollowersPage.UI;
using UIManaging.Pages.PublishPage;
using UIManaging.Pages.UserProfile.Ui.Buttons;
using UIManaging.Pages.UserProfile.Ui.ProfileHelper;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UserProfile.Ui
{
    internal sealed class UserProfilePage : GenericPage<UserProfileArgs>
    {
        private const int USER_POST_TAB_INDEX = 0;
        private const int USER_TAGGED_POST_INDEX = 2;
        private const int USER_TASK_POST_INDEX = 1;
        private const float RAYCAST_TARGET_ALPHA = 0.5f;

        public override PageId Id => PageId.UserProfile;

        [SerializeField] private RectTransform _navigationBarRectTransform;
        [SerializeField] private ProfileScrollablePanel _scrollablePanel;
        [SerializeField] private Button _backButton;
        [SerializeField] private GameObject _discoverPageButton;
        [SerializeField] private Button _openFollowersButton;
        [SerializeField] private Button _openFriendsButton;
        [SerializeField] private Button _openFollowedButton;
        [SerializeField] private FollowUserButton _followUserButton;
        [SerializeField] private ShareProfileButton _shareMyProfileButton;
        [SerializeField] private CanvasGroup _sendMessageCanvasGroup;
        [SerializeField] private Button _sendMessageButton;
        [SerializeField] private OpenEditProfileButton _editProfileButton;
        [SerializeField] private UserPortrait _smallPortraitView;
        [SerializeField] private UserPortrait _portraitView;
        [SerializeField] private NicknameView _nicknameView;
        [SerializeField] private GameObject _nicknameNotificationMark;
        [SerializeField] private RankBadgeView _rankBadgeView;
        [SerializeField] private ProfileKPIView _profileKPIView;
        [SerializeField] private UserProfileVideosGrid[] _userProfileVideosGrids;
        [SerializeField] private VideosTabsManagerView _tabsManagerView;
        [SerializeField] private CanvasGroup _userPostsCanvasGroup;
        [SerializeField] private CanvasGroup _userTaggedPostsCanvasGroup;
        [SerializeField] private CanvasGroup _userTaskPostsCanvasGroup;
        [SerializeField] private LocalUserProfileHelper _localProfileHelper;
        [SerializeField] private RemoteUserProfileHelper _remoteProfileHelper;
        [SerializeField] private GameObject _videoGridPlaceholder;
        [SerializeField] private MoreOptionsMenu _moreOptionsMenu;
        [SerializeField] private Button _moreOptionsButton;
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private Image _raycastBlocker;
        [SerializeField] private QuestButton _questButton;
        [SerializeField] private SlideInOutBehaviour _slideInOutBehaviour;
        [Header("Profile Info")]
        [SerializeField] [Required] private ProfileInfoPanel _profileInfoPanel;
        [SerializeField] private Button _crewButton;
        [SerializeField] private TMP_Text _crewNameText;
        [SerializeField] private ContentSizeFitter _crewButtonSizeFitter;

        private BaseUserProfileHelper _currentProfileHelper;

        private Vector2 _initialLastLevelsPanelSizeDelta;
        private readonly List<RectTransform> _levelsPanelRectTransforms = new List<RectTransform>();

        private Dictionary<int, bool> _videoTabLoadState;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private PageManager _pageManager;
        [Inject] private FollowersManager _followersManager;
        [Inject] private PublishVideoHelper _publishVideoHelper;
        [Inject] private FollowRecommendationsListModelProvider _recommendationsProvider;
        [Inject] private IQuestManager _questManager;
        [Inject] private SnackBarHelper _snackbarManager;
        [Inject] private ProfileLocalization _localization;
        [Inject] private PopupManager _popupManager;
        [Inject] private IScenarioManager _scenarioManager;

        private int _currentTabIndex;
        private bool _tabsWereInitialized;
        private TabModel[] _tabModels;
        private int _videoGridDownloadDataCount;
        private bool _isCreatingChat;

        private CancellationTokenSource _tokenSource;

        private TaskCompletionSource<object> _videoGridTaskCompletionSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public ProfileKPIView ProfileKpiView => _profileKPIView;
        public BaseUserProfileHelper UserProfileHelper => _currentProfileHelper;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            foreach (var videoGrid in _userProfileVideosGrids)
            {
                _levelsPanelRectTransforms.Add(videoGrid.GetComponent<RectTransform>());
            }

            _levelsPanelRectTransforms.Add(_videoGridPlaceholder.GetComponent<RectTransform>());
            _initialLastLevelsPanelSizeDelta = _levelsPanelRectTransforms.First().sizeDelta;
            _moreOptionsButton.onClick.AddListener(OnMoreOptionsClicked);
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _followersManager.LocalUserFollowed.OnStartedFollowingUserEvent += OnStartedFollowingUser;
            _followersManager.LocalUserFollowed.OnStoppedFollowingUserEvent += OnStoppedFollowingUser;
            _tabsManagerView.TabSelectionCompleted += OnTabSelected;
            _openFollowersButton.onClick.AddListener(OnOpenFollowersButtonClicked);
            _openFriendsButton.onClick.AddListener(OnOpenFriendsButtonClicked);
            _openFollowedButton.onClick.AddListener(OnOpenFollowedButtonClicked);
            _crewButton.onClick.AddListener(OpenCrewPage);
            _sendMessageButton.onClick.AddListener(OnSendMessageButtonClicked);
            _publishVideoHelper.VideoPublished += OnNewVideoPublished;
            _questButton.Button.onClick.AddListener(OnQuestButton);
            _questButton.HidingBegin += OnQuestHidingBegin;

            foreach (var videoGrid in _userProfileVideosGrids)
            {
                videoGrid.DataDownloaded += VideoGridDataDownloaded;
            }

            _videoGridDownloadDataCount = _userProfileVideosGrids.Length;
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _followersManager.LocalUserFollowed.OnStartedFollowingUserEvent -= OnStartedFollowingUser;
            _followersManager.LocalUserFollowed.OnStoppedFollowingUserEvent -= OnStoppedFollowingUser;
            _tabsManagerView.TabSelectionCompleted -= OnTabSelected;
            _openFollowersButton.onClick.RemoveListener(OnOpenFollowersButtonClicked);
            _openFriendsButton.onClick.RemoveListener(OnOpenFriendsButtonClicked);
            _openFollowedButton.onClick.RemoveListener(OnOpenFollowedButtonClicked);
            _crewButton.onClick.RemoveListener(OpenCrewPage);
            _sendMessageButton.onClick.RemoveListener(OnSendMessageButtonClicked);
            _publishVideoHelper.VideoPublished -= OnNewVideoPublished;
            _questButton.Button.onClick.RemoveListener(OnQuestButton);
            _questButton.HidingBegin -= OnQuestHidingBegin;

            foreach (var videoGrid in _userProfileVideosGrids)
            {
                videoGrid.DataDownloaded -= VideoGridDataDownloaded;
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager) { }

        protected override void OnDisplayStart(UserProfileArgs args)
        {
            _backButton.interactable = true;

            base.OnDisplayStart(args);

            var isLocalUser = args == null || args.GroupId == 0 || _bridge.Profile.GroupId == args.GroupId;
            SetupProfileHelper(isLocalUser);
            
            _discoverPageButton.SetActive(UserProfileHelper.IsCurrentUser);
            
            InitVideoTabLoadState();
            OnTabSelected(USER_POST_TAB_INDEX);
            _tokenSource = new CancellationTokenSource();

            gameObject.SetActive(true);

            ResetScrollablePanel();
            RefreshBackButton();
            RefreshNavigationBar();
            
            _currentProfileHelper.RefreshKPIView(_tokenSource.Token);
            
            RefreshLastLevelsPanelSize();

            StartLoadingUI(isLocalUser, _tokenSource.Token);

            if (_currentProfileHelper.IsCurrentUser)
            {
                var userBalanceModel = new StaticUserBalanceModel(_dataHolder);
                _userBalanceView.ContextData?.CleanUp();
                _userBalanceView.Initialize(userBalanceModel);
                _dataHolder.UpdateBalance(_tokenSource.Token);
                PrefetchFollowRecommendationData();
            } 
            else 
            {
                _userBalanceView.SetActive(false);
            }
            
            _questManager.UpdateQuestData();
            _slideInOutBehaviour.Show();
            
            _raycastBlocker.SetActive(false);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            CleanUpInternal();
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void SetupMessageButton()
        {
            var profile = _currentProfileHelper.Profile;
            var canSendMessage = !_currentProfileHelper.IsCurrentUser && profile.YouFollowUser && profile.UserFollowsYou 
                              && _dataHolder.UserProfile.ChatAvailableAfterTime <= DateTime.UtcNow
                              && _currentProfileHelper.Profile?.ChatAvailableAfterTime <= DateTime.UtcNow;
            _sendMessageCanvasGroup.alpha = canSendMessage ? 1.0f : 0.75f;
        }

        private void OnQuestButton()
        {
            _raycastBlocker.SetActive(true);
            DOTween.To(t => _raycastBlocker.SetAlpha(t), 0, RAYCAST_TARGET_ALPHA, 0.4f).SetEase(Ease.InOutQuad);
            _slideInOutBehaviour.SlideOut();
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
                _raycastBlocker.SetActive(false);

                if (!_questManager.IsComplete)
                {
                    return;
                }
                
                _questButton.QuestCompleteAnimation();
            });
        }
        
        private void OnBackButtonClicked()
        {
            _backButton.interactable = false;
            _pageManager.MoveBack();
        }

        private void OnStartedFollowingUser(Profile profile)
        {
            if (profile.MainGroupId == OpenPageArgs.GroupId)
            {
                _followersManager.GetRemoteUserFollower(OpenPageArgs.GroupId).PrefetchData();
            }
        }

        private void OnStoppedFollowingUser(Profile profile)
        {
            if (profile.MainGroupId == OpenPageArgs.GroupId)
            {
                _followersManager.GetRemoteUserFollower(OpenPageArgs.GroupId).PrefetchData();
            }
        }

        private void OnOpenFollowersButtonClicked()
        {
            OpenFollowersPage(BaseFollowersPageArgs.FOLLOWERS_TAB_INDEX);
        }
        
        private void OnOpenFriendsButtonClicked()
        {
            OpenFollowersPage(BaseFollowersPageArgs.FRIENDS_TAB_INDEX);
        }

        private void OnOpenFollowedButtonClicked()
        {
            OpenFollowersPage(BaseFollowersPageArgs.FOLLOWING_TAB_INDEX);
        }

        private void OpenFollowersPage(int initialTabIndex)
        {
            var followersPageArgs = _currentProfileHelper.GetFollowersPageArgs(initialTabIndex);
            _pageManager.MoveNext(PageId.FollowersPage, followersPageArgs);
        }

        private async void OnSendMessageButtonClicked()
        {
            if (_dataHolder.UserProfile.ChatAvailableAfterTime > DateTime.UtcNow)
            {
                var config = new DirectMessagesLockedPopupConfiguration();
                
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(config.PopupType);
                return;
            }
            
            if (_currentProfileHelper.Profile.ChatAvailableAfterTime > DateTime.UtcNow)
            {
                _snackbarManager.ShowMessagesLockedSnackBar(_localization.MessagesLockedSnackBar);
                return;
            }
            
            if (!_currentProfileHelper.Profile.YouFollowUser || !_currentProfileHelper.Profile.UserFollowsYou)
            {
                _snackbarManager.ShowInformationSnackBar(_localization.UnableToChatWithNotFriendSnackbarMessage);
                return;
            }

            if (_isCreatingChat)
            {
                return;
            }
        
            _isCreatingChat = true;

            var createResult = await _bridge.CreateChat(new SaveChatModel { GroupIds = new List<long> { OpenPageArgs.GroupId, _bridge.Profile.GroupId } });

            if (createResult.IsError)
            {
                Debug.LogError($"Failed to create chat, reason: {createResult.ErrorMessage}");
                _isCreatingChat = false;
                return;
            }

            if (createResult.IsSuccess)
            {
                var infoResult = await _bridge.GetChatById(createResult.CreatedChatId, _tokenSource.Token);

                _isCreatingChat = false;

                if (infoResult.IsRequestCanceled || _tokenSource == null || _tokenSource.Token.IsCancellationRequested)
                {
                    Debug.LogWarning($"Canceled chat info request.");
                    return;
                }
                
                if (infoResult.IsError)
                {
                    Debug.LogError($"Failed to receive chat info, reason: {infoResult.ErrorMessage}");
                    return;
                }

                if (infoResult.IsSuccess)
                {
                    Manager.MoveNext(new ChatPageArgs(infoResult.Model));
                }
            }
        }

        private void OnTabSelected(int tabIndex)
        {
            _currentTabIndex = tabIndex;
            
            switch (tabIndex)
            {
                case USER_POST_TAB_INDEX:
                    _userTaggedPostsCanvasGroup.SetActive(false);
                    _userTaskPostsCanvasGroup.SetActive(false);
                    _userPostsCanvasGroup.SetActive(true);
                    LoadVideoForTab(tabIndex, _currentProfileHelper.InitLevelsPanelArgs);
                    break;
                case USER_TAGGED_POST_INDEX:
                    _userPostsCanvasGroup.SetActive(false);
                    _userTaskPostsCanvasGroup.SetActive(false);
                    _userTaggedPostsCanvasGroup.SetActive(true);
                    LoadVideoForTab(tabIndex, _currentProfileHelper.InitTaggedLevelPanelArgs);
                    break;
                case USER_TASK_POST_INDEX:
                    _userPostsCanvasGroup.SetActive(false);
                    _userTaggedPostsCanvasGroup.SetActive(false);
                    _userTaskPostsCanvasGroup.SetActive(true);
                    LoadVideoForTab(tabIndex, _currentProfileHelper.InitTaskLevelPanelArgs);
                    break;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void StartLoadingUI(bool isLocalUser, CancellationToken token)
        {
            if (!await _currentProfileHelper.LoadUserProfileAsync(token)) return;
            if (IsDestroyed) return;

            LoadPortraitAsync(token);
            LoadVideoGridAsync(token);
            ShowLoadedContent(isLocalUser);
            RefreshRemoteUserButtons();

            _profileInfoPanel.ShowBio(_currentProfileHelper.Profile);
            _shareMyProfileButton.UpdateProfileURL(_currentProfileHelper.Profile.NickName);

            SetupMessageButton();
            _currentProfileHelper.ForceUpdateLayouts();
        }

        private void LoadVideoGridAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            _videoGridPlaceholder.SetActive(false);
            SetupTabs();
        }

        private void LoadPortraitAsync(CancellationToken token)
        {
            var profile = _currentProfileHelper.Profile; 

            _smallPortraitView.InitializeAsync(profile, Resolution._128x128, token, true);
            _portraitView.InitializeAsync(profile, Resolution._256x256, token, true);
        }

        private void ShowLoadedContent(bool isLocalUser)
        {
            _currentProfileHelper.UpdateRelatedUI(true);

            _portraitView.ShowContent();
            _smallPortraitView.ShowContent();
            
            var showNotification = isLocalUser &&  !_dataHolder.HasSetupCredentials;
            _nicknameView.Initialize(new NicknameModel 
            {
                Nickname = _currentProfileHelper.Profile.NickName,
                ShowNotificationMark = showNotification,
                OnClicked = ExecuteNicknameChangingScenario
            });
            _nicknameNotificationMark.SetActive(showNotification);
            _rankBadgeView.Initialize(_currentProfileHelper.Profile.CreatorScoreBadge);
            InitCrewButton();
        }

        private void CleanUpInternal()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;

            _currentProfileHelper.UpdateRelatedUI(false);
            _currentProfileHelper.CleanUp();

            _portraitView.CleanUp();
            _smallPortraitView.CleanUp();

            _nicknameView.CleanUp();
            _rankBadgeView.CleanUp();

            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.CleanUp();
            
            _tabsWereInitialized = false;

            foreach (var videosGrid in _userProfileVideosGrids)
            {
                videosGrid.CleanUp();
            }

            _videoGridPlaceholder.SetActive(true);
        }

        private void ResetScrollablePanel()
        {
            _scrollablePanel.ResetToInitialState();
        }

        private void RefreshLastLevelsPanelSize()
        {
            var sizeDelta = _levelsPanelRectTransforms.First().sizeDelta;
            var bottomOffset = _currentProfileHelper.IsCurrentUser
                ? _initialLastLevelsPanelSizeDelta.y - _navigationBarRectTransform.rect.size.y
                : 0f;

            sizeDelta = new Vector2(sizeDelta.x, bottomOffset);
            foreach (var levelsPanelRectTransform in _levelsPanelRectTransforms)
            {
                levelsPanelRectTransform.sizeDelta = sizeDelta;
            }
        }

        private void RefreshRemoteUserButtons()
        {
            if (!_currentProfileHelper.IsCurrentUser)
            {
                var followersArgs = new FollowUserButtonArgs(_currentProfileHelper.Profile);
                _followUserButton.Initialize(followersArgs);
                _followUserButton.SetActive(true);
                _sendMessageButton.SetActive(true);
                _editProfileButton.SetActive(false);
            }
            else
            {
                _followUserButton.CleanUp();
                _followUserButton.SetActive(false);
                _sendMessageButton.SetActive(false);
                _editProfileButton.SetActive(true);
            }
        }

        private void RefreshBackButton()
        {
            _backButton.gameObject.SetActive(OpenPageArgs.ShowBackButton);
            _backButton.interactable = OpenPageArgs.ShowBackButton;
        }

        private void SetupProfileHelper(bool isLocalUser)
        {
            _localProfileHelper.enabled = isLocalUser;
            _remoteProfileHelper.enabled = !isLocalUser;

            _currentProfileHelper = isLocalUser ? _localProfileHelper : _remoteProfileHelper;
        }

        private void RefreshNavigationBar()
        {
            _navigationBarRectTransform.gameObject.SetActive(OpenPageArgs.ShowNavigationBar);
        }

        private void VideoGridDataDownloaded()
        {
            _videoGridDownloadDataCount--;
            if (_videoGridDownloadDataCount != 0) return;

            if (!_tabsWereInitialized)
            {
                _videoGridTaskCompletionSource.SetResult(null);
            }
            else
            {
                SetupTabs();
            }
        }

        private void SetupTabs()
        {
            var postsTabName = $"<sprite=1> {GetNonDraftLevelsCount()}";
            var taggedTabName = $"<sprite=0> {GetTaggedInVideoCount()}";
            var taskTabName = $"<sprite=2> {GetTaskVideoCount()}";

            if (_tabsWereInitialized)
            {
                SetTabsNames(postsTabName, taggedTabName, taskTabName);
            }
            else
            {
                InitializeTabs(postsTabName, taggedTabName, taskTabName);
            }

            _videoGridDownloadDataCount = _userProfileVideosGrids.Length;
        }

        private void SetTabsNames(string postsTabName, string taggedTabName, string taskTabName)
        {
            _tabModels[USER_POST_TAB_INDEX].SetName(postsTabName);
            _tabModels[USER_TAGGED_POST_INDEX].SetName(taggedTabName);
            _tabModels[USER_TASK_POST_INDEX].SetName(taskTabName);
        }

        private void InitializeTabs(string postsTabName, string taggedTabName, string taskTabName)
        {
            _tabModels = new[]
            {
                new TabModel(USER_POST_TAB_INDEX, postsTabName),
                new TabModel(USER_TASK_POST_INDEX, taskTabName),
                new TabModel(USER_TAGGED_POST_INDEX, taggedTabName),
            };
            
            _tabsManagerView.gameObject.SetActive(true);
            _tabsManagerView.Init(new TabsManagerArgs(_tabModels));
            _tabsWereInitialized = true;
        }

        private int GetTaggedInVideoCount()
        {
            return _currentProfileHelper.Profile.KPI.TaggedInVideoCount;
        }

        private int GetTaskVideoCount()
        {
            return _currentProfileHelper.Profile.KPI.TaskVideoCount;
        }

        private int GetNonDraftLevelsCount()
        {
            var profileKpi = _currentProfileHelper.ProfileKPI;
            return _currentProfileHelper.IsCurrentUser
                ? profileKpi.TotalVideoCount - profileKpi.TaskVideoCount
                : profileKpi.PublishedVideoCount;
        }
        
        private async void OnNewVideoPublished(Video video)
        {
            _userProfileVideosGrids[_currentTabIndex].UpdateFirstPage();
            
            if (await _currentProfileHelper.LoadUserProfileAsync(_tokenSource.Token))
            {
                SetupTabs();
            }
        }

        private void OnMoreOptionsClicked()
        {
            _moreOptionsMenu.Display(_currentProfileHelper.Profile);
        }
        
        private bool IsTabLoaded(int tabIndex)
        {
            return _videoTabLoadState[tabIndex];
        }

        private void SetTabAsLoaded(int tabIndex)
        {
            _videoTabLoadState[tabIndex] = true;
        }

        private void LoadVideoForTab(int tabIndex, Func<BaseVideoListLoader> videoLoader)
        {
            if (IsTabLoaded(tabIndex)) return;

            var levelsPanelArgs = videoLoader();
            _userProfileVideosGrids[tabIndex].Initialize(levelsPanelArgs);
            SetTabAsLoaded(tabIndex);
        }
        
        private void InitVideoTabLoadState()
        {
            _videoTabLoadState = new Dictionary<int, bool>
            {
                { USER_POST_TAB_INDEX, false },
                { USER_TASK_POST_INDEX, false },
                { USER_TAGGED_POST_INDEX, false }
            };
        }
        
        private async void PrefetchFollowRecommendationData()
        {
            await _recommendationsProvider.PrefetchData();
        }
        
        private void InitCrewButton()
        {
            const float maxTextWidth = 550f;
            
            var hasCrew = _currentProfileHelper.Profile.CrewProfile != null;
            _crewButton.SetActive(hasCrew);
            
            if (!hasCrew) return;
            
            _crewNameText.text = _currentProfileHelper.Profile.CrewProfile.Name;

            var rect = (RectTransform)_crewButton.transform;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            if (_crewNameText.preferredWidth > maxTextWidth)
            {
                _crewButtonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                _crewNameText.overflowMode = TextOverflowModes.Ellipsis;
                rect.sizeDelta = new Vector2(maxTextWidth, rect.sizeDelta.y);
            }
            else
            {
                _crewButtonSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                _crewNameText.overflowMode = TextOverflowModes.Overflow;
            }
        }
        
        private async void OpenCrewPage()
        {
            if (UserProfileHelper.Profile.CrewProfile == null) return;
            
            if (UserProfileHelper.Profile.CrewProfile.Id == _dataHolder.UserProfile.CrewProfile?.Id)
            {
                Manager.MoveNext(new CrewPageArgs());
            }
            else
            {
                var result = await _bridge.GetCrew(UserProfileHelper.Profile.CrewProfile.Id, default);
                
                 if (result.IsSuccess)
                 {
                     Manager.MoveNext(new CrewInfoPageArgs(result.Model.ToCrewShortInfo()));
                 }
            }
        }

        private void ExecuteNicknameChangingScenario()
        {
            _scenarioManager.ExecuteNicknameEditingScenario();
        }
    }
}