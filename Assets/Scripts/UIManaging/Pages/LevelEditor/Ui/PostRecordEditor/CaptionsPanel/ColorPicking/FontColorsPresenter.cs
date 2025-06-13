using System;
using System.Linq;
using Extensions;
using Modules.AssetsStoraging.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel.ColorPicking
{
    internal sealed class FontColorsPresenter: MonoBehaviour
    {
        [SerializeField] private FontColorsListView _colorsListView;
        [Inject] private IDataFetcher _dataFetcher;
        private FontColorModel[] _models;

        public event Action<Color> ColorPicked;

        public void Initialize(string selectedColorHex)
        {
            SetupModels(selectedColorHex);
            _colorsListView.Init(_models, OnColorPicked);
        }

        private void SetupModels(string selectedColorHex)
        {
            if (_models == null)
            {
                CreateModels();
            }

            SetupSelectionState(selectedColorHex);
        }

        private void CreateModels()
        {
            var colorsHex = _dataFetcher.MetadataStartPack.CaptionMetadata.Colors.Select(x => x.ColorPairs.First().Font).ToArray();
            _models = new FontColorModel[colorsHex.Length];
            for (var i = 0; i < colorsHex.Length; i++)
            {
                var colorHex = colorsHex[i];
                var fontColor = ColorExtension.HexToColor(colorHex);
                var checkmarkColor = fontColor == Color.white ? Color.black : Color.white;
                var model = new FontColorModel(fontColor, checkmarkColor);
                _models[i] = model;
            }
        }
        
        private void SetupSelectionState(string selectedColorHex)
        {
            var color = ColorExtension.HexToColor(selectedColorHex);
            for (var i = 0; i < _models.Length; i++)
            {
                var model = _models[i];
                model.IsSelected = string.IsNullOrEmpty(selectedColorHex) ? i == 0 : model.Color == color;
            }
        }

        private void OnColorPicked(FontColorModel model)
        {
            _models.First(x => x.IsSelected).IsSelected = false;
            model.IsSelected = true;
            _colorsListView.Refresh();
            ColorPicked?.Invoke(model.Color);
        }
    }
}