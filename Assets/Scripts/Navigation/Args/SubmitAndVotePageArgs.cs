using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;

namespace UIManaging.Pages.VotingFeed
{
    public class SubmitAndVotePageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.SubmitAndVote;

        public Action MoveBack { get; set; }
        public Action MoveNext { get; set; }
    }
}