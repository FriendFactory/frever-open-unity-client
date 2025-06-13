using System.Text.RegularExpressions;

namespace UIManaging.Pages.Common.UsersManagement
{
    public static class UserNameValidator
    {
        public static UserNameValidationResult ValidateUserNameSymbols(string userName)
        {
            var errorMessage = $"{userName} is invalid, it can only contain letters or digits. ";
            
            var regex = new Regex("^[a-zA-Z0-9-._@+]+$", RegexOptions.IgnoreCase);
            var isValid = regex.IsMatch(userName);
            var message = isValid ? null : errorMessage;
            return new UserNameValidationResult
            {
                IsValid = isValid,
                ErrorMessage = message
            };
        }
    }
}