using Extensions;
using Modules.WardrobeManaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class ColorPalettePanel : MonoBehaviour
    {
        [SerializeField]
        private int _maxItemsCount = 7;
        [SerializeField]
        private ColorItem _itemPrefab;
        [SerializeField]
        private RectTransform _holder;
        [SerializeField]
        private Button _palletButton;

        public event Action<string, Color32> ColorPicked;
        public Action ColorButtonClicked;

        [Inject]
        private ClothesCabinet _clothesCabinet;
        private List<ColorItem> _colorItems = new List<ColorItem>();
        private string _colorName = "";

        private void Start()
        {
            _palletButton.onClick.AddListener(OnColorButtonClicked);
        }

        public void UpdateColorsList(long sharedColorId)
        {
            CleanItems();
            var counter = 0;
            var umaColor = _clothesCabinet.GetUmaSharedColor(sharedColorId);
            if (umaColor == null)
            {
                Debug.LogWarning($"Uma color with Id {sharedColorId} not found in Start Pack");
                return;
            }
            _colorName = umaColor.Name;
            var baseSize = (_palletButton.transform as RectTransform).GetSize();
            foreach (var color in umaColor.Colors)
            {
                var item = Instantiate(_itemPrefab, _holder);
                (item.transform as RectTransform).SetSize(baseSize);
                item.Setup(_colorName, new Color32().ConvertFromIntColor(color));
                item.ItemSelected += OnItemSelected;
                _colorItems.Add(item);
                counter++;
                if (counter >= _maxItemsCount) break;
            }
        }

        public void UpdateSelections(Dictionary<string, Color32> selectedColors)
        {
            if (selectedColors.TryGetValue(_colorName, out var color))
            {
                foreach (var colorItem in _colorItems)
                {
                    colorItem.Selected = colorItem.Color.Equals(color);
                }
            }
        }

        private void OnItemSelected(string colorName, Color32 color)
        {
            ColorPicked?.Invoke(colorName, color);
        }

        private void CleanItems()
        {
            for (int i = _colorItems.Count - 1; i >= 0; i--)
            {
                Destroy(_colorItems[i].gameObject);
            }
            _colorItems.Clear();
        }

        private void OnColorButtonClicked()
        {
            ColorButtonClicked?.Invoke();
        }
    }
}