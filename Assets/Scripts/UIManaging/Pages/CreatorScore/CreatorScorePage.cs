using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;
using Bridge;
using Bridge.Models.ClientServer.CreatorScore;
using Bridge.Services.UserProfile;
using Common.UserBalance;
using DG.Tweening;
using Extensions;
using Modules.AssetsStoraging.Core;
using TipsManagment;
using UIManaging.Common.RankBadge;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.CreatorScore;
using UIManaging.Pages.Tasks;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

public class CreatorScorePage : GenericPage<CreatorScorePageArgs>
{
    private const float FADE_IN_TIME = 0.25f;
    private const int CREATOR_LEVEL_REQUIRED_TO_RATE = 14;
    
    [SerializeField] private CanvasGroup _rootCanvasGroup;
    [SerializeField] private UserPortraitView _userPortraitView;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private TMP_Text _creatorScoreText;
    [SerializeField] private TMP_Text _followersText;
    [SerializeField] private TMP_Text _likesText;
    [SerializeField] private TMP_Text _videosCountText;
    [SerializeField] private RankBadgeItemView[] _badgeItemViews;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _showInfoButton;
    [SerializeField] private Button _hideInfoButton;
    [SerializeField] private CanvasGroup _infoScreen;
    [SerializeField] private RewardFlowManager _rewardFlowManager;
    [SerializeField] private UserBalanceView _userBalanceView;

    [SerializeField] private CreatorBadgeRewardItemView _activeRewardItem;
    [SerializeField] private CreatorBadgeRewardItemView _swappedRewardItem;

    [SerializeField] private RankDescriptionItem[] _rankDescriptionItems;

    [SerializeField] private Sprite _lockedBadgeSprite;
    [SerializeField] private Sprite[] _badgeBackgroundSprites;

    [Inject] private IBridge _bridge;
    [Inject] private IDataFetcher _dataFetcher;
    [Inject] private PageManager _pageManager;
    [Inject] private PopupManager _popupManager;
    [Inject] private PopupManagerHelper _popupManagerHelper;
    [Inject] private CreatorScoreHelper _creatorScoreHelper;
    [Inject] private LocalUserDataHolder _dataHolder;
    [Inject] private TipManager _tipManager;
    [Inject] private CreatorScorePageLocalization _localization;
    [Inject] private RankBadgeManager _rankBadgeManager; 

    private MyProfile _profile;
    private List<CreatorBadge> _rankUpRewards;

    private int _userRank = -1;
    private bool _claimedRankUp;

    public override PageId Id => PageId.CreatorScore;

    //---------------------------------------------------------------------
    // Messages
    //---------------------------------------------------------------------

    private void Awake()
    {
        _showInfoButton.onClick.AddListener(ShowInfoScreen);
        _hideInfoButton.onClick.AddListener(HideInfoScreen);
        _backButton.onClick.AddListener(OnBackButtonClicked);
    }

    //---------------------------------------------------------------------
    // Public
    //---------------------------------------------------------------------

    //---------------------------------------------------------------------
    // Protected
    //---------------------------------------------------------------------

    protected override void OnInit(PageManager pageManager)
    {
        _rankUpRewards = GetRankUpRewards();
    }

    protected override async void OnDisplayStart(CreatorScorePageArgs pageManager)
    {
        base.OnDisplayStart(OpenPageArgs);
        
       _rewardFlowManager.Initialize(_dataHolder.UserBalance, _dataHolder.LevelingProgress.Xp);

        var userBalanceModel = new StaticUserBalanceModel(_dataHolder);
        _userBalanceView.ContextData?.CleanUp();
        _userBalanceView.Initialize(userBalanceModel);

        _userRank = -1;
        _rootCanvasGroup.alpha = 0;
        _activeRewardItem.gameObject.SetActive(false);

        await RefreshRewardsAsync();

        if (IsDestroyed || !gameObject.activeInHierarchy) return;
        
        UpdateUserPortraitView();
        _rootCanvasGroup.DOFade(1f, FADE_IN_TIME);

        if (OpenPageArgs.ShowHintsOnDisplay 
            && GetNextReward()?.CreatorScoreRequired <= _profile.LevelingProgress.CreatorScore)
        {
            _tipManager.ShowTipsById(TipId.QuestCreatorScore);
        }
    }

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------

