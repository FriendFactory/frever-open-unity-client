using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Common;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using Modules.SignUp;
using Modules.TempSaves.Manager;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding
{
    [UsedImplicitly]
    internal sealed class OnboardingScenario : ScenarioBase<OnboardingArgs, OnboardingContext>, IOnboardingScenario
    {
        private const ScenarioState ENTRY_STATE = ScenarioState.OnboardingLoadingState;

        private readonly TempFileManager _tempFileManager;
        private readonly IBridge _bridge;
        private readonly CharacterManager _characterManager;
        private readonly ThumbnailsHelper _thumbnailsHelper;
        private IScenarioManager _scenarioManager;
        private ISignUpService _signUpService;
        
        public OnboardingScenario(DiContainer diContainer, IBridge bridge, CharacterManager characterManager, 
            TempFileManager tempFileManager) : base(diContainer)
        {
            _tempFileManager = tempFileManager;
            _bridge = bridge;
            _characterManager = characterManager;
            _thumbnailsHelper = new ThumbnailsHelper(bridge);
        }
        
        protected override async Task<OnboardingContext> SetupContext()
        {
            var context = await LoadOnboardingContext(
                _tempFileManager.GetData<OnboardingContext.Serializable>(Constants.Onboarding.CONTEXT_LOCAL_FILE_PATH + _bridge.Profile?.Id)
             ?? new OnboardingContext.Serializable { CurrentState = ENTRY_STATE });
            
            return context;
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = Resolve<OnboardingStatesSetup>();
            return Task.FromResult(setup.States);
        }

        protected override ITransition SetupEntryTransition()
        {
            _scenarioManager = DiContainer.Resolve<IScenarioManager>();
            _scenarioManager.StateRun += OnStateRun;
            _signUpService = DiContainer.Resolve<ISignUpService>();

            if (InitialArgs.StartFromSignIn)
            {
                Context.SerializableContext.CurrentState = ScenarioState.SignInOverlay;
            }
            
            return new EntryTransition(InitialArgs, _signUpService, Context.SerializableContext.CurrentState);
        }

        private void OnStateRun(ScenarioState state)
        {
            if (States.First(stateObj => stateObj.Type == state).IsExitState && _bridge.Profile != null)
            {
                _scenarioManager.StateRun -= OnStateRun;
                _tempFileManager.RemoveTempFile(Constants.Onboarding.CONTEXT_LOCAL_FILE_PATH + _bridge.Profile?.Id);
                return;
            }

            if (_bridge.Profile == null) return;
            
            Context.SerializableContext.CurrentState = state;
        }

        private async Task<OnboardingContext> LoadOnboardingContext(OnboardingContext.Serializable serializableContext)
        {
            var context = new OnboardingContext
            {
                SerializableContext = serializableContext
            };

            if (context.SerializableContext.CharacterStyles != null)
            {
                context.CharacterStyles = await _thumbnailsHelper.LoadThumbnailsAsync(context.SerializableContext.CharacterStyles);
            }

            if (context.CharacterEditor.Character != null)
            {
                _characterManager.SetCharacterSilent(context.CharacterEditor.Character.Id);
            }

            return context;
        }
        
        private sealed class EntryTransition: EntryTransitionBase<OnboardingContext>
        {
            public override ScenarioState To { get; }

            private readonly ISignUpService _signUpService;
            
            public EntryTransition(ScenarioArgsBase args, ISignUpService signUpService, ScenarioState to) : base(args)
            {
                To = to;
                _signUpService = signUpService;
            }

            protected override async Task UpdateContext()
            {
                Context.IsNewCharacter = true;
                Context.AllowBackFromGenderSelection = false;
                await _signUpService.Initialize();
                await base.UpdateContext();
            }
        }
    }
}