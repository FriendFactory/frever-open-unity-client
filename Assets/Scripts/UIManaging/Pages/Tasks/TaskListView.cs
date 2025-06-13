using System.Collections;
using EnhancedUI.EnhancedScroller;
using Abstract;
using Common.TimeManaging;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public class TaskListView : BaseContextDataView<ITaskListModel>, IEnhancedScrollerDelegate
    {
        private const float TIMER_UPDATE_PERIOD = 1f;
        
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _itemPrefab;
        [SerializeField] private GameObject _noTasksObj;
        [SerializeField] private TextMeshProUGUI _noTasksTimeText;
        
        private float _cellSize;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _enhancedScroller.scrollerScrolled += OnScrollerScrolled;
            _cellSize = _enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal 
                ? _itemPrefab.GetComponent<RectTransform>().rect.width
                : _itemPrefab.GetComponent<RectTransform>().rect.height;
        }

        private void OnEnable()
        {
            StartCoroutine(TimerCoroutine());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _enhancedScroller.scrollerScrolled -= OnScrollerScrolled;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ReloadData()
        {
            _enhancedScroller.ReloadData();
        }
        
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_itemPrefab);
            var taskView = cellView.GetComponent<TaskViewBase>();
            var taskModel = ContextData.Tasks[dataIndex];
            
            taskView.Initialize(taskModel);
            taskView.SetParentScrollRect(_enhancedScroller.ScrollRect);
            
            return cellView;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellSize;
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.Tasks?.Count ?? 0;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            ContextData.TasksChanged += UpdateTaskList;
            _enhancedScroller.ReloadData();
            
            if (_noTasksObj != null)
            {
                _noTasksObj.SetActive(false);
            }
            
            ContextData.RequestPage();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            if (ContextData != null)
            {
                ContextData.TasksChanged -= UpdateTaskList;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdateTaskList()
        {
            _enhancedScroller._Resize(true);
            
            if (_noTasksObj != null)
            {
                _noTasksObj.SetActive(ContextData.Tasks.Count == 0);
            }

            UpdateTimer();
        }
        
        private void OnScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            var scrolledToNextPage = _enhancedScroller.ScrollPosition >= _enhancedScroller.ScrollSize - _cellSize;

            if (!scrolledToNextPage)
            {
                return;
            }
            
            ContextData.RequestPage();
        }

        private IEnumerator TimerCoroutine()
        {
            while (gameObject.activeInHierarchy)
            {
                yield return new WaitForSeconds(TIMER_UPDATE_PERIOD);

                UpdateTimer();
            }
        }

        private void UpdateTimer()
        {
            if (_noTasksTimeText != null && ContextData != null)
            {
                _noTasksTimeText.text = ContextData.TimeToNewTasks.HasValue 
                    ? ContextData.TimeToNewTasks.Value.ToHoursMinutesString() 
                    : "---";
            }
        }
    }
}