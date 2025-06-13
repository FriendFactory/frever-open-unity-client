using Bridge.Models.ClientServer.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Filtering.UI
{
    public class FilteringPanelUI : MonoBehaviour
    {
        [SerializeField]
        private ToggleGroup _sortingGroup;
        [SerializeField]
        private Button _applyButton;
        [SerializeField]
        private Button _resetButton;
        [SerializeField]
        private Toggle _freeToggle;
        [SerializeField]
        private Toggle _coinToggle;
        [SerializeField]
        private Toggle _gemToggle;
        [SerializeField]
        private List<Toggle> _sortingToggles;

        public event Action<FilteringSetting> FilteringApplied;

        private FilteringSetting _filteringSetting = new FilteringSetting();
        private FilteringSetting _initialSetting = new FilteringSetting();
        

        private void Awake()
        {
            _applyButton.onClick.AddListener(OnApplyClicked);
            _resetButton.onClick.AddListener(OnResetClicked);
            _freeToggle.onValueChanged.AddListener((isOn) => UpdateCostFiltering());
            _coinToggle.onValueChanged.AddListener((isOn) => UpdateCostFiltering());
            _gemToggle.onValueChanged.AddListener((isOn) => UpdateCostFiltering());

            foreach (var toggle in _sortingToggles)
            {
                toggle.onValueChanged.AddListener(OnSortingChanged);
            }
        }

        public void SetFilteringSettings(FilteringSetting setting)
        {
            _filteringSetting = setting;
            SaveInitialSetting(setting);
            _sortingToggles[(int)_filteringSetting.Sorting].isOn = true;

            var freeIsOn = setting.AssetPriceFilter == AssetPriceFilter.Free
                                || setting.AssetPriceFilter == AssetPriceFilter.WithFreeAndHardCurrency
                                || setting.AssetPriceFilter == AssetPriceFilter.WithFreeAndSoftCurrency;
            var coinIsOn = setting.AssetPriceFilter == AssetPriceFilter.WithSoftCurrency
                                || setting.AssetPriceFilter == AssetPriceFilter.WithFreeAndSoftCurrency
                                || setting.AssetPriceFilter == AssetPriceFilter.WithSoftCurrencyAndHardCurrency;
            var gemIsOn = setting.AssetPriceFilter == AssetPriceFilter.WithHardCurrency
                                || setting.AssetPriceFilter == AssetPriceFilter.WithFreeAndHardCurrency
                                || setting.AssetPriceFilter == AssetPriceFilter.WithSoftCurrencyAndHardCurrency;

            _freeToggle.isOn = freeIsOn;
            _coinToggle.isOn = coinIsOn;
            _gemToggle.isOn = gemIsOn;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _resetButton.interactable = !_filteringSetting.IsNoFiltersSetted;
        }

        public void Hide()
        {
            gameObject.SetActive(false);         
        }

        public void OnClickOutside()
        {
            SetFilteringSettings(_initialSetting);
            Hide();
        }

        private void SaveInitialSetting(FilteringSetting setting)
        {
            _initialSetting = setting;
        }

        private void OnSortingChanged(bool isOn)
        {
            if (isOn)
            {
                UpdateSortingSettings(); 
            }
            _resetButton.interactable = !_filteringSetting.IsNoFiltersSetted;
        }

        private void UpdateSortingSettings()
        {
            var indexOfSorting = _sortingToggles.FindIndex(x=>x.isOn);
            _filteringSetting.Sorting = (AssetSorting)indexOfSorting;
        }

        private void UpdateCostFiltering()
        {
            var filtering = 0;
            if(_freeToggle.isOn && _coinToggle.isOn && _gemToggle.isOn)
            {
                _filteringSetting.AssetPriceFilter = (AssetPriceFilter)filtering;
                return;
            }

            if (_freeToggle.isOn)
            {
                filtering += (int)AssetPriceFilter.Free;
            }

            if(_coinToggle.isOn)
            {
                filtering += (int)AssetPriceFilter.WithSoftCurrency;
            }

            if (_gemToggle.isOn)
            {
                filtering += (int)AssetPriceFilter.WithHardCurrency;
            }

            if (filtering > 3) filtering++;
            if (filtering == 3 && !_gemToggle.isOn) filtering++; // Fenrir 10.01.2023 / hotfix before we changed AssetPriceFilter values ​​to be a flag

            _filteringSetting.AssetPriceFilter = (AssetPriceFilter)filtering;

            _resetButton.interactable = !_filteringSetting.IsNoFiltersSetted;
        }

        private void OnApplyClicked()
        {
            FilteringApplied?.Invoke(_filteringSetting);
            Hide();
        }

        private void OnResetClicked()
        {
            _filteringSetting = new FilteringSetting();
            FilteringApplied?.Invoke(_filteringSetting);
            SetFilteringSettings(_filteringSetting);
            Hide();
        }
    }
}
