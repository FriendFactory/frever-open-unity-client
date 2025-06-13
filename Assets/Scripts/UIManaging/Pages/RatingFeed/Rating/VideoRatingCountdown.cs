using System;
using Common.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class VideoRatingCountdown : BaseContextlessPanel
    {
        private CountdownTimer _countdownTimer;

        [SerializeField] private float _duration = 10f;
        [SerializeField] private float _almostUpThreshold = 5f;
        [SerializeField] private TMP_Text _countdownText;
        [SerializeField] private Image _progressBar;
        
        private bool _isAlmostUpFired;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action TimeExpired;
        public event Action TimeAlmostExpired;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void StartCountdown()
        {
            _countdownTimer.Start();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _countdownTimer ??= new CountdownTimer(_duration);
            _isAlmostUpFired = false;
            
            _countdownTimer.TimeExpired += OnTimeExpired;
        }
        
        protected override void BeforeCleanUp()
        {
            if (_countdownTimer == null) return;
            
            _countdownTimer.TimeExpired -= OnTimeExpired;
            _countdownTimer.Stop();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Update()
        {
            if (_countdownTimer == null) return;
            if (!_countdownTimer.IsRunning) return;
            
            if (!_isAlmostUpFired && _countdownTimer.CurrentTime <= _almostUpThreshold)
            {
                _isAlmostUpFired = true;
                TimeAlmostExpired?.Invoke();
            }
            
            var progress = _countdownTimer.CurrentTime / _duration;
            _progressBar.fillAmount = Mathf.Clamp01(1f - progress);
            
            var time = Mathf.Clamp(Mathf.Ceil(_countdownTimer.CurrentTime), 0f, _duration);
            _countdownText.text = time.ToString();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnTimeExpired() => TimeExpired?.Invoke();
    }
}