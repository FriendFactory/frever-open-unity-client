using Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.SelectionPanel
{
    public abstract class SelectionItemView<TSelectionItemModel> : BaseContextDataView<TSelectionItemModel> 
        where TSelectionItemModel: class, ISelectionItemModel
    {
        [SerializeField] private Button _deselectButton;
        
        protected override void OnInitialized()
        {
            _deselectButton.onClick.AddListener(OnDeselect);
        }

        protected override void BeforeCleanup()
        {
            _deselectButton.onClick.RemoveListener(OnDeselect);
            
            base.BeforeCleanup();
        }

        private void OnDeselect()
        {
            ContextData.IsSelected = false;
        }
    }

    public sealed class SelectionItemView : SelectionItemView<ISelectionItemModel> { }
}