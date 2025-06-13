using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class PostRecordEditorMoveBackTransition: TransitionBase, IContextDependant<ILevelCreationScenarioContext>
    {
        private readonly ITransition[] _transitions;
        private ILevelCreationScenarioContext _context;
        
        public override ScenarioState To => _context.SavedAsDraft ? ScenarioState.ProfileExit : ScenarioState.LevelEditor;

        public PostRecordEditorMoveBackTransition(ITransition[] transitions)
        {
            _transitions = transitions;
        }

        public override async Task Run()
        {
            await RunSilently();
            OnFinished();
        }

        public override Task RunSilently()
        {
            return _transitions.First(x=>x.To == To).RunSilently();
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