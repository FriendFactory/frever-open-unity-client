namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    internal sealed class ShoppingCartHeaderModel : ShoppingCartItemModel
    {
        public string Title { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ShoppingCartHeaderModel(string title)
        {
            Title = title;
        }
    }
}