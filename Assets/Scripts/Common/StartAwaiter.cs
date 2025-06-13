using System;
using UnityEngine;

namespace Common
{
    public class StartAwaiter: MonoBehaviour
    {
        public event Action OnStart;
        
        private void Start()
        {
            OnStart?.Invoke();
        }
    }
}