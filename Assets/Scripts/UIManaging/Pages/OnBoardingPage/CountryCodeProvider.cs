using System;
using System.Linq;
using Modules.SignUp;
using Newtonsoft.Json;
using SA.CrossPlatform.App;
using SA.iOS.UIKit;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.OnBoardingPage
{
    public sealed class CountryCodeProvider
    {
        private const string FALLBACK_DEFAULT_COUNTRY_CODE = "GB";
        private const string COUNTRY_CODES_PATH = "CountryCodes";
        
        private CountryCode[] _countryCodes;
        private ISN_UIWheelPickerController _iosWheelPicker;

        public event Action<int> CountryCodeChanged;
        public CountryCode SelectedCode { get; private set; }

        public CountryCode DefaultCountryCode => GetCountryCodeInfo(UM_Locale.GetCurrentLocale().CountryCode)
                                              ?? GetCountryCodeInfo(FALLBACK_DEFAULT_COUNTRY_CODE);

        public CountryCodeProvider()
        {
            LoadCountryCodes();
            
            if (Application.platform == RuntimePlatform.IPhonePlayer) InitialiseIOsCodePicker();
        }

        public void ShowPicker(PopupManager popupManager)
        {
            SelectedCode ??= DefaultCountryCode;
            
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _iosWheelPicker.Show(OnIOsPickerValueChange);
                return;
            }
            
            popupManager.SetupPopup(new CountryCodePickerPopupConfiguration(_countryCodes, SelectedCode, OnCountryCodePopupClosed));
            popupManager.ShowPopup(PopupType.CountryCode, true);
        }

        public void HidePicker(PopupManager popupManager)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer) return;
            
            popupManager.ClosePopupByType(PopupType.CountryCode);
        }
        
        private void LoadCountryCodes()
        {
            if (_countryCodes != null && _countryCodes.Length != 0) return;
            
            var countryCodesTxt = Resources.Load<TextAsset>(COUNTRY_CODES_PATH);
            _countryCodes = JsonConvert.DeserializeObject<CountryCode[]>(countryCodesTxt.text);
        }
        
        private void InitialiseIOsCodePicker()
        {
            if (_iosWheelPicker != null) return;
            var values = _countryCodes.Select(x => x.Name).ToList();
            _iosWheelPicker = new ISN_UIWheelPickerController(values);
        }
        
        private void OnIOsPickerValueChange(ISN_UIWheelPickerResult result)
        {
            if (!result.IsSucceeded || result.State != ISN_UIWheelPickerStates.Done) return;

            var pickerValue = result.Value;
            SelectedCode = _countryCodes.First(x => x.Name == pickerValue);
            CountryCodeChanged?.Invoke(SelectedCode.DialCode);
        }
        
        private void OnCountryCodePopupClosed(object result)
        {
            if (result is null) return;

            SelectedCode = (CountryCode)result;
            CountryCodeChanged?.Invoke(SelectedCode.DialCode);
        }
        
        private CountryCode GetCountryCodeInfo(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode)) return null;
            var isIso3Code = countryCode.Length == 3; //iso3 code contains 3 characters
            if (isIso3Code)
            {
                return _countryCodes.FirstOrDefault(code => string.Equals(code.IsoCode3, countryCode, StringComparison.InvariantCultureIgnoreCase));
            }
            return _countryCodes.FirstOrDefault(code => string.Equals(code.IsoCode2, countryCode, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}