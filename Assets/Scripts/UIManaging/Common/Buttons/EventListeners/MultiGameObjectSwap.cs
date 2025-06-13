using UnityEngine;

namespace UIManaging.Common.Buttons.EventListeners
{
    public class MultiGameObjectSwap: MonoBehaviour
    {
        [SerializeField] private ButtonInteractableStateChangedEventEmitter _eventEmitter;
        [Header("Targets")]
        [SerializeField] private GameObject[] _isOnGameObjects;
        [SerializeField] private GameObject[] _isOffGameObjects;
        
        private void OnEnable()
        {
            _eventEmitter.StateChanged.AddListener(Swap);
            
            Swap(_eventEmitter.IsOn);
        }
        
        private void OnDisable()
        {
            _eventEmitter.StateChanged.RemoveListener(Swap);
        }
        
        private void Swap(bool isOn)
        {
            foreach (var isOnGameObject in _isOnGameObjects)
            {
                isOnGameObject.SetActive(isOn);
            }
            
            foreach (var isOffGameObject in _isOffGameObjects)
            {
                isOffGameObject.SetActive(!isOn);
            }
        }
    }
}