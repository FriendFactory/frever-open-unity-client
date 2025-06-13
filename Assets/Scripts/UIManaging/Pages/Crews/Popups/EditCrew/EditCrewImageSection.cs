using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using UIManaging.Animated.Behaviours;
using UIManaging.Localization;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using FileInfo = Bridge.Models.Common.Files.FileInfo;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    public class EditCrewImageSection : BaseContextDataView<IThumbnailOwner>
    {
        [SerializeField] private Button _uploadButton;
        
        [Space]
        [SerializeField] private CrewBackgroundView _backgroundView;
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonBehaviour;
        [SerializeField] private FileUploadWidget _fileUploadWidget;

        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewPageLocalization _localization;

        public event Action<List<FileInfo>> ImageChangeRequested;

        private void OnEnable()
        {
            _uploadButton.onClick.AddListener(OnUploadButtonClicked);
            _fileUploadWidget.OnPhotoConversionSuccess += MediaConversionFinished;
            _fileUploadWidget.OnMediaConversionError += HandleMediaConversionFailure;
        }

        private void OnDisable()
        {
            _uploadButton.onClick.RemoveAllListeners();
            _fileUploadWidget.OnPhotoConversionSuccess -= MediaConversionFinished;
            _fileUploadWidget.OnMediaConversionError -= HandleMediaConversionFailure;
        }

        protected override void OnInitialized()
        {
            _backgroundView.Initialize(ContextData);
            _fileUploadWidget.MediaType = NativeGallery.MediaType.Image;
        }

        private async void OnUploadButtonClicked()
        {
            _backgroundView.CancelLoading();
            await _bridge.CleanCacheFromConvertedFilesAsync();
            _fileUploadWidget.ShowGallery();
            _skeletonBehaviour.SetActive(true);
            _skeletonBehaviour.Play();
        }

        private void MediaConversionFinished(PhotoFullInfo photoFullInfo)
        {
            _skeletonBehaviour.FadeOut();
            var config = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.ConfirmCrewBackgroundChange,
                Title = _localization.UpdatePhotoPopupTitle,
                Description =_localization.UpdatePhotoPopupDescription,
                YesButtonSetTextColorRed = false,
                YesButtonText = _localization.UpdatePhotoPopupConfirmButton,
                OnYes = () => OnBackgroundChangeConfirmed(photoFullInfo),
                NoButtonText = _localization.UpdatePhotoPopupCancelButton,
                OnNo = OnBackgroundChangeCanceled,
            };
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, true);
        }

        private void HandleMediaConversionFailure(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message);
        }

        private void OnBackgroundChangeCanceled()
        {
            _bridge.CleanCacheFromConvertedFilesAsync();
            _skeletonBehaviour.SetActive(true);
            _skeletonBehaviour.Play();
            _backgroundView.Refresh();
        }

        private void OnBackgroundChangeConfirmed(PhotoFullInfo photoFullInfo)
        {
            ImageChangeRequested?.Invoke(photoFullInfo.Files);
        }
    }
}