using System;
using BrunoMikoski.AnimationSequencer;
using Common.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.RatingFeed.Rating
{
    [RequireComponent(typeof(Toggle))]
    internal sealed class VideoRatingToggle : BaseContextPanel<int>
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private AnimationSequencerController _animationSequencer;
        [SerializeField] private AnimationSequencerController _shakeAnimationSequencer;
        
        private RectTransform _rectTransform;

        protected override bool IsReinitializable => true;

        public event Action<bool, int> ValueChanged;
        
        public bool IsOn
        {
            get => _toggle.isOn;
            set
            {
                _toggle.isOn = value;
                
                PlayAnimation(value);
            }
        }
        
        public RectTransform RectTransform => _rectTransform;
        
        public void PlayShakeAnimation() => _shakeAnimationSequencer.Play();

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_toggle) return;

            _toggle = GetComponent<Toggle>();
        }
        #endif

        private void Awake()
        {
            _rectTransform = _toggle.GetComponent<RectTransform>();
        }

        protected override void OnInitialized()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }
        
        protected override void BeforeCleanUp()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void PlayAnimation(bool isOn)
        {
            if (!isOn)
            {
                _animationSequencer.ResetToInitialState();
            }
            else
            {
                _animationSequencer.PlayForward();
            }
        }

        private void OnValueChanged(bool isOn) => ValueChanged?.Invoke(isOn, ContextData);
    }
}