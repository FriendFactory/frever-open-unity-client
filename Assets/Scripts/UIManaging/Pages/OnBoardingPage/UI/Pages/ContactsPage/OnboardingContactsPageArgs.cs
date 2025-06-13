using System;
using Navigation.Core;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnboardingContactsPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.OnboardingContactsPage;
        public Action OnContinueButtonClick { get; set; }

        public OnboardingContactsPageArgs(Action continueButtonClick)
        {
            OnContinueButtonClick = continueButtonClick;
        }
    }
}