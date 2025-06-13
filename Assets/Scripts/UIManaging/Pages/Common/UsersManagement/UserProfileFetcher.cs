using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using UnityEngine;

namespace UIManaging.Pages.Common.UsersManagement
{
    public sealed class UserProfileFetcher
    {
        private readonly IBridge _bridge;
        public MyProfile UserInfo { get; private set; }
        public Profile Profile { get; private set; }

        public bool IsUserInfoFetched => UserInfo != null;
        public bool IsUserProfileFetched => Profile != null;

        public event Action<MyProfile> UserInfoFetched;
        public event Action<Profile> UserProfileFetched;
        
        public UserProfileFetcher(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async void Fetch()
        {
            var fetchingTasks = new Task[2];
            fetchingTasks[0] = FetchUserInfo();
            fetchingTasks[1] = FetchUserProfile();
            await Task.WhenAll(fetchingTasks);
        } 
        
        private async Task FetchUserInfo()
        {
            var resp = await _bridge.GetCurrentUserInfo();
            if (resp.IsSuccess)
            {
                UserInfo = resp.Profile;
                UserInfoFetched?.Invoke(UserInfo);
            }
            else
            {
                Debug.LogError($"Failed to load user info: {_bridge.Profile.GroupId}");
            }
        }

        private async Task FetchUserProfile()
        {
            var resp = await _bridge.GetMyProfile();
            if (resp.IsSuccess)
            {
                Profile = resp.Profile;
                UserProfileFetched?.Invoke(Profile);
            }
            else
            {
                Debug.LogError($"Failed to load profile: {_bridge.Profile.GroupId}");
            }
        }
    }
}