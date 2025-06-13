using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class RaceSelectionPageArs: PageArgs
    {
        public Action<Race> RaceSelected;
        public Action MoveBackRequested;
        public override PageId TargetPage => PageId.RaceSelection;
    }
}