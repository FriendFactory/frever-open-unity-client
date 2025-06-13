using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using UnityEngine;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    internal sealed class VfxAssetSelector : PaginatedAssetSelectorModel<VfxPaginationLoader, VfxInfo>
    {
        public override DbModelType AssetType => DbModelType.Vfx;
        
        public VfxAssetSelector(PaginatedAssetSelectorParameters<VfxPaginationLoader, VfxInfo> parameters) : base(parameters)
        {
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(0, false);
            SetAssetSelectionHandler(assetSelectionHandler);
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
        
            var activeVfx = @event?.GetVfx();

            if (activeVfx == null)
            {
                SetSelectedItems(silent: silent);
                return;
            }

            if (dataFetcher != null)
            {
                AddItems(new[]
                {
                    new AssetSelectionVfxModel(Resolution._128x128, activeVfx,
                                               dataFetcher.MetadataStartPack.VfxCategories
                                                        .First(x => x.Id == activeVfx.VfxCategoryId).Name)
                });
            }

            SetSelectedItems(new[] { activeVfx.Id }, new[] { activeVfx.VfxCategoryId }, silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return (AssetSelectionHandler.SelectedModels.Count == 0 ? (long?)null : AssetSelectionHandler.SelectedModels[0].ItemId) == @event.GetVfx()?.Id;
        }
    }
}