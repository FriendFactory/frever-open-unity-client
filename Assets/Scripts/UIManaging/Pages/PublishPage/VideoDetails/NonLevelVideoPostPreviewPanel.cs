using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal sealed class NonLevelVideoPostPreviewPanel: BasePostVideoPreviewPanel<NonLevelVideoPostPreviewPanelModel>
    {
        [SerializeField] private Button _previewButton;
        [SerializeField] private RawImage _rawImage;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _previewButton.onClick.AddListener(OnPreviewClicked);
            
            _rawImage.texture = ContextData.VideoThumbnail;
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _previewButton.onClick.RemoveListener(OnPreviewClicked);
            
            _rawImage.texture = null;
            
            Destroy(ContextData.VideoThumbnail);
        }

        private void OnPreviewClicked()
        {
            ContextData.OnPreviewRequested?.Invoke();
        }
    }
}