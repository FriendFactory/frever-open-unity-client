using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TipsManagment;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class ClaimSeasonRewardsQuestRedirection: IQuestRedirection
    {
        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        
        public string QuestType => QuestManaging.QuestType.CLAIM_LEVEL_UP_REWARD;
        
        public void Redirect()
        {
            var args = new HomePageArgs
            {
                ShowHomePagePopups = false
            };
            
            _pageManager.PageDisplayed += OnPageDisplayed;
            _pageManager.MoveNext(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                _tipManager.ShowTipsById(TipId.QuestClaimSeasonRewards);
            }
        }
    }
}