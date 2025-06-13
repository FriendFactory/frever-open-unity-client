using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.AccountVerification.Models;
using Bridge.Authorization.Models;
using Bridge.Results;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Localization;
using UnityEngine;

namespace Modules.AccountVerification
{
    [UsedImplicitly]
    public class AccountVerificationService
    {
        private readonly IAccountVerificationBridge _bridge;
        private readonly OnboardingServerErrorLocalization _serverErrorLocalization;

        public AccountVerificationService(IAccountVerificationBridge bridge, OnboardingServerErrorLocalization serverErrorLocalization)
        {
            _bridge = bridge;
            _serverErrorLocalization = serverErrorLocalization;
        }

        public async Task<CredentialStatus> GetCredentialStatusAsync(CancellationToken token)
        {
            var result = await _bridge.GetCredentialsStatus(token);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get credential status # {result.ErrorMessage}");
                return null;
            }

            return result.Model;
        }

        public async Task<VerificationResult> SendVerificationCodeAsync(VerifiableCredentialType type, string credential, bool isNew)
        {
            var verificationTask = _bridge.SendVerificationCode(type, credential, isNew);

            return await PerformVerificationTaskAsync(verificationTask);
        }

        public async Task<VerificationResult<VerifyUserResponse>> VerifyCredentialsAsync(IVerificationMethod verificationMethod)
        {
            var verificationTask = GetVerificationTask();
            
            return await PerformVerificationTaskAsync(verificationTask);
                
            Task<Result<VerifyUserResponse>> GetVerificationTask()
            {
                switch (verificationMethod.Type)
                {
                    case CredentialType.Email:
                    case CredentialType.PhoneNumber:
                        return _bridge.VerifyCredentials(verificationMethod.Type, verificationMethod.VerificationCode);
                    case CredentialType.Password:
                        return _bridge.VerifyCredentials(verificationMethod.Type, verificationMethod.Input);
                    case CredentialType.AppleId:
                    case CredentialType.GoogleId:
                        return _bridge.VerifyCredentials(verificationMethod.Type, verificationMethod.VerificationToken);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public IVerificationMethod ToVerificationMethodModel(ICredentials credentials)
        {
            var output = new VerificationMethod(credentials.CredentialType);
            switch (credentials)
            {
                case UsernameAndPasswordCredentials userNameCreds:
                    output.Input = userNameCreds.Password;
                    break;
                case EmailCredentials emailCredentials:
                    output.Input = emailCredentials.Email;
                    output.VerificationCode = emailCredentials.VerificationCode;
                    break;
                case PhoneNumberCredentials phoneNumberCredentials:
                    output.Input = phoneNumberCredentials.PhoneNumber;
                    output.VerificationCode = phoneNumberCredentials.VerificationCode;
                    break;
                case AppleAuthCredentials appleAuthCredentials:
                    output.Input = appleAuthCredentials.AppleId;
                    output.VerificationToken = appleAuthCredentials.AppleIdentityToken;
                    break;
                case GoogleAuthCredentials googleAuthCredentials:
                    output.Input = googleAuthCredentials.GoogleId;
                    output.VerificationToken = googleAuthCredentials.GoogleIdentityToken;
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(credentials), (int)credentials.CredentialType, typeof(CredentialType));
            }

            return output;
        }

        public async Task<VerificationResult> AddVerificationMethodAsync(IVerificationMethod verificationMethod)
        {
            var verificationTask = GetAddVerificationMethodTask();

            return await PerformVerificationTaskAsync(verificationTask);
            
            Task<Result> GetAddVerificationMethodTask()
            {
                switch (verificationMethod.Type)
                {
                    case CredentialType.Email:
                        return _bridge.AddVerificationMethod(VerifiableCredentialType.Email, verificationMethod.Input, verificationMethod.VerificationCode);
                    case CredentialType.PhoneNumber:
                        return _bridge.AddVerificationMethod(VerifiableCredentialType.PhoneNumber, verificationMethod.Input, verificationMethod.VerificationCode);
                    case CredentialType.Password:
                        return _bridge.AddVerificationMethod(verificationMethod.Input);
                    case CredentialType.AppleId:
                        return _bridge.AddVerificationMethod(LinkableCredentialType.Apple, verificationMethod.VerificationToken, verificationMethod.Input);
                    case CredentialType.GoogleId:
                        return _bridge.AddVerificationMethod(LinkableCredentialType.Google, verificationMethod.VerificationToken, verificationMethod.Input);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public async Task<VerificationResult> ChangeVerificationMethodAsync(IVerificationMethod method)
        {
            var verificationTask = GetChangeVerificationMethodTask();

            return await PerformVerificationTaskAsync(verificationTask);
            
            Task<Result> GetChangeVerificationMethodTask()
            {
                switch (method.Type)
                {
                    case CredentialType.Email:
                        return _bridge.ChangeVerificationMethod(VerifiableCredentialType.Email, method.Input, method.VerificationCode, method.VerificationToken);
                    case CredentialType.PhoneNumber:
                        return _bridge.ChangeVerificationMethod(VerifiableCredentialType.PhoneNumber, method.Input, method.VerificationCode, method.VerificationToken);
                    case CredentialType.Password:
                        return _bridge.ChangeVerificationMethod(method.Input, method.VerificationToken);
                    case CredentialType.AppleId:
                    case CredentialType.GoogleId:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public async Task<Result> RemoveVerificationMethodAsync(IVerificationMethod verificationMethod)
        {
            return await _bridge.RemoveVerificationMethod(verificationMethod.Type, verificationMethod.VerificationCode, verificationMethod.VerificationToken);
        }

        private async Task<VerificationResult> PerformVerificationTaskAsync(Task<Result> verificationTask)
        {
            var result = await verificationTask;

            if (result.IsSuccess) return new VerificationResult();

            var errorCode = result.GetErrorCodeFromErrorMessage() ?? result.ErrorMessage;
            var errorMessage = _serverErrorLocalization.GetLocalized(errorCode);

            return new VerificationResult(errorCode, errorMessage);
        }
        
        private async Task<VerificationResult<TModel>> PerformVerificationTaskAsync<TModel>(Task<Result<TModel>> verificationTask)
        {
            var result = await verificationTask;

            if (result.IsSuccess) return new VerificationResult<TModel>(result.Model);

            var errorCode = result.GetErrorCodeFromErrorMessage() ?? result.ErrorMessage;
            var errorMessage = _serverErrorLocalization.GetLocalized(errorCode);

            return new VerificationResult<TModel>(errorCode, errorMessage);
        }
    }
}