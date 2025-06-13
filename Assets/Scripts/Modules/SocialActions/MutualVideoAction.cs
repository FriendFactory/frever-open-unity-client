using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using Common.BridgeAdapter;
using Models;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Modules.SocialActions
{
    public class MutualVideoAction : ISocialAction
    {
        private readonly Guid _recommendationId;
        private readonly long _actionId;
        private readonly long _videoId;
        private readonly long[] _characterIds;
        private readonly IBridge _bridge;
        private readonly InformationPopupConfiguration _loadingPopupConfiguration;
        private readonly IScenarioManager _scenarioManager;
        private readonly ILevelService _levelService;
        private readonly CharacterManager _characterManager;
        private readonly PopupManager _popupManager;

        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        public MutualVideoAction(Guid recommendationId, long actionId, long videoId, long[] characterIds, IBridge bridge, IScenarioManager scenarioManager,
            ILevelService levelService, CharacterManager characterManager, PopupManager popupManager)
        {
            ProjectContext.Instance.Container.Inject(this);
            
            _recommendationId = recommendationId;
            _actionId = actionId;
            _videoId = videoId;
            _characterIds = characterIds;
            
            _bridge = bridge;
            _scenarioManager = scenarioManager;
            _levelService = levelService;
            _characterManager = characterManager;
            _popupManager = popupManager;
            
            _loadingPopupConfiguration = new InformationPopupConfiguration
            {
                PopupType = PopupType.Loading, Title = _loadingOverlayLocalization.SettingTheStageHeader
            };
        }
        
        public async void Execute()
        {
            OpenLoadingPopup();
            
            var level = await GetLevel();
            if (level is null)
            {
                _popupManager.ClosePopupByType(_loadingPopupConfiguration.PopupType);
                return;
            }

            var characters = await _characterManager.GetCharacterFullInfos(_characterIds);
            
            // TODO: for now it's considered to be a deprecated feature. Otherwise, use the IPSelection Popup here
            _scenarioManager.ExecuteLevelRemixingSocialAction(_recommendationId, _actionId, level, _videoId, characters);
            _popupManager.ClosePopupByType(_loadingPopupConfiguration.PopupType);
        }

        private void OpenLoadingPopup()
        {
            _popupManager.SetupPopup(_loadingPopupConfiguration);
            _popupManager.ShowPopup(_loadingPopupConfiguration.PopupType, true);
        }

        private async Task<Level> GetLevel()
        {
            var videoResult = await _bridge.GetVideoAsync(_videoId);
            if (videoResult.IsError)
            {
                Debug.LogError(videoResult.ErrorMessage);
                return null;
            }
            
            var video = videoResult.ResultObject;
            var levelResult = await _levelService.GetLevelAsync(video.LevelId.Value);
            if (!levelResult.IsSuccess)
            {
                Debug.LogError(levelResult.ErrorMessage);
                return null;
            }

            return levelResult.Level;
        }
    }
}