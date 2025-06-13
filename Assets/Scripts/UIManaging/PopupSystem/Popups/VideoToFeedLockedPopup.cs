using I2.Loc;
using Modules.AssetsStoraging.Core;
using TMPro;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.CreatorScore;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    public class VideoToFeedLockedPopup : AlertPopup
    {
        [SerializeField] private Image _badgeImage;
        [SerializeField] private TMP_Text _scoreRequiredText;
        [SerializeField] private LocalizedString _descriptionTextFormat;
        
        [Inject] private CreatorScoreHelper _creatorScoreHelper;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private RankBadgeManager _rankBadgeManager; 

        protected override void OnConfigure(AlertPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);

            var badgeIndex = _creatorScoreHelper.GetBadgeRank(_dataFetcher.MetadataStartPack.UnlockVideoToFeedOnLevel);
            _badgeImage.sprite =
                _rankBadgeManager.GetBadgeSprite(badgeIndex, RankBadgeType.Normal);

            var unlockScore = _dataFetcher.MetadataStartPack
                                          .CreatorBadges[_dataFetcher.MetadataStartPack.UnlockVideoToFeedOnLevel]
                                          .CreatorScoreRequired;
            
            _scoreRequiredText.text = unlockScore.ToString();
            _descriptionText.text = string.Format(_descriptionTextFormat, unlockScore);
        }
    }
}