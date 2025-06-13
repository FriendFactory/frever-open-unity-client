namespace Modules.AccountVerification
{
    public static class VerificationResultExtensions
    {
        private const string ALREADY_USED_ERROR_CODE_SUFFIX = "_USED";
        
        public static bool ErrorCodeIsUsed(this VerificationResult result)
        {
            return result.ErrorCode.EndsWith(ALREADY_USED_ERROR_CODE_SUFFIX);
        }
    }
}