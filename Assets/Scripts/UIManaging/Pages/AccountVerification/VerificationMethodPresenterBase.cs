using System;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Modules.AccountVerification.LoginMethods;
using Navigation.Core;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification
{
    [UsedImplicitly]
    public abstract class VerificationMethodPresenterBase
    {
        protected readonly AccountVerificationService AccountVerificationService;
        
        private readonly PageManager _pageManager;
        private readonly VerificationCodePageArgsFactory _codePageArgsFactory;
        private readonly AccountVerificationLocalization _localization;
        private readonly LoginMethodsProvider _loginMethodsProvider;
        private readonly LocalUserDataHolder _localUserDataHolder;

        public bool IsInitialized { get; private set; }
        
        protected IVerificationMethod Model { get; private set; }
        protected IVerificationMethodView View { get; private set; }
        
        protected abstract VerificationMethodOperationType OperationType { get; }
        
        protected VerificationMethodPresenterBase(
            PageManager pageManager,
            VerificationCodePageArgsFactory codePageArgsFactory,
            AccountVerificationService accountVerificationService,
            AccountVerificationLocalization localization,
            LoginMethodsProvider loginMethodsProvider,
            LocalUserDataHolder localUserDataHolder
            )
        {
            _pageManager = pageManager;
            _codePageArgsFactory = codePageArgsFactory;
            AccountVerificationService = accountVerificationService;
            _localization = localization;
            _loginMethodsProvider = loginMethodsProvider;
            _localUserDataHolder = localUserDataHolder;
        }

        public void Initialize(IVerificationMethod model, IVerificationMethodView view)
        {
            Model = model;
            View = view;

            View.NextRequested += OnNextButtonClicked;
            View.BackRequested += MoveBack;
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            View.NextRequested -= OnNextButtonClicked;
            View.BackRequested -= MoveBack;
            
            IsInitialized = false;
        }

        protected abstract Task<VerificationResult> UpdateVerificationMethodAsync(IVerificationMethod method);
        
        private async void OnNextButtonClicked()
        {
            try
            {
                View.ToggleLoading(true);

                var isPasswordType = Model.Type == CredentialType.Password;
                if (isPasswordType)
                {
                    var passwordResult = await UpdateVerificationMethodAsync(Model);
                    if (passwordResult.IsError)
                    {
                        View.ShowValidationError(passwordResult.ErrorMessage);
                        return;
                    }
                    
                    Complete();
                    return;
                }
                
                var type = Model.Type is CredentialType.Email ? VerifiableCredentialType.Email : VerifiableCredentialType.PhoneNumber; 
                var result = await AccountVerificationService.SendVerificationCodeAsync(type, Model.Input, true);
                if (result.IsError)
                {
                    // phone number key is already used in onboarding server localization with different value
                    _localization.OverrideVerificationResultErrorMessage(Model.Type, result);
                    View.ShowValidationError(result.ErrorMessage);
                    return;
                }
                
                MoveToSendCodePage();
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
        
        private void MoveToSendCodePage()
        {
            var pageArgs = _codePageArgsFactory.CreatePageArgs(Model, true);

            pageArgs.MoveNextRequested = MoveNextRequested; 
            
            _pageManager.MoveNext(pageArgs);

            async void MoveNextRequested()
            {
                try
                {
                    var result = await UpdateVerificationMethodAsync(Model);
                    if (result.IsError)
                    {
                        pageArgs.MoveNextFailed?.Invoke();
                        return;
                    }

                    Complete();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void Complete()
        {
            _loginMethodsProvider.LoginMethodsUpdated += UpdateLocalUserInfo;
            
            var result = new VerificationMethodUpdateResult(Model.Type, OperationType);
            
            StaticBus<VerificationMethodUpdatedEvent>.Post(new VerificationMethodUpdatedEvent(result));
        }

        private async void UpdateLocalUserInfo()
        {
            _loginMethodsProvider.LoginMethodsUpdated -= UpdateLocalUserInfo;
            await _localUserDataHolder.RefreshUserInfoAsync();
        }
        
        private void MoveBack() => _pageManager.MoveBack();
    }
}