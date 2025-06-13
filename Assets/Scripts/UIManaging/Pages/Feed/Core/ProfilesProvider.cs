using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;

namespace UIManaging.Pages.Feed.Core
{
    /// <summary>
    /// Provides profiles with implemented cache
    /// Used in feed to get video author profiles info and prevent sending the same request multiple times
    /// </summary>
    internal interface IProfilesProvider
    {
        Task<Profile> GetProfile(long groupId, CancellationToken token);
        void ResetCache();
    }
    
    [UsedImplicitly]
    internal sealed class ProfilesProvider: IProfilesProvider
    {
        private readonly IBridge _bridge;
        private readonly Dictionary<long, Profile> _cache = new Dictionary<long, Profile>();

        public ProfilesProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<Profile> GetProfile(long groupId, CancellationToken token)
        {
            if (_cache.ContainsKey(groupId))
            {
                return _cache[groupId];
            }
            
            var result = await _bridge.GetProfile(groupId, token);
            if (result.IsSuccess)
            {
                _cache[groupId] = result.Profile;
            }
            return !result.IsSuccess ? null : result.Profile;
        }

        public void ResetCache()
        {
            _cache.Clear();
        }
    }
}