    private void UpdateProfileStats()
    {
        _creatorScoreText.text = _profile.LevelingProgress.CreatorScore.ToString();
        _followersText.text = _profile.LevelingProgress.TotalFollowers.ToShortenedString();
        _likesText.text = _profile.LevelingProgress.TotalLikes.ToShortenedString();
        _videosCountText.text = _profile.LevelingProgress.TotalVideos.ToShortenedString();
    }

    private void UpdateUserPortraitView()
    {
        if (_profile.MainCharacterId != null)
        {
            _userPortraitView.Initialize(new UserPortraitModel
            {
                Resolution = Resolution._256x256,
                UserGroupId = _bridge.Profile.GroupId
            });
        }

        _usernameText.text = _profile.Nickname;
    }

    private async Task RefreshRewardsAsync()
    {
        _profile = (await _bridge.GetCurrentUserInfo()).Profile;

        if (IsDestroyed || !gameObject.activeInHierarchy) return;

        if (_claimedRankUp)
        {
            ShowFeatureUnlockPopup();
            _popupManagerHelper.ShowRateAppPopup();
        }

        _claimedRankUp = false;
        
        ShowNextReward();
        UpdateProfileStats();

        var userRank = GetCurrentBadgeRank();

        if (userRank == _userRank) return;

        _userRank = userRank;

        _backgroundImage.sprite = _badgeBackgroundSprites[userRank];
        _backgroundImage.color = _backgroundImage.sprite ? Color.white : Color.clear;

        SetupBadgeScroller();
    }

    private void ShowFeatureUnlockPopup()
    {
        if (!_claimedRankUp) return;
        
        if (_dataFetcher.MetadataStartPack.UnlockCrewCreationOnLevel == _profile.LevelingProgress.CreatorScoreBadge)
        {
            var config = new AlertPopupConfiguration
            {
                PopupType = PopupType.CrewCreationUnlocked
            };

            _popupManager.PushPopupToQueue(config);
            return;
        }

        if (_dataFetcher.MetadataStartPack.UnlockVideoToFeedOnLevel == _profile.LevelingProgress.CreatorScoreBadge)
        {
            var config = new AlertPopupConfiguration()
            {
                PopupType = PopupType.VideoToFeedUnlocked
            };

            _popupManager.PushPopupToQueue(config);
        }
    }

    private async void RefreshRewards()
    {
        if (IsDestroyed || !gameObject.activeInHierarchy) return;
        await RefreshRewardsAsync();
    }

    private void SetupBadgeScroller()
    {
        if (_rankUpRewards == null) return;

        var rank = GetCurrentBadgeRank();

        _badgeItemViews[0].Initialize(true, 0, _rankBadgeManager.GetBadgeSprite(0));

        for (var i = 0; i < _rankUpRewards.Count; i++)
        {
            var badge = _rankUpRewards[i];
            var view = _badgeItemViews[i + 1];
            var isUnlocked = rank >= i + 1;
            var badgeSprite = _rankBadgeManager.GetBadgeSprite(i + 1);
            var rankDescriptionItem = _rankDescriptionItems[i];

            view.Initialize(isUnlocked, badge.CreatorScoreRequired, isUnlocked ? badgeSprite : _lockedBadgeSprite);
            rankDescriptionItem.Init(badgeSprite, _localization.GetRankNameLocalized(i), badge.CreatorScoreRequired);
            rankDescriptionItem.SetActive(true);
        }
    }

    private List<CreatorBadge> GetRankUpRewards()
    {
        return _dataFetcher.MetadataStartPack.CreatorBadges?
                           .Where(item => !string.IsNullOrEmpty(item.Milestone))
                           .ToList();
    }

    private int GetCurrentBadgeRank()
    {
        return _creatorScoreHelper.GetBadgeRank(_profile.LevelingProgress.CreatorScoreBadge);
    }

    private static bool IsMilestoneReward(CreatorBadge reward)
    {
        return !string.IsNullOrEmpty(reward.Milestone) && reward.Milestone != "0";
    }

