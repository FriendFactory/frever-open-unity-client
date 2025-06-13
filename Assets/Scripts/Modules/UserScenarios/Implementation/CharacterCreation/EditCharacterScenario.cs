using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Navigation.Core;
using Zenject;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class EditCharacterScenario : CharacterRelatedScenarioBase<EditCharacterArgs>, IEditCharacterScenario
    {
        public EditCharacterScenario(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, ScenarioState.CharacterEditor);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var characterEditor = ResolveState<CharacterEditorState>();
            var exit = ResolveState<PreviousPageExitState>();
            characterEditor.MoveBack = new EmptyTransition(exit);
            characterEditor.MoveNext = new EmptyTransition(exit);
            return Task.FromResult(new IScenarioState[] { characterEditor, exit });
        }

        private sealed class EntryTransition: EntryTransitionBase<ICharacterCreationContext>
        {
            private readonly EditCharacterArgs _args;
            public override ScenarioState To { get; }

            public EntryTransition(EditCharacterArgs args, ScenarioState to) : base(args)
            {
                _args = args;
                To = to;
            }

            protected override Task UpdateContext()
            {
                Context.IsNewCharacter = false;
                Context.Character = _args.Character;
                Context.ThemeCollectionId = _args.ThemeId;
                Context.CategoryTypeId = _args.CategoryTypeId;
                return base.UpdateContext();
            }
        }
    }
}