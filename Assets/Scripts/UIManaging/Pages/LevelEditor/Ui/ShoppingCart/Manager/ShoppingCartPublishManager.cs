using System.Collections.Generic;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Models;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartPublishManager : ShoppingCartManager
    {
        [SerializeField] private GameObject _balancePanel;
        
        private LocalUserDataHolder _userData;
        private List<IPurchasable> _storeAssets;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        public void Construct(LocalUserDataHolder userData)
        {
            _userData = userData;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(Level levelData)
        {
            _storeAssets = levelData.GetStoreAssets(_userData.PurchasedAssets);
            UpdateConfirmedAssets();
        }

        public override void ShowCart()
        {
            base.ShowCart();
            _balancePanel.SetActive(true);
        }

        public override List<IPurchasable> GetSelectedAssets()
        {
            return null;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override List<IPurchasable> GetStoreAssets()
        {
            return _storeAssets;
        }

        protected override void OnPanelClosed()
        {
            _balancePanel.SetActive(false);
        }
    }
}