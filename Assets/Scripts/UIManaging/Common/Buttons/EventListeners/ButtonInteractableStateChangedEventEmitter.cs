using System;
using UIManaging.Common.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Buttons.EventListeners
{
    [RequireComponent(typeof(Button))]
    public class ButtonInteractableStateChangedEventEmitter: BaseStateChangedEventEmitter<Button>
    {
        private bool _interactable;

        public bool IsOn => Target.interactable;

        protected override void Subscribe() { }
        protected override void Unsubscribe() { }

        private void Update()
        {
            if (Target.interactable == _interactable) return;
            
            _interactable = Target.interactable;
            OnStateChanged(_interactable);
        }
    }
}