using System.Threading;
using Bridge;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.FollowersPage.UI.Search;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.VotingResult
{
    internal sealed class FollowAccountPopup: BasePopup<FollowAccountConfiguration>
    {
        [SerializeField] private Button _seeProfileButton;
        [SerializeField] private SearchUserFollowItemView _profileView;

        [Inject] private PageManager _pageManager;
        
        protected override void OnConfigure(FollowAccountConfiguration configuration)
        {
            _seeProfileButton.onClick.AddListener(OnProfileButtonClicked);
            _profileView.ProfileButtonClicked += Hide;
            
            _profileView.Initialize(configuration.Profile);
        }

        public override void Hide(object result)
        {
            _profileView.CleanUp();
            
            _profileView.ProfileButtonClicked -= Hide;
            _seeProfileButton.onClick.RemoveListener(OnProfileButtonClicked);
            
            base.Hide(result);
        }
        
        private void OnProfileButtonClicked()
        {
            var pageArgs = new UserProfileArgs(Configs.Profile.MainGroupId, null);
            _pageManager.MoveNext(pageArgs);
            Hide();
        }

    }
}