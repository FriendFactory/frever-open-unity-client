using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using Modules.UserScenarios.Implementation.UserNameEditing.States;
using Modules.UserScenarios.Implementation.UserNameEditing.Transitions;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Modules.UserScenarios.Implementation.UserNameEditing
{
    [UsedImplicitly]
    internal sealed class EditNameScenario : ScenarioBase<EditNameArgs, EditNameContext>, IEditNameScenario
    {
        public EditNameScenario(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, Resolve<LocalUserDataHolder>());
        }

        protected override Task<EditNameContext> SetupContext()
        {
            return Task.FromResult(new EditNameContext());
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var userNameState = ResolveState<EditUserNameState>();
            var addLoginMethodState = ResolveState<AddLoginMethodState>();
            var exitState = ResolveState<ExitEditUserNameState>();
            var verificationState = ResolveState<VerificationState>();

            var enterAddLoginMethod = new EmptyTransition(addLoginMethodState.Type);
            var saveAndExit = ResolveTransition<SaveAndExitTransition>();
            var moveBackFromVerification = ResolveTransition<BackToAddLoginMethodTransition>();
            
            userNameState.MoveNext = new MoveNextFromEditNameStateTransition(
                new ITransition[] { enterAddLoginMethod, saveAndExit }, 
                Resolve<LocalUserDataHolder>()
            );
            userNameState.MoveBack = saveAndExit;
    
            addLoginMethodState.MoveNextNoVerification = saveAndExit;
            addLoginMethodState.MoveNext = new EmptyTransition(verificationState);
            addLoginMethodState.MoveBack = new EmptyTransition(userNameState);
    
            verificationState.MoveNext = saveAndExit;
            verificationState.MoveBack = moveBackFromVerification;
    
            return Task.FromResult(new IScenarioState[] { 
                userNameState,
                addLoginMethodState,
                verificationState, 
                exitState 
            });
        }
        
        private sealed class EntryTransition: EntryTransitionBase<EditNameContext>
        {
            private readonly LocalUserDataHolder _localUserDataHolder;
            public override ScenarioState To => ScenarioState.EditUserName;
            
            public EntryTransition(ScenarioArgsBase scenarioArgs, LocalUserDataHolder localUserDataHolder) : base(scenarioArgs)
            {
                _localUserDataHolder = localUserDataHolder;
            }

            protected override Task UpdateContext()
            {
                Context.OriginalName = _localUserDataHolder.NickName;
                Context.SelectedName = _localUserDataHolder.NickName;
                return base.UpdateContext();
            }
        }
    }
}
