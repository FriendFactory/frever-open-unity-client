using System;
using Bridge.Models.ClientServer.Assets;
using I2.Loc;
using Modules.VideoStreaming.UIAnimators;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class PrivacySettingsPage : GenericPage<PrivacySettingsPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private PageUiAnimator _pageUiAnimator;
        [SerializeField] private CharacterPrivacyButton _characterPrivacyButton;
        [SerializeField] private Button _blockedAccountsButton;
        [SerializeField] private GameObject _dataPrivacyHeader;
        [SerializeField] private GameObject _dataPrivacyWidget;

        [Header("Localization")]
        [SerializeField] private LocalizedString _pageHeader;

        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        
        private bool _isOtherPageOpenedOnTop;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.PrivacySettings;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            #if UNITY_IOS
            _dataPrivacyHeader.SetActive(false);
            _dataPrivacyWidget.SetActive(false);
            #endif
        }

        private void OnEnable()
        {
            _characterPrivacyButton.Access = _localUserDataHolder.UserProfile.CharacterAccess;
            
            _characterPrivacyButton.OnAccessSet += OnCharacterAccessSet;
            _blockedAccountsButton.onClick.AddListener(OnBlockedAccountsClicked);
        }

        private void OnDisable()
        {
            _characterPrivacyButton.OnAccessSet -= OnCharacterAccessSet;
            _blockedAccountsButton.onClick.RemoveListener(OnBlockedAccountsClicked);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _pageHeaderView.Init(new PageHeaderArgs(_pageHeader, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
            
        }

        protected override void OnDisplayStart(PrivacySettingsPageArgs args)
        {
            _pageUiAnimator.PrepareForDisplay();
            
            if (!_isOtherPageOpenedOnTop)
            {
                _pageUiAnimator.PlayShowAnimation(() => base.OnDisplayStart(args));
            }
            else
            {
                base.OnDisplayStart(args);
                _isOtherPageOpenedOnTop = false;
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            if (_isOtherPageOpenedOnTop)
            {
                base.OnHidingBegin(onComplete);
            }
            else
            {
                _pageUiAnimator.PlayHideAnimation(()=>OnHideAnimationFinished(onComplete));
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnBlockedAccountsClicked()
        {
            _isOtherPageOpenedOnTop = true;
            var pageArgs = new BlockedAccountsPageArgs();
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };
            _pageManager.MoveNext(PageId.BlockedAccountsPage, pageArgs, transitionArgs);
        }
     
        private void OnHideAnimationFinished(Action onComplete)
        {
            // Check if page is not destroyed yet (scene unloaded)
            if (this != null)
            {
                base.OnHidingBegin(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private async void OnCharacterAccessSet(CharacterAccess access)
        {
            await _localUserDataHolder.SetAccessForCharacter(access);
        }
    }
}