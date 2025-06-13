using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    public class CharacterSelectionToggle : BaseCharacterSelectionElement
    {
        [SerializeField] private Toggle _toggle;

        public Toggle Toggle => _toggle;
        
        private ToggleGroup _toggleGroup;
        
        public override void SetActive(bool value)
        {
            base.SetActive(value);
            _toggle.interactable = value;
        }
        
        private void OnEnable()
        {
            Refresh();
        }

        protected override void OnCharacterChanged()
        {
            base.OnCharacterChanged();
            Refresh();
        }

        private void Refresh()
        {
            if (HasCharacter)
            {
                _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }

            _toggle.enabled = HasCharacter;
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (value)
            {
                InvokeOnClickedEvent();
            }
        }
    }
}
