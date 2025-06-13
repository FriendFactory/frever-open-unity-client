using Navigation.Core;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage.UI
{
    /// <summary>
    /// Subscribe it to the secret gesture for environment selection on non-login page
    /// </summary>
    internal sealed class OpenLoginPageWithEnvironmentSelection: MonoBehaviour
    {
        [Inject] private PageManager _pageManager;

        public void Execute()
        {
            if (_pageManager.CurrentPage.Id == PageId.OnBoardingPage) return;
            _pageManager.MoveNext(new OnBoardingPageArgs()
            {
                ShowEnvironmentSelection = true
            });
        }
    }
}