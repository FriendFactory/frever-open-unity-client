using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class CharacterAssetSwitchSelector : BaseCharacterAssetSelector
    {
        public override DbModelType AssetType => DbModelType.Character;
        
        public CharacterAssetSwitchSelector(Action<IEntity> onCharacterSelectionChanged, PaginatedAssetSelectorParameters<CharactersPaginationLoader, CharacterInfo> assetSelectorParameters) : base(assetSelectorParameters)
        {        
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(1, false);
            assetSelectionHandler.SetExtraItemSelectionChangeAction(onCharacterSelectionChanged);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public override bool ShouldShowSpawnFormationPanel()
        {
            return true;
        }

        public override bool ShouldShowCharactersSelectionPanel()
        {
            return false;
        }

        public override bool ShouldShowCharactersSwitchablePanel()
        {
            return true;
        }
    }
}