    private async void ClaimReward(CreatorBadgeRewardItemView sender, CreatorBadge creatorBadge)
    {
        if (creatorBadge.Rewards == null) return;

        var claimRewardResults =
            await Task.WhenAll(creatorBadge.Rewards.Select(reward => _bridge.ClaimCreatorScoreReward(reward.Id)));
        
        if (IsDestroyed || !gameObject.activeInHierarchy) return;
        
        if (claimRewardResults.All(result => result.IsSuccess))
        {
            var currentRank = GetCurrentBadgeRank();
            _claimedRankUp = IsMilestoneReward(creatorBadge);

            var nextRank = _claimedRankUp ? currentRank + 1 : currentRank;
            var badgeSprite = _rankBadgeManager.GetBadgeSprite(currentRank);
            var config =
                new CreatorScoreLevelUpPopupConfiguration(currentRank, nextRank, creatorBadge.Level, badgeSprite, creatorBadge.Rewards,
                                                          PlayRewardFlowAnimation);
            _popupManager.PushPopupToQueue(config);
        }
        else
        {
            ShowAlertMessage(claimRewardResults.FirstOrDefault()?.ErrorMessage);
            RefreshRewards();
        }
        
        sender.SetClaimButtonState(false);

        void PlayRewardFlowAnimation(object result)
        {
            if (IsDestroyed || !gameObject.activeInHierarchy) return;
            
            RefreshRewards();

            var fullSoftCurrency = 0;
            var fullHardCurrency = 0;

            foreach (var reward in claimRewardResults)
            {
                fullSoftCurrency += reward.SoftCurrency ?? 0;
                fullHardCurrency += reward.HardCurrency ?? 0;
            }

            if (fullSoftCurrency > 0 || fullHardCurrency > 0)
            {
                _rewardFlowManager.StartAnimation(0, fullSoftCurrency,  fullHardCurrency);
            }
        }
    }

    private void ShowAlertMessage(string errorMessage)
    {
        var popupDescription = string.Empty;

        switch (errorMessage)
        {
            case "RewardNotAvailable":
            {
                popupDescription = _localization.RewardNotAvailableMessage;
                break;
            }
        }

        var config = new AlertPopupConfiguration
        {
            Title = _localization.RewardClaimFailedPopupTitle,
            Description = popupDescription,
            ConfirmButtonText = _localization.RewardClaimFailedCloseButton,
            PopupType = PopupType.AlertPopup
        };

        _popupManager.PushPopupToQueue(config);
    }

    private void ShowNextReward()
    {
        (_activeRewardItem, _swappedRewardItem) = (_swappedRewardItem, _activeRewardItem);

        _swappedRewardItem.PlayHideAnimation();
        
        if ( _dataFetcher.MetadataStartPack.CreatorBadges.IsNullOrEmpty()) return;

        var reward = GetNextReward();

        if (reward == default) return;

        var canBeClaimed = _profile.LevelingProgress.CreatorScore >= reward.CreatorScoreRequired;
        var isMilestone = IsMilestoneReward(reward);

        var model = new CreatorBadgeRewardModel
        {
            Badge = reward,
            CanBeClaimed = canBeClaimed,
            ClaimReward = ClaimReward,
            IsMilestone = isMilestone
        };

        if (isMilestone)
        {
            var nextRank = _creatorScoreHelper.GetBadgeRank(reward.Level);
            model.BadgeSprite = _rankBadgeManager.GetBadgeSprite(nextRank);
        }

        _activeRewardItem.Initialize(model);
        _activeRewardItem.PlayShowAnimation();
    }

    private void ShowInfoScreen()
    {
        _infoScreen.blocksRaycasts = true;
        _infoScreen.interactable = true;
        _infoScreen.DOKill();
        _infoScreen.DOFade(1, 0.1f).SetEase(Ease.OutQuad);
    }

    private void HideInfoScreen()
    {
        _infoScreen.blocksRaycasts = false;
        _infoScreen.interactable = false;
        _infoScreen.DOKill();
        _infoScreen.DOFade(0, 0.1f).SetEase(Ease.InQuad);
    }

    private void OnBackButtonClicked()
    {
        _pageManager.MoveBack();
    }

    private CreatorBadge GetNextReward()
    {
        return _dataFetcher.MetadataStartPack.CreatorBadges.FirstOrDefault(
                item => item.Level > _profile.LevelingProgress.CreatorScoreBadge
                     && !item.Rewards.IsNullOrEmpty());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _infoScreen.DOKill();
    }
}