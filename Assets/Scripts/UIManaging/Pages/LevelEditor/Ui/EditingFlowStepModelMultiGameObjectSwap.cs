using Common.Abstract;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class EditingFlowStepModelMultiGameObjectSwap: BaseContextPanel<EditingStepModel>
    {
        [Header("Targets")]
        [SerializeField] private GameObject[] _isOnGameObjects;
        [SerializeField] private GameObject[] _isOffGameObjects;
        
        protected override void OnInitialized()
        {
            ContextData.FirstInFlowChanged += Swap;
            
            Swap(ContextData.IsFirstInFlow);
        }
        
        protected override void BeforeCleanUp()
        {
            ContextData.FirstInFlowChanged -= Swap;
        }

        private void Swap(bool firstStepInFlow)
        {
            foreach (var isOnGameObject in _isOnGameObjects)
            {
                isOnGameObject.SetActive(firstStepInFlow);
            }
            
            foreach (var isOffGameObject in _isOffGameObjects)
            {
                isOffGameObject.SetActive(!firstStepInFlow);
            }
        }
    }
}