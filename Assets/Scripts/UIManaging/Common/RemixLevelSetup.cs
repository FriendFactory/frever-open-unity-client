using System;
using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Common.BridgeAdapter;
using JetBrains.Annotations;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using Zenject;

namespace UIManaging.Common
{
    public enum RemixType
    {
        Simple,
        Shuffled
    }
    
    [UsedImplicitly]
    public sealed class RemixLevelSetup
    {
        private long _levelId;
        private long _videoId;
        private Action _onRemixCanceled;
        private long? _initialTemplateId;

        private readonly PopupManager _popupManager;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly IBridge _bridge;
        private readonly ILevelService _levelService;
        private readonly IScenarioManager _scenarioManager;
        private readonly InformationPopupConfiguration _loadingPopupConfiguration;
        private readonly LoadingOverlayLocalization _loadingOverlayLocalization;
        private readonly IUniverseManager _universeManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public RemixLevelSetup(PopupManager popupManager, IBridge bridge, SnackBarHelper snackBarHelper, ILevelService levelService, IScenarioManager scenarioManager, LoadingOverlayLocalization loadingOverlayLocalization, IUniverseManager universeManager)
        {
            ProjectContext.Instance.Container.Inject(this);
            
            _popupManager = popupManager;
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
            _levelService = levelService;
            _scenarioManager = scenarioManager;
            _loadingOverlayLocalization = loadingOverlayLocalization;
            _universeManager = universeManager;

            _loadingPopupConfiguration = new InformationPopupConfiguration
            {
                PopupType = PopupType.Loading, Title = _loadingOverlayLocalization.SettingTheStageHeader
            };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(Video video, Action onRemixCanceled = null)
        {
            if (!video.LevelId.HasValue)
            {
                throw new InvalidOperationException($"Can't remix video which has no level. Video id: {video.Id}");
            }
            
            _levelId = video.LevelId.Value;
            _videoId = video.Id;
            _onRemixCanceled = onRemixCanceled;
            _initialTemplateId = video.MainTemplate?.Id;

            InitiateRemixing();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OpenLoadingPopup()
        {
            _popupManager.SetupPopup(_loadingPopupConfiguration);
            _popupManager.ShowPopup(_loadingPopupConfiguration.PopupType, true);
        }

        private void InitiateRemixing()
        {
            if (Constants.Features.AI_REMIX_ENABLED)
            {
                var config = new RemixOptionsPopupConfiguration
                {
                    OnSimpleRemixChosen = () => OnRemixTypeSelected(RemixType.Simple),
                    OnShuffledRemixChosen = () => OnRemixTypeSelected(RemixType.Shuffled),
                    OnCancelled = _onRemixCanceled
                };
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(config.PopupType);
            }
            else
            {
                OnRemixTypeSelected(RemixType.Simple);
            }
        }

        private void OnRemixTypeSelected(RemixType remixType)
        { 
            _universeManager.SelectUniverse(OnUniverseSelected, _onRemixCanceled);
            return;

            async void OnUniverseSelected(Universe universe)
            {
                OpenLoadingPopup();

                var result = await GetLevelAsync(remixType);

                if (!result.IsSuccess)
                {
                    if (!await IsVideoAvailable())
                    {
                        CloseLoadingPopup();
                        DisplayErrorMessage();
                        return;
                    }

                    if (result.ErrorMessage.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
                    {
                        _snackBarHelper.ShowAssetInaccessibleSnackBar();
                    }

                    _onRemixCanceled?.Invoke();
                }
                else
                {
                    _scenarioManager.ExecuteLevelRemixing(universe, result.Level, _videoId, _onRemixCanceled, _initialTemplateId);
                }

                _popupManager.ClosePopupByType(_loadingPopupConfiguration.PopupType);
            }
        }

        private Task<LevelResult> GetLevelAsync(RemixType remixType)
        {
            switch (remixType)
            {
                case RemixType.Simple:
                    return _levelService.GetLevelAsync(_levelId);
                case RemixType.Shuffled:
                    return _levelService.GetShuffledLevelAsync(_levelId);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DisplayErrorMessage()
        {
            _snackBarHelper.ShowInformationSnackBar("Video was deleted or set to private. Can't remix", 2);
        }

        private async Task<bool> IsVideoAvailable()
        {
            var videoResult = await _bridge.GetVideoAsync(_videoId);
            return videoResult.IsSuccess;
        }
        
        private void CloseLoadingPopup()
        {
            _popupManager.ClosePopupByType(_loadingPopupConfiguration.PopupType); 
        }
    }
}