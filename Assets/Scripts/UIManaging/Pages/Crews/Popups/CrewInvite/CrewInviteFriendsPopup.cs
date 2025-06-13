using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer;
using Extensions;
using Navigation.Args;
using UIManaging.Animated.Behaviours;
using UIManaging.Localization;
using UIManaging.Pages.UserSelection;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups
{
    internal sealed class CrewInviteFriendsPopup : BasePopup<CrewInviteFriendPopupConfiguration>
    {
        [SerializeField] private List<Button> _closeButtons = new List<Button>();
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;
        [Space]
        [SerializeField] private UserSelectionWidget _invitedUsersSelectionWidget;
        [SerializeField] private Button _sendButton;

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewPageLocalization _localization;
        
        private UserSelectionPanelModel _selectionPanelModel;
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _sendButton.onClick.AddListener(OnSendButton);
        }

        private void OnEnable()
        {
            if(Configs is null) return;
            
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            _animatedBehaviour.PlayInAnimation(null);
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
            _tokenSource.CancelAndDispose();
        }

        protected override async void OnConfigure(CrewInviteFriendPopupConfiguration configuration)
        {
            _tokenSource = new CancellationTokenSource();
            var invitedUsers = await GetListOfInvitedUsers(configuration.CrewId);

            var invitedUsersModels = invitedUsers?.Length > 0
                ? invitedUsers.Select(profile => new UserSelectionItemModel(profile)).ToArray()
                : null;

            
            _selectionPanelModel = new UserSelectionPanelModel(100,
                                                               null,
                                                               null,
                                                               _bridge.Profile.GroupId,
                                                               UserSelectionPageArgs.UsersFilter.Friends,
                                                                invitedUsers?.Select(profile => profile.Id));

            _selectionPanelModel.ItemSelectionChanged += OnUserSelectionChanged;
            _invitedUsersSelectionWidget.Initialize(_selectionPanelModel);
            
            UpdateSendButton();
        }
        
        private void OnUserSelectionChanged(UserSelectionItemModel _)
        {
            UpdateSendButton();
        }

        private void UpdateSendButton()
        {
            _sendButton.interactable = _invitedUsersSelectionWidget.SelectedUsers.Count > 0;
        }

        private void OnCloseButtonClicked()
        {
            _animatedBehaviour.PlayOutAnimation(OnOutAnimationCompleted);

            void OnOutAnimationCompleted()
            {
                Hide();
            }
        }

        private async Task<GroupShortInfo[]> GetListOfInvitedUsers(long crewId)
        {
            var result = await _bridge.GetInvitedUsers(crewId, _tokenSource.Token);
            
            if (result.IsSuccess && result.Models?.Length > 0)
            {
                var profiles = await _bridge.GetProfilesShortInfo(result.Models);
                return profiles.IsSuccess ? profiles.Profiles : null;
            }
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);

            return null;
        }

        private async void OnSendButton()
        {
            if (_invitedUsersSelectionWidget.SelectedUsers.Count <= 0) return;
            
            var result = await _bridge.InviteUsersToCrew(Configs.CrewId,
                                            _invitedUsersSelectionWidget.SelectedUsers
                                                                        .Select(user => user.Id).ToArray());

            if (result.IsSuccess)
            {
                _snackBarHelper.ShowSuccessDarkSnackBar(_localization.InviteSuccessSnackbarMessage);
            }
            else
            {
                _snackBarHelper.ShowFailSnackBar(_localization.InviteFailedSnackbarMessage);
            }
        }
    }
}