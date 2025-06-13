using UnityEngine;
using UnityEngine.UI;
using Bridge;
using System.Linq;
using System;
using Common;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class CategoryItem : ImagedWardrobeUIItem<WardrobeCategory>
    {
        public Toggle Toggle => _toggle;

        [SerializeField]
        private Color32 _selectedColor;
        [SerializeField]
        private Color32 _unselectedColor;
        [SerializeField]
        private Toggle _toggle;

        public override void Init(IBridge bridge)
        {
            base.Init(bridge);
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void ChangeSelectionVisual()
        {
            base.ChangeSelectionVisual();
            _thumbnail.color = Selected ? _selectedColor : _unselectedColor;
        }

        protected override void UpdateIsNew()
        {
            var categoryEntity = Entity;
            _newIcon.SetActive(categoryEntity.HasNew);
        }

        private void OnValueChanged(bool isOn)
        {
            Selected = isOn;
        }
    }
}
