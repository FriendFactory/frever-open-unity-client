using System;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class OnboardingContactsContinueButtonArgs
    {
        public SelectionPanelModel SelectionPanelModel { get; }
        public Action OnContinueButtonClick;

        public OnboardingContactsContinueButtonArgs(SelectionPanelModel selectionPanelModel, Action continueButtonClick)
        {
            OnContinueButtonClick = continueButtonClick;
            SelectionPanelModel = selectionPanelModel;
        }
    }
}