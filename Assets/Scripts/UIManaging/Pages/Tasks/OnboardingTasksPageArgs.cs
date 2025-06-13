using System;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;

namespace UIManaging.Pages.Tasks
{
    public class OnboardingTasksPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.OnboardingTasksPage;

        public TaskFullInfo OpenedWithTask { get; set; }
        public Action<long, long> MoveNext { get; }
        public Action RewardCompleted { get; }
        public bool VideoUploaded { get; }

        public bool ClaimedFirstReward { get; }

        public OnboardingTasksPageArgs(Action<long, long> moveNext, bool claimedFirstReward, Action rewardCompleted, TaskFullInfo openedWithTask, bool videoUploaded)
        {
            MoveNext = moveNext;
            RewardCompleted = rewardCompleted;
            OpenedWithTask = openedWithTask;
            ClaimedFirstReward = claimedFirstReward;
            VideoUploaded = videoUploaded;
        }
    }
}