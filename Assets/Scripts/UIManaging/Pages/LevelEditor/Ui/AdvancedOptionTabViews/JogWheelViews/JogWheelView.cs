using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews
{
    public abstract class JogWheelView : AdvancedOptionTabView
    {
        [SerializeField] protected JogWheelInputHandler JogWheelInputHandler;
        [SerializeField] protected TextMeshProUGUI _currentValueText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private RectTransform _defaultIndicator;
        
        protected Action<float> ScrollbarValueChanged;
        protected Action<float> ValueSet;
        protected Action<float> DragEnded;
        
        protected float IndicatorOriginPosX;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected abstract float MaxValue { get; }
        protected abstract float MinValue { get; }
        protected abstract float DefaultValue { get; }
        protected float SavedValue { get; set; }
        protected float TotalValue => Mathf.Abs(MaxValue - MinValue);

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            SetupIndicatorPosition();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Display()
        {
            base.Display();
            _resetButton.onClick.AddListener(Reset);
        }

        public override void Hide()
        {
            base.Hide();
            _resetButton.onClick.RemoveListener(Reset);
        }

        public override void CleanUp()
        {
            JogWheelInputHandler.RemoveAllListeners();
        }

        public override void Setup()
        {
            JogWheelInputHandler.AddOnValueChangedListener(OnScrollBarPositionChanged);
            JogWheelInputHandler.DragEnded += OnJogWheelDragEnded;
            ScrollbarValueChanged += SetCurrentValueText;
            ScrollbarValueChanged += OnScrollBarValueChanged;
            SetupJogView();
        }

        public override void Reset()
        {
            SetValue(DefaultValue);
        }

        public override void Discard()
        {
            SetValue(SavedValue);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual void SetCurrentValueText(float value)
        {
            _currentValueText.text = value.ToString("F1");
        }

        protected virtual void SetupJogView()
        {
            SetupIndicatorPosition();
        }

        protected void SetValue(float value, bool setSilent = false)
        {
            JogWheelInputHandler.SetScrollRectNormalizedPosition(GetScrollBarPosition(value));
            SetCurrentValueText(value);

            if (!setSilent)
            {
                ValueSet?.Invoke(value);
            }
        }

        protected virtual Vector2 GetScrollBarPosition(float targetValue)
        {
            var scrollBarValue = Mathf.Abs(targetValue - MinValue) / TotalValue;
            scrollBarValue = Mathf.Clamp01(scrollBarValue);
            return Vector2.right * scrollBarValue;
        }

        protected virtual void OnScrollBarPositionChanged(Vector2 position)
        {
            var value = Mathf.Clamp01(position.x);
            ScrollbarValueChanged?.Invoke(TotalValue * value + MinValue);
        }

        protected void SetupIndicatorPosition()
        {
            var xPosition = GetIndicatorXPosition();
            _defaultIndicator.anchoredPosition = new Vector2(xPosition, _defaultIndicator.anchoredPosition.y);
        }

        protected virtual float GetIndicatorXPosition()
        {
            var scrollBarSizeDeltaX = JogWheelInputHandler.GetScrollRectContentSizeDelta().x;
            var defaultPercentage = Mathf.Abs(DefaultValue - MinValue) / TotalValue;
            var defaultPos = -scrollBarSizeDeltaX * defaultPercentage;
            IndicatorOriginPosX = -scrollBarSizeDeltaX / 2f;
            var posDiff = Mathf.Abs(IndicatorOriginPosX - defaultPos);
            var xPosition = posDiff;

            if (IndicatorOriginPosX <= defaultPos)
            {
                xPosition = -posDiff;
            }

            return xPosition;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnScrollBarValueChanged(float value)
        {
            if (Math.Abs(value - DefaultValue) < 0.001f) return;

            OnSettingChanged();
        }

        private void OnJogWheelDragEnded(Vector2 position)
        {
            DragEnded?.Invoke(position.x * TotalValue);
        }
    }
}