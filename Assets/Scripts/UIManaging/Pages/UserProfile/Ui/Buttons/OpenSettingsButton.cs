using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    public sealed class OpenSettingsButton : MonoBehaviour
    {
        [SerializeField] private GameObject _notificationMarker;
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            _notificationMarker.SetActive(!_localUserDataHolder.HasSetupCredentials);
        }

        private void OnClick()
        {
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };
            _pageManager.MoveNext(PageId.AppSettings, new AppSettingsPageArgs(), transitionArgs);
        }
    }
}
