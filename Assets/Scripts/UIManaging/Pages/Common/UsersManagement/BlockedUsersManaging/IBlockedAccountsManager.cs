using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Results;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;

namespace UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging
{
    public interface IBlockedAccountsManager
    {
        event Action<Profile> AccountBlocked;
        IEnumerable<Profile> BlockedAccounts { get; }
        Task Initialize();
        Task<Result> BlockAccount(Profile account);
        Task<Result> UnblockAccount(Profile account);
        bool IsUserBlocked(long groupId);
    }

    [UsedImplicitly]
    internal sealed class BlockedAccountsManager: IBlockedAccountsManager
    {
        private readonly IBlockUserService _blockUserService;
        private BlockedAccountsCache _blockedAccounts;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public IEnumerable<Profile> BlockedAccounts => _blockedAccounts.Accounts;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<Profile> AccountBlocked;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public BlockedAccountsManager(IBlockUserService blockUserService)
        {
            _blockUserService = blockUserService;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task Initialize()
        {
            var blockedAccounts = await FetchBlockedAccounts();
            _blockedAccounts = new BlockedAccountsCache(blockedAccounts);
        }

        public async Task<Result> BlockAccount(Profile account)
        {
            var result = await _blockUserService.BlockUser(account.MainGroupId);
            if (result.IsSuccess)
            {
                _blockedAccounts.Add(account);
                AccountBlocked?.Invoke(account);
            }
            return result;
        }
        
        public async Task<Result> UnblockAccount(Profile account)
        {
            var result = await _blockUserService.UnBlockUser(account.MainGroupId);
            if (result.IsSuccess)
            {
                _blockedAccounts.Remove(account.MainGroupId);
            }
            return result;
        }

        public bool IsUserBlocked(long groupId)
        {
            return _blockedAccounts != null && _blockedAccounts.IsBlocked(groupId);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task<Profile[]> FetchBlockedAccounts()
        {
            var blockedAccountsResp = await _blockUserService.GetBlockedProfiles();
            if (blockedAccountsResp.HttpStatusCode == 403) return Array.Empty<Profile>();
            if (blockedAccountsResp.IsError)
            {
                throw new Exception($"Failed to get blocked accounts. Reason: {blockedAccountsResp.ErrorMessage}");
            }

            return blockedAccountsResp.Profiles;
        }
    }
}