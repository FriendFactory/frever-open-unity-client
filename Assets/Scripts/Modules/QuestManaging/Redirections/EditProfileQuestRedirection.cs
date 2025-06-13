using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TipsManagment;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class EditProfileQuestRedirection: IQuestRedirection
    {
        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        
        public string QuestType => QuestManaging.QuestType.WRITE_BIO;
        
        public void Redirect()
        {
            var args = new UserProfileArgs();
            
            _pageManager.PageDisplayed += OnPageDisplayed;
            _pageManager.MoveNext(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                _tipManager.ShowTipsById(TipId.QuestEditProfile);
            }
        }
    }
}