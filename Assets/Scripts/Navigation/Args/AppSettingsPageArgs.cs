using Navigation.Core;

namespace Navigation.Args
{
    public sealed class AppSettingsPageArgs : PageArgs
    {
        public override PageId TargetPage { get; } = PageId.AppSettings;
    }
}