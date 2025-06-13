using Common;
using Common.ApplicationCore;
using Configs;
using Modules.AppsFlyerManaging;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsStoraging.Core;
using Modules.ContentModeration;
using Modules.CrashRecovery;
using Modules.EditorsCommon;
using Modules.FollowRecommendations;
using Modules.Haptics;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Assets.Caption;
using Modules.SocialActions;
using Modules.Sound;
using Modules.VideoRecording;
using Tayx.Graphy;
using TipsManagment;
using UIManaging.Common.Templates;
using UIManaging.Pages.CreatorScore;
using UIManaging.Pages.EditUsername;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace Installers
{
    internal sealed class BindingsInstaller : MonoInstaller
    {
        [SerializeField] private AudioSourceManager _audioSourceManager;
        [SerializeField] private ARSession _arSession;
        [SerializeField] private ARFaceManager _arFaceManager;
        [SerializeField] private ARCameraManager _arCameraManager;
        [SerializeField] private FetcherConfig _fetcherConfig;
        [SerializeField] private TutorialConfig _tutorialConfig;
        [SerializeField] private SessionInfo _sessionInfo;
        [SerializeField] private DefaultSubCategoryColors _defaultSubCategoryColors;
        [SerializeField] private DNASlidersGroupingSettings _dnaSlidersGroupingSettings;
        [SerializeField] private ColorPalletHidingConfiguration _colorPalletHidingConfiguration;
        [SerializeField] private Object _showOnlyCaptionPrefab;
        [SerializeField] private VideoRecorderBase _avProVideoRecorderPrefab;
        [SerializeField] private SoundBank _soundBank;
        [SerializeField] private SocialActionModelFactory _socialActionModelFactory;
        [SerializeField] private AppsFlyerManager _appsFlyerManager;
        [SerializeField] private EditUsernameLocalization _editUsernameLocalization;
        [Header("Character configs")]
        [SerializeField] private CharacterManagerConfig _characterManagerConfig;
        [SerializeField] private BoneColliderSettings[] _boneColliderSettings;
        [Header("Tools")] 
        [SerializeField] private GraphyManager _graphyManagerPrefab;
        [SerializeField] private AnimationCurveScriptableObject _watermarkFadeAnimation;
        [SerializeField] private CameraFocusAnimationCurveProvider _cameraFocusAnimationCurveProvider;
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.BindAvatarDisplay();
            Container.BindAndSetupBridge();
            Container.BindAmplitudeServices();
            Container.BindErrorTracking();
            Container.BindRenderingPipelineManager();
            Container.BindBlockedAccountsServices();
            Container.BindMemoryManager();
            Container.BindPermissionServices();
            Container.BindNativeGallery();
            Container.BindInputServices();
            Container.BindFetchingServices(_fetcherConfig);
            Container.BindMiscellaneousServices(_sessionInfo);
            Container.BindUserServices();
            Container.BindVideoServices();
            Container.BindOnBoardingServices(_tutorialConfig);
            Container.BindAssetManager();
            Container.BindAssetServices(_audioSourceManager);
            Container.BindLevelServices();
            Container.BindFaceAnimationServices();
            Container.BindVideoUploadingServices();
            Container.BindCommonUIServices();
            Container.BindARServices(_arFaceManager, _arCameraManager, _arSession);
            Container.BindVideoRecorder(_avProVideoRecorderPrefab);
            Container.BindContactsProvider();
            Container.BindCameraSystemServices();
            Container.BindLevelManager(_cameraFocusAnimationCurveProvider);
            Container.BindNotificationServices();
            Container.BindCharacterServices(_characterManagerConfig, _boneColliderSettings);
            Container.BindUMAServices(_defaultSubCategoryColors, _dnaSlidersGroupingSettings, _colorPalletHidingConfiguration);
            Container.BindThumbnailServices();
            Container.BindFollowers();
            Container.BindPostRecordEditorServices();
            Container.BindDeviceAudioOutputControl();
            Container.BindCharacterEditCommands();
            Container.BindBridgeAdapters();
            Container.SetupExecutionOrder();
            Container.BindPaginationLoaders();
            Container.BindFactory<CaptionView, CaptionLoader.CaptionViewFactory>().FromComponentInNewPrefab(_showOnlyCaptionPrefab);
            Container.Bind<CaptionPanelRotationGestureSource>().AsTransient();
            Container.BindInterfacesAndSelfTo<EditorSettingsProvider>().AsSingle();
            Container.BindUserScenarioManaging();
            Container.BindSeasonRewards();
            Container.BindStatsMonitor(_graphyManagerPrefab);
            Container.BindIAPServices();
            Container.BindSoundServices(_soundBank);
            Container.BindDeeplinkServices();
            Container.BindVotingServices();
            Container.BindInterfacesAndSelfTo<CreatorScoreHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<TemplatesLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<FollowRecommendationsListModelProvider>().AsSingle();
            Container.BindSocialActionsServices(_socialActionModelFactory);
            Container.BindInterfacesAndSelfTo<TextContentValidator>().AsSingle();
            Container.BindFrameRateServices();
            Container.BindThemeCollectionService();
            Container.BindCrewServices();
            Container.BindCharacterEditorServices();
            Container.BindAppsFlyerServices(_appsFlyerManager);
            Container.BindVideoMessagePageServices();
            Container.BindLocalizationServices();
            Container.BindInterfacesAndSelfTo<HapticFeedbackManager>().AsSingle();
            Container.BindAccountVerificationServices();
            Container.BindUniverseServices();
            Container.Bind<AnimationCurve>().WithId(Constants.Binding.WATERMARK_ANIMATION)
                     .FromInstance(_watermarkFadeAnimation.Curve).AsSingle();
            Container.BindInterfacesAndSelfTo<AppStuckCacheCleaner>().AsSingle();
            Container.BindPageLoadTrackers();
            Container.Bind<EditUsernameLocalization>().FromInstance(_editUsernameLocalization);
        }
    }
}