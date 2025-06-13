using System.Collections.Generic;
using System.Globalization;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Common;
using Extensions;
using Modules.Crew;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.FollowersPage.UI;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.Popups.ManageCrewMember
{
    internal sealed class ManageCrewMemberPopup : BasePopup<ManageCrewMemberPopupConfiguration>
    {
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;

        [SerializeField] private UserPortraitView _userPortrait;
        [SerializeField] private TMP_Text _joinedDate;
        [SerializeField] private TMP_Text _lastLoggedInDate;
        
        [Space]
        [SerializeField] private CrewMemberManageSection _managePanel;
        [SerializeField] private CrewMemberRoleSection _roleSection;

        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _roleText;
        
        [SerializeField] private FollowUserButton _followUserButton;
        
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private CrewService _crewService;
        [Inject] private CrewPageLocalization _localization;

        private void OnEnable()
        {
            if (Configs is null) return;

            _animatedBehaviour.PlayInAnimation(null);
            
            _closeButtons.ForEach(b => b.interactable = true);
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
        }

        protected override void OnConfigure(ManageCrewMemberPopupConfiguration configuration)
        {
            ProjectContext.Instance.Container.InjectGameObject(gameObject);
            
            _usernameText.text = Configs.NickName;
            _roleText.text = _localization.GetRankLocalized(Configs.MemberRoleId);

            _lastLoggedInDate.text = Configs.LastLogin;
            _joinedDate.text = Configs.Joined;

            var userRoleId = Configs.MemberRoleId;
            var memberIsLeader = userRoleId == Constants.Crew.LEADER_ROLE_ID;
            var localUserIsAdmin = _crewService.LocalUserIsAdmin;
            
            _managePanel.Initialize(new CrewMemberManageSectionModel(Configs.MemberGroupId, Configs.NickName, Configs.MemberRoleId));
            
            _roleSection.SetActive(localUserIsAdmin && !memberIsLeader);
            if (localUserIsAdmin && !memberIsLeader) _roleSection.Initialize(new CrewMemberRolesSectionModel(Configs.MemberGroupId, Configs.MemberRoleId));

            SetupFollowButton();
        }

        private async void SetupFollowButton()
        {
            _followUserButton.SetActive(false);
            
            var profile = await _bridge.GetProfile(Configs.MemberGroupId);

            if (!profile.IsSuccess)
            {
                return;
            }
            
            _userPortrait.Initialize(new UserPortraitModel
            {
                UserGroupId = profile.Profile.MainGroupId,
                UserMainCharacterId = profile.Profile.MainCharacter.Id,
                MainCharacterThumbnail = profile.Profile.MainCharacter.Files,
                Resolution = Resolution._256x256
            });
            
            if (Configs.MemberGroupId == _userData.GroupId) return;
            
            _followUserButton.SetActive(true);
            _followUserButton.Initialize(new FollowUserButtonArgs(profile.Profile));
        }

        private void OnCloseButtonClicked()
        {
            _animatedBehaviour.PlayOutAnimation(OnOutComplete);
            _closeButtons.ForEach(b => b.interactable = false);

            void OnOutComplete()
            {
                Hide();
            }
        }
    }
}