using System.Linq;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal class AssetSelectorsHolder
    {
        protected readonly MainAssetSelectorModel CurrentManagerModel;

        private readonly MainAssetSelectorModel[] _managerModels;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public TabsManagerArgs TabsManagerArgs { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public AssetSelectorsHolder(MainAssetSelectorModel[] managerModels)
        {
            _managerModels = managerModels;
            CurrentManagerModel = _managerModels[0];

            var tabModels = new TabModel[_managerModels.Length];
        
            for (var i = 0; i < _managerModels.Length; i++)
            {
                tabModels[i] = new TabModel(i, _managerModels[i].DisplayName);
            }
        
            TabsManagerArgs = new TabsManagerArgs(tabModels, tabModels.First().Index);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void OnTabSelected(AssetSelectorView assetSelectorView, int tabIndex)
        {
            assetSelectorView.Initialize(_managerModels[tabIndex]);
        }
    }
}