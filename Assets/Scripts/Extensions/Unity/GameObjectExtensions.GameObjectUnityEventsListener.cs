using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Extensions
{
    public static partial class GameObjectExtensions
    {
        /// <summary>
        ///   Listen to Unity game object life cycle events.
        ///   For now Destroy event is only handled, but we can extend it in future on demand
        /// </summary>
        private sealed class GameObjectUnityEventsListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            public event Action GameObjectEnabled;
            
            private List<Action> _destroyCallbacks;
            private List<Action> _onAnimatorMoveCallbacks;
            private List<Action<Scene>> _onMovedToAnotherSceneCallbacks;
            private List<Action<PointerEventData>> _onPointerDownCallbacks;
            private List<Action<PointerEventData>> _onPointerUpCallbacks;

            private Scene _currentScene;

            //---------------------------------------------------------------------
            // Public
            //---------------------------------------------------------------------
            
            public void AddListenerToDestroyEvent(Action callback)
            {
                AddCallback(ref _destroyCallbacks, callback);
            }

            public void RemoveListenerToDestroyEvent(Action callback)
            {
                RemoveCallback(_destroyCallbacks, callback);
            }

            public void AddListenerToOnAnimatorMove(Action callback)
            {
                AddCallback(ref _onAnimatorMoveCallbacks, callback);
            }

            public void RemoveListenerFromOnAnimatorMove(Action callback)
            {
                RemoveCallback(_onAnimatorMoveCallbacks, callback);
            }

            public void AddListenerToOnMovedToAnotherScene(Action<Scene> callback)
            {
                AddCallback(ref _onMovedToAnotherSceneCallbacks, callback);
            }

            public void RemoveListenerFromOnMovedToAnotherScene(Action<Scene> callback)
            {
                RemoveCallback(_onMovedToAnotherSceneCallbacks, callback);
            }

            public void AddListenerToOnPointerUp(Action<PointerEventData> callback)
            {
                AddCallback(ref _onPointerUpCallbacks, callback);
            }
            
            public void RemoveListenerToOnPointerUp(Action<PointerEventData> callback)
            {
                RemoveCallback(_onPointerUpCallbacks, callback);
            }
            
            public void AddListenerToOnPointerDown(Action<PointerEventData> callback)
            {
                AddCallback(ref _onPointerDownCallbacks, callback);
            }
            
            public void RemoveListenerToOnPointerDown(Action<PointerEventData> callback)
            {
                RemoveCallback(_onPointerDownCallbacks, callback);
            }

            //---------------------------------------------------------------------
            // Messages
            //---------------------------------------------------------------------
            
            private void Awake()
            {
                _currentScene = gameObject.scene;
            }
            
            private void OnEnable()
            {
                GameObjectEnabled?.Invoke();
            }

            private void OnAnimatorMove()
            {
                if (_onAnimatorMoveCallbacks == null) return;
                for (var i = 0; i < _onAnimatorMoveCallbacks.Count; i++)
                {
                    _onAnimatorMoveCallbacks[i]?.Invoke();
                }
            }

            private void OnTransformParentChanged()
            {
                if (_currentScene == gameObject.scene) return;
                _currentScene = gameObject.scene;
                if (_onMovedToAnotherSceneCallbacks == null) return;
                foreach (var callback in _onMovedToAnotherSceneCallbacks)
                {
                    callback?.Invoke(_currentScene);
                }
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_onPointerDownCallbacks == null) return;

                foreach (var callback in _onPointerDownCallbacks)
                {
                    callback?.Invoke(eventData);
                }
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                if (_onPointerUpCallbacks == null) return;

                foreach (var callback in _onPointerUpCallbacks)
                {
                    callback?.Invoke(eventData);
                }
            }
            
            private void OnDestroy()
            {
                if (_destroyCallbacks == null) return;

                for (var i = _destroyCallbacks.Count - 1; i >= 0; i--)
                {
                    var callback = _destroyCallbacks[i];
                    callback?.Invoke();
                }
            }

            //---------------------------------------------------------------------
            // Helpers
            //---------------------------------------------------------------------

            private static void AddCallback<T>(ref List<T> callbacksCollection, T callback)
            {
                if (callbacksCollection == null)
                {
                    callbacksCollection = new List<T>();
                }
                if (callbacksCollection.Contains(callback)) return;
                callbacksCollection.Add(callback);
            }

            private static void RemoveCallback<T>(ICollection<T> callbacksCollection, T callback)
            {
                if (callbacksCollection == null || !callbacksCollection.Contains(callback)) return;
                callbacksCollection.Remove(callback);
            }
        }
    }
}