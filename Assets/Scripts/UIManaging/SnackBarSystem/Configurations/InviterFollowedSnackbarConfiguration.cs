using System;

namespace UIManaging.SnackBarSystem.Configurations
{
    public sealed class InviterFollowedSnackbarConfiguration : SnackBarConfiguration
    {
        private const string TITLE = "You are now following";

        internal override SnackBarType Type => SnackBarType.InviterFollowed;
        public readonly string Nickname;
        public readonly Action OnClick;
        
        public InviterFollowedSnackbarConfiguration(string nickname, Action onClick)
        {
            Title = TITLE;
            Nickname = nickname;
            OnClick = onClick;
        }
    }
}