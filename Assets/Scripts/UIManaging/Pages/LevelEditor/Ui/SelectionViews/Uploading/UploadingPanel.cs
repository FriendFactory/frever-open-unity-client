using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.EditingPage;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading
{
    internal sealed class UploadingPanel : MonoBehaviour
    {
        private const float CONVERTING_TIMEOUT = 60f;
        
        [FormerlySerializedAs("_uploadButton")] 
        [SerializeField] private FileUploadWidget _fileUploadWidget;
        [SerializeField] private EditingPageLoading _loadingScreen;
        
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private ILevelManager _levelManager;

        private PhotoFullInfo CurrentPhoto => TargetEvent.GetPhoto();
        private VideoClipFullInfo CurrentVideo => TargetEvent.GetVideo();
        private Event TargetEvent => _levelManager.TargetEvent;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void Awake()
        {
            //_fileUploadWidget.MediaType = NativeGallery.MediaType.Image | NativeGallery.MediaType.Video;
            _fileUploadWidget.MediaConversionStarted += OnMediaSelected;
            _fileUploadWidget.OnPhotoConversionSuccess += OnPhotoConverted;
            _fileUploadWidget.OnVideoConversionSuccess += OnVideoConverted;
            _fileUploadWidget.OnMediaConversionError += OnConversionError;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void RefreshState(bool autoOpenGallery)
        {
            var setLocation = _levelManager.TargetEvent.GetSetLocation();
            NativeGallery.MediaType mediaTypeInt = 0;
            if (setLocation.AllowPhoto) mediaTypeInt |= NativeGallery.MediaType.Image;
            if (setLocation.AllowVideo) mediaTypeInt |= NativeGallery.MediaType.Video;

            if ((int)mediaTypeInt <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            _fileUploadWidget.MediaType = mediaTypeInt;

            OpenGalleryIfNeeded(autoOpenGallery, setLocation.PhotoAutoPick);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        internal void OnSetLocationChanged(ISetLocationAsset setLocationAsset)
        {
            RefreshState(true);
        }
        
        private void OpenGalleryIfNeeded(bool autoOpenGallery, bool promptGalleryOnStart)
        {
            if (autoOpenGallery && promptGalleryOnStart && CurrentPhoto == null && CurrentVideo == null)
            {
                _fileUploadWidget.ShowGallery();
            }
        }

        private void OnMediaSelected()
        {
            _loadingScreen.Show(CONVERTING_TIMEOUT);
        }
        
        private void OnPhotoConverted(PhotoFullInfo photo)
        {
            _levelManager.ApplySetLocationBackground(photo);
            _loadingScreen.Hide();
        }
        
        private void OnVideoConverted(VideoClipFullInfo video)
        {
            _levelManager.ApplySetLocationBackground(video);
            _loadingScreen.Hide();
        }
        
        private void OnConversionError(string error)
        {
            _loadingScreen.Hide();
            _snackBarHelper.ShowInformationSnackBar(string.IsNullOrEmpty(error) 
                                                        ? Constants.ErrorMessage.WRONG_FILE_FORMAT
                                                        : error);

        }
    }
}