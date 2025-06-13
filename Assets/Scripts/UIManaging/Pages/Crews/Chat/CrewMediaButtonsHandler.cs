using System;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    public class CrewMediaButtonsHandler : MonoBehaviour
    {
        [SerializeField] private Button _arrowButton;
        [SerializeField] private Button _photoButton;
        [SerializeField] private Button _videoButton;
        [Space]
        [SerializeField] private MediaButtonsAnimator _animator;
        [Space]
        [SerializeField] private CrewChatVideoSelectionPanel _videoSelectionPanel;

        [Inject] private INativeGallery _nativeGallery;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        internal Action MediaSelectionStarted;
        internal Action<string, Texture2D> PhotoSelected;
        internal Action<Video, Texture2D> VideoSelected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _arrowButton.onClick.AddListener(OnArrowButtonClick);
            _photoButton.onClick.AddListener(OnPhotoButtonClick);
            _videoButton.onClick.AddListener(OnVideoButtonClick);
        }

        private void OnDisable()
        {
            _arrowButton.onClick.RemoveListener(OnArrowButtonClick);
            _photoButton.onClick.RemoveListener(OnPhotoButtonClick);
            _videoButton.onClick.RemoveListener(OnVideoButtonClick);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ShowMediaButtons()
        {
            _animator.ShowMediaButtons();
        }

        public void HideMediaButtons()
        {
            _animator.HideMediaButtons();
        }

        public void HideVideoPanel() => _videoSelectionPanel.Hide();

        //---------------------------------------------------------------------
        // Handlers
        //---------------------------------------------------------------------

        private void OnArrowButtonClick()
        {
            _animator.ShowMediaButtons();
        }

        private void OnPhotoButtonClick()
        {
            _nativeGallery.GetMixedMediaFromGallery(OnPhotoSelected, NativeGallery.MediaType.Image);
            MediaSelectionStarted?.Invoke();

            void OnPhotoSelected(string path)
            {
                if (string.IsNullOrEmpty(path)) return;
                
                var thumbnail = _nativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                PhotoSelected?.Invoke(path, thumbnail);
            }
        }

        private void OnVideoButtonClick()
        {
            _videoSelectionPanel.Show(OnVideoSelected);
            MediaSelectionStarted?.Invoke();

            void OnVideoSelected(Video video, Texture2D thumbnail)
            {
                VideoSelected?.Invoke(video, thumbnail);
            }
        }
    }
}