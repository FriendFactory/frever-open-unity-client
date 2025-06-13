using Navigation.Args;
using System.Collections;
using System.Collections.Generic;
using UIManaging.Common;
using UIManaging.Common.Args.Views.LevelPreviews;
using UIManaging.EnhancedScrollerComponents;
using UnityEngine;

namespace UIManaging.Pages.DraftsPage
{
    internal class DraftsRow : LevelPreviewItemsRow
    {
        public BaseLevelItemArgs SelectedItem;

        public void UpdateSelectedItem()
        {
            foreach (var item in Views)
            {
                var draftItem = item as DraftItem;
                draftItem.SetSelected(item.ContextData == SelectedItem);
            }
        }

        protected override void AfterSetup(BaseLevelItemArgs[] itemsModels)
        {
            base.AfterSetup(itemsModels);
            UpdateSelectedItem();
        }
    }
}