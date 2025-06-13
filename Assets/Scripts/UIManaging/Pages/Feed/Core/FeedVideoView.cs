using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models;
using Bridge.Models.VideoServer;
using Bridge.Services.UserProfile;
using Bridge.VideoServer.Models;
using Common;
using Components;
using Extensions;
using Extensions.DateTime;
using Modules.Amplitude;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.Crew;
using Modules.QuestManaging;
using Modules.VideoStreaming.Feed;
using Navigation.Args;
using Navigation.Core;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Common.VideoDetails;
using UIManaging.Pages.Common.VideoDetails.VideoAttributes;
using UIManaging.Pages.Feed.GamifiedFeed;
using UIManaging.Pages.Feed.Ui.Feed;
using UIManaging.Pages.Feed.Ui.Feed.FeedTaggedUsers;
using UIManaging.Pages.FollowersPage.UI;
using UIManaging.Pages.PreRemixPage.Ui;
using UIManaging.Pages.VideosBasedOnTemplatePage;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class FeedVideoView : BaseContextDataView<FeedVideoModel>
    {
        [SerializeField] private RankUsernameView mixMasterNicknameText;
        [SerializeField] private GameObject _uploadedVideoAttribute;
        [SerializeField] private GameObject _friendAttribute;
        [SerializeField] private GameObject _crewmateAttribute;
        [SerializeField] private ScrollRect _attributesRow;
        [SerializeField] private Image _attributesRowMask;
        [SerializeField] private GameObject _bottomPanel;
        [SerializeField] private GameObject votingResultContainer;
        [SerializeField] private GameObject votingBattleNameContainer;
        [SerializeField] private GameObject likesContainer;
        [SerializeField] private GameObject _viewsDate;
        [SerializeField] private GameObject postedDateAndViewsAmountContainer;
        [SerializeField] private TextMeshProUGUI votingBattleNameText;
        [SerializeField] private TextMeshProUGUI votingResultText;
        [SerializeField] private TextMeshProUGUI postedDateText;
        [SerializeField] private TextMeshProUGUI viewsAmountText;
        [SerializeField] private TextMeshProUGUI originalCreatorText;
        [SerializeField] private FeedVideoSongPanel _songPanel;
        [SerializeField] private TextMeshProUGUI likesAmountText;
        [SerializeField] private FeedVideoDescription _videoDescription;
        [SerializeField] private Button portraitProfileButton;
        [SerializeField] private Button nickNameProfileButton;
        [SerializeField] private Button originalCreatorButton;
        [SerializeField] private DisplayUGUI displayUgui;
        [SerializeField] private FeedLikeToggle likeToggle;
        [SerializeField] private GamifiedFeedLikesToggle _gamifiedFeedLikesToggle;
        [SerializeField] private FeedVideoFollowToggle feedVideoFollowToggle;
        [SerializeField] private CommentsButton commentsButton;
        [SerializeField] private Button moreButton;
        [SerializeField] private ClickListener clicksListener;
        [SerializeField] private PlayButtonAnimator _playButtonAnimator;
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private FeedTaggedUsersButton _taggedUsersButton;
        [SerializeField] private TextMeshProUGUI _remixesAmountText;
        [SerializeField] private RemixButton _remixButton;
        [SerializeField] private TemplateUsedForVideoButton _templateUsedForVideoButton;
        [SerializeField] private Button _templateDisabledButton;
        [SerializeField] private GameObject _videoIsNotAvailableOverlay;
        [SerializeField] private TextMeshProLinkHandler _textMeshProLinkHandler;
        [SerializeField] private List<AccessObjects> _accessObjects;
        [SerializeField] private VideoPartOfTaskButton _videoPartOfTaskButton;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private UseTemplateButton _startChallengeButton;
        [SerializeField] private UseMessageTemplateButton _useMessageTemplateButton;
        [SerializeField] private TemplateUsedForVideoButton _useTemplateButton;
        
        [SerializeField] private FollowUserButton _followTriggerButton;

        [SerializeField] private RectTransform _videoInfoRect;
        [SerializeField] private GameObject _rightSizeButtonHolder;
        [SerializeField] private StarScoreView votingResultScore;
        [SerializeField] private List<LinkTypeToButton> _linkTypeToButtonList;
        [SerializeField] private VideoRatingBadge _ratingRewardBadge;
        
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private IDataFetcher _userData;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IQuestManager _questManager;
        [Inject] private FollowersManager _followersManager;
        [Inject] private CrewService _crewService;
        [Inject] private FeedLocalization _localization;
        [Inject] private SnackBarHelper _snackbarHelper;

        private double _timeToEnd;
        private bool _isVideoReadyToPlay;
        private CancellationTokenSource _eventTemplateCancellationToken;
        private Action _onVideoReady;
        private bool _showFollowTriggerButton;
      
        private long[] _templateIdsToIgnore;
        private RectTransform _profileGroupParent;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsReady => IsInitialized && ContextData.IsVideoAvailable && _isVideoReadyToPlay;
        public bool IsVideoLoading { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsTarget { get; set; }

        public long VideoId => ContextData.Video.Id;
        public Video Video => ContextData.Video;
        private bool LikeVideoOnDoubleClick => ContextData.ShowActionsBar;
        public bool IsOwner => ContextData.Video.GroupId == _localUser.GroupId;

        public bool ShowFollowTriggerButton => _showFollowTriggerButton
                                            && Video.GroupId != _localUser.GroupId
                                            && !Video.IsFollower
                                            && (_videoManager.IsFollowingRecommended(Video) || Video.IsFollowed);

        private IEnumerable<long> TemplatesToIgnore
        {
            get
            {
                return _templateIdsToIgnore ??
                       (_templateIdsToIgnore = new[] { _userData.DefaultTemplateId });
            }
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<long, string> OnOpenProfileButtonClickedEvent;
        public event Action<FeedVideoModel> OnMoreButtonClickedEvent;
        public event Action<FeedVideoView> OnStartedLoadingVideoEvent;
        public event Action<FeedVideoView> OnVideoReadyToPlayEvent;
        public event Action<FeedVideoView> OnVideoNotAvailableEvent;
        public event Action OnVideoFinishedPlayingEvent;
        public event Action<int> PointerDown;
        public event Action<int> PointerUp;
        public event Action OnTemplateDisabledButton;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _profileGroupParent = portraitProfileButton.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!displayUgui.CurrentMediaPlayer.Control.IsPlaying()) return;

            if (_timeToEnd <= 0f)
            {
                _timeToEnd = displayUgui.CurrentMediaPlayer.Info.GetDuration();
                OnVideoFinishedPlayingEvent?.Invoke();
            }

            _timeToEnd -= Time.deltaTime;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            displayUgui.CurrentMediaPlayer.CloseMedia();
            displayUgui.CurrentMediaPlayer.StopAllCoroutines();
            _isVideoReadyToPlay = false;
            IsVideoLoading = false;
            ClearTexture();
            CancelEventTemplateRequest();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void AddCommentsButtonOnClickListener(UnityAction onClick)
        {
            if (!ContextData.Video.AllowComment) return;

            commentsButton.Initialize(new CommentsButtonModel(onClick, IsOwner));
        }

        public void TaggedUsersButtonSubscribe(UnityAction<List<TaggedGroup>> onClick)
        {
            _taggedUsersButton.OnClick -= onClick;
            _taggedUsersButton.OnClick += onClick;
        }

        public void DownloadVideo(bool autoPlay)
        {
            if (!ContextData.IsVideoAvailable) return;
            if (!displayUgui.CurrentMediaPlayer.gameObject.activeInHierarchy) return;

            IsVideoLoading = true;
            _isVideoReadyToPlay = false;

            OnStartedLoadingVideoEvent?.Invoke(this);
            
            #if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR_WIN
            if (!autoPlay && _onVideoReady == null)
            {
                _onVideoReady = RunMediaPlayerPrewarming;
            }
            #endif
            
            var mediaPlayer = displayUgui.CurrentMediaPlayer;
            mediaPlayer.GetCurrentPlatformOptions().httpHeaders.Clear();
            mediaPlayer.GetCurrentPlatformOptions().httpHeaders.Add(ContextData.HttpHeader.name, ContextData.HttpHeader.value);
            mediaPlayer.OpenMedia(ContextData.FileLocation, ContextData.Url, autoPlay);

            #if UNITY_EDITOR
                mediaPlayer.AudioMuted = EditorUtility.audioMasterMute;
            #endif
        }

        public void PlayVideo()
        {
            if (!displayUgui.CurrentMediaPlayer.gameObject.activeInHierarchy) return;
            if (!IsReady)
            {
                ClearTexture();
            }
            
            OnStartedLoadingVideoEvent?.Invoke(this);
            if (IsReady)
            {
                PlayVideoInternal();
            }
            else
            {
                _onVideoReady = PlayVideoInternal;
                if (!IsVideoLoading)
                {
                    DownloadVideo(false);
                }
            }
        }

        public void RewindAndStop()
        {
            if (!displayUgui.CurrentMediaPlayer.gameObject.activeInHierarchy) return;
            
            if (IsReady)
            {
                RewindAndStopInternal();
            }
            else
            {
                _onVideoReady = RewindAndStopInternal;
            }
        }

        public void CloseMedia()
        {
            if (!displayUgui.CurrentMediaPlayer.gameObject.activeInHierarchy) return;

            _isVideoReadyToPlay = false;
            IsVideoLoading = false;
            displayUgui.CurrentMediaPlayer.CloseMedia();
            ClearTexture();
        }

        public void RefreshKPIValues(VideoKPI videoKPI)
        {
            if (ContextData.IsPostedDateAndViewsAmountActive)
            {
                viewsAmountText.text = $"{videoKPI.Views.ToShortenedString()} views";
            }
            commentsButton.RefreshCount(videoKPI.Comments);
            likesAmountText.text = videoKPI.Likes.ToString();
            _remixesAmountText.text = videoKPI.Remixes.ToString();
        }

        public void RefreshVideoPrivacy(VideoAccess access)
        {
            ContextData.Video.Access = access;
            UpdatePrivacyLabels(access);
            PrepareTemplateButton();
        }

        public void InitializeVideoAvailabilityState()
        {
            _videoIsNotAvailableOverlay.SetActive(!ContextData.IsVideoAvailable);
            if (!ContextData.IsVideoAvailable)
            {
                OnVideoNotAvailableEvent?.Invoke(this);
            }
        }

        private void SetupActionsBar(bool showActionsBar)
        {
            _rightSizeButtonHolder.SetActive(showActionsBar);
            portraitProfileButton.interactable = showActionsBar;
            nickNameProfileButton.interactable = showActionsBar;
            moreButton.interactable = showActionsBar;
        }
        
        public void StopMediaPlayer()
        {
            displayUgui.CurrentMediaPlayer.Stop();
            SetIsPlaying(false);
        }

        public void StartMediaPlayer()
        {
            displayUgui.CurrentMediaPlayer.Play();
        }

        public void SetAspectRatioFitterMode(AspectRatioFitter.AspectMode aspectMode)
        {
            var rect = (RectTransform)displayUgui.transform;

            const float aspect9to16 = 0.56f;
            const float aspect3to4 = 0.75f;
            var aspect = (float)Screen.width / Screen.height;
            var is9to16 = Mathf.Abs(aspect - aspect9to16) < 0.01f;
            var is3to4 = Mathf.Abs(aspect - aspect3to4) < 0.03f;

            if (is9to16)
            {
                aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            }
            // Uncomment to force video player to FitInParent for 3:4 screens
            // else if (is3to4)
            // {
            //     aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            // }
            
            _aspectRatioFitter.aspectMode = aspectMode;
            
            if (aspectMode == AspectRatioFitter.AspectMode.FitInParent && !is3to4)
            {
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0, 150f);
            }
            else
            {
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        public void ShowRewardBadge()
        {
            var result = Video.RatingResult;
            if (result is { IsCompleted: true })
            {
                _ratingRewardBadge.Show(result.Rating);
            }
            else
            {
                _ratingRewardBadge.Hide();
            }
        }

        public void HideRewardBadge()
        {
            _ratingRewardBadge.Hide();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _bottomPanel.gameObject.SetActive(true);
            _isVideoReadyToPlay = false;
            IsVideoLoading = false;
            _showFollowTriggerButton = true;
            _taggedUsersButton.gameObject.SetActive(ContextData.Video.TaggedGroups?.Length > 0);
            _taggedUsersButton.Setup(ContextData.Video.TaggedGroups);
            PrepareRemixButton();
            PrepareCommentsButton();
            ClearTexture();
            PrepareUserNicknameText();
            PrepareVotingBattle();
            displayUgui.ScaleMode = ContextData.ScaleMode ?? Constants.Feed.DEFAULT_SCALE_MODE;
            
            var isPostedDateAndViewsAmountActive = ContextData.IsPostedDateAndViewsAmountActive;
            var isSongNameActive = !string.IsNullOrEmpty(ContextData.Video.SongName);
            
            if (gameObject.activeInHierarchy)
            {
                postedDateAndViewsAmountContainer.SetActive(isPostedDateAndViewsAmountActive);
                if (isPostedDateAndViewsAmountActive)
                {
                    StartCoroutine(RefreshPostedDateText());
                }
            }

            _songPanel.SetActive(isSongNameActive);
            _viewsDate.SetActive(isPostedDateAndViewsAmountActive);

            RefreshKPIValues(ContextData.Video.KPI);
            _videoDescription.Init(ContextData.Video);
            _songPanel.Initialize(ContextData.Video);
            PrepareFollowToggle();
            SubscribeEvents();
            RefreshPortraitView();
            PrepareUploadedVideoAttribute();
            PrepareFollowingStatusAttributes();
            PrepareOriginalCreatorAttribute();
            UpdatePrivacyLabels(ContextData.VideoAccess);
            UpdateLinkButtons();
            PrepareTemplateButtons();
            
            SetupActionsBar(ContextData.ShowActionsBar);
            
            _videoInfoRect.SetActive(ContextData.ShowVideoDescription);
            if (ContextData.ShowVideoDescription)
            {
                _videoDescription.SimplifyForOnboarding(!ContextData.ShowActionsBar);
            }

            PrepareAttributesRow();

            if (_gamifiedFeedLikesToggle)
            {
                _gamifiedFeedLikesToggle.Initialize(new GamifiedFeedLikesModel(ContextData.Video, IsOwner));
                _gamifiedFeedLikesToggle.ValueChanged += OnGamifiedLikeToggleValueChanged;
            }
            else
            {
                PrepareLikeToggle();
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_profileGroupParent);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ClearTexture();
            UnSubscribeEvents();

            _bottomPanel.gameObject.SetActive(false);
            displayUgui.CurrentMediaPlayer.CloseMedia();
            displayUgui.CurrentMediaPlayer.Events.RemoveListener(OnEventInvoked);
            _isVideoReadyToPlay = false;
            IsVideoLoading = false;

            _videoDescription.BeforeCleanup();
            
            if (_songPanel.IsInitialized)
            {
                _songPanel.CleanUp();
            }

            if (_gamifiedFeedLikesToggle && _gamifiedFeedLikesToggle.IsInitialized)
            {
                _gamifiedFeedLikesToggle.ValueChanged -= OnGamifiedLikeToggleValueChanged;
                _gamifiedFeedLikesToggle.CleanUp();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlayVideoInternal()
        {
            displayUgui.CurrentMediaPlayer.Play();
            _songPanel.StartScrolling();
        }

        private void RewindAndStopInternal()
        {
            displayUgui.CurrentMediaPlayer.AutoStart = false;
            displayUgui.CurrentMediaPlayer.Rewind(true);
            displayUgui.CurrentMediaPlayer.Stop();
            _songPanel.StopScrolling();
        }

        private void RefreshLikesText()
        {
            likesAmountText.text = ContextData.Video.KPI.Likes.ToString();
        }

        private void RefreshPortraitView()
        {
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.Video.GroupId,
                UserMainCharacterId = ContextData.Video.Owner.MainCharacterId.Value,
                MainCharacterThumbnail = ContextData.Video.Owner.MainCharacterFiles
            };

            _userPortraitView.Initialize(userPortraitModel);
        }

        private void UpdateLinkButtons()
        {
            foreach (var linkTypeToButton in _linkTypeToButtonList)
            {
                var linkTypeStr = linkTypeToButton.LinkType.ToString();
                var isEnabled = ContextData.Video.Links?.ContainsKey(linkTypeStr.ToLowerInvariant()) ?? false;
                _showFollowTriggerButton = _showFollowTriggerButton && !isEnabled;
                linkTypeToButton.Button.SetActive(isEnabled);
            }
        }

        private void SetIsPlaying(bool value)
        {
            IsPlaying = value;
        }

        private void OnEventInvoked(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            if (eventType == MediaPlayerEvent.EventType.Started)
            {
                SetIsPlaying(true);
            }
        }

        private void OnClickListenerClicked()
        {
            _playButtonAnimator.PlayAnimation(!IsPlaying);
            
            if (IsPlaying)
            {
                StopMediaPlayer();
            }
            else
            {
                displayUgui.CurrentMediaPlayer.Play();
            }
        }

        private IEnumerator RefreshPostedDateText()
        {
            postedDateText.gameObject.SetActive(false);
            postedDateText.text = ContextData.Video.CreatedTime.GetVideoPostedFormattedTime();
            yield return null;
            postedDateText.SetActive(true);
        }

        private void OnEventFired(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            if (eventType != MediaPlayerEvent.EventType.FirstFrameReady) return;

            displayUgui.color = Color.white;
            _isVideoReadyToPlay = true;
            IsVideoLoading = false;
            _onVideoReady?.Invoke();
            _onVideoReady = null;
            OnVideoReadyToPlayEvent?.Invoke(this);
        }

        #if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR_WIN
        private void RunMediaPlayerPrewarming()
        {
            StartCoroutine(PrewarmMediaPlayer());
        }
        #endif

        private void OnMoreButtonClicked()
        {
            OnMoreButtonClickedEvent?.Invoke(ContextData);
        }

        private void OnLikeToggleValueChanged(bool value)
        {
            ContextData.Video.KPI.Likes += value ? 1 : -1;
            ContextData.Video.LikedByCurrentUser = value;
            RefreshLikesText();
            likeToggle.RefreshUI(value);
            _videoManager.LikeVideo(ContextData.Video.Id, Video.GroupId, value, _questManager.ShowQuestLikesSnackBar);
        }

        private void OnGamifiedLikeToggleValueChanged(bool isOn)
        {
            _videoManager.LikeVideo(ContextData.Video.Id, Video.GroupId, isOn, _questManager.ShowQuestLikesSnackBar);
        }

        private void SwitchVideoLike()
        {
            if (_gamifiedFeedLikesToggle)
            {
                _gamifiedFeedLikesToggle.Toggle.isOn = !ContextData.Video.LikedByCurrentUser;
            }
            else
            {
                likeToggle.Toggle.isOn = !ContextData.Video.LikedByCurrentUser;
            }
        }

        private void OnOpenProfileButtonClicked()
        {
            OnOpenProfileButtonClickedEvent?.Invoke(ContextData.Video.GroupId, ContextData.Video.Owner.Nickname);
        }
        
        private void OnOriginalCreatorButtonClicked()
        {
            _pageManager.MoveNext(new PreRemixPageArgs(ContextData.Video));
        }
        
        private void OnTemplateDisabledButtonClicked()
        {
            OnTemplateDisabledButton?.Invoke();
        }

        private void OnLinkClicked(ExternalLinkType linkType)
        {
            var linkTypeStr = linkType.ToString().ToLowerInvariant();

            if (!ContextData.Video.Links.ContainsKey(linkTypeStr))
            {
                Debug.LogError($"Link not found for type {linkTypeStr}");
                return;
            }
            
            Application.OpenURL(ContextData.Video.Links[linkTypeStr]);
        }

        private void PrepareVotingBattle()
        {
            var isVotingBattle = ContextData.Video.IsVotingTask;
            var isVotingBattleComplete = isVotingBattle && ContextData.Video.BattleResult != null;
            
            likesContainer.SetActive(!isVotingBattle);
            votingResultContainer.SetActive(isVotingBattleComplete);
            votingBattleNameContainer.SetActive(isVotingBattle);

            if (isVotingBattle)
            {
                votingBattleNameText.text = ContextData.Video.TaskName;
                LayoutRebuilder.ForceRebuildLayoutImmediate(votingBattleNameContainer.GetComponent<RectTransform>());
            }

            if (isVotingBattleComplete)
            {
                votingResultScore.SetRating(ContextData.Video.BattleResult.Score);
                votingResultText.text = GetOrdinal(ContextData.Video.BattleResult.Place);
            }
        }

        private string GetOrdinal(int place)
        {
            switch (place)
            {
                case 1: return _localization.ChallengePlace1;
                case 2: return _localization.ChallengePlace2;
                case 3: return _localization.ChallengePlace3;
                case 4: return _localization.ChallengePlace4;
                case 5: return _localization.ChallengePlace5;
                case 6: return _localization.ChallengePlace6;
                case 7: return _localization.ChallengePlace7;
                case 8: return _localization.ChallengePlace8;
                case 9: return _localization.ChallengePlace9;
                case 10: return _localization.ChallengePlace10;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        private void PrepareRemixButton()
        {
            if (!ContextData.Video.IsRemixable || !ContextData.Video.AllowRemix)
            {
                _remixButton.gameObject.SetActive(false);
                return;
            }
            
            _remixButton.gameObject.SetActive(!ContextData.CanUseForRemix);
            
            if (!ContextData.CanUseForRemix)
            {
                _remixButton.Initialize(ContextData);
            }
        }

        private void PrepareUserNicknameText()
        {
            var nickName = ContextData.Video.Owner.Nickname;

            var creatorScore = ContextData.Video.GroupCreatorScoreBadge;
            
            #if UNITY_EDITOR && FEED_SHOW_VIDEO_IDS
                mixMasterNicknameText.Initialize($"[{ContextData.Video.Id}] {nickName}", creatorScore);
            #else
                mixMasterNicknameText.Initialize(nickName, creatorScore);
            #endif
        }

        private void ParseHyperlink(string linkId, string linkText)
        {
            var split = linkId.Split(':');

            if (split.Length == 0) return;

            switch (split[0])
            {
                case "mention":
                    ShowProfile(split[1]);
                    break;
                case "hashtag":
                    ShowHashtag(split[1]);
                    break;
            }
        }

        private void ShowHashtag(string linkId)
        {
            if (!long.TryParse(linkId, out var hashtagId)) return;
            
            var hashtagInfo = ContextData.Video.Hashtags.FirstOrDefault(item => item.Id == hashtagId);
            
            if(hashtagInfo == null) return;

            var args = new VideosBasedOnHashtagPageArgs()
            {
                TemplateName = hashtagInfo.Name,
                TemplateInfo = null,
                HashtagInfo = hashtagInfo,
                UsageCount = (int)hashtagInfo.UsageCount
            };

            _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, args);
        }

        private void ShowProfile(string linkId)
        {
            if (!long.TryParse(linkId, out var groupId)) return;
            
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(groupId, null));
        }
        
        private void PrepareLikeToggle()
        {
            likeToggle.EnableAnimation(false);
            likeToggle.Toggle.isOn = ContextData.Video.LikedByCurrentUser;
            likeToggle.EnableAnimation(true);
            likeToggle.RefreshUI(likeToggle.Toggle.isOn);
            likeToggle.Toggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
        }
        private void PrepareFollowToggle()
        {
            var isLocalUser = _bridge.Profile.GroupId == ContextData.Video.GroupId;
            feedVideoFollowToggle.gameObject.SetActive(!isLocalUser);
            if (!isLocalUser)
            {
                feedVideoFollowToggle.Init(ContextData.Video.GroupId);
            }
        }

        private async void PrepareTemplateButtons()
        {
            PrepareTemplateButton();
            PrepareVidePartOfTaskButton();
            await PrepareStartChallengeButton();
            PrepareUseMessageTemplateButton();
            PrepareFollowTriggerButton();
        }

        private void PrepareUseMessageTemplateButton()
        {
            var isMessage = ContextData.Video.LevelTypeId == ServerConstants.LevelType.VIDEO_MESSAGE;
            _useMessageTemplateButton.SetActive(isMessage);
            _showFollowTriggerButton = _showFollowTriggerButton && !isMessage;
            if (!isMessage) return;
            _useMessageTemplateButton.Initialize(ContextData.Video);
        }

        private void PrepareOriginalCreatorAttribute()
        {
            var isRemixed = ContextData.Video?.RemixedFromLevelId.HasValue ?? default;
            
            originalCreatorButton.gameObject.SetActive(isRemixed);
            
            if (!isRemixed) return;

            originalCreatorText.text = $"<b>@{ContextData.Video.OriginalCreator.Nickname}</b>";
        }
        
        private void PrepareFollowingStatusAttributes()
        {
            var isFriend = ContextData.Video.IsFriend;
            _friendAttribute.SetActive(isFriend);
            _crewmateAttribute.SetActive(!isFriend && ContextData.Video.GroupId != _localUser.GroupId && _crewService.Model != null && _crewService.Model.Members.Any(member => member.Group.Id == ContextData.Video.GroupId));
        }

        private void PrepareUploadedVideoAttribute()
        {
            _uploadedVideoAttribute.SetActive(!ContextData.Video.LevelId.HasValue);
        }

        private async void PrepareAttributesRow()
        {
            await Task.Yield();
            await Task.Yield();
            
            foreach (Transform attribute in _attributesRow.content.transform)
            {
                if(!attribute.gameObject.activeSelf) continue;
                _attributesRow.SetActive(true);
                _attributesRow.horizontalNormalizedPosition = 0;
                _attributesRow.horizontal = _attributesRow.content.rect.width > _attributesRow.viewport.rect.width;
                _attributesRowMask.enabled = _attributesRow.horizontal;
                return;
            }
            
            _attributesRow.SetActive(false);
        }
        
        private void UpdatePrivacyLabels(VideoAccess access)
        {
            var isLocalUser = _bridge.Profile.GroupId == ContextData.Video.GroupId;
            
            foreach (var accessObjects in _accessObjects)
            {
                foreach (var target in accessObjects.targets)
                {
                    target.SetActive(accessObjects.access == access && isLocalUser);
                }
            }
        }

        private void CancelEventTemplateRequest()
        {
            if (_eventTemplateCancellationToken == null) return;
            
            _eventTemplateCancellationToken.Cancel();
            _eventTemplateCancellationToken = null;
        }

        private void PrepareTemplateButton()
        {
            _templateDisabledButton.gameObject.SetActive(false);

            if (_useTemplateButton)
            {
                _useTemplateButton.SetActive(false);
            }

            var showButton = ShouldShowVideoBasedOnTemplateButton()
                          && Video.MainTemplate != null
                          && Video.Access == VideoAccess.Public;

            if (!showButton)
            {
                _templateUsedForVideoButton.Show(false);

                if (Video.GeneratedTemplateId.HasValue
                 && Video.GroupId == _bridge.Profile.GroupId
                 && Video.Access != VideoAccess.Public)
                {
                    _templateDisabledButton.gameObject.SetActive(true);
                }

                return;
            }

            _templateUsedForVideoButton.Show(true);
            _templateUsedForVideoButton.Initialize(Video.MainTemplate.Id, Video.MainTemplate.Title, ContextData.OnJoinTemplateClick);

            if (!_useTemplateButton || ShowFollowTriggerButton) return;

            if (_useTemplateButton is PlayButton playAndFollowButton)
            {
                PreparePlayButton(playAndFollowButton);
            }
            else
            {
                PrepareVideoBasedOnTemplateButton(_useTemplateButton);
            }
        }

        private void PrepareVideoBasedOnTemplateButton(TemplateUsedForVideoButton button)
        {
            button.SetActive(true);
            button.Initialize(Video.MainTemplate.Id, Video.MainTemplate.Title, ContextData.OnJoinTemplateClick);
        }

        private void PreparePlayButton(PlayButton button)
        {
            button.SetActive(true);
            button.Initialize(Video.MainTemplate.Id, Video.MainTemplate.Title, ContextData.OnJoinTemplateClick);
        }

        private void PrepareVidePartOfTaskButton()
        {
            if (ContextData.Video.IsVotingTask || ContextData.Video.TaskId == 0 || ContextData.OpenedWithTask == ContextData.Video.TaskId)
            {
                _videoPartOfTaskButton.Show(false);
                return;
            }
            
            _videoPartOfTaskButton.Show(true);
            _videoPartOfTaskButton.Initialize(ContextData.Video.TaskId, ContextData.Video.TaskName);
        }
        
        private async Task PrepareStartChallengeButton()
        {
            _startChallengeButton.SetActive(false);
            
            var taskId = ContextData.Video.IsVotingTask ? ContextData.Video.TaskId : ContextData.OpenedWithTask;
            if (taskId == 0 || !ContextData.ShowChallengeButtonInDescription)
            {
                return;
            }
            
            var result = await _bridge.GetTaskFullInfoAsync(taskId);
            if (result.IsError)
            {
                return;
            }
            
            if (!result.IsSuccess) return;
            
            _showFollowTriggerButton = false;
            _startChallengeButton.SetActive(true);
            var isTaskFeed = ContextData.OpenedWithTask != 0;
            _startChallengeButton.Setup(result.Model, ContextData.Video.GroupId == _bridge.Profile.GroupId, isTaskFeed, ContextData.JoinTaskButtonClick);
        }

        private async void PrepareFollowTriggerButton()
        {
            _followTriggerButton.SetActive(ShowFollowTriggerButton);
            
            if (!ShowFollowTriggerButton) return;

            var isFollower = await _followersManager.IsFollower(Video.GroupId);
            var isFollowed = await _followersManager.IsFollowed(Video.GroupId);
                
            _followTriggerButton.Initialize(new FollowUserButtonArgs(new Profile()
            {
                MainGroupId = ContextData.Video.GroupId,
                NickName = ContextData.Video.Owner.Nickname,
                UserFollowsYou = isFollower,
                YouFollowUser = isFollowed
            }));
        }
        
        private void SubscribeEvents()
        {
            portraitProfileButton.onClick.AddListener(OnOpenProfileButtonClicked);
            nickNameProfileButton.onClick.AddListener(OnOpenProfileButtonClicked);
            originalCreatorButton.onClick.AddListener(OnOriginalCreatorButtonClicked);
            displayUgui.CurrentMediaPlayer.Events.AddListener(OnEventInvoked);
            displayUgui.CurrentMediaPlayer.Events.AddListener(OnEventFired);
            moreButton.onClick.AddListener(OnMoreButtonClicked);
            _templateDisabledButton.onClick.AddListener(OnTemplateDisabledButtonClicked);

            foreach (var linkTypeToButton in _linkTypeToButtonList)
            {
                linkTypeToButton.Button.onClick.AddListener(() => OnLinkClicked(linkTypeToButton.LinkType));
            }

            clicksListener.OnClickedEvent += OnClickListenerClicked;
            if (LikeVideoOnDoubleClick)
            {     
                clicksListener.OnDoubleClickedEvent += SwitchVideoLike;
            }
            clicksListener.PointerDown += OnPointerDown;
            clicksListener.PointerUp += OnPointerUp;
            _remixButton.ButtonClicked += StopMediaPlayer;
            
            _textMeshProLinkHandler.HyperlinkHandled += ParseHyperlink;

            _followersManager.Followed += UpdateFollowingState;
            _followersManager.UnFollowed += UpdateFollowingState;
            
            _followersManager.FollowingStarted += OnFollowingStarted;
            _followersManager.UnfollowingStarted += OnUnfollowingStarted;

            _videoManager.LikeCountChanged += OnLikeCountChanged;
        }

        private void OnUnfollowingStarted(long groupId)
        {
            if (IsTarget || Video.GroupId != groupId) return;
            
            _followTriggerButton.SetActive(true);
        }

        private void OnFollowingStarted(long groupId)
        {
            if (IsTarget || Video.GroupId != groupId) return;
            
            PrepareFollowTriggerButton();
            PrepareFollowToggle();
        }

        private void OnLikeCountChanged(long videoId, long groupId)
        {
            if (Video.Id == videoId || Video.GroupId != groupId) return;
            
            PrepareFollowTriggerButton();
        }

        private void UpdateFollowingState(Profile profile)
        {
            if (profile.MainGroupId != ContextData.Video.GroupId) return;
            ContextData.Video.IsFollower = profile.YouFollowUser;
            PrepareFollowToggle();
            
            if (!IsTarget) PrepareFollowTriggerButton();
            
            if (!profile.YouFollowUser) return;
            
            _snackbarHelper.ShowInformationSnackBar(
                string.Format( profile.UserFollowsYou
                                   ? _localization.FriendsWithUserSnackbarMessageFormat 
                                   : _localization.FollowingUserSnackbarMessageFormat, 
                               profile.NickName));
        }

        private void UnSubscribeEvents()
        {
            portraitProfileButton.onClick.RemoveListener(OnOpenProfileButtonClicked);
            nickNameProfileButton.onClick.RemoveListener(OnOpenProfileButtonClicked);
            originalCreatorButton.onClick.RemoveListener(OnOriginalCreatorButtonClicked);
            moreButton.onClick.RemoveListener(OnMoreButtonClicked);
            displayUgui.CurrentMediaPlayer.Events.RemoveListener(OnEventFired);
            likeToggle.Toggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);
            _templateDisabledButton.onClick.RemoveListener(OnTemplateDisabledButtonClicked);

            foreach (var linkTypeToButton in _linkTypeToButtonList)
            {
                linkTypeToButton.Button.onClick.RemoveAllListeners();
            }
            
            clicksListener.OnClickedEvent -= OnClickListenerClicked;
            clicksListener.OnDoubleClickedEvent -= SwitchVideoLike;
            clicksListener.PointerDown -= OnPointerDown;
            clicksListener.PointerUp -= OnPointerUp;
            _remixButton.ButtonClicked -= StopMediaPlayer;
            _textMeshProLinkHandler.HyperlinkHandled -= ParseHyperlink;
            
            _followersManager.Followed -= UpdateFollowingState;
            _followersManager.UnFollowed -= UpdateFollowingState;
            
            _followersManager.FollowingStarted -= OnFollowingStarted;
            _followersManager.UnfollowingStarted -= OnUnfollowingStarted;
            
            _videoManager.LikeCountChanged -= OnLikeCountChanged;
        }

        #if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR_WIN
        private IEnumerator PrewarmMediaPlayer()
        {
            if (displayUgui.Player.AutoStart) yield break;

            displayUgui.Player.Play();
            yield return null;
            displayUgui.Player.Pause();
        }
        #endif

        private bool ShouldShowVideoBasedOnTemplateButton()
        {
            var template = ContextData.Video.MainTemplate;
            var show = template != null && !TemplatesToIgnore.Contains(template.Id) && ContextData.ShowBasedOnTemplateButton;
            return show;
        }
        
        private void ClearTexture()
        {
            displayUgui.color = Color.black;
        }

        private void OnPointerDown(int pointerId)
        {
            PointerDown?.Invoke(pointerId);
        }

        private void OnPointerUp(int pointerId)
        {
            PointerUp?.Invoke(pointerId);
        }

        private void PrepareCommentsButton()
        {
            commentsButton.SetActive(ContextData.Video.AllowComment);
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        [Serializable]
        private struct AccessObjects
        {
            public VideoAccess access;
            public List<GameObject> targets;
        }

        [Serializable]
        private struct LinkTypeToButton
        {
            public ExternalLinkType LinkType;
            public Button Button;
        }
    }
}