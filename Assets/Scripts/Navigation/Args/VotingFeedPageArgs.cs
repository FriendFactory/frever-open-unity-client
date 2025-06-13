using System;
using System.Collections.Generic;
using Navigation.Core;
using UIManaging.Pages.VotingFeed.Interfaces;

namespace Navigation.Args
{
    public class VotingFeedPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.VotingFeed;

        public List<BattleData> AllBattleData { get; set; }
        public long TaskId { get; set; }
        public string BattleName { get; set; }
        public Action MoveBack { get; set; }
        public Action MoveNext { get; set; }
    }
}