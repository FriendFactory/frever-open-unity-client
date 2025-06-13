using JetBrains.Annotations;
using Navigation.Args.Feed;
using Navigation.Core;
using TipsManagment;
using UIManaging.Pages.Common.VideoManagement;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
	[UsedImplicitly]
	public class FeedSwipeQuestRedirection: IQuestRedirection
	{
		[Inject] private PageManager _pageManager;
		[Inject] private TipManager _tipManager;
		[Inject] private VideoManager _videoManager;
        
		public string QuestType => Modules.QuestManaging.QuestType.FEED_SWIPE;
		public void Redirect()
		{
			if (_pageManager.IsCurrentPage(PageId.Feed))
			{
				OnPageDisplayed();
				return;
			}
			var args = new GeneralFeedArgs(_videoManager);

			_pageManager.PageDisplayed += OnPageDisplayed;
			_pageManager.MoveNext(args);

			void OnPageDisplayed(PageData pageData = new())
			{
				_pageManager.PageDisplayed -= OnPageDisplayed;
				_tipManager.ShowTipsById(TipId.OnboardingFeedSwipe);
			}
		}
	}
}