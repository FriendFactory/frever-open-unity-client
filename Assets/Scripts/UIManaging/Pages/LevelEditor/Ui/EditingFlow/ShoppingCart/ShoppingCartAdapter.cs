using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using BrunoMikoski.AnimationSequencer;
using Common.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class ShoppingCartAdapter: BaseContextlessPanel 
    {
        [SerializeField]private global::Common.ShoppingCart.ShoppingCart _shoppingCart;
        [SerializeField] private AnimationSequencerController _animationSequencer;
        [SerializeField] private Button _shoppingCartCloseButton;
        
        public event Action ShoppingCartClosed;
        public event Action<IEntity, bool> ItemSelectionChanged;
        public event Action PurchaseItemsRequested;

        protected override void OnInitialized()
        {
            _shoppingCart.ShoppingCartClosed += OnShoppingCartClosed;
            _shoppingCart.ItemSelectionChanged += OnItemSelectionChanged;
            _shoppingCart.PurchasingItemsRequested += OnPurchaseItemsRequested;
            
            _shoppingCartCloseButton.onClick.AddListener(OnClose);
        }

        protected override void BeforeCleanUp()
        {
            _shoppingCart.ShoppingCartClosed -= OnShoppingCartClosed;
            _shoppingCart.ItemSelectionChanged -= OnItemSelectionChanged;
            _shoppingCart.PurchasingItemsRequested -= OnPurchaseItemsRequested;
            
            _shoppingCartCloseButton.onClick.RemoveListener(OnClose);
            
            _animationSequencer.Kill();
        }

        public void ShowShoppingCart(List<WardrobeShortInfo> items, Action onComplete)
        {
            if (_shoppingCart.IsShown) return;
            
            _shoppingCart.Setup(items, onComplete);
            
            _animationSequencer.OnStartEvent.AddListener(OnShoppingCartAnimationStarted);
            _animationSequencer.PlayForward();
        }

        public void CloseShoppingCart(Action onClose = null)
        {
            // do not complete, because otherwise the OnCompleted callback will be called immediately
            _animationSequencer.PlayBackwards(false, () =>
            {
                _shoppingCart.Close();
                onClose?.Invoke();
            });
        }
        
        public void LockShoppingCart() => _shoppingCart.Lock();
        public void UnlockShoppingCart() => _shoppingCart.Unlock();

        private void OnShoppingCartAnimationStarted()
        {
            _animationSequencer.OnStartEvent.RemoveListener(OnShoppingCartAnimationStarted); 
            
            _shoppingCart.Show(false);
        }

        private void OnItemSelectionChanged(IEntity item, bool isSelected) => ItemSelectionChanged?.Invoke(item, isSelected);
        private void OnShoppingCartClosed() => ShoppingCartClosed?.Invoke();
        private void OnPurchaseItemsRequested() => PurchaseItemsRequested?.Invoke();
        private void OnClose() => CloseShoppingCart(null);
    }
}