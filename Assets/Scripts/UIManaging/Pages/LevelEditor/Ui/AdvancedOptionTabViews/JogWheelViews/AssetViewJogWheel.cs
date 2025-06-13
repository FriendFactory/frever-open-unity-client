using System;
using System.Globalization;
using TMPro;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews
{
    internal abstract class AssetViewJogWheel : MonoBehaviour
    {
        [SerializeField] protected ScrollRect ScrollBar;
        [SerializeField] private TextMeshProUGUI _displayValueText;

        private float _originPosX;
        private SliderEventHandler _sliderEventHandler;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<float> ScrollBarHorizontalPositionUpdated;
        
        //---------------------------------------------------------------------
        // Actions
        //---------------------------------------------------------------------
        
        protected Action<float> ScrollbarValueChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private SliderEventHandler SliderEventHandler
        {
            get
            {
                if (_sliderEventHandler == null)
                {
                    _sliderEventHandler = ScrollBar.gameObject.AddComponent<SliderEventHandler>();
                }
                return _sliderEventHandler;
            }
        }
        protected float ValueRange => Mathf.Abs(MaxValue - MinValue);
        protected abstract float MaxValue { get; }
        protected abstract float MinValue { get; }
        protected abstract float DefaultValue { get; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            OnValueChanged(ScrollBar.normalizedPosition);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void Display(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public virtual void Setup()
        {
            ScrollBar.onValueChanged.RemoveListener(OnValueChanged);
            ScrollBar.onValueChanged.AddListener(OnValueChanged);
            _displayValueText.text = DefaultValue.ToString(CultureInfo.InvariantCulture);
        }

        public virtual void SetValueSilent(float value)
        {
            value = Mathf.Abs(value - MinValue) / ValueRange;
            value = Mathf.Clamp01(value);
            ScrollBar.normalizedPosition = Vector2.right * value;
            SetDisplayText(value * ValueRange);
        }

        public void SetOnPointerDownListener(Action action)
        {
            SliderEventHandler.PointerDown = action;
        }
        public void SetOnPointerUpListener(Action action)
        {
            SliderEventHandler.PointerUp = action;
        }

        public virtual void Reset()
        {
            SetValue(DefaultValue);
        }

        public void ResetEvents()
        {
            SliderEventHandler.PointerUp = OnPointerUp;
            SliderEventHandler.PointerDown = null;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void SetValue(float value)
        {
            value = Mathf.Abs(value - MinValue) / ValueRange;
            value = Mathf.Clamp01(value);
            ScrollBar.normalizedPosition = Vector2.right * value;
            OnValueChanged(ScrollBar.normalizedPosition);
        }

        protected void SetDisplayText(float value)
        {
            _displayValueText.text = GetTextFormat(value);
        }

        protected virtual string GetTextFormat(float value)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);
            return text;
        }

        protected virtual void OnPointerUp()
        {
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnValueChanged(Vector2 position)
        {
            var convertedValue = ConvertToSliderValue(position);
            ScrollbarValueChanged?.Invoke(convertedValue);
            ScrollBarHorizontalPositionUpdated?.Invoke(Mathf.Clamp01(position.x));
            SetDisplayText(convertedValue);
        }
        
        private float ConvertToSliderValue(Vector2 position)
        {
            var value = Mathf.Clamp01(position.x);
            var convertedValue = ValueRange * value;
            return Mathf.Clamp(convertedValue, MinValue, MaxValue);
        }
    }
}