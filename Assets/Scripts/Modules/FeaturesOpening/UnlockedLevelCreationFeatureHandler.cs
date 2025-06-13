using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace Modules.FeaturesOpening
{
    public interface IUnlockingLevelCreationFeatureTracker
    {
        bool IsFeatureUnlockedInThisSession { get; }
        bool WasUserNotified { get; }

        void NotifyUser();
    }
    [UsedImplicitly]
    internal sealed class UnlockedLevelCreationFeatureHandler: IUnlockingLevelCreationFeatureTracker
    {
        private readonly PopupManagerHelper _popupManagerHelper;
        private readonly IScenarioManager _scenarioManager;
        private readonly IAppFeaturesManager _appFeaturesManager;
        
        public bool IsFeatureUnlockedInThisSession { get; private set; }
        public bool WasUserNotified { get; private set; }
        
        public UnlockedLevelCreationFeatureHandler(
            PopupManagerHelper popupManagerHelper,
            IScenarioManager scenarioManager, 
            IAppFeaturesManager appFeaturesManager)
        {
            _popupManagerHelper = popupManagerHelper;
            _scenarioManager = scenarioManager;
            _appFeaturesManager = appFeaturesManager;
            if (_appFeaturesManager.IsInitialized)
            {
                Initialize();
            }
            else
            {
                _appFeaturesManager.Initialized += Initialize;
            }
        }

        public void NotifyUser()
        {
            _popupManagerHelper.ShowUnLockedLevelCreationFeaturePopup(OpenLevelEditor);
            WasUserNotified = true;
            
            void OpenLevelEditor()
            {
                _popupManagerHelper.ShowIPSelectionPopup(universe => _scenarioManager.ExecuteNewLevelCreation(universe));
            }
        }

        private void Initialize()
        {
            _appFeaturesManager.Initialized -= Initialize;
            
            if (_appFeaturesManager.IsCreationNewLevelAllowed) return;

            _appFeaturesManager.CreationNewLevelUnlocked += () =>
            {
                IsFeatureUnlockedInThisSession = true;
            };
        }
    }
}