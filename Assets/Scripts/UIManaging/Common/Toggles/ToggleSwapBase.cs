using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Toggles
{
    public abstract class ToggleSwapBase: MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;

        private void Awake()
        {
            _toggle.toggleTransition = UnityEngine.UI.Toggle.ToggleTransition.None;
            _toggle.graphic = null;
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(Swap);
            
            Swap(_toggle.isOn);
        }
        
        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(Swap);
        }

        public void Toggle(bool isOn)
        {
            Swap(isOn);
        }

        protected abstract void Swap(bool isOn);
    }
}