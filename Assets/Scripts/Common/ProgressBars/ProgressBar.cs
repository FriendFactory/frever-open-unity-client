using System;
using UnityEngine;

namespace Common.ProgressBars
{
    public abstract class ProgressBar : MonoBehaviour, IProgressBar
    {
        private const float MIN_PROGRESS_VALUE = 0f;
        private const float MAX_PROGRESS_VALUE = 1f;

        private float _value;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        public void Reset()
        {
            SetProgress(MIN_PROGRESS_VALUE);
        }

        //---------------------------------------------------------------------
        // IProgressBar
        //---------------------------------------------------------------------
        public event Action<float> ValueChanged;
        
        public float Value
        {
            get => _value;
            set => SetProgress(value);
        }

        public float Min => MIN_PROGRESS_VALUE;
        public float Max => MAX_PROGRESS_VALUE;
        
        //---------------------------------------------------------------------
        // Other
        //---------------------------------------------------------------------
        private void SetProgress(float value)
        {
            if (!Mathf.Approximately(_value, value))
            {
                _value = Mathf.Clamp(value, MIN_PROGRESS_VALUE, MAX_PROGRESS_VALUE);
                var roundedValue = Mathf.RoundToInt(_value * 100f);
                OnValueChanged(_value, $"{roundedValue.ToString()}%");
                
                // Notify subscriber(s)
                ValueChanged?.Invoke(_value);
            }
        }

        protected abstract void OnValueChanged(float value, string textValue);
    }
}