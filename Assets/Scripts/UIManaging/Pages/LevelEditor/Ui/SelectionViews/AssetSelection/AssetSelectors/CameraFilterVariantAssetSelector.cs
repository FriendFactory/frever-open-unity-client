using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common.Files;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class CameraFilterVariantAssetSelector : AssetSelectorModel
    {
        private long _currentCameraFilterId;
    
        public override DbModelType AssetType => DbModelType.CameraFilterVariant;
        
        public CameraFilterVariantAssetSelector(string displayName)
            : base(displayName)
        {
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(0, true);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public void ShowNoItems()
        {
            var emptyItemsArray = new List<AssetSelectionItemModel>();
            GridSpawnerModel.SetItems(emptyItemsArray);
            AssetSelectionHandler.UnselectAllSelectedItems();
        }

        public void SetCurrentCameraFilterId(long cameraFilterID)
        {
            _currentCameraFilterId = cameraFilterID;
            GridSpawnerModel.SetItems(GetItemsToShow());
            UnlockItems();
        }

        public override List<AssetSelectionItemModel> GetItemsToShow()
        {
            var filteredItems = base.GetItemsToShow().Where(itm => itm.ParentAssetId == _currentCameraFilterId)
                                        .OrderBy(itm => itm.ItemIndices.ContainsKey("CameraFilterId:" + _currentCameraFilterId) 
                                                ? itm.ItemIndices["CameraFilterId:" + _currentCameraFilterId]
                                                : -1)
                                        .ToList();
            return filteredItems;
        }

        public override void SetSelectedItemsAsInEvent(ILevelManager levelManager, Event @event,
            IDataFetcher dataFetcher = null,
            bool silent = true)
        {
            base.SetSelectedItemsAsInEvent(levelManager, @event, dataFetcher, silent);

            if (!silent)
            {
                AssetSelectionHandler.UnselectAllSelectedItems();
            }
        
            var filterController = @event.GetCameraFilterController();

            if (filterController == null)
            {
                SetCurrentCameraFilterId(-1);
                SetSelectedItems(silent: silent);
                return;
            }

            var filter = filterController.CameraFilter;
            var filterVariant = filterController.CameraFilterVariant;

            if (dataFetcher != null)
            {
                AddItems(new[]
                {
                    new AssetSelectionCameraFilterVariantModel(0, Resolution._128x128, filterVariant, 
                                                               filter.CameraFilterCategoryId, filter, 
                                                               dataFetcher.MetadataStartPack.CameraFilterCategories
                                                                  .First(x => x.Id == filter.CameraFilterCategoryId)
                                                                  .Name)
                });
            }

            SetCurrentCameraFilterId(filter.Id);
            SetSelectedItems(new[] { filterVariant.Id }, silent: silent);
        }
        
        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return AssetSelectionHandler.SelectedModels.Count > 0 && AssetSelectionHandler.SelectedModels[0].ItemId == @event.GetCameraFilterVariant().Id;
        }
    }
}
