namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs
{
    internal sealed class FovAdvancedOptionTab : AdvancedCameraTab
    {
        public override bool ResetOnSettingsChanged => true;
        protected override string Name => Localization.ZoomButton;
    }
}
