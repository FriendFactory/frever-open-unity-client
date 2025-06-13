using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class LeaveLevelEditorTransition: TransitionBase, IContextDependant<ILevelCreationScenarioContext>
    {
        private ILevelCreationScenarioContext _context;
        private readonly ITransition[] _transitions;
        
        public override ScenarioState To => _context.LevelEditor.OnMoveBack;
        private ITransition TargetTransition => _transitions.First(x=>x.To == To);

        public LeaveLevelEditorTransition(ITransition[] transitions)
        {
            _transitions = transitions;
        }

        public override async Task Run()
        {
            if (_context.SavedAsDraft)
            {
                _context.LevelEditor.OnMoveBack = ScenarioState.ProfileExit;
            }
            
            await RunSilently();
            OnFinished();
        }

        public override Task RunSilently()
        {
            return TargetTransition.RunSilently();
        }

        public void SetContext(ILevelCreationScenarioContext context)
        {
            _context = context;
            foreach (var transition in _transitions.Where(x=>x is IContextDependant<ILevelCreationScenarioContext>).Cast<IContextDependant<ILevelCreationScenarioContext>>())
            {
                transition.SetContext(context);
            }
        }
    }
}