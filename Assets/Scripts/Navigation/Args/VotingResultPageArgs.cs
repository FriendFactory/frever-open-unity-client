using Bridge.Models.ClientServer.Battles;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class VotingResultPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.VotingResult;
        public readonly long TaskId;
        public readonly string TaskName;
        public readonly BattleResult[] BattleResults;

        public VotingResultPageArgs(long taskId, string taskName, BattleResult[] battleResults): this(taskId, taskName)
        {
            BattleResults = battleResults;
        }
        
        public VotingResultPageArgs(long taskId, string taskName)
        {
            TaskId = taskId;
            TaskName = taskName;
        }
    }
}