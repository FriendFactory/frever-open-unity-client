using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Authorization.Results;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Models.ClientServer.UserActivity;
using Bridge.Results;
using Bridge.Services.UserProfile;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.InAppPurchasing;
using SA.iOS.Foundation;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

#if UNITY_ANDROID
using SA.Android.App;
#endif

namespace UIManaging.Pages.Common.UsersManagement
{
    [UsedImplicitly]
    public sealed class LocalUserDataHolder
    {
        private const int AGE_GATE = 13;

        private readonly IBridge _bridge;
        private readonly AmplitudeManager _amplitudeManager;

        public event Action UserBalanceUpdated;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public DateTime BirthDate { get; set; }
        public DateTime RegistrationDate { get; private set; }
        public string NickName => UserProfile.NickName;
        public bool? DataCollection { get; private set; } = true;
        public bool IsModerator { get; private set; }
        public bool IsEmployee { get; private set; }
        public long GroupId => UserProfile.MainGroupId;
        public Profile UserProfile { get; private set; }
        public UserBalance UserBalance { get; private set; }
        public UserCard LevelingProgress { get; private set; }
        public PurchasedAssetsData PurchasedAssets { get; private set; }
        public bool HasPremiumPass => LevelingProgress.IsPremium;
        public FeatureSettings FeatureSettings { get; private set; }
        public bool IsStarCreator { get; private set; }
        public bool IsStarCreatorCandidate { get; private set; }
        public string Bio { get; private set; }
        public Dictionary<string, string> BioLinks { get; private set; }
        public string DetectedLocationCountry { get; set; }
        public bool MusicEnabled { get; set; }
        public bool IsOnboardingCompleted { get; set; }
        public InitialAccountBalanceInfo InitialAccountBalance { get; set; }
        public bool IsNewUser { get; set; }
        public bool IsLoadingProfile { get; private set; }
        public bool HasSetupCredentials { get; private set; }
        public bool IsUnderAge => GetUserFullYears() < AGE_GATE;

