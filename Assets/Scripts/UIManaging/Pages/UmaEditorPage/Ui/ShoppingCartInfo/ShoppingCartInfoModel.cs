using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Modules.FreverUMA;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo
{
    [UsedImplicitly]
    public class ShoppingCartInfoModel: IInitializable, IDisposable
    {
        private readonly UmaLevelEditor _umaLevelEditor;
        private readonly WardrobePanelPurchaseHelper _wardrobePanelPurchaseHelper;
        private readonly UmaLevelEditorPanelModel _umaLevelEditorPanelModel;
        private readonly ICharacterEditor _characterEditor;

        private int _itemsCount;
        private bool _hasSufficientFunds = true;

        public int ItemsCount
        {
            get => _itemsCount;
            private set
            {
                if (_itemsCount == value) return;
                
                _itemsCount = value;
                
                ItemsCountChanged?.Invoke(ItemsCount);
            }
        }

        public bool HasSufficientFunds
        {
            get => _hasSufficientFunds;
            private set
            {
                if (_hasSufficientFunds == value) return;
                
                _hasSufficientFunds = value;
                
                SufficientFundsChanged?.Invoke(_hasSufficientFunds);
            }
        }
        
        public event Action<int> ItemsCountChanged;
        public event Action<bool> SufficientFundsChanged;

        public ShoppingCartInfoModel(UmaLevelEditor umaLevelEditor, WardrobePanelPurchaseHelper wardrobePanelPurchaseHelper, UmaLevelEditorPanelModel umaLevelEditorPanelModel, ICharacterEditor characterEditor)
        {
            _umaLevelEditor = umaLevelEditor;
            _wardrobePanelPurchaseHelper = wardrobePanelPurchaseHelper;
            _umaLevelEditorPanelModel = umaLevelEditorPanelModel;
            _characterEditor = characterEditor;
        }

        public void Initialize()
        {
            _umaLevelEditor.WardrobeChanged += RefreshState;
            _umaLevelEditor.OutfitSelected += OnOutfitSelected;
            
            _umaLevelEditorPanelModel.PanelOpened += RefreshState;

            _characterEditor.CharacterUndressed += OnCharacterUndressed;
        }

        public void Dispose()
        {
            _umaLevelEditor.WardrobeChanged -= RefreshState;
            _umaLevelEditor.OutfitSelected -= OnOutfitSelected;
            
            _umaLevelEditorPanelModel.PanelOpened -= RefreshState;
            
            _characterEditor.CharacterUndressed -= OnCharacterUndressed;
        }
        
        public List<WardrobeShortInfo> GetNotOwnedWardrobes() => _wardrobePanelPurchaseHelper.GetNotOwnedWardrobes();

        public void RefreshState()
        {
            var notOwnedItems = GetNotOwnedWardrobes();
            
            ItemsCount = notOwnedItems.Count;

            HasSufficientFunds = _wardrobePanelPurchaseHelper.CheckSufficientFunds();
        }

        private void OnCharacterUndressed()
        {
            ItemsCount = 0;
            HasSufficientFunds = true;
        }

        private void OnOutfitSelected(OutfitFullInfo _)
        {
            RefreshState();
        }
    }
}