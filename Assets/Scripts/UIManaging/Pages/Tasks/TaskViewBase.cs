using UnityEngine;
using UnityEngine.UI;
using Abstract;
using UIManaging.Pages.Tasks.TaskVideosGridPage;

namespace UIManaging.Pages.Tasks
{
    public abstract class TaskViewBase : BaseContextDataView<TaskModel>
    {
        [SerializeField] private TaskDetailsBase _taskDetailsHeader;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void SetParentScrollRect(ScrollRect scrollRect) { }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _taskDetailsHeader.Initialize(new TaskDetailsHeaderArgs(ContextData.Task));
        }
    }

    public abstract class ActiveTaskViewBase : TaskViewBase
    {
        [SerializeField] private GameObject _activeBackground;
        [SerializeField] private GameObject _inactiveBackground;
        [SerializeField] private Button _joinButton;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var activeBg = ContextData.Task.SoftCurrencyPayout > 0 || ContextData.Task.XpPayout > 0;
            
            _activeBackground.SetActive(activeBg);
            _inactiveBackground.SetActive(!activeBg);
            _joinButton.onClick.AddListener(OnTaskClicked);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _joinButton.onClick.RemoveListener(OnTaskClicked);
        }

        private void OnTaskClicked()
        {
            ContextData?.OnTaskClicked();
        }
    }
}