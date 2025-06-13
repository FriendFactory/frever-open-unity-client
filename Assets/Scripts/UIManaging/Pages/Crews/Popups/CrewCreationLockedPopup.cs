using I2.Loc;
using Modules.AssetsStoraging.Core;
using Navigation.Core;
using TMPro;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.CreatorScore;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CrewCreationLockedPopup : BasePopup<AlertPopupConfiguration>
{
    private const string UNLOCK_REQUIREMENT_SCORE_TEXT = "To be able to create you own crew you need to reach {0} Creator score ";

    [SerializeField] private Button _okButton;
    [SerializeField] private Button _backgroundButton;
    [SerializeField] private Image _badgeImage;
    [SerializeField] private TMP_Text _scoreRequiredText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private LocalizedString _descriptionTextFormat;

    [Inject] private CreatorScoreHelper _creatorScoreHelper;
    [Inject] private IDataFetcher _dataFetcher;
    [Inject] private RankBadgeManager _rankBadgeManager; 

    private void Awake()
    {
        _okButton.onClick.AddListener(Hide);
        _backgroundButton.onClick.AddListener(Hide);
    }

    protected override void OnConfigure(AlertPopupConfiguration configuration)
    {
        var unlockScore = _dataFetcher.MetadataStartPack
                                      .CreatorBadges[_dataFetcher.MetadataStartPack.UnlockCrewCreationOnLevel]
                                      .CreatorScoreRequired;
        _descriptionText.text = string.Format(_descriptionTextFormat, unlockScore);
        InitCreatorScoreBadgeView();
    }

    protected void InitCreatorScoreBadgeView()
    {
        var badgeIndex = _creatorScoreHelper.GetBadgeRank(_dataFetcher.MetadataStartPack.UnlockCrewCreationOnLevel);
        _badgeImage.sprite = _rankBadgeManager.GetBadgeSprite(badgeIndex, RankBadgeType.Normal);
        var unlockScore = _dataFetcher.MetadataStartPack
                                      .CreatorBadges[_dataFetcher.MetadataStartPack.UnlockCrewCreationOnLevel]
                                      .CreatorScoreRequired;
        _scoreRequiredText.text = unlockScore.ToString();
    }
}
