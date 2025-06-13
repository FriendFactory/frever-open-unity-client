using TMPro;
using UIManaging.Common.SelectionPanel;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal abstract class ShareSelectionSelectedItem<TModel>: SelectionItemView<TModel> where TModel : class, ISelectionItemModel
    {
        [SerializeField] private TextMeshProUGUI _title;
        
        protected abstract string Title { get; }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _title.text = Title;
            
            RefreshPortraitImage();
        }
        
        protected abstract void RefreshPortraitImage();
    }
}