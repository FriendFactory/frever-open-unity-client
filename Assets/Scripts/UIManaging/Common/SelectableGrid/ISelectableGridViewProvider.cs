using System;

namespace UIManaging.Common.SelectableGrid
{
    public interface ISelectableGridViewProvider
    {
        long Id { get; }
        event Action OnIdChangedEvent;
    }
}