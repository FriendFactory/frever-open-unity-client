using UnityEngine;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    internal sealed class PartOfTheTaskVideoAttribute: VideoAttribute
    {
        [SerializeField] private VideoPartOfTaskButton _videoPartOfTaskButton;

        protected override void OnBecomeVisible()
        {
            _videoPartOfTaskButton.Initialize(ContextData.Video.TaskId, ContextData.Video.TaskName);
        }
        
        protected override bool ShouldBeVisible() => !ContextData.Video.IsVotingTask && ContextData.Video.TaskId != 0 && ContextData.OpenedWithTask != ContextData.Video.TaskId;
    }
}