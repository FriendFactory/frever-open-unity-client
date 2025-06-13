using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class EditTemplatePopupConfiguration: PopupConfiguration
    {
        public string TemplateName { get; }
        public Action<bool, string> NameChanged { get; }
        public Action BackButton { get; }

        public EditTemplatePopupConfiguration(string templateName, Action<bool, string> nameChanged, Action backButton) : base(PopupType.EditTemplate, null)
        {
            TemplateName = templateName;
            NameChanged = nameChanged;
            BackButton = backButton;
        }
    }
}