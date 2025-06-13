using JetBrains.Annotations;
using Navigation.Args.Feed;
using Navigation.Core;
using TipsManagment;
using UIManaging.Pages.Common.VideoManagement;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class RemixVideoQuestRedirection: IQuestRedirection
    {
        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        [Inject] private VideoManager _videoManager;
        
        public string QuestType => QuestManaging.QuestType.REMIX_VIDEO;
        public void Redirect()
        {
            var args = new GeneralFeedArgs(_videoManager)
            {
                FindVideoWithRemix = true
            };

            _pageManager.PageDisplayed += OnPageDisplayed;
            _pageManager.MoveNext(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                _tipManager.ShowTipsById(TipId.QuestRemixVideo);
            }
        }
    }
}