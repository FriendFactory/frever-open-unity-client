using System;
using System.Threading;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.Seasons;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Home
{
    internal sealed class SeasonPopup : BasePopup<SeasonPopupConfiguration>
    {
        [SerializeField] private SeasonThumbnailBackground _seasonThumbnail;
        [SerializeField] private TMP_Text _seasonName;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _exploreButton;
        [SerializeField] private Button _backgroundOverlayButton;
        
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private PageManager _pageManager;
        [Inject] private SeasonPageLocalization _localization; 

        private Action _exploreSeasonClicked;
        
        private async void Start()
        {
            _exploreButton.onClick.AddListener(OnExploreButtonClick);
            _backgroundOverlayButton.onClick.AddListener(OnClickedOutsidePopup);
            _seasonThumbnail.SetImageIndex(2);
            await _seasonThumbnail.InitializeAsync(Resolution._512x512, new CancellationToken());
            _seasonThumbnail.ShowContent();
        }

        public override void Show()
        {
            base.Show();
            var seasonName = _dataFetcher.CurrentSeason.Title;
            _seasonName.text = seasonName;
            _description.text = string.Format(_localization.SeasonExploreDescription, seasonName);
        }

        protected override void OnConfigure(SeasonPopupConfiguration configuration)
        {
            _exploreSeasonClicked = configuration.ExploreSeason;
        }

        private void OnExploreButtonClick()
        {
            if (_exploreSeasonClicked == null)
            {
                var args = new SeasonPageArgs(0);
                _pageManager.PageDisplayed += OnNextPageDisplayed;
                _pageManager.MoveNext(args);
            }
            else
            {
                Hide();
                _exploreSeasonClicked?.Invoke();    
            }
        }
        
        private void OnNextPageDisplayed(PageData pageData)
        {
            _pageManager.PageDisplayed -= OnNextPageDisplayed;
            Hide();
        }

        private void OnClickedOutsidePopup()
        {
            Configs.OnExploreSeasonIgnored?.Invoke();
            Hide();
        }
    }
}