using System.Threading;
using Abstract;
using Bridge;
using Bridge.Services.UserProfile;
using Modules.CharacterManagement;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.FollowersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowerView : BaseContextDataView<FollowerViewModel>
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _followersAmountText;
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private Button _portraitProfileButton;
        [SerializeField] private Button _nicknameProfileButton;

        [Inject] private IBridge _bridge;
        [Inject] private FollowersManager _followersManager;
        [Inject] private CharacterManager _characterManager;
        [Inject] private PageManager _pageManager;
        
        private CancellationTokenSource _cancellationTokenSource;

        protected bool AllowGoToUserProfile { get; set; } = true;

        protected override void OnInitialized()
        {
            if (AllowGoToUserProfile)
            {
                _portraitProfileButton.onClick.AddListener(OnProfileButtonClicked);
                _nicknameProfileButton.onClick.AddListener(OnProfileButtonClicked);
            }

            var nickname = string.IsNullOrWhiteSpace(ContextData.UserProfile.NickName) ? "<Name Is Null>" : ContextData.UserProfile.NickName;
            _nameText.text = nickname.ToUpper();
            RefreshFollowersAmountText();
            RefreshPortraitImage();

            _followersManager.Followed += OnUnFollowedOrFollowed;
            _followersManager.UnFollowed += OnUnFollowedOrFollowed;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            _portraitProfileButton.onClick.RemoveListener(OnProfileButtonClicked);
            _nicknameProfileButton.onClick.RemoveListener(OnProfileButtonClicked);

            _followersManager.Followed -= OnUnFollowedOrFollowed;
            _followersManager.UnFollowed -= OnUnFollowedOrFollowed;
            
            _cancellationTokenSource?.Cancel();
        }

        private void OnUnFollowedOrFollowed(Profile profile)
        {
            if (profile.MainGroupId != ContextData.UserProfile.MainGroupId) return;
            OnFollowStateChanged(profile);
        }
        
        private void OnFollowStateChanged(Profile profile)
        {
            ContextData.UserProfile.KPI.FollowersCount = profile.KPI.FollowersCount;
            RefreshFollowersAmountText(); 
        }

        private void RefreshFollowersAmountText()
        {
            _followersAmountText.text = ContextData.IsContact ? "From your contacts" : $"Followers {ContextData.UserProfile.KPI.FollowersCount}";
        }

        private void RefreshPortraitImage()
        {
            if (ContextData.UserProfile.MainCharacter == null) return;
            
            var portraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.UserProfile.MainGroupId,
                UserMainCharacterId =  ContextData.UserProfile.MainCharacter.Id,
                MainCharacterThumbnail =  ContextData.UserProfile.MainCharacter.Files
            };
            
            _userPortraitView.Initialize(portraitModel);
        }

        private void OnProfileButtonClicked()
        {
            var userGroupId = ContextData.UserProfile.MainGroupId;
            var nickname = ContextData.UserProfile.NickName;
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(userGroupId, nickname));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanUp();    
        }
    }
}