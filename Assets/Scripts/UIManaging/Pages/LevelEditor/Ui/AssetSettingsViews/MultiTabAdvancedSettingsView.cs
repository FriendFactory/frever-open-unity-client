using System.Collections.Generic;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs;
using UIManaging.Pages.LevelEditor.Ui.AdvancedSettings;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    internal class MultiTabAdvancedSettingsView : AdvancedSettingsView
    {
        [SerializeField] public List<MultipleAdvancedOptionTab> Tabs = new List<MultipleAdvancedOptionTab>();

        public override void Setup()
        {
            if (IsSetup) return;
            SetupTabs();
            SetupTabViews();
            base.Setup();
        }

        public override void Display()
        {
            base.Display();
            SaveSettingsBeforeEditing();
        }

        public override void CleanUp()
        {
            foreach (var tab in Tabs)
            {
                if (tab is ISubscribeGesture subscribeTab)
                {
                    subscribeTab.UnsubscribeGesture();
                }
            }

            foreach (var tab in Tabs)
            {
                tab.CleanUp();
            }

            base.CleanUp();
        }
        
        public override void ResetTabs()
        {
            foreach (var tab in Tabs)
            {
                tab.Reset();
            }
        }

        protected override void OnDiscard()
        {
            DiscardSettingsChangesInTabs();
            base.OnDiscard();
        }

        private void SetupTabs()
        {
            foreach (var tab in Tabs)
            {
                tab.SetupTab(HideTabViews);
            }
        }
        
        private void HideTabViews(AdvancedOptionTab currentTab)
        {
            foreach (var tab in Tabs)
            {
                if (tab == currentTab)
                {
                    continue;
                }

                tab.SetToggle(false);
            }
        }
        
        private void SetupTabViews()
        {
            foreach (var tab in Tabs)
            {
                if (tab is ISubscribeGesture subscribeTab)
                {
                    subscribeTab.SubscribeGesture();
                }
            }

            EnableOptionTabs(false);

            if (Tabs.Count > 0)
            {
                Tabs[0].SetToggle(true);
            }
        }
        
        private void EnableOptionTabs(bool enabled)
        {
            foreach (var tab in Tabs)
            {
                tab.DisplayView(enabled);
                tab.SetToggle(enabled);
            }
        }

        protected override void OnEventLoaded()
        {
            foreach (var tab in Tabs)
            {
                tab.UpdateViewComponents();
            }
        }

        private void SaveSettingsBeforeEditing()
        {
            foreach (var tab in Tabs)
            {
                tab.SaveSettingsBeforeEditing();
            }
        }

        private void DiscardSettingsChangesInTabs()
        {
            foreach (var tab in Tabs)
            {
                tab.DiscardChanges();
            }
        }
    }
}