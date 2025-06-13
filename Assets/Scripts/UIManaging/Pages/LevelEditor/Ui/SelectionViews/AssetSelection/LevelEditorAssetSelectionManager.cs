using JetBrains.Annotations;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal class LevelEditorAssetSelectionManager : AssetSelectionViewManager
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(LevelEditorPageModel editorPageModel)
        {
            EditorPageModel = editorPageModel;
        }
    }
}