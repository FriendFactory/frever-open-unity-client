using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs
{
    internal sealed class RangeAdvancedOptionTab : AdvancedCameraTab, ISubscribeGesture
    {
        public override bool ResetOnSettingsChanged => true;

        protected override string Name => Localization.RangeButton;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SubscribeGesture()
        {
            CameraInputController.OrbitRadiusUpdated += UpdateSliderValue;
        }

        public void UnsubscribeGesture()
        {
            CameraInputController.OrbitRadiusUpdated -= UpdateSliderValue;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateSliderValue(float value)
        {
            var tabView = AdvancedOptionTabView as RangeTabView;
            tabView.UpdateSliderValue();
        }
    }
}
