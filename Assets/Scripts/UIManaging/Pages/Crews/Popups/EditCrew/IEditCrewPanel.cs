using System;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    internal interface IEditCrewPanel
    {
        Action RequestBackAction { get; set; }
        Action RequestCloseAction { get; set; }
        
        void Show();
        void Hide();
        void OnCloseButtonClicked();
    }
}