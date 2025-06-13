using System.Threading.Tasks;
using Extensions;
using Models;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    internal abstract class CharacterSelectionToEditorTaskTransitionBase : TransitionBase<ILevelCreationScenarioContext>,
                                                                           IResolvable
    {
        protected override async Task UpdateContext()
        {
            if (Context.LevelData != null)
            {
                await SetupCharacters(Context.LevelData);
            }
            
            if (Context.InitialTemplateId.HasValue)
            {
                Context.LevelEditor.CharactersToUseInTemplate = Context.CharacterSelection.PickedCharacters;
            }
        }
        
        private Task SetupCharacters(Level level)
        {
            if (Context.CharacterSelection.PickedCharacters == null)
            {
                return Task.CompletedTask;
            }
            
            level.RemoveAllOutfits();
            level.ReplaceCharacters(Context.CharacterSelection.PickedCharacters);
            return Task.CompletedTask;
        }
    }
}