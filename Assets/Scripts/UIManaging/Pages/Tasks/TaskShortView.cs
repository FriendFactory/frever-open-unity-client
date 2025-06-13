using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    internal sealed class TaskShortView : ActiveTaskViewBase
    {
        [SerializeField] private TaskVideoThumbnail _taskVideoThumbnail;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _taskVideoThumbnail.Initialize(ContextData);
        }
    }
}