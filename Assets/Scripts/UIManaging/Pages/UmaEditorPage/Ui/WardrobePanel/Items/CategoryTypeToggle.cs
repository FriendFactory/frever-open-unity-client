using Bridge.Models.ClientServer.StartPack.Metadata;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class CategoryTypeToggle : MonoBehaviour
    {
        [SerializeField]
        private Toggle _toggle;

        [SerializeField]
        private Text _text;

        [SerializeField]
        private Color32 _selectedColor;
        [SerializeField]
        private Color32 _unselectedColor;

        [SerializeField]
        private Image _image;

        [SerializeField]
        private SpriteById[] _imageById;

        public WardrobeCategoryType CategoryType { get; private set; }
        public Toggle Toggle => _toggle;

        private void Start()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(_toggle.isOn);
        }

        public void Setup(WardrobeCategoryType categoryType)
        {
            CategoryType = categoryType;
            SetImageById(CategoryType.Id);
        }

        private void SetImageById(long id)
        {
            var spriteById = _imageById.FirstOrDefault(x=>x.Id.Equals(id));
            if(spriteById == null) return;
            _image.sprite = spriteById.Sprite;
        }

        private void OnValueChanged(bool isOn)
        {
            var color = isOn ? _selectedColor : _unselectedColor;
            _text.color = color;
            _image.color = color;
        }

        [Serializable]
        private class SpriteById
        {
            public long Id;
            public Sprite Sprite;
        }

    }
}