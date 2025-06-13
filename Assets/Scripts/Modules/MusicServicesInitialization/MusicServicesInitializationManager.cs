using System;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.MusicLicenseManaging;
using UIManaging.Pages.Common.UserLoginManagement;
using UnityEngine;

namespace Modules.MusicServicesInitialization
{
    [UsedImplicitly]
    public sealed class MusicServicesInitializationManager: IAsyncInitializable, IDisposable
    {
        private readonly UserAccountManager _userAccountManager;
        private readonly MusicLicenseValidator _musicLicenseValidator;
        private readonly IDataFetcher _dataFetcher;
        
        public bool IsInitialized { get; private set; }

        public MusicServicesInitializationManager(UserAccountManager userAccountManager, MusicLicenseValidator musicLicenseValidator, IDataFetcher dataFetcher)
        {
            _userAccountManager = userAccountManager;
            _musicLicenseValidator = musicLicenseValidator;
            _dataFetcher = dataFetcher;
            
            _userAccountManager.OnUserLoggedIn += OnUserLoggedIn;
            _userAccountManager.OnUserLoggedOut += OnUserLoggedOut;
        }

        private async void OnUserLoggedIn()
        {
            try
            {
                _userAccountManager.OnUserLoggedIn -= OnUserLoggedIn;

                await InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnUserLoggedOut()
        {
            CleanUp();
            
            _userAccountManager.OnUserLoggedIn += OnUserLoggedIn;
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            await _musicLicenseValidator.InitializeAsync(token);
            await _dataFetcher.FetchSongsAsync();
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            _userAccountManager.OnUserLoggedIn -= OnUserLoggedIn;
            
            IsInitialized = false;
        }

        public void Dispose()
        {
            CleanUp();
        }
    }
}