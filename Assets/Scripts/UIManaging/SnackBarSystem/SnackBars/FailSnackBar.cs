using UIManaging.SnackBarSystem.Configurations;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal class FailSnackBar: SnackBar<FailSnackBarConfiguration>
    {
        public override SnackBarType Type => SnackBarType.Fail;
        
        protected override void OnConfigure(FailSnackBarConfiguration configuration) { }
    }
}