using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class MessagesLockedSnackBar : SnackBar<MessagesLockedSnackBarConfiguration>
    {
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.MessagesLocked;

        protected override void OnConfigure(MessagesLockedSnackBarConfiguration configuration)
        {
        }
    }
}