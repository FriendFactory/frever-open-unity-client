using Modules.AccountVerification;

namespace UIManaging.Pages.AccountVerification
{
    public class VerificationMethodUpdateViewModel
    {
        public IVerificationMethod VerificationMethod { get; }
        public VerificationMethodPageTextData TextData { get; }

        public VerificationMethodUpdateViewModel(IVerificationMethod verificationMethod,VerificationMethodPageTextData textData)
        {
            VerificationMethod = verificationMethod;
            TextData = textData;
        }
    }
}