using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Common
{
    [UsedImplicitly]
    public class StartAwaiterHelper
    {
        public void AwaitStart(Action onStart, Transform container)
        {
            var obj = new GameObject("StartAwaiter");
            obj.transform.SetParent(container);
            
            var awaiter = obj.AddComponent<StartAwaiter>();
            awaiter.OnStart += OnStart;
            
            void OnStart()
            {
                awaiter.OnStart -= OnStart;
                onStart?.Invoke();
                UnityEngine.Object.Destroy(obj);
            }
        }
    }
}