using System.Collections.Generic;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Crews.Popups;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class ViewAllCrewMembersPopup : BasePopup<ViewAllCrewMembersPopupConfiguration>
    {
        [SerializeField] private ViewAllCrewMembersList _membersList;

        [Space] 
        [SerializeField] private List<Button> _closeButtons = new List<Button>();
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;

        private void OnEnable()
        {
            if (Configs is null) return;

            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            _animatedBehaviour.PlayInAnimation(null);
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
        }

        protected override void OnConfigure(ViewAllCrewMembersPopupConfiguration configuration)
        {
            var model = new ViewAllCrewMembersListModel(configuration.CrewId, configuration.LocalUserGroupId, configuration.MembersCount, Configs.BlockedMembers);
            _membersList.Initialize(model);
        }

        private void OnCloseButtonClicked()
        {
            _animatedBehaviour.PlayOutAnimation(OnOutAnimationCompleted);

            void OnOutAnimationCompleted()
            {
                Hide();
            }
        }
    }
}