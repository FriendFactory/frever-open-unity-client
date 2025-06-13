using System;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.AccountVerification.LoginMethods;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    [UsedImplicitly]
    public sealed class VerifyUserPresenter
    {
        private readonly VerificationFlowProvider _verificationFlowProvider;
        private readonly PopupManager _popupManager;
        private readonly LoginMethodsProvider _loginMethodsProvider;
        private readonly AccountVerificationLocalization _localization;

        private IVerificationMethodView View { get; set; }
        private VerificationMethodUpdateFlowModel MethodUpdateFlowModel { get; set; }
        private VerificationFlowHandler VerificationFlowHandler { get; set; }

        public VerifyUserPresenter(VerificationFlowProvider verificationFlowProvider, PopupManager popupManager, LoginMethodsProvider loginMethodsProvider, AccountVerificationLocalization localization)
        {
            _popupManager = popupManager;
            _loginMethodsProvider = loginMethodsProvider;
            _localization = localization;
            _verificationFlowProvider = verificationFlowProvider;
            _popupManager = popupManager;
        }

        public void Initialize(VerificationMethodUpdateFlowModel methodUpdateFlowModel, IVerificationMethodView view)
        {
            MethodUpdateFlowModel = methodUpdateFlowModel;
            View = view;

            MethodUpdateFlowModel.UserVerificationMethodChanged += OnUserVerificationMethodChanged;

            View.NextRequested += OnNextRequested;
            View.BackRequested += OnBackRequested;

            VerificationFlowHandler = _verificationFlowProvider.GetFlowHandler(MethodUpdateFlowModel);
        }


        public void CleanUp()
        {
            View.NextRequested -= OnNextRequested;
            View.BackRequested -= OnBackRequested;
        }

        private async void OnNextRequested()
        {
            try
            {
                View.ToggleLoading(true);

                var type = MethodUpdateFlowModel.UserVerificationMethod.Type;
                if (!_loginMethodsProvider.IsLinked(type))
                {
                    View.ShowValidationError(_localization.GetInvalidMessage(type));
                    return;
                }

                var verificationResult = await VerificationFlowHandler.VerifyCredentialsAsync();
                
                if (verificationResult.IsCanceled) return;
                
                if (verificationResult.IsError)
                {
                    View.ShowValidationError(verificationResult.ErrorMessage ?? verificationResult.ErrorCode);
                    return;
                }
                
                _popupManager.ClosePopupByType(PopupType.VerifyUser);
                VerificationFlowHandler.MoveNext();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                View.ToggleLoading(false);
            }
        }
        
        private void OnUserVerificationMethodChanged(IVerificationMethod verificationMethod)
        {
            VerificationFlowHandler = _verificationFlowProvider.GetFlowHandler(MethodUpdateFlowModel);

            if (verificationMethod.Type.IsLinkable())
            {
                OnNextRequested();
            }
        }

        private void OnBackRequested() => _popupManager.ClosePopupByType(PopupType.VerifyUser);
    }
}