using System;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Crews.Sidebar;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups
{
    internal class CrewTopListItem : BaseContextDataView<CrewTopListItemModel>
    {
        [SerializeField] private TMP_Text _place;
        [SerializeField] private GameObject _localUserCrewHighlight;
        [SerializeField] private ThumbnailLoader _portrait;
        [SerializeField] private TMP_Text _crewName;
        [SerializeField] private TMP_Text _score;
        [SerializeField] private Button _button;

        [Inject] private PopupManager _popupManger;
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IBridge _bridge;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private async void OnClick()
        {
            if (_localUser.UserProfile.CrewProfile.Id == ContextData.Crew.Id) return;

            var result = await _bridge.GetCrew(ContextData.Crew.Id, default);
            if (!result.IsSuccess) return;
            
            _popupManger.ClosePopupByType(PopupType.CrewTopList);
            _pageManager.MoveNext(new CrewInfoPageArgs(result.Model.ToCrewShortInfo()));
        }

        protected override void OnInitialized()
        {
            _localUserCrewHighlight.SetActive(_localUser.UserProfile.CrewProfile?.Id == ContextData.Crew.Id);
            _place.text = ContextData.Place >= 0 ? ContextData.Place.ToString() : "--";
            _crewName.text = ContextData.Crew.Name;
            _score.text = ContextData.Crew.TrophyScore.ToString();
            _portrait.Initialize(ContextData.Crew);
        }
    }
}