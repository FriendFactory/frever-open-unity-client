using System;
using AppStart;
using Bridge;
using Common;
using Extensions;
using Modules.UserScenarios.Common;
using Modules.AppsFlyerManaging;
using Navigation.Core;
using SimpleDiskUtils;
using UIManaging.Pages.Authorization.Ui.LoginLogic;
using UIManaging.Pages.OnBoardingPage.Registration;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UiManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class OnBoardingPage : GenericPage<OnBoardingPageArgs>
    {
        [SerializeField] private Button signUpButton;
        [SerializeField] private Button loginButton;
        [SerializeField] private GameObject _registrationBlocker;
        [SerializeField] private EnvironmentDropdown _environmentDropdown;
        
        private PopupManagerHelper _popupManagerHelper;
        private SignUpAvailabilityService _availabilityService;
        private IBridge _bridge;
        private IScenarioManager _scenarioManager;
        private IAppsFlyerService _appsFlyerService;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.OnBoardingPage;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public void Construct(IAppsFlyerService appsFlyerService, IBridge bridge, IScenarioManager scenarioManager)
        {
            _bridge = bridge;
            _appsFlyerService = appsFlyerService;
            _availabilityService = new SignUpAvailabilityService(bridge);
            _scenarioManager = scenarioManager;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(OnBoardingPageArgs args)
        {
            base.OnDisplayStart(args);
            
            signUpButton.interactable = true;
            /*
             * Set value to null to handle case of the user that logged out and wants to create new account
             */
            _appsFlyerService.ToggleTracking(null);
            
            if (AppEntryContext.VideoPlayerCanvas != null)
            {
                AppEntryContext.VideoPlayerCanvas.SetActive(true);
                AppEntryContext.VideoPlayerCanvas.Play();
                AppEntryContext.VideoPlayerCanvas.HideLoadingIndicator();
            }
    
            if (args.ShowEnvironmentSelection)
            {
                _environmentDropdown.Show();
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);

            if (AppEntryContext.VideoPlayerCanvas != null)
            {
                AppEntryContext.VideoPlayerCanvas.SetActive(false);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void OnSingUpButtonClicked()
        {
            signUpButton.interactable = false;
            
            var canSignUpTask = _availabilityService.CanSignUp();
            await canSignUpTask;

            if (!canSignUpTask.Result)
            {
                _registrationBlocker.SetActive(true);
                signUpButton.interactable = true;
                return;
            }

            if (IsCacheInitialized() || IsEnoughDiskSpaceAvailable())
            {
                _scenarioManager.ExecuteOnboarding();
            }
            else
            {
                _popupManagerHelper.OpenDiskSpacePopup();
            }
        }

        private void OnLoginButtonClicked()
        {
            _scenarioManager.ExecuteSignIn();
        }

        private void OnEnable()
        {
            signUpButton.onClick.AddListener(OnSingUpButtonClicked);
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }

        private void OnDisable()
        {
            signUpButton.onClick.RemoveListener(OnSingUpButtonClicked);
            loginButton.onClick.RemoveListener(OnLoginButtonClicked);
        }

        private bool IsCacheInitialized()
        {
            return !_bridge.IsCacheEmpty;
        }

        private static bool IsEnoughDiskSpaceAvailable()
        {
            var freeSpaceMb = DiskUtils.CheckAvailableSpace();
            return freeSpaceMb >= Constants.Memory.MIN_DISK_SPACE_REQUIRED_MB;
        }
    }
}