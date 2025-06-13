using System;
using Navigation.Core;

namespace UiManaging.Pages.OnBoardingPage.UI.Args
{
    public class OnBoardingToggleSelectorPageArgs : OnBoardingBasePageArgs
    {
        public override PageId TargetPage { get; } = PageId.OnBoardingToggleSelectorPage;

        public Action<int> SetValue;
        public Action OnAllTogglesUnselectedAction;
    }
}