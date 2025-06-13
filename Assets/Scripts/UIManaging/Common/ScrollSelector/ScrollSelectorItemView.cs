using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Common.ScrollSelector
{
    public class ScrollSelectorItemView : BaseContextDataView<ScrollSelectorItemModel>
    {
        [SerializeField] private TextMeshProUGUI _text;

        protected override void OnInitialized()
        {
            _text.text = ContextData.Label;
        }
    }
}