using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TipsManagment;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class CreatorScoreQuestRedirection: IQuestRedirection
    {
        private const int REQUIRED_SCORE = 100;
        
        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        [Inject] private LocalUserDataHolder _localUser;
        
        public string QuestType => QuestManaging.QuestType.REACH_CREATOR_SCORE;
        public void Redirect()
        {
            var args = new CreatorScorePageArgs()
            {
                ShowHintsOnDisplay = true
            };

            _pageManager.MoveNext(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                _tipManager.ShowTipsById(TipId.QuestCreatorScore);
            }
        }
    }
}