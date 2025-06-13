using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews
{
    internal sealed class PhotoPreviewPanel : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private AspectRatioFitter _aspectFitter;
        [SerializeField] private Button _closeButton;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            _image.texture = null;
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(Texture2D photo)
        {
            _aspectFitter.aspectRatio = photo.width / (float) photo.height;
            _image.texture = photo;
            this.SetActive(true);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCloseButtonClicked()
        {
            this.SetActive(false);
        }
    }
}