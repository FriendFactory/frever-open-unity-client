using System;

namespace UIManaging.Pages.Feed.GamifiedFeed
{
    public interface IVideoKPICount
    {
        public long Count { get; }
        public bool IsOwner { get; }

        public event Action<long> Changed;
    }
}