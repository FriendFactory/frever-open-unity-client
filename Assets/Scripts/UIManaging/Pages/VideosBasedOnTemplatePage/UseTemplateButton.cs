using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.VideoServer;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.FeaturesOpening;
using Modules.LevelManaging.Editing.Templates;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.VotingResult;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.VideosBasedOnTemplatePage
{
    public sealed class UseTemplateButton : MonoBehaviour
    {
        private const string VOTING_IN_PROGRESS_MESSAGE = "Your video is still being voted on";

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonText;

        [Inject] private ITemplateProvider _templateProvider;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IAppFeaturesManager _featuresManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private IVotingBattleResultManager _votingBattleResultManager;
        [Inject] private MusicDownloadHelper _musicDownloadHelper;
        [Inject] private FeedLocalization _feedLocalization;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IUniverseManager _universeManager;

        private long _templateId;
        private HashtagInfo _hashtagInfo;
        private TaskFullInfo _taskFullInfo;
        private bool _isYourVideo;
        private bool _isOnTaskFeed;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(long templateId, UnityAction callback = null)
        {
            _hashtagInfo = null;
            _templateId = templateId;
            _button.interactable = true;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(callback ?? OnApplyTemplateClick);
            _buttonText.text = _feedLocalization.UseTemplateButton;
        }
        
        public void Setup(HashtagInfo hashtagInfo)
        {
            _hashtagInfo = hashtagInfo;
            _button.interactable = true;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnApplyHashtagClick);
            _buttonText.text =_feedLocalization.JoinHashtagButton;
        }

        public void Setup(TaskFullInfo taskFullInfo, bool isYourVideo, bool isTaskFeed, UnityAction callback = null)
        {
            _taskFullInfo = taskFullInfo;
            _templateId = _taskFullInfo.TemplateId ?? 0;
            _isOnTaskFeed = isTaskFeed;
            _isYourVideo = isYourVideo;
            _button.interactable = true;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(callback ?? OnJoinChallengeClick);
            
            SetupLabel();
        }

        public void Interactable(bool value)
        {
            _button.interactable = value;
        }
        

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupLabel()
        {
            if (_isOnTaskFeed)
            {
                _buttonText.text = _feedLocalization.JoinChallengeButton;
                return;
            }
            
            _buttonText.text = _taskFullInfo.TaskType == TaskType.Voting 
                ? _taskFullInfo.XpPayout == 0 && _taskFullInfo.SoftCurrencyPayout == 0 
                    ? _isYourVideo 
                        ? _feedLocalization.ChallengeResultsButton
                        : _feedLocalization.ExploreChallengesButton
                    : _feedLocalization.JoinStyleChallengeButton 
                : _feedLocalization.JoinChallengeButton;
        }

        private async void OnApplyTemplateClick()
        {
            if (!_featuresManager.IsCreationNewLevelAllowed)
            {
                _popupManagerHelper.ShowLockedLevelCreationFeaturePopup(_featuresManager.ChallengesRemainedForEnablingNewLevelCreation);
                return;
            }
            
            if (! await CheckMusicAvailable()) return;

            _universeManager.SelectUniverse(universe =>
            {
                _button.interactable = false;

                var args = new CreateNewLevelBasedOnTemplateScenarioArgs
                {
                    Universe = universe,
                    Template = new TemplateInfo {Id = _templateId},
                    OnCancelCallback = () => Interactable(true)
                };
                _scenarioManager.ExecuteNewLevelCreationBasedOnTemplate(args);
            });
        }

        private void OnApplyHashtagClick()
        {
            _universeManager.SelectUniverse(universe =>
            {
                _button.interactable = false;
                _scenarioManager.ExecuteNewLevelCreation(universe: universe, _hashtagInfo);
            });
        }

        private async void OnJoinChallengeClick()
        {
            LogTaskJoined();
            
            if (! await CheckMusicAvailableForTask()) return;
            
            if (_taskFullInfo.TaskType == TaskType.Voting)
            {
                if (_taskFullInfo.XpPayout == 0 && _taskFullInfo.SoftCurrencyPayout == 0)
                {
                    if (!_isYourVideo)
                    {
                        _button.interactable = false;
                    
                        var pageArgs = new TasksPageArgs();
                    
                        _pageManager.MoveNext(pageArgs);
                    }
                    else if (_taskFullInfo.BattleResultReadyAt < DateTime.UtcNow)
                    {
                        _button.interactable = false;
                        
                        var popupConfig = new LoadingIndicatorPopupConfiguration();
                        
                        _popupManager.SetupPopup(popupConfig);
                        _popupManager.ShowPopup(PopupType.LoadingIndicator, true);
            
                        var battleResult = await _votingBattleResultManager.GetVotingBattleResult(_taskFullInfo.Id);
                        var pageArgs = new VotingResultPageArgs(_taskFullInfo.Id, _taskFullInfo.Name, battleResult);
                    
                        popupConfig.Hide();
                        _pageManager.MoveNext(pageArgs);
                    }
                    else
                    {
                        _snackBarHelper.ShowSuccessDarkSnackBar(VOTING_IN_PROGRESS_MESSAGE);
                    }
                } 
                else if (_taskFullInfo.Deadline < DateTime.UtcNow)
                {
                    var pageArgs = new TasksPageArgs();
                    
                    _pageManager.MoveNext(pageArgs);
                }
                else 
                {
                    _button.interactable = false;

                    if (_isOnTaskFeed)
                    {
                        var args = new TaskDetailsPageArgs(_taskFullInfo);
                        _pageManager.MoveNext(args);
                        return;
                    }
                    
                    var pageArgs = new TasksPageArgs();
                    _pageManager.MoveNext(pageArgs);
                }
            }
            else
            {
                _universeManager.SelectUniverse(universe =>
                {
                    _button.interactable = false;
                    _scenarioManager.ExecuteTask(universe, _taskFullInfo);
                });
            }
        }

        private async Task<bool> CheckMusicAvailable()
        {
            _button.interactable = false;
            var templateEvent = await _templateProvider.GetTemplateEvent(_templateId);
            if (templateEvent == null)
            {
                _button.interactable = true;
                return false;
            }

            var isAvailable = await _musicDownloadHelper.CheckIfMusicAvailableAsync(templateEvent);
            
            _button.interactable = true;
            return isAvailable;
        }
        
        private async Task<bool> CheckMusicAvailableForTask()
        {
            _button.interactable = false;
            var isAvailable = await _musicDownloadHelper.CheckIfMusicAvailableForLevelAsync(_taskFullInfo.LevelId ?? 0);
            
            _button.interactable = true;
            return isAvailable;
        }
        
        private void LogTaskJoined()
        {
            var levelUpMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.TASK_ID] = _taskFullInfo.Id,
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.TASK_JOINED, levelUpMetaData);
        }
    }
}