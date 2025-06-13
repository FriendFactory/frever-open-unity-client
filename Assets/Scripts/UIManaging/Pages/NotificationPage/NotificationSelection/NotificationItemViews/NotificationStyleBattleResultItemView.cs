using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Extensions;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationStyleBattleResultItemView : NotificationItemView<NotificationStyleBattleResultItemModel>
    {
        [SerializeField] private Button[] _buttons;

        [Inject] private PageManager _pageManager;
        [Inject] private IBridge _bridge;

        private CancellationTokenSource _cancellationTokenSource;

        private long _taskId;

        protected override string Description => _localization.StyleBattleResultFormat;

        private void OnEnable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            foreach (var button in _buttons)
            {
                button.onClick.AddListener(ShowResultPage);
            }
        }

        private void OnDisable()
        {
            _cancellationTokenSource.CancelAndDispose();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _taskId = ContextData.GroupId;
        }

        private async void ShowResultPage()
        {
            var taskInfo = await RequestTaskInfo();
            if (taskInfo == null) return;
            if(_cancellationTokenSource.IsCancellationRequested) return;
            
            var args = new VotingResultPageArgs(taskInfo.Id, taskInfo.Name);
            _pageManager.MoveNext(args);
        }

        private async Task<TaskFullInfo> RequestTaskInfo()
        {
            var taskInfoResult = await _bridge.GetTaskFullInfoAsync(_taskId, _cancellationTokenSource.Token);
            if (taskInfoResult.IsSuccess) return taskInfoResult.Model;
            
            if(taskInfoResult.IsError) Debug.LogError(taskInfoResult.ErrorMessage);
            
            return null;
        }
    }
}
