namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public class CreatorScoreModel
    {
        public int CreatorScore { get; }
        public int ScoreProgress { get; }
        public int CreatorScoreBadge { get; }

        public CreatorScoreModel(int creatorScoreBadge, int creatorScore, int scoreProgress)
        {
            CreatorScoreBadge = creatorScoreBadge;
            CreatorScore = creatorScore;
            ScoreProgress = scoreProgress;
        }
    }
}