using System.Collections.Generic;
using Modules.SignUp;

namespace UIManaging.Pages.EditUsername
{
    public class UsernameValidationResult
    {
        public bool IsValid { get; }
        public bool UsernameTaken { get; }
        public Dictionary<RequirementType, bool> FailedRequirements { get; }
        public List<string> UsernameSuggestions { get; }

        public UsernameValidationResult()
        {
            IsValid = true;
        }

        public UsernameValidationResult(Dictionary<RequirementType, bool> failedRequirements)
        {
            FailedRequirements = failedRequirements;
        }

        public UsernameValidationResult(List<string> usernameSuggestions)
        {
            IsValid = false;
            UsernameTaken = true;
            UsernameSuggestions = usernameSuggestions;
        }
    }
}