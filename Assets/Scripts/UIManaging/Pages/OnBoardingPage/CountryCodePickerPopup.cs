using System;
using Modules.SignUp;
using UIManaging.Pages.Common.RegistrationInputFields.CountryCodePickers;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;

namespace UIManaging.Pages.OnBoardingPage
{
    public sealed class CountryCodePickerPopupConfiguration : PopupConfiguration
    {
        public CountryCode[] CountryCodes { get; }
        public CountryCode StartingCountryCode { get; }
        
        public CountryCodePickerPopupConfiguration(CountryCode[] countryCodes, CountryCode startingCountryCode, 
            Action<object> onClose) : base(PopupType.CountryCode, onClose)
        {
            CountryCodes = countryCodes;
            StartingCountryCode = startingCountryCode;
        }
    }

    internal sealed class CountryCodePickerPopup : BasePopup<CountryCodePickerPopupConfiguration>
    {
        [SerializeField] private CountryCodePicker _codePicker;

        public override void Show()
        {
            base.Show();

            _codePicker.Initialize(Configs.CountryCodes, Configs.StartingCountryCode);
            _codePicker.OnCountrySelected += OnCountrySelected;
            _codePicker.OnSelectionCanceled += OnSelectionCanceled;
        }

        private void OnDisable()
        {
            _codePicker.OnCountrySelected -= OnCountrySelected;
            _codePicker.OnSelectionCanceled -= OnSelectionCanceled;
        }

        protected override void OnConfigure(CountryCodePickerPopupConfiguration cfg)
        {
        }
        
        private void OnCountrySelected(CountryCode countryCode)
        {
            Hide(countryCode);
        }

        private void OnSelectionCanceled()
        {
            Hide(null);
        }
    }
}