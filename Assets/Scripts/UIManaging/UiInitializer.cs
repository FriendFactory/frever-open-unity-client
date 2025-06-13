using System.Threading.Tasks;
using AppStart;
using Bridge;
using Common;
using DigitalRubyShared;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Loading;
using UIManaging.Pages.NoConnectionPage;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UIManaging.Pages.UpdateAppPage;
using UnityEngine;
using Zenject;

namespace UIManaging
{
    public class UiInitializer: MonoBehaviour
    {
        [SerializeField] private PageManager _uiManager;

        private PageManager _pageManager;
        private UserAccountManager _userAccountManager;
        private FingersScript _fingersScript; 
        private VideoManager _videoManager;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        public void Construct(PageManager pageManager, UserAccountManager userLoginManager, 
                              FingersScript fingersScript, VideoManager videoManager)
        {
            _pageManager = pageManager;
            _userAccountManager = userLoginManager;
            _fingersScript = fingersScript;
            _videoManager = videoManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Run()
        {
            _uiManager.Init();
            FingersInit();

            if (AppEntryContext.State == EntryState.LoggedIn)
            {
                await SetupUserAccountManager();
                OpenLoadingPage();
                return;
            }

            if (AppEntryContext.State == EntryState.NoInternetConnection)
            {
                _pageManager.MoveNext(new NoConnectionPageArgs());
                return;
            }
            
            if (AppEntryContext.State == EntryState.OutdatedApp)
            {
                _pageManager.MoveNext(new UpdateAppPageArgs());
                return;
            }
            
            OpenLoginPage();
        }

        private async Task SetupUserAccountManager()
        {
            await _userAccountManager.OnLoggedIn();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OpenLoginPage()
        {
            _uiManager.MoveNext(PageId.OnBoardingPage, new OnBoardingPageArgs());
        }

        private void OpenLoadingPage()
        {
            var args = new LoadingPageArgs(PageId.StartupLoadingPage)
            {
                FetchDefaults = true,
                WaitForStartPack = true
            };

            args.OnDataFetchedAction = OpenNextAfterOnboardingPage;
            _uiManager.MoveNext(args);
        }

        private void FingersInit()
        {
            _fingersScript.CaptureGestureHandler = CaptureGestureHandler;
        }
        
        //Allows finger gestures to pass through gameobjects with specified tags
        private static bool? CaptureGestureHandler(GameObject obj)
        {
            if (obj.tag.Contains(Constants.Gestures.ALLOW_GESTURE_PASSTHROUGH_TAG))
            {
                return false;
            }

            return null;
        }
        
        private void OpenNextAfterOnboardingPage()
        {
            _pageManager.MoveNext(PageId.Feed, new GeneralFeedArgs(_videoManager));
        }
    }
}