using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Modules.AccountVerification.LoginMethods;
using Modules.AccountVerification.Providers;
using Navigation.Core;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.AccountVerification;
using UIManaging.Pages.AccountVerification.AccountConfirmation;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    [UsedImplicitly]
    internal sealed class LoginMethodsPreviewPanelPresenter
    {
        private readonly VerificationMethodPageArgsFactory _verificationMethodPageArgsFactory;
        private readonly PageManager _pageManager;
        private readonly PopupManager _popupManager;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly IPopupParentManager _popupParentManager;
        private readonly AccountVerificationLocalization _localization;
        private readonly AddLinkableLoginMethodHandler _linkableLoginMethodHandler;

        private LoginMethodsPreviewPanelModel _model;
        private LoginMethodsPreviewPanel _view;

        public LoginMethodsPreviewPanelPresenter(VerificationMethodPageArgsFactory verificationMethodPageArgsFactory,
            PageManager pageManager, PopupManager popupManager, SnackBarHelper snackBarHelper,
            IPopupParentManager popupParentManager, AccountVerificationService accountVerificationService,
            AccountVerificationLocalization localization, ICredentialsHandler credentialsHandler)
        {
            _verificationMethodPageArgsFactory = verificationMethodPageArgsFactory;
            _pageManager = pageManager;
            _popupManager = popupManager;
            _snackBarHelper = snackBarHelper;
            _popupParentManager = popupParentManager;
            _localization = localization;
            _linkableLoginMethodHandler = new AddLinkableLoginMethodHandler(accountVerificationService, credentialsHandler);
        }

        public bool IsInitialized { get; private set; }
        public bool IsRequesting { get; private set; }

        public void Initialize(LoginMethodsPreviewPanelModel model, LoginMethodsPreviewPanel view)
        {
            _model = model;
            _view = view;

            _view.NextButtonClicked += OnNextButtonClicked;

            IsInitialized = true;
        }

        public void CleanUp()
        {
            _view.NextButtonClicked -= OnNextButtonClicked;
            
            _popupManager.RemoveAllFromPool(PopupType.VerifyUser);

            IsInitialized = false;
        }

        private void OnNextButtonClicked(ILoginMethodInfo credentials)
        {
            if (credentials.Type.IsLinkable() && !credentials.IsLinked)
            {
                if (IsRequesting) return;
                
                AddLinkableId();
                return;
            }

            if (!credentials.IsLinked)
            {
                _pageManager.MoveNext(_verificationMethodPageArgsFactory.CreateAddPageArgs(credentials.Type, VerificationMethodOperationType.Add));
                return;
            }

            switch (credentials.Type)
            {
                case CredentialType.Password:
                    MoveToChangeMethodPage(credentials);
                    break;
                case CredentialType.Email:
                case CredentialType.PhoneNumber:
                    ShowAvailableOperationsSheet(credentials);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowAvailableOperationsSheet(ILoginMethodInfo credentials)
        {
            var configuration = CreateActionSheetPopupConfiguration();

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);

            ActionSheetPopupConfiguration CreateActionSheetPopupConfiguration()
            {
                var variants = new List<KeyValuePair<string, Action>>()
                {
                    new("Unlink", MoveToRemoveMethodPage),
                    new("Change", MoveToChangeMethodPageAction),
                };

                return new ActionSheetPopupConfiguration()
                {
                    PopupType = PopupType.AnimatedActionSheet,
                    Variants = variants,
                };

                void MoveToRemoveMethodPage()
                {
                    var unlinkAllowed = _model.GetModels().Count(model => model.IsLinked) > 1;
                    if (!unlinkAllowed)
                    {
                        var unlinkFailedMessage = _localization.UnlinkFailedMessage;
                        _snackBarHelper.ShowFailSnackBar(unlinkFailedMessage);
                        return;
                    }

                    ShowVerifyUserPopup(credentials.Type, VerificationMethodOperationType.Remove);
                }

                void MoveToChangeMethodPageAction() => MoveToChangeMethodPage(credentials);
            }
        }

        private void MoveToChangeMethodPage(ILoginMethodInfo credentials)
        {
            ShowVerifyUserPopup(credentials.Type, VerificationMethodOperationType.Change);
        }

        private void ShowVerifyUserPopup(CredentialType type, VerificationMethodOperationType operationType)
        {
            if (!_popupParentManager.TryGetParent(PopupType.VerifyUser, out var parent))
            {
                throw new ArgumentNullException();
            }

            var configuration = new VerifyUserPopupConfiguration(new VerificationMethod(type), operationType);

            _popupManager.SetupPopup(configuration, parent);
            _popupManager.ShowPopup(PopupType.VerifyUser);
        }

        private async void AddLinkableId()
        {
            try
            {
                IsRequesting = true;

                var result = await _linkableLoginMethodHandler.LinkAsync();
                
                if (result.IsCanceled) return;

                StaticBus<VerificationMethodUpdatedEvent>.Post(new VerificationMethodUpdatedEvent(result));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                IsRequesting = false;
            }
        }

    }
}