using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.MusicLicense
{
    internal class MusicLicensePanelToggle: MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _text;
        [Header("L10N")] 
        [SerializeField] private LocalizedString _allSoundsLoc;
        [SerializeField] private LocalizedString _commercialSoundsLoc;
        
        [Inject] private MusicLicenseManager _musicLicenseManager;

        public event Action<bool> ValueChanged; 

        private void Awake()
        {
            _toggle.isOn = false;
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            
            _musicLicenseManager.MusicLicenseChanged += OnMusicLicenseChanged;
        }
        
        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            
            _musicLicenseManager.MusicLicenseChanged -= OnMusicLicenseChanged;
        }

        private void Start()
        {
            OnMusicLicenseChanged(_musicLicenseManager.ActiveLicenseType);
        }

        public void SetState(bool isOn)
        {
            _toggle.isOn = isOn;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            ValueChanged?.Invoke(isOn);
        }

        private void OnMusicLicenseChanged(MusicLicenseType musicLicenseType)
        {
            var header = musicLicenseType == MusicLicenseType.AllSounds ? _allSoundsLoc : _commercialSoundsLoc;
            _text.text = header.ToString();
            _toggle.isOn = false;
        }
    }
}