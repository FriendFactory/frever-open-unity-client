using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class OptionsPopup: BasePopup<OptionsPopupConfiguration>
    {
        [SerializeField] private List<OptionColorData> _colors;
        [SerializeField] private OptionView _optionPrefab;
        [SerializeField] private Transform _optionsContainer;
        
        private readonly List<OptionView> _optionsViewPool = new List<OptionView>();
        private readonly List<OptionView> _activeViews = new List<OptionView>();

        protected override void OnConfigure(OptionsPopupConfiguration configuration)
        {
            PoolAllActiveViews();
            foreach (var option in configuration.Options)
            {
                var view = GetView();
                _activeViews.Add(view);
                view.SetActive(true);
                var textColor = _colors.First(x => x.ColorType == option.Color).ColorValue;
                view.Init(option.Name, option.OnSelected, textColor);
            }
        }

        private OptionView GetView()
        {
            if (!_optionsViewPool.Any())
            {
                return Instantiate(_optionPrefab, _optionsContainer);
            }
            var output = _optionsViewPool[0];
            _optionsViewPool.RemoveAt(0);
            return output;

        }

        private void PoolAllActiveViews()
        {
            foreach (var activeView in _activeViews)
            {
                activeView.SetActive(false);
            }
            _optionsViewPool.AddRange(_activeViews);
            _activeViews.Clear();
        }
        
        [Serializable]
        private struct OptionColorData
        {
            public OptionColor ColorType;
            public Color ColorValue;
        }
    }
}