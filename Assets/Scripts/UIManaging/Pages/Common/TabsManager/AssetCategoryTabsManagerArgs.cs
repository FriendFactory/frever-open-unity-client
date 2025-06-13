using System;
using Extensions;

namespace UIManaging.Pages.Common.TabsManager
{
    public sealed class AssetCategoryTabsManagerArgs: TabsManagerArgs
    {
        public const int MY_ASSETS_TAB_INDEX = -1;
        public const int RECOMMENDED_TAB_INDEX = -2;
        public const string MY_ASSETS_TAB_NAME = "My Assets";
        public const string RECOMMENDED_TAB_NAME = "Suggested";
        
        public bool ShowMyAssetsCategory { get; }
        public bool ShowRecommendedCategory => _showRecommendedCategoryFunc.Invoke();

        private readonly Func<bool> _showRecommendedCategoryFunc;

        public AssetCategoryTabsManagerArgs(TabModel[] tabs, bool showMyAssetsCategory, Func<bool> showRecommendedCategoryFunc) : base(tabs)
        {
            ShowMyAssetsCategory = showMyAssetsCategory;
            _showRecommendedCategoryFunc = showRecommendedCategoryFunc;
        }

        public AssetCategoryTabsManagerArgs(TabModel[] tabs, int initialTabIndex, bool showMyAssetsCategory, Func<bool> showRecommendedCategoryFunc) 
            : base(tabs, initialTabIndex)
        {
            ShowMyAssetsCategory = showMyAssetsCategory;
            _showRecommendedCategoryFunc = showRecommendedCategoryFunc;
        }
    }
}