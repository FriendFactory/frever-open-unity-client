using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using JetBrains.Annotations;
using Modules.Amplitude;
using Modules.AppsFlyerManaging;
using Modules.SentryManaging;
using OneSignalHelpers;
using UIManaging.Pages.Common.UsersManagement;

namespace Modules.UserInitialization
{
    [UsedImplicitly]
    public class UserInitializationService: IAsyncInitializable
    {
        private readonly IBridge _bridge;
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly OneSignalManager _oneSignalManager;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly SentryManager _sentryManager;
        private readonly AppsFlyerService _appsFlyerService;

        public bool IsInitialized { get; private set; }

        public UserInitializationService(IBridge bridge, LocalUserDataHolder localUserDataHolder,
            OneSignalManager oneSignalManager, AmplitudeManager amplitudeManager, SentryManager sentryManager,
            AppsFlyerService appsFlyerService)
        {
            _bridge = bridge;
            _localUserDataHolder = localUserDataHolder;
            _oneSignalManager = oneSignalManager;
            _amplitudeManager = amplitudeManager;
            _sentryManager = sentryManager;
            _appsFlyerService = appsFlyerService;
        }
        
        public async Task InitializeAsync(CancellationToken token = default)
        {
            if (IsInitialized) return;
            
            await _localUserDataHolder.DownloadProfile();

            _oneSignalManager.Initialize(_bridge.Environment, _amplitudeManager);
            _oneSignalManager.SetExternalUserId(_bridge.Profile.GroupId.ToString());

            await _amplitudeManager.SetupAmplitude(_localUserDataHolder.GetUserFullYears());

            SetupSentry();

            LogAmplitudeEvents(_bridge.SessionId, _localUserDataHolder.DetectedLocationCountry);

            _appsFlyerService.OnLoggedIn();

            IsInitialized = true;
        }

        public void CleanUp()
        {
            _appsFlyerService.OnLoggedOut();
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.LOGOUT_SUCCESSFUL);
            _amplitudeManager.ChangeCoppaControl(true);

            IsInitialized = false;
        }

        private void SetupSentry()
        {
            var isModeratorOrEmployee = _localUserDataHolder.IsModerator || _localUserDataHolder.IsEmployee;
            _sentryManager.UpdateUserProfileScope(_bridge.Profile, isModeratorOrEmployee);
        }

        private void LogAmplitudeEvents(Guid sessionId, string detectedCountry)
        {
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.LOGIN_SUCCESSFUL, new Dictionary<string, object>
            {
                {AmplitudeEventConstants.EventProperties.SESSION_ID, sessionId.ToString()}
            });

            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.DETECTED_COUNTRY, new Dictionary<string, object>
            {
                {AmplitudeEventConstants.EventProperties.COUNTRY_ISO, detectedCountry}
            });
        }
    }
}