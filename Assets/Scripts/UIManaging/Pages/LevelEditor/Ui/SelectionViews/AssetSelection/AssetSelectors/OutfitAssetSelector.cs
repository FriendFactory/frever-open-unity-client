using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Localization;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    internal sealed class OutfitAssetSelector : MainAssetSelectorModel
    {
        [Inject] private UmaEditorCategoriesLocalization _categoriesLocalization;
        
        private readonly bool _shouldShowCreateNewOutfitButton;
        public override DbModelType AssetType => DbModelType.Outfit;

        public override string NoAvailableAssetsMessage => _localization.OutfitSearchEmptyPlaceholder;

        public OutfitAssetSelector(string displayName, bool shouldShowCreateNewOutfitButton) : base(displayName)
        {
            _shouldShowCreateNewOutfitButton = shouldShowCreateNewOutfitButton;
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(0, false);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        protected override TabModel[] GetTabs()
        {
            return new[]
            {
                new TabModel(0, _categoriesLocalization.FavouriteOutfitsTab),
                new TabModel(1, _categoriesLocalization.RecentOutfitsTab)
            };
        }

        public override bool IsSearchable()
        {
            return false;
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

            var outfitIds = new List<long>();

            if (levelManager.EditingCharacterSequenceNumber < 0)
            {
                SetSelectedItems(GetSameSelectedOutfitForAllCharacters(@event), silent: silent);
                return;
            }

            var controller = @event?.GetCharacterController(levelManager.EditingCharacterSequenceNumber);
            if (controller?.Outfit != null)
            {
                outfitIds.Add(controller.Outfit.Id);
            }

            SetSelectedItems(outfitIds.ToArray(), silent: silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return (AssetSelectionHandler.SelectedModels.Count == 0 
                    ? (long?)null 
                    : AssetSelectionHandler.SelectedModels[0].ItemId) 
                == @event.GetCharacterController(levelManager.EditingCharacterSequenceNumber)?.OutfitId;
        }

        public override bool ShouldShowCreateNewOutfitPanel()
        {
            return _shouldShowCreateNewOutfitButton;
        }

        private long[] GetSameSelectedOutfitForAllCharacters(Event @event)
        {
            var outfitIds = @event.GetOutfitIds();
            var hasSameOnAllCharacters = outfitIds.Length > 1 && outfitIds.Distinct().Count() == 1;
            return hasSameOnAllCharacters ? outfitIds : Array.Empty<long>();
        }
    }
}