using Bridge.Models.Common;
using TMPro;
using UIManaging.Localization;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class TextWardrobeUIItem<T> : EntityUIItem<T> where T: INamed, IEntity
    {
        [SerializeField]
        protected TextMeshProUGUI _textComponent;

        [SerializeField]
        private Color32 _selectedColor;
        [SerializeField]
        private Color32 _unselectedColor;
        
        public void SetLocalizedName(LocalizationMapping localization)
        {
            _textComponent.text = localization.GetLocalized(Entity.Name);
        }
        
        protected override void ChangeSelectionVisual()
        {
            _textComponent.color = (Selected) ? _selectedColor : _unselectedColor;
        }
    }
}