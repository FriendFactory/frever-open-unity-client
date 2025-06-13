using System;
using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class TabView : BaseTabView
    {
        [SerializeField] protected TextMeshProUGUI _tabNameText;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            RefreshNameText();
            ContextData.OnNameChangedEvent += OnNameChanged;
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ContextData.OnNameChangedEvent -= OnNameChanged;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnNameChanged()
        {
            RefreshNameText();
        }

        private void RefreshNameText()
        {
            _tabNameText.text = ContextData.Name;
            gameObject.name = $"{GetType().Name} [{ContextData.Name}]";
        }
    }
}