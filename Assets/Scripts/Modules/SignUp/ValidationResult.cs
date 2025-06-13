using System.Collections.Generic;

namespace Modules.SignUp
{
    public struct ValidationResult
    {
        public bool IsValid;
        public string ReasonPhrase;
        public string ErrorCode;
        public Dictionary<RequirementType, bool> RequirementFailed;
    }
}