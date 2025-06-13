using Abstract;
using Extensions;
using Modules.Crew;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.Popups
{
    internal sealed class CrewInviteListItem : BaseContextDataView<CrewInviteListItemModel>
    {
        [SerializeField] private RawImage _portrait;
        [SerializeField] private TMP_Text _username;
        [SerializeField] private TMP_Text _followerCount;
        [SerializeField] private CrewInviteButton _crewInviteButton;

        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;
        [Inject] private CrewService _crewService;
        [Inject] private CrewPageLocalization _localization;

        private void OnEnable()
        {
            _crewInviteButton.OnInviteActionRequested = RequestInviteAction;
            _portrait.SetActive(false);
        }

        private void OnDisable()
        {
            _portrait.texture = null;
            _crewInviteButton.OnInviteActionRequested = null;
        }

        protected override void OnInitialized()
        {
            _username.text = ContextData.Username;
            
            _followerCount.text = 
                string.Format(ContextData.FollowersCount > 1 
                                ? _localization.CrewListItemFollowersPluralCounterFormat
                                : _localization.CrewListItemFollowersCounterFormat,
                                ContextData.FollowersCount);
            
            _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(ContextData.GroupId, Resolution._128x128, OnPortraitDownloaded, cancellationToken: ContextData.Token);
            _crewInviteButton.Initialize(ContextData.Invited);
        }

        private void OnPortraitDownloaded(Texture2D portrait)
        {
            if (IsDestroyed) return;
            _portrait.texture = portrait;
            _portrait.SetActive(true);
        }

        private async void RequestInviteAction()
        {
            if (ContextData.Invited)
            {
                return;
            }

            var success = await _crewService.InviteUserToCrew(ContextData.GroupId);
            _crewInviteButton.Initialize(success);
        }
    }
}