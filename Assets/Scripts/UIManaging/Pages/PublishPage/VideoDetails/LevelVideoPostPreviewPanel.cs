using UIManaging.Common;
using UIManaging.Pages.PublishPage.Buttons;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal sealed class LevelVideoPostPreviewPanel: BasePostVideoPreviewPanel<LevelVideoPostPreviewPanelModel>
    {
        [SerializeField] private LevelThumbnail _levelThumbnail;
        [SerializeField] private PreviewLevelButton _previewButton;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _previewButton.SetCallback(ContextData.OnPreviewRequested);

            if (_levelThumbnail.IsInitialized) return;
            
            _levelThumbnail.Initialize(ContextData.Level);
        }
    }
}