using System;
using System.Linq;
using System.Threading;
using Bridge;
using Common.Publishers;
using Extensions;
using Models;
using Modules.InputHandling;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.LevelEditor.Ui.ShoppingCart;
using UIManaging.Pages.PublishPage.Buttons;
using UIManaging.Pages.PublishPage.VideoDetails;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    internal sealed class PublishPage : PublishPageBase<PublishPageArgs, LevelVideoMessageSettingsPanel, LevelVideoPostSettingsPanel>
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private PublishVideoButtonStateControl _publishVideoButtonStateControl;
        [SerializeField] private ShoppingCartPublishManager _cartManager;
        [SerializeField] private Button _saveToDraftsButton;
        [SerializeField] private PublishTypeSelectionPanel _publishTypeSelectionPanel;
        [SerializeField] private LevelVideoPostSettingsPanel _postVideoSettingsPanel;
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private PublishPageLocalization _localization;
        [SerializeField] private LevelVideoPostPreviewPanel _previewPanel;

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private IInputManager _inputManager;
        [Inject] private IPublishVideoController _publishVideoController;
        [Inject] private PublishVideoHelper _publishHelper;
        
        private ShareDestinationData _shareDestinationData;
        private Level _levelData;
        private CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.PublishPage;
        private bool IsPublishingAllowed => !(_publishVideoController.IsSavingInProgress || _publishHelper.IsPublishing);
        protected override PublishingType CurrentPublishingType => _publishTypeSelectionPanel.CurrentlySelected;

        protected override PublishPageArgs BackToPageArgs
        {
            get
            {
                var backToArgs = OpenPageArgs.Clone() as PublishPageArgs;
                backToArgs.PublishingType = _publishTypeSelectionPanel.CurrentlySelected;
                return backToArgs;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _saveToDraftsButton.onClick.AddListener(()=> OpenPageArgs.OnSaveToDraftsRequested?.Invoke());
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _cartManager.BuyButtonClicked += OnBuyButtonClicked;
            PublishButton.PublishVideoRequested += StartVideoPublishing;
            _publishVideoController.SubscribeToEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _cartManager.BuyButtonClicked -= OnBuyButtonClicked;
            PublishButton.PublishVideoRequested -= StartVideoPublishing;
            _publishVideoController.UnsubscribeFromEvents();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {
        }

        protected override void OnDisplayStart(PublishPageArgs args)
        {
            base.OnDisplayStart(args);
            _levelData = args.LevelData;
            
            _tokenSource?.CancelAndDispose();
            _tokenSource = new CancellationTokenSource();
            
            _inputManager.Enable(false);
            _cartManager.Init(_levelData);
            
            PostVideoDescriptionPanel.VideoDescriptionUpdated += UpdateLevelDescription;
   
            _backButton.onClick.AddListener(OnBackButton);
            SetupPublishButton();
            _publishTypeSelectionPanel.Init(args.PublishingType);
            _publishTypeSelectionPanel.PublishTypeChanged += SwitchSettingsPanel;
            _shareDestinationData = new ShareDestinationData
            {
                Chats = args.ShareDestination.Chats,
                Users = args.ShareDestination.Users,
            };
            VideoMessageSettingsPanel.DestinationSelected += x => _shareDestinationData = x;
            _postVideoSettingsPanel.DestinationSelected += x => _shareDestinationData = x;
            _postVideoSettingsPanel.Init(_levelData, args.VideoUploadingSettings, args.OriginalCreator, args.InitialTemplate);
            VideoMessageSettingsPanel.Init(_levelData, () => OpenPageArgs.OnPreviewRequested?.Invoke(_levelData, ExtractVideoUploadingSettings()), args.VideoUploadingSettings);
            
            _saveToDraftsButton.SetActive(!args.LevelData.IsVideoMessageBased());
            
            PostVideoDescriptionPanel.RawDescriptionText = _levelData.Description;

            var previewRequested = new Action(() => OpenPageArgs.OnPreviewRequested?.Invoke(_levelData, ExtractVideoUploadingSettings()));
            var previewPanelModel = new LevelVideoPostPreviewPanelModel(previewRequested, _levelData);
            
            _previewPanel.Initialize(previewPanelModel);
            
            RefreshPageSettings();
        }
        
        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            UpdateLevelDescription(PostVideoDescriptionPanel.RawDescriptionText);
            _inputManager.Enable(true);
            
            _previewPanel.CleanUp();
        }

        protected override void ShowSettingsPanel() => SwitchSettingsPanel(CurrentPublishingType);

        protected override void RefreshPageSettings()
        {
            switch (CurrentPublishingType)
            {
                case PublishingType.Post:
                    _postVideoSettingsPanel.Refresh();
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
        
        private async void StartVideoPublishing()
        {
            if (!IsPublishingAllowed)
            {
                OnPublishingForbidden();
                return;
            }

            if (CurrentPublishingType == PublishingType.Post && PostVideoDescriptionPanel.IsCharLimitExceeded)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.DescriptionCharacterLimitMessage, 2);
                PostVideoDescriptionPanel.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededChanged;
                OnPublishingForbidden();
                return;
            }

            if (!await ValidateText() || !await ValidateDestination())
            {
                OnPublishingForbidden();
                return;
            }

            var videoUploadSettings = ExtractVideoUploadingSettings();
            OpenPageArgs.OnPublishRequested?.Invoke(videoUploadSettings);
        }

        private VideoUploadingSettings ExtractVideoUploadingSettings()
        {
            var selectedIds = _postVideoSettingsPanel.SelectedUsers.Select(profile => profile.Id).ToList();

            var videoUploadSettings = new VideoUploadingSettings
            {
                PublishingType = _publishTypeSelectionPanel.CurrentlySelected,
                PublishInfo = new PublishInfo
                {
                    Access = _postVideoSettingsPanel.Access,
                    SelectedUsers = selectedIds,
                    DescriptionText = PostVideoDescriptionPanel.ParsedDescriptionText,
                    SaveToDevice = _postVideoSettingsPanel.SaveToDevice,
                    IsLandscapeMode = _postVideoSettingsPanel.IsLandscapeMode,
                    IsUltraHDMode = _postVideoSettingsPanel.IsUltraHDMode,
                    GenerateTemplate = _postVideoSettingsPanel.GenerateTemplate,
                    GenerateTemplateWithName = _postVideoSettingsPanel.GenerateTemplateName,
                    ExternalLinkType = _postVideoSettingsPanel.ExternalLinkType,
                    ExternalLink = _postVideoSettingsPanel.ExternalLink,
                    OnClearPublishData = () =>
                    {
                        _postVideoSettingsPanel.TaggedUsers = null;
                        _postVideoSettingsPanel.SelectedUsers = null;
                        _postVideoSettingsPanel.ExternalLink = null;
                        _postVideoSettingsPanel.ExternalLinkType = ExternalLinkType.Invalid;
                    },
                    AllowRemix = _postVideoSettingsPanel.ContentAccessSettings.AllowRemix,
                    AllowComment= _postVideoSettingsPanel.ContentAccessSettings.AllowComment,
                },
                MessagePublishInfo = new MessagePublishInfo
                {
                    MessageText = VideoMessageSettingsPanel.Message
                }
            };
            
            if (_publishTypeSelectionPanel.CurrentlySelected == PublishingType.Post)
            {
                var messagePublishModel = videoUploadSettings.MessagePublishInfo;
                messagePublishModel.ShareDestination.Chats = _postVideoSettingsPanel.ShareDestinationData?.Chats;
                messagePublishModel.ShareDestination.Users = _postVideoSettingsPanel.ShareDestinationData?.Users;
            }
            else
            {
                var messagePublishModel = videoUploadSettings.MessagePublishInfo;
                messagePublishModel.ShareDestination.Chats = VideoMessageSettingsPanel.ShareDestinationData?.Chats;
                messagePublishModel.ShareDestination.Users = VideoMessageSettingsPanel.ShareDestinationData?.Users;
                messagePublishModel.MessageText = VideoMessageSettingsPanel.Message;
            }

            return videoUploadSettings;
        }
        
        private void SetupPublishButton()
        {
            RefreshPublishButton();
        }

        private void RefreshPublishButton()
        {
            _publishVideoButtonStateControl.UpdateIconAndLabel(_publishTypeSelectionPanel.CurrentlySelected);
        }

        private void OnBackButton()
        {
            OpenPageArgs.OnMoveBackRequested?.Invoke();
        }
        
        private void UpdateLevelDescription(string descriptionText)
        {
            _levelData.Description = descriptionText;
            _bridge.UpdateLevelDescription(_levelData.Id, _levelData.Description);
        }

        private void OnBuyButtonClicked()
        {
            StartVideoPublishing();
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
                    _postVideoSettingsPanel.Show();
                    VideoMessageSettingsPanel.SetActive(false);
                    break;
                case PublishingType.VideoMessage:
                    VideoMessageSettingsPanel.Show();
                    _postVideoSettingsPanel.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(publishingType), publishingType, null);
            }
            
            RefreshPageSettings();
        }
    }
}