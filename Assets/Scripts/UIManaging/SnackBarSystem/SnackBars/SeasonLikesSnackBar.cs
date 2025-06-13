using System;
using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class SeasonLikesSnackBar : SnackBar<SeasonLikesSnackBarConfiguration>, INotificationSource
    {
        public event Action NotificationReceived;
        public long? NotificationId { get; private set; }
        public long? QuestId { get; private set; }
        public bool ShouldRead => true;
        
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.SeasonLikes;
        
        protected override void OnConfigure(SeasonLikesSnackBarConfiguration configuration)
        {
            NotificationId = configuration.NotificationId;
            QuestId = configuration.QuestId;
            NotificationReceived?.Invoke();
        }
    }
}