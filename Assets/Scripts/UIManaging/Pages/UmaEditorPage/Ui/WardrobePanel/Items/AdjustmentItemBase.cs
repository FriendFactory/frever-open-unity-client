using Bridge.Models.ClientServer.StartPack.Metadata;
using System;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public abstract class AdjustmentItemBase : TextWardrobeUIItem<UmaAdjustment>
    {
        public event Action<float> AdjustmentChanged;
        public event Action<float, float> AdjustmentChangedDiff;

        protected bool _notUserChange = false;
        protected float _defaultValue;
        protected float _previousValue = 0.5f;

        public abstract void SetValue(float value);

        public virtual void SetDefaultValue(float value)
        {
            _defaultValue = value;
        }

        public void RaiseAdjustmentChanged(float value)
        {
            AdjustmentChanged?.Invoke(value);
        }

        public void RaiseAdjustmentChangedDiff(float startValue, float newValue)
        {
            AdjustmentChangedDiff?.Invoke(startValue, newValue);
        }
    }
}
