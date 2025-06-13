using Bridge.Services.UserProfile;
using Common.Abstract;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.EditUsername;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.UI.AccountManagement.Username
{
    internal sealed class AccountDetailsPreviewPanel: BaseContextPanel<Profile>
    {
        [SerializeField] private UsernamePreview _usernamePreview;

        [Inject] private LocalUserDataHolder _localUserDataHolder;
        
        protected override void OnInitialized()
        {
            var model = new EditUsernameModel(_localUserDataHolder.NickName, _localUserDataHolder.UsernameUpdateAvailableOn);
            _usernamePreview.Initialize(model);
        }

        protected override void BeforeCleanUp()
        {
            _usernamePreview.CleanUp();
        }
    }
}