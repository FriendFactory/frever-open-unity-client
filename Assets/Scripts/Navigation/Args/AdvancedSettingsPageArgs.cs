using Navigation.Core;

namespace Navigation.Args
{
    public sealed class AdvancedSettingsPageArgs : PageArgs
    {
        public override PageId TargetPage { get; } = PageId.AdvancedSettings;
    }
}