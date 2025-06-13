namespace Modules.AccountVerification
{
    public class VerificationResult
    {
        public bool IsError { get; set; }
        public bool IsCanceled { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public VerificationResult()
        {
            IsError = false;
        }

        public VerificationResult(bool isCanceled = true)
        {
            IsCanceled = true;
        }

        public VerificationResult(string errorCode, string errorMessage)
        {
            IsError = true;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
    
    public class VerificationResult<TModel>
    {
        public TModel Model { get; }
        public bool IsError { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        
        public VerificationResult(TModel model)
        {
            Model = model;
            IsError = false;
        }

        public VerificationResult(string errorCode, string errorMessage)
        {
            IsError = true;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}