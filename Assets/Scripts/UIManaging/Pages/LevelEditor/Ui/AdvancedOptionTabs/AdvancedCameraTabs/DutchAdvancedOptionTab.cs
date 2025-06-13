namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs
{
    internal sealed class DutchAdvancedOptionTab : AdvancedCameraTab, ISubscribeGesture
    {
        public override bool ResetOnSettingsChanged => false;
        protected override string Name => Localization.SpinButton;
        public void SubscribeGesture()
        {
        }

        public void UnsubscribeGesture()
        {
        }
    }
}
