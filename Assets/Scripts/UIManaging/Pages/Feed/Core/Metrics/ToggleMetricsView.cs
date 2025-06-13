using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Feed.Core.Metrics
{
    public abstract class ToggleMetricsView : BaseContextDataView<ToggleMetricsModel>
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Color _defaultTextColor = Color.white;
        [SerializeField] private Color _activeTextColor = new Color(0.98f, 0.36f, 0.67f);

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _toggle.isOn = ContextData.IsOn;
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            ContextData.OnMetricsValueChangedEvent += RefreshText;
            ContextData.OnSetIsOnEvent += OnSetIsOn;
            RefreshText();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            
            if(ContextData == null) return;
            
            ContextData.OnMetricsValueChangedEvent -= RefreshText;
            ContextData.OnSetIsOnEvent -= OnSetIsOn;
        }

        protected virtual void OnToggleSet()
        {
        }

        protected virtual void OnToggleUnset()
        {
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSetIsOn()
        {
            _toggle.isOn = ContextData.IsOn;
        }

        private void RefreshText()
        {
            _amountText.text = ContextData.MetricsValue.ToString();
            _amountText.color = _toggle.isOn ? _activeTextColor : _defaultTextColor;
        }

        private void OnToggleValueChanged(bool value)
        {
            if (value)
            {
                ContextData.Add();
                OnToggleSet();
            }
            else
            {
                ContextData.Subtract();
                OnToggleUnset();
            }

            ContextData.SetIsOnSilent(value);
        }
    }
}