using System;
using Abstract;
using DG.Tweening;
using Extensions;
using I2.Loc;
using TMPro;
using TweenExtensions;
using UIManaging.Common;
using UIManaging.Localization;
using UIManaging.Pages.CreatorScore;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CreatorBadgeRewardItemView : BaseContextDataView<CreatorBadgeRewardModel>
{
    private const float ANIMATION_DURATION = 0.45f;
    private const float ANIM_TRANSLATE_Y = 100f;
    
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _rewardAmountText;
    [SerializeField] private ThumbnailLoader _rewardAssetIcon;
    [SerializeField] private Image _maskImage;
    [SerializeField] private Image _badgeImage;
    [SerializeField] private Image _rewardImage;
    [SerializeField] private Image _rewardBackgroundImage;
    [SerializeField] private Button _claimButton;
    [SerializeField] private Image _claimButtonImage;
    [SerializeField] private Image _checkMarkImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private Sprite _softCurrencyIcon;
    [SerializeField] private Sprite _hardCurrencyIcon;

    [SerializeField] private Sprite _activeClaimButtonIcon;
    [SerializeField] private Sprite _inactiveClaimButtonIcon;
    
    [SerializeField] private Sprite _softRewardBackgroundSprite;
    [SerializeField] private Sprite _hardRewardBackgroundSprite;
    [SerializeField] private Sprite _assetRewardBackgroundSprite;

    [Inject] private CreatorScorePageLocalization _localization;
    [Inject] private CreatorScoreHelper _creatorScoreHelper;
    
    private Sequence _showSequence;
    private Sequence _hideSequence;

    //---------------------------------------------------------------------
    // Messages
    //---------------------------------------------------------------------
    
    private void Awake()
    {
        _claimButton.onClick.AddListener(OnClaimButton);
        SetupAnimations();
    }

    //---------------------------------------------------------------------
    // Public
    //---------------------------------------------------------------------
    
    public void SetClaimButtonState(bool isActive)
    {
        _claimButtonImage.sprite = isActive ? _activeClaimButtonIcon : _inactiveClaimButtonIcon;
        _claimButton.enabled = isActive;
    }

    public void PlayShowAnimation()
    {
        if (IsDestroyed || !gameObject.activeInHierarchy) return;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 0;
        _rectTransform.anchoredPosition = new Vector2(0, - ANIM_TRANSLATE_Y);
        _rectTransform.SetAsLastSibling();
        _showSequence.Restart();
    }
    
    public void PlayHideAnimation()
    {
        if (IsDestroyed || !gameObject.activeInHierarchy) return;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 1;
        _rectTransform.anchoredPosition = Vector2.zero;
        _hideSequence.Restart();
    }

    protected override void OnInitialized()
    {
        if (IsDestroyed) return;
        
        gameObject.SetActive(true);
        _canvasGroup.interactable = true;
        _canvasGroup.alpha = 1f;
        _rewardImage.gameObject.SetActive(false);
        _rewardAssetIcon.gameObject.SetActive(false);
        _rewardAmountText.SetActive(false);
        _badgeImage.SetActive(false);
        _maskImage.enabled = true;
        _headerText.text = ContextData.IsMilestone 
            ? _localization.GetRankNameLocalized(_creatorScoreHelper.GetBadgeRank(ContextData.Badge.Level) - 1)
            : _localization.BonusRewardTitle;
        _descriptionText.text = string.Format(_localization.RewardUnlockRequirementFormat, ContextData.Badge.CreatorScoreRequired);
        _checkMarkImage.gameObject.SetActive(ContextData.CanBeClaimed);
        SetClaimButtonState(ContextData.CanBeClaimed);
        
        if (ContextData.IsMilestone)
        {
            _badgeImage.gameObject.SetActive(true);
            _badgeImage.sprite = ContextData.BadgeSprite;
            _maskImage.enabled = false;
            return;
        }

        if (ContextData.Badge.Rewards.IsNullOrEmpty()) return;
        
        var reward = ContextData.Badge.Rewards[0];

        if (reward.SoftCurrency.HasValue)
        {
            _rewardImage.sprite = _softCurrencyIcon;
            _rewardAmountText.text = reward.SoftCurrency.Value.ToString();
            _rewardImage.gameObject.SetActive(true);
            _rewardAmountText.SetActive(true);
            _rewardBackgroundImage.sprite = _softRewardBackgroundSprite;
            return;
        }
        
        if (reward.HardCurrency.HasValue)
        {
            _rewardImage.sprite = _hardCurrencyIcon;
            _rewardAmountText.text = reward.HardCurrency.Value.ToString();
            _rewardImage.gameObject.SetActive(true);
            _rewardAmountText.SetActive(true);
            _rewardBackgroundImage.sprite = _hardRewardBackgroundSprite;
            return;
        }
        
        _rewardAssetIcon.gameObject.SetActive(true);
        _rewardBackgroundImage.sprite = _assetRewardBackgroundSprite;
        _rewardAssetIcon.Initialize(reward);
    }
    
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    
    private void OnClaimButton()
    {
        ContextData.ClaimReward?.Invoke(this, ContextData.Badge);
        SetClaimButtonState(false);
    }
    
    private void SetupAnimations()
    {
        _showSequence = DOTween.Sequence()
                               .Join(_rectTransform.DOAnchorPosY(0, ANIMATION_DURATION))
                               .Join(_canvasGroup.DOFade(1f, ANIMATION_DURATION))
                               .SetAutoKill(false)
                               .Pause();
        
        _hideSequence = DOTween.Sequence()
                               .Join(_rectTransform.DOAnchorPosY(ANIM_TRANSLATE_Y, ANIMATION_DURATION))
                               .Join(_canvasGroup.DOFade(0f, ANIMATION_DURATION))
                               .SetAutoKill(false)
                               .Pause();
    }


    public void Hide()
    {
        _canvasGroup.alpha = 0;
    }
}
