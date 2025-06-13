using System.Linq;
using System.Threading.Tasks;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    internal sealed class EnterCharacterEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public override ScenarioState To => ScenarioState.CharacterEditor;
        
        public EnterCharacterEditorTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        protected override Task UpdateContext()
        {
            Context.CharacterEditor.ShowTaskInfo = false;
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            var characterAssets = _levelManager.GetCurrentCharactersAssets();
            var characterAsset = characterAssets.First(x=>x.Id == Context.CharacterEditor.Character.Id);
            _levelManager.UnloadAllAssets(characterAsset);
            return base.OnRunning();
        }
    }
}