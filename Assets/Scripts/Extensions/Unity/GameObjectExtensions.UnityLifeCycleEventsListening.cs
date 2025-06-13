using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Extensions
{
    public static partial class GameObjectExtensions
    {
        public static void AddListenerToDestroyEvent(this GameObject gameObject, Action callback)
        {    
            var eventSourceComponent = gameObject.AddOrGetComponent<GameObjectUnityEventsListener>();
            eventSourceComponent.AddListenerToDestroyEvent(callback);
        }
        
        public static void RemoveListenerFromDestroyEvent(this GameObject gameObject, Action callback)
        {
            if (!ValidateTarget(gameObject, out var eventSourceComponent)) return;
            eventSourceComponent.RemoveListenerToDestroyEvent(callback);
        }

        public static void AddListenerToEnabledEvent(this GameObject gameObject, Action callback)
        {
            var eventSourceComponent = gameObject.AddOrGetComponent<GameObjectUnityEventsListener>();
            eventSourceComponent.GameObjectEnabled += callback;
        }

        public static void RemoveListenerFromEnabledEvent(this GameObject gameObject, Action callback)
        {
            if (!ValidateTarget(gameObject, out var eventSourceComponent)) return;
            eventSourceComponent.GameObjectEnabled -= callback;
        }

        public static void AddListenerToOnAnimatorMove(this GameObject gameObject, Action onAnimatorMove)
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            
            var eventSourceComponent = gameObject.AddOrGetComponent<GameObjectUnityEventsListener>();
            eventSourceComponent.AddListenerToOnAnimatorMove(onAnimatorMove);
        }
        
        public static void RemoveListenerFromOnAnimatorMove(this GameObject gameObject, Action onAnimatorMove)
        {
            if (!ValidateTarget(gameObject, out var eventSourceComponent)) return;
            eventSourceComponent.RemoveListenerFromOnAnimatorMove(onAnimatorMove);
        }

        public static void AddListenerToOnGameObjectMovedToAnotherScene(this GameObject gameObject, Action<Scene> onChangedScene)
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            
            var eventSourceComponent = gameObject.AddOrGetComponent<GameObjectUnityEventsListener>();
            eventSourceComponent.AddListenerToOnMovedToAnotherScene(onChangedScene);
        }
        
        public static void RemoveListenerFromOnGameObjectMovedToAnotherScene(this GameObject gameObject, Action<Scene> onChangedScene)
        {
            if (!ValidateTarget(gameObject, out var eventSourceComponent)) return;
            eventSourceComponent.RemoveListenerFromOnMovedToAnotherScene(onChangedScene);
        }
        
        public static void AddListenerToOnPointerUp(this GameObject gameObject, Action<PointerEventData> callback)
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            
            var eventSourceComponent = gameObject.AddOrGetComponent<GameObjectUnityEventsListener>();
            eventSourceComponent.AddListenerToOnPointerUp(callback);
        }
        
        public static void RemoveListenerFromOnPointerUp(this GameObject gameObject, Action<PointerEventData> callback)
        {
            if (!ValidateTarget(gameObject, out var eventSourceComponent)) return;
            eventSourceComponent.RemoveListenerToOnPointerUp(callback);
        }
        
        public static void AddListenerToOnPointerDown(this GameObject gameObject, Action<PointerEventData> callback)
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
            
            var eventSourceComponent = gameObject.AddOrGetComponent<GameObjectUnityEventsListener>();
            eventSourceComponent.AddListenerToOnPointerDown(callback);
        }
        
        public static void RemoveListenerFromOnPointerDown(this GameObject gameObject, Action<PointerEventData> callback)
        {
            if (!ValidateTarget(gameObject, out var eventSourceComponent)) return;
            eventSourceComponent.RemoveListenerToOnPointerDown(callback);
        }
        
        private static bool ValidateTarget(GameObject gameObject, out GameObjectUnityEventsListener eventSourceComponent)
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

            eventSourceComponent = gameObject.GetComponent<GameObjectUnityEventsListener>();
            if (eventSourceComponent != null) return true;
            Debug.LogWarning($"No listeners added to {gameObject.name}");
            return false;
        }
    }
}