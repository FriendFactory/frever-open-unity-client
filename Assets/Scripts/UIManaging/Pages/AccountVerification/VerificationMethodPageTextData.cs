namespace UIManaging.Pages.AccountVerification
{
    public sealed class VerificationMethodPageTextData
    {
        public string Header { get; set; }
        public string Description { get; set; }
        public string ContinueButtonLabel { get; set; }

        public VerificationMethodPageTextData(string header, string description, string continueButtonLabel)
        {
            Header = header;
            Description = description;
            ContinueButtonLabel = continueButtonLabel;
        }
    }
}