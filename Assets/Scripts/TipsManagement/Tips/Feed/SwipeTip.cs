using System.Threading.Tasks;
using UIManaging.Pages.Feed.Core;
using UnityEngine;

namespace TipsManagment
{
    internal class SwipeTip : TextTip
    {
        private FeedScrollView _feedScroll;

        protected override void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                CheckSwipeDone();
            }
        }

        public override async Task Activate()
        {
            await base.Activate();
            if (TargetTransform is null) return; // Check if target destoroyed (time sensitive)
            _feedScroll = TargetTransform.GetComponent<FeedScrollView>();
            if (_feedScroll is null) return;
            _feedScroll.OnViewScrolledDownEvent += OnSwipeUp;

            StartTip();
        }

        private void CheckSwipeDone()
        {
            if (TaskSource.Task.Status == TaskStatus.Running || TaskSource.Task.Status == TaskStatus.WaitingForActivation)
            {
                TaskSource.SetCanceled();
                _feedScroll.OnViewScrolledDownEvent -= OnSwipeUp;
            }
        }

        private void OnSwipeUp()
        {
            _feedScroll.OnViewScrolledDownEvent -= OnSwipeUp;
            TaskSource.TrySetResult(true);
        }

        protected override void PositionateTip(){}

        protected override bool TargetOnScreen()
        {
            return true;
        }
    }
}