using System;
using System.Collections.Generic;
using Bridge;
using Common.Services;
using Extensions;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.EditTemplate;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.VideosBasedOnTemplatePage
{
    internal sealed class VideosBasedOnTemplatePage : GenericPage<BaseVideoTemplatePageArgs>
    {
        private const int ORIGINAL_VIDEO_FEED_INDEX = 10;
        [SerializeField] private PageHeaderView _pageHeaderView;
        [Space]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _usedText;
        [Space]
        [SerializeField] private UseTemplateButton _useTemplateButton;
        [SerializeField] private VideoList _videosGrid;
        [SerializeField] private Button _contextMenuButton;
        [SerializeField] private VideoThumbnail _originalVideoThumbnail;
        [SerializeField] private Button _originalVideoThumbnailButton;

        [SerializeField] private GameObject _templateCreatorGroup;
        [SerializeField] private UserPortraitView _templateCreatorAvatar;
        [SerializeField] private TMP_Text _templateCreatorName;
        [SerializeField] private Button _templateCreatorButton;

        [Inject] private IBridge _bridge;
        [Inject] private VideoManager _videoManager;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private ITemplateManagingService _templateManagingService;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private FeedLocalization _feedLocalization;
        
        private BaseVideoListLoader _baseTemplateVideoListLoader;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.VideosBasedOnTemplatePage;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            if (_contextMenuButton != null)
                _contextMenuButton.onClick.AddListener(ShowContextMenu);
            
            _templateCreatorButton.onClick.AddListener(OpenCreatorProfile);
        }

        private void OpenCreatorProfile()
        {
            _pageManager.MoveNext(new UserProfileArgs(OpenPageArgs.TemplateInfo.CreatorId, null));
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }
        
        protected override void OnDisplayStart(BaseVideoTemplatePageArgs args)
        {
            base.OnDisplayStart(args);

            var moveBackAction = args.OnBackButtonRequested ?? _pageManager.MoveBack;
            _pageHeaderView.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, moveBackAction)));

            UpdateTitleText();
            SetupUseTemplateButton();
            SetupTemplateCreatorView();
            SetPeopleUsedAmountText();
            SetupTemplateThumbnail();

            _baseTemplateVideoListLoader = OpenPageArgs.GetVideoListLoader(_pageManager,_videoManager);
            _contextMenuButton.gameObject.SetActive(args.TemplateInfo?.CreatorId == _bridge.Profile.GroupId);
            _videosGrid.Initialize(_baseTemplateVideoListLoader);
            _originalVideoThumbnailButton.onClick.AddListener(OnOriginalVideoClicked);
            _popupManager.ClosePopupByType(PopupType.Loading);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _originalVideoThumbnailButton.onClick.RemoveListener(OnOriginalVideoClicked);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateTitleText()
        {
            _titleText.text = OpenPageArgs.TemplateType == TemplateType.Hashtag
                ? $"#{OpenPageArgs.TemplateName}"
                : OpenPageArgs.TemplateName;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_titleText.transform.parent);
        }
        
        private void SetupTemplateThumbnail()
        {
            var openedForTemplate = OpenPageArgs.TemplateType == TemplateType.Challenge;
            _originalVideoThumbnail.gameObject.SetActive(openedForTemplate);
            
            if(!openedForTemplate) return;
            
            var templateVideoUrl = _bridge.GetTemplateVideoUrl(OpenPageArgs.TemplateInfo);
            _originalVideoThumbnail.Initialize(new VideoThumbnailModel(templateVideoUrl));
        }

        private void SetupUseTemplateButton()
        {
            if (OpenPageArgs.TemplateType == TemplateType.Hashtag)
            {
                _useTemplateButton.Setup(((VideosBasedOnHashtagPageArgs)OpenPageArgs).HashtagInfo);
            }
            else
            {
                _useTemplateButton.Setup(OpenPageArgs.TemplateInfo.Id, OpenPageArgs.OnJoinTemplateRequested);
            }
        }

        private void SetPeopleUsedAmountText()
        {
            var amount = OpenPageArgs.UsageCount.ToShortenedString();
            _usedText.text = string.Format(_feedLocalization.RemixUsedCounterFormat, amount);
        }

        private async void SetupTemplateCreatorView()
        {
            var openedForTemplate = OpenPageArgs.TemplateType == TemplateType.Challenge;
            
            _templateCreatorGroup.SetActive(openedForTemplate);
            _templateCreatorName.text = string.Empty;
            
            if(!openedForTemplate) return;
            
            _templateCreatorAvatar.Initialize(new UserPortraitModel() {UserGroupId = OpenPageArgs.TemplateInfo.CreatorId});
            var profileResult = await _bridge.GetProfile(OpenPageArgs.TemplateInfo.CreatorId);

            if (profileResult.IsSuccess)
            {
                _templateCreatorName.text = profileResult.Profile.NickName;
            }
        }

        private void ShowContextMenu()
        {
            var variants = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>(_feedLocalization.TemplateEditOption, () =>
                {
                    _pageManager.MoveNext(new EditTemplateFeedPageArgs
                    {
                        TemplateName = OpenPageArgs.TemplateName,
                        NameUpdatedCallback = OnTemplateRenamed,
                        GenerateTemplate = true,
                        IsVideoPublic = true,
                        OpenForRename = true
                    });
                }),
            };
            
            var config = new ActionSheetPopupConfiguration
            {
                MainVariantIndexes = Array.Empty<int>(),
                PopupType = PopupType.ActionSheet,
                Variants = variants
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private async void OnTemplateRenamed(bool generateTemplate, string newTemplateName)
        {
            if (newTemplateName == OpenPageArgs.TemplateName) return;
            
            var result = await _templateManagingService.ChangeTemplateName(OpenPageArgs.TemplateInfo, newTemplateName);

            if (result.IsSuccess)
            {
                _snackBarHelper.ShowInformationSnackBar(_feedLocalization.TemplateRenameCompletedSnackbarMessage, 2f);
                OpenPageArgs.TemplateName = newTemplateName;
                if (OpenPageArgs.TemplateInfo != null)
                {
                    OpenPageArgs.TemplateInfo.Title = newTemplateName;
                }
                UpdateTitleText();
                return;
            }

            _snackBarHelper.ShowInformationSnackBar(_feedLocalization.TemplateNotAvailableSnackbarMessage, 2f);
        }

        private void OnOriginalVideoClicked()
        {
            _pageManager.MoveNext(PageId.Feed, new VideosBasedOnTemplateFeedArgs(_videoManager, 
                                      OpenPageArgs.TemplateInfo.OriginalVideoId,
                                      ORIGINAL_VIDEO_FEED_INDEX, OpenPageArgs.TemplateInfo.Id));
        }
    }
}