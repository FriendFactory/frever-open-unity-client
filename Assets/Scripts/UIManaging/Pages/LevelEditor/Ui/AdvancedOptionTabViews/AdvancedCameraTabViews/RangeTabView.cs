using Common;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSettingsHandling;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    internal sealed class RangeTabView : CameraJogWheelView
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected override float MinValue => CameraSetting.OrbitRadiusMin;
        protected override float MaxValue => CameraSetting.OrbitRadiusMax;
        protected override float DefaultValue => CameraSetting.OrbiRadiusStart;
        protected override CameraAnimationProperty CameraProperty => CameraAnimationProperty.OrbitRadius;
        private CameraSetting CameraSetting => CameraSystem.GetCurrentCameraSetting();
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void UpdateSliderValue()
        {
            SetValue(CameraSystem.GetValueOf(CameraProperty));
        }
    }
}