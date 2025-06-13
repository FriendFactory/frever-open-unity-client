using Modules.QuestManaging;
using UIManaging.Pages.Common.VideoManagement;
using Zenject;

namespace UIManaging.Pages.Feed.Core.Metrics
{
    public class LikeMetricsView : ToggleMetricsView
    {
        [Inject] private VideoManager _videoManager;
        [Inject] private IQuestManager _questManager;

        protected override void OnToggleSet()
        {
            _videoManager.LikeVideo(ContextData.EntityId, ContextData.GroupId, true, OnLikeSuccess);
        }

        protected override void OnToggleUnset()
        {
            _videoManager.LikeVideo(ContextData.EntityId, ContextData.GroupId, false, OnDislikeSuccess);
        }

        private void OnLikeSuccess()
        {
            _questManager.ShowQuestLikesSnackBar();
            base.OnToggleSet();
        }

        private void OnDislikeSuccess()
        {
            base.OnToggleUnset();
        }
    }
}