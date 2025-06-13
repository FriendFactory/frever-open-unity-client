using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TipsManagment;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    public class JoinCrewRedirection : IQuestRedirection
    {
        public string QuestType => QuestManaging.QuestType.JOIN_CREW;

        [Inject] private PageManager _pageManager;
        [Inject] private TipManager _tipManager;
        
        public void Redirect()
        {
            _tipManager.ShowTipsById(TipId.QuestJoinCrew);
        }
    }
}