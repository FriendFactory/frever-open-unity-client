using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Tasks
{
    internal sealed class VotingTaskShortView : TaskViewBase
    {
        [SerializeField] private Button _viewResultsButton;
        [SerializeField] private GameObject _viewResultsButtonView;
        [SerializeField] private TaskVideoThumbnail _taskVideoThumbnail;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var isReady = ContextData.Task.BattleResultReadyAt <= DateTime.UtcNow;
            _viewResultsButtonView.SetActive(isReady);
            _viewResultsButton.onClick.AddListener(OnClicked);
            
            _taskVideoThumbnail.Initialize(ContextData);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _viewResultsButton.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            ContextData.OnTaskClicked();
        }
    }
}