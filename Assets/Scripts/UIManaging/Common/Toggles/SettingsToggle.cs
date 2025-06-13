using System;
using UI.UIAnimators;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Toggles
{
    public abstract class SettingsToggle : MonoBehaviour
    {
        [SerializeField] protected Toggle Toggle;

        [SerializeField] private ParallelUiAnimationPlayer _animationPlayer;

        private Action<bool> _onToggle;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsOn
        {
            get => Toggle.isOn;
            set => Toggle.isOn = value;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        public void AddListener(Action<bool> onToggle)
        {
            _onToggle += onToggle;
        }
        
        public void RemoveListener(Action<bool> onToggle)
        {
            _onToggle -= onToggle;
        }

        public void SetWithoutNotify(bool isOn)
        {
            Toggle.SetIsOnWithoutNotify(isOn);
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected virtual void OnEnable()
        {
            if (Toggle.isOn)
            {
                _animationPlayer.PlayShowAnimationInstant();
            }
            else
            {
                _animationPlayer.PlayHideAnimationInstant();
            }
            
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            Toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void OnTransitionStarted() {}

        protected virtual void OnTransitionCompleted() {}
        
        protected virtual void OnValueChanged(bool value)
        {
            _onToggle?.Invoke(value);
        }

        protected void PlayToggleState(bool isOn)
        {
            if (isOn)
            {
                _animationPlayer.PlayShowAnimation(OnTransitionCompleted);
            }
            else
            {
                _animationPlayer.PlayHideAnimation(OnTransitionCompleted);
            }
        }
    }
}