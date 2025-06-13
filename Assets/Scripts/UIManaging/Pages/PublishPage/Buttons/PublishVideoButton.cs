using System;
using Modules.Amplitude;
using Zenject;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class PublishVideoButton : PublishPageButtonBase
    {
        [Inject] private AmplitudeManager _amplitudeManager;
        
        public event Action PublishVideoRequested;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnButtonClicked()
        {
            if (IsActionAllowed())
            {
                _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.PUBLISH_BUTTON_PRESSED);
                PublishVideoRequested?.Invoke();
            }
        }
    }
}