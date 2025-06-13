using System;
using System.Threading.Tasks;
using Bridge.Services.UserProfile;
using Common;
using Common.Publishers;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;

namespace Modules.FeaturesOpening
{
    /// <summary>
    /// Responsible
    /// </summary>
    public interface IAppFeaturesManager
    {
        bool IsInitialized { get; }
        event Action Initialized;

        bool IsCreationNewLevelAllowed { get; }
        int ChallengesRemainedForEnablingNewLevelCreation { get; }
        event Action CreationNewLevelUnlocked;
        Task Initialize();
    }
    
    [UsedImplicitly]
    internal sealed class AppFeaturesManager: IAppFeaturesManager
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly IPublishVideoHelper _publishVideoHelper;
        private readonly LockedFeaturesFilesLoader _lockedFeaturesFilesLoader;
        private bool _initialized;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action Initialized;
        public event Action CreationNewLevelUnlocked;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
      
        public bool IsInitialized => FeatureSettings != null;
        public bool IsCreationNewLevelAllowed => FeatureSettings.AllowCreatingNewLevel;
        public int ChallengesRemainedForEnablingNewLevelCreation => FeatureSettings.RequiredVideoCount - FeatureSettings.CurrentVideoCount;
        private FeatureSettings FeatureSettings => _localUserDataHolder.FeatureSettings;
        private bool AllFeaturesOpened => FeatureSettings.AllowCreatingNewLevel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public AppFeaturesManager(LocalUserDataHolder localUserDataHolder, IPublishVideoHelper publishVideoHelper, LockedFeaturesFilesLoader lockedFeaturesFilesLoader)
        {
            _localUserDataHolder = localUserDataHolder;
            _publishVideoHelper = publishVideoHelper;
            _lockedFeaturesFilesLoader = lockedFeaturesFilesLoader;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task Initialize()
        {
            if (_initialized) return;

            await RefreshFeatureSettingsData();
            if (AllFeaturesOpened)
            {
                OnInitialized();
                return;
            }
           
            if (!FeatureSettings.AllowCreatingNewLevel)
            {
                _publishVideoHelper.VideoUploaded += OnVideoUploaded;
                _lockedFeaturesFilesLoader.Fetch();
            }

            OnInitialized();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private Task RefreshFeatureSettingsData()
        {
            return _localUserDataHolder.RefreshUserInfoAsync();
        }

        private async void OnVideoUploaded()
        {
            await RefreshFeatureSettingsData();
            
            if (!FeatureSettings.AllowCreatingNewLevel) return;
            
            CreationNewLevelUnlocked?.Invoke();
            _publishVideoHelper.VideoUploaded -= OnVideoUploaded;
        }

        private void OnInitialized()
        {
            _initialized = true;
            Initialized?.Invoke();
        }
    }
}