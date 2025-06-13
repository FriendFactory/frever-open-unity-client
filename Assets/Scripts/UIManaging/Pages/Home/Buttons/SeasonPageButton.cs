using Extensions;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.SeasonPage;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Navigation.Args.SeasonPageArgs;

namespace UIManaging.Pages.Home
{
    public class SeasonPageButton : ButtonBase
    {
        [SerializeField] private Tab _startingTab;
        [Space]
        [SerializeField] private Image _notificationBadgeMask;
        [SerializeField] private GameObject _notificationBadge;

        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private SeasonRewardsHelper _seasonRewardsHelper;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            Interactable = _dataFetcher.CurrentSeason != null;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void RefreshNotificationBadge()
        {
            var availableRewards = await _seasonRewardsHelper.IsClaimableRewardAvailable();

            if (this.IsDestroyed()) return;
            
            var rewardsAvailable = availableRewards != 0;
            _notificationBadgeMask.enabled = rewardsAvailable;
            _notificationBadge.SetActive(rewardsAvailable);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClick()
        {
            var args = new SeasonPageArgs(_startingTab);
            Manager.MoveNext(PageId.SeasonInfo, args);
        }
    }
}