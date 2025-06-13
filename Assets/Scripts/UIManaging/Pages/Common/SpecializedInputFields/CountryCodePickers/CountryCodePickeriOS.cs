using System.Linq;
using SA.iOS.UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.RegistrationInputFields.CountryCodePickers
{
    internal sealed class CountryCodePickeriOS : CountryCodePickerBase
    {
        [SerializeField] private Button _countryCodeButton;

        private ISN_UIWheelPickerController _countryPickerController;

        protected override void Setup()
        {
            _countryCodeButton.onClick.AddListener(ShowCountryWheelPicker);

            var values = CountryCodes.Select(x => x.Name).ToList();
            _countryPickerController = new ISN_UIWheelPickerController(values);
        }

        private void ShowCountryWheelPicker()
        {
            _countryPickerController?.Show(OnWheelPickerResultUpdated);
        }

        private void OnWheelPickerResultUpdated(ISN_UIWheelPickerResult result)
        {
            if (!result.IsSucceeded || result.State != ISN_UIWheelPickerStates.Done) return;

            var pickerValue = result.Value;
            var countryCode = CountryCodes.First(x => x.Name == pickerValue).DialCode;
            SetCountryCodeText(countryCode);
        }
    }
}
