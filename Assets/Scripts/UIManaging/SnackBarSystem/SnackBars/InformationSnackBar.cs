using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class InformationSnackBar : SnackBar<InformationSnackBarConfiguration>
    {
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.Information;

        protected override void OnConfigure(InformationSnackBarConfiguration configuration)
        {
        }
    }
}