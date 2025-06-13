using Bridge;
using Common.ShoppingCart;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class CharacterEditorShoppingItem : ShoppingCartItem
    {
        [SerializeField]
        private WardrobeUIItem _wardrobeUIItem;

        public override void Init(IBridge bridge)
        {
            base.Init(bridge);
            _wardrobeUIItem.Init(bridge);
        }

        public override void Setup<T>(T entity)
        {
            base.Setup(entity);
            _wardrobeUIItem.Setup(entity);
        }
    }
}
