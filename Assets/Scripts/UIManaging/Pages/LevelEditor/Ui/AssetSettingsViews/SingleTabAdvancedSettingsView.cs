using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews;
using UIManaging.Pages.LevelEditor.Ui.AdvancedSettings;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public class SingleViewAdvancedSettingsView : AdvancedSettingsView
    {
        [SerializeField] private AdvancedOptionTabView _tabView;

        public override void Setup()
        {
            if (IsSetup) return;
            _tabView.Setup();
            base.Setup();
        }

        public override void Display()
        {
            base.Display();
            _tabView.Display();
        }

        public override void Hide()
        {
            base.Hide();
            _tabView.Hide();
        }

        public override void ResetTabs()
        {
            _tabView.Reset();
        }

        public override void CleanUp()
        {
            _tabView.CleanUp();
            base.CleanUp();
        }

        protected override void OnEventLoaded()
        {
            _tabView.UpdateViewComponents();
        }
    }
}