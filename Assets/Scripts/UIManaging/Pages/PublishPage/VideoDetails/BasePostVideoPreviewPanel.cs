using Common.Abstract;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal abstract class BasePostVideoPreviewPanel<TModel>: BaseContextPanel<TModel> where TModel: BasePostVideoPreviewPanelModel
    {
        protected override void OnInitialized()
        {
        }
    }
}