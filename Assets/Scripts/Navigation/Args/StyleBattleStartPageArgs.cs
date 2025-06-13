using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;

namespace UIManaging.Pages.VotingFeed
{
    public class StyleBattleStartPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.StyleBattleStart;

        public TaskFullInfo Task { get; set; }
        public List<string> DressCodes { get; set; }
        public Action MoveBack { get; set; }
        public Action MoveNext { get; set; }
    }
}