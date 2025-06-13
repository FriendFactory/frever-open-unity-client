using Modules.AppsFlyerManaging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

internal sealed class AdIdSettingsWidget : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Button _button;

    [Inject] private IAppsFlyerService _appsFlyerService;
    
    private void Awake()
    {
        _toggle.interactable = false;
        _toggle.isOn = _appsFlyerService.UserOptedOut;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }

    private void OnButtonClick()
    {
        _appsFlyerService.ToggleTracking(!_toggle.isOn);
        _toggle.isOn = !_toggle.isOn;
    }
}
