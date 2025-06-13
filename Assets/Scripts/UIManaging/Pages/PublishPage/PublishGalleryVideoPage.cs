using System;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using Bridge;
using Bridge.Models;
using Bridge.Models.VideoServer;
using Bridge.VideoServer;
using Common.Publishers;
using Extensions;
using Models;
using Modules.AppsFlyerManaging;
using Navigation.Args;
using Navigation.Core;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.PublishPage.Buttons;
using UIManaging.Pages.PublishPage.VideoDetails;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Views;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    internal sealed class PublishGalleryVideoPage : PublishPageBase<PublishGalleryVideoPageArgs, NonLevelVideoMessageSettingsPanel, NonLevelVideoPostSettingsPanel>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private PrivacyButton _privacyButton;
        [SerializeField] private PublishVideoButtonStateControl _publishVideoButtonStateControl;
        [SerializeField] private Button _previewVideoButton;
        [SerializeField] private Button _cancelVideoPreviewButton;
        [SerializeField] private DisplayUGUI _displayUgui;
        [SerializeField] private GameObject _previewVideoParent;
        [SerializeField] private Button _externalLinksButton;
        [SerializeField] private TextMeshProUGUI _externalLinksText;
        [SerializeField] private Color _externalLinksColorDiscord;
        [SerializeField] private Color _externalLinksColorOther;
        [SerializeField] private PublishTypeSelectionPanel _publishTypeSelectionPanel;
        [SerializeField] private PublishPageLocalization _localization;
        [SerializeField] private NonLevelVideoPostPreviewPanel _previewPanel;
 
        [Inject] private INativeGallery _nativeGallery;
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private PageManager _pageManager;
        [Inject] private SnackBarHelper _snackBarHelper;

        private string _videoPath;
        private int _videoDurationSec;
        private Texture2D _videoTexture;
        private string _externalLink;
        private ExternalLinkType _externalLinkType = ExternalLinkType.Invalid;
        private ShareDestinationData _shareDestinationData;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.PublishGalleryVideoPage;

        protected override PublishingType CurrentPublishingType => _publishTypeSelectionPanel.CurrentlySelected;

        protected override PublishGalleryVideoPageArgs BackToPageArgs
        {
            get
            {
                var backToArgs = OpenPageArgs.Clone() as PublishGalleryVideoPageArgs;
                backToArgs.PublishingType = CurrentPublishingType;
                return backToArgs;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _privacyButton.Access = VideoAccess.Public;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _externalLinksButton.onClick.AddListener(OpenExternalLinksPage);
            PublishButton.PublishVideoRequested += StartVideoUploading;
            _previewVideoButton.onClick.AddListener(PreviewVideo);
            _cancelVideoPreviewButton.onClick.AddListener(CancelVideoPreview);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _externalLinksButton.onClick.RemoveListener(OpenExternalLinksPage);
            PublishButton.PublishVideoRequested -= StartVideoUploading;
            _previewVideoButton.onClick.RemoveListener(PreviewVideo);
            _cancelVideoPreviewButton.onClick.RemoveListener(CancelVideoPreview);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnDisplayStart(PublishGalleryVideoPageArgs args)
        {
            base.OnDisplayStart(args);
            _videoPath = args.VideoData.Path;
            _videoDurationSec = args.VideoData.DurationSec;
            _pageHeaderView.Init(new PageHeaderArgs(_localization.PageHeaderPostVideo, new ButtonArgs(null, args.OnMoveBack ?? _pageManager.MoveBack)));
            _videoTexture = _nativeGallery.GetVideoThumbnail(_videoPath);

            SetupExternalLinksButton();
            
            _publishTypeSelectionPanel.Init(args.PublishingType);
            _publishTypeSelectionPanel.PublishTypeChanged += SwitchSettingsPanel;
            
            PostVideoDescriptionPanel.DescriptionPlaceholder = _localization.GetVideoDescriptionPlaceholder(CurrentPublishingType);
            
            _shareDestinationData = new ShareDestinationData
            {
                Chats = args.ShareDestination.Chats,
                Users = args.ShareDestination.Users,
            };

            VideoMessageSettingsPanel.DestinationSelected += x => _shareDestinationData = x;
            PostSettingsPanel.DestinationSelected += x => _shareDestinationData = x;
            
            _previewPanel.Initialize(new NonLevelVideoPostPreviewPanelModel(PreviewVideo, _videoTexture));
            
            VideoPostAttributesModel.UploadVideo.Value = true;
            
            RefreshPageSettings();
        }
        
        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _previewPanel.CleanUp();
        }

        protected override void ShowSettingsPanel() => SwitchSettingsPanel(CurrentPublishingType);

        protected override void OnInit(PageManager pageManager)
        {
        }
        
        protected override void RefreshPageSettings()
        {
            switch (CurrentPublishingType)
            {
                case PublishingType.Post:
                    break;
                case PublishingType.VideoMessage:
                    VideoMessageSettingsPanel.SetShareDestinationData(_shareDestinationData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentPublishingType), CurrentPublishingType, null);
            }
            
            _pageHeaderView.Header = _localization.GetPageHeader(CurrentPublishingType);
            PostVideoDescriptionPanel.DescriptionPlaceholder = _localization.GetVideoDescriptionPlaceholder(CurrentPublishingType);

            RefreshPublishButton();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupExternalLinksButton()
        {
            _externalLinksButton.SetActive(_localUserDataHolder.IsStarCreator); // only for star creators
            _externalLinksText.text = _externalLinkType == ExternalLinkType.Invalid ? "" : _localization.LinkAddedText;
            _externalLinksText.color = _externalLinkType == ExternalLinkType.Discord
                ? _externalLinksColorDiscord
                : _externalLinksColorOther;
        }
        
        private async void StartVideoUploading()
        {
            PublishButton.Interactable = false;
            
            if (CurrentPublishingType == PublishingType.Post && PostVideoDescriptionPanel.IsCharLimitExceeded)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.DescriptionCharacterLimitMessage, 2);
                PostVideoDescriptionPanel.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededChanged;
                OnPublishingForbidden();
                return;
            }

            var isValid = await ValidateText();
            if (!isValid)
            {
                OnPublishingForbidden();
                return;
            }

            var publishType = _publishTypeSelectionPanel.CurrentlySelected == PublishingType.VideoMessage 
                ? ServerConstants.VideoPublishingType.VIDEO_MESSAGE 
                : ServerConstants.VideoPublishingType.STANDARD;
            var deployData = new DeployNonLevelVideoReq(_videoPath, _videoDurationSec, _privacyButton.Access, publishType, PostVideoDescriptionPanel.ParsedDescriptionText, links:new Dictionary<string, string>(), taggedFriendIds:_privacyButton.SelectedUsers.Select(u => u.Id).ToArray());
            
            if (_externalLinkType != ExternalLinkType.Invalid)
            {
                var linkTypeStr = _externalLinkType.ToString();
                deployData.Links[linkTypeStr] = _externalLink;
            }

            if (!await ValidateDestination())
            {
                OnPublishingForbidden();
                return;
            }
            
            var selectedIds = PostSettingsPanel.SelectedUsers.Select(profile => profile.Id).ToList();
            var videoUploadSettings = new VideoUploadingSettings
            {
                PublishingType = _publishTypeSelectionPanel.CurrentlySelected,
                PublishInfo = new PublishInfo
                {
                    Access = PostSettingsPanel.Access,
                    SelectedUsers = selectedIds,
                    DescriptionText = PostVideoDescriptionPanel.ParsedDescriptionText,
                    ExternalLinkType = PostSettingsPanel.ExternalLinkType,
                    ExternalLink = PostSettingsPanel.ExternalLink,
                    AllowComment = PostSettingsPanel.ContentAccessSettings.AllowComment,
                    OnClearPublishData = () =>
                    {
                        PostSettingsPanel.TaggedUsers = null;
                        PostSettingsPanel.SelectedUsers = null;
                        PostSettingsPanel.ExternalLink = null;
                        PostSettingsPanel.ExternalLinkType = ExternalLinkType.Invalid;
                    }
                },
                MessagePublishInfo = new MessagePublishInfo
                {
                    MessageText = VideoMessageSettingsPanel.Message
                }
            };

            if (_publishTypeSelectionPanel.CurrentlySelected == PublishingType.Post)
            {
                var messagePublishModel = videoUploadSettings.MessagePublishInfo;
                messagePublishModel.ShareDestination.Chats = PostSettingsPanel.ShareDestinationData?.Chats;
                messagePublishModel.ShareDestination.Users = PostSettingsPanel.ShareDestinationData?.Users;
            }
            else
            {
                var messagePublishModel = videoUploadSettings.MessagePublishInfo;
                messagePublishModel.ShareDestination.Chats = VideoMessageSettingsPanel.ShareDestinationData?.Chats;
                messagePublishModel.ShareDestination.Users = VideoMessageSettingsPanel.ShareDestinationData?.Users;
                messagePublishModel.MessageText = VideoMessageSettingsPanel.Message;
            }

            OpenPageArgs.OnMoveForward.Invoke(videoUploadSettings);
            
            AppsFlyer.sendEvent(AppsFlyerConstants.VIDEO_SUCCESSFULLY_CREATED, null);
            
            _externalLink = null;
            _externalLinkType = ExternalLinkType.Invalid;
        }

        private void OpenExternalLinksPage()
        {
            _pageManager.MoveNext(new ExternalLinksPageArgs
            {
                IsActive = _externalLinkType != ExternalLinkType.Invalid,
                CurrentLink = _externalLink,
                OnSave = (linkType, link) =>
                {
                    _externalLinkType = linkType;
                    _externalLink = link;

                    if (_externalLinkType != ExternalLinkType.Invalid)
                    {
                        _pageManager.MoveBack();
                    }
                }
            });
        }

        private void PreviewVideo()
        {
            OpenVideo();
        }

        private void CancelVideoPreview()
        {
            ClosePreview();
        }

        private void OpenVideo()
        {
            _previewVideoParent.SetActive(true);
            var mediaPlayer = _displayUgui.CurrentMediaPlayer;
            mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, _videoPath);
            mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
        }

        private void ClosePreview()
        {
            var mediaPlayer = _displayUgui.CurrentMediaPlayer;
            mediaPlayer.CloseMedia();
            mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
            _previewVideoParent.SetActive(false);
        }

        private void OnMediaPlayerEvent(MediaPlayer player, MediaPlayerEvent.EventType type, ErrorCode errorCode)
        {
            if (type != MediaPlayerEvent.EventType.FinishedPlaying) return;
            ClosePreview();
        }

        private void OnCharacterLimitExceededChanged(bool isExceeded)
        {
            PublishButton.Interactable = !isExceeded;
            PostVideoDescriptionPanel.CharacterLimitExceededStatusChanged -= OnCharacterLimitExceededChanged;
        }
        
        private void SwitchSettingsPanel(PublishingType publishingType)
        {
            switch (publishingType)
            {
                case PublishingType.Post:
                    PostSettingsPanel.Show();
                    PostSettingsPanel.Init();
                    PostSettingsPanel.Refresh();
                    VideoMessageSettingsPanel.SetActive(false);
                    break;
                case PublishingType.VideoMessage:
                    VideoMessageSettingsPanel.Show();
                    VideoMessageSettingsPanel.Init(PreviewVideo, _videoTexture);
                    PostSettingsPanel.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(publishingType), publishingType, null);
            }
            
            RefreshPageSettings();
        }
        
        private void RefreshPublishButton()
        {
            _publishVideoButtonStateControl.UpdateIconAndLabel(_publishTypeSelectionPanel.CurrentlySelected);
        }
    }
}