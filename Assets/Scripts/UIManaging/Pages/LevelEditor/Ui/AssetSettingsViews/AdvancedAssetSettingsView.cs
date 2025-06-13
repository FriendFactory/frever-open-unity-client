using System;
using Extensions;
using UIManaging.Pages.LevelEditor.Ui.AdvancedSettings;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public class AdvancedAssetSettingsView : AssetSettingsView
    {
        [SerializeField] protected AdvancedSettingsView _advancedSettingsView;
        [SerializeField] private Button _advancedSettingsButton;
        
        public event Action SettingChanged;
        
        private CanvasGroup _mainViewCanvasGroup;
        private CanvasGroup _canvasGroup;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Setup()
        {
            _advancedSettingsView.Setup();
            _advancedSettingsButton.onClick.AddListener(DisplayAdvancedSettingsView);
            _advancedSettingsView.SettingChanged += OnSettingChanged;
        }

        public override void PartialHide()
        {
        }

        public override void PartialDisplay()
        {
        }

        public void AddListenerToAdvancedSettingsButton(UnityAction onClick)
        {
            _advancedSettingsButton.onClick.AddListener(onClick);
        }

        public void SetMainViewCanvasGroup(CanvasGroup canvasGroup)
        {
            _mainViewCanvasGroup = canvasGroup;
        }

        public void HideAdvancedSettingsView()
        {
            _advancedSettingsView.Hide();
        }

        public override void CleanUp()
        {
            _advancedSettingsView.CleanUp();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void ResetAdvancedView()
        {
            _advancedSettingsView.ResetTabs();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DisplayAdvancedSettingsView()
        {
            _advancedSettingsView.Display();
            _advancedSettingsView.Hidden += OnHideAdvancedSettingsView;
            _canvasGroup.Set(0f, false, false);
            _mainViewCanvasGroup.Set(0f, false, true);
        }

        private void OnHideAdvancedSettingsView()
        {
            _advancedSettingsView.Hidden -= OnHideAdvancedSettingsView;
            _mainViewCanvasGroup.Enable();
            _canvasGroup.Enable();
        }
        
        private void OnSettingChanged()
        {
            SettingChanged?.Invoke();
        }
    }
}
