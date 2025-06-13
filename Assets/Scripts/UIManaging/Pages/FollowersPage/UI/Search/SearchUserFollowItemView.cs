using System.Threading;
using Bridge;
using Bridge.Services.UserProfile;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.FollowersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI.Search
{
    public class SearchUserFollowItemView : SearchUserBaseItemView
    {
        [SerializeField] private FollowUserButton _followUserButton;

        [Inject] private IBridge _bridge;
        [Inject] private FollowersManager _followersManager;
        [Inject] private PageManager _pageManager;

        private CancellationTokenSource _cancellationTokenSource;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _followUserButton.Initialize(new FollowUserButtonArgs(ContextData));
            _followersManager.Followed += OnUserFollowedOrUnFollowed;
            _followersManager.UnFollowed += OnUserFollowedOrUnFollowed;
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _followersManager.Followed -= OnUserFollowedOrUnFollowed;
            _followersManager.UnFollowed -= OnUserFollowedOrUnFollowed;
            _cancellationTokenSource?.Cancel();
        }
        
        protected override void OnProfileButtonClicked()
        {
            if (IsLocalUser())
            {
                GoToLocalUserProfile();
            }
            else
            {
                GoToRemoteUserProfile();
            }
            
            base.OnProfileButtonClicked();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnUserFollowedOrUnFollowed(Profile profile)
        {
            if (profile.MainGroupId != ContextData.MainGroupId) return;
            
            ContextData.KPI.FollowersCount = profile.KPI.FollowersCount;
            RefreshFollowersAmountText();
        }

        private void GoToLocalUserProfile()
        {
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs());
        }

        private void GoToRemoteUserProfile()
        {
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(ContextData.MainGroupId, ContextData.NickName));
        }
        
        private bool IsLocalUser()
        {
            return _bridge.Profile.GroupId == ContextData.MainGroupId;
        }
    }
}