using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.Common.Files;
using Models;
using Navigation.Args;
using Navigation.Core;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingDonePageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.VotingDone;

        public TaskFullInfo Task { get; set; }
        public List<string> DressCodes { get; set; }
        public Level CreatedLevel { get; set; }
        public long MainCharacterId { get; set; }
        public List<FileInfo> MainCharacterThumbnail { get; set; }
        public string UserNickname { get; set; }
        public Action PublishStart { get; set; }
        public Action MoveBack { get; set; }
    }
}