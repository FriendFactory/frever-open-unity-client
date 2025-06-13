using System;
using Navigation.Core;

namespace UIManaging.Pages.ContactsPage
{
    public abstract class OnBoardingContactsPageArgs : PageArgs
    {
        public string TitleText;
        private Action OnContinue;
    }
}
