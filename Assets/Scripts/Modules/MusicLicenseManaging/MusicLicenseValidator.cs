using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace Modules.MusicLicenseManaging
{
    [UsedImplicitly]
    public class MusicLicenseValidator: IAsyncInitializable 
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        
        public bool IsInitialized { get; private set; }
        public bool IsPremiumSoundsEnabled { get; private set; }

        private MusicLicenseValidator(LocalUserDataHolder localUserDataHolder)
        {
            _localUserDataHolder = localUserDataHolder;
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            await _localUserDataHolder.RefreshUserInfoAsync();
            
            var flagEnabled = IsPremiumSoundsFlagEnabled();
            var musicEnabled = _localUserDataHolder.MusicEnabled;

            IsPremiumSoundsEnabled = flagEnabled && musicEnabled;
            IsInitialized = true;

        }
        
        public void CleanUp()
        {
            IsInitialized = false;
            IsPremiumSoundsEnabled = false;
        }
        
        private bool IsPremiumSoundsFlagEnabled()
        {
            if (Application.isEditor) return true;
        
            var variantValue = AmplitudeManager.GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.PREMIUM_SOUNDS);
            return variantValue == "on";
        }
    }
}