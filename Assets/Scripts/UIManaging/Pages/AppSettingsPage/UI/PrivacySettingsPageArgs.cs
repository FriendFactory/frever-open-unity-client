using Navigation.Core;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    public sealed class PrivacySettingsPageArgs : PageArgs
    {
        public override PageId TargetPage { get; } = PageId.PrivacySettings;
    }
}