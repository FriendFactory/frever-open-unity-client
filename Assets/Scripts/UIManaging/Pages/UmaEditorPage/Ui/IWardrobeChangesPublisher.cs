using System;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public interface IWardrobeChangesPublisher
    {
        public event Action WardrobeStartChanging;
        public event Action WardrobeChanged;
    }
}