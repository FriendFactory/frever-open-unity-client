using System;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    internal interface IStateChangeEventsSource<out TState> where TState: Enum
    {
        void RegisterListener(IStateChangeListener<TState> listener);
        void UnregisterListener(IStateChangeListener<TState> listener);
    }
}