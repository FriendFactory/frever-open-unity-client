using System;
using Bridge;
using Modules.AccountVerification.LoginMethods;
using Modules.VideoStreaming.UIAnimators;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.AccountVerification;
using UIManaging.Pages.AppSettingsPage.LoginMethods;
using UIManaging.Pages.AppSettingsPage.UI.AccountManagement.Username;
using UIManaging.Pages.AppSettingsPage.UI.Args;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.UsersManagement;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;
#if UNITY_IOS
using Common;
using SA.Foundation.Templates;
using SA.iOS.AuthenticationServices;
#endif

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class ManageAccountPage : GenericPage<ManageAccountPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private Button _deleteAccountButton;
        [SerializeField] private PageUiAnimator _pageUiAnimator;
        [SerializeField] private LoginMethodsPreviewPanel _loginMethodsPreviewPanel;
        [FormerlySerializedAs("_usernamePreviewPanel")] [SerializeField] private AccountDetailsPreviewPanel _accountDetailsPreviewPanel;

        [Inject] private PageManager _pageManager;
        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private UserAccountManager _userAccountManager;
        [Inject] private LoginMethodsProvider _loginMethodsProvider;
        [Inject] private VerificationMethodPageArgsFactory _verificationMethodPageArgsFactory;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private LoginMethodsPreviewPanelPresenter _loginMethodsPreviewPresenter;
        [Inject] private LocalUserDataHolder _localUserDataHolder;

        public override PageId Id => PageId.ManageAccountPage;

        private bool _revokingAppleCredentials;
        private ManageAccountPageLoc _loc;
        private LoginMethodsPreviewPanelModel _loginMethodsPreviewModel;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || !_revokingAppleCredentials) return;
            CheckAppleCredentialStatus();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _loc = GetComponent<ManageAccountPageLoc>();
            _pageHeaderView.Init(new PageHeaderArgs(_loc.PageHeader, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
        }
        
        protected override void OnDisplayStart(ManageAccountPageArgs args)
        {
            _loginMethodsPreviewModel = new LoginMethodsPreviewPanelModel(_loginMethodsProvider);
            _loginMethodsPreviewPresenter.Initialize(_loginMethodsPreviewModel, _loginMethodsPreviewPanel);
            
            _loginMethodsPreviewPanel.Initialize(_loginMethodsPreviewModel);
            
            _accountDetailsPreviewPanel.Initialize(_localUserDataHolder.UserProfile);
            
            _deleteAccountButton.onClick.AddListener(OnDeleteAccountButtonClicked);
            _pageHeaderView.SetBackButtonInteractivity(false);
            _pageUiAnimator.PrepareForDisplay();
            _pageUiAnimator.PlayShowAnimation(() => OnShowAnimationFinished(args));
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _loginMethodsPreviewPresenter.CleanUp();
            _loginMethodsPreviewPanel.CleanUp();
            _loginMethodsPreviewModel.Dispose();
            
            _accountDetailsPreviewPanel.CleanUp();
            
            _deleteAccountButton.onClick.RemoveListener(OnDeleteAccountButtonClicked);
            _pageUiAnimator.PlayHideAnimation(()=>base.OnHidingBegin(onComplete));
        }

        private void OnDeleteAccountButtonClicked()
        {
            DisplayDeleteAccountPopup();
        }

        private void DisplayDeleteAccountPopup()
        {
            var confirmPopupConfiguration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDark,
                Title = _loc.DeletePopupTitle,
                Description = _loc.DeletePopupDesc,
                YesButtonText = _loc.DeletePopupConfirm,
                NoButtonText = _loc.DeletePopupCancel,
                OnYes = DeleteAccount
            };

            _popupManager.SetupPopup(confirmPopupConfiguration);
            _popupManager.ShowPopup(confirmPopupConfiguration.PopupType);
        }

        private void DeleteAccount()
        {
            _userAccountManager.DeleteAccount(OnUsedDataDeleted);
        }

        private void OnUsedDataDeleted()
        {
            var isUserRegisteredWithApple = _bridge.Profile.RegisteredWithAppleId;
            if (isUserRegisteredWithApple)
            {
                LogoutUser();
                ShowAppleTokenRevokingPopup();
            }
            else
            {
                LogoutUser(MoveToOnBoardingPage);
            }
        }

        private void ShowAppleTokenRevokingPopup()
        {
            var confirmPopupConfiguration = new RevokeAppleTokenPopupConfiguration
            {
                PopupType = PopupType.RevokeAppleTokenInstruction,
                Title = _loc.RevokeAppleTokenPopupTitle,
                OnOpenSettingsClicked = OnOpenSettingsClicked
            };

            void OnOpenSettingsClicked()
            {
                Application.OpenURL("app-settings:");
                _revokingAppleCredentials = true;
            }

            _popupManager.SetupPopup(confirmPopupConfiguration);
            _popupManager.ShowPopup(confirmPopupConfiguration.PopupType);
        }

        private void LogoutUser(Action onComplete = null)
        {
            _userAccountManager.Logout(onComplete, OnLogoutFailed);
        }

        private void MoveToOnBoardingPage()
        {
            _pageManager.MoveNext(PageId.OnBoardingPage, new OnBoardingPageArgs());
        }

        private void OnLogoutFailed(string errorMessage)
        {
            throw new InvalidOperationException($"Failed to logout user. Reason: {errorMessage}");
        }

        private void OnShowAnimationFinished(ManageAccountPageArgs args)
        {
            base.OnDisplayStart(args);
            _pageHeaderView.SetBackButtonInteractivity(true);
        }

        private void CheckAppleCredentialStatus()
        {
        #if UNITY_IOS
            var provider = new ISN_ASAuthorizationAppleIDProvider();
            var userId = PlayerPrefs.GetString(Constants.AppleId.USER_ID_IDENTIFIER);
            var request = provider.CreateRequest();
            provider.GetCredentialStateForUserID(userId, CredentialState);
            
            void CredentialState(ISN_ASAuthorizationAppleIDProviderCredentialState state, SA_iError error)
            {
                if (state != ISN_ASAuthorizationAppleIDProviderCredentialState.Revoked) return;
                
                _revokingAppleCredentials = false;
                _popupManager.ClosePopupByType(PopupType.RevokeAppleTokenInstruction);
                MoveToOnBoardingPage();
            } 
        #endif
        }
    }
}