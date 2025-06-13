using System;
using System.Collections;
using System.Collections.Generic;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Modules.UserScenarios.Common;
using Common.TimeManaging;
using Extensions;
using Modules.Amplitude;
using Modules.UniverseManaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    public sealed class TaskDetailsHeader : TaskDetailsBase
    {
        [Inject] private IScenarioManager _scenarioManager;
        
        [SerializeField] private TaskHeaderBackground _background;
        [SerializeField] private TMP_Text _taskName;
        [SerializeField] private TMP_Text _taskDescription;
        [SerializeField] private Button _joinChallengeButton;
        [Header("Subtitle")]
        [SerializeField] private GameObject _timeContainer;
        [SerializeField] private TMP_Text _timeLeftText;
        [Space]
        [SerializeField] private GameObject _usersJoinedContainer;
        [SerializeField] private TMP_Text _usersJoinedText;
        [SerializeField] private string _usersJoinedFormat = "{0} joined";

        [Header("Rewards")] 
        [SerializeField] private GameObject _rewardsHeader;
        [SerializeField] private RectTransform _rewardsLayout;
        [SerializeField] private GameObject _coinRewardContainer;
        [SerializeField] private TMP_Text _coinRewardAmount;
        [SerializeField] private GameObject _hardCurrencyRewardContainer;
        [SerializeField] private TMP_Text _hardCurrencyRewardAmount;
        [SerializeField] private GameObject _xpRewardContainer;
        [SerializeField] private TMP_Text _xpRewardAmount;
        [SerializeField] private GameObject _completedContainer;

        [Inject] private IBridge _bridge;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private IUniverseManager _universeManager;
        
        private readonly WaitForSecondsRealtime _waitForOneSecond = new(1);
        private DateTime _deadline;
        private Coroutine _deadlineCountdownCoroutine;
        private TaskFullInfo _taskFullInfo;
        private bool _requireTaskInfoUpdate;
        
        public override void Initialize(TaskDetailsHeaderArgs args)
        {
            CleanUp();

            _requireTaskInfoUpdate = args.RequireTaskInfoUpdate;
            _taskName.text = args.TaskName;

            if (_taskDescription != null)
            {
                _taskDescription.text = args.TaskDescription;
            }

            _deadline = args.Deadline;
            var taskActive = _deadline > DateTime.UtcNow;
            var taskCompleted = taskActive && (args.CoinReward > 0 || args.XpReward > 0);
            
            if (args.TaskFullInfo != null && _taskFullInfo is null)
            {
                _taskFullInfo = args.TaskFullInfo;

                if (_joinChallengeButton != null)
                {
                    _joinChallengeButton.onClick.AddListener(OnJoinTaskButtonClick);
                    _joinChallengeButton.interactable = true;
                }
            }
            
            _background.Setup(args.TaskType, taskCompleted);
            
            SetupClock(taskActive && args.TaskType != TaskType.Onboarding);
            SetupUsersJoined(args.TaskType != TaskType.Onboarding, args.CreatorsCount);
            SetupRewardSection(taskCompleted, args.CoinReward, -1, args.XpReward);

            if (_completedContainer != null)
            {
                _completedContainer.SetActive(taskActive && args.CoinReward <= 0 && args.XpReward <= 0);
            }
        }

        private void OnEnable()
        {
            if (!_deadline.Equals(default))
                StartDeadlineCountdown();
        }

        private void OnDisable()
        {
            if (!_deadline.Equals(default))
                StopDeadlineCountdown();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CleanUp()
        {
            _taskFullInfo = null;
            
            if (_joinChallengeButton != null)
            {
                _joinChallengeButton.onClick.RemoveListener(OnJoinTaskButtonClick);
            }
        }
        
        private void StartDeadlineCountdown()
        {
            if (_deadlineCountdownCoroutine != null)
                StopCoroutine(_deadlineCountdownCoroutine);

            _deadlineCountdownCoroutine = StartCoroutine(DeadlineCountdownRoutine());
        }

        private IEnumerator DeadlineCountdownRoutine()
        {
            while (true)
            {
                if (_deadline <= DateTime.UtcNow)
                {
                    yield return null;
                    continue;
                }
                
                var timeLeft = _deadline - DateTime.UtcNow;

                _timeLeftText.text = $"{timeLeft.ToFormattedString()} left";
                
                yield return _waitForOneSecond;
            }
        }

        private void StopDeadlineCountdown()
        {
            if (_deadlineCountdownCoroutine is null)
                return;

            StopCoroutine(_deadlineCountdownCoroutine);
            _deadlineCountdownCoroutine = null;
        }

        private void SetupClock(bool showClock)
        {
            if (_deadline.Equals(default) || _deadline <= DateTime.UtcNow)
            {
                _timeContainer.SetActive(false);
                return;
            }
                
            var timeLeft = _deadline - DateTime.UtcNow;

            _timeLeftText.text = $"{timeLeft.ToFormattedString()} left";
            _timeContainer.SetActive(showClock);
        }

        private void SetupUsersJoined(bool showUsersJoined, int creatorsCount)
        {
            if (_usersJoinedText != null)
                _usersJoinedText.text = string.Format(_usersJoinedFormat, creatorsCount.ToShortenedString());

            if (_usersJoinedContainer != null)
                _usersJoinedContainer.SetActive(showUsersJoined && creatorsCount > -1);
        }

        private void SetupRewardSection(bool taskActive, int softCurrencyPayout, int hardCurrencyPayout, int xpPayout)
        {
            if (_rewardsHeader != null)
            {
                _rewardsHeader.SetActive(taskActive);
            }
            
            if (softCurrencyPayout > -1)
            {
                _coinRewardContainer.SetActive(taskActive && softCurrencyPayout > 0);
                _coinRewardAmount.text = softCurrencyPayout.ToString();
            }
            else
            {
                _coinRewardContainer.SetActive(false);
            }

            // Disabled for MVP
            if (hardCurrencyPayout > -1)
            {
                _hardCurrencyRewardContainer.SetActive(false);
                //_hardCurrencyRewardContainer.SetActive(taskActive);
            }

            if (xpPayout > -1)
            {
                _xpRewardAmount.text = xpPayout.ToString();
                _xpRewardContainer.SetActive(taskActive && xpPayout > 0);
            }
            else
            {
                _xpRewardContainer.SetActive(false);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardsLayout);
        }

        private async void OnJoinTaskButtonClick()
        {
            _joinChallengeButton.interactable = false;

            if (_requireTaskInfoUpdate)
            {
                var result = await _bridge.GetTaskFullInfoAsync(_taskFullInfo.Id);

                if (result.IsError)
                {
                    Debug.LogError($"Failed to update task full info, reason: {result.ErrorMessage}");
                }

                if (result.IsSuccess)
                {
                    _taskFullInfo = result.Model;
                }
            }

            LogTaskJoined();

            _universeManager.SelectUniverse(universe =>
            {
                if (_taskFullInfo.TaskType == TaskType.Voting)
                {
                    _scenarioManager.ExecuteVotingFeed(universe, _taskFullInfo);
                }
                else
                { 
                    _scenarioManager.ExecuteTask(universe, _taskFullInfo);
                }
            });
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