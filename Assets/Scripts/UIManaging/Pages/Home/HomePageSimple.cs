using System;
using System.Threading;
using Bridge;
using Common.UserBalance;
using DG.Tweening;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.DeepLinking;
using Modules.QuestManaging;
using Modules.UniverseManaging;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Animated.Behaviours;
using UIManaging.Common;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

#if UNITY_ANDROID && !UNITY_EDITOR
using Common.Permissions;
using SA.Android.Manifest;
using Common;
#endif

namespace UIManaging.Pages.Home
{
    public sealed class HomePageSimple : GenericPage<HomePageSimpleArgs>
    {
        private const float RAYCAST_TARGET_ALPHA = 0.5f;
        
        public override PageId Id => PageId.HomePageSimple;

        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private UserPortrait _thumbnailPortrait;
        [SerializeField] private SeasonThumbnailBackground _seasonThumbnailBackground;
        [SerializeField] private RewardFlowManager _rewardFlowManager;
        [SerializeField] private Image _raycastBlocker;
        [SerializeField] private SeasonPageButton _seasonPageButton;
        [SerializeField] private OpenCreatorScoreButton _openCreatorScoreButton;
        [SerializeField] private QuestButton _questButton;
        [SerializeField] private SlideInOutBehaviour _slideInOutBehaviour;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private PopupManager _popupManager;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IInvitationLinkHandler _invitationLinkHandler;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private IQuestManager _questManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private IUniverseManager _universeManager;

        #if UNITY_ANDROID && !UNITY_EDITOR
        [Inject] private AndroidPermissionsHelper _androidPermissionsHelper;
        #endif

        private HomePagePopupHelper _homePagePopupHelper;
        private CancellationTokenSource _tokenSource;
        private bool _isInitialized;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _questButton.Button.onClick.AddListener(OnQuestButton);
            _questButton.HidingBegin += OnQuestHidingBegin;
        }
        
        private void OnDisable()
        {
            _questButton.Button.onClick.RemoveListener(OnQuestButton);
            _questButton.HidingBegin -= OnQuestHidingBegin;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            _seasonThumbnailBackground.CleanUp();
            _thumbnailPortrait.CleanUp();
            
            _homePagePopupHelper?.Dispose();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _homePagePopupHelper = new HomePagePopupHelper(_bridge, _popupManager, _dataHolder,
                                                           _rewardFlowManager, _invitationLinkHandler, _snackBarHelper, 
                                                           _pageManager, new FeedPopupHelper(_popupManager, 
                                                               _dataFetcher, _questManager, _amplitudeManager, _universeManager));
        }

