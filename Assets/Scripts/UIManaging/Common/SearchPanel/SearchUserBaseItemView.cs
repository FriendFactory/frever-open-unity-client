using System;
using Abstract;
using Bridge.Services.UserProfile;
using Common;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.SearchPanel
{
    public abstract class SearchUserBaseItemView : BaseContextDataView<Profile>
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _followersAmountText;
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private Button _profileButton;

        [Inject] private UserListItemLocalization _localization;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<Profile> ProfileButtonClicked;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public long Id => ContextData.MainGroupId;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            if (_profileButton != null)
            {
                _profileButton.onClick.AddListener(OnProfileButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (_profileButton != null)
            {
                _profileButton.onClick.RemoveListener(OnProfileButtonClicked);
            }
        }
        
        protected override void OnDestroy()
        {
            CleanUp();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            var nickname = string.IsNullOrWhiteSpace(ContextData.NickName) ? "<Name Is Null>" : ContextData.NickName;
            _nameText.text = nickname;
            RefreshFollowersAmountText();
            RefreshPortraitImage();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ProfileButtonClicked = null;
        }

        protected void RefreshFollowersAmountText()
        {
            if (_followersAmountText == null) return;
            _followersAmountText.text = string.Format(_localization.FollowersCounterTextFormat, ContextData.KPI.FollowersCount);
        }

        protected virtual void OnProfileButtonClicked()
        {
            ProfileButtonClicked?.Invoke(ContextData);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RefreshPortraitImage()
        {
            if (ContextData.MainCharacter == null) return;
            
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.MainGroupId,
                UserMainCharacterId = ContextData.MainCharacter.Id,
                MainCharacterThumbnail = ContextData.MainCharacter.Files,
            };

            _userPortraitView.Initialize(userPortraitModel);
        }
    }
}