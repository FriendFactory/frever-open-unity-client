using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIManaging.Common;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using Zenject;
using Bridge;
using Bridge.Models.Common.Files;
using Bridge.Models.VideoServer;
using Modules.Amplitude;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.PopupSystem;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.PreRemixPage.Ui
{
    internal sealed class PreRemixPage : GenericPage<PreRemixPageArgs>
    {
        public override PageId Id => PageId.PreRemixPage;

        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private Button _originalVideoButton;
        [SerializeField] private TextMeshProUGUI _creatorText;
        [SerializeField] private TextMeshProUGUI _peopleUsedText;
        [SerializeField] private Button _remixButton;
        [SerializeField] private VideoList _videosGrid;
        [SerializeField] private VideoThumbnail _originalVideoThumbnail;
        [SerializeField] private GameObject _unavailableOriginalVideoThumbnailOverlay;
        [SerializeField] private Button _originalCreatorProfileButton;
        [SerializeField] private RawImage _originalCreatorProfileImage;
        
        [Inject] private IBridge _bridge;
        [Inject] private VideoManager _videoManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private RemixLevelSetup _remixSetup;
        [Inject] private IBlockedAccountsManager _blockedAccountsManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private FeedLocalization _localization;

        private PageManager _pageManager;
        private PreRemixPageArgs _pageArgs;
        private RemixVideoListLoader _videoListLoader;
        private long _originalCreatorGroupId;
        private string _originalCreatorNickName;

        private Video OpenedFromVideo => _pageArgs.Video;

        protected override void OnInit(PageManager pageManager)
        {
            _pageManager = pageManager;
        }
        
        protected override void OnDisplayStart(PreRemixPageArgs args)
        {
            _pageArgs = args;
            base.OnDisplayStart(args);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.REMIX_INSPO_PAGE);
            SetPeopleUsedAmountText(0);
            _videoListLoader = new RemixVideoListLoader(args.Video.Id, _pageArgs.Video.RemixedFromVideoId, _videoManager, _pageManager, _bridge);
            _videoListLoader.NewPageAppended += OnRemixVideoDownload;
            _videosGrid.Initialize(_videoListLoader);
            _pageHeaderView.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, OnBackButtonClicked)));
            
            _originalCreatorGroupId = _pageArgs.Video.OriginalCreator?.Id ?? _pageArgs.Video.GroupId;
            var hasOriginalCreator = _originalCreatorGroupId != _pageArgs.Video.GroupId;
            _originalCreatorNickName = hasOriginalCreator
                ? _pageArgs.Video.OriginalCreator.Nickname
                : _pageArgs.Video.Owner.Nickname;
            
            _creatorText.text = hasOriginalCreator
                ? $"{args.Video.OriginalCreator.Nickname}"
                : $"{args.Video.Owner.Nickname}";
            
            SetupOriginalVideo(args, hasOriginalCreator, _originalCreatorGroupId);
            SetupProfileButton();
            SetupProfileImage();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _originalVideoButton.onClick.RemoveAllListeners();
            _originalCreatorProfileButton.onClick.RemoveAllListeners();
        }

        private async void SetupOriginalVideo(PreRemixPageArgs args, bool hasOriginalCreator, long originalCreatorId)
        {
            if (hasOriginalCreator && _blockedAccountsManager.IsUserBlocked(originalCreatorId))
            {
                _unavailableOriginalVideoThumbnailOverlay.SetActive(true);
                return;
            }
            
            _unavailableOriginalVideoThumbnailOverlay.SetActive(false);
            var video = await GetOriginalVideoModel(args.Video);
            _unavailableOriginalVideoThumbnailOverlay.SetActive(video==null);
            if (video == null) return;
            
            SetupCreatorThumbnailButtons(hasOriginalCreator);

            _originalVideoThumbnail.Initialize(new VideoThumbnailModel(video.ThumbnailUrl));
            _originalVideoThumbnail.SetupMediaPlayer();
        }

        private async Task<Video> GetOriginalVideoModel(Video targetVideo)
        {
            if (!targetVideo.RemixedFromVideoId.HasValue)
            {
                return targetVideo;
            }

            var resp = await _bridge.GetVideoAsync(targetVideo.RemixedFromVideoId.Value);
            return resp.ResultObject;
        }

        private void SetupProfileButton()
        {
            _originalCreatorProfileButton.onClick.AddListener(OnProfileClicked);
        }

        private async void SetupProfileImage()
        {
            if (_originalCreatorProfileImage.texture != null)
            {
                Destroy(_originalCreatorProfileImage.texture);
                _originalCreatorProfileImage.texture = null;
            }
            
            long characterId;
            List<FileInfo> thumbnails;
            
            if (OpenedFromVideo.OriginalCreator != null)
            {
                var creator = OpenedFromVideo.OriginalCreator;
                characterId = creator.MainCharacterId.Value;
                thumbnails = creator.MainCharacterFiles;
            }
            else
            {
                characterId = OpenedFromVideo.Owner.MainCharacterId.Value;
                thumbnails = OpenedFromVideo.Owner.MainCharacterFiles;
            }

            var thumbnailResp = await _bridge.GetCharacterThumbnailAsync(characterId,
                                                     thumbnails.First(x => x.Resolution == Resolution._128x128));
            if (thumbnailResp.IsSuccess)
            {
                _originalCreatorProfileImage.texture = thumbnailResp.Object as Texture2D;
            }
        }

        private void OnProfileClicked()
        {
            if (_blockedAccountsManager.IsUserBlocked(_originalCreatorGroupId))
            {
                OpenRestrictedAccessPopup();
            }
            else
            {
                _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(_originalCreatorGroupId, _originalCreatorNickName));
            }
        }

        private void OnRemixVideoDownload()
        {
            _videoListLoader.NewPageAppended -= OnRemixVideoDownload;

            var peopleUsedAmount = 0;

            if (_videoListLoader.LevelPreviewArgs != null && _videoListLoader.LevelPreviewArgs.Count > 0)
            {
                peopleUsedAmount = _videoListLoader.LevelPreviewArgs.Select(levelItem => levelItem.Video.GroupId)
                    .Distinct().Count();
            }

            SetPeopleUsedAmountText(peopleUsedAmount);
        }
        
        private void SetPeopleUsedAmountText(int amount)
        {
            _peopleUsedText.text = string.Format(_localization.RemixUsedCounterFormat, amount);
        }

        private void OnBackButtonClicked()
        {
            _pageManager.MoveBackTo(PageId.Feed);
        }

        private void SetupCreatorThumbnailButtons(bool hasOriginalCreator)
        {
            var remixedFromVideoId = _pageArgs.Video.RemixedFromVideoId ?? 0;
            var originalCreatorVideoId = hasOriginalCreator ? remixedFromVideoId : _pageArgs.Video.Id;
            
            _originalVideoButton.onClick.AddListener(()=>GoToVideo(originalCreatorVideoId, originalCreatorVideoId, originalCreatorVideoId));
        }

        private void OnEnable()
        {
            _remixButton.onClick.AddListener(StartRemixSetup);
        }

        private void OnDisable()
        {
            _remixButton.onClick.RemoveListener(StartRemixSetup);
            _videosGrid.CleanUp();
        }

        private void StartRemixSetup()
        {
            _remixSetup.Setup(_pageArgs.Video);
        }
        
        private BaseFeedArgs GetVideoArgs(long? remixedFromVideoId, long videoId, long idOfFirstVideoToShow)
        {
            return new RemixFeedArgs(remixedFromVideoId, videoId, _bridge, _videoManager, idOfFirstVideoToShow);
        }

        private void GoToVideo(long? remixedFromVideoId, long videoId, long idOfFirstVideoToShow)
        {
            _pageManager.MoveNext(PageId.Feed, GetVideoArgs(remixedFromVideoId, videoId, idOfFirstVideoToShow));
        }
        
        private void OpenRestrictedAccessPopup()
        {
            _popupManagerHelper.ShowUserProfileRestrictedPopup();
        }
    }
}