using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    /// <summary>
    /// Initialize active and inactive game objects on the scene
    /// </summary>
    internal abstract class SceneStateListenersInitializer<TState> : MonoBehaviour
        where TState: Enum
    {
        [Inject] private IStateChangeEventsSource<TState> _stateChangeEventsSource;
        [SerializeField] private List<MonoBehaviour> _sceneStateListeners;
        
        public void Initialize()
        {
            foreach (var listener in _sceneStateListeners.Cast<StateListenerBase<TState>>())
            {
                listener.Initialize(_stateChangeEventsSource);
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            FindAllListenersInTheScene();
        }
        
        [ContextMenu("FindAllListenersInTheScene")]
        private void FindAllListenersInTheScene()
        {
            if (_sceneStateListeners == null)
            {
                _sceneStateListeners = new List<MonoBehaviour>();
            }
            _sceneStateListeners.Clear();
            
            var root = gameObject.transform.root;
            foreach (Transform child in root)
            {
                var listeners = child.GetComponentsInChildren<StateListenerBase<TState>>(true);
                _sceneStateListeners.AddRange(listeners);
            }
        }
        #endif
    }
}