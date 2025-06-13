using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public sealed class TaskListLimitedView : TaskListView
    {
        private const int MAX_VIDEOS_AMOUNT = 5;

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.Min(base.GetNumberOfCells(scroller), MAX_VIDEOS_AMOUNT);
        }
    }
}