using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using UIManaging.PopupSystem;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class SignInExitFromOverlayState : ExitStateBase<ISignupContext>, IResolvable
    {
        [Inject] private PopupManager _popupManager;

        public override ScenarioState Type => ScenarioState.SignInExitFromOverlay;
        
        public override void Run()
        {
            //_popupManager.ClosePopupByType(PopupType.Login);
        }
    }
}