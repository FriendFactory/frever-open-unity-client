using UIManaging.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UIManaging.Pages.Tasks
{
    internal sealed class TaskView : ActiveTaskViewBase
    {
        [SerializeField] private ScrollConflictManager _scrollConflictManager;
        [SerializeField] private VideoList _videoListView;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _videoListView.Initialize(ContextData);
        }

        public override void SetParentScrollRect(ScrollRect scrollRect)
        {
            _scrollConflictManager.ParentScrollRect = scrollRect;
        }
    }
}