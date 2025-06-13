using System;
using System.Threading;
using Modules.PhotoBooth.Profile;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.ProfilePhotoEditing;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using EditUsernamePageArgs = UIManaging.Pages.EditUsername.EditUsernamePageArgs;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.EditProfilePage.UI
{
    internal sealed class EditProfilePage : AnimatedGenericPage<EditProfilePageArgs>
    {
        [SerializeField] private PageHeaderActionView _pageHeaderActionView;
        [Header("Profile Background")]
        [SerializeField] private Button _profileBackgroundButton;
        [SerializeField] private UserPortrait _userBackground;
        [Header("Profile Photo")]
        [SerializeField] private Button _profilePhotoButton;
        [SerializeField] private UserPortrait _userPortrait;
        [Header("Username")]
        [SerializeField] private Button _usernameButton;
        [SerializeField] private TMP_Text _usernameText;
        [Header("Bio")]
        [SerializeField] private Button _bioButton;
        [SerializeField] private TMP_Text _bioText;
        [Header("Links")]
        [SerializeField] private Button _linksButton;

        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IScenarioManager _scenarioManager;

        private ButtonArgs _backButtonsArgs;
        private bool _buttonArgsInitialized;
        private CancellationTokenSource _tokenSource;
        private EditProfilePageLoc _loc;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.EditProfile;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _loc = GetComponent<EditProfilePageLoc>();

            if (!_buttonArgsInitialized)
            {
                _backButtonsArgs = new ButtonArgs(string.Empty, OnBackButtonClicked);
                _buttonArgsInitialized = true;
            }

            _pageHeaderActionView.Init(new PageHeaderActionArgs(_loc.PageHeader, _backButtonsArgs, new ButtonArgs(null, null)));

            _profileBackgroundButton.onClick.AddListener(OnProfileBackgroundButtonClicked);
            _profilePhotoButton.onClick.AddListener(OnProfilePhotoButtonClicked);
            _usernameButton.onClick.AddListener(OnUsernameButtonClicked);
            _bioButton.onClick.AddListener(OnBioButtonClicked);
            _linksButton.onClick.AddListener(OnLinksButtonClicked);
        }
        
        protected override void OnDisplayStart(EditProfilePageArgs args)
        {
            base.OnDisplayStart(args);
            _tokenSource = new CancellationTokenSource();

            RefreshUsername();
            RefreshBio();

            _userBackground.InitializeAsync(_localUserDataHolder.UserProfile, Resolution._256x256, _tokenSource.Token);
            _userBackground.ShowContent();

            _userPortrait.InitializeAsync(_localUserDataHolder.UserProfile, Resolution._128x128, _tokenSource.Token);
            _userPortrait.ShowContent();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void CleanUp()
        {
            _usernameButton.interactable = true;
            _bioButton.interactable = true;
            _linksButton.interactable = true;

            _userBackground.CleanUp();
            _userPortrait.CleanUp();

            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            _pageManager.MoveBack();
        }

        private void OnUsernameButtonClicked()
        {
            _usernameButton.interactable = false;
            IsOtherPageOpenedOnTop = true;
            
            _scenarioManager.ExecuteNicknameEditingScenario();
        }

        private void OnProfileBackgroundButtonClicked()
        {
            GoToProfilePhotoEditorPage(ProfilePhotoType.Background);
        }

        private void OnProfilePhotoButtonClicked()
        {
            GoToProfilePhotoEditorPage(ProfilePhotoType.Profile);
        }

        private void GoToProfilePhotoEditorPage(ProfilePhotoType profilePhotoType)
        {
            var args = new ProfilePhotoEditorPageArgs
            {
                Profile = _localUserDataHolder.UserProfile,
                PhotoType = profilePhotoType,
                OnConfirmBackPageId = Id
            };

            Manager.MoveNext(PageId.ProfilePhotoEditor, args);
        }

        private void RefreshUsername()
        {
            _usernameText.SetText(_localUserDataHolder.NickName);
        }

        private void RefreshBio()
        {
            var bio = string.IsNullOrEmpty(_localUserDataHolder.Bio)
                ? _loc.BioPlaceholder.ToString()
                : _localUserDataHolder.Bio;

            _bioText.SetText(bio);
        }

        private void OnBioButtonClicked()
        {
            _bioButton.interactable = false;
            IsOtherPageOpenedOnTop = true;

            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };

            _pageManager.MoveNext(PageId.EditBio, new EditBioPageArgs(_localUserDataHolder.Bio), transitionArgs);
        }

        private void OnLinksButtonClicked()
        {
            _linksButton.interactable = false;
            IsOtherPageOpenedOnTop = true;

            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };

            _pageManager.MoveNext(PageId.EditBioLinks, new EditBioLinksPageArgs(_localUserDataHolder.BioLinks), transitionArgs);
        }
    }
}