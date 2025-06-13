using System.Threading;
using Abstract;
using Bridge;
using Bridge.Services.UserProfile;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups.TaggedUsers
{
    internal sealed class FeedTaggedUserItemView : BaseContextDataView<FeedTaggedUserItemModel>
    {
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private Button _openProfileButton;
        [SerializeField] private UserPortraitView _userPortraitView;

        [Inject] private PageManager _pageManager;
        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;

        private CancellationTokenSource _cancellationSource;
        private bool _failedToRetrieveProfile;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _cancellationSource = new CancellationTokenSource();
            SetupUiElements();
        }

        protected override void BeforeCleanup()
        {
            if (_cancellationSource != null) _cancellationSource.Cancel();
            base.BeforeCleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetupUiElements()
        {
            if (IsDestroyed) return;
            _userNameText.text = ContextData.NickName;
            _openProfileButton.onClick.RemoveAllListeners();
            _openProfileButton.onClick.AddListener(ShowProfile);
            UpdateUserPortrait();
        }

        private async void UpdateUserPortrait()
        {
            _userPortraitView.gameObject.SetActive(false);
            var profileResult = await _bridge.GetProfile(ContextData.GroupId, _cancellationSource.Token);
            if (profileResult.IsError || profileResult.IsRequestCanceled) _failedToRetrieveProfile = true;
            
            RefreshPortraitImage(profileResult.Profile);
        }

        private void ShowProfile()
        {
            if (_failedToRetrieveProfile)
            {
                ShowProfileErrorPopup();
                return;
            }
            
            _popupManager.ClosePopupByType(PopupType.TaggedUsers);
            
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(ContextData.GroupId, ContextData.NickName));
        }

        private void RefreshPortraitImage(Profile profile)
        {
            if (profile == null) return;
            
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = profile.MainGroupId,
                UserMainCharacterId = profile.MainCharacter.Id,
                MainCharacterThumbnail = profile.MainCharacter.Files,
            };

            _userPortraitView.gameObject.SetActive(true);
            _userPortraitView.Initialize(userPortraitModel);
        }
        
        private void ShowProfileErrorPopup()
        {
            var config = new AlertPopupConfiguration
            {
                PopupType  = PopupType.AlertWithTitlePopup,
                ConfirmButtonText = "Ok",
                Description = "Oops looks like something is wrong, we can't find this user! ",
                Title = "Error",
            };
                
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

    }
}
