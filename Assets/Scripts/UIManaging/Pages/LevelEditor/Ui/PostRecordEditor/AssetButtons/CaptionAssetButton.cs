namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class CaptionAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.TryToOpenNewCaptionAddingPanel();
        }
    }
}