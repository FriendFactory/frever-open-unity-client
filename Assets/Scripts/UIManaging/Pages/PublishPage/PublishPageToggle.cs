using System;
using UIManaging.Common.Toggles;
using UnityEngine;

namespace UIManaging.Pages.PublishPage
{
    public sealed class PublishPageToggle : SettingsToggle
    {
        [SerializeField] private GameObject _onGameObject;
        [SerializeField] private GameObject _offGameObject;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private Func<bool> _allowToggle;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetActive(bool isActive)
        {
            if (!isActive) IsOn = false;
            gameObject.SetActive(isActive);
        }

        public void AddToggleValidation(Func<bool> validate)
        {
            _allowToggle = validate;
        }
        
        public void RemoveToggleValidation()
        {
            _allowToggle = null;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshObjects();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnTransitionCompleted()
        {
            RefreshObjects();
        }

        protected override void OnTransitionStarted()
        {
            if (!Toggle.isOn && _onGameObject != null)
            {
                _onGameObject.SetActive(false);
            }

            if (Toggle.isOn && _offGameObject != null)
            {
                _offGameObject?.SetActive(false);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RefreshObjects()
        {
            if (_onGameObject != null)
            {
                _onGameObject.SetActive(Toggle.isOn);
            }

            if (_offGameObject != null)
            {
                _offGameObject.SetActive(!Toggle.isOn);
            }
        }
        
        protected override void OnValueChanged(bool value)
        {
            var allowToggling = _allowToggle == null || _allowToggle();
            if (!allowToggling)
            {
                Toggle.SetIsOnWithoutNotify(!value);
                return;
            }
            
            OnTransitionStarted();
            PlayToggleState(value);
            
            base.OnValueChanged(value);
        }
    }
}