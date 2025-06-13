using System;
using System.Threading.Tasks;
using BrunoMikoski.AnimationSequencer;
using Common.Abstract;
using UIManaging.Pages.RatingFeed.Rating;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.RatingFeed
{
    internal sealed class RatingFeedBoardingView : BaseContextView<RatingFeedViewModel>
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private VideoRatingCountdown _countdown;
        [SerializeField] private AnimationSequencerController _animationSequencer;

        private bool _isBoardingDone;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action BoardingDone;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task FadeOutAsync(bool hideOnComplete = true)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            _animationSequencer.Play(() => {
                if (hideOnComplete) Hide();
                
                tcs.SetResult(true);
            });

            await tcs.Task;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _startButton.interactable = false;

            _startButton.onClick.AddListener(OnTimeExpired);
            
            _countdown.Initialize();
            _countdown.StartCountdown();
            
            ContextData.Initialized += OnModelInitialized;

            _countdown.TimeExpired += OnTimeExpired;
        }

        protected override void BeforeCleanUp()
        {
            _startButton.onClick.RemoveListener(OnTimeExpired);
            
            _countdown.CleanUp();
            
            ContextData.Initialized -= OnModelInitialized;
            
            _countdown.TimeExpired -= OnTimeExpired;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnTimeExpired()
        {
            if (_isBoardingDone) return;

            BoardingDone?.Invoke();
            _isBoardingDone = true;
        }

        private void OnModelInitialized() => _startButton.interactable = true;
    }
}