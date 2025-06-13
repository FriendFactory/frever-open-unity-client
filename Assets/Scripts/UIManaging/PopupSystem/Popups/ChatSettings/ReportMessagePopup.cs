using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    public class ReportMessagePopup : BasePopup<ReportMessagePopupConfiguration>
    {
        [SerializeField] private ReportMessagePanel _reportPanel;

        public void OnEnable()
        {
            _reportPanel.HideActionRequested += Hide;
        }

        public void OnDisable()
        {
            _reportPanel.HideActionRequested -= Hide;
        }

        protected override void OnConfigure(ReportMessagePopupConfiguration configuration)
        {
            _reportPanel.Initialize(new ReportMessageModel(configuration.ChatId, configuration.MessageId));
        }

        protected override void OnHidden()
        {
            _reportPanel.CleanUp();
            
            base.OnHidden();
        }
    }
}