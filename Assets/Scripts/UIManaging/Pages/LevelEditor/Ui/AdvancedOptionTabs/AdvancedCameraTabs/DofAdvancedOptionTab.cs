namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs
{
    internal sealed class DofAdvancedOptionTab : AdvancedCameraTab
    {
        public override bool ResetOnSettingsChanged => true;
        protected override string Name => Localization.BlurButton;
    }
}
