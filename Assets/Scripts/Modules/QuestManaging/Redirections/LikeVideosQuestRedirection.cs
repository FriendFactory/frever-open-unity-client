using JetBrains.Annotations;
using Navigation.Args.Feed;
using Navigation.Core;
using TipsManagment;
using UIManaging.Pages.Common.VideoManagement;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class LikeVideosQuestRedirection: IQuestRedirection
    {
        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        [Inject] private VideoManager _videoManager;
        
        public string QuestType => QuestManaging.QuestType.LIKE_VIDEO;
        public void Redirect()
        {
            var args = new GeneralFeedArgs(_videoManager);
            
            _pageManager.MoveNext(args);
        }
    }
}