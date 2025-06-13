using Extensions;
using UIManaging.Common.Buttons.Label;
using UnityEngine;
using UnityEngine.Events;

namespace UIManaging.Common.Buttons
{
    public class LoadableButton: BaseButton
    {
        [SerializeField] private ButtonTextLabel _label;
        
        private bool _isLoading;

        public UnityEvent OnClicked;
        public UnityEvent<bool> OnToggleLoading;

        public bool Interactable
        {
            get => Button.interactable;
            set => Button.interactable = value;
        }

        public string Label
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public void ToggleLoading(bool isOn)
        {
            if (isOn == _isLoading) return;
            
            _isLoading = isOn;
            
            _label.SetActive(!_isLoading);
            Button.interactable = !_isLoading;
            
            OnToggleLoading?.Invoke(isOn);
        }
        
        protected override void OnClickHandler()
        {
            OnClicked?.Invoke();
        }
    }
}