        public int CurrentLevel => LevelingProgress.Xp.CurrentLevel?.Level ?? 0;
        public long[] ClaimedRewards => LevelingProgress.RewardClaimed ?? Array.Empty<long>();
        public DateTime? UsernameUpdateAvailableOn { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public LocalUserDataHolder(IBridge bridge, AmplitudeManager amplitudeManager, IIAPManager iapManager, UserProfileFetcher userProfileFetcher)
        {
            _bridge = bridge;
            _amplitudeManager = amplitudeManager;
            iapManager.PurchasedSeasonPass += OnPremiumPassPurchased;

            if (userProfileFetcher.IsUserInfoFetched)
            {
                SetupUserInfo(userProfileFetcher.UserInfo);
            }
            else
            {
                userProfileFetcher.UserInfoFetched += SetupUserInfo;
            }

            if (userProfileFetcher.IsUserProfileFetched)
            {
                UserProfile = userProfileFetcher.Profile;
            }
            else
            {
                userProfileFetcher.UserProfileFetched += profile => { UserProfile ??= profile; };
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void UpdateCountryInfo()
        {
            #if UNITY_IOS
            var currentLocale = ISN_NSLocale.CurrentLocale;
            var countryCode = currentLocale.CountryCode;
            #elif UNITY_ANDROID
            var currentLocale = AN_Locale.GetDefault();
            var countryCode = currentLocale.CountryCode;
            #endif

            var downloadedCountries = await DownloadCountries();
            var matchingCountry = downloadedCountries.FirstOrDefault(
                country => country.Iso3Code.StartsWith(countryCode, true, CultureInfo.InvariantCulture));
            var countryId = matchingCountry?.Id;

            if (countryId == null)
            {
                return;
            }

            var result = await _bridge.UpdateUserCountry(countryId.Value);
            if (result.IsError) Debug.LogError($"Failed to update user's country. Reason: {result.ErrorMessage}");
        }

        public async void SetUserGenderId(long id)
        {
            await _bridge.UpdateUserGender(id);
        }

        public void AddSoftCurrency(int amount)
        {
            UserBalance.SoftCurrencyAmount += amount;
            UserBalanceUpdated?.Invoke();
        }
        
        public void AddHardCurrency(int amount)
        {
            UserBalance.HardCurrencyAmount += amount;
            UserBalanceUpdated?.Invoke();
        }
        
        public async Task DownloadProfile(bool refreshUserInfo = true)
        {
            IsLoadingProfile = true;
            //todo: check why we need to load 2 different data: user profile and user info
            if (refreshUserInfo)
            {
                await RefreshUserInfoInternal();
            }

            var result = await _bridge.GetMyProfile();
            UserProfile = result.Profile;
            IsLoadingProfile = false;
        }

        public async Task UpdateBalance(CancellationToken token = default, bool silently = false)
       {
            var result = await _bridge.GetUserBalance(token);
            
            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogError($"Failed to download user balance. Reason: {result.ErrorMessage}");
                return;
            }
            
            UserBalance = result.Model;
            if(!silently) UserBalanceUpdated?.Invoke();
            _amplitudeManager.SendUserCurrencyProperties(result.Model);
       }
        
        public async Task UpdatePurchasedAssetsInfo(CancellationToken token = default)
        {
            var result = await _bridge.GetPurchasedAssetsInfo(token);
            
            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogError($"Failed to download user balance. Reason: {result.ErrorMessage}");
                return;
            }
            
            PurchasedAssets = result.Model;
        }

        public async Task<UpdateUsernameResult> UpdateUserName(string username)
        {
            var res = await _bridge.UpdateUsername(username);
            if (res.Ok)
            {
                UsernameUpdateAvailableOn = res.UsernameUpdateAvailableOn;
                UserProfile.NickName = username;
            }
            return res;
        }

        public int GetUserFullYears()
        {
            var currentDate = DateTime.UtcNow;
            
            if (BirthDate > currentDate) return 0;
            
            var timeSpan = currentDate - BirthDate;
            var age = DateTime.MinValue.AddSeconds(timeSpan.TotalSeconds);
            return age.Year - 1;
        }

        public void SetMainCharacter(CharacterInfo character)
        {
            UserProfile.MainCharacter = character;
        }
        
        public async Task SetAccessForCharacter(CharacterAccess access, Action onSuccess = null)
        {
            var updateCharacterAccessTask = await _bridge.UpdateCharacterAccess(access);

            if (updateCharacterAccessTask.IsSuccess)
            {
                UserProfile.CharacterAccess = access;
                onSuccess?.Invoke();
            }

            if (updateCharacterAccessTask.IsError)
            {
                Debug.LogError($"Failed to update {nameof(CharacterInfo.Access)} to value \"{access}\". Reason: {updateCharacterAccessTask.ErrorMessage}");
            }
        }

        public async Task<bool> RefreshUserInfoAsync()
        {
            var result = await RefreshUserInfoInternal();
            if (!result) return false;
            
            UserBalanceUpdated?.Invoke();
            return true;
        }

        public async void RefreshUserInfo()
        {
            await RefreshUserInfoAsync();
        }

        public async Task<Result> SetProfileBio(string bio)
        {
            var result = await _bridge.UpdateUserBio(bio);
            if (result.IsSuccess) Bio = bio;
            return result;
        }

        public async Task<Result> SetProfileBioLinks(Dictionary<string, string> links)
        {
            var result = await _bridge.UpdateUserBioLinks(links);
            if (result.IsSuccess) BioLinks = links;
            return result;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task<bool> RefreshUserInfoInternal()
        {
            var profileResult = await _bridge.GetCurrentUserInfo();

            if (profileResult.IsSuccess)
            {
                var result = profileResult.Profile;
                await SetupUserInfoAsync(result);
                return true;
            }

            if (profileResult.HttpStatusCode == 403) return true;

            Debug.LogError($"Failed to download {nameof(User)} with {nameof(User.MainGroupId)}={_bridge.Profile.GroupId}. Reason: {profileResult.ErrorMessage}");
            return false;
        }

        private async void SetupUserInfo(MyProfile profile)
        {
            await SetupUserInfoAsync(profile);
        }
        
        private async Task SetupUserInfoAsync(MyProfile profile)
        {
            if (profile.BirthDate != null) BirthDate = profile.BirthDate.Value;
            DataCollection = profile.DataCollectionEnabled ?? true;

            IsModerator = profile.IsEmployee;
            IsEmployee = profile.IsEmployee;
            IsStarCreator = profile.IsStarCreator;
            IsStarCreatorCandidate = profile.IsStarCreatorCandidate;

            UserBalance = profile.UserBalance;
            LevelingProgress = profile.LevelingProgress;
            RegistrationDate = profile.RegistrationDate;
            Bio = profile.Bio ?? string.Empty;
            BioLinks = profile.BioLinks;
            DetectedLocationCountry = profile.DetectedLocationCountry;
            MusicEnabled = profile.MusicEnabled;
            IsOnboardingCompleted = profile.IsOnboardingCompleted;
            InitialAccountBalance = profile.InitialAccountBalance;
            UsernameUpdateAvailableOn =  profile.UsernameUpdateAvailableOn;
            HasSetupCredentials = profile.HasUpdatedCredentials;
            //todo: remove creating of instance after we have uploaded feature to Stage and Prod
            FeatureSettings = profile.FeatureSettings ?? new FeatureSettings {AllowCreatingNewLevel = true};

            _amplitudeManager.SendUserCurrentLevelProperty(profile.LevelingProgress.Xp.CurrentLevel.Level);

            await UpdatePurchasedAssetsInfo();
        }

        private async Task<CountryInfo[]> DownloadCountries()
        {
            var response = await _bridge.GetCountriesListAsync();

            if (response.IsSuccess)
            {
                var countries = response.Models;
                return countries;
            }

            Debug.LogError($"Failed to download {nameof(Country)}. Reason: {response.ErrorMessage}");
            return null;
        }

        private void OnPremiumPassPurchased(SeasonPassProduct seasonPassProduct)
        {
            LevelingProgress.IsPremium = true;
        }
    }
}