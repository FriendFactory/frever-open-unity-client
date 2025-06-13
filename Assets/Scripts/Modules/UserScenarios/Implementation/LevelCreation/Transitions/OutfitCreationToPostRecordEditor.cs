using System.Linq;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class OutfitCreationToPostRecordEditor: TransitionBase<LevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.PostRecordEditor;

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            ApplyNewCreatedOutfit();
            return base.OnRunning();
        }

        private void ApplyNewCreatedOutfit()
        {
            var outfit = Context.CharacterEditor.Outfit;
            var targetCharacterId = Context.CharacterEditor.Character.Id;
            foreach (var ev in Context.LevelData.Event)
            {
                var hasTargetCharacter = ev.CharacterController.Any(x => x.CharacterId == targetCharacterId);
                if (!hasTargetCharacter) continue;
                var characterController = ev.GetCharacterControllerByCharacterId(targetCharacterId);
                characterController.SetOutfit(outfit);
            }
        }
    }
}