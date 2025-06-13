namespace UIManaging.Common.Buttons
{
    public class SaveVideoButtonArgs
    {
        public SaveVideoButtonArgs(long videoId)
        {
            VideoId = videoId;
        }

        public long VideoId { get; }
    }
}