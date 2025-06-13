using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using UnityEngine;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class CameraFilterAssetSelector : PaginatedAssetSelectorModel<CameraFilterPaginationLoader, CameraFilterInfo>
    {
        public override DbModelType AssetType => DbModelType.CameraFilter;
        
        public CameraFilterAssetSelector(PaginatedAssetSelectorParameters<CameraFilterPaginationLoader, CameraFilterInfo> parameters) : base(parameters)
        {
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(0, false);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public override List<AssetSelectionItemModel> GetItemsToShow(PaginationLoaderType type, long? categoryId = null, string filter = null)
        {
            if (type != PaginationLoaderType.Search)
            {
                return base.GetItemsToShow(type, categoryId, filter);
            }

            if (filter == null)
            {
                Debug.LogError("Searching for null string");
                return null;
            }
            
            var regex = new Regex(string.Format(ASSET_SEARCH_REGEXP_FORMAT, Regex.Escape(filter)), 
                                  RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return GetItemsToShow().Where(itm => (regex.IsMatch(itm.DisplayName) 
                                               || itm.RepresentedObject is CameraFilterInfo info 
                                               && info.CameraFilterVariants.Any(variant => regex.IsMatch(variant.Name))) 
                                              && itm.ItemIndices.ContainsKey(filter))
                                   .OrderBy(itm => itm.ItemIndices[filter]).ToList();
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
                SetSelectedItems(silent: silent);
                return;
            }

            var activeFilter = filterController.CameraFilter;

            if (activeFilter == null)
            {
                SetSelectedItems(silent: silent);
                return;
            }

            if (dataFetcher != null)
            {
                var assetData = dataFetcher.DefaultUserAssets.PurchasedAssetsData;
                var purchasedIds = assetData.GetPurchasedAssetsByType(DbModelType.CameraFilter);
            
                AddItems(new[]
                {
                    new AssetSelectionCameraFilterModel(
                        Resolution._128x128, 
                        activeFilter, 
                        dataFetcher.MetadataStartPack.CameraFilterCategories
                                   .First(x => x.Id ==
                                               activeFilter.CameraFilterCategoryId)
                                   .Name)
                    {
                        IsPurchased = purchasedIds.Contains(activeFilter.Id)
                    }
                });
            }

            SetSelectedItems(new[] { activeFilter.Id }, new[] { activeFilter.CameraFilterCategoryId }, silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return (AssetSelectionHandler.SelectedModels.Count == 0 ? (long?)null : AssetSelectionHandler.SelectedModels[0].ItemId) == @event.GetCameraFilter()?.Id;
        }
    }
}
