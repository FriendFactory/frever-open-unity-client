using System;
using UnityEngine;

namespace Extensions
{
    public static partial class GameObjectExtensions
    {
        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

            var component = gameObject.GetComponent<T>();

            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
        
        public static void AddComponentIfMissed<T>(this GameObject gameObject) where T : Component
        {
            gameObject.AddOrGetComponent<T>();
        }

        public static void SetParent(this GameObject gameObject, Transform parent)
        {
            if (gameObject == null) throw new ArgumentNullException();
            
            gameObject.transform.SetParent(parent);
        }
        
        public static void SetAllActive(this GameObject[] gameObjects, bool active)
        {
            if (gameObjects == null) throw new ArgumentNullException();
            gameObjects.ForEach(x => x.SetActive(active));
        }
    }
}