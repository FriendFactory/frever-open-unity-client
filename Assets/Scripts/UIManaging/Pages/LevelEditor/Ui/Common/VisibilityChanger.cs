using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    internal abstract class VisibilityChanger<TState> : MonoBehaviour, IStateChangeEventsSource<TState> 
        where TState: Enum
    {
        private readonly List<IStateChangeListener<TState>> _listeners = new List<IStateChangeListener<TState>>();
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            StartListeningStateChanging(OnStateChanged);
        }

        private void OnDestroy()
        {
            StopListeningStateChanging(OnStateChanged);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void RegisterListener(IStateChangeListener<TState> listener)
        {
            if(_listeners.Contains(listener)) return;
            _listeners.Add(listener);
        }

        public void UnregisterListener(IStateChangeListener<TState> listener)
        {
            if(!_listeners.Contains(listener)) return;
            _listeners.Add(listener);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void StartListeningStateChanging(Action<TState> onStateChanged);
        protected abstract void StopListeningStateChanging(Action<TState> onStateChanged);
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnStateChanged(TState state)
        {
            foreach (var listener in _listeners)
            {
                listener.OnStateChanged(state);
            }
        }
    }
}