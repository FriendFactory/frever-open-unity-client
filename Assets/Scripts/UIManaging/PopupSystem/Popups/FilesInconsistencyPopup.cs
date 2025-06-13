using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class FilesInconsistencyPopup : AlertPopup
    {
        [SerializeField] private Button _closeButton;

        protected override void OnConfigure(AlertPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            _closeButton.onClick.AddListener(Hide);
        }

        public override void Hide()
        {
            base.Hide();
            _closeButton.onClick.RemoveListener(Hide);
        }
    }
}
