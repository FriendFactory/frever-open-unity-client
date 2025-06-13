using Common;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Toggles
{
    [RequireComponent(typeof(Toggle))]
    public class TogglePlayerPrefsSync: MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private bool _initialState;
        [SerializeField] private string _playerPrefsKey = "KeyId";
        
        private PlayerPrefsBooleanFlag _playerPrefsFlag;
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_toggle)
            {
                _toggle = GetComponent<Toggle>();
            }
        }
        #endif

        private void Awake()
        {
            _playerPrefsFlag = new PlayerPrefsBooleanFlag(_playerPrefsKey);

            _toggle.isOn = _playerPrefsFlag.TryGetValue(out var isOn) ? isOn : _initialState;
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }
        
        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            _playerPrefsFlag.Set(isOn);
        }
    }
}