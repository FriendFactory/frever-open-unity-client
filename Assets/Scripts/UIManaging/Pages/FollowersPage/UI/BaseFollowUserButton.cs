using System;
using System.Collections;
using System.Collections.Generic;
using Abstract;
using Bridge;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI
{
    public abstract class BaseFollowUserButton<TArgs> : BaseContextDataView<TArgs> where TArgs: FollowUserButtonArgs
    {
        [SerializeField] private Transform _followButtonParent;
        [SerializeField] private Transform _unfollowButtonParent;
        [SerializeField] private TextMeshProUGUI _followButtonText;
        [SerializeField] private TextMeshProUGUI _unfollowButtonText;
        [SerializeField] private Button _followButton;
        [SerializeField] private Button _unfollowButton;

        [Inject] protected UserListItemLocalization _localization;
        
        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        
        private LayoutGroup _layoutGroup;

        protected virtual string FollowButtonText => _localization.FollowButton;
        protected virtual string FollowBackButtonText => _localization.FollowBackButton;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void OnEnable()
        {
            StartCoroutine(UpdateLayout());
        }
        
        protected override void OnInitialized()
        {
            RefreshUI(ContextData.IsFollowing);
        }

        protected void RefreshUI(bool follow)
        {
            _unfollowButtonParent.gameObject.SetActive(follow);
            _followButtonParent.gameObject.SetActive(!follow);
            _unfollowButton.onClick.RemoveListener(OnUnfollowButtonClicked);
            _followButton.onClick.RemoveListener(OnFollowButtonClicked);
            
            var isLocalUser = ContextData.UserGroupId == _bridge.Profile.GroupId;
            if (isLocalUser)
            {
                return;
            }
            
            if (follow)
            {
                var isFriends = ContextData.IsFollowedBy;
                _unfollowButton.onClick.AddListener(OnUnfollowButtonClicked);
                _unfollowButtonText.text = isFriends ? _localization.FriendsButton : _localization.FollowingButton;
            }
            else
            {
                _followButton.onClick.AddListener(OnFollowButtonClicked);
                _followButtonText.text = ContextData.IsFollowedBy ? FollowBackButtonText : FollowButtonText;
            }

            if (gameObject.activeInHierarchy)
            {
                // layout might be updated after enabling of GO, so, we need to fire layout update once again
                // meanwhile, initialization could happen before enabling, so, we need to keep both layout updates
                StartCoroutine(UpdateLayout());
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _unfollowButton.onClick.RemoveListener(OnUnfollowButtonClicked);
            _followButton.onClick.RemoveListener(OnFollowButtonClicked);
        }
        
        protected virtual void OnUnfollowButtonClicked()
        {
            if (ContextData.IsFollowedBy)
            {
                ShowUnfollowActionSheet();
            }
            else
            {
                OnUnfollowButtonClickedInternal();
            }
        }
        
        protected abstract void OnFollowButtonClicked();

        protected abstract void OnUnfollowButtonClickedInternal();
        

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowUnfollowActionSheet()
        {
            var configuration = new ActionSheetUserConfiguration
            {
                PopupType = PopupType.ActionSheetUser,
                Profile = ContextData.Profile,
                Variants = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>(_localization.UnfollowPopupUnfollowOption, OnUnfollowButtonClickedInternal)
                }
            };
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        private IEnumerator UpdateLayout() // force update for button scale
        {
            yield return null;
            
            _followButtonText.text += " "; // thanks, i hate it
        }

        protected void OnFollowStatusUpdated(bool following)
        {
            ContextData.IsFollowing = following;
        }
    }
}