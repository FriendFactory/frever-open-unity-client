using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionCategoryPanel : BaseContextDataView<ShareSelectionCategoryModel>
    {
        [SerializeField] private TMP_Text _title;

        protected override void OnInitialized()
        {
            _title.text = ContextData.Title;
        }
    }
}