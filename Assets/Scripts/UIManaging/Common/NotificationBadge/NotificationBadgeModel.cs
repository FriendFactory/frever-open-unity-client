using System;

namespace UIManaging.Pages.Common
{
    public class NotificationBadgeModel
    {
        public int NotificationCount
        {
            get => _notificationCount;
            set
            {
                _notificationCount = value;
                
                NotificationCountChanged?.Invoke(_notificationCount);
            }
        }

        public event Action<int> NotificationCountChanged;

        private int _notificationCount;
        
        public NotificationBadgeModel(int notificationCount)
        {
            NotificationCount = notificationCount;
        }
    }
}