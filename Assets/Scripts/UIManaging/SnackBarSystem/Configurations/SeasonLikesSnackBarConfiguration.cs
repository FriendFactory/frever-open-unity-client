namespace UIManaging.SnackBarSystem.Configurations
{
    public sealed class SeasonLikesSnackBarConfiguration : SnackBarConfiguration
    {
        internal override SnackBarType Type => SnackBarType.SeasonLikes;

        public long? NotificationId;
        public long? QuestId;
    }
}