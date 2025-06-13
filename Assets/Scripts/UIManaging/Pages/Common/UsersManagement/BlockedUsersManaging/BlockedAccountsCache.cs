using System.Collections.Generic;
using System.Linq;
using Bridge.Services.UserProfile;

namespace UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging
{
    internal sealed class BlockedAccountsCache
    {
        private readonly List<Profile> _blockedAccounts;
        public IReadOnlyCollection<Profile> Accounts => _blockedAccounts;

        public BlockedAccountsCache(Profile[] blockedProfiles)
        {
            _blockedAccounts = new List<Profile>(blockedProfiles);
        }

        public bool IsBlocked(long accountGroupId)
        {
            return Accounts.Any(x => x.MainGroupId == accountGroupId);
        }

        public void Add(Profile profile)
        {
            if (IsBlocked(profile.MainGroupId)) return;
            
            _blockedAccounts.Add(profile);
        }

        public void Remove(long groupId)
        {
            if (!IsBlocked(groupId)) return;
            
            var cachedProfile = _blockedAccounts.FirstOrDefault(x => x.MainGroupId == groupId);
            _blockedAccounts.Remove(cachedProfile);
        }
    }
}