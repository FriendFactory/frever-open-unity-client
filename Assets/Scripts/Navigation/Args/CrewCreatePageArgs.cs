using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.Common.Files;
using Navigation.Core;

namespace Navigation.Args
{
    public class CrewCreatePageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.CrewCreate;

        public SaveCrewModel SaveCrewModel = new SaveCrewModel
        {
            Name = string.Empty,
            Description = string.Empty,
            IsPublic = false,
            Files = new List<FileInfo>()
        };
    }
}