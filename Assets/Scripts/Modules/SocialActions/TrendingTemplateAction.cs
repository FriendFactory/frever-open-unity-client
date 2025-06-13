using System;
using Bridge.Models.ClientServer.Template;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine.SceneManagement;
using Zenject;

namespace Modules.SocialActions
{
    public class TrendingTemplateAction : ISocialAction
    {
        private readonly Guid _recommendationId;
        private readonly long _actionId;
        private readonly TemplateInfo _templateInfo;
        private readonly IScenarioManager _scenarioManager;
        private readonly PopupManager _popupManager;
        
        private readonly InformationPopupConfiguration _loadingPopupConfiguration;

        //todo: fix injection or delete obsolete script because the type is resolved manually via construcotr
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        [Inject] private IMetadataProvider _metadataProvider;

        public TrendingTemplateAction(Guid recommendationId, long actionId, TemplateInfo templateInfo, IScenarioManager scenarioManager,
            PopupManager popupManager)
        {
            ProjectContext.Instance.Container.Inject(this);
            
            _recommendationId = recommendationId;
            _actionId = actionId;
            _templateInfo = templateInfo;
            _scenarioManager = scenarioManager;
            _popupManager = popupManager;
            
            _loadingPopupConfiguration = new InformationPopupConfiguration
            {
                PopupType = PopupType.Loading, Title = _loadingOverlayLocalization.SettingTheStageHeader
            };
        }
        
        public void Execute()
        {
            
            _popupManager.SetupPopup(new IPSelectionPopupConfiguration(_metadataProvider.MetadataStartPack.Universes,
                universe =>
                {
                    SceneManager.sceneLoaded += OnPageSwitch;
                    OpenLoadingPopup();
            
                    var args = new CreateNewLevelBasedOnTemplateScenarioArgs
                    {
                        RecommendationId = _recommendationId,
                        SocialActionId = _actionId,
                        Template = _templateInfo,
                        ShowGridPage  = true,
                    };
            
                    _scenarioManager.ExecuteNewLevelCreationBasedOnTemplateSocialAction(args);
                }));
            _popupManager.ShowPopup(PopupType.IPSelection);
        }
        
        private void OpenLoadingPopup()
        {
            _popupManager.SetupPopup(_loadingPopupConfiguration);
            _popupManager.ShowPopup(_loadingPopupConfiguration.PopupType, true);
        }

        private void OnPageSwitch(Scene arg0, LoadSceneMode loadSceneMode)
        {
            _popupManager.ClosePopupByType(_loadingPopupConfiguration.PopupType);
            
            SceneManager.sceneLoaded -= OnPageSwitch;
        }
        
    }
}