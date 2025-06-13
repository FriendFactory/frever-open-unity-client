using System.Collections.Generic;
using Extensions;
using TMPro;
using UIManaging.Common.SearchPanel;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowersEmptyListMessageHandler: MonoBehaviour
    {
        [SerializeField] private TMP_Text _noFriendsText;
        [SerializeField] private TMP_Text _noFollowingText;
        [SerializeField] private TMP_Text _noFollowersText;
        [Space]
        [SerializeField] private TMP_Text _remoteUserText;

        [Inject] private ProfileLocalization _localization;

        private Dictionary<UsersFilter, string> _userFilterMessagesMap;

        private void Awake()
        {
            _userFilterMessagesMap = new Dictionary<UsersFilter, string>()
            {
                {UsersFilter.Friends, _localization.RemoteUserFriendListPlaceholder},
                {UsersFilter.Followers, _localization.RemoteUserFollowersListPlaceholder},
                {UsersFilter.Followed, _localization.RemoteUserFollowingListPlaceholder},
            };
            
            HideMessage();
        }

        public void ShowLocalUserMessage(bool isLocalUser, UsersFilter filter)
        {
            HideMessage();

            if (!isLocalUser && _userFilterMessagesMap.TryGetValue(filter, out var message))
            {
                _remoteUserText.SetActive(true);
                _remoteUserText.SetText(message);
                return;
            }
            
            switch (filter)
            {
                case UsersFilter.Friends:
                    _noFriendsText.SetActive(true);
                    break;
                case UsersFilter.Followers:
                    _noFollowersText.SetActive(true);
                    break;
                case UsersFilter.Followed:
                    _noFollowingText.SetActive(true);
                    break;
            }
        }

        public void HideMessage()
        {
            _noFriendsText.SetActive(false);
            _noFollowingText.SetActive(false);
            _noFollowersText.SetActive(false);
            
            _remoteUserText.SetActive(false);
        }
    }
}