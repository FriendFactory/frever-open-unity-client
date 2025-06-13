using Models;
using System.Threading;
using TMPro;
using UIManaging.Common;
using UnityEngine;

namespace UIManaging.Pages.DraftsPage
{
    public sealed class DraftItem : VideoListItem
    {
        private const string DATE_FORMAT = "dd/MM HH:mm";

        [SerializeField] private TextMeshProUGUI _createdDateText;
        [SerializeField] private GameObject _createdDateParentGameObject;

        [SerializeField] private LevelThumbnail _levelThumbnail;
        [SerializeField] private GameObject _galleryVideoIconGameObject;
        [SerializeField] private GameObject _selectionIndicator;

        private CancellationTokenSource _profileCancellationSource;
        private bool _isSelected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        protected override void OnDisable()
        {
            base.OnDisable();
            _profileCancellationSource?.Cancel();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            _selectionIndicator.SetActive(isSelected);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        protected override void OnInitialized()
        {
            var args = ContextData;
            _createdDateParentGameObject.SetActive(args.ShowCreationDate);

            var showGalleryVideoIcon = args.Video?.LevelId == null && args.Level == null;
            _galleryVideoIconGameObject.SetActive(showGalleryVideoIcon);

            if (args.ShowCreationDate)
            {
                _createdDateText.text = args.Level.CreatedTime.ToString(DATE_FORMAT);
            }

            RefreshButtons();

            base.OnInitialized();
        }

        protected override void RefreshThumbnail()
        {
            var args = ContextData;
            var hasVideoModel = args.Video != null;

            _videoThumbnail.gameObject.SetActive(hasVideoModel);
            _levelThumbnail.gameObject.SetActive(!hasVideoModel);

            if (hasVideoModel)
            {
                SwitchContentIsNotAvailableOverlay(false);
                _videoThumbnail.Initialize(ContextData);
                _levelThumbnail.CleanUp();
            }
            else
            {
                SwitchContentIsNotAvailableOverlay(false);
                _levelThumbnail.Initialize(args.Level);
            }
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        private void RefreshButtons()
        {
            var args = ContextData;

            _button.gameObject.SetActive(args.IsInteractable);
        }
    }
}