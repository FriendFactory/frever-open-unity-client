using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class ColorItem : UIItem
    {
        [SerializeField] private Image _colorImage;
        [SerializeField] private Image _selectionImage;

        public event Action<string, Color32> ItemSelected;
        public Color32 Color => _color;
        public string ColorName => _colorName;

        private Color32 _color;
        private string _colorName;

        public void Setup(string colorName, Color32 color)
        {
            _colorImage.color = color;
            _color = color;
            _colorName = colorName;
            GetComponent<Button>().onClick.AddListener(OnItemSelected);
        }

        protected override void ChangeSelectionVisual()
        {
            _selectionImage.gameObject.SetActive(Selected);
        }

        private void OnItemSelected()
        {
            ItemSelected?.Invoke(_colorName, _color);
        }
    }
}
