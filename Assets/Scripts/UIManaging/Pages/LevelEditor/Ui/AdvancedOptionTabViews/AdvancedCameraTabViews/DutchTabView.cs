using Modules.CameraSystem.CameraAnimations;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    internal sealed class DutchTabView : CameraJogWheelView
    {
        protected override float MaxValue => 180f;
        protected override float MinValue => -180f;
        protected override float DefaultValue => 0;

        protected override CameraAnimationProperty CameraProperty => CameraAnimationProperty.Dutch;
    }
}
