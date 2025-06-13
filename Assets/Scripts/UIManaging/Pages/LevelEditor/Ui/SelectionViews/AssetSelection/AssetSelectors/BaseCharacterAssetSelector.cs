using Bridge.Models.ClientServer.Assets;
using Common;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public abstract class BaseCharacterAssetSelector : PaginatedAssetSelectorModel<CharactersPaginationLoader, CharacterInfo>
    {
        public override string NoAvailableAssetsMessage =>
            TabsManagerArgs.SelectedTabIndex == Constants.FRIENDS_TAB_INDEX 
                ? _localization.FriendCharacterSearchEmptyPlaceholder
                : _localization.AssetSearchEmptyPlaceholder;

        protected BaseCharacterAssetSelector(PaginatedAssetSelectorParameters<CharactersPaginationLoader, CharacterInfo> assetSelectorParameters) 
            : base(assetSelectorParameters) { }

        protected override TabModel[] GetTabs()
        {
            var tabModels = new[]
            {
                new TabModel(Constants.MY_FREVERS_TAB_INDEX, "My"),
                new TabModel(Constants.FRIENDS_TAB_INDEX, "Friends"),
                new TabModel(Constants.STAR_CREATORS_TAB_INDEX, "Icons")
            };
            
            return tabModels;
        }
        
        public override bool ShouldShowRevertButton()
        {
            return false;
        }
    }
}