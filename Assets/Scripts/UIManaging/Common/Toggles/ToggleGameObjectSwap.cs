using UnityEngine;

namespace UIManaging.Common.Toggles
{
    public class ToggleGameObjectSwap: ToggleSwapBase
    {
        [SerializeField] private GameObject _isOnGameObject;
        [SerializeField] private GameObject _isOffGameObject;

        protected override void Swap(bool isOn)
        {
            if (_isOffGameObject)
            {
                _isOffGameObject.SetActive(!isOn);
            }
            
            if (_isOnGameObject)
            {
                _isOnGameObject.SetActive(isOn);
            }
        }
    }
}