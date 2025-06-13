using System;
using UnityEngine;

namespace Modules.InputHandling
{
    public interface IBackButtonEventHandler
    {
        void AddButton(GameObject gameObject, Action callback, bool persistent = false);
        void RemoveButton(GameObject gameObject);
        void ProcessEvents(bool state);
    }
}