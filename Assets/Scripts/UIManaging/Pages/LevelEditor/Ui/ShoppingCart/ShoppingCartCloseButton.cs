using UIManaging.Common.Buttons;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartCloseButton : BaseButton
    {
        [SerializeField] private ShoppingCartPanel _shoppingCartPanel;

        protected override void OnClickHandler()
        {
            _shoppingCartPanel.Hide();
        }
    }
}