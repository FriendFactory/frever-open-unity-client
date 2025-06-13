using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public sealed class TemplateFilterPopup : BasePopup<TemplateFilterPopupConfiguration>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;

        [SerializeField] private Toggle _allCharsButton;
        [SerializeField] private Toggle _oneCharButton;
        [SerializeField] private Toggle _twoCharsButton;
        [SerializeField] private Toggle _threeCharsButton;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _backgroundButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnEnable()
        {
            _allCharsButton.onValueChanged.AddListener(ApplyFilters);
            _oneCharButton.onValueChanged.AddListener(ApplyFilters);
            _twoCharsButton.onValueChanged.AddListener(ApplyFilters);
            _threeCharsButton.onValueChanged.AddListener(ApplyFilters);
        }

        private void OnDisable()
        {
            _allCharsButton.onValueChanged.RemoveListener(ApplyFilters);
            _oneCharButton.onValueChanged.RemoveListener(ApplyFilters);
            _twoCharsButton.onValueChanged.RemoveListener(ApplyFilters);
            _threeCharsButton.onValueChanged.RemoveListener(ApplyFilters);

        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _backgroundButton.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(TemplateFilterPopupConfiguration configuration)
        {
            UpdateCharacterToggles(configuration.CharactersCount);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateCharacterToggles(int? charactersCount)
        {
            _allCharsButton.isOn = (charactersCount == null);
            _oneCharButton.isOn = (charactersCount == 1);
            _twoCharsButton.isOn = (charactersCount == 2);
            _threeCharsButton.isOn = (charactersCount == 3);
        }

        private void ApplyFilters(bool isOn)
        {
            if (!isOn) return;
            
            int? characters = null;

            if (_oneCharButton.isOn)
                characters = 1;
            else if (_twoCharsButton.isOn)
                characters = 2;
            else if (_threeCharsButton.isOn)
                characters = 3;

            Hide(characters);
        }

        private void OnCloseButtonClick()
        {
            Hide(-1);
        }
    }
}