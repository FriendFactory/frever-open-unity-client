using System;
using JetBrains.Annotations;
using Modules.MusicLicenseManaging;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.MusicLicense
{
    [UsedImplicitly]
    internal sealed class MusicLicenseManager: IInitializable, IDisposable
    {
        private readonly MusicLicenseTypePanel _musicLicenseTypePanel;
        private readonly MusicLicenseValidator _musicLicenseValidator;

        public MusicLicenseType ActiveLicenseType { get; private set; }
        public bool PremiumSoundsEnabled => _musicLicenseValidator.IsPremiumSoundsEnabled;
        
        public event Action<MusicLicenseType> MusicLicenseChanged;

        public MusicLicenseManager(MusicLicenseTypePanel musicLicenseTypePanel, MusicLicenseValidator musicLicenseValidator)
        {
            _musicLicenseTypePanel = musicLicenseTypePanel;
            _musicLicenseValidator = musicLicenseValidator;
        }

        public void Initialize()
        {
            _musicLicenseTypePanel.MusicLicenseChanged += OnLicenseToggleValueChanged;
        }

        public void Dispose()
        {
            _musicLicenseTypePanel.MusicLicenseChanged -= OnLicenseToggleValueChanged;
        }

        private void OnLicenseToggleValueChanged(MusicLicenseType licenseType)
        {
            if (ActiveLicenseType == licenseType) return;
            
            ActiveLicenseType = licenseType;
            
            MusicLicenseChanged?.Invoke(licenseType);
        }
    }
}