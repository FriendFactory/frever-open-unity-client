using QFSW.QC;
using UnityEngine.Scripting;

namespace Development
{
    [Preserve]
    public class HiddenCommandFilter : IQcSuggestionFilter
    {
        public bool IsSuggestionPermitted(IQcSuggestion suggestion, SuggestionContext context)
        {
            // Hide the sudo command from the suggestions
            return !suggestion.PrimarySignature.Equals("sudo");
        }
    }
}