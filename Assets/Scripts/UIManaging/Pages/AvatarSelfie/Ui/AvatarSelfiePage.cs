using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Services.SelfieAvatar;
using Common.Permissions;
using JetBrains.Annotations;
using UnityEngine;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine.UI;
using Zenject;
using Modules.FaceAndVoice.Face.Facade;
using Navigation.Args;
using Navigation.Core;
using UIManaging.SnackBarSystem;

namespace UIManaging.Pages.AvatarSelfie.Ui
{
    internal sealed class AvatarSelfiePage : GenericPage<AvatarSelfieArgs>
    {
        private const string UNCENTERED_FACE_ERROR = "Face or Hair Images Invalid";
        
        [SerializeField] private Button _backButton;
        [SerializeField] private AvatarSelfieView _avatarSelfieView;
        [SerializeField] private AvatarSelfiePageLocalization _localization;
        
        [Inject] private IArSessionManager _arSessionManager;
        [Inject] private IPermissionsHelper _permissionsHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private bool _isPermissionRequestPending;
        private PopupManager _popupManager;
        private IBridge _bridge;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.AvatarSelfie;

        private bool DeviceCameraRunning => _arSessionManager.IsActive;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        public void Construct(PopupManager popupManager, IBridge bridge)
        {
            _popupManager = popupManager;
            _bridge = bridge;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {    
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _avatarSelfieView.Init(GenerateRecipe, _popupManager, _bridge);
        }

        protected override void OnDisplayStart(AvatarSelfieArgs args)
        {
            base.OnDisplayStart(args);
            
            TryRunCamera();
            
            _avatarSelfieView.gameObject.SetActive(true);
            if(PlayerPrefs.HasKey("isFirstTime")) {
                OpenQuestionPopUp(_localization.WelcomePopupTitle, _localization.WelcomePopupMessage);
                PlayerPrefs.SetString("isFirstTime", "false");
            }
            
            _avatarSelfieView.SetGender(args.Gender);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _arSessionManager.SetARActive(false);
            _avatarSelfieView.gameObject.SetActive(false);
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void GenerateRecipe(SelfieAvatarResult result)
        {
            if (result.IsSuccess)
            {
                GoToPreviewPage(result.SelfieJson);
            }
            else
            {
                DisplaySelfieFailedPopup(result.ErrorMessage);
            }
        }

        private void OnBackButtonClicked()
        {
            OpenPageArgs.BackButtonClicked?.Invoke();
        }

        private void GoToPreviewPage(JSONSelfie json)
        {
            OpenPageArgs.OnSelfieTaken.Invoke(json);
        }

        private void OpenQuestionPopUp(string title, string desc)
        {
            var questionPopupConfiguration = new QuestionPopupConfiguration()
            {
                PopupType = PopupType.Question,
                Title = title,
                Description = desc,
                Answers = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>("OK", null)
                }
            };
            _popupManager.SetupPopup(questionPopupConfiguration);
            _popupManager.ShowPopup(questionPopupConfiguration.PopupType);
        }

        private void OpenPermissionSettingsPopup()
        {
            if (_popupManager.IsPopupOpen(PopupType.ActionSheet)) return;

            var popupConfig = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Description = _localization.CameraPermissionPopupTitle,
                CancelButtonText = _localization.LeaveButton,
                OnClose = OnPermissionPopupClosed,
                Variants = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>(_localization.OpenSettingsButton, OpenAppPermissionSettings),
                }
            };
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(PopupType.ActionSheet, true);
        }

        private void OnPermissionPopupClosed(object obj)
        {
            OnBackButtonClicked();
        }

        private void TryRunCamera()
        {
            var permission = _permissionsHelper.GetPermissionState(PermissionTarget.Camera);
            switch (permission)
            {
                case PermissionStatus.NotDetermined:
                    RequestCameraPermission();
                    break;
                case PermissionStatus.Restricted:
                case PermissionStatus.Denied:
                    OpenPermissionSettingsPopup();
                    break;
                case PermissionStatus.Authorized:
                    ActivateDeviceCamera();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RequestCameraPermission()
        {
            _isPermissionRequestPending = true;
            _permissionsHelper.RequestPermission(PermissionTarget.Camera, OnPermissionGranted, (x)=>OnPermissionDenied());
            
            void OnPermissionGranted()
            {
                _isPermissionRequestPending = false;
                ActivateDeviceCamera();
            }

            void OnPermissionDenied()
            {
                _isPermissionRequestPending = false;
                OpenPermissionSettingsPopup();
            }
        }
        
        private void ActivateDeviceCamera()
        {
            _arSessionManager.SetARActive(true);
        }

        private void OpenAppPermissionSettings()
        {
            _permissionsHelper.OpenNativeAppPermissionMenu();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(!hasFocus || DeviceCameraRunning || _isPermissionRequestPending) return;
            TryRunCamera();
        }
        
        private void DisplaySelfieFailedPopup(string message)
        {
            if (message.Contains(UNCENTERED_FACE_ERROR))
            {
                SnackBarErrorMessage();
            }
            else
            {
                ServerDownPopup();
            }
        }

        private void ServerDownPopup()
        {
            var confirmInformationPopup = new ServerDownPopupConfiguration
            {
                PopupType = PopupType.ServerDown,
                Title = _localization.ServerDownPopupTitle,
                Description = _localization.ServerDownPopupMessage,
                ConfirmButtonText = _localization.ServerDownPopupCloseButton,
                OnConfirm = OnBackButtonClicked
            };

            _popupManager.SetupPopup(confirmInformationPopup);
            _popupManager.ShowPopup(confirmInformationPopup.PopupType);
        }

        private void SnackBarErrorMessage()
        {
            _snackBarHelper.ShowInformationSnackBar(_localization.CenterFaceMessage, 4);
            _avatarSelfieView.StartCapturing();
        }
    }
}
