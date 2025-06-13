using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer;
using Bridge.Models.VideoServer;
using Bridge.Services.UserProfile;
using Extensions;
using Models;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.PublishPage.VideoDetails.Attributes;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal abstract class PostVideoSettingsPanelBase : VideoSettingsPanelBase
    {
        [SerializeField] private PrivacyButton _privacyButton;
        [SerializeField] private Button _externalLinksButton;
        [SerializeField] private TextMeshProUGUI _externalLinksText;
        [SerializeField] private Color _externalLinksColorDiscord;
        [SerializeField] private Color _externalLinksColorOther;
        [SerializeField] protected PublishPageLocalization _localization;
        
        [Inject] protected IBridge Bridge;
        [Inject] protected LocalUserDataHolder LocalUserDataHolder;
        [Inject] protected PageManager PageManager;
        [Inject] protected VideoPostAttributesModel VideoPostAttributesModel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public string ExternalLink { get; set; }
        public ExternalLinkType ExternalLinkType = ExternalLinkType.Invalid;
        
        private bool PrivacyButtonEnabled
        {
            get => _privacyButton.enabled;
            set => _privacyButton.enabled = value;
        }

        public List<GroupShortInfo> SelectedUsers
        {
            get => _privacyButton.SelectedUsers;
            set => _privacyButton.SelectedUsers = value;
        }

        public List<GroupShortInfo> TaggedUsers
        {
            get => _privacyButton.TaggedUsers;
            set
            {
                _privacyButton.TaggedUsers = value;
                VideoPostAttributesModel.TaggedUsersCount.Value = value.Count;
            }
        }

        public VideoAccess Access
        {
            get => _privacyButton.Access;
            protected set
            {
                _privacyButton.Access = value;
                VideoPostAttributesModel.VideoAccess.Value = value;
            }
        }

        public abstract IPublishVideoContentAccessSettings ContentAccessSettings { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Refresh()
        {
            RefreshExternalLinkButtonState();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void Init()
        {
            base.Init();
            
            _privacyButton.Access = Access;
            _privacyButton.OnAccessSet += OnPrivacyValueChanged;
            
            if (TaggedUsers == null && SelectedUsers == null)
            {
                SetPrivacyButtonProfiles(GetTaggedMemberGroupIds(), default); //todo: pass non default token
            }
            
            SetupExternalLinksButton();
        }

        protected virtual void OnPrivacyValueChanged(VideoAccess access)
        {
            VideoPostAttributesModel.VideoAccess.Value = access;
        }

        protected abstract long[] GetTaggedMemberGroupIds();
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void SetPrivacyButtonProfiles(long[] taggedIds, CancellationToken token)
        {
            PrivacyButtonEnabled = false;

            if (taggedIds == null || taggedIds.Length == 0)
            {
                TaggedUsers = new List<GroupShortInfo>();
                SelectedUsers = new List<GroupShortInfo>();
                PrivacyButtonEnabled = true;
                return;
            }
            
            var result = await Bridge.GetProfilesShortInfo(taggedIds, token);

            if (result.IsError)
            {
                Debug.LogError($"Failed to receive tagged and mentioned profiles, reason: {result.ErrorMessage}");
                TaggedUsers = new List<GroupShortInfo>();
                SelectedUsers = new List<GroupShortInfo>();
                PrivacyButtonEnabled = true;
                return;
            }

            if (result.IsSuccess)
            {
                TaggedUsers = result.Profiles.ToList();
                SelectedUsers = result.Profiles.ToList();
                PrivacyButtonEnabled = true;
            }
        }
        
        private void OnMentionSelected(Profile profile)
        {
            if (!profile.UserFollowsYou || !profile.YouFollowUser || profile.MainGroupId == Bridge.Profile.Id)
            {
                return;
            }
            
            AddSelectedUsers(new GroupShortInfo
            {
                Id = profile.MainGroupId,
                Nickname = profile.NickName,
                MainCharacterId = profile.MainCharacter.Id,
                MainCharacterFiles = profile.MainCharacter.Files
            });
        }
        
        private void AddSelectedUsers(GroupShortInfo user)
        {
            SelectedUsers.Add(user);
            _privacyButton.UpdateText();
        }
        
        private void SetupExternalLinksButton()
        {
            _externalLinksButton.onClick.AddListener(OpenExternalLinksPage);
            _externalLinksButton.SetActive(LocalUserDataHolder.IsStarCreator); // only for star creators
            RefreshExternalLinkButtonState();
        }

        private void RefreshExternalLinkButtonState()
        {
            _externalLinksText.text = ExternalLinkType == ExternalLinkType.Invalid ? "" : _localization.LinkAddedText;
            _externalLinksText.color = ExternalLinkType == ExternalLinkType.Discord
                ? _externalLinksColorDiscord
                : _externalLinksColorOther;
        }
        
        private void OpenExternalLinksPage()
        {
            PageManager.MoveNext(new ExternalLinksPageArgs
            {
                IsActive = ExternalLinkType != ExternalLinkType.Invalid,
                CurrentLink = ExternalLink,
                OnSave = (linkType, link) =>
                {
                    ExternalLinkType = linkType;
                    ExternalLink = link;

                    if (ExternalLinkType != ExternalLinkType.Invalid)
                    {
                        PageManager.MoveBack();
                    }
                }
            });
        }
    }
}