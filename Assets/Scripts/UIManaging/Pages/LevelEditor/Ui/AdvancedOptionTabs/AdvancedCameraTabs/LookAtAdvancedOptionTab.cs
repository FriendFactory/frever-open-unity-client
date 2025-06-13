namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs
{
    internal sealed class LookAtAdvancedOptionTab : AdvancedCameraTab
    {
        public override bool ResetOnSettingsChanged => false;
        protected override string Name => Localization.FollowButton;
    }
}
