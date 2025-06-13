using System;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.VotingResult;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    [RequireComponent(typeof(Button))]
    public class VideoPartOfTaskButton : MonoBehaviour
    {
        private const string PASSED_DEADLINE_MESSAGE = "Sorry, deadline for this challenge has passed";
        
        [SerializeField] private TMP_Text _taskNameText;

        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private IVotingBattleResultManager _votingBattleResultManager;
        [Inject] private SnackBarHelper _snackBarHelper;

        private Button _button;
        private long _taskId;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.interactable = true;
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(long taskId, string taskName)
        {
            _taskId = taskId;
            SetTaskName(taskName);
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private async void OnClick()
        {
            _button.interactable = false;
            var popupConfig = new LoadingIndicatorPopupConfiguration();
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(PopupType.LoadingIndicator, true);
            
            var taskFullInfoResult = await _bridge.GetTaskFullInfoAsync(_taskId);
            
            if (taskFullInfoResult.IsRequestCanceled) return;
            
            if (taskFullInfoResult.IsError)
            {
                popupConfig.Hide();
                Debug.LogError(taskFullInfoResult.ErrorMessage);
                _button.interactable = true;
                return;
            }

            if (taskFullInfoResult.Model.TaskType != TaskType.Voting)
            {
                var args = new TaskVideosGridPageArgs(taskFullInfoResult.Model.Id);
                _pageManager.MoveNext(PageId.TaskVideoGrid, args);
                return;
            }

            if (taskFullInfoResult.Model.Deadline < DateTime.UtcNow &&
                !(taskFullInfoResult.Model.SoftCurrencyPayout == 0 && taskFullInfoResult.Model.XpPayout == 0))
            {
                _snackBarHelper.ShowSuccessDarkSnackBar(PASSED_DEADLINE_MESSAGE);
                popupConfig.Hide();
                _button.interactable = true;
                return;
            }

            if (taskFullInfoResult.Model.SoftCurrencyPayout == 0 && taskFullInfoResult.Model.XpPayout == 0)
            {
                if (taskFullInfoResult.Model.BattleResultReadyAt <= DateTime.UtcNow)
                {
                    var battleResult = await _votingBattleResultManager.GetVotingBattleResult(_taskId);

                    var args = new VotingResultPageArgs(_taskId, taskFullInfoResult.Model.Name, battleResult);
                    _pageManager.MoveNext(PageId.VotingResult, args);
                }
                else
                {
                    var config = new WaitVotingResultPopupConfiguration();
                    _popupManager.SetupPopup(config);
                    _popupManager.ShowPopup(config.PopupType);
                    popupConfig.Hide(); 
                    _button.interactable = true;
                }
            }
            else
            {
                var args = new TaskDetailsPageArgs(taskFullInfoResult.Model);
                _pageManager.MoveNext(PageId.TaskDetails, args);
            }
        }

        private void SetTaskName(string taskName)
        {
            _taskNameText.text = taskName;
        }
    }
}