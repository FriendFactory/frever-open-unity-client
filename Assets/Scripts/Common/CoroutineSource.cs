using System;
using System.Collections;
using UnityEngine;

namespace Common
{
    public interface ICoroutineSource
    {
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
        void SafeStopCoroutine(Coroutine routine);
        Coroutine ExecuteWithDelay(float delay, Action action);
        Coroutine ExecuteWithRealtimeDelay(float delay, Action action);
        Coroutine ExecuteWithFrameDelay(Action action);
        Coroutine ExecuteAtEndOfFrame(Action action);
        Coroutine ExecuteWithFramesDelay(int framesToWait, Action action);
    }
    
    public sealed class CoroutineSource : MonoBehaviour, ICoroutineSource
    {
        //---------------------------------------------------------------------
        // Singleton
        //---------------------------------------------------------------------
        
        private static CoroutineSource _source;

        public static ICoroutineSource Instance
        {
            get
            {
                if (_source == null)
                {
                    _source = new GameObject("CoroutineSource").AddComponent<CoroutineSource>();
                    DontDestroyOnLoad(_source.gameObject);
                }

                return _source;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SafeStopCoroutine(Coroutine coroutine)
        {
            if (this != null && coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        
        public Coroutine ExecuteWithDelay(float delay, Action action)
        {
            return StartCoroutine(ExecuteWithDelayCoroutine(delay, action));
        }
        
        public Coroutine ExecuteWithRealtimeDelay(float delay, Action action)
        {
            return StartCoroutine(ExecuteWithRealtimeDelayCoroutine(delay, action));
        }

        public Coroutine ExecuteWithFrameDelay(Action action)
        {
            return ExecuteWithFramesDelay(1, action);
        }
        
        public Coroutine ExecuteAtEndOfFrame(Action action)
        {
            return StartCoroutine(OnEndOfFrame(action));
        }

        public Coroutine ExecuteWithFramesDelay(int framesToWait, Action action)
        {
            return StartCoroutine(ExecuteWithFramesDelayCoroutine(framesToWait, action));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator ExecuteWithFramesDelayCoroutine(int framesToWait, Action action)
        {
            for (int i = 0; i < framesToWait; i++)
            {
                yield return null;
            }

            action?.Invoke();
        }

        private IEnumerator ExecuteWithDelayCoroutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        private IEnumerator ExecuteWithRealtimeDelayCoroutine(float delay, Action action)
        {
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();
        }
        
        private IEnumerator OnEndOfFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }
    }
}