using UIManaging.Pages.Common.SongOption.MusicLicense;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    public class MusicGalleryView: BaseContextlessView
    {
        [SerializeField] private MusicTypeGalleryViewBase _allMusicGalleryView;
        [SerializeField] private MusicTypeGalleryViewBase _commercialMusicGalleryView;
        
        [Inject] private MusicLicenseManager _musicLicenseManager; 
        
        protected override void OnInitialized() { }

        protected override void OnActivated()
        {
            _musicLicenseManager.MusicLicenseChanged += OnMusicLicenseChanged;
        }

        protected override void OnDeactivated()
        {
            _musicLicenseManager.MusicLicenseChanged -= OnMusicLicenseChanged;
        }

        private void OnMusicLicenseChanged(MusicLicenseType musicLicenseType)
        {
            switch (musicLicenseType)
            {
                case MusicLicenseType.AllSounds:
                    if (!_allMusicGalleryView.IsInitialized) _allMusicGalleryView.Initialize();
                    _commercialMusicGalleryView.Hide();
                    _allMusicGalleryView.Show();
                    break;
                case MusicLicenseType.CommercialSounds:
                    if (!_commercialMusicGalleryView.IsInitialized) _commercialMusicGalleryView.Initialize();
                    _allMusicGalleryView.Hide();
                    _commercialMusicGalleryView.Show();
                    break;
            }
        }

        protected override void OnShow()
        {
            OnMusicLicenseChanged(_musicLicenseManager.ActiveLicenseType);
        }

        protected override void OnHide()
        {
            _allMusicGalleryView.Hide();
            _commercialMusicGalleryView.Hide();
        }

        protected override void BeforeCleanUp()
        {
            if (_allMusicGalleryView.IsInitialized)
            {
                _allMusicGalleryView.CleanUp();
            }

            if (_commercialMusicGalleryView.IsInitialized)
            {
                _commercialMusicGalleryView.CleanUp();
            }
        }
    }
}