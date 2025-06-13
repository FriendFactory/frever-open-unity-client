using Navigation.Core;

namespace Navigation.Args
{
    public class CreatePostPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.CreatePost;
        
        public object GalleryState { get; set; }
    }
}