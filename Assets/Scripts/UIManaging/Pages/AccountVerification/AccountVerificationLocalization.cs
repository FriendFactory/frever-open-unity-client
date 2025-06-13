using System;
using Bridge.AccountVerification.Models;
using I2.Loc;
using Modules.AccountVerification;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification
{
    [CreateAssetMenu(menuName = "L10N/AccountVerificationLocalization", fileName = "AccountVerificationLocalization")]
    public sealed class AccountVerificationLocalization: ScriptableObject
    {
        [SerializeField] private LocalizedString _email;
        [SerializeField] private LocalizedString _emailAddress;
        [SerializeField] private LocalizedString _phoneNumber;
        [SerializeField] private LocalizedString _password;
        [SerializeField] private LocalizedString _appleId;
        [SerializeField] private LocalizedString _googleId;
        [Header("Method Update")]
        [SerializeField] private LocalizedString _updateMethodPageDescription;
        [SerializeField] private LocalizedString _updatePasswordPageDescription;
        [SerializeField] private LocalizedString _updateMethodPageAddPasswordHeader;
        [SerializeField] private LocalizedString _updateMethodPageAddHeader;
        [SerializeField] private LocalizedString _updateMethodPageChangeHeader;
        [SerializeField] private LocalizedString _methodAddedMessage;
        [SerializeField] private LocalizedString _methodChangedMessage;
        [SerializeField] private LocalizedString _methodRemovedMessage;
        [SerializeField] private LocalizedString _methodAddedFailedMessage;
        [SerializeField] private LocalizedString _methodChangedFailedMessage;
        [SerializeField] private LocalizedString _methodRemovedFailedMessage;
        [Header("Error Handling")]
        [SerializeField] private LocalizedString _methodInvalidError;
        [SerializeField] private LocalizedString _unlinkFailedError;
        [SerializeField] private LocalizedString _phoneNumberUsedError;
        [Header("Next Button")] 
        [SerializeField] private LocalizedString _sendCodeButtonLabel;
        [SerializeField] private LocalizedString _continueButtonLabel;
        [SerializeField] private LocalizedString _finishButtonLabel;

        public string UnlinkFailedMessage => _unlinkFailedError;

        public VerificationMethodPageTextData GetPageTextData(CredentialType type, VerificationMethodOperationType operationType, string nickname)
        {
            var buttonLabel = type is CredentialType.Password ? _finishButtonLabel : _sendCodeButtonLabel;
            var header = GetHeader();
            var description = GetDescription();

            return new VerificationMethodPageTextData(header, description, buttonLabel);

            string GetDescription() => type is CredentialType.Password
                ? string.Format(_updatePasswordPageDescription, nickname)
                : _updateMethodPageDescription.ToString();

            string GetHeader()
            {
                var methodName = GetMethodName(type);
                methodName = type is CredentialType.Email ? _emailAddress : methodName;
                var headerFormat = operationType is VerificationMethodOperationType.Add
                    ? _updateMethodPageAddHeader
                    : _updateMethodPageChangeHeader;
                var pageHeader = operationType is VerificationMethodOperationType.Add && type is CredentialType.Password
                    ? _updateMethodPageAddPasswordHeader.ToString()
                    : string.Format( headerFormat, methodName.ToLower());

                return pageHeader;
            } 
        }

        public  VerifyUserPopupTextData GetVerifyUserPageData(IVerificationMethod verificationMethod)
        {
            var buttonLabel = verificationMethod.Type == CredentialType.Password ? _continueButtonLabel : _sendCodeButtonLabel;

            return new VerifyUserPopupTextData(buttonLabel);
        }

        public string GetInvalidMessage(CredentialType type)
        {
            var methodName = GetMethodName(type);
            
            return string.Format(_methodInvalidError, methodName.ToLower());
        }

        public string GetMethodUpdatedMessage(CredentialType type, VerificationMethodOperationType operationType, bool isSuccess)
        {
            var methodName = GetMethodName(type);
            var message = GetMessageFormatted(); 
            
            return string.Format(message, methodName);

            string GetMessageFormatted()
            {
                switch (operationType)
                {
                    case VerificationMethodOperationType.Add:
                        return isSuccess ? _methodAddedMessage : _methodAddedFailedMessage;
                    case VerificationMethodOperationType.Change:
                        return isSuccess ? _methodChangedMessage : _methodChangedFailedMessage;
                    case VerificationMethodOperationType.Remove:
                        return isSuccess ? _methodRemovedMessage : _methodRemovedFailedMessage;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null);
                }
            }
        }

        public void OverrideVerificationResultErrorMessage(CredentialType type, VerificationResult result)
        {
            if (type != CredentialType.PhoneNumber || !result.ErrorCodeIsUsed()) return;

            result.ErrorMessage = _phoneNumberUsedError;
        }

        private string GetMethodName(CredentialType type)
        {
            switch (type)
            {
                case CredentialType.Email:
                    return _email;
                case CredentialType.PhoneNumber:
                    return _phoneNumber;
                case CredentialType.Password:
                    return _password;
                case CredentialType.AppleId:
                    return _appleId;
                case CredentialType.GoogleId:
                    return _googleId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}