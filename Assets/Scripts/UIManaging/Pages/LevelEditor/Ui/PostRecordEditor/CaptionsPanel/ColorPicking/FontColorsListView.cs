using System;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel.ColorPicking
{
    internal sealed class FontColorsListView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScrollerCellView _fontColorViewPrefab;
        [SerializeField] private EnhancedScroller _scroller;
        
        private FontColorModel[] _models;
        private Action<FontColorModel> _onColorPicked;
        
        public void Init(FontColorModel[] models, Action<FontColorModel> onSelected)
        {
            _models = models;
            _onColorPicked = onSelected;
            _scroller.Delegate = this;
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _models.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _fontColorViewPrefab.GetComponent<RectTransform>().GetWidth();
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var view = scroller.GetCellView(_fontColorViewPrefab);
            var colorComponent = view.GetComponent<FontColorView>();
            var model = _models[dataIndex];
            colorComponent.Setup(model, _onColorPicked);
            return view;
        }

        public void Refresh()
        {
            var visibleCells = _scroller.GetActiveViews();
            foreach (var visibleCell in visibleCells)
            {
                var view = visibleCell.GetComponent<FontColorView>();
                view.Refresh();
            }
        }
    }

    internal sealed class FontColorModel
    {
        public readonly Color Color;
        public readonly Color CheckmarkColor;
        public bool IsSelected { get; set; }

        public FontColorModel(Color color, Color checkmarkColor)
        {
            Color = color;
            CheckmarkColor = checkmarkColor;
        }
    }
}
