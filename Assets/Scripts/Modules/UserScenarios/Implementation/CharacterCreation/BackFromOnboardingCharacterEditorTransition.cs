using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class BackFromOnboardingCharacterEditorTransition : TransitionBase<ICharacterCreationContext>, IResolvable
    {
        public override ScenarioState To => _to;

        private ScenarioState _to;

        protected override Task UpdateContext()
        {
            _to = ScenarioState.CombinedStyleSelection;
            Context.SelectedCreateMode = Context.JsonSelfie == null ? CreateMode.Preset : CreateMode.Selfie;
            Context.JsonSelfie = null;
            Context.Character = null;
            return Task.CompletedTask;
        }
    }
}