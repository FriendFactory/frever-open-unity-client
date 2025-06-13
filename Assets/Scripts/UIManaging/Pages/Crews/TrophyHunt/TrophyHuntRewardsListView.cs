using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using Modules.Crew;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Rewards;
using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.Crews.TrophyHunt.Models;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.TrophyHunt
{
    public class TrophyHuntRewardsListView : BaseEnhancedScrollerView<TrophyHuntRewardListItem, CrewRewardWrapper>
    {
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private TrophyHuntRewardsProgressBar _progressBar;
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animation;
        [SerializeField] private RawImage _backgroundImage;

        [Inject] private CrewService _crewService;

        public int TrophyScore { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
        }
        
        private void OnEnable()
        {
            _animation.PlayInAnimation(null);
            _backgroundImage.texture = _crewService.GetTrophyHuntRewardsBackground();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _scroller.cellViewVisibilityChanged += SetupProgressBar;
        }

        private async void SetupProgressBar(EnhancedScrollerCellView cellView)
        {
            _progressBar.transform.SetParent(cellView.transform.parent, false);
            _progressBar.transform.SetAsLastSibling();
            _scroller.AppendedObjectsCount = 1;
            _progressBar.Init(ContextData.Items.Select(reward => reward.Reward.RequiredTrophyScore).ToArray(), 
                              (int)GetCellViewSize(_scroller, 0), 
                              TrophyScore);
            _scroller.cellViewVisibilityChanged -= SetupProgressBar;

            await Task.Delay(500);
            
            JumpToLastUnclaimed();
        }

        private void JumpToLastUnclaimed()
        {
            var firstAvailableRewardIndex = ContextData.Items.ToList()
                                                       .FindIndex(reward => reward.Reward.RequiredTrophyScore <= TrophyScore
                                                                         && reward.State == RewardState.Available);

            firstAvailableRewardIndex--;

            if (firstAvailableRewardIndex < 0)
            {
                firstAvailableRewardIndex = ContextData.Items.ToList()
                                                       .FindIndex(reward => reward.Reward.RequiredTrophyScore > TrophyScore);
                firstAvailableRewardIndex -= 2;
            }

            if (firstAvailableRewardIndex <= 0) return;

            var tweenTime = Mathf.Max(0.2f, 0.1f * firstAvailableRewardIndex);

            _scroller.JumpToDataIndex(firstAvailableRewardIndex, tweenTime: tweenTime,
                                      tweenType: EnhancedScroller.TweenType.easeOutQuad);
        }

        private void OnCloseButtonClicked()
        {
            _animation.PlayOutAnimation(OnAnimationDone);

            void OnAnimationDone()
            {
                gameObject.SetActive(false);
            }
        }
    }
}