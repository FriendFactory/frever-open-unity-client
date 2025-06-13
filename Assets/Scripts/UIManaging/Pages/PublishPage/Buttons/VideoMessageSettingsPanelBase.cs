using UIManaging.Common.InputFields;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal abstract class VideoMessageSettingsPanelBase : VideoSettingsPanelBase
    {
        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _messageInputField;

        public string Message
        {
            get => _messageInputField.Text;
            protected set => _messageInputField.Text = value;
        }

        private bool _isInitialised;

        protected override void Init()
        {
            if (_isInitialised) return;
            
            base.Init();
            _messageInputField.OnSelectionChanged.AddListener(isSelected =>
            {
                if (isSelected)
                {
                    OnInputFieldSelected();
                }
                else
                {
                    OnInputFieldDeselected();
                }
            });
            _isInitialised = true;
        }
        
        public void OpenSendDestinationSelection()
        {
            SendDestinationSelectionButton.OpenSelectionPopup();
        }
    }
}