using System;
using Extensions.DateTime;

namespace UIManaging.Common
{
    public abstract class UserTimestampItemModel
    {
        public event Action MovingToProfileStart;
        public event Action MovingToProfileFinished;
        
        public DateTime TimeStamp { get; }
        public string TimeStampText => TimeStamp.ElapsedTimeText();
        public long GroupId { get;}
        
        private readonly DateTime _utcTimestamp;

        protected UserTimestampItemModel(long groupId, DateTime utcTimestamp)
        {
            GroupId = groupId;
            TimeStamp = utcTimestamp;
        }

        public void OnMovingToProfileStart()
        {
            MovingToProfileStart?.Invoke();
        }
        public void OnMovingToProfileFinished()
        {
            MovingToProfileFinished?.Invoke();
        }
    }
}
