using System;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.PopupSystem.Popups.Shuffle;

namespace UIManaging.PopupSystem.Configurations
{
    public class AssetTypeSelectionPopupConfiguration: PopupConfiguration
    {
        public AssetTypeListModel AssetTypeListModel { get; }
        public Action<ShuffleModel> ConfirmAction { get; }
        public Action CancelAction { get; }
        
        public AssetTypeSelectionPopupConfiguration(AssetTypeListModel assetTypeListModel, Action<ShuffleModel> confirmAction, Action cancelAction = null) 
            : base(PopupType.AssetTypeSelection, null, "")
        {
            AssetTypeListModel = assetTypeListModel;
            ConfirmAction = confirmAction;
            CancelAction = cancelAction;
        }
    }
}