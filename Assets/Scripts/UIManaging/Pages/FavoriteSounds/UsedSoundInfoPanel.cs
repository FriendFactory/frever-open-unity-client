using System;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Common.Abstract;
using Extensions;
using I2.Loc;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.FavoriteSounds;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    public class UsedSoundInfoPanel: BaseContextPanel<UsedSoundItemModel>
    {
        [SerializeField] private TMP_Text _soundName;
        [SerializeField] private TMP_Text _artistName;
        [SerializeField] private Button _openSoundUploaderProfileButton;
        [SerializeField] private Image _arrow;
        [SerializeField] private TMP_Text _usageCount;
        [Header("L10N")] 
        [SerializeField] private LocalizedString _usageCountLoc;

        [Inject] private PageManager _pageManager;

        private GroupShortInfo _owner;
        
        protected override bool IsReinitializable => true;

        private void Awake()
        {
            _openSoundUploaderProfileButton.SetActive(false);
            _arrow.SetActive(false);
        }

        protected override void OnInitialized()
        {
            _owner = GetOwner(ContextData.Sound);
            
            SetSoundName(ContextData.Sound);
            SetArtistName(ContextData.Sound);
            SetUsageCount(ContextData.UsageCount);
            
            if (_owner == null) return;
            
            _openSoundUploaderProfileButton.SetActive(true);
            _arrow.SetActive(true);

            _openSoundUploaderProfileButton.onClick.AddListener(OpenProfilePage);
        }

        protected override void BeforeCleanUp()
        {
            if (_owner != null)
            {
                _openSoundUploaderProfileButton.onClick.RemoveListener(OpenProfilePage);
                
                _openSoundUploaderProfileButton.SetActive(false);
                _arrow.SetActive(false);
            }
            
            _owner = null;
        }

        private void OpenProfilePage()
        {
            if (_owner == null) return;
            
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(_owner.Id, _owner.Nickname));
        }

        private void SetSoundName(IPlayableMusic sound) => _soundName.text = sound.GetSoundName();
        private void SetUsageCount(int usageCount) => _usageCount.text = $"{usageCount.ToString()} {(string)_usageCountLoc}";

        private void SetArtistName(IPlayableMusic sound)
        {
            _artistName.text = _owner != null ? _owner.Nickname : sound.GetArtistName();
        }

        private GroupShortInfo GetOwner(IPlayableMusic sound)
        {
            switch (sound)
            {
                case FavouriteMusicInfo favoriteSound:
                    return favoriteSound.Type == SoundType.UserSound ? favoriteSound.Owner : null;
                case UserSoundFullInfo userSound:
                    return userSound.Owner;
                case SongInfo _:
                case ExternalTrackInfo _:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sound));
            }
        }
    }
}