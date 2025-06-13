using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.UserProfile.Ui.ProfileHelper;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    public class OpenEditProfileButton : MonoBehaviour
    {
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserProfileHelper _profileHelper;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };

            _pageManager.MoveNext(PageId.EditProfile, new EditProfilePageArgs(_profileHelper.Profile), transitionArgs);
        }
    }
}