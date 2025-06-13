using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UIManaging.SnackBarSystem.SnackBars;

internal sealed class PurchaseSuccessSnackBar : SnackBar<PurchaseSuccessSnackBarConfiguration>
{
    public override SnackBarType Type => SnackBarType.PurchaseSuccess;

    protected override void OnConfigure(PurchaseSuccessSnackBarConfiguration configuration)
    {

    }
}
