using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class InformationDarkSnackBar : SnackBar<InformationDarkSnackBarConfiguration>
    {
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.InformationDark;

        protected override void OnConfigure(InformationDarkSnackBarConfiguration configuration)
        {
        }
    }
}