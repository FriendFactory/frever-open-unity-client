using System.Collections.Generic;
using Bridge.Models.ClientServer.CreatorScore;
using I2.Loc;
using TMPro;
using UIManaging.Animated;
using UIManaging.Localization;
using UIManaging.Pages.SeasonPage;
using UIManaging.Pages.Tasks.RewardPopUp;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks.TaskRewardFlow
{
    public class CreatorScoreLevelUpPopup : BasePopup<CreatorScoreLevelUpPopupConfiguration>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private AnimatedCreatorScoreLevel _levelAnimation;
        [SerializeField] private Image _badgeImage;
        
        [SerializeField] private TaskCompletedPopupRewardElement _softCurrencyReward;
        [SerializeField] private TaskCompletedPopupRewardElement _hardCurrencyReward;
        [SerializeField] private AssetRewardItem _assetReward;

        [Inject] private CreatorScorePageLocalization _localization;
        
        private void Awake()
        {
            _closeButton.onClick.AddListener(ClosePopup);
        }

        private void OnEnable()
        {
            if (Configs != null && Configs.NextMilestone > Configs.Milestone) _levelAnimation.Animate(Configs.Milestone, Configs.NextMilestone);
        }

        protected override void OnConfigure(CreatorScoreLevelUpPopupConfiguration configuration)
        {
            PopulateItems(configuration.Rewards);

            if (configuration.NextMilestone > configuration.Milestone)
            {
                _headerText.text = _localization.LevelUpHeader;
            }
            else
            {
                _headerText.text = _localization.RewardClaimedHeader;
                _badgeImage.sprite = configuration.BadgeSprite;
                _badgeImage.color = configuration.BadgeSprite != null
                    ? Color.white
                    : Color.clear;
            }
        }

        private void ClosePopup()
        {
            Hide(null);
        }

        private void PopulateItems(IEnumerable<CreatorBadgeReward> rewards)
        {
            _softCurrencyReward.gameObject.SetActive(false);
            _hardCurrencyReward.gameObject.SetActive(false);
            _assetReward.gameObject.SetActive(false);

            var softRewards = 0;
            var hardRewards = 0;
            
            foreach (var reward in rewards)
            {
                softRewards += reward.SoftCurrency ?? 0;
                hardRewards += reward.HardCurrency ?? 0;

                if (reward.Asset != null)
                {
                    _assetReward.gameObject.SetActive(true);
                    _assetReward.Initialize(reward);
                }
            }
            
            if(softRewards > 0)
            {
                _softCurrencyReward.gameObject.SetActive(true);
                _softCurrencyReward.Show(softRewards.ToString());
            }
            
            if (hardRewards > 0)
            {
                _hardCurrencyReward.gameObject.SetActive(true);
                _hardCurrencyReward.Show(hardRewards.ToString());
            }
        }
    }
}