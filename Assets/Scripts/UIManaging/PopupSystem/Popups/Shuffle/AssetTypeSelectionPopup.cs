using System.Linq;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    public class AssetTypeSelectionPopup: ConfigurableBasePopup<AssetTypeSelectionPopupConfiguration>
    {
        [SerializeField] private AssetTypeListView _assetTypeListView;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        protected AssetTypeListModel AssetTypeListModel => Config.AssetTypeListModel;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(OnConfirm);
            _cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirm);
            _cancelButton.onClick.RemoveListener(OnCancel);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(AssetTypeSelectionPopupConfiguration configuration)
        {
            AssetTypeListModel.ItemSelectionChanged += OnSelectionChanged;
            
            _assetTypeListView.Initialize(AssetTypeListModel);
            
            UpdateConfirmButton();
        }

        protected override void OnHidden()
        {
            _assetTypeListView.CleanUp();

            if (AssetTypeListModel != null)
            {
                AssetTypeListModel.ItemSelectionChanged -= OnSelectionChanged;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected virtual void OnConfirm()
        {
            var shuffleModel = new ShuffleModel(AssetTypeListModel.SelectedItems
                                                                  .Select(model => model.Type)
                                                                  .Aggregate((type1, type2) => type1 | type2));

            Config.ConfirmAction?.Invoke(shuffleModel);
        }

        private void OnCancel()
        {
            Config.CancelAction?.Invoke();
            Hide();
        }

        private void OnSelectionChanged(AssetTypeModel model)
        {
            UpdateConfirmButton();
        }

        private void UpdateConfirmButton()
        {
            _confirmButton.interactable = Config.ConfirmAction != null && AssetTypeListModel?.SelectedItems.Count > 0;
        }
    }
}