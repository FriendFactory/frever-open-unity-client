using JetBrains.Annotations;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartEditorPanel : ShoppingCartPanel
    {
        private BaseEditorPageModel _pageModel;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(BaseEditorPageModel pageModel)
        {
            _pageModel = pageModel;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Show()
        {
            base.Show();
            _pageModel.OnShoppingCartOpened();
        }

        public override void Hide()
        {
            base.Hide();
            _pageModel.OnShoppingCartClosed();
        }
    }
}