namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class VerifyUserViewModel
    {
        public VerificationMethodUpdateFlowModel VerificationMethodUpdateFlowModel { get; }
        public VerifyUserPopupTextData TextData { get; set; }

        public VerifyUserViewModel(VerificationMethodUpdateFlowModel verificationMethodUpdateFlowModel, VerifyUserPopupTextData textData)
        {
            VerificationMethodUpdateFlowModel = verificationMethodUpdateFlowModel;
            TextData = textData;
        }
    }
}