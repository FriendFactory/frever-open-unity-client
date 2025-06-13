using System.Linq;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers
{
    public class MultipleItemAssetSelectionHandler : BaseAssetSelectionHandler
    {
        public MultipleItemAssetSelectionHandler(int minSelectedItemsAmount, int maxSelectedItemsAmount, bool canRefreshSelection) : base(minSelectedItemsAmount, maxSelectedItemsAmount, canRefreshSelection) {}
    
        public override void ChangeItemSelection(AssetSelectionItemModel itemModel, bool value)
        {
            var isItemAlreadySelected = IsItemAlreadySelected(itemModel.ItemId, itemModel.CategoryId);
            var itemsWithSameId = AllItems.Where(x => x.ItemId == itemModel.ItemId && x.CategoryId == itemModel.CategoryId);

            if (isItemAlreadySelected)
            {
                var canUnselectThisItem = UniqueSelectedItemModels.Count - 1 >= MinSelectedItemsAmount;
            
                if (!value && canUnselectThisItem)
                {
                    foreach (var item in itemsWithSameId)
                    {
                        item.SetIsSelected(false);
                        RemoveItem(item);
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
                var canSelectThisItem = UniqueSelectedItemModels.Count + 1 <= MaxSelectedItemsAmount;
            
                if (value && canSelectThisItem)
                {
                    foreach (var item in itemsWithSameId)
                    {
                        item.SetIsSelected(true);
                        AddItem(item);
                    }
                    ExtraItemSelectionChangeAction?.Invoke(itemModel.RepresentedObject);
                    InvokeOnAmountOfSelectedItemsChangedEvent();
                }
            }
        }
    }
}