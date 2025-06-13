using Common.Abstract;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo
{
    public sealed class ShoppingCartInfo: BaseContextPanel<ShoppingCartInfoModel>
    {
        [SerializeField] private ShoppingBagCounterUI _shoppingBagCounter;
        [SerializeField] private GameObject _insufficientFundsPanel;
        
        protected override void OnInitialized()
        {
            ContextData.ItemsCountChanged += OnItemsCountChanged;
            ContextData.SufficientFundsChanged += OnSufficientFundsChanged;
            
            OnSufficientFundsChanged(ContextData.HasSufficientFunds);
            OnItemsCountChanged(ContextData.ItemsCount);
        }
        
        protected override void BeforeCleanUp()
        {
            ContextData.ItemsCountChanged -= OnItemsCountChanged;
            ContextData.SufficientFundsChanged -= OnSufficientFundsChanged;
        }

        private void OnSufficientFundsChanged(bool hasSufficientFunds)
        {
            _insufficientFundsPanel.SetActive(!hasSufficientFunds);
        }

        private void OnItemsCountChanged(int itemsCount)
        {
            _shoppingBagCounter.SetBagNumber(itemsCount);
        }
    }
}