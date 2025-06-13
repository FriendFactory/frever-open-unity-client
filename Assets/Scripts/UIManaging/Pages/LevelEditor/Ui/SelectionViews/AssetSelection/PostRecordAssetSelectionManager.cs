using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal class PostRecordAssetSelectionManager : AssetSelectionViewManager
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(PostRecordEditorPageModel editorPageModel)
        {
            EditorPageModel = editorPageModel;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            var contextData = AssetSelectorView.ContextData;
            var isPurchasable = false;
            
            switch (contextData)
            {
                case BodyAnimationAssetSelector _:
                case SetLocationAssetSelector _:
                case VfxAssetSelector _:
                case CameraFilterAssetSelector _:
                    isPurchasable = true;
                    break;
            }
            
            ((PostRecordEditorPageModel) EditorPageModel).OnAssetSelectionViewOpened(isPurchasable);
        }

        protected override void Close()
        {
            base.Close();
            ((PostRecordEditorPageModel) EditorPageModel).OnAssetSelectionViewClosed();
        }
    }
}