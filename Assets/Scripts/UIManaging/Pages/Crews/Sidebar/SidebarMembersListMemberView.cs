using System;
using System.Globalization;
using System.Threading;
using Abstract;
using Common;
using Extensions;
using Extensions.DateTime;
using Modules.Crew;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common;
using UIManaging.Localization;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMembersListMemberView : BaseContextDataView<SidebarMembersListMemberModel>
    {
        [SerializeField] private RawImage _portrait;
        [SerializeField] private GameObject _statusDot;
        [SerializeField] private TMP_Text _username;
        [SerializeField] private TMP_Text _lastSeen;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private GameObject _optionsButtonIcon;

        [Space] 
        [SerializeField] private AnimatedSkeletonBehaviour _animatedSkeletonBehaviour;
        
        private CancellationTokenSource _tokenSource;

        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;
        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private CrewService _crewService;
        [Inject] private CrewPageLocalization _crewPageLocalization;

        private void OnEnable()
        {
            _optionsButton.onClick.AddListener(OnOptionsButtonClicked);
            _animatedSkeletonBehaviour.Play();
            _tokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _optionsButton.onClick.RemoveAllListeners();
            _tokenSource.CancelAndDispose();
            _tokenSource = null;
        }

        protected override void OnInitialized()
        {
            _statusDot.SetActive(ContextData.IsOnline);
            UpdateOnlineText();

            _optionsButton.interactable = _crewService.LocalUserIsAdmin;
            _optionsButtonIcon.SetActive(_crewService.LocalUserIsAdmin);

            if (!ContextData.IsInitialized && _tokenSource != null) 
            {
                ContextData.OnDataChange += OnModelChange;
                _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(ContextData.GroupId, Resolution._128x128, ContextData.UpdateWithFetchedData, null, _tokenSource.Token);
                
                return;
            }

            OnModelChange(ContextData);
        }

        private void UpdateOnlineText()
        {
            if (ContextData.IsOnline)
            {
                _lastSeen.text = _crewPageLocalization.OnlineNow;
                return;
            }

            _lastSeen.text = string.Format(_crewPageLocalization.LastSeenOnlineTimeFormat,
                                           ContextData.LastSeen.OnlineTimeText());
        }

        private void OnModelChange(SidebarMembersListMemberModel model)
        {
            _username.text = model.UserName;
            _portrait.texture = model.ProfileImage;
            
            _animatedSkeletonBehaviour.FadeOut();
            ContextData.OnDataChange -= OnModelChange;
        }

        private void OnOptionsButtonClicked()
        {
            var cfg = new ManageCrewMemberPopupConfiguration
            {
                CrewId = ContextData.CrewId,
                MemberGroupId = ContextData.GroupId,
                MemberRoleId = ContextData.RoleId,
                NickName = ContextData.UserName,
                LastLogin = ContextData.LastSeen.OnlineTimeText(),
                Joined = ContextData.Joined
            };
            
            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(PopupType.ManageCrewMember);
        }
    }
}