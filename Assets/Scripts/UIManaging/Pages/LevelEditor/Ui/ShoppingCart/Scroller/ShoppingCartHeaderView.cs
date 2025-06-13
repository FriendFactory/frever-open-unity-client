using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    internal sealed class ShoppingCartHeaderView : BaseContextDataView<ShoppingCartHeaderModel>
    {
        [SerializeField] private TextMeshProUGUI _titleText;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _titleText.text = ContextData.Title;
        }
    }
}