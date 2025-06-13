using Abstract;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Zenject;

namespace UIManaging.Pages.Feed.Core
{
    public sealed class UseMessageTemplateButton : BaseContextDataButton<Video>
    {
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IUniverseManager _universeManager;
        
        protected override void OnInitialized() { }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
            _universeManager.SelectUniverse(universe =>
            {
                _scenarioManager.ExecuteVideoMessageCreation(universe: universe, publishingType: PublishingType.Post);
                _button.interactable = false;
            });
        }
    }
}