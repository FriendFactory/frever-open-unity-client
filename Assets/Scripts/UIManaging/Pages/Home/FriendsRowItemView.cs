using System;
using Abstract;
using Bridge.Services.UserProfile;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Home
{
    public class FriendsRowItemView : BaseContextDataView<Profile>
    {
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private TMP_Text _nicknameText;
        [SerializeField] private Button _button;

        [Inject] private PageManager _pageManager;
        
        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }
        
        protected override void OnInitialized()
        {
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.MainGroupId,
                UserMainCharacterId = ContextData.MainCharacter.Id,
                MainCharacterThumbnail = ContextData.MainCharacter.Files
            };
            
            _nicknameText.text = ContextData.NickName;
        
            _userPortraitView.Initialize(userPortraitModel);
        }
        
        private void OnClick()
        {
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(ContextData.MainGroupId, ContextData.NickName));
        }
    }
}