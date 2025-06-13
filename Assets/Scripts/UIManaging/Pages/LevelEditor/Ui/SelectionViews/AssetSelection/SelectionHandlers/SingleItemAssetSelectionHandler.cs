using System.Linq;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers
{
    public class SingleItemAssetSelectionHandler : BaseAssetSelectionHandler
    {
        public SingleItemAssetSelectionHandler(int minSelectedItemsAmount, bool canRefreshSelection) : base(minSelectedItemsAmount, 1, canRefreshSelection) {}

        public override void ChangeItemSelection(AssetSelectionItemModel itemModel, bool value)
        {
            if (itemModel == null) return;
        
            var isItemAlreadySelected = IsItemAlreadySelected(itemModel.ItemId, itemModel.CategoryId);
            var itemsWithSameId = AllItems.Where(x => x.ItemId == itemModel.ItemId && x.CategoryId == itemModel.CategoryId).ToArray();

            if (isItemAlreadySelected)
            {
                var canUnselectThisItem = UniqueSelectedItemModels.Count - 1 >= MinSelectedItemsAmount;
            
                if (!value && canUnselectThisItem)
                {
                    foreach (var item in itemsWithSameId)
                    {
                        RemoveItem(item);
                        item.SetIsSelected(false);
                    }
                    ExtraItemSelectionChangeAction?.Invoke(itemModel.RepresentedObject);
                    InvokeOnAmountOfSelectedItemsChangedEvent();
                }
                else if (CanRefreshSelection)
                {
                    itemModel.SetIsSelected(true);
                }
            }
            else
            {
                var canSelectThisItem = UniqueSelectedItemModels.Count <= MaxSelectedItemsAmount;

                if (value && canSelectThisItem)
                {
                    UnselectAllSelectedItemsSilently();
                    foreach (var item in itemsWithSameId)
                    {
                        AddItem(item);
                        item.SetIsSelected(true);
                    }
                    ExtraItemSelectionChangeAction?.Invoke(itemModel.RepresentedObject);
                    InvokeOnAmountOfSelectedItemsChangedEvent();
                }
            }
        }
    }
}