        protected override void OnDisplayStart(HomePageSimpleArgs args)
        {
            base.OnDisplayStart(args);

            _raycastBlocker.SetActive(args.EnableInputBlocker);
            _tokenSource = new CancellationTokenSource();

            var userBalanceModel = new StaticUserBalanceModel(_dataHolder);
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.Initialize(userBalanceModel);

            LoadPortraitAsync(_tokenSource.Token);
            LoadBackgroundAsync(_tokenSource.Token);

            if (args.OpenedWithTask != null)
            {
                _rewardFlowManager.Initialize(_dataHolder.UserBalance, _dataHolder.LevelingProgress.Xp);
                var config = new TaskCompletedPopupConfiguration(args.OpenedWithTask, StartTaskRewardAnimation);
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(PopupType.TaskCompletedPopup);
            }
            else
            {
                _homePagePopupHelper.ShowHomePagePopups();

                #if UNITY_ANDROID && !UNITY_EDITOR
                ShowPhoneCallPermissionPopup();
                #endif
            }

            if (args.RewardModel != null) HandleInviteReward();

            RefreshButtonsWithBadges();
            _questManager.UpdateQuestData();
            _slideInOutBehaviour.Show();
            _raycastBlocker.SetActive(OpenPageArgs.EnableInputBlocker);
            
            _isInitialized = true;
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _isInitialized = false;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();

            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnQuestButton()
        {
            _raycastBlocker.SetActive(true);
            DOTween.To(t => _raycastBlocker.SetAlpha(t), 0, RAYCAST_TARGET_ALPHA, 0.4f).SetEase(Ease.InOutQuad);
            _slideInOutBehaviour.SlideOut();
        }
       
        private void OnQuestHidingBegin()
        {
            if (_questManager.IsComplete)
            {
                _questButton.Button.interactable = false;
            }
            
            if (!OpenPageArgs.EnableInputBlocker)
            {
                DOTween.To(t => _raycastBlocker.SetAlpha(t), RAYCAST_TARGET_ALPHA, 0, 0.4f).SetEase(Ease.InOutQuad)
                       .OnComplete(() => _raycastBlocker.SetActive(false));
            }
            
            _slideInOutBehaviour.SlideIn(() =>
            {
                if (!_questManager.IsComplete)
                {
                    return;
                }
                
                _questButton.QuestCompleteAnimation();
            });
        }
        
        private async void LoadPortraitAsync(CancellationToken token)
        {
            if (_isInitialized)
            {
                return;
            }

            var result = await _bridge.GetMyProfile(token);
            if (_tokenSource.IsCancellationRequested) return;

            if (result.IsError)
            {
                Debug.LogError($"Loading portrait error: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                await _thumbnailPortrait.InitializeAsync(result.Profile, Resolution._512x512, token);
            }

            if (_tokenSource.IsCancellationRequested)
            {
                return;
            }

            _thumbnailPortrait.ShowContent();
        }

        private async void LoadBackgroundAsync(CancellationToken token)
        {
            if (_isInitialized)
            {
                return;
            }

            await _seasonThumbnailBackground.InitializeAsync(Resolution._512x512, token);

            if (_tokenSource.IsCancellationRequested)
            {
                return;
            }

            _seasonThumbnailBackground.ShowContent();
        }

        private async void RefreshButtonsWithBadges()
        {
            _openCreatorScoreButton.UpdateSprite();

            await _dataHolder.DownloadProfile();

            _seasonPageButton.RefreshNotificationBadge();
            _openCreatorScoreButton.RefreshNotificationBadge();
        }

        private void StartTaskRewardAnimation(object o)
        {
            var task = OpenPageArgs.OpenedWithTask;
            OpenPageArgs.OpenedWithTask = null;
            _rewardFlowManager.FlowCompleted += OnFlowCompleted;
            _rewardFlowManager.StartAnimation(task.XpPayout, task.SoftCurrencyPayout, 0);
        }

        private void OnFlowCompleted(RewardFlowResult rewardFlowResult)
        {
            _rewardFlowManager.FlowCompleted -= OnFlowCompleted;
            OpenPageArgs.RewardEnd?.Invoke();
        }

        private void HandleInviteReward()
        {
            _rewardFlowManager.StartAnimation(0, OpenPageArgs.RewardModel.SoftCurrency, 0);
            OpenPageArgs.RewardModel = null;
        }
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        private void ShowPhoneCallPermissionPopup()
        {
            if(_androidPermissionsHelper.HasPhoneCallPermission()) return;
            
            if(!DetectHeadset.Detect()) return;
            
            var bluetoothHeadsetWarningPopupDisplayed =
                PlayerPrefs.GetInt(Constants.HomePage.ANDROID_BT_HEADSET_WARNING_DISPLAYED, default);
            
            if(bluetoothHeadsetWarningPopupDisplayed > 0) return;
            
            var permissionPopupConfiguration = new DialogPopupConfiguration
            {
                PopupType = PopupType.PhoneCallPermissionPopup,
                YesButtonText = "Allow",
                NoButtonText = "Skip",
                OnYes = ShowPermissionDialog,
                OnClose = OnPopupClose
            };
            
            _popupManager.PushPopupToQueue(permissionPopupConfiguration);

            void ShowPermissionDialog()
            {
                _androidPermissionsHelper.RequestManifestPermission(AMM_ManifestPermission.READ_PHONE_STATE,
                                                                    null, null);
            }
            
            void OnPopupClose(object result)
            {
                PlayerPrefs.SetInt(Constants.HomePage.ANDROID_BT_HEADSET_WARNING_DISPLAYED, 1);
            } 
        }
        #endif
    }
}