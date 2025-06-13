using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class SpawnPositionAssetSelector : AssetSelectorModel
    {
        private long _currentSetLocationId;
        
        public override DbModelType AssetType => DbModelType.CharacterSpawnPosition;

        public SpawnPositionAssetSelector(string displayName) : base(displayName)
        {
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(1, false);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public void SetCurrentSetLocationId(long setLocationId)
        {
            if (_currentSetLocationId == setLocationId)
            {
                return;
            }
            
            _currentSetLocationId = setLocationId;
            GridSpawnerModel.SetItems(GetItemsToShow());
        }
        
        public override List<AssetSelectionItemModel> GetItemsToShow()
        {
            var filteredItems = base.GetItemsToShow()
                                    .Where(itm => itm.ParentAssetId == _currentSetLocationId && ((CharacterSpawnPositionInfo)itm.RepresentedObject).AvailableForSelection)
                                    .OrderBy(itm => itm.ItemIndices.ContainsKey("SetLocationId:" + _currentSetLocationId) 
                                                 ? itm.ItemIndices["SetLocationId:" + _currentSetLocationId]
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
        
            var activeSetLocation = @event.GetSetLocation();
            
            if (dataFetcher != null)
            {
                var models = activeSetLocation.CharacterSpawnPosition.Select(
                    spawnPos => new AssetSelectionSpawnPositionModel(0, Resolution._128x128, spawnPos, activeSetLocation.CategoryId, activeSetLocation.Id, 
                                                                     activeSetLocation.Name));
                AddItems(models);
            }
            
            SetCurrentSetLocationId(activeSetLocation.Id);
            var currentSpawnPosition = activeSetLocation.CharacterSpawnPosition.First(pos => pos.Id == @event.CharacterSpawnPositionId);
            SetSelectedItems(new[] { currentSpawnPosition.Id }, silent: silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return AssetSelectionHandler.SelectedModels.Count > 0 && AssetSelectionHandler.SelectedModels[0].ItemId == @event.CharacterSpawnPositionId;
        }
    }
}