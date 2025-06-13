using System;
using JetBrains.Annotations;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    [UsedImplicitly]
    public class UmaLevelEditorPanelModel
    {
        public bool IsPanelOpened { get; private set; }
        
        public event Action PanelOpened;
        public event Action PanelClosed;

        public void OnPanelOpened()
        {
            IsPanelOpened = true;
            
            PanelOpened?.Invoke();
        }

        public void OnPanelClosed()
        {
            IsPanelOpened = false;
            
            PanelClosed?.Invoke();
        }
    }
}