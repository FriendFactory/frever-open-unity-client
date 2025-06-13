using System;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.CameraSystem.CameraSystemCore;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public sealed class CharacterAssetSelector : BaseCharacterAssetSelector
    {
        private readonly CameraZoomOutHelper _zoomOutHelper;

        public override DbModelType AssetType => DbModelType.Character;
        
        public CharacterAssetSelector(Action<object> onCharacterSelectionChanged, 
                int minSelectedAmount, int maxSelectedAmount, ICameraSystem cameraSystem, PaginatedAssetSelectorParameters<CharactersPaginationLoader, CharacterInfo> assetSelectorParameters) : base(assetSelectorParameters)
        {
            _zoomOutHelper = new CameraZoomOutHelper(cameraSystem);
            var assetSelectionHandler = new MultipleItemAssetSelectionHandler(minSelectedAmount, maxSelectedAmount, false);
            assetSelectionHandler.SetExtraItemSelectionChangeAction(onCharacterSelectionChanged);
            SetAssetSelectionHandler(assetSelectionHandler); 
        }

        public override bool ShouldShowSpawnFormationPanel()
        {
            return true;
        }

        public override bool ShouldShowCharactersSelectionPanel()
        {
            return true;
        }

        public override void OnOpened()
        {
            base.OnOpened();
            
            _zoomOutHelper.ZoomOut();
        }

        public override void OnClosed()
        {
            base.OnClosed();
            
            _zoomOutHelper.ReturnToOriginalZoom();
        }
    }
}