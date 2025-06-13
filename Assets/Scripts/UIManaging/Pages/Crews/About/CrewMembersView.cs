using Abstract;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal class CrewMembersView : BaseContextDataView<CrewMembersModel>
    {
        [SerializeField] private Button _viewAllButton;
        [SerializeField] private CrewMembersListView _membersList;

        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUser;

        private void OnEnable()
        {
            _viewAllButton.onClick.AddListener(OnViewAllClicked);
        }

        private void OnDisable()
        {
            _viewAllButton.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            var listModel = ContextData.ToCrewListModel();
            _membersList.Initialize(listModel);
        }

        private void OnViewAllClicked()
        {
            var membersCount = ContextData.Members?.Length ?? 0;
            var blockedMembers = ContextData.MembersCount - membersCount;
            var config = new ViewAllCrewMembersPopupConfiguration(ContextData.CrewId, _localUser.GroupId, membersCount, blockedMembers);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _membersList.CleanUp();
        }
    }
}