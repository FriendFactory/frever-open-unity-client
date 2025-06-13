using Modules.CameraSystem.CameraAnimations;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    internal sealed class DofTabView : CameraJogWheelView
    {
        protected override float MaxValue => CameraSystem.GetCurrentCameraSetting().DepthOfFieldMax;
        protected override float MinValue => CameraSystem.GetCurrentCameraSetting().DepthOfFieldMin;
        protected override float DefaultValue => CameraSystem.GetCurrentCameraSetting().DepthOfFieldStart;
        protected override CameraAnimationProperty CameraProperty => CameraAnimationProperty.DepthOfField;
    }
}