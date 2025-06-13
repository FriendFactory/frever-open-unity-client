using System;
using Common.Abstract;
using UI.UIAnimators;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage.Buttons
{
    public class VideoPostSettingsToggle: BaseContextlessPanel
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private ParallelUiAnimationPlayer _animationPlayer;
        
        private Action<bool> _onToggle;
        private Func<bool> _allowToggle;

        protected override bool IsReinitializable => true;

        public bool IsOn
        {
            get => Toggle.isOn;
            set
            {
                Toggle.isOn = value;
                OnValueChanged(value);
            }
        }

        private Toggle Toggle => _toggle;
        
        public void SetWithoutNotify(bool isOn)
        {
            Toggle.SetIsOnWithoutNotify(isOn);
        }
        
        public void AddListener(Action<bool> onToggle)
        {
            _onToggle += onToggle;
        }
        
        public void RemoveListener(Action<bool> onToggle)
        {
            _onToggle -= onToggle;
        }
        
        public void AddToggleValidation(Func<bool> validate)
        {
            _allowToggle = validate;
        }
        
        public void RemoveToggleValidation()
        {
            _allowToggle = null;
        }
        
        protected override void OnInitialized()
        {
            if (Toggle.isOn)
            {
                _animationPlayer.PlayShowAnimationInstant();
            }
            else
            {
                _animationPlayer.PlayHideAnimationInstant();
            }
            
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void BeforeCleanUp()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            if (isOn && _allowToggle != null)
            {
                var validationResult = _allowToggle.Invoke();
                if (!validationResult) return;
            }
            
            _onToggle?.Invoke(isOn);
            
            PlayToggleStateAnimation(isOn);
        }

        private void PlayToggleStateAnimation(bool isOn)
        {
            if (isOn)
            {
                _animationPlayer.PlayShowAnimation();
            }
            else
            {
                _animationPlayer.PlayHideAnimation();
            }
        }
    }
}