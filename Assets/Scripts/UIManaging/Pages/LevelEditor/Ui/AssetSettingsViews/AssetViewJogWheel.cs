using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public abstract class AssetViewJogWheel : MonoBehaviour
    {
        [SerializeField] protected ScrollRect ScrollBar;
        [SerializeField] private TextMeshProUGUI _displayValueText;

        protected Action<float> ScrollbarValueChanged;
        protected abstract float MaxValue { get; }
        protected abstract float MinValue { get; }
        protected abstract float DefaultValue { get; }
        private float _totalValue => Mathf.Abs(MaxValue - MinValue);
        private float _originPosX;

        public void Display(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void CleanUp()
        {
            ScrollBar.onValueChanged.RemoveAllListeners();
        }

        public virtual void Setup()
        {
            ScrollBar.onValueChanged.AddListener(OnValueChanged);
            _displayValueText.text = DefaultValue.ToString(CultureInfo.InvariantCulture);
        }

        protected virtual void SetValue(float value)
        {
            value = Mathf.Abs(value - MinValue) / _totalValue;
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

        public virtual void Reset()
        {
            SetValue(DefaultValue);
        }
        
        private void OnValueChanged(Vector2 position)
        {
            var value = Mathf.Clamp01(position.x);
            var convertedValue = _totalValue * value + MinValue;
            ScrollbarValueChanged?.Invoke(convertedValue);
            SetDisplayText(convertedValue);
        }
    }
}