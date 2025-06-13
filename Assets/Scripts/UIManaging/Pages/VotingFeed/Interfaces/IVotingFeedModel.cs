namespace UIManaging.Pages.VotingFeed.Interfaces
{
    public interface IVotingFeedModel
    {
        int MaxIterations { get; }
        int CurrentIteration { get; }
        BattleData CurrentBattleData { get; }

        void StartNextVote();
        void VoteForVideo(long videoId);
        void FinishVoting();
    }
}