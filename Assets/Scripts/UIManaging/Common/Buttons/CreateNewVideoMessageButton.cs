using Common.Publishers;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Buttons
{
    internal sealed class CreateNewVideoMessageButton: MonoBehaviour
    {
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IUniverseManager _universeManager;
        private Button _button;
        
        private void Awake()
        {
            _button =  GetComponent<Button>();
            _button.onClick.AddListener(ExecuteVideoMessageCreation);
        }

        private void ExecuteVideoMessageCreation()
        {
            _universeManager.SelectUniverse(universe =>
            {
                _scenarioManager.ExecuteVideoMessageCreation(universe: universe,
                                                             publishingType: PublishingType.Post);
                _button.interactable = false;
            });
        }
    }
}