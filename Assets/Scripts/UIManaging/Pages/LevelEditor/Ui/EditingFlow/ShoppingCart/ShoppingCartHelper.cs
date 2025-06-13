using System;
using UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ShoppingCart
{
    public sealed class ShoppingCartHelper: MonoBehaviour
    {
        [SerializeField] private ShoppingCartAdapter _shoppingCartAdapter;

        [Inject] private ShoppingCartInfoModel _shoppingCartInfoModel;
        [Inject] private ShoppingCartController _shoppingCartController;

        public bool HasNotPurchasedItems => _shoppingCartInfoModel.GetNotOwnedWardrobes().Count > 0;

        public event Action ShoppingCartClosed;
        
        public void Initialize()
        {
            _shoppingCartAdapter.Initialize();
            
            _shoppingCartAdapter.ShoppingCartClosed += OnShoppingCartClosed;
        }

        public void Cleanup()
        {
            _shoppingCartAdapter.CleanUp();
            
            _shoppingCartAdapter.ShoppingCartClosed -= OnShoppingCartClosed;
        }

        public void Show(Action onPurchased)
        {
            var items = _shoppingCartInfoModel.GetNotOwnedWardrobes();
            _shoppingCartAdapter.ShowShoppingCart(items, OnComplete);

            async void OnComplete()
            {
                try
                {
                    _shoppingCartAdapter.LockShoppingCart();
                    
                    await _shoppingCartController.PurchaseAssetsAsync();
                    
                    _shoppingCartAdapter.CloseShoppingCart(() => {
                        _shoppingCartAdapter.UnlockShoppingCart();
                        onPurchased?.Invoke();
                    });
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnShoppingCartClosed()
        {
            ShoppingCartClosed?.Invoke();
        }
    }
}