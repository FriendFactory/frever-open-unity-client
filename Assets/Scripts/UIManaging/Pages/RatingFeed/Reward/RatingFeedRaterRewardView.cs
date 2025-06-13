using System;
using System.Threading.Tasks;
using Bridge;
using BrunoMikoski.AnimationSequencer;
using Common.Abstract;
using Models;
using Modules.Animation.Lottie;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Reward
{
    internal sealed class RatingFeedRaterRewardView: BaseContextView<Level>
    {
        [SerializeField] private Button _claimRewardButton;
        [SerializeField] private FlyingCoinsAnimationController _flyingCoinsAnimationController;
        [SerializeField] private AnimationSequencerController _fadeInSequencer;
        [SerializeField] private AnimationSequencerController _fadeOutSequencer;
        [SerializeField] private LottieAnimationPlayer _successAnimationPlayer;

        [Inject] private IBridge _bridge;
        
        public event Action Shown;
        public event Action RewardClaimed;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task FadeInAsync(bool showOnStart = true)
        {
            if (showOnStart) Show();
            
            var tcs = new TaskCompletionSource<bool>();
            _fadeInSequencer.Play(() => { tcs.SetResult(true); });
            await tcs.Task;

            _successAnimationPlayer.Play();

            Shown?.Invoke();
        }

        public async Task FadeOutAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            _fadeOutSequencer.Play(() => { tcs.SetResult(true); });
            await tcs.Task;
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _flyingCoinsAnimationController.Initialize(new RewardModel(RatingFeedConstants.RATER_REWARD_SOFT_CURRENCY));
            _claimRewardButton.onClick.AddListener(OnClaimRequested);
        }
        
        protected override void BeforeCleanUp()
        {
            _flyingCoinsAnimationController.CleanUp();
            _claimRewardButton.onClick.RemoveListener(OnClaimRequested);
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private async void OnClaimRequested()
        {
            try
            {
                _claimRewardButton.interactable = false;

                await ClaimRewardAsync(); // the result is ignored for now
                
                _claimRewardButton.interactable = true;

                // prevent multiple clicks while the animation is playing with active button state
                _claimRewardButton.onClick.RemoveListener(OnClaimRequested);
                
                await _flyingCoinsAnimationController.PlayAnimationAsync();
                
                RewardClaimed?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task<bool> ClaimRewardAsync()
        {
            var result = await _bridge.ClaimVideoRaterReward(ContextData.Id);
            
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return false;
            }

            return true;
        }
    }
}