using Bridge.Models.VideoServer;

namespace UIManaging.Common.Hashtags
{
    internal sealed class HashtagsListModel
    {
        public HashtagInfo[] Hashtags { get; }

        public HashtagsListModel(HashtagInfo[] hashtags)
        {
            Hashtags = hashtags;
        }
    }
}