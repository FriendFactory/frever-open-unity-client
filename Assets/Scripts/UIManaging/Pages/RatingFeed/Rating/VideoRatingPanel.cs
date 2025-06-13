using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Abstract;
using Extensions;
using Modules.Sound;
using UIManaging.Pages.RatingFeed.Signals;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class VideoRatingPanel : BaseContextPanel<VideoRating>
    {
        [SerializeField] private List<VideoRatingToggle> _ratingToggles;
        [SerializeField] private VideoRatingCountdown _countdown;
        [SerializeField] private VideoRatingCountdown _bigCountdown;
        [SerializeField] private VideoRatingOverlay  _videoRatingOverlay;
        [Header("Animation")] 
        [SerializeField] private float _perAnimationStepDelay = 0.05f;
        [SerializeField] private float _endAnimationDelay = 0.2f;
        [SerializeField] private float _shakeAnimationDelay = 0.2f;
        [Header("SFX")] 
        [SerializeField] private AutoplaySoundTrigger _alarmSoundTrigger;

        [Inject] private SignalBus _signalBus;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void StartRatingCountdown()
        {
            _videoRatingOverlay.Hide();
            _countdown.StartCountdown();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            for (var i = 0; i < _ratingToggles.Count; i++)
            {
                var rating = i + 1;
                var toggle = _ratingToggles[i];
                
                toggle.Initialize(rating);

                toggle.IsOn = false;
                toggle.ValueChanged += OnToggleValueChanged;
            }

            _bigCountdown.SetActive(false);

            _countdown.SetActive(true);
            _countdown.Initialize();
            _countdown.TimeExpired += OnTimeExpired;
            _countdown.TimeAlmostExpired += OnTimeAlmostExpired;
        }

        protected override void BeforeCleanUp()
        {
            ClearToggleSubscriptions();

            _countdown.CleanUp();

            if (_bigCountdown.IsInitialized)
            {
                _bigCountdown.TimeExpired -= OnTimeExpired;
                _bigCountdown.CleanUp();
            }

            _countdown.TimeExpired -= OnTimeExpired;
            _countdown.TimeAlmostExpired -= OnTimeAlmostExpired;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnToggleValueChanged(bool isOn, int rating)
        {
            if (!isOn) return;

            ClearToggleSubscriptions();
            SetScore(rating);
        }

        private async void SetScore(int score)
        {
            try
            {
                await SetSequentiallyWithDelayAsync(score);
                
                ContextData.SetScore(score);  
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
        }

        private async Task SetSequentiallyWithDelayAsync(int score)
        {
            for (var i = 1; i < score; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(_perAnimationStepDelay)); 
                
                _ratingToggles[i].IsOn = true;
            }

            var targetPosition = _ratingToggles[score - 1].RectTransform.position;
            
            _signalBus.Fire(new ScoreAnimationFinishedSignal(score, targetPosition));
            
            await Task.Delay(TimeSpan.FromSeconds(_endAnimationDelay));
        }
        
        private void ClearToggleSubscriptions()
        {
            _ratingToggles.ForEach(button => button.ValueChanged -= OnToggleValueChanged);
        }

        private void OnTimeExpired() => SetScore(RatingFeedConstants.DEFAULT_RATING);
        
        private async void OnTimeAlmostExpired()
        {
            try
            {
                _countdown.TimeExpired -= OnTimeExpired;
                _countdown.TimeAlmostExpired -= OnTimeAlmostExpired;
                _countdown.SetActive(false);

                _bigCountdown.SetActive(true);
                _bigCountdown.Initialize();
                _bigCountdown.StartCountdown();
                _bigCountdown.TimeExpired += OnTimeExpired;

                _videoRatingOverlay.Show();

                _alarmSoundTrigger.PlaySound();

                await Task.Delay(TimeSpan.FromSeconds(_shakeAnimationDelay));

                _ratingToggles.ForEach(toggle => toggle.PlayShakeAnimation());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}