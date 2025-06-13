using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Common.UploadingPopups
{
    internal sealed class VideoUploadingPopup : SlideInLeftPopup<VideoUploadingPopupConfiguration>
    {
        [SerializeField] private GameObject _loadingSpinner;

        public override bool NonBlockingQueue => true;

        public override void Show()
        {
            base.Show();
            _loadingSpinner.SetActive(true);
        }

        public override void Hide(object result)
        {
            StartCoroutine(DelayedHideRoutine());
        }
        
        protected override void OnConfigure(VideoUploadingPopupConfiguration configuration)
        {
            
        }
    }
}
