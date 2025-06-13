using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Services.UserProfile;

namespace UIManaging.Pages.Common.FollowersManagement.UserLists
{
    public abstract class BaseUserList
    {
        protected const int REQUEST_USER_COUNT = 20;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<List<Profile>> OnChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool IsListFetched => Profiles != null;
        private bool _isListFetching;

        /// <summary>
        /// Returns first 20 profiles from the DB
        /// </summary>
        public List<Profile> Profiles { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        /// <summary>
        /// Returns first 20 profiles from the DB asynchronously
        /// </summary>
        public async Task<ICollection<Profile>> GetProfilesAsync()
        {
            if (IsListFetched)
            {
                return Profiles;
            }

            await TryFetchProfiles();
            
            return Profiles;
        }
        
        public abstract Task<ProfilesResult<Profile>> GetNextProfilesAsync(int take = 20, int skip = 0);
        
        public async void PrefetchData()
        {
            if (IsListFetched)
            {
                InvokeOnChanged();
                return;
            }
            
            await FetchProfiles();
        }

        public void RemoveFromCachedList(long groupId)
        {
            Profiles?.RemoveAll(x => x.MainGroupId == groupId);
        }

        public void UpdateProfileCachedList(Profile profile)
        {
            var profileToUpdate = Profiles.First(x => x.MainGroupId == profile.MainGroupId);
            var index = Profiles.IndexOf(profileToUpdate);
            Profiles[index] = profile;
            InvokeOnChanged();
        }

        public bool ExistInCachedList(long groupId)
        {
            return Profiles != null && Profiles.Any(x => x.MainGroupId == groupId);
        }

        public async Task RefreshListFromBackend()
        {
            Profiles = null;
            PrefetchData();
            await WaitWhileFetchingFinished();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected abstract Task<ProfilesResult<Profile>> GetProfiles();

        protected void InvokeOnChanged()
        {
            OnChanged?.Invoke(Profiles);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async Task FetchProfiles()
        {
            _isListFetching = true;
            
            var response = await GetProfiles();

            if (response.IsSuccess)
            {
                Profiles = response.Profiles.ToList();
                InvokeOnChanged();
            }

            _isListFetching = false;
        }
        
        private async Task TryFetchProfiles()
        {
            if (!_isListFetching)
            {
                await FetchProfiles();
            }
            else
            {
                await WaitWhileFetchingFinished();   
            }
        }
        
        private async Task WaitWhileFetchingFinished()
        {
            while (_isListFetching)
            {
                await Task.Delay(20);
            }
        }
    }
}