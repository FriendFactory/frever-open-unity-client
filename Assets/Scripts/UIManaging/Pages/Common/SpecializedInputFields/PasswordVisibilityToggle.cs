using UnityEngine;
using AdvancedInputFieldPlugin;
using Extensions;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    public class PasswordVisibilityToggle : MonoBehaviour
    {
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
        [SerializeField] private AdvancedInputField _inputField;
#else
        [SerializeField] private TMP_InputField _inputField;
#endif
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _iconEnabled;
        [SerializeField] private Image _iconDisabled;

        private void Awake()
        {
            if (_toggle == null)
            {
                _toggle = GetComponent<Toggle>();
            }
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(UpdateValue);
            _toggle.isOn = true;
            _inputField.CharacterValidation = CharacterValidation.ALPHANUMERIC;
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(UpdateValue);
        }

        private void UpdateValue(bool value)
        {
            // in other case password visibility is controlled by live decoration filter
            if (_inputField.ContentType == ContentType.PASSWORD)
            {
                _inputField.VisiblePassword = !value;
                _inputField.CharacterValidation = CharacterValidation.ALPHANUMERIC;
            }
            
            _iconEnabled.SetActive(value);
            _iconDisabled.SetActive(!value);
        }
    }
}