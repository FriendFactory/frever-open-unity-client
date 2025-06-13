using Modules.CameraSystem.CameraAnimations;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    internal sealed class FovTabView : CameraJogWheelView
    {
        protected override float MaxValue => CameraSystem.GetCurrentCameraSetting().FoVMax;
        protected override float MinValue => CameraSystem.GetCurrentCameraSetting().FoVMin;
        protected override float DefaultValue => CameraSystem.GetCurrentCameraSetting().FoVStart;
        protected override CameraAnimationProperty CameraProperty => CameraAnimationProperty.FieldOfView;

        protected override float GetIndicatorXPosition()
        {
            var scrollBarSizeDeltaX = JogWheelInputHandler.GetScrollRectContentSizeDelta().x;
            var defaultPercentage = 1 - (Mathf.Abs(DefaultValue - MinValue) / TotalValue);
            var defaultPos = -scrollBarSizeDeltaX * defaultPercentage;
            IndicatorOriginPosX = -scrollBarSizeDeltaX / 2f;
            var posDiff = IndicatorOriginPosX - defaultPos;
            var xPosition = posDiff;

            if (IndicatorOriginPosX <= defaultPos)
            {
                xPosition = posDiff;
            }

            return xPosition;
        }

        protected override Vector2 GetScrollBarPosition(float targetValue)
        {
            var scrollBarValue = Mathf.Abs(targetValue - MinValue);
            scrollBarValue = (TotalValue - targetValue) / TotalValue;
            return new Vector2(scrollBarValue, 0);
        }

        protected override void OnScrollBarPositionChanged(Vector2 position)
        {
            var value = Mathf.Clamp01(position.x);
            var reversedValue = TotalValue * (1 - value);
            ScrollbarValueChanged?.Invoke(reversedValue);
        }

        protected override void OnValueChanged(float value)
        {
            base.OnValueChanged(value);
            CameraSystem.GetCurrentCameraSetting().FoVCurrent = value;
        }
    }
}