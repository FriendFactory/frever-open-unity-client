using Bridge.Models.ClientServer.Level.Shuffle;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.AssetButtons;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Shuffle;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class ShuffleButton : BaseAssetButton
    {
        [SerializeField] private bool _showConfirmationPopup;

        [Inject] private ILevelManager _levelEditor;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private AssetTypeListModel _assetTypeListModel;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            _levelEditor.ShufflingBegun += RefreshState;
            _levelEditor.ShufflingFailed += RefreshState;
            _levelEditor.ShufflingDone += RefreshState;
            _levelEditor.AssetUpdateStarted += OnAssetUpdateStarted;
            _levelEditor.AssetUpdateCancelled += OnAssetUpdatingCompleted;
            _levelEditor.AssetUpdateFailed += OnAssetUpdatingCompleted;
            _levelEditor.AssetUpdateCompleted += OnAssetUpdatingCompleted;
            _levelEditor.CharactersOutfitsUpdatingBegan += RefreshState;
            _levelEditor.CharactersOutfitsUpdated += RefreshState;
            RefreshState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _levelEditor.ShufflingBegun -= RefreshState;
            _levelEditor.ShufflingFailed -= RefreshState;
            _levelEditor.ShufflingDone -= RefreshState;
            _levelEditor.AssetUpdateStarted -= OnAssetUpdateStarted;
            _levelEditor.AssetUpdateCancelled -= OnAssetUpdatingCompleted;
            _levelEditor.AssetUpdateFailed -= OnAssetUpdatingCompleted;
            _levelEditor.AssetUpdateCompleted -= OnAssetUpdatingCompleted;
            _levelEditor.CharactersOutfitsUpdatingBegan -= RefreshState;
            _levelEditor.CharactersOutfitsUpdated -= RefreshState;
        }
        
        protected override void OnClicked()
        {
            if (_showConfirmationPopup)
            {
                _popupManagerHelper.OpenShuffleEventPopup(OnShuffle, null);
            }
            else
            {
                OnShuffle();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnShuffle()
        {
            var config = new AssetTypeSelectionPopupConfiguration(_assetTypeListModel, OnShufflingClicked)
            {
                PopupType = PopupType.AIShuffle
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
            
            void OnShufflingClicked(ShuffleModel shuffleModel)
            {
                if (_levelEditor.IsShuffling) return;
                _levelEditor.ShuffleAI(shuffleModel);
            }
        }

        private void RefreshState()
        {
            Interactable = !_levelEditor.IsShuffling && !_levelEditor.IsChangingAsset && !_levelEditor.IsChangingOutfit;
        }
        
        private void OnAssetUpdateStarted(DbModelType type, long id)
        {
            RefreshState();
        }
        
        private void OnAssetUpdatingCompleted(DbModelType type)
        {
            RefreshState();
        }
    }
}