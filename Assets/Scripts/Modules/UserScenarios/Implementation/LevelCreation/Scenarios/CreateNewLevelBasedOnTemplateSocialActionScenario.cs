using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class CreateNewLevelBasedOnTemplateActionScenario :
        LevelCreationScenarioBase<CreateNewLevelBasedOnTemplateScenarioArgs>,
        ICreateNewLevelBasedOnTemplateSocialActionScenario
    {
        public CreateNewLevelBasedOnTemplateActionScenario(DiContainer diContainer,
            IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider)
            : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.TemplateActionSetup);
            return Task.FromResult(setup.States);
        }

        private sealed class EntryTransition : EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly CreateNewLevelBasedOnTemplateScenarioArgs _args;

            private ScenarioState _to;
            public override ScenarioState To => _to;

            public EntryTransition(CreateNewLevelBasedOnTemplateScenarioArgs args) : base(args)
            {
                _args = args;
            }

            protected override Task UpdateContext()
            {
                Context.SocialActionId = _args.SocialActionId;
                Context.RecommendationId = _args.RecommendationId;
                Context.InitialTemplateId = _args.Template.Id;
                Context.LevelEditor.TemplateId = Context.InitialTemplateId;
                Context.LevelEditor.LinkTemplateToEvent = true;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.LevelEditor.OnLevelEditorLoaded = _args.OnTemplateLoaded;
                Context.LevelEditor.ShowDressingStep = true;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.OnLevelCreationCanceled += _args.OnCancelCallback;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                
                _to = ScenarioState.TemplateGrid;
                return Task.CompletedTask;
            }
        }
    }
}