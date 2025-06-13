using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartEditorManager : ShoppingCartManager
    {
        private ILevelManager _levelManager;
        private LocalUserDataHolder _userData;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        public void Construct(ILevelManager levelManager, LocalUserDataHolder userData)
        {
            _levelManager = levelManager;
            _userData = userData;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _levelManager.EventStarted += OnEditorLoaded;
            _levelManager.EventSaved += UpdateConfirmedAssets;
            _levelManager.EventDeleted += UpdateConfirmedAssets;
        }

        private void OnDisable()
        {
            _levelManager.EventStarted -= OnEditorLoaded;
            _levelManager.EventSaved -= UpdateConfirmedAssets;
            _levelManager.EventDeleted -= UpdateConfirmedAssets;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override List<IPurchasable> GetSelectedAssets()
        {
            var purchasedAssets = _userData.PurchasedAssets;
            var selectedAssets = _levelManager.TargetEvent.GetStoreAssets(purchasedAssets);
            
            selectedAssets.RemoveAll(
                selected => ConfirmedAssets.Any(
                    confirmed => selected.AssetOffer.AssetId == confirmed.AssetOffer.AssetId));
            
            return selectedAssets;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override List<IPurchasable> GetStoreAssets()
        {
            var purchasedAssets = _userData.PurchasedAssets;
            return _levelManager.CurrentLevel.GetStoreAssets(purchasedAssets);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnEditorLoaded()
        {
            _levelManager.EventStarted -= OnEditorLoaded;
            UpdateConfirmedAssets();
        }
    }
}