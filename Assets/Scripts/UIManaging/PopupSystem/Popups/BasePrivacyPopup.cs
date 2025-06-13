using System;
using System.Collections.Generic;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    internal abstract class BasePrivacyPopup<TAccess, TPrivacyToggle, TConfiguration> : BasePopup<TConfiguration>
        where TAccess : Enum
        where TPrivacyToggle : PrivacyToggleBase<TAccess>
        where TConfiguration : BasePrivacyPopupConfiguration<TAccess>
    {
        [SerializeField] protected List<TPrivacyToggle> options;

        protected TAccess SelectedOption;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            foreach (var option in options)
            {
                option.OnValueSelected += OnValueSelected;
            }
        }

        private void OnDestroy()
        {
            foreach (var option in options)
            {
                option.OnValueSelected -= OnValueSelected;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetValue(TAccess access)
        {
            SelectedOption = access;
            
            foreach (var option in options)
            {
                option.SetValue(access);
            }
        }

        public virtual void Close()
        {
            Hide(SelectedOption);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(TConfiguration configuration)
        {
            foreach (var option in options)
            {
                option.Initialize();
            }
            
            SetValue(configuration.CurrentAccess);
        }
        
        protected virtual void OnValueSelected(TAccess access)
        {
            SelectedOption = access;
            Close();
        }
    }
}