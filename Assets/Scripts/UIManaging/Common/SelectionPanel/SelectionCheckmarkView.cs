using Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.SelectionPanel
{
    public abstract class SelectionCheckmarkView<TSelectionItemModel> : BaseContextDataView<TSelectionItemModel> 
        where TSelectionItemModel: class, ISelectionItemModel
    {
        [SerializeField] private Toggle _selectionToggle;
        [SerializeField] private Color _enabledColor;
        [SerializeField] private Color _disabledColor;
        
        protected override void OnInitialized()
        {
            _selectionToggle.onValueChanged.AddListener(OnValueChanged);
            ContextData.SelectionChanged += UpdateCheckmark;
            
            _selectionToggle.graphic.color = ContextData.IsLocked ? _disabledColor : _enabledColor;
            UpdateCheckmark();
        }

        protected override void BeforeCleanup()
        {
            _selectionToggle.onValueChanged.RemoveListener(OnValueChanged);

            if (ContextData != null)
            {
                ContextData.SelectionChanged -= UpdateCheckmark;
            }

            base.BeforeCleanup();
        }

        private void OnValueChanged(bool value)
        {
            ContextData.IsSelected = value;
        }

        private void UpdateCheckmark()
        {
            _selectionToggle.SetIsOnWithoutNotify(ContextData.IsSelected);
        }
    }

    public sealed class SelectionCheckmarkView : SelectionCheckmarkView<ISelectionItemModel> { }
}