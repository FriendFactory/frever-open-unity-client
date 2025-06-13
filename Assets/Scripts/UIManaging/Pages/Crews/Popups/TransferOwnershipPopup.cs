using System.Collections.Generic;
using Modules.Crew;
using UIManaging.Animated.Behaviours;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class TransferOwnershipPopup : BasePopup<TransferOwnershipPopupConfiguration>
    {
        
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private Button _doneButton;

        [SerializeField] private TransferOwnershipList _list;

        [Space]
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedSlideInOut;

        [Inject] private CrewService _crewService;
        [Inject] private PopupManager _popupManager;
        [Inject] private CrewPageLocalization _localization;

        private long _selectedUserId;

        private void OnEnable()
        {
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseClicked));
            _doneButton.onClick.AddListener(OnDoneClicked);
            _animatedSlideInOut.PlayInAnimation(null);
            _list.NewLeaderSelected += OnNewLeaderSelected;
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
            _doneButton.onClick.RemoveAllListeners();
            _list.NewLeaderSelected -= OnNewLeaderSelected;
            _selectedUserId = -1;
        }

        protected override void OnConfigure(TransferOwnershipPopupConfiguration configuration)
        {
            var model = new TransferOwnershipListModel(configuration.Members, configuration.LocalGroupId, _localization);
            _list.Initialize(model);
            _doneButton.interactable = false;
            _selectedUserId = -1;
        }

        private void OnNewLeaderSelected(long groupId)
        {
            _selectedUserId = groupId;
            _doneButton.interactable = _selectedUserId >= 0;
        }
        
        private void OnCloseClicked()
        {
            _animatedSlideInOut.PlayOutAnimation(Hide);
            _selectedUserId = -1;
        }

        private void OnDoneClicked()
        {
            if (_selectedUserId < 0) return;

            _crewService.TransferOwnership(_selectedUserId, Hide);
            _popupManager.ClosePopupByType(PopupType.ManageCrewMember);
        }
    }
}