using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TipsManagment;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class ClaimSeasonLikesQuestRedirection: IQuestRedirection
    {
        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        
        public string QuestType => Modules.QuestManaging.QuestType.CLAIM_SEASON_QUEST_REWARD;
        
        public void Redirect()
        {
            var args = new SeasonPageArgs(SeasonPageArgs.Tab.Rewards);

            _pageManager.PageDisplayed += OnPageDisplayed;
            _pageManager.MoveNext(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                _tipManager.ShowTipsById(TipId.QuestClaimSeasonLikes);
            }
        }
    }
}