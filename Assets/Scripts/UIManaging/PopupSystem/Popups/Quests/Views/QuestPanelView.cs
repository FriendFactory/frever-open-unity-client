using Abstract;
using Bridge.Models.ClientServer.Onboarding;
using DG.Tweening;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Quests.Views
{
    public class QuestPanelView : BaseContextDataView<IQuestPanelModel>
    {
        private const float ANIMATION_DELAY = 0.5f;
        private const float ANIMATION_DURATION = 0.5f;
        
        [SerializeField] private QuestGroupView _mainQuestGroup;
        [SerializeField] private QuestGroupView _animatedQuestGroup;
        [SerializeField] private QuestRewardView _mainQuestReward;
        [SerializeField] private QuestRewardView _animatedQuestReward;
        [SerializeField] private CanvasGroup _mainQuestCanvasGroup;
        [SerializeField] private CanvasGroup _animatedQuestCanvasGroup;
        [SerializeField] private RectTransform _mainQuestRectTransform;
        [SerializeField] private RectTransform _animatedQuestRectTransform;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _counterText;
        
        protected override void OnInitialized()
        {
            UpdatePanel();

            ContextData.QuestsUpdated += UpdatePanel;
            ContextData.RewardClaimed += AnimatePanel;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            ContextData.QuestsUpdated -= UpdatePanel;
            ContextData.RewardClaimed -= AnimatePanel;
            
            _mainQuestGroup.CleanUp();
            _animatedQuestGroup.CleanUp();
            _mainQuestReward.CleanUp();
            _animatedQuestReward.CleanUp();
        }

        private void UpdatePanel()
        {
            _mainQuestGroup.Initialize(ContextData.QuestGroupModel);
            _mainQuestReward.Initialize(ContextData.QuestRewardModel);
            
            _animatedQuestGroup.CleanUp();
            _animatedQuestReward.CleanUp();
            _animatedQuestRectTransform.SetActive(false);
            
            _titleText.text = ContextData.CurrentTitle;
            _counterText.text = $"{ContextData.CurrentQuestGroupNumber}/{ContextData.TotalQuestGroupNumber}";
        }

        private void AnimatePanel(OnboardingReward reward)
        {
            if (ContextData.CurrentQuestGroupNumber > ContextData.TotalQuestGroupNumber)
            {
                return;
            }
            
            _animatedQuestRectTransform.SetActive(true);
            _animatedQuestGroup.Initialize(ContextData.QuestGroupModel);
            _animatedQuestReward.Initialize(ContextData.QuestRewardModel);
            
            var mainStartAnchorPos = _mainQuestRectTransform.anchoredPosition;
            var animatedStartAnchorPos = _animatedQuestRectTransform.anchoredPosition;

            DOTween.Sequence()
                   .AppendInterval(ANIMATION_DELAY)
                   .Append(_mainQuestCanvasGroup.DOFade(0, ANIMATION_DURATION)
                                                .SetEase(Ease.InOutCubic))
                   .Join(_mainQuestRectTransform.DOAnchorPosX(mainStartAnchorPos.x - animatedStartAnchorPos.x, ANIMATION_DURATION)
                                                .SetEase(Ease.InOutCubic)
                                                .SetRelative())
                   .Join(_animatedQuestCanvasGroup.DOFade(1, ANIMATION_DURATION)
                                                  .SetEase(Ease.InOutCubic))
                   .Join(_animatedQuestRectTransform
                        .DOAnchorPosX(mainStartAnchorPos.x, ANIMATION_DURATION)
                        .SetEase(Ease.InOutCubic))
                   .AppendCallback(() =>
                    {
                        _mainQuestCanvasGroup.alpha = 1;
                        _animatedQuestCanvasGroup.alpha = 0;
                        
                        _mainQuestRectTransform.anchoredPosition = mainStartAnchorPos;
                        _animatedQuestRectTransform.anchoredPosition = animatedStartAnchorPos;
                        
                        UpdatePanel();
                    });
        }
    }
}