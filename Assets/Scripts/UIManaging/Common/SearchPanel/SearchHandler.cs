using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer;
using Bridge.Services.UserProfile;
using Extensions;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Common.SearchPanel
{
    public class SearchHandler : MonoBehaviour
    {
        private const int PROFILES_PER_REQUEST = 20;

        [SerializeField] private SearchPanelView _searchPanel;
        [SerializeField] private SearchListView _searchListView;
        [SerializeField] private bool _reloadOnClear = true;
        [FormerlySerializedAs("_filterUsersWithCrew")] [SerializeField] private bool _includeUsersWithCrew;
        [SerializeField] private bool _excludeMinors;

        [Inject] private IBridge _bridge;
        [Inject] private FollowersManager _followersManager;
        [Inject] private LocalUserDataHolder _localUser;
        
        private readonly List<Profile> _userProfiles = new List<Profile>();
        private readonly List<Profile> _notDisplayedProfiles = new List<Profile>();
        private readonly List<Profile> _customProfiles = new List<Profile>();

        private CancellationTokenSource _cancellationTokenSource;

        private bool _handlingEnabled = true;
        private long _targetProfileGroupId;
        private UsersFilter _usersFilter;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action SearchCleared;
        public event Action SearchedProfilesLoaded;
        public event Action<Profile> ProfileButtonClicked;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public IEnumerable<long> FilterGroupIds { get; set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _followersManager.PrefetchDataForLocalUser();

            _searchListView.ItemDisplayed += OnProfileDisplayed;
            _searchListView.ProfileButtonClicked += OnProfileButtonClicked;
            _followersManager.Followed += OnFollowOrUnFollow;
            _followersManager.UnFollowed += OnFollowOrUnFollow;

            UnsubscribeFromInput();
            SubscribeToInput();
        }

        private void OnDestroy()
        {
            _searchListView.ItemDisplayed -= OnProfileDisplayed;
            _searchListView.ProfileButtonClicked -= OnProfileButtonClicked;
            _searchPanel.InputCompleted -= OnSearchChanged;
            _searchPanel.InputCleared -= OnSearchCleared;
            _followersManager.Followed -= OnFollowOrUnFollow;
            _followersManager.UnFollowed -= OnFollowOrUnFollow;
        }

        private void OnEnable()
        {
            if(!_handlingEnabled) return;
            _searchPanel.ClearWithoutNotify();
        }

        private void OnDisable()
        {
            Cancel();
            ClearList();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetSearchHandling(bool isEnabled)
        {
            _handlingEnabled = isEnabled;
            UnsubscribeFromInput();

            if (!_handlingEnabled)
            {
                return;
            }
            
            ClearList();
            OnSearchChanged(_searchPanel.Text);
            SubscribeToInput();
        }

        public void SetTargetProfileToLocalUser()
        {
            _targetProfileGroupId = _bridge.Profile.GroupId;
        }

        public void SetTargetProfile(long groupId)
        {
            _targetProfileGroupId = groupId;
        }

        public void SetUsersFilter(UsersFilter filter, bool silent = false)
        {
            _usersFilter = filter;

            ClearList();

            if (silent)
            {
                RefreshSearchedProfiles();
            }
            else
            {
                ReloadSearchedProfiles(_searchPanel.Text);
            }
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual SearchListModel CreateSearchListModel(ICollection<Profile> profiles, bool isSearchResult)
        {
            return new SearchListModel(profiles.ToArray(), isSearchResult);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SubscribeToInput()
        {
            _searchPanel.InputCompleted += OnSearchChanged;
            _searchPanel.InputCleared += OnSearchCleared;
        }

        private void UnsubscribeFromInput()
        {
            _searchPanel.InputCompleted -= OnSearchChanged;
            _searchPanel.InputCleared -= OnSearchCleared;
        }

        private void OnSearchChanged(string inputText)
        {
            _userProfiles.Clear();
            _notDisplayedProfiles.Clear();
            ReloadSearchedProfiles(inputText);
        }

        private void ClearList()
        {
            _userProfiles.Clear();
            _notDisplayedProfiles.Clear();

            var searchListModel = new SearchListModel(Array.Empty<Profile>(), false);
            _searchListView.Initialize(searchListModel);
            _searchListView.Reload();
        }

        private async void ReloadSearchedProfiles(string searchName)
        {
            await GetSearchedProfiles(searchName, true);
            
            _searchListView.Reload();
            SearchedProfilesLoaded?.Invoke();
        }

        private async void RefreshSearchedProfiles()
        {
            await GetSearchedProfiles(_searchPanel.Text);
        }

        private async Task GetSearchedProfiles(string searchName, bool reload = false)
        {
            var searchedProfiles = await GetProfiles(searchName);

            if (searchedProfiles.Profiles?.Length == PROFILES_PER_REQUEST)
            {
                SubscribeScrollEvents();
            }
            
            if (searchedProfiles.IsRequestCanceled || searchedProfiles.IsError)
            {
                return;
            }
            
            var uniqueNewProfiles = searchedProfiles.Profiles.Where(newProfile => _userProfiles.All(profile => profile.MainGroupId != newProfile.MainGroupId)).ToArray();

            if (FilterGroupIds != null && FilterGroupIds.Any())
            {
                uniqueNewProfiles = uniqueNewProfiles.Where(profile => !FilterGroupIds.Contains(profile.MainGroupId))
                                                     .ToArray();
            }

            if (_userProfiles.Count != 0 && uniqueNewProfiles.Length == 0) return;

            _userProfiles.AddRange(uniqueNewProfiles);
            _notDisplayedProfiles.AddRange(uniqueNewProfiles);

            var searchListModel = CreateSearchListModel(_userProfiles, !string.IsNullOrEmpty(searchName));

            _searchListView.Initialize(searchListModel);

            if (reload)
            {
                _searchListView.Reload();
            }
            else
            {
                _searchListView.Refresh();
            }
        }

        private void OnProfileDisplayed(long id)
        {
            var displayedProfile = _notDisplayedProfiles.FirstOrDefault(x => x.MainGroupId == id);
            if (displayedProfile == null) return;

            _notDisplayedProfiles.Remove(displayedProfile);
            GetNextProfilesIfAllHasBeenDisplayed();
        }
        
        private void OnProfileButtonClicked(Profile profile)
        {
            ProfileButtonClicked?.Invoke(profile);
        }

        private void GetNextProfilesIfAllHasBeenDisplayed()
        {
            var allProfilesDisplayed = _notDisplayedProfiles.IsNullOrEmpty();
            if (!allProfilesDisplayed) return;
            
            RefreshSearchedProfiles();
        }

        private void Cancel()
        {
            if (_cancellationTokenSource == null) return;
            
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
        }
        
        private void OnSearchCleared()
        {
            _userProfiles.Clear();
            _notDisplayedProfiles.Clear();

            if (_reloadOnClear)
            {
                ReloadSearchedProfiles(string.Empty);
            }
            
            SearchCleared?.Invoke();
        }

        private void OnFollowOrUnFollow(Profile profile)
        {
            var profileToUpdate = _userProfiles.FirstOrDefault(x => x.MainGroupId == profile.MainGroupId);
            if (profileToUpdate == null) return;

            var index = _userProfiles.IndexOf(profileToUpdate);
            _userProfiles[index] = profile;
            _searchListView.SetProfiles(_userProfiles.ToArray());
        }

        private async Task<ProfilesResult<Profile>> GetProfiles(string searchQuery)
        {
            ProfilesResult<Profile> result;
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            switch (_usersFilter)
            {
                case UsersFilter.All:
                    result = await _bridge.GetProfiles( PROFILES_PER_REQUEST, _userProfiles.Count, searchQuery, excludeMinors:_excludeMinors, cancellationToken:_cancellationTokenSource.Token);
                    break;
                case UsersFilter.Friends:
                    result = await _bridge.GetFriends(_targetProfileGroupId, PROFILES_PER_REQUEST, _userProfiles.Count, searchQuery, _includeUsersWithCrew, cancellationToken:_cancellationTokenSource.Token);
                    break;
                case UsersFilter.Followers:
                    result = await _bridge.GetFollowersFor(_targetProfileGroupId, PROFILES_PER_REQUEST, _userProfiles.Count, searchQuery, cancellationToken:_cancellationTokenSource.Token);
                    break;
                case UsersFilter.Followed:
                    result = await _bridge.GetFollowedBy(_targetProfileGroupId, PROFILES_PER_REQUEST, _userProfiles.Count, searchQuery, cancellationToken:_cancellationTokenSource.Token);
                    break;
                case UsersFilter.CustomList:
                    result = GetFilteredCustomProfiles(searchQuery);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            return result;
        }

        private ProfilesResult<Profile> GetFilteredCustomProfiles(string searchQuery)
        {
            var filteredProfiles = string.IsNullOrWhiteSpace(searchQuery)
                ? _customProfiles
                : _customProfiles.Where(member => member.NickName.ToLower().Contains(searchQuery.ToLower()));
            var result = new ProfilesResult<Profile>(filteredProfiles.ToArray());
            return result;
        }

        private void SubscribeScrollEvents()
        {
            _searchListView.OnScrolledToLastScreen += LoadNextPage;
        }
        
        private void UnsubscribeScrollEvents()
        {
            _searchListView.OnScrolledToLastScreen -= LoadNextPage;
        }

        private void LoadNextPage()
        {
            UnsubscribeScrollEvents();
            RefreshSearchedProfiles();
        }

        public void SetUserList(IEnumerable<GroupShortInfo> users)
        {
            _usersFilter = UsersFilter.CustomList;
            _customProfiles.Clear();
            _customProfiles.AddRange(
                users.Where(member => member.Id != _localUser.GroupId)
                     .Select(member => new Profile
                      {
                          MainGroupId = member.Id,
                          NickName = member.Nickname,
                          MainCharacter = new CharacterInfo
                          {
                              Id = member.MainCharacterId ?? 0,
                              Files = member.MainCharacterFiles
                          }
                      }));
            RefreshSearchedProfiles();
        }
    }
}