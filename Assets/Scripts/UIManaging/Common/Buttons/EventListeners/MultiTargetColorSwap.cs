using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Buttons.EventListeners
{
    public class MultiTargetColorSwap: MonoBehaviour
    {
        [Serializable]
        public class ColorSwapTarget
        {
            public Graphic target;
            public Color isOnColor = Color.white;
            public Color isOffColor = Color.gray;
        }
        
        [SerializeField] private ButtonInteractableStateChangedEventEmitter _eventEmitter;
        [Header("Targets")]
        [SerializeField] private ColorSwapTarget[] _colorSwapTargets;
        
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
            foreach (var colorSwapTarget in _colorSwapTargets)
            {
                colorSwapTarget.target.color = isOn ? colorSwapTarget.isOnColor : colorSwapTarget.isOffColor;
            }
        }
    }
}