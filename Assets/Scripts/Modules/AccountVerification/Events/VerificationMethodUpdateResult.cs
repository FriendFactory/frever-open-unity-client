using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification.Events
{
    public sealed class VerificationMethodUpdateResult
    {
        public bool IsSuccess { get; set; }
        public bool IsCanceled { get; set; }
        public CredentialType Type { get; }
        public VerificationMethodOperationType OperationType { get; }
        public string Message { get; }

        public VerificationMethodUpdateResult(CredentialType type, VerificationMethodOperationType operationType, bool isSuccess = true, string message = null)
        {
            IsSuccess = isSuccess;
            Type = type;
            OperationType = operationType;
            Message = message;
        }

        public VerificationMethodUpdateResult(bool isCanceled)
        {
            IsSuccess = false;
            IsCanceled = true;
        }
    }
}