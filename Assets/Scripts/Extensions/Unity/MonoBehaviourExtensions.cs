using System;
using UnityEngine;

namespace Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static void SetActive(this MonoBehaviour monoBehaviour, bool value)
        {
            try
            {
                monoBehaviour.gameObject.SetActive(value);
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("Trying to set active state of destroyed MonoBehaviour?\n" +
                                 $"{e.Message}");
            }
        }

        public static bool IsActiveSelf(this MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.activeSelf;
        }

        public static void SafeStopCoroutine(this MonoBehaviour monoBehaviour, Coroutine coroutine)
        {
            if (monoBehaviour != null && coroutine != null)
            {
                monoBehaviour.StopCoroutine(coroutine);
            }
        }

        public static bool IsDestroyed(this MonoBehaviour monoBehaviour)
        {
            return monoBehaviour == null;
        }
    }
}