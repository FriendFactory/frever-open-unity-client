using Modules.InputHandling;
using UIManaging.SnackBarSystem.Configurations;
using Zenject;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class AssetLoadingSnackBar : SnackBar<AssetLoadingSnackBarConfiguration>
    {
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;
        
        //---------------------------------------------------------------------
        // SnackBar
        //---------------------------------------------------------------------
        public override SnackBarType Type => SnackBarType.AssetLoading;

        protected override void OnConfigure(AssetLoadingSnackBarConfiguration configuration)
        {
            _backButtonEventHandler.ProcessEvents(false);
        }

        public override void OnHidden()
        {
            base.OnHidden();
            
            _backButtonEventHandler.ProcessEvents(true);
        }

        protected override void OnSwipeUp() { }
    }
}