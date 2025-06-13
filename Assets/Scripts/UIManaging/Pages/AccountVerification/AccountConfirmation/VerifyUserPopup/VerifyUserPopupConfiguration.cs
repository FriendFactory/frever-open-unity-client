using System;
using Modules.AccountVerification;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    public sealed class VerifyUserPopupConfiguration: PopupConfiguration
    {
        public IVerificationMethod VerificationMethod { get; }
        public VerificationMethodOperationType OperationType { get; }

        public VerifyUserPopupConfiguration(IVerificationMethod verificationMethod, VerificationMethodOperationType operationType, Action<object> onClose = null, string title = "") : base(PopupType.VerifyUser, onClose, title)
        {
            VerificationMethod = verificationMethod;
            OperationType = operationType;
        }
    }
}