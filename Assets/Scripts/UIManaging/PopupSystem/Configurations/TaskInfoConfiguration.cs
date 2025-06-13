using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class TaskInfoConfiguration: PopupConfiguration
    {
        public string Description { get; set; }

        public TaskInfoConfiguration() : base(PopupType.TaskInfo, null, null)
        {
        }

        public TaskInfoConfiguration(Action<object> onClose, string title = "") : base(PopupType.TaskInfo, onClose, title)
        {
        }
    }
}