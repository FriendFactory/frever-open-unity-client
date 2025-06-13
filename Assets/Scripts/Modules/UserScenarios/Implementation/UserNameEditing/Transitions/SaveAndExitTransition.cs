using System.Threading.Tasks;
using Bridge;
using JetBrains.Annotations;
using Modules.AccountVerification.LoginMethods;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.EditUsername;
using UIManaging.SnackBarSystem;

namespace Modules.UserScenarios.Implementation.UserNameEditing.Transitions
{
    [UsedImplicitly]
    internal sealed class SaveAndExitTransition: TransitionBase<EditNameContext>, IResolvable
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly SnackBarHelper _snackbarHelper;
        private readonly EditUsernameLocalization _localization;
        private readonly LoginMethodsProvider _loginMethodsProvider;
        
        public override ScenarioState To => ScenarioState.ExitEditingUserName;

        public SaveAndExitTransition(LocalUserDataHolder localUserDataHolder, SnackBarHelper snackbarHelper, EditUsernameLocalization localization, LoginMethodsProvider loginMethodsProvider)
        {
            _localUserDataHolder = localUserDataHolder;
            _snackbarHelper = snackbarHelper;
            _localization = localization;
            _loginMethodsProvider = loginMethodsProvider;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            if (Context.SelectedName == Context.OriginalName) return;
            
            var result = await _localUserDataHolder.UpdateUserName(Context.SelectedName);
            if (!result.Ok)
            {
                _snackbarHelper.ShowFailSnackBar(_localization.UsernameUpdateFailed);
                return;
            }

            if (Context.LoginMethodUpdated)
            {
                _snackbarHelper.ShowSuccessDarkSnackBar(_localization.UsernameAndLoginMethodUpdated);
            }
            else
            {
                _snackbarHelper.ShowSuccessDarkSnackBar(_localization.UsernameUpdated);
            }
            
            await _loginMethodsProvider.UpdateLoginMethodsAsync();
        }
    }
}