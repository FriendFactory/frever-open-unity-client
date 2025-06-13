using System.Threading.Tasks;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    internal abstract class CharacterRelatedScenarioBase<TArgs>: ScenarioBase<TArgs, ICharacterCreationContext>
        where TArgs: IScenarioArgs
    {
        protected CharacterRelatedScenarioBase(DiContainer diContainer) : base(diContainer)
        {
        }
        
        protected sealed override Task<ICharacterCreationContext> SetupContext()
        {
            var context = new CharacterCreationContext();
            return Task.FromResult<ICharacterCreationContext>(context);
        }
    }
    
    internal abstract class CharacterCreationScenarioBase: CharacterRelatedScenarioBase<CreateNewCharacterArgs>
    {
        protected CharacterCreationScenarioBase(DiContainer diContainer) : base(diContainer)
        {
        }
        
        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = Resolve<CharacterCreationStatesSetup>();
            return Task.FromResult(setup.States);
        }
    }
}