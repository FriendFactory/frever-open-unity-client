using System.Linq;
using Bridge;
using Common;
using Extensions;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    public class OpenFriendsPageButton : ButtonBase
    {
        [SerializeField] private int _tabIndex;
        [SerializeField] private GameObject _dotIndicator;

        [Inject] private LocalUserDataHolder _userData;
        [Inject] private FollowersManager _followersManager;
        [Inject] private IInvitationBridge _bridge;

        protected override async void OnEnable()
        {
            base.OnEnable();
            var result = await _bridge.GetInvitationCode();
            if (this.IsDestroyed()) return;
           
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            if(_userData.IsStarCreator) return;
            
            var model = result.Model;
            var hasAvailableRewards = model.InviteGroups != null && model.InviteGroups.Any();
            _dotIndicator.SetActive(PlayerPrefs.GetInt(Constants.DotIndicators.Invite, 0) == 0 || hasAvailableRewards);
        }
        
        protected override void OnClick()
        {
            Manager.MoveNext(PageId.FollowersPage, new UserFollowersPageArgs(_followersManager, _tabIndex) {IsLocalUser = true});
        }
    }
}
