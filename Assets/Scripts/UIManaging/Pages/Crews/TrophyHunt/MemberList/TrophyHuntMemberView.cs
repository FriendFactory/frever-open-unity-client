using System;
using Abstract;
using Bridge.Models.ClientServer.Crews;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.TrophyHunt
{
    public class TrophyHuntMemberView : BaseContextDataView<TrophyHuntMemberListModel>
    {
        [SerializeField] private UserPortraitView _userPortrait;
        [SerializeField] private TMP_Text _userNameText;
        [SerializeField] private TMP_Text _ratingText;
        [SerializeField] private TMP_Text _userScoreText;
        [SerializeField] private Button _profileButton;
        [SerializeField] private Graphic _background;

        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _highlightedColor;

        [Inject] private PageManager _pageManager;
        
        private void Awake()
        {
            _profileButton.onClick.AddListener(OpenProfilePage);
        }

        protected override void OnInitialized()
        {
            _userPortrait.Initialize(new UserPortraitModel
            {
                MainCharacterThumbnail = ContextData.Member.Group.MainCharacterFiles,
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.Member.Group.Id,
                UserMainCharacterId = ContextData.Member.Group.MainCharacterId ?? default
            });

            _userNameText.text = ContextData.Member.Group.Nickname;
            _userScoreText.text = ContextData.Member.Trophies.ToString();
            
            _ratingText.text = (ContextData.LeaderboardRank + 1).ToString();
            _background.color = ContextData.IsLocalUser ? _highlightedColor : _defaultColor;
        }

        private void OpenProfilePage()
        {
            _pageManager.MoveNext(new UserProfileArgs(ContextData.Member.Group.Id, ContextData.Member.Group.Nickname));
        }
    }
}