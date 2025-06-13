using System;
using Models;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class VideoMessagePageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.VideoMessage;
        public Level Level;
        public Action OnMoveBackRequested;
        public Action<Level> OnMoveNext;
        public Action OnLevelCreationRequested;
        public Action<NonLeveVideoData> OnNonLevelVideoUploadRequested;
    }
}
