using System;
using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static void SetEulerAngleX(this Transform target, float val)
        {
            var euler = target.eulerAngles;
            euler.x = val;
            target.eulerAngles = euler;
        }
        
        public static void SetEulerAngleY(this Transform target, float val)
        {
            var euler = target.eulerAngles;
            euler.y = val;
            target.eulerAngles = euler;
        }
        
        public static void SetEulerAngleZ(this Transform target, float val)
        {
            var euler = target.eulerAngles;
            euler.z = val;
            target.eulerAngles = euler;
        }
        
        public static void SetLocalEulerAngleX(this Transform target, float val)
        {
            var euler = target.localEulerAngles;
            euler.x = val;
            target.localEulerAngles = euler;
        }
        
        public static void SetLocalEulerAngleY(this Transform target, float val)
        {
            var euler = target.localEulerAngles;
            euler.y = val;
            target.localEulerAngles = euler;
        }
        
        public static void SetLocalEulerAngleZ(this Transform target, float val)
        {
            var euler = target.localEulerAngles;
            euler.z = val;
            target.localEulerAngles = euler;
        }

        public static void SetLocalPositionX(this Transform target, float val)
        {
            var localPos = target.localPosition;
            localPos.x = val;
            target.localPosition = localPos;
        }
        
        public static void SetLocalPositionY(this Transform target, float val)
        {
            var localPos = target.localPosition;
            localPos.y = val;
            target.localPosition = localPos;
        }
        
        public static void SetLocalPositionZ(this Transform target, float val)
        {
            var localPos = target.localPosition;
            localPos.z = val;
            target.localPosition = localPos;
        }
        
        public static void SetPositionX(this Transform target, float val)
        {
            var worldPos = target.position;
            worldPos.x = val;
            target.position = worldPos;
        }
        
        public static void SetPositionY(this Transform target, float val)
        {
            var worldPos = target.position;
            worldPos.y = val;
            target.position = worldPos;
        }
        
        public static void SetPositionZ(this Transform target, float val)
        {
            var worldPos = target.position;
            worldPos.z = val;
            target.position = worldPos;
        }
        
        public static void AddListenerToDestroyEvent(this Transform transform, Action callback)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            transform.gameObject.AddListenerToDestroyEvent(callback);
        }
        
        public static void RemoveListenerFromDestroyEvent(this Transform transform, Action callback)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            transform.gameObject.RemoveListenerFromDestroyEvent(callback);
        }
    }
}
