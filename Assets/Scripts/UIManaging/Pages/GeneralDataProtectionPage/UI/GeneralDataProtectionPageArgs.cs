using System;
using Navigation.Core;

namespace UIManaging.Pages.GeneralDataProtectionPage.Ui
{
    public class GeneralDataProtectionPageArgs : PageArgs
    {
        public override PageId TargetPage { get; } = PageId.GeneralDataProtection;

        public Action<bool> OnGeneralDataProtectionToggleChanged;
    }
}