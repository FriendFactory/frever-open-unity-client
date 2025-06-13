using UnityEngine;

namespace UIManaging.Pages.RatingFeed.Signals
{
    internal sealed class ScoreAnimationFinishedSignal
    {
        public int Score { get; }
        public Vector3 TargetPosition { get;  }

        public ScoreAnimationFinishedSignal(int score, Vector3 targetPosition)
        {
            Score = score;
            TargetPosition = targetPosition;
        }
    }
}