namespace Modules.AccountVerification
{
    public class CredentialsRequestResult
    {
        public bool IsError { get; }
        public bool IsCanceled { get; }
        public string AppleId { get; }
        public string AppleToken { get; } 
        public string ErrorMessage { get; }

        public CredentialsRequestResult(string appleId, string appleToken)
        {
            IsError = false;
            AppleId = appleId;
            AppleToken = appleToken;
        }

        public CredentialsRequestResult(bool isCanceled = true)
        {
            IsError = true;
            IsCanceled = isCanceled;
        }

        public CredentialsRequestResult(string errorMessage)
        {
            IsError = true;
            ErrorMessage = errorMessage;
        }
    }
}