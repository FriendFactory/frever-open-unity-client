using System;
using System.Linq;
using System.Threading;
using Bridge;
using Common.UserBalance;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.DeepLinking;
using Modules.QuestManaging;
using Modules.ThemeCollection;
using Modules.UniverseManaging;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Animated.Behaviours;
using UIManaging.Common;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

#if UNITY_ANDROID && !UNITY_EDITOR
using Common;
using Common.Permissions;
using SA.Android.Manifest;
#endif
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.PopupSystem.Configurations;
using UnityEngine.UI;

namespace UIManaging.Pages.Home
{
    public sealed class HomePage : GenericPage<HomePageArgs>
    {
        private const float RAYCAST_TARGET_ALPHA = 0.5f;
        private const float SCROLL_BACK_BUTTON_DELTA = 0.1f;
        
        public override PageId Id => PageId.HomePage;

        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private RewardFlowManager _rewardManager;
        [SerializeField] private Image _raycastBlocker;
        [SerializeField] private OpenCreatorScoreButton _openCreatorScoreButton;
        [SerializeField] private ThumbnailSection _thumbnailSection;
        [SerializeField] private CollectionsSectionView _collectionsSection;
        [SerializeField] private Button _scrollBackButton;
        [SerializeField] private EnhancedScroller _collectionsScroller;
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
        [Inject] private IThemeCollectionService _collectionService;
        [Inject] private UsersManager _usersManager;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IUniverseManager _universeManager;

        #if UNITY_ANDROID && !UNITY_EDITOR
        [Inject] private AndroidPermissionsHelper _androidPermissionsHelper;
        #endif

        private HomePagePopupHelper _homePagePopupHelper;
        private CancellationTokenSource _tokenSource;
        private float _defaultScrollPos;
        

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _questButton.Button.onClick.AddListener(OnQuestButton);
            _questButton.HidingBegin += OnQuestHidingBegin;
            _thumbnailSection.RaceToggled += OnRaceToggled;
        }

        private void OnDisable()
        {
            _questButton.Button.onClick.RemoveListener(OnQuestButton);
            _questButton.HidingBegin -= OnQuestHidingBegin;
            _thumbnailSection.RaceToggled -= OnRaceToggled;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            _homePagePopupHelper?.Dispose();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _homePagePopupHelper = new HomePagePopupHelper(_bridge, _popupManager, _dataHolder,
                                                           _rewardManager, _invitationLinkHandler, _snackBarHelper, 
                                                           _pageManager, new FeedPopupHelper(_popupManager, 
                                                               _dataFetcher, _questManager, _amplitudeManager, _universeManager));

            _pageManager = pageManager;
        }

        protected override void OnDisplayStart(HomePageArgs args)
        {
            base.OnDisplayStart(args);

            _thumbnailSection.Initialize();
            
            _raycastBlocker.SetActive(args.EnableInputBlocker);
            _tokenSource = new CancellationTokenSource();

            var userBalanceModel = new StaticUserBalanceModel(_dataHolder);
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.Initialize(userBalanceModel);

            var model = new CollectionSectionModel(_collectionService.ThemeCollections);
            _collectionsSection.Initialize(model);

            if (args.OpenedWithTask != null)
            {
                _rewardManager.Initialize(_dataHolder.UserBalance, _dataHolder.LevelingProgress.Xp);
                
                var config = new TaskCompletedPopupConfiguration(args.OpenedWithTask, StartTaskRewardAnimation);
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(PopupType.TaskCompletedPopup);
            }
            else if (args.ShowHomePagePopups)
            {
                _homePagePopupHelper.ShowHomePagePopups();

                #if UNITY_ANDROID && !UNITY_EDITOR
                ShowPhoneCallPermissionPopup();
                #endif
            }

            if (args.RewardModel != null) HandleInviteReward();

            //_socialActionSection.RefreshSocialActionList();
            _slideInOutBehaviour.Show();
            _raycastBlocker.SetActive(OpenPageArgs.EnableInputBlocker);
                
            RefreshButtonsWithBadges();

            _questManager.UpdateQuestData();
            _dataFetcher.FetchSeason();
            
            _defaultScrollPos = _collectionsScroller.Container.position.y;
            
            UpdateScrollBackButton();

            _scrollBackButton.onClick.AddListener(AnimateScroll);
            _collectionsScroller.ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
        
        protected override void OnHidingBegin(Action onComplete)
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _scrollBackButton.onClick.RemoveListener(AnimateScroll);
            _collectionsScroller.ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);

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
        
        private async void RefreshButtonsWithBadges()
        {
            _openCreatorScoreButton.UpdateSprite();

            await _dataHolder.DownloadProfile();

            _openCreatorScoreButton.RefreshNotificationBadge();
        }
        
        private void OnScrollValueChanged(Vector2 pos)
        {
            UpdateScrollBackButton();
        }

        private void UpdateScrollBackButton()
        {
            var isActive = Mathf.Abs(_collectionsScroller.Container.position.y - _defaultScrollPos) > SCROLL_BACK_BUTTON_DELTA;

            if (_scrollBackButton.gameObject.activeSelf != isActive)
            {
                _scrollBackButton.SetActive(isActive);
            }
        }

        private void AnimateScroll()
        {
            _collectionsScroller.Container.DOMoveY(_defaultScrollPos, 0.5f).SetEase(Ease.InOutCubic);
        }

        private void StartTaskRewardAnimation(object o)
        {
            var task = OpenPageArgs.OpenedWithTask;
            OpenPageArgs.OpenedWithTask = null;

            _rewardManager.FlowCompleted += OnFlowCompleted;
            _rewardManager.StartAnimation(0, task.SoftCurrencyPayout, 0);
        }

        private void OnFlowCompleted(RewardFlowResult rewardFlowResult)
        {
            _rewardManager.FlowCompleted -= OnFlowCompleted;
            OpenPageArgs.RewardEnd?.Invoke();
        }

        private void OnRaceToggled(bool toggleValue)
        {
            var raceId = _characterManager.RaceMainCharacters.First(c => c.Value != _characterManager.SelectedCharacter.Id).Key;
            SwitchRace(raceId);
            
            var model = new CollectionSectionModel(_collectionService.ThemeCollections);
            _collectionsSection.Initialize(model);

            _thumbnailSection.UpdateThumbnail();
        }
        
        private void SwitchRace(long raceId)
        {
            var characterId = _characterManager.RaceMainCharacters[raceId];
            var character = _characterManager.UserCharacters.First(c => c.Id == characterId);
            var universe = _metadataProvider.MetadataStartPack.GetUniverseByRaceId(raceId);
            
            _dataHolder.SetMainCharacter(character);
            _characterManager.SelectCharacter(character);
            _usersManager.UpdateMainCharacterIdForLocalUserOnServer(characterId, universe.Id);
        }

        private void HandleInviteReward()
        {
            _rewardManager.Initialize(_dataHolder.UserBalance, _dataHolder.LevelingProgress.Xp);
            
            _rewardManager.StartAnimation(0, OpenPageArgs.RewardModel.SoftCurrency, 0);
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