using System.Collections.Generic;
using System.Linq;
using Bridge;
using Common.Publishers;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.InputHandling;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common;
using UIManaging.Pages.LevelEditor.Ui.ShoppingCart;
using UIManaging.Pages.PublishPage.Buttons;
using UIManaging.Pages.SharingPage.Ui;
using UIManaging.PopupSystem.Popups.Views;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    internal sealed class OnboardingPublishPage : GenericPage<OnboardingPublishPageArgs>
    {
        private const long  PUBLIC_GROUP = 1;

        [SerializeField] private DescriptionPanel _descriptionPanel;
        [SerializeField] private LevelThumbnail _levelThumbnail;
        [SerializeField] private PrivacyButton _privacyButton;
        [SerializeField] private GameObject _taggedMembersCountParent;
        [SerializeField] private TextMeshProUGUI _taggedMembersCount;
        [SerializeField] private PublishVideoButton _publishButton;
        [SerializeField] private ShoppingCartPublishManager _cartManager;
        [SerializeField] private PreviewLevelButton _previewButton;
        [SerializeField] private PublishVideoButtonStateControl _publishVideoButtonStateControl;
        
        private Level _levelData;
        private IBridge _bridge;
        private AmplitudeManager _amplitudeManager;
        private SnackBarHelper _snackBarHelper;
        private IInputManager _inputManager;
        private IPublishVideoController _publishVideoController;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.OnboardingPublishPage;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, AmplitudeManager amplitudeManager, 
            SnackBarHelper snackBarHelper, IInputManager inputManager, IPublishVideoController publishVideoController)
        {
            _bridge = bridge;
            _amplitudeManager = amplitudeManager;
            _snackBarHelper = snackBarHelper;
            _inputManager = inputManager;
            _publishVideoController = publishVideoController;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _cartManager.BuyButtonClicked += OnBuyButtonClicked;
            _publishButton.PublishVideoRequested += StartVideoPublishing;
            _publishVideoController.SubscribeToEvents();
        }

        private void OnDisable()
        {
            _cartManager.BuyButtonClicked -= OnBuyButtonClicked;
            _publishButton.PublishVideoRequested -= StartVideoPublishing;
            _publishVideoController.UnsubscribeFromEvents();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void StartVideoPublishing()
        {
            if (_descriptionPanel.IsCharLimitExceeded)
            {
                _snackBarHelper.ShowInformationSnackBar("Post up to 250 characters.", 2);
                _descriptionPanel.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededChanged;
                return;
            }
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.PUBLISH_BUTTON_PRESSED,
                                                          new Dictionary<string, object> { ["LevelId"] = OpenPageArgs.LevelData.Id } );
            
            var videoUploadingSettings = new VideoUploadingSettings
            {
                PublishingType = PublishingType.Post,
                PublishInfo = new PublishInfo
                {
                    Access = _privacyButton.Access,
                    DescriptionText = _descriptionPanel.GetParsedText()
                }
            };
            OpenPageArgs.OnMoveNextRequested?.Invoke(videoUploadingSettings);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {
            
        }

        protected override void OnDisplayStart(OnboardingPublishPageArgs args)
        {
            base.OnDisplayStart(args);
            _levelData = args.LevelData;
            _inputManager.Enable(false);
            _previewButton.SetCallback(() => args.OnPreviewRequested?.Invoke(_levelData));

            _levelThumbnail.Initialize(_levelData);
            SetupTaggedMembersCount(args.ShowTaggedMembersCount);
            SetupDescription(args.Description);
            _cartManager.Init(_levelData);
            _privacyButton.Access = args.DefaultVideoAccess;
            //_privacyButton.TaggedUsers = GetTaggedMemberGroupIds(_levelData); TODO: fix this, possibly? not sure this is even needed anymore
            SetupPublishButton();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupPublishButton()
        {
            _publishVideoButtonStateControl.UpdateIconAndLabel(PublishingType.Post);
        }

        private void SetupTaggedMembersCount(bool show)
        {
            _taggedMembersCountParent.SetActive(show);
            if (!show)
            {
                return;
            }
            
            _taggedMembersCount.text = GetTaggedMemberGroupIds(_levelData).Count.ToString();
        }

        private void SetupDescription(string text)
        {
            _descriptionPanel.InputFieldAdapter.Text = text;
            _descriptionPanel.InputFieldAdapter.ForceUpdate();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private HashSet<long> GetTaggedMemberGroupIds(Level level)
        {
            var groups = new HashSet<long>(level.Event
                                            .SelectMany(ev => ev.CharacterController)
                                            .Select(controller => controller.Character.GroupId)
                                            .Where(id => id != PUBLIC_GROUP && id != _bridge.Profile.GroupId)
                                            .Distinct());  //  DWC 2021 upgrade: .ToHashSet() is causing ambiguous issues
            return groups;
        }
        
        private void OnBuyButtonClicked()
        {
            StartVideoPublishing();
        }

        private void OnCharacterLimitExceededChanged(bool isExceeded)
        {
            _publishButton.Interactable = !isExceeded;
            _descriptionPanel.CharacterLimitExceededStatusChanged -= OnCharacterLimitExceededChanged;
        }
    }
}