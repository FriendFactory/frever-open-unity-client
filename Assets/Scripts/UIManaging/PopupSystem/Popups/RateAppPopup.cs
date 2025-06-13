using System.Collections.Generic;
using Common;
using DG.Tweening;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Extensions;
using Modules.Amplitude;

namespace UIManaging.PopupSystem.Popups
{
    public class RateAppPopup : BasePopup<RateAppPopupConfiguration>
    {
        private const int MAX_RATING = 5;

        [SerializeField] private Button[] _starButtons;

        [SerializeField] private Color _enabledColor;
        [SerializeField] private Color _disabledColor;

        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _notNowButton;
        
        [SerializeField] private float _animationDuration = 0.2f;

        [Inject] private PopupManager _popupManager;
        [Inject] private AmplitudeManager _amplitudeManager;

        private int _rating;

        private Sequence _sequence;

        private void Awake()
        {
            for (var i = 0; i < MAX_RATING; i++)
            {
                var rating = i + 1;
                _starButtons[i].onClick.AddListener(() => SetRating(rating));
            }

            _submitButton.onClick.AddListener(SubmitRating);
            _notNowButton.onClick.AddListener(Hide);
        }

        private void SubmitRating()
        {
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.INTERNAL_RATING,
                new Dictionary<string, object> { [AmplitudeEventConstants.EventProperties.RATING] = _rating});
            
            if (_rating == MAX_RATING)
            {
                InAppReviewService.RequestReview();
            }
            else
            {
                _popupManager.PushPopupToQueue(new DialogDarkPopupConfiguration 
                { 
                    PopupType = PopupType.RateAppAccepted,
                    OnYes = OpenFeedbackPage,
                    NoButtonText = "Not now"
                });
            }

            Hide();
        }

        private void OpenFeedbackPage()
        {
            Application.OpenURL (Constants.Feedback.FEEDBACK_EMAIL);
        }

        protected override void OnConfigure(RateAppPopupConfiguration configuration)
        {
            SetRating(MAX_RATING);
        }

        private void SetRating(int rating)
        {
            _rating = rating;

            _sequence?.Kill(true);
            _sequence = DOTween.Sequence();
            
            for (var i = 0; i < MAX_RATING; i++)
            {
                _starButtons[i].enabled = false;
                _sequence.Join(_starButtons[i].targetGraphic.DOColor(i < _rating ? _enabledColor : _disabledColor, _animationDuration));
            }

            for (var i = 0; i < _rating; i++)
            {
                var starButton = _starButtons[i];
                var tween = starButton
                           .transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1f), _animationDuration, vibrato: 0,
                                                   elasticity: 0);
                _sequence.Join(tween.SetDelay(0.02f * i));
            }

            _sequence.OnComplete(() => _starButtons.ForEach(button =>
            {
                button.enabled = true;
                button.transform.localScale = Vector3.one;
            }));
        }

    }
    
    public class RateAppPopupConfiguration : PopupConfiguration
    {
        public RateAppPopupConfiguration() : base(PopupType.RateApp, null)
        {

        }
    }
}