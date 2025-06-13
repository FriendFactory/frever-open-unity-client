using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.VotingFeed.Transitions
{
    [UsedImplicitly]
    internal class VotingFeedToVotingDoneTransition: TransitionBase<VotingFeedContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.VotingDone;

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            foreach (var battleData in Context.AllBattleData.SelectMany(battle => 
                    battle.BattleVideos.Select(battleVideo => battleVideo.VideoModel)))
            {
                battleData.CleanUp();
            }
            
            return base.OnRunning();
        }
    }
}