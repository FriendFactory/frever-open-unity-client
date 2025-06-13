using Common.Permissions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NoConnectionPage
{
    internal sealed class PermissionSettingsLink : MonoBehaviour
    {
        private Button _settingsButton;
        [Inject] private IPermissionsHelper _permissionsHelper;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _settingsButton = GetComponent<Button>();
            _settingsButton.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _settingsButton.onClick.RemoveListener(OnClick);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClick()
        {
            _permissionsHelper.OpenNativeAppPermissionMenu();
        }
    }
}