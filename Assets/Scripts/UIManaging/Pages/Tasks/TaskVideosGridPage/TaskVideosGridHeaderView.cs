using Abstract;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    public struct TaskVideosGridHeaderViewArgs
    {
        public readonly TaskDetailsHeaderArgs TaskDetailsHeaderArgs;
        public readonly bool TaskActive;

        public TaskVideosGridHeaderViewArgs(TaskDetailsHeaderArgs taskDetailsHeaderArgs, bool taskActive)
        {
            TaskDetailsHeaderArgs = taskDetailsHeaderArgs;
            TaskActive = taskActive;
        }
    }
    
    internal sealed class TaskVideosGridHeaderView : BaseContextDataView<TaskVideosGridHeaderViewArgs>
    {
        [SerializeField] private TaskDetailsHeader _header;
        [SerializeField] private GameObject _activeTaskBackground;
        [SerializeField] private GameObject _completedTaskBackground;
        [SerializeField] private GameObject _featuredObj;
        [SerializeField] private GameObject _placeholderObj;

        protected override void OnInitialized()
        {
            if (ContextData.TaskDetailsHeaderArgs == null)
            {
                _placeholderObj.SetActive(true);
                _header.SetActive(false);
                return;
            }
            
            _placeholderObj.SetActive(false);
            _header.SetActive(true);
            _header.Initialize(ContextData.TaskDetailsHeaderArgs);
            _activeTaskBackground.SetActive(ContextData.TaskActive);
            _featuredObj.SetActive(ContextData.TaskActive);
            _completedTaskBackground.SetActive(!ContextData.TaskActive);

            var layoutElement = GetComponent<LayoutElement>();
            layoutElement.minHeight = 0f;
            var rectTransform = (RectTransform)transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}