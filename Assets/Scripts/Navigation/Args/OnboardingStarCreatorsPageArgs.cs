using System;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class OnboardingStarCreatorsPageArgs : PageArgs
    {
        public Action OnContinueButtonClick;
        
        public override PageId TargetPage => PageId.OnboardingStarCreatorsPage;
    }
}