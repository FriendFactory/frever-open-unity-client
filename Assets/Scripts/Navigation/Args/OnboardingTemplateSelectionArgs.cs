using System;
using Navigation.Core;

namespace Navigation.Args
{
    public class OnboardingTemplateSelectionArgs : PageArgs
    {
        public override PageId TargetPage => PageId.OnBoardingTemplateSelection;
        public Action OnSkipButtonClicked;
    }
}