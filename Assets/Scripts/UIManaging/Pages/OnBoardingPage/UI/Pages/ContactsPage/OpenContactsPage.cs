using Navigation.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OpenContactsPage: MonoBehaviour
    {
        [Inject] private PageManager _pageManager;
        
        [Button]
        private void Open()
        {
            // _pageManager.MoveNext(new OnboardingContactsPageArgs());
        }
    }
}