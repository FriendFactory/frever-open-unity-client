using Modules.AccountVerification;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal sealed class VerificationMethodInputViewModel
    {
        public IVerificationMethod Method { get; }
        public bool ShowHeader { get; }

        public VerificationMethodInputViewModel(IVerificationMethod method, bool showHeader)
        {
            Method = method;
            ShowHeader = showHeader;
        }
    }
}