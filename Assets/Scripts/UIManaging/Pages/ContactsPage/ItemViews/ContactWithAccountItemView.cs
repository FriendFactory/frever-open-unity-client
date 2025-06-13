using System.Threading;
using Bridge;
using Bridge.Services.UserProfile;
using Modules.Contacts;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.ContactsPage.ItemViews
{
    internal abstract class ContactWithAccountItemView : ContactsItemView<ContactWithAccountItemModel>
    {
        [SerializeField] private Button _thumbnailButton;
        [SerializeField] private Button _nameButton;
        [SerializeField] private Image _defaultThumbnail;
        [SerializeField] private UserPortraitView _userPortrait;
        
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        [Inject] private FollowersManager _followersManager;
        
        private CancellationTokenSource _cancellationSource;
        
        protected Profile UserProfile;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected bool AllowGoToUserProfile { get; set; } = true;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            CancelLoading();
            DownloadUserProfile();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _thumbnailButton.onClick.RemoveListener(GoToProfile);
            _nameButton.onClick.RemoveListener(GoToProfile);
        }
        
        protected virtual void OnUserProfileDownloaded()
        {
            SetupUserThumbnail();
            
            if (AllowGoToUserProfile)
            {
                _thumbnailButton.onClick.AddListener(GoToProfile);
                _nameButton.onClick.AddListener(GoToProfile);
            }

            SubText.text = UserProfile.NickName;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void DownloadUserProfile()
        {
            _cancellationSource = new CancellationTokenSource();
            var result = await _bridge.GetProfile(ContextData.GroupId, _cancellationSource.Token);
            if (!result.IsSuccess) return;
            
            UserProfile = result.Profile;
            OnUserProfileDownloaded();
        }

        private void GoToProfile()
        {
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true
            };

            _pageManager.MoveNext
            (
                PageId.UserProfile,
                new UserProfileArgs(UserProfile.MainGroupId, UserProfile.NickName),
                transitionArgs
            );
        }

        private void OnDisable()
        {
            CancelLoading();
        }

        private void SetupUserThumbnail()
        {
            _defaultThumbnail.gameObject.SetActive(false);
            _userPortrait.gameObject.SetActive(false);
                
            if (UserProfile.MainCharacter == null)
            {
                _defaultThumbnail.gameObject.SetActive(true);
                return;
            }

            var model = new UserPortraitModel()
            {
                Resolution = Resolution._128x128,
                UserGroupId = UserProfile.MainGroupId,
                UserMainCharacterId = UserProfile.MainCharacter.Id,
                MainCharacterThumbnail = UserProfile.MainCharacter.Files
            };
            
            _userPortrait.gameObject.SetActive(true);
            _userPortrait.Initialize(model);
        }

        private void CancelLoading()
        {
            if(_cancellationSource == null) return;
            _cancellationSource.Cancel();
            _cancellationSource = null;
        }
    }
}