using System;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AssetUIManagers
{
    internal abstract class BaseAssetUIHandler
    {
        public AssetSelectorsHolder AssetSelectorsHolder { get; protected set; }
    }
}
