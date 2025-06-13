using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class InformationShortSnackBar : SnackBar<InformationShortSnackBarConfiguration>
    {
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.InformationShort;

        protected override void OnConfigure(InformationShortSnackBarConfiguration configuration)
        {
        }
    }
}