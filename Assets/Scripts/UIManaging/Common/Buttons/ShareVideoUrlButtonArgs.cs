namespace UIManaging.Common.Buttons
{
    public class ShareVideoUrlButtonArgs
    {
        public long VideoId { get; }

        public ShareVideoUrlButtonArgs(long videoId)
        {
            VideoId = videoId;
        }
    }
}