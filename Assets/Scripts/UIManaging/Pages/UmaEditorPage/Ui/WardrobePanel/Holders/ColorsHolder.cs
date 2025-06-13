using System;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class ColorsHolder : BaseWardrobePanelUIHolder
    {
        public event Action<string, Color32> ColorPicked;

        private Dictionary<long, List<ColorItem>> _colorsInUmaShared = new Dictionary<long, List<ColorItem>>();
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(WardrobeSubCategory subCategory)
        {
            if (subCategory.UmaSharedColorId == null || _colorsInUmaShared.ContainsKey(subCategory.UmaSharedColorId.Value)) return;
    
            var colors = new List<ColorItem>();
            _colorsInUmaShared.Add(subCategory.UmaSharedColorId.Value, colors);
            var umaSharedColors = _clothesCabinet.GetUmaSharedColor(subCategory.UmaSharedColorId.Value);
            foreach (var color in umaSharedColors.Colors)
            {
                var colorItemGO = Instantiate(_itemPrefab, _content);
                var colorItem = colorItemGO.GetComponent<ColorItem>();
                colorItem.Setup(umaSharedColors.Name, new Color32().ConvertFromIntColor(color));
                colorItem.ItemSelected += OnItemSelected;
                colors.Add(colorItem);
                colorItem.gameObject.SetActive(false);
            }
        }

        public void UpdateSelections(Dictionary<string, Color32> selectedColors)
        {
            var items = _colorsInUmaShared.Values.SelectMany(x => x);
            foreach (var colorItem in items)
            {
                if (!selectedColors.TryGetValue(colorItem.ColorName, out var color)) continue;

                colorItem.Selected = colorItem.Color.Equals(color);
            }
        }

        public void ShowColorsForSubCategory(WardrobeSubCategory targetSubCategory)
        {
            foreach (var sharedId in _colorsInUmaShared.Keys)
            {
                var activate = sharedId == targetSubCategory.UmaSharedColorId;
                var colors = _colorsInUmaShared[sharedId];
                foreach (var colorItem in colors)
                {
                    colorItem.gameObject.SetActive(activate);
                }
            }
        }

        public override void Clear()
        {
            var items = _colorsInUmaShared.Values.SelectMany(item => item);
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }
            _colorsInUmaShared = new Dictionary<long, List<ColorItem>>();
            ColorPicked = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnItemSelected(string colorName, Color32 color)
        {
            ColorPicked?.Invoke(colorName, color);
        }
    }
}