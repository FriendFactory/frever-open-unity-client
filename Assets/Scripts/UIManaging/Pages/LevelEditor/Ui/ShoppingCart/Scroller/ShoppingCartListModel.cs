using System.Linq;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    internal sealed class ShoppingCartListModel
    {
        public ShoppingCartItemModel[] Items { get;}
        public ShoppingCartAssetModel[] AssetItems { get; }

        public ShoppingCartListModel(ShoppingCartItemModel[] items)
        {
            Items = items;
            AssetItems = items.OfType<ShoppingCartAssetModel>().ToArray();
        }
    }
}