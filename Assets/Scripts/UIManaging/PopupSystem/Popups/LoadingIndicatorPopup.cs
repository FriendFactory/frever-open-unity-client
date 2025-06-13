using Navigation.Core;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal class LoadingIndicatorPopup : BasePopup<LoadingIndicatorPopupConfiguration>
    {
        [Inject] private PageManager _pageManager;

        private void OnDisable()
        {
            if (_pageManager is null) return;
            
            _pageManager.PageDisplayed -= OnPageChange;
            Configs?.ClearEvents();
        }

        protected override void OnConfigure(LoadingIndicatorPopupConfiguration configuration)
        {
            configuration.OnHideRequested += Hide;
            _pageManager.PageDisplayed += OnPageChange;
        }

        private void OnPageChange(PageData pageData)
        {
            gameObject.SetActive(false);
            Hide();
        }
    }
}