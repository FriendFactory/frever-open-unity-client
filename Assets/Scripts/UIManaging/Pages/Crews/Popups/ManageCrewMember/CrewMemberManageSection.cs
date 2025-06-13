using Abstract;
using Common;
using Modules.Crew;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups.ManageCrewMember
{
    internal sealed class CrewMemberManageSectionModel
    {
        public readonly long UserId;
        public readonly string Nickname;
        public readonly long RoleId;

        public CrewMemberManageSectionModel(long userId, string nickname, long roleId)
        {
            UserId = userId;
            Nickname = nickname;
            RoleId = roleId;
        }
    }
    internal sealed class CrewMemberManageSection : BaseContextDataView<CrewMemberManageSectionModel>
    {
        private const float INACTIVE_ALPHA = 0.25f;
        
        [SerializeField] private Button _transferOwnershipButton;
        [SerializeField] private CanvasGroup _transferCanvasGroup;
        [SerializeField] private Button _kickButton;
        [SerializeField] private CanvasGroup _kickButtonCanvasGroup;

        [Inject] private CrewService _crewService;
        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUser;

        private void OnEnable()
        {
            _transferOwnershipButton.onClick.AddListener(OnOwnershipTransferRequested);
            _kickButton.onClick.AddListener(KickUser);
        }

        private void OnDisable()
        {
            _transferOwnershipButton.onClick.RemoveAllListeners();
            _kickButton.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            var canTransferOwnership = _crewService.LocalUserIsLeader;
            _transferCanvasGroup.alpha = canTransferOwnership ? 1.0f : INACTIVE_ALPHA;
            _transferCanvasGroup.interactable = canTransferOwnership;
            
            var canBeKicked = ContextData.UserId != _localUser.GroupId &&
                              ContextData.RoleId != Constants.Crew.LEADER_ROLE_ID;
            
            _kickButtonCanvasGroup.alpha = canBeKicked ? 1.0f : INACTIVE_ALPHA;
            _kickButtonCanvasGroup.interactable = canBeKicked;
        }

        private void OnOwnershipTransferRequested()
        {
            if (_localUser.GroupId == ContextData.UserId)
            {
                var cfg = new TransferOwnershipPopupConfiguration(_crewService.Model.Id, _crewService.Model.Members,
                                                                  _localUser.GroupId);
                _popupManager.SetupPopup(cfg);
                _popupManager.ShowPopup(cfg.PopupType, true);

                return;
            }
            _crewService.TransferOwnership(ContextData.UserId, OnSuccess);
            
            void OnSuccess() => _popupManager.ClosePopupByType(PopupType.ManageCrewMember);
        }

        private void KickUser()
        {
            _crewService.TryKickMember(ContextData.UserId, ContextData.Nickname, OnKickSuccessful);
        }

        private void OnKickSuccessful()
        {
            _popupManager.ClosePopupByType(PopupType.ManageCrewMember);
        }
    }
}