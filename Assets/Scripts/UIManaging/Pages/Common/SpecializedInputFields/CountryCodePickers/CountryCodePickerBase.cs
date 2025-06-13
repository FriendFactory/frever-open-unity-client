using System;
using System.Linq;
using Modules.SignUp;
using Newtonsoft.Json;
using SA.CrossPlatform.App;
using UnityEngine;

namespace UIManaging.Pages.Common.RegistrationInputFields.CountryCodePickers
{
    public abstract class CountryCodePickerBase : MonoBehaviour
    {
        private string _countryCodeText;

        protected CountryCode[] CountryCodes;
        protected CountryCode DefaultCountryCode;

        public void Initialize(CountryCode[] countryCodes, CountryCode defaultCountryCode)
        {
            CountryCodes = countryCodes;
            DefaultCountryCode = defaultCountryCode;
            SetCountryCodeText(defaultCountryCode.DialCode);
            Setup();
        }

        public string GetSelectedCountryCode()
        {
            return _countryCodeText;
        }

        public string RemoveDialCode(string text)
        {
            if(text is null) text = string.Empty;
            
            var result = text;
            
            foreach (var countryCode in CountryCodes)
            {
                var dialCode = $"+{countryCode.DialCode}";
                if (!text.StartsWith(dialCode)) continue;

                result = text.Replace(dialCode, string.Empty);
                break;
            }

            return result;
        }

        protected abstract void Setup();

        protected void SetCountryCodeText(int countryCode)
        {
            _countryCodeText = $"+{countryCode}";
        }
    }
}
