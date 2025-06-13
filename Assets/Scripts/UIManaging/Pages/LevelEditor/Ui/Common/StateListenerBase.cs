using System;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    internal abstract class StateListenerBase<TState> : MonoBehaviour, IStateChangeListener<TState>
        where TState: Enum
    {
        protected IStateChangeEventsSource<TState> EventsSource;
        
        public void Initialize(IStateChangeEventsSource<TState> eventsSource)
        {
            EventsSource = eventsSource;
            OnInitialize();
        }

        protected abstract void OnInitialize();

        public abstract void OnStateChanged(TState state);

        protected void StartListenToStateChanging()
        {
            EventsSource.RegisterListener(this);
        }

        protected void StopListenToStateChanging()
        {
            EventsSource.UnregisterListener(this);
        }
        
        private void OnDestroy()
        {
            StopListenToStateChanging();
        }
    }
}