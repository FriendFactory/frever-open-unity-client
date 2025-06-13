using System;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    internal interface IStateChangeListener<in TState> where TState: Enum
    {
        void Initialize(IStateChangeEventsSource<TState> eventsSource);
        void OnStateChanged(TState state);
    }
}