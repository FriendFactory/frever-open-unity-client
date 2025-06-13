using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    public sealed class TaskDetailsPage : GenericPage<TaskDetailsPageArgs>
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TaskVideosGridHeaderView _taskDetailsHeader;
        [SerializeField] private List<TextMeshProUGUI> _rewardTexts;
        [SerializeField] private GameObject _rewardPanel;
        
        [Inject] private IBridge _bridge;
        [Inject] private VideoManager _videoManager;
        [Inject] private PageManager _pageManager;

        public override PageId Id => PageId.TaskDetails;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClick);
        }

        //---------------------------------------------------------------------
        // Page
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(TaskDetailsPageArgs args)
        {
            base.OnDisplayStart(args);
                
            SetupUI();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _taskDetailsHeader.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBackButtonClick()
        {
            _pageManager.MoveBack();
        }

        private void SetupUI()
        {
            var taskModel = new TaskModel(_videoManager, _pageManager, _bridge, OpenPageArgs.TaskFullInfo);
            
            _taskDetailsHeader.Initialize(taskModel.CreateViewArgs(OpenPageArgs.Visited));
            OpenPageArgs.Visited = true;

            if (OpenPageArgs.TaskFullInfo.BattleRewards == null)
            {
                _rewardPanel.SetActive(false);
            }
            else
            {
                _rewardPanel.SetActive(true);
                
                for (var i = 0; i < _rewardTexts.Count; i++)
                {
                    var targetReward = OpenPageArgs.TaskFullInfo.BattleRewards.FirstOrDefault(reward => reward.Place == i + 1);

                    if (targetReward == null)
                    {
                        Debug.LogError($"No data for {i + 1} place reward");
                        continue;
                    }
                
                    _rewardTexts[i].text = targetReward.SoftCurrency.ToString();
                }
            }
        }
    }
}