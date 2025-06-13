using System.Linq;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.CreatorScore;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Extensions;
using UIManaging.Common.RankBadge;

public class OpenCreatorScoreButton : ButtonBase
{
    [SerializeField] private Image _badgeImage;
    [SerializeField] private Image _notificationBadgeMask;
    [SerializeField] private GameObject _notificationBadge;
    
    [Inject] private LocalUserDataHolder _userDataHolder;
    [Inject] private IDataFetcher _dataFetcher;
    [Inject] private CreatorScoreHelper _creatorScoreHelper;
    [Inject] private RankBadgeManager _rankBadgeManager; 

    public void UpdateSprite()
    {
        var badgeIndex = _creatorScoreHelper.GetBadgeRank(_userDataHolder.UserProfile.CreatorScoreBadge);
        _badgeImage.sprite = _rankBadgeManager.GetBadgeSprite(badgeIndex, RankBadgeType.Homepage);
    }

    public void RefreshNotificationBadge()
    {
        var rank = _creatorScoreHelper.GetBadgeRank(_userDataHolder.LevelingProgress.CreatorScoreBadge);
        _badgeImage.sprite = _rankBadgeManager.GetBadgeSprite(rank, RankBadgeType.Homepage);

        var reward =
            _dataFetcher.MetadataStartPack.CreatorBadges?.FirstOrDefault(
                item => item.Level > _userDataHolder.LevelingProgress.CreatorScoreBadge);

        var canBeClaimed = reward != default && _userDataHolder.LevelingProgress.CreatorScore >= reward.CreatorScoreRequired;

        if (this.IsDestroyed()) return;
        _notificationBadgeMask.enabled = canBeClaimed;
        _notificationBadge.SetActive(canBeClaimed);
    }

    //---------------------------------------------------------------------
    // Protected
    //---------------------------------------------------------------------

    protected override void OnClick()
    {
        var args = new CreatorScorePageArgs { ShowHintsOnDisplay = false };
        Manager.MoveNext(PageId.CreatorScore, args);
    }
}