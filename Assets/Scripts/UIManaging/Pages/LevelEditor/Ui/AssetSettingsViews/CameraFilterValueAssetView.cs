using System;
using UIManaging.Common;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    internal sealed class CameraFilterValueAssetView : AdvancedOptionTabViews.JogWheelViews.AssetViewJogWheel
    {
        public Action<float> ValueChanged;

        public float CurrentValue { get; private set; }
        protected override float MaxValue => 1;
        protected override float MinValue => 0;
        protected override float DefaultValue => 1;

        public override void Setup()
        {
            ScrollBar.gameObject.AddComponent<SliderEventHandler>();
            ScrollbarValueChanged += UpdateFilterValue;
            base.Setup();
            SetValue(DefaultValue);
        }

        protected override string GetTextFormat(float value)
        {
            if (DefaultValue != 0 && MaxValue != 0)
            {
                value /= DefaultValue;
            }

            return value.ToString("F1");
        }

        public override void SetValueSilent(float value)
        {
            CurrentValue = value;
            base.SetValueSilent(value);
            Refresh();
        }

        private void Refresh()
        {
            SetDisplayText(CurrentValue);
        }

        private void UpdateFilterValue(float value)
        {
            CurrentValue = value;
            ValueChanged?.Invoke(value);
        }
    }
}