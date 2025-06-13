using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.MusicLicense
{
    internal class MusicLicenseToggle : MonoBehaviour
    {
        [SerializeField] private MusicLicenseType _musicLicenseType;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private bool _initialToggleState = true;

        public event Action<MusicLicenseType> ToggleActivated;

        private void Awake()
        {
            _toggle.isOn = _initialToggleState;
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }
        
        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void Set(bool state)
        {
            _toggle.isOn = state;
        }

        private void OnValueChanged(bool isOn)
        {
            if (isOn)
            {
                ToggleActivated?.Invoke(_musicLicenseType);
            }
        }
    }
}