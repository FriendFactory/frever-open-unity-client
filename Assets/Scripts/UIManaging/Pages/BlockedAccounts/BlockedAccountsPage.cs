using System;
using System.Linq;
using Bridge.Services.UserProfile;
using I2.Loc;
using Modules.VideoStreaming.UIAnimators;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.BlockedAccounts
{
    internal sealed class BlockedAccountsPage : GenericPage<BlockedAccountsPageArgs>
    {
        public override PageId Id => PageId.BlockedAccountsPage;

        [Inject] private IBlockedAccountsManager _blockUserService;
        [Inject] private PageManager _pageManager;
        [SerializeField] private PageUiAnimator _pageUiAnimator;
        [SerializeField] private BlockedAccountListView _blockedAccountsView;
        [SerializeField] private PageHeaderView _pageHeaderView;
        [Header("Localization")]
        [SerializeField] private LocalizedString _pageHeader;
        
        protected override void OnInit(PageManager pageManager)
        {
            var headerButtonArgs = new ButtonArgs(string.Empty, _pageManager.MoveBack);
            var pageHeaderArgs = new PageHeaderArgs(_pageHeader, headerButtonArgs);
            _pageHeaderView.Init(pageHeaderArgs);
        }

        protected override void OnDisplayStart(BlockedAccountsPageArgs args)
        {
            DisplayBlockedAccountsAsync();
            _pageUiAnimator.PrepareForDisplay();
            _pageUiAnimator.PlayShowAnimation(() => base.OnDisplayStart(args));
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _pageUiAnimator.PlayHideAnimation(onComplete);
        }

        private void DisplayBlockedAccountsAsync()
        {
            var blockedAccounts = _blockUserService.BlockedAccounts;
            DisplayBlockedAccounts(blockedAccounts.ToArray());
        }

        private void DisplayBlockedAccounts(Profile[] blockedProfiles)
        {
            _blockedAccountsView.Display(blockedProfiles);
        }
    }
}