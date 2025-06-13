using System;
using Common.Publishers;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class PublishGalleryVideoPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.PublishGalleryVideoPage;
        public NonLeveVideoData VideoData;
        public PublishingType PublishingType;
        public Action OnMoveBack;
        public Action<VideoUploadingSettings> OnMoveForward;
        public ShareDestination ShareDestination;
    }
}
