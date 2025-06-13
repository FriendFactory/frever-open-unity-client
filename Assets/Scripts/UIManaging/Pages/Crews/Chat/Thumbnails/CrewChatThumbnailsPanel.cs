using System;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    internal class CrewChatThumbnailsPanel : MonoBehaviour
    {
        [SerializeField] private CrewChatThumbnailItem _photoThumbnail;
        [SerializeField] private CrewChatThumbnailItem _videoThumbnail;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        internal Action<bool> PanelShown;

        internal Action PhotoThumbnailRemoved;
        internal Action VideoThumbnailRemoved;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void AddPhotoThumbnail(Texture2D thumbnail)
        {
            this.SetActive(true);
            _photoThumbnail.RemoveButton.onClick.AddListener(RemovePhotoThumbnail);
            _photoThumbnail.UpdateThumbnail(thumbnail);
            _photoThumbnail.SetActive(true);
            PanelShown?.Invoke(true);
        }

        public void RemovePhotoThumbnail()
        {
            _photoThumbnail.SetActive(false);
            _photoThumbnail.RemoveButton.onClick.RemoveListener(RemovePhotoThumbnail);
            _photoThumbnail.DestroyThumbnailTexture();

            PhotoThumbnailRemoved?.Invoke();

            if (_videoThumbnail.gameObject.activeSelf) return;

            this.SetActive(false);
            PanelShown?.Invoke(false);
        }

        public void AddVideoThumbnail(Texture2D thumbnail)
        {
            this.SetActive(true);
            _videoThumbnail.RemoveButton.onClick.AddListener(RemoveVideoThumbnail);
            _videoThumbnail.UpdateThumbnail(thumbnail);
            _videoThumbnail.SetActive(true);
            PanelShown?.Invoke(true);
        }

        public void RemoveVideoThumbnail()
        {
            _videoThumbnail.SetActive(false);
            _videoThumbnail.RemoveButton.onClick.RemoveListener(RemoveVideoThumbnail);
            _videoThumbnail.DestroyThumbnailTexture();

            VideoThumbnailRemoved?.Invoke();

            if (_photoThumbnail.gameObject.activeSelf) return;

            this.SetActive(false);
            PanelShown?.Invoke(false);
        }

        public void RemoveAllThumbnails()
        {
            RemovePhotoThumbnail();
            RemoveVideoThumbnail();
        }
    }
}