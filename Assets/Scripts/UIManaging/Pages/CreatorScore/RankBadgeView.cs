using Abstract;
using Common;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.CreatorScore;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Args.Views.Profile
{
    public class RankBadgeView : BaseContextDataView<int>
    {
        [SerializeField] private Image _badgeImage;

        [Inject] private CreatorScoreHelper _creatorScoreHelper;
        [Inject] private RankBadgeManager _rankBadgeManager; 

        protected override void OnInitialized()
        {
            var userRank = _creatorScoreHelper.GetBadgeRank(ContextData);
            var displayBadge = userRank >= Constants.CreatorScore.DISPLAY_BADGE_FROM_LEVEL;

            _badgeImage.sprite = _rankBadgeManager.GetBadgeSprite(userRank);
            _badgeImage.color = displayBadge ? Color.white : Color.clear;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _badgeImage.sprite = null;
            _badgeImage.color = Color.clear;
        }
    }
}