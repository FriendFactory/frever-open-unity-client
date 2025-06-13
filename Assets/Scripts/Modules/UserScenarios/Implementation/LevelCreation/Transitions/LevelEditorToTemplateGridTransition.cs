using System.Threading.Tasks;
using Modules.UserScenarios.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    internal sealed class LevelEditorToTemplateGridTransition :TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        [Inject] private PopupManager _popupManager;
        
        public override ScenarioState To => ScenarioState.TemplateGrid;
        protected override Task UpdateContext()
        {
            var loadingPopupConfig = new InformationPopupConfiguration()
            {
                PopupType = PopupType.Loading, Title = "Cleaning up the stage"
            };
            _popupManager.ShowPopup(loadingPopupConfig.PopupType);
            
            return Task.CompletedTask;
        }
    }
}