using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using UnityEngine;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public sealed class SetLocationAssetSelector : PaginatedAssetSelectorModel<SetLocationPaginationLoader, SetLocationFullInfo>
    {
        private const string RECOMMENDED_CATEGORY_NAME = "Recommended";
        
        private readonly bool _allowPhoto;
        private readonly bool _allowVideo;
        
        public override DbModelType AssetType => DbModelType.SetLocation;

        public SetLocationAssetSelector(PaginatedAssetSelectorParameters<SetLocationPaginationLoader, SetLocationFullInfo> parameters, 
            bool allowPhoto = true, bool allowVideo = true) : base(parameters)
        {
            _allowPhoto = allowPhoto;
            _allowVideo = allowVideo;
            GridSpawnerModel = new EnhancedScrollerGridSpawnerModel<AssetSelectionItemModel>(AllItems);
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(1, false);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public override bool ShouldShowRevertButton()
        {
            return false;
        }

        public override bool ShouldShowUploadingPanel(SetLocationFullInfo setLocation)
        {
            return setLocation != null && (_allowPhoto && setLocation.AllowPhoto || _allowVideo && setLocation.AllowVideo);
        }

        public override bool ShouldShowTimeOfDayJogWheel(ISetLocationAsset setLocation)
        {
            return setLocation?.DayNightController != null;
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
                                               || itm.RepresentedObject is SetLocationFullInfo info 
                                               && info.CharacterSpawnPosition.Any(variant => regex.IsMatch(variant.Name))) 
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
        
            var activeSetLocation = @event.GetSetLocation();

            if (dataFetcher != null)
            {
                var assetData = dataFetcher.DefaultUserAssets.PurchasedAssetsData;
                var purchasedIds = assetData.GetPurchasedAssetsByType(DbModelType.SetLocation);

                AddItems(new[]
                {
                    new AssetSelectionSetLocationModel(
                        Resolution._128x128,
                        activeSetLocation,
                        dataFetcher.MetadataStartPack.SetLocationCategories
                                   .First(x => x.Id ==
                                               activeSetLocation.CategoryId).Name)
                    {
                        IsPurchased = purchasedIds.Contains(activeSetLocation.Id)
                    }
                });
            }

            SetSelectedItems(new[] { activeSetLocation.Id }, 
                                   new[] { activeSetLocation.CategoryId }, silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return AssetSelectionHandler.SelectedModels.Count > 0 && AssetSelectionHandler.SelectedModels[0].ItemId == @event.GetSetLocationId();
        }

        public override void SetTabToSelection(bool allowAutoSwitchToRecommended = false)
        {
            var recommendedTab =
                TabsManagerArgs.Tabs.FirstOrDefault(item => item.Name == RECOMMENDED_CATEGORY_NAME);
            if (recommendedTab != null)
            {
                TabsManagerArgs.SetSelectedTabIndex(recommendedTab.Index);
                return;
            }
            
            base.SetTabToSelection(allowAutoSwitchToRecommended);
        }
    }
}
