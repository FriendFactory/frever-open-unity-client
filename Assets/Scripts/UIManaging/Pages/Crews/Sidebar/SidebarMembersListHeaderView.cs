using System.Collections.Generic;
using Abstract;
using Bridge.Models.ClientServer.Crews;
using Common;
using Modules.Crew;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMembersListHeaderView : BaseContextDataView<SidebarMembersListHeaderModel>
    {
        [SerializeField] private TMP_Text _membersCounter;
        [SerializeField] private CrewInviteButton _inviteButton;

        [Inject] private CrewService _crewService;
        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewPageLocalization _localization;

        private void OnEnable()
        {
            _inviteButton.OnInviteActionRequested += OnInviteButtonClicked;
            
            if (ContextData is null) return;
            
            _crewService.CrewModelUpdated += OnCrewDataUpdated;
            _crewService.MembersListUpdated += OnMembersListUpdated;
        }

        private void OnDisable()
        {
            _inviteButton.OnInviteActionRequested -= OnInviteButtonClicked;
            _crewService.CrewModelUpdated -= OnCrewDataUpdated;
        }

        protected override void OnInitialized()
        {
            _membersCounter.text = ContextData.MembersCountText;
            _inviteButton.Initialize(!UserCanInvite());
        }

        private void OnInviteButtonClicked()
        {
            if (!UserCanInvite())
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.InviteAccessRestrictedSnackbarMessage);
                return;
            }

            _inviteButton.Interactable = false;
            _popupManager.SetupPopup(new CrewInviteFriendPopupConfiguration(ContextData.CrewId, ContextData.MemberIds, _localUser.GroupId, OnInvitePopupClosed));
            _popupManager.ShowPopup(PopupType.CrewInviteFriends, true);
        }

        private bool UserCanInvite()
        {
            return ContextData.UserRole <= Constants.Crew.COORDINATOR_ROLE_ID || _crewService.Model.IsPublic;
        }

        private void OnInvitePopupClosed(object _)
        {
            _inviteButton.Interactable = true;
        }

        private void OnCrewDataUpdated(CrewModel crewModel)
        {
            Refresh();
        }

        private void OnMembersListUpdated(IReadOnlyCollection<CrewMember> readOnlyCollection)
        {
            Refresh();
        }

        private void Refresh()
        {
            ContextData.UserRole = _crewService.LocalUserMemberData.RoleId;
            _inviteButton.Initialize(!UserCanInvite());
        }
    }
}