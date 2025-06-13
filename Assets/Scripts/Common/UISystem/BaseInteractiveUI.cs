using System;
using UnityEngine;

namespace Common.UI
{
    public abstract class BaseInteractiveUI : MonoBehaviour
    {
        public event Action Interacted;

        protected virtual void OnUIInteracted()
        {
            Interacted?.Invoke();
        }
    }
}