using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedSetLocationTabViews;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public class SetLocationSettingsView : AdvancedAssetSettingsView
    {
        [SerializeField] private TimeOfDayView _timeOfDayView;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Setup()
        {
            _timeOfDayView.Setup();
            base.Setup();
        }

        public override void PartialHide()
        {
            _timeOfDayView.Display(false);
        }

        public override void PartialDisplay()
        {
            _timeOfDayView.Display(HasDayNightController());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private bool HasDayNightController()
        {
            return _timeOfDayView.DayNightController != null;
        }
    }
}