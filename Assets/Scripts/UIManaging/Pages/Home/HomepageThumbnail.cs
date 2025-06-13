using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Extensions;
using Modules.AssetsStoraging.Core;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Home
{
    public class HomepageThumbnail : MonoBehaviour
    {
        private const float IPHONE_ASPECT_RATIO = 0.463f;
        
        [SerializeField] private UserPortrait _thumbnailPortrait;
        [SerializeField] private List<PortraitAdjustment> _portraitPosition;
        [SerializeField] private SeasonThumbnailBackground _seasonThumbnailBackground;
        
        [Inject] private IBridge _bridge;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private LocalUserDataHolder _userData;

        private CancellationTokenSource _tokenSource;
        private long? _currentSeasonId;
        
        private Vector3 _alternativePortraitScale => new Vector3(1.25f, 1.25f);

        public void Initialize()
        {
            _currentSeasonId = _dataFetcher.CurrentSeason?.Id;
            UpdateImages();
        }

        public void UpdateImages()
        {
            _tokenSource?.CancelAndDispose();
            _tokenSource = new CancellationTokenSource();
            
            LoadPortraitAsync(_tokenSource.Token);
            LoadBackgroundAsync(_tokenSource.Token);
        }

        private void OnEnable()
        {
            _dataFetcher.OnSeasonFetched += RefreshBackground;
        }

        private void OnDisable()
        {
            _dataFetcher.OnSeasonFetched -= RefreshBackground;

            if (_tokenSource == null)
            {
                return;
            }
            
            _tokenSource?.CancelAndDispose();
            _tokenSource = null;
        }

        private void OnDestroy()
        {
            _thumbnailPortrait.CleanUp();
            _seasonThumbnailBackground.CleanUp();
        }

        private async void LoadPortraitAsync(CancellationToken token)
        {
            if (_userData.UserProfile is null)
            {
                await _userData.DownloadProfile();

                if (_tokenSource == null || _tokenSource.IsCancellationRequested) return;

            } 
            var userProfile = _userData.UserProfile;
            
            await _thumbnailPortrait.InitializeAsync(userProfile, Resolution._512x512, token, true);

            if (_tokenSource == null || _tokenSource.IsCancellationRequested) return;
   
            var screenRation = (float)Screen.width / Screen.height;
            if (screenRation > IPHONE_ASPECT_RATIO)
            {
                _thumbnailPortrait.transform.localScale = _alternativePortraitScale;
            }
            _thumbnailPortrait.ShowContent();
            var race = _dataFetcher.MetadataStartPack.GetRaceByGenderId(userProfile.MainCharacter.GenderId);
            var adjustment = _portraitPosition.First(x => x.RaceId == race.Id);
            _thumbnailPortrait.GetComponent<RectTransform>().anchoredPosition = adjustment.AnchoredPosition;
            _thumbnailPortrait.transform.localScale = adjustment.Scale * Vector3.one;
        }

        private async void LoadBackgroundAsync(CancellationToken token)
        {
            await _seasonThumbnailBackground.InitializeAsync(Resolution._512x512, token);

            if (_tokenSource == null || _tokenSource.IsCancellationRequested) return;
           
            _seasonThumbnailBackground.ShowContent();
        }

        private void RefreshBackground()
        {
            if (_dataFetcher.CurrentSeason?.Id == _currentSeasonId)
            {
                return;
            }

            _currentSeasonId = _dataFetcher.CurrentSeason?.Id;
            
            _seasonThumbnailBackground.CleanUp();
            
            _tokenSource?.CancelAndDispose();
            _tokenSource = new CancellationTokenSource();
            
            LoadPortraitAsync(_tokenSource.Token);
            LoadBackgroundAsync(_tokenSource.Token);
        }
    }

    [Serializable]
    internal struct PortraitAdjustment
    {
        public long RaceId;
        public Vector2 AnchoredPosition;
        public float Scale;
    }
}