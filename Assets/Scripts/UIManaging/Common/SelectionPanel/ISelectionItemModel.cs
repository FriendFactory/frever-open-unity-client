using System;

namespace UIManaging.Common.SelectionPanel
{
    public interface ISelectionItemModel
    {
        long Id { get; }
        bool IsSelected { get; set; }
        bool IsLocked { get; set; }

        event Action SelectionChanged;
        event Action SelectionChangeLocked;
    }
}