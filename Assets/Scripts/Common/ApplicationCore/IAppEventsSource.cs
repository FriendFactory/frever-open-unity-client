using System;
using UnityEngine;

namespace Common.ApplicationCore
{
    /// <summary>
    /// The single place to share application lifecycle events, especially to non-MonoBehaviour types,
    /// which lucks ability using OnApplicationQuit, OnApplicationFocused cla
    /// </summary>
    public interface IAppEventsSource
    {
        event Action ApplicationQuit;
        event Action<bool> ApplicationFocused;
        event Action GUI;
    }

    internal sealed class AppEventsSource: MonoBehaviour, IAppEventsSource
    {
        public event Action ApplicationQuit;
        public event Action<bool> ApplicationFocused;
        public event Action GUI;
        
        private void OnApplicationFocus(bool hasFocus)
        {
            ApplicationFocused?.Invoke(hasFocus);
        }
        
        private void OnApplicationQuit()
        {
            ApplicationQuit?.Invoke();
        }

        private void OnGUI()
        {
            GUI?.Invoke();
        }
    }
}