using JetBrains.Annotations;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    [UsedImplicitly]
    public class AssetTypeListModel: SelectionListModel<AssetTypeModel>
    {
        public AssetTypeListModel(ShuffleAssetTypesSettings shuffleSettings) 
            : base(shuffleSettings.MaxSelected, shuffleSettings.AssetTypeModels, shuffleSettings.SelectedIds, shuffleSettings.LockedIds)
        {
        }
    }
}