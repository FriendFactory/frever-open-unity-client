using System;
using System.Linq;
using Modules.SignUp;
using UIManaging.Common.ScrollSelector;
using UnityEngine;

namespace UIManaging.Pages.Common.RegistrationInputFields.CountryCodePickers
{
    public class CountryCodePicker : CountryCodePickerBase
    {
        [SerializeField] private CountryCodesScrollView _countryCodesView;

        public event Action<CountryCode> OnCountrySelected;
        public event Action OnSelectionCanceled;
        
        protected override void Setup()
        {
            _countryCodesView.Initialize(GetCountryCodesScrollSelectorModel());

            _countryCodesView.OnCanceled += OnCanceled;
            _countryCodesView.OnConfirmed += OnConfirmed;
        }

        private void OnDisable()
        {
            _countryCodesView.OnCanceled -= OnCanceled;
            _countryCodesView.OnConfirmed -= OnConfirmed;
        }

        private ScrollSelectorModel GetCountryCodesScrollSelectorModel()
        {
            var initialDataIndex = Array.IndexOf(CountryCodes, DefaultCountryCode);
            var countryCodesItems = CountryCodes.Select(code => new ScrollSelectorItemModel(code.Name)).ToArray();
            var countryCodesModel = new ScrollSelectorModel(countryCodesItems, Mathf.Max(0,initialDataIndex));

            return countryCodesModel;
        }

        private void OnCanceled()
        {
            OnSelectionCanceled?.Invoke();
        }

        private void OnConfirmed(int dataIndex)
        {
            var index = Mathf.Clamp(dataIndex, 0, CountryCodes.Length - 1);
            var countryCode = CountryCodes[index];

            OnCountrySelected?.Invoke(countryCode);
        }
    }
}