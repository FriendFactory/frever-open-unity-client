using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal sealed class AssetCameraSelectorsHolder : AssetSelectorsHolder
    {
        private readonly CameraAnimationGenerator _cameraAnimationGenerator;

        public AssetCameraSelectorsHolder(MainAssetSelectorModel[] managerModels,
            CameraAnimationGenerator cameraAnimationGenerator) : base(managerModels)
        {
            _cameraAnimationGenerator = cameraAnimationGenerator;
        }

        public override void OnTabSelected(AssetSelectorView assetSelectorView, int tabIndex)
        {
            if (tabIndex != 0) return;
            if (assetSelectorView.ContextData == null)
            {
                assetSelectorView.Initialize(CurrentManagerModel);
            }
        }
    }
}