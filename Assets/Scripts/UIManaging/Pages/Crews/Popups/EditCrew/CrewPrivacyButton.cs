using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    [RequireComponent(typeof(Button))]
    internal sealed class CrewPrivacyButton : MonoBehaviour
    {
        [SerializeField] private GameObject _toggleOnIcon;
        [SerializeField] private GameObject _toggleOffIcon;
        
        private Button _button;

        public event Action OnClicked;

        public bool Interactable => _button.interactable;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        public void SetInteractable(bool value)
        {
            _button.interactable = value;
            _toggleOnIcon.SetActive(!value);           
            _toggleOffIcon.SetActive(value);
        }

        private void OnButtonClicked()
        {
            OnClicked?.Invoke();
        }
    }
}