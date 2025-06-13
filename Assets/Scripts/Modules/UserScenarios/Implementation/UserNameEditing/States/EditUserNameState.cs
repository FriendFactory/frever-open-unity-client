using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.EditUsername;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;

namespace Modules.UserScenarios.Implementation.UserNameEditing.States
{
    [UsedImplicitly]
    internal sealed class EditUserNameState : StateBase<EditNameContext>, IResolvable
    {
        public override ScenarioState Type => ScenarioState.EditUserName;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public ITransition MoveNext;
        public ITransition MoveBack;

        private readonly PageManager _pageManager;
        private readonly PopupManager _popupManager;
        private readonly EditUsernameLocalization _localization;
        private readonly LocalUserDataHolder _localUserDataHolder;

        public EditUserNameState(PageManager pageManager, LocalUserDataHolder localUserDataHolder, PopupManager popupManager, EditUsernameLocalization localization)
        {
            _pageManager = pageManager;
            _popupManager = popupManager;
            _localization = localization;
            _localUserDataHolder = localUserDataHolder;
        }

        public override void Run()
        {
            var pageArgs = new EditUsernamePageArgs(Context.SelectedName)
            {
                UpdateRequested = OnUpdateRequested
            };
            _pageManager.MoveNext(pageArgs);
        }

        private void OnUpdateRequested(string username)
        {
            if (_localUserDataHolder.HasSetupCredentials)
            {
                ShowConfirmationDialog();
            } 
            else
            {
                UpdateUsername(username);
            }

            void ShowConfirmationDialog()
            {
                var confirmPopupConfiguration = new DialogDarkPopupConfiguration
                {
                    PopupType = PopupType.DialogDarkV3,
                    Title = _localization.ConfirmTitle,
                    Description = _localization.ConfirmDescription,
                    YesButtonText = _localization.ConfirmYes,
                    NoButtonText = _localization.ConfirmNo,
                    OnYes = () => UpdateUsername(username),
                };

                _popupManager.SetupPopup(confirmPopupConfiguration);
                _popupManager.ShowPopup(confirmPopupConfiguration.PopupType);
            }
        }
        
        private void UpdateUsername(string username)
        {
            Context.SelectedName = username;
            MoveNext.Run();
        }
    }
}
