namespace UIManaging.Pages.AccountVerification
{
    public sealed class VerifyUserPopupTextData
    {
        public string ContinueButtonLabel { get; set; }

        public VerifyUserPopupTextData(string continueButtonLabel)
        {
            ContinueButtonLabel = continueButtonLabel;
        }
    }
}