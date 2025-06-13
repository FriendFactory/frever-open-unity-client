using System;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal class BasePostVideoPreviewPanelModel
    {
        public Action OnPreviewRequested { get; }

        public BasePostVideoPreviewPanelModel(Action onPreviewRequested)
        {
            OnPreviewRequested = onPreviewRequested;
        }
    }
}