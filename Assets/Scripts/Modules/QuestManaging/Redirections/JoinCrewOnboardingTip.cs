using Navigation.Args;
using Navigation.Core;
using TipsManagment;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    public class JoinCrewOnboardingTip : OnboardingTip
    {
        [Inject] private PageManager _pageManager;
        
        public override void CompleteTip()
        {
            var args = new CrewSearchPageArgs();
            _pageManager.MoveNext(args);
            
            base.CompleteTip();
        }
    }
}