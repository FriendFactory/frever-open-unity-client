using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using AIFLogger;
using Bridge;
using Bridge.Services.UserProfile;
using Extensions;
using Modules.Crew;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Crews.Popups.CrewInvite;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class CrewInviteFriendsSearchView : BaseContextDataView<CrewInviteFriendsSearchViewModel>
    {
        private const int PROFILES_PER_REQUEST = 20;

        [Inject] private CrewService _crewService;
        [Inject] private IBridge _bridge;

        [SerializeField] private SearchPanelView _searchPanel;
        [SerializeField] private CrewInviteSearchListView _listView;

        private CrewInviteSearchListModel _searchListModel;
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _searchPanel.InputCompleted += OnSearchTermChanged;
            _searchPanel.InputCleared += OnSearchFieldCleared;

            _crewService.UserInvited += OnUserInvited;
        }

        private void OnDisable()
        {
            if (_tokenSource != null)
            {
                _tokenSource.CancelAndDispose();
                _tokenSource = null;
            }
            
            _searchPanel.ClearWithoutNotify();
        }

        protected override void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            _searchListModel = new CrewInviteSearchListModel(ContextData.InvitedUsers, _tokenSource.Token);
            _listView.Initialize(_searchListModel);
            _searchPanel.Clear();
            OnSearchFieldCleared();
        }

        private void OnSearchTermChanged(string searchTerm)
        {
            ClearList();
            RefreshSearchedProfiles();
        }

        private void OnSearchFieldCleared()
        {
            ClearList();
            RefreshSearchedProfiles();
        }

        private async Task RequestProfiles(string searchQuery)
        {
            var skip = _searchListModel.Friends.Count;
            var result = await _bridge.GetFriends(ContextData.LocalUserGroupId, PROFILES_PER_REQUEST, skip, searchQuery, false, false, _tokenSource.Token);
            
            if (result.Profiles?.Length == PROFILES_PER_REQUEST)
            {
                SubscribeScrollEvents();
            }
            
            if (!result.IsSuccess)
            {
                if (result.IsError) Debug.LogError(result.ErrorMessage);
                return;
            }

            if (ContextData.MemberIds == null) return;
            
            var profiles = result.Profiles.Where(p => ContextData.MemberIds.Contains(p.MainGroupId) == false)
                             .Where(p => p.CrewProfile == null).ToArray();

            _searchListModel.Friends.AddRange(profiles);
            ContextData.MemberIds = _searchListModel.Friends.Select(p => p.MainGroupId).ToList();
            _listView.Reload();
        }

        private void OnUserInvited(long invitedId)
        {
            _searchListModel.InvitedUsers.Add(invitedId);
        }
        
        private void ClearList()
        {
            _searchListModel.Friends.Clear();
            _listView.Reload();
        }
        
        private void SubscribeScrollEvents()
        {
            _listView.OnScrolledToLastScreen += LoadNextPage;
        }
        
        private void UnsubscribeScrollEvents()
        {
            _listView.OnScrolledToLastScreen -= LoadNextPage;
        }
        
        private async void RefreshSearchedProfiles()
        {
            await RequestProfiles(_searchPanel.Text);
        }

        private void LoadNextPage()
        {
            UnsubscribeScrollEvents();
            RefreshSearchedProfiles();
        }
    }
}