using System.Threading.Tasks;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.UserNameEditing.Transitions
{
    internal sealed class MoveNextFromEditNameStateTransition : SwitchTransitionBase<EditNameContext>
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        
        public MoveNextFromEditNameStateTransition(ITransition[] subTransitions, LocalUserDataHolder localUserDataHolder) : base(subTransitions)
        {
            _localUserDataHolder = localUserDataHolder;
        }

        protected override Task UpdateContext()
        {
            Destination = _localUserDataHolder.HasSetupCredentials ? ScenarioState.ExitEditingUserName : ScenarioState.AddLoginMethod;
            return Task.CompletedTask;
        }
    }
}
