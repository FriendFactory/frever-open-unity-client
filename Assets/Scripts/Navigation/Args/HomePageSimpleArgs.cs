using System;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;
using UIManaging.Rewards.Models;

namespace Navigation.Args
{
    public class HomePageSimpleArgs: PageArgs
    {
        public TaskFullInfo OpenedWithTask { get; set; }
        public Action RewardEnd { get; set; }
        public bool EnableInputBlocker { get; set; } = false;
        public InvitationAcceptedRewardModel RewardModel { get; set; }

        public override PageId TargetPage => PageId.HomePageSimple;
    }
}