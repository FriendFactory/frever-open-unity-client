using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.MusicLicense
{
    internal enum MusicLicenseType
    {
        AllSounds = 0,
        CommercialSounds = 1,
    }

    internal sealed class MusicLicenseTypePanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panelGroup;
        [SerializeField] private Button _clickOutsideButton;
        [SerializeField] private MusicLicenseToggle[] _toggles;

        private MusicLicenseType ActiveLicenseType { get; set; } 

        public event Action<MusicLicenseType> MusicLicenseChanged;
        public event Action ClickedOutside;

        private void OnEnable()
        {
            _toggles.ForEach(toggle => toggle.ToggleActivated += OnToggleActivated);
            
            _clickOutsideButton.onClick.AddListener(OnClickOutside);
        }

        private void OnDisable()
        {
            _toggles.ForEach(toggle => toggle.ToggleActivated -= OnToggleActivated);
            
            _clickOutsideButton.onClick.RemoveListener(OnClickOutside);
        }

        // delegate control to Animator
        public void SetActive(bool state)
        {
            _panelGroup.SetActive(state);
        }

        private void OnToggleActivated(MusicLicenseType musicLicenseType)
        {
            if (musicLicenseType == ActiveLicenseType) return;

            ActiveLicenseType = musicLicenseType;
            
            MusicLicenseChanged?.Invoke(musicLicenseType);
        }

        private void OnClickOutside() => ClickedOutside?.Invoke();
    } 
}