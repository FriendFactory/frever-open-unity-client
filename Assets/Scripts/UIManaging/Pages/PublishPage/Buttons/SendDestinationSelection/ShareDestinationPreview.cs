using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Chat;
using Bridge.Services.UserProfile;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection
{
    internal sealed class ShareDestinationPreview : MonoBehaviour
    {
        [SerializeField] private int _displayCount;
        [SerializeField] private MoreView _moreViewPrefab;
        [SerializeField] private UserView _userViewPrefab;
        [SerializeField] private GroupChatView _groupChatView;
        [SerializeField] private CrewChatView _crewCharViewPrefab;
        
        private readonly List<GameObject> _views = new List<GameObject>();
        private long _localUserGroupId;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(long localUserGroupId)
        {
            _localUserGroupId = localUserGroupId;
        }
        
        public void Show(ShareDestinationData shareDestinationData)
        {
            DestroyViews();
            ShowIcons(shareDestinationData);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowIcons(ShareDestinationData shareDestinationData)
        {
            var totalItems = GetTotalItemsToShowCount(shareDestinationData);
            if (totalItems == 0) return;

            var requireDisplayCount = totalItems > _displayCount;
            var spawnedViewCount = 0;
            if (shareDestinationData.Users != null)
            {
                foreach (var user in shareDestinationData.Users)
                {
                    SpawnUserView(user);
                    spawnedViewCount++;
                    if (HasReachedViewLimit()) break;
                }
            }

            if (shareDestinationData.Chats != null && !HasReachedViewLimit())
            {
                foreach (var chat in shareDestinationData.Chats)
                {
                    SpawnChatView(chat);
                    spawnedViewCount++;
                    if (HasReachedViewLimit()) break;
                }
            }
            
            if (requireDisplayCount)
            {
                SpawnMoreView(totalItems - spawnedViewCount);
            }

            bool HasReachedViewLimit()
            {
                if (requireDisplayCount) return spawnedViewCount == _displayCount - 1;
                return spawnedViewCount == _displayCount;
            }
        }

        private int GetTotalItemsToShowCount(ShareDestinationData shareDestinationData)
        {
            var totalCount = 0;
            if (shareDestinationData.Users != null)
            {
                totalCount += shareDestinationData.Users.Length;
            }

            if (shareDestinationData.Chats != null)
            {
                totalCount += shareDestinationData.Chats.Length;
            }
            return totalCount;
        }

        private void SpawnUserView(Profile profile)
        {
            var view = Instantiate(_userViewPrefab, transform);
            view.Display(profile);
            _views.Add(view.gameObject);
        }

        private void SpawnMoreView(int remainedTargetsCount)
        {
            var view = Instantiate(_moreViewPrefab, transform);
            view.Display(remainedTargetsCount);
            _views.Add(view.gameObject);
        }

        private void SpawnChatView(ChatShortInfo chatShortInfo)
        {
            if (chatShortInfo.Type == ChatType.Private)
            {
                DisplayPrivateChat(chatShortInfo);
                return;
            }

            if (chatShortInfo.Type == ChatType.Crew)
            {
                DisplayCrewChat();
                return;
            }
            
            if(chatShortInfo.Members.Length > 1)
            {
                DisplayGroupChat(chatShortInfo);
            }
            else
            {
                DisplaySingleMemberGroupChat(chatShortInfo);
            }
        }

        private void DisplaySingleMemberGroupChat(ChatShortInfo chatShortInfo)
        {
            var view = Instantiate(_userViewPrefab, transform);
            view.Display(chatShortInfo.Members.First());
            _views.Add(view.gameObject);
        }

        private void DisplayGroupChat(ChatShortInfo chatShortInfo)
        {
            var view = Instantiate(_groupChatView, transform);
            view.Display(chatShortInfo, _localUserGroupId);
            _views.Add(view.gameObject);
        }

        private void DisplayPrivateChat(ChatShortInfo chatShortInfo)
        {
            var view = Instantiate(_userViewPrefab, transform);
            view.Display(chatShortInfo.Members.First(x => x.Id != _localUserGroupId));
            _views.Add(view.gameObject);
        }

        private void DisplayCrewChat()
        {
            var view = Instantiate(_crewCharViewPrefab, transform);
            view.Display();
            _views.Add(view.gameObject);
        }

        private void DestroyViews()
        {
            foreach (var view in _views)
            {
                Destroy(view);
            }
            _views.Clear();
        }
    }
}