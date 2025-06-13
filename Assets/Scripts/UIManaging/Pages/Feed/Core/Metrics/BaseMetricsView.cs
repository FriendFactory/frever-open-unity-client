using Abstract;
using Core.DataWrapper;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Feed.Core.Metrics
{
    public abstract class BaseMetricsView : BaseContextDataView<LongDataWrapper>
    {
        [SerializeField] private TextMeshProUGUI amountText;

        protected override void OnInitialized()
        {
            ContextData.OnValueChangedEvent += RefreshAmountText;
            RefreshAmountText();
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ContextData.OnValueChangedEvent -= RefreshAmountText;
        }

        private void RefreshAmountText()
        {
            amountText.text = ContextData.Value.ToShortenedString();
        }
    }
}