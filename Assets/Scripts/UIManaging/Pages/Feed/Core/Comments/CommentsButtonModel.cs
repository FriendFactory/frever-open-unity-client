using System;
using UIManaging.Pages.Feed.GamifiedFeed;
using UnityEngine.Events;

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class CommentsButtonModel: IVideoKPICount
    {
        public UnityAction OnClick { get; }
        
        public long Count
        {
            get => _value;
            set
            {
                _value = value;
                
                Changed?.Invoke(value);
            }
        }

        public bool IsOwner { get; }

        public event Action<long> Changed;
        
        private long _value;

        public CommentsButtonModel(UnityAction onClick, bool isOwner)
        {
            OnClick = onClick;
            IsOwner = isOwner;
        }
    }
}