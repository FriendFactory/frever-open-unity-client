using System;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class PreviewLevelButton : PublishPageButtonBase
    {
        private Action _onClick;
        
        public void SetCallback(Action executePreviewAction)
        {
            _onClick = executePreviewAction;
        }

        protected override void OnButtonClicked()
        {
            _onClick?.Invoke();
        }
    }
}
