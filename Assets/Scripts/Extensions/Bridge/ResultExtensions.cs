using Bridge.Results;
using Newtonsoft.Json;

namespace Extensions
{
    public static class ResultExtensions
    {
        public static string GetErrorCodeFromErrorMessage(this Result result)
        {
            if (string.IsNullOrEmpty(result.ErrorMessage)) return string.Empty;
            
            var errorObject = JsonConvert.DeserializeAnonymousType(result.ErrorMessage, new
            {
                errorCode = string.Empty,
                ErrorCode = string.Empty, 
            });

            return !string.IsNullOrEmpty(errorObject.errorCode) ? errorObject.errorCode : errorObject.ErrorCode;
        }
    }
}