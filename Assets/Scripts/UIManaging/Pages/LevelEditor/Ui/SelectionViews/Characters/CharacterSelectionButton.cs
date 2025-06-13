using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    public class CharacterSelectionButton : BaseCharacterSelectionElement
    {
        [SerializeField] private Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(InvokeOnClickedEvent);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(InvokeOnClickedEvent);
        }

        public override void SetActive(bool value)
        {
            base.SetActive(value);
            _button.interactable = value;
        }
    